// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.Reactive.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// ReactiveGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ReactiveGenerator
{
    internal static readonly string GeneratorName = typeof(ReactiveGenerator).FullName!;
    internal static readonly string GeneratorVersion = typeof(ReactiveGenerator).Assembly.GetName().Version.ToString();

#if ROSYLN_412
    private static Result<PropertyInfo?>? GetPropertyInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;

        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType, out var attributeData))
        {
            return default;
        }

        if (symbol is not IPropertySymbol propertySymbol)
        {
            return default;
        }

        if (!propertySymbol.IsPartialDefinition || propertySymbol.IsStatic)
        {
            return default;
        }

        if (!propertySymbol.IsTargetTypeValid())
        {
            builder.Add(
                    InvalidReactiveError,
                    propertySymbol,
                    propertySymbol.ContainingType,
                    propertySymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        var accessModifier = $"{propertySymbol.SetMethod?.DeclaredAccessibility} set".ToLower();
        if (accessModifier.StartsWith("public", StringComparison.Ordinal))
        {
            accessModifier = "set";
        }
        else if (accessModifier.Contains("and"))
        {
            accessModifier = accessModifier.Replace("and", " ");
        }

        token.ThrowIfCancellationRequested();

        var inheritance = propertySymbol.IsVirtual ? " virtual" : propertySymbol.IsOverride ? " override" : string.Empty;
        var useRequired = string.Empty;
        var typeNameWithNullabilityAnnotations = propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldName = propertySymbol.GetGeneratedFieldName();
        var propertyName = propertySymbol.Name;

        // Get the nullability info for the property
        propertySymbol.GetNullabilityInfo(
        context.SemanticModel,
        out var isReferenceTypeOrUnconstraindTypeParameter,
        out var includeMemberNotNullOnSetAccessor);

        ImmutableArray<string> forwardedAttributesString = [];
        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(propertySymbol.ContainingType);

        token.ThrowIfCancellationRequested();

        return new(
            new(
            targetInfo,
            typeNameWithNullabilityAnnotations,
            fieldName,
            propertyName,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedAttributesString,
            accessModifier,
            inheritance,
            useRequired,
            true),
            builder.ToImmutable());
    }
#endif

    /// <summary>
    /// Gets the observable method information.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="token">The token.</param>
    /// <returns>
    /// The value.
    /// </returns>
    private static Result<PropertyInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;

        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType, out var attributeData))
        {
            return default;
        }

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        if (!fieldSymbol.IsTargetTypeValid())
        {
            builder.Add(
                    InvalidReactiveError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        // Get AccessModifier enum value from the attribute
        attributeData.TryGetNamedArgument("SetModifier", out int accessModifierArgument);
        var accessModifier = accessModifierArgument switch
        {
            1 => "protected set",
            2 => "internal set",
            3 => "private set",
            4 => "internal protected set",
            5 => "private protected set",
            6 => "init",
            _ => "set",
        };

        token.ThrowIfCancellationRequested();

        // Get Inheritance value from the attribute
        attributeData.TryGetNamedArgument("Inheritance", out int inheritanceArgument);
        var inheritance = inheritanceArgument switch
        {
            1 => " virtual",
            2 => " override",
            3 => " new",
            _ => string.Empty,
        };

        token.ThrowIfCancellationRequested();

        // Get Inheritance value from the attribute
        attributeData.TryGetNamedArgument("UseRequired", out bool useRequiredArgument);
        var useRequired = useRequiredArgument ? "required " : string.Empty;

        token.ThrowIfCancellationRequested();

        // Get the property type and name
        var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldName = fieldSymbol.Name;
        var propertyName = fieldSymbol.GetGeneratedPropertyName();

        if (fieldName == propertyName)
        {
            builder.Add(
                    ReactivePropertyNameCollisionError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        // Get the nullability info for the property
        fieldSymbol.GetNullabilityInfo(
        context.SemanticModel,
        out var isReferenceTypeOrUnconstraindTypeParameter,
        out var includeMemberNotNullOnSetAccessor);

        token.ThrowIfCancellationRequested();
        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;

        context.GetForwardedAttributes(
            builder,
            fieldSymbol,
            fieldDeclaration.AttributeLists,
            token,
            out var forwardedAttributesString);

        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(fieldSymbol.ContainingType);

        token.ThrowIfCancellationRequested();

        return new(
            new(
            targetInfo,
            typeNameWithNullabilityAnnotations,
            fieldName,
            propertyName,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedAttributesString,
            accessModifier,
            inheritance,
            useRequired,
            false),
            builder.ToImmutable());
    }

    /// <summary>
    /// Generates the source code.
    /// </summary>
    /// <param name="containingTypeName">The contain type name.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing class visibility.</param>
    /// <param name="containingType">The containing type.</param>
    /// <param name="properties">The properties.</param>
    /// <returns>The value.</returns>
    private static string GenerateSource(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, PropertyInfo[] properties)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(properties.Select(p => p.TargetInfo.ParentInfo).ToArray());

        var classes = GenerateClassWithProperties(containingTypeName, containingNamespace, containingClassVisibility, containingType, properties);

        return
$$"""
// <auto-generated/>
using ReactiveUI;

#pragma warning disable
#nullable enable

namespace {{containingNamespace}}
{
    {{parentClassDeclarationsString}}{{classes}}{{closingBrackets}}
}
#nullable restore
#pragma warning restore
""";
    }

    /// <summary>
    /// Generates the source code.
    /// </summary>
    /// <param name="containingTypeName">The contain type name.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing class visibility.</param>
    /// <param name="containingType">The containing type.</param>
    /// <param name="properties">The properties.</param>
    /// <returns>The value.</returns>
    private static string GenerateClassWithProperties(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, PropertyInfo[] properties)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = string.Join("\n", properties.Select(GetPropertySyntax));

        return
$$"""
/// <summary>
    /// Partial class for the {{containingTypeName}} which contains ReactiveUI Reactive property initialization.
    /// </summary>
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{propertyDeclarations}}
    }
""";
    }

    /// <summary>
    /// Generates property declarations for the given observable method information.
    /// </summary>
    /// <param name="propertyInfo">Metadata about the observable property.</param>
    /// <returns>A string containing the generated code for the property.</returns>
    private static string GetPropertySyntax(PropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyName is null)
        {
            return string.Empty;
        }

        var fieldSyntax = string.Empty;
        var partialModifier = propertyInfo.IsProperty ? "partial " : string.Empty;
        if (propertyInfo.IsProperty)
        {
            fieldSyntax = $"private {propertyInfo.TypeNameWithNullabilityAnnotations} {propertyInfo.FieldName};";
        }

        var propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedAttributes));

        if (propertyInfo.IncludeMemberNotNullOnSetAccessor || propertyInfo.IsReferenceTypeOrUnconstrainedTypeParameter)
        {
            return
$$"""
        {{fieldSyntax}}
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        {{propertyInfo.TargetInfo.TargetVisibility}}{{propertyInfo.Inheritance}} {{partialModifier}}{{propertyInfo.UseRequired}}{{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}}
        { 
            get => {{propertyInfo.FieldName}};
            [global::System.Diagnostics.CodeAnalysis.MemberNotNull("{{propertyInfo.FieldName}}")]
            {{propertyInfo.AccessModifier}} => this.RaiseAndSetIfChanged(ref {{propertyInfo.FieldName}}, value);
        }
""";
        }

        return
$$"""
        {{fieldSyntax}}
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        {{propertyInfo.TargetInfo.TargetVisibility}}{{propertyInfo.Inheritance}} {{partialModifier}}{{propertyInfo.UseRequired}}{{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} { get => {{propertyInfo.FieldName}}; {{propertyInfo.AccessModifier}} => this.RaiseAndSetIfChanged(ref {{propertyInfo.FieldName}}, value); }
""";
    }
}
