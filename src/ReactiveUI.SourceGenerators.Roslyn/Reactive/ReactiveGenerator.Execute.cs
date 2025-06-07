// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                    InvalidReactiveObjectError,
                    propertySymbol,
                    propertySymbol.ContainingType,
                    propertySymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        // Get Property AccessModifier.
        var propertyAccessModifier = propertySymbol.DeclaredAccessibility.ToString().ToLower();
        if (propertyAccessModifier?.Contains("and") == true)
        {
            propertyAccessModifier = propertyAccessModifier.Replace("and", " ");
        }
        else if (propertyAccessModifier?.Contains("or") == true)
        {
            propertyAccessModifier = propertyAccessModifier.Replace("or", " ");
        }

        token.ThrowIfCancellationRequested();

        // Get Set AccessModifier.
        var setAccessModifier = $"{propertySymbol.SetMethod?.DeclaredAccessibility} set".ToLower();
        if (setAccessModifier.StartsWith("public", StringComparison.Ordinal))
        {
            setAccessModifier = "set";
        }
        else if (setAccessModifier?.Contains("and") == true)
        {
            if (setAccessModifier.Contains("protectedandinternal"))
            {
                setAccessModifier = setAccessModifier.Replace("protectedandinternal", "private protected");
            }
            else
            {
                setAccessModifier = setAccessModifier.Replace("and", " ");
            }
        }
        else if (setAccessModifier?.Contains("or") == true)
        {
            setAccessModifier = setAccessModifier.Replace("or", " ");
        }

        if (propertyAccessModifier == "private" && setAccessModifier == "private set")
        {
            setAccessModifier = "set";
        }
        else if (propertyAccessModifier == "internal" && setAccessModifier == "internal set")
        {
            setAccessModifier = "set";
        }
        else if (propertyAccessModifier == "protected" && setAccessModifier == "protected set")
        {
            setAccessModifier = "set";
        }
        else if (propertyAccessModifier == "protected internal" && setAccessModifier == "protected internal set")
        {
            setAccessModifier = "set";
        }
        else if (propertyAccessModifier == "private protected" && setAccessModifier == "private protected set")
        {
            setAccessModifier = "set";
        }

        token.ThrowIfCancellationRequested();

        var inheritance = propertySymbol.IsVirtual ? " virtual" : propertySymbol.IsOverride ? " override" : string.Empty;

        var useRequired = propertySymbol.IsRequired ? "required " : string.Empty;

        var typeNameWithNullabilityAnnotations = propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldName = propertySymbol.GetGeneratedFieldName();

        if (context.SemanticModel.Compilation is CSharpCompilation compilation && compilation.LanguageVersion == LanguageVersion.Preview)
        {
            fieldName = "field";
        }

        var propertyName = propertySymbol.Name;

        // Get the nullability info for the property
        propertySymbol.GetNullabilityInfo(
        context.SemanticModel,
        out var isReferenceTypeOrUnconstraindTypeParameter,
        out var includeMemberNotNullOnSetAccessor);

        var propertyDeclaration = (PropertyDeclarationSyntax)context.TargetNode;

        context.GetForwardedAttributes(
            builder,
            propertySymbol,
            propertyDeclaration.AttributeLists,
            token,
            out var forwardedAttributesString);

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
            setAccessModifier!,
            inheritance,
            useRequired,
            true,
            propertyAccessModifier!),
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
                    InvalidReactiveObjectError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        // Get AccessModifier enum value from the attribute
        attributeData.TryGetNamedArgument("SetModifier", out int accessModifierArgument);
        var setAccessModifier = accessModifierArgument switch
        {
            1 => "protected set",
            2 => "internal set",
            3 => "private set",
            4 => "protected internal set",
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
            setAccessModifier,
            inheritance,
            useRequired,
            false,
            "public"),
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
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations([.. properties.Select(p => p.TargetInfo.ParentInfo)]);

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

        var setFieldName = propertyInfo.FieldName;
        var getFieldName = propertyInfo.FieldName;
        if (propertyInfo.FieldName == "value")
        {
            setFieldName = "this.value";
        }

        var fieldSyntax = string.Empty;
        var partialModifier = propertyInfo.IsProperty ? "partial " : string.Empty;
        string propertyAttributes;

        if (propertyInfo.IsProperty && propertyInfo.FieldName != "field")
        {
            propertyAttributes = string.Join("\n        ", propertyInfo.ForwardedAttributes);
            fieldSyntax =
$$"""
{{propertyAttributes}}
        private {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.FieldName}};
""";
        }

        if (propertyInfo.IsProperty)
        {
            propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage);
        }
        else
        {
            propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedAttributes));
        }

        var accessModifier = propertyInfo.PropertyAccessModifier;
        var setAccessModifier = propertyInfo.SetAccessModifier;

        if (propertyInfo.IncludeMemberNotNullOnSetAccessor || propertyInfo.IsReferenceTypeOrUnconstrainedTypeParameter)
        {
            return
$$"""
        {{fieldSyntax}}
        /// <inheritdoc cref="{{setFieldName}}"/>
        {{propertyAttributes}}
        {{accessModifier}}{{propertyInfo.Inheritance}} {{propertyInfo.UseRequired}}{{partialModifier}}{{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}}
        { 
            get => {{getFieldName}};
            [global::System.Diagnostics.CodeAnalysis.MemberNotNull("{{setFieldName}}")]
            {{setAccessModifier}} => this.RaiseAndSetIfChanged(ref {{setFieldName}}, value);
        }
""";
        }

        return
$$"""
        {{fieldSyntax}}
        /// <inheritdoc cref="{{setFieldName}}"/>
        {{propertyAttributes}}
        {{accessModifier}}{{propertyInfo.Inheritance}} {{propertyInfo.UseRequired}}{{partialModifier}}{{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} { get => {{getFieldName}}; {{setAccessModifier}} => this.RaiseAndSetIfChanged(ref {{setFieldName}}, value); }
""";
    }
}
