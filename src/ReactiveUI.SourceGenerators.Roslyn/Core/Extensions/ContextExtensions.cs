// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
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
    /// <summary>The first ReactiveUI version that supports the current generator behavior.</summary>
    private const int CurrentGeneratorBehaviorMinimumMajorVersion = 23;

    /// <summary>The metadata name of the ReactiveUI primitive void type.</summary>
    private const string RxVoidMetadataName = "ReactiveUI.Primitives.RxVoid";

    /// <summary>Provides extension members for compilations.</summary>
    /// <param name="compilation">The compilation to extend.</param>
    extension(Compilation compilation)
    {
        /// <summary>Gets the ReactiveUI integration supported by this compilation.</summary>
        /// <returns>The ReactiveUI API and command behavior supported by this compilation.</returns>
        internal ReactiveUiIntegration GetReactiveUiIntegration()
        {
            if (compilation.GetTypeByMetadataName("ReactiveUI.Reactive.ReactiveCommand") is not null)
            {
                return new(ReactiveUiApi.SystemReactive, true);
            }

            var legacyCommand = compilation.GetTypeByMetadataName("ReactiveUI.ReactiveCommand");
            if (legacyCommand is not null)
            {
                var majorVersion = legacyCommand.ContainingAssembly.Identity.Version.Major;
                return HasPrimitiveRxVoid(legacyCommand)
                    ? new(ReactiveUiApi.Primitives, true)
                    : new(ReactiveUiApi.Legacy, majorVersion >= CurrentGeneratorBehaviorMinimumMajorVersion);
            }

            return GetReferencedReactiveUiIntegration(compilation);
        }
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
            ImmutableArrayBuilder<DiagnosticInfo> builder,
            ISymbol symbol,
            in SyntaxList<AttributeListSyntax> attributeListSyntaxes,
            CancellationToken token,
            out ImmutableArray<string> forwardedAttributes)
        {
            using var attributes = ImmutableArrayBuilder<AttributeInfo>.Rent();
            AddAutomaticForwardedAttributes(symbol, attributes, token);
            AddExplicitForwardedAttributes(context, builder, symbol, attributeListSyntaxes, attributes, token);
            forwardedAttributes = ConvertToSourceAttributes(attributes.ToImmutable());
        }
    }

    /// <summary>Provides extension members for incremental generator initialization contexts.</summary>
    /// <param name="context">The initialization context to extend.</param>
    extension(in IncrementalGeneratorInitializationContext context)
    {
        /// <summary>Gets a provider for the ReactiveUI API integration used by the compilation.</summary>
        /// <returns>A provider that produces the ReactiveUI integration details.</returns>
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
        ImmutableArrayBuilder<AttributeInfo> attributes,
        CancellationToken token)
    {
        var symbolAttributes = symbol.GetAttributes();
        if (IsReactivePartialProperty(symbol, symbolAttributes) || symbolAttributes.Length <= 1)
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

    /// <summary>Determines whether a symbol is a partial property decorated with <c>ReactiveAttribute</c>.</summary>
    /// <param name="symbol">The symbol to inspect.</param>
    /// <param name="attributes">The attributes declared on the symbol.</param>
    /// <returns>Whether the symbol is a reactive partial property.</returns>
    private static bool IsReactivePartialProperty(ISymbol symbol, ImmutableArray<AttributeData> attributes)
    {
        if (symbol is not IPropertySymbol)
        {
            return false;
        }

        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass?.HasFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType) == true)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether an attribute should be forwarded automatically.</summary>
    /// <param name="attribute">The attribute to inspect.</param>
    /// <returns>Whether the attribute should be forwarded.</returns>
    private static bool ShouldForwardAttribute(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;
        return IsValidationAttribute(attributeClass)
            || IsJsonAttribute(attributeClass)
            || IsDataAnnotationAttribute(attributeClass)
            || IsSerializationAttribute(attributeClass);
    }

    /// <summary>Determines whether a type is a validation attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a validation attribute.</returns>
    private static bool IsValidationAttribute(INamedTypeSymbol? attributeClass) =>
        attributeClass?.InheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ValidationAttribute") == true;

    /// <summary>Determines whether a type is a JSON serialization attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a JSON serialization attribute.</returns>
    private static bool IsJsonAttribute(INamedTypeSymbol? attributeClass) =>
        attributeClass?.InheritsFromFullyQualifiedMetadataName("System.Text.Json.Serialization.JsonAttribute") == true;

    /// <summary>Determines whether a type is a supported data annotation attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a supported data annotation attribute.</returns>
    private static bool IsDataAnnotationAttribute(INamedTypeSymbol? attributeClass) =>
        HasOrInheritsFrom(attributeClass, "System.ComponentModel.DataAnnotations.UIHintAttribute")
        || HasOrInheritsFrom(attributeClass, "System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute")
        || HasMetadataName(attributeClass, "System.ComponentModel.DataAnnotations.DisplayAttribute")
        || HasMetadataName(attributeClass, "System.ComponentModel.DataAnnotations.EditableAttribute")
        || HasMetadataName(attributeClass, "System.ComponentModel.DataAnnotations.KeyAttribute");

    /// <summary>Determines whether a type is a supported serialization attribute.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>Whether the type is a supported serialization attribute.</returns>
    private static bool IsSerializationAttribute(INamedTypeSymbol? attributeClass) =>
        HasMetadataName(attributeClass, "System.Runtime.Serialization.DataMemberAttribute")
        || HasMetadataName(attributeClass, "System.Runtime.Serialization.IgnoreDataMemberAttribute");

    /// <summary>Determines whether an attribute type has or inherits from a metadata name.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <param name="metadataName">The metadata name to match.</param>
    /// <returns>Whether the type has or inherits from the metadata name.</returns>
    private static bool HasOrInheritsFrom(INamedTypeSymbol? attributeClass, string metadataName) =>
        attributeClass?.HasOrInheritsFromFullyQualifiedMetadataName(metadataName) == true;

    /// <summary>Determines whether an attribute type has a metadata name.</summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <param name="metadataName">The metadata name to match.</param>
    /// <returns>Whether the type has the metadata name.</returns>
    private static bool HasMetadataName(INamedTypeSymbol? attributeClass, string metadataName) =>
        attributeClass?.HasFullyQualifiedMetadataName(metadataName) == true;

    /// <summary>Adds attributes explicitly targeted at the generated property or field.</summary>
    /// <param name="context">The generator context.</param>
    /// <param name="diagnostics">The builder to receive diagnostics.</param>
    /// <param name="symbol">The source symbol.</param>
    /// <param name="attributeLists">The attribute lists to inspect.</param>
    /// <param name="attributes">The destination attributes.</param>
    /// <param name="token">The cancellation token.</param>
    private static void AddExplicitForwardedAttributes(
        in GeneratorAttributeSyntaxContext context,
        ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        ISymbol symbol,
        in SyntaxList<AttributeListSyntax> attributeLists,
        ImmutableArrayBuilder<AttributeInfo> attributes,
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
        ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        ISymbol symbol,
        AttributeSyntax attribute,
        ImmutableArrayBuilder<AttributeInfo> attributes,
        CancellationToken token)
    {
        if (!context.SemanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeType))
        {
            diagnostics.Add(InvalidPropertyTargetedAttributeOnObservableAsPropertyField, attribute, symbol, attribute.Name);
            return;
        }

        if (!AttributeInfo.TryCreate(
                attributeType,
                context.SemanticModel,
                attribute.ArgumentList?.Arguments ?? [],
                token,
                out var attributeInfo))
        {
            diagnostics.Add(InvalidPropertyTargetedAttributeExpressionOnObservableAsPropertyField, attribute, symbol, attribute.Name);
            return;
        }

        attributes.Add(attributeInfo);
    }

    /// <summary>Converts forwarded attribute metadata to generated source text.</summary>
    /// <param name="attributes">The attributes to convert.</param>
    /// <returns>The generated source text.</returns>
    private static ImmutableArray<string> ConvertToSourceAttributes(ImmutableArray<AttributeInfo> attributes)
    {
        using var sourceAttributes = ImmutableArrayBuilder<string>.Rent();
        foreach (var attribute in attributes)
        {
            sourceAttributes.Add(attribute.ToString());
        }

        return sourceAttributes.ToImmutable();
    }

    /// <summary>Determines whether a ReactiveUI command exposes the primitive void type.</summary>
    /// <param name="command">The command type to inspect.</param>
    /// <returns>Whether a create method exposes <c>ReactiveUI.Primitives.RxVoid</c>.</returns>
    private static bool HasPrimitiveRxVoid(INamedTypeSymbol command)
    {
        foreach (var member in command.GetMembers("Create"))
        {
            if (member is not IMethodSymbol method || method.ReturnType is not INamedTypeSymbol returnType)
            {
                continue;
            }

            foreach (var typeArgument in returnType.TypeArguments)
            {
                if (typeArgument.ToDisplayString() == RxVoidMetadataName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Gets the integration details from a referenced ReactiveUI assembly.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The detected ReactiveUI integration.</returns>
    private static ReactiveUiIntegration GetReferencedReactiveUiIntegration(Compilation compilation)
    {
        foreach (var assembly in compilation.ReferencedAssemblyNames)
        {
            if (assembly.Name == AttributeDefinitions.ReactiveUI)
            {
                return new(
                    ReactiveUiApi.Legacy,
                    assembly.Version.Major >= CurrentGeneratorBehaviorMinimumMajorVersion);
            }
        }

        return default;
    }
}
