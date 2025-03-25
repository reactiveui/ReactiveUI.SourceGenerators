// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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
public sealed partial class ObservableAsPropertyGenerator
{
    private static Result<ObservableFieldInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        // Skip symbols without the target attribute
        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ObservableAsPropertyAttributeType, out var attributeData))
        {
            return default;
        }

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        // Validate the target type
        if (!fieldSymbol.IsTargetTypeValid())
        {
            builder.Add(
                    InvalidReactiveObjectError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        // Get the can PropertyName member, if any
        attributeData.TryGetNamedArgument("ReadOnly", out bool? isReadonly);

        // Get Inheritance value from the attribute
        attributeData.TryGetNamedArgument("Inheritance", out int? inheritanceArgument);
        var inheritance = inheritanceArgument switch
        {
            1 => " virtual",
            2 => " override",
            3 => " new",
            _ => string.Empty,
        };

        token.ThrowIfCancellationRequested();

        attributeData.TryGetNamedArgument("UseProtected", out bool useProtected);
        var useProtectedModifier = useProtected ? "protected" : "private";

        token.ThrowIfCancellationRequested();

        // Get the property type and name
        var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldName = fieldSymbol.Name;
        var propertyName = fieldSymbol.GetGeneratedPropertyName();

        // Check for name collisions
        if (fieldName == propertyName)
        {
            builder.Add(
                ReactivePropertyNameCollisionError,
                fieldSymbol,
                fieldSymbol.ContainingType,
                fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;
        var initializer = fieldDeclaration.Declaration.Variables.FirstOrDefault()?.Initializer?.ToFullString();

        token.ThrowIfCancellationRequested();

        context.GetForwardedAttributes(
                builder,
                fieldSymbol,
                fieldDeclaration.AttributeLists,
                token,
                out var forwardedPropertyAttributes);

        token.ThrowIfCancellationRequested();

        // Get the nullability info for the property
        fieldSymbol.GetNullabilityInfo(
                context.SemanticModel,
                out var isReferenceTypeOrUnconstraindTypeParameter,
                out var includeMemberNotNullOnSetAccessor);

        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(fieldSymbol.ContainingType);

        return new(
            new(
            targetInfo,
            typeNameWithNullabilityAnnotations,
            fieldName,
            propertyName,
            initializer,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedPropertyAttributes,
            isReadonly == false ? string.Empty : "readonly",
            useProtectedModifier,
            inheritance),
            builder.ToImmutable());
    }

    private static string GenerateSource(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, ObservableFieldInfo[] properties)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(properties.Select(p => p.TargetInfo.ParentInfo).ToArray());

        var classes = GenerateClassWithProperties(containingTypeName, containingNamespace, containingClassVisibility, containingType, properties);

        return $$"""
// <auto-generated/>
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
    private static string GenerateClassWithProperties(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, ObservableFieldInfo[] properties)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = string.Join("\n", properties.Select(GetPropertySyntax));

        return
$$"""
/// <summary>
    /// Partial class for the {{containingTypeName}} which contains ReactiveUI Observable As Property initialization.
    /// </summary>
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{propertyDeclarations}}
    }
""";
    }

    private static string GetPropertySyntax(ObservableFieldInfo propertyInfo)
    {
        var propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedAttributes));

        var getter = $$"""{ get => {{propertyInfo.FieldName}} = {{propertyInfo.FieldName}}Helper?.Value ?? {{propertyInfo.FieldName}}; }""";

        // If the property is nullable, we need to add a null check to the getter
        if (propertyInfo.TypeNameWithNullabilityAnnotations.EndsWith("?"))
        {
            getter = $$"""{ get => {{propertyInfo.FieldName}} = ({{propertyInfo.FieldName}}Helper == null ? {{propertyInfo.FieldName}} : {{propertyInfo.FieldName}}Helper.Value); }""";
        }

        var helperTypeName = $"{propertyInfo.AccessModifier} ReactiveUI.ObservableAsPropertyHelper<{propertyInfo.TypeNameWithNullabilityAnnotations}>?";

        // If the property is readonly, we need to change the helper to be non-nullable
        if (propertyInfo.IsReadOnly == "readonly")
        {
            helperTypeName = $"{propertyInfo.AccessModifier} readonly ReactiveUI.ObservableAsPropertyHelper<{propertyInfo.TypeNameWithNullabilityAnnotations}>";
        }

        return $$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}Helper"/>
        {{helperTypeName}} {{propertyInfo.FieldName}}Helper;

        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        public {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} {{getter}}
""";
    }
}
