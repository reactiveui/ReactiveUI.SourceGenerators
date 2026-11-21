// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Provides extension members used while processing generator contexts.</summary>
internal static class ContextExtensions
{
    /// <summary>Provides extension members for compilations.</summary>
    /// <param name="compilation">The compilation to extend.</param>
    extension(Compilation compilation)
    {
        /// <summary>Gets the ReactiveUI integration supported by this compilation.</summary>
        /// <returns>The ReactiveUI API, view API and property observation supported by this compilation.</returns>
        internal ReactiveUiIntegration GetReactiveUiIntegration() =>
            new(GetApi(compilation), GetViewApi(compilation), ViewApiRules.HasObservedProperty(compilation));
    }

    /// <summary>Provides extension members for generator attribute syntax contexts.</summary>
    /// <param name="context">The generator attribute syntax context to extend.</param>
    extension(in GeneratorAttributeSyntaxContext context)
    {
        /// <summary>Gets attributes that should be forwarded from the source member.</summary>
        /// <param name="builder">The builder to receive diagnostics.</param>
        /// <param name="symbol">The source symbol.</param>
        /// <param name="attributeListSyntaxes">The attribute lists declared on the source member.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="forwardedAttributes">The generated attribute source text.</param>
        internal void GetForwardedAttributes(
            ArrayBuilder<DiagnosticInfo> builder,
            ISymbol symbol,
            in SyntaxList<AttributeListSyntax> attributeListSyntaxes,
            CancellationToken token,
            out EquatableArray<string> forwardedAttributes)
        {
            using var attributes = ArrayBuilder<AttributeInfo>.Rent();
            AddAutomaticForwardedAttributes(symbol, attributes, token);
            AddExplicitForwardedAttributes(context, builder, symbol, attributeListSyntaxes, attributes, token);
            forwardedAttributes = ConvertToSourceAttributes(attributes.WrittenSpan);
        }
    }

    /// <summary>Provides extension members for incremental generator initialization contexts.</summary>
    /// <param name="context">The initialization context to extend.</param>
    extension(in IncrementalGeneratorInitializationContext context)
    {
        /// <summary>Gets a provider for the ReactiveUI API integration used by the compilation.</summary>
        /// <returns>A provider that produces the ReactiveUI integration details.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IncrementalValueProvider<ReactiveUiIntegration> ReactiveUiIntegration() =>
            context.CompilationProvider.Select(static (compilation, token) =>
            {
                token.ThrowIfCancellationRequested();
                return compilation.GetReactiveUiIntegration();
            });
    }

    /// <summary>Adds attributes automatically forwarded from a source symbol.</summary>
    /// <param name="symbol">The source symbol.</param>
    /// <param name="attributes">The destination attributes.</param>
    /// <param name="token">The cancellation token.</param>
    private static void AddAutomaticForwardedAttributes(
        ISymbol symbol,
        ArrayBuilder<AttributeInfo> attributes,
        CancellationToken token)
    {
        // The only property this is called for is a [Reactive] partial property. It already carries its own
        // attributes, so nothing is forwarded to it automatically.
        var symbolAttributes = symbol.GetAttributes();
        if (symbol is IPropertySymbol || symbolAttributes.Length <= 1)
        {
            return;
        }

        foreach (var attribute in symbolAttributes)
        {
            token.ThrowIfCancellationRequested();
            if (ShouldForwardAttribute(attribute))
            {
                attributes.Add(AttributeInfo.Create(attribute));
            }
        }
    }

    /// <summary>Determines whether an attribute should be forwarded automatically.</summary>
    /// <param name="attribute">The attribute to inspect.</param>
    /// <returns>Whether the attribute should be forwarded.</returns>
    /// <remarks>Roslyn gives an attribute it cannot bind an error type, never a null class.</remarks>
    private static bool ShouldForwardAttribute(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass!;
        return IsValidationAttribute(attributeClass)
            || IsJsonAttribute(attributeClass)
            || IsDataAnnotationAttribute(attributeClass)
            || IsSerializationAttribute(attributeClass);
    }

    /// <summary>Determines whether a type is a validation attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a validation attribute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidationAttribute(INamedTypeSymbol attributeClass) =>
        attributeClass.InheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ValidationAttribute");

    /// <summary>Determines whether a type is a JSON serialization attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a JSON serialization attribute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsJsonAttribute(INamedTypeSymbol attributeClass) =>
        attributeClass.InheritsFromFullyQualifiedMetadataName("System.Text.Json.Serialization.JsonAttribute");

    /// <summary>Determines whether a type is a supported data annotation attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a supported data annotation attribute.</returns>
    private static bool IsDataAnnotationAttribute(INamedTypeSymbol attributeClass) =>
        attributeClass.HasOrInheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.UIHintAttribute")
        || attributeClass.HasOrInheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute")
        || attributeClass.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.DisplayAttribute")
        || attributeClass.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.EditableAttribute")
        || attributeClass.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.KeyAttribute");

    /// <summary>Determines whether a type is a supported serialization attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a supported serialization attribute.</returns>
    private static bool IsSerializationAttribute(INamedTypeSymbol attributeClass) =>
        attributeClass.HasFullyQualifiedMetadataName("System.Runtime.Serialization.DataMemberAttribute")
        || attributeClass.HasFullyQualifiedMetadataName("System.Runtime.Serialization.IgnoreDataMemberAttribute");

    /// <summary>Adds attributes explicitly targeted at the generated property or field.</summary>
    /// <param name="context">The generator context.</param>
    /// <param name="diagnostics">The builder to receive diagnostics.</param>
    /// <param name="symbol">The source symbol.</param>
    /// <param name="attributeLists">The attribute lists to inspect.</param>
    /// <param name="attributes">The destination attributes.</param>
    /// <param name="token">The cancellation token.</param>
    private static void AddExplicitForwardedAttributes(
        in GeneratorAttributeSyntaxContext context,
        ArrayBuilder<DiagnosticInfo> diagnostics,
        ISymbol symbol,
        in SyntaxList<AttributeListSyntax> attributeLists,
        ArrayBuilder<AttributeInfo> attributes,
        CancellationToken token)
    {
        foreach (var attributeList in attributeLists)
        {
            if (!IsPropertyOrFieldTarget(attributeList))
            {
                continue;
            }

            token.ThrowIfCancellationRequested();
            foreach (var attribute in attributeList.Attributes)
            {
                AddExplicitForwardedAttribute(context, diagnostics, symbol, attribute, attributes, token);
            }
        }
    }

    /// <summary>Determines whether an attribute list targets the generated property or field.</summary>
    /// <param name="attributeList">The attribute list to inspect.</param>
    /// <returns>Whether the list targets a generated member.</returns>
    private static bool IsPropertyOrFieldTarget(AttributeListSyntax attributeList) =>
        attributeList.Target?.Identifier is SyntaxToken(SyntaxKind.PropertyKeyword)
        or SyntaxToken(SyntaxKind.FieldKeyword);

    /// <summary>Adds one explicitly forwarded attribute or reports the appropriate diagnostic.</summary>
    /// <param name="context">The generator context.</param>
    /// <param name="diagnostics">The builder to receive diagnostics.</param>
    /// <param name="symbol">The source symbol.</param>
    /// <param name="attribute">The attribute syntax to inspect.</param>
    /// <param name="attributes">The destination attributes.</param>
    /// <param name="token">The cancellation token.</param>
    private static void AddExplicitForwardedAttribute(
        in GeneratorAttributeSyntaxContext context,
        ArrayBuilder<DiagnosticInfo> diagnostics,
        ISymbol symbol,
        AttributeSyntax attribute,
        ArrayBuilder<AttributeInfo> attributes,
        CancellationToken token)
    {
        if (!context.SemanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeType))
        {
            diagnostics.Add(InvalidForwardedAttributeType, attribute, symbol, attribute.Name);
            return;
        }

        if (!AttributeInfo.TryCreate(
                attributeType,
                context.SemanticModel,
                attribute.ArgumentList?.Arguments ?? [],
                token,
                out var attributeInfo))
        {
            diagnostics.Add(InvalidForwardedAttributeExpression, attribute, symbol, attribute.Name);
            return;
        }

        attributes.Add(attributeInfo);
    }

    /// <summary>Converts forwarded attribute metadata to generated source text.</summary>
    /// <param name="attributes">The attributes to convert.</param>
    /// <returns>The generated source text.</returns>
    private static EquatableArray<string> ConvertToSourceAttributes(ReadOnlySpan<AttributeInfo> attributes)
    {
        using var sourceAttributes = ArrayBuilder<string>.Rent();
        foreach (var attribute in attributes)
        {
            sourceAttributes.Add(attribute.ToString());
        }

        return sourceAttributes.ToEquatableArray();
    }

    /// <summary>Gets the ReactiveUI API a compilation uses.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The System.Reactive API when <c>ReactiveUI.Reactive</c> is referenced, otherwise the Primitives API.</returns>
    private static ReactiveUiApi GetApi(Compilation compilation) =>
        compilation.GetTypeByMetadataName("ReactiveUI.Reactive.ReactiveCommand") is not null
            ? ReactiveUiApi.SystemReactive
            : ReactiveUiApi.Primitives;

    /// <summary>Gets which assembly declares the <c>IViewFor</c> interfaces and the view locator a compilation uses.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The view API, chosen by <see cref="ViewApiRules.GetViewNamespace"/>.</returns>
    private static ReactiveUiViewApi GetViewApi(Compilation compilation) =>
        ViewApiRules.GetViewNamespace(compilation) == ViewApiRules.BindingReactiveNamespace
            ? ReactiveUiViewApi.BindingReactive
            : ReactiveUiViewApi.Binding;
}
