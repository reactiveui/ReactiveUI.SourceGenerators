﻿// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
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
/// ReactiveCollectionGenerator.
/// </summary>
public sealed partial class ReactiveCollectionGenerator
{
    private static Result<ReactiveCollectionFieldInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        // Skip symbols without the target attribute
        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveCollectionAttributeType, out var attributeData))
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

    private static string GenerateSource(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, ReactiveCollectionFieldInfo[] properties)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations([.. properties.Select(p => p.TargetInfo.ParentInfo)]);

        var classes = GenerateClassWithProperties(containingTypeName, containingNamespace, containingClassVisibility, containingType, properties);

        return $$"""
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
    private static string GenerateClassWithProperties(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, ReactiveCollectionFieldInfo[] properties)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = string.Join("\n", properties.Select(GetPropertySyntax));

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{propertyDeclarations}}

        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static global::System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged(IReactiveObject @this, string propName)=> (_, _) =>  @this.RaisePropertyChanged(propName);
    }
""";
    }

    private static string GetPropertySyntax(ReactiveCollectionFieldInfo propertyInfo)
    {
        var propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedAttributes));

        return $$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        public {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}}
        {
            get => {{propertyInfo.FieldName}};
            set
            {
                if (value == null)
                {
                    {{propertyInfo.PropertyName}}.CollectionChanged -= CollectionChanged(this, nameof({{propertyInfo.PropertyName}}));
                }

                {{propertyInfo.FieldName}} = value;
                this.RaisePropertyChanged(nameof({{propertyInfo.PropertyName}}));

                if ({{propertyInfo.FieldName}} != null)
                {
                    // Remove the old handler if it exists
                    {{propertyInfo.PropertyName}}.CollectionChanged -= CollectionChanged(this, nameof({{propertyInfo.PropertyName}}));

                    {{propertyInfo.PropertyName}}.CollectionChanged += CollectionChanged(this, nameof({{propertyInfo.PropertyName}}));
                }
            }
        }
""";
    }
}
