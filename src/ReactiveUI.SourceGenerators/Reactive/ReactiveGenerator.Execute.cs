// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.Reactive.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// ReactiveGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ReactiveGenerator
{
    internal static readonly string GeneratorName = typeof(ReactiveGenerator).FullName!;
    internal static readonly string GeneratorVersion = typeof(ReactiveGenerator).Assembly.GetName().Version.ToString();

    private static readonly string[] excludeFromCodeCoverage = ["[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]"];

    /// <summary>
    /// Gets the observable method information.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="token">The token.</param>
    /// <returns>The value.</returns>
    private static PropertyInfo? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var symbol = context.TargetSymbol;

        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType, out var attributeData))
        {
            return default;
        }

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        if (!IsTargetTypeValid(fieldSymbol))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        // Get AccessModifier enum value from the attribute
        attributeData.TryGetNamedArgument("SetModifier", out int? accessModifierArgument);
        var accessModifier = accessModifierArgument switch
        {
            0 => "public",
            1 => "protected",
            2 => "internal",
            3 => "private",
            4 => "internal protected",
            5 => "private protected",
            _ => "public",
        };

        token.ThrowIfCancellationRequested();

        // Get the property type and name
        var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldName = fieldSymbol.Name;
        var propertyName = fieldSymbol.GetGeneratedPropertyName();

        token.ThrowIfCancellationRequested();

        // Get the nullability info for the property
        GetNullabilityInfo(
            fieldSymbol,
            context.SemanticModel,
            out var isReferenceTypeOrUnconstraindTypeParameter,
            out var includeMemberNotNullOnSetAccessor);

        // Get the attributes for the field
        var attributes = fieldSymbol.GetAttributes()
            .Where(x => x.AttributeClass?.HasFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType) == false)
            .ToImmutableArray();
        PropertyAttributeData[] propertyAttributes = [];
        if (attributes.Length > 0)
        {
            // Generate attribute list for fields.
            propertyAttributes = attributes.GenerateAttributes(
                AttributeTargets.Property,
                token);
        }

        var forwardedAttributes = new ForwardAttributes(propertyAttributes);
        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(fieldSymbol.ContainingType);

        token.ThrowIfCancellationRequested();

        return new(
            targetInfo.FileHintName,
            targetInfo.TargetName,
            targetInfo.TargetNamespace,
            targetInfo.TargetNamespaceWithNamespace,
            targetInfo.TargetVisibility,
            targetInfo.TargetType,
            typeNameWithNullabilityAnnotations,
            fieldName,
            propertyName,
            null,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedAttributes,
            accessModifier);
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
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = string.Join("\n\r", properties.Select(GetPropertySyntax));

        return
$$"""
// <auto-generated/>
using ReactiveUI;

#pragma warning disable
#nullable enable

namespace {{containingNamespace}}
{
{{AddTabs(1)}}/// <summary>
{{AddTabs(1)}}/// Partial class for the {{containingTypeName}} which contains ReactiveUI Reactive property initialization.
{{AddTabs(1)}}/// </summary>
{{AddTabs(1)}}{{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
{{AddTabs(1)}}{
{{AddTabs(2)}}[global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{propertyDeclarations}}
{{AddTabs(1)}}}
}
#nullable restore
#pragma warning restore
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

        var setModifier = propertyInfo.AccessModifier + " ";
        if (setModifier == "public ")
        {
            setModifier = string.Empty;
        }

        var propertyAttributes = string.Join("\n\t\t", excludeFromCodeCoverage.Concat(propertyInfo.ForwardedAttributes.Attributes.Select(x => x.FormatAttributes())));

        if (propertyInfo.IncludeMemberNotNullOnSetAccessor)
        {
            return
$$"""
{{AddTabs(2)}}/// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
{{AddTabs(2)}}{{propertyAttributes}}
{{AddTabs(2)}}{{propertyInfo.TargetVisibility}} {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}}
{{AddTabs(2)}}{ 
{{AddTabs(3)}}get => {{propertyInfo.FieldName}};
{{AddTabs(3)}}[global::System.Diagnostics.CodeAnalysis.MemberNotNull("{{propertyInfo.FieldName}}")]
{{AddTabs(3)}}{{setModifier}}set => this.RaiseAndSetIfChanged(ref {{propertyInfo.FieldName}}, value);
{{AddTabs(2)}}}
""";
        }

        return
$$"""
{{AddTabs(2)}}/// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
{{AddTabs(2)}}{{propertyAttributes}}
{{AddTabs(2)}}{{propertyInfo.TargetVisibility}} {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} { get => {{propertyInfo.FieldName}}; {{setModifier}}set => this.RaiseAndSetIfChanged(ref {{propertyInfo.FieldName}}, value); }
""";
    }

    private static string AddTabs(int tabCount) => new('\t', tabCount);

    /// <summary>
    /// Validates the containing type for a given field being annotated.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>Whether or not the containing type for <paramref name="fieldSymbol"/> is valid.</returns>
    private static bool IsTargetTypeValid(IFieldSymbol fieldSymbol)
    {
        var isObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
        var isIObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
        var hasObservableObjectAttribute = fieldSymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

        return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
    }

    /// <summary>
    /// Gets the nullability info on the generated property.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
    /// <param name="isReferenceTypeOrUnconstraindTypeParameter">Whether the property type supports nullability.</param>
    /// <param name="includeMemberNotNullOnSetAccessor">Whether MemberNotNullAttribute should be used on the setter.</param>
    private static void GetNullabilityInfo(
        IFieldSymbol fieldSymbol,
        SemanticModel semanticModel,
        out bool isReferenceTypeOrUnconstraindTypeParameter,
        out bool includeMemberNotNullOnSetAccessor)
    {
        // We're using IsValueType here and not IsReferenceType to also cover unconstrained type parameter cases.
        // This will cover both reference types as well T when the constraints are not struct or unmanaged.
        // If this is true, it means the field storage can potentially be in a null state (even if not annotated).
        isReferenceTypeOrUnconstraindTypeParameter = !fieldSymbol.Type.IsValueType;

        // This is used to avoid nullability warnings when setting the property from a constructor, in case the field
        // was marked as not nullable. Nullability annotations are assumed to always be enabled to make the logic simpler.
        // Consider this example:
        //
        // partial class MyViewModel : ReactiveObject
        // {
        //    public MyViewModel()
        //    {
        //        Name = "Bob";
        //    }
        //
        //    [Reactive]
        //    private string _name;
        // }
        //
        // The [MemberNotNull] attribute is needed on the setter for the generated Name property so that when Name
        // is set, the compiler can determine that the name backing field is also being set (to a non null value).
        // Of course, this can only be the case if the field type is also of a type that could be in a null state.
        includeMemberNotNullOnSetAccessor =
            isReferenceTypeOrUnconstraindTypeParameter &&
            fieldSymbol.Type.NullableAnnotation != NullableAnnotation.Annotated &&
            semanticModel.Compilation.HasAccessibleTypeWithMetadataName("System.Diagnostics.CodeAnalysis.MemberNotNullAttribute");
    }
}
