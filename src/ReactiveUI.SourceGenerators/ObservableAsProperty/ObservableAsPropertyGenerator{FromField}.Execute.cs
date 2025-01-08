// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
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
        if (!IsTargetTypeValid(fieldSymbol))
        {
            builder.Add(
                    InvalidObservableAsPropertyError,
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
        var propertyName = GetGeneratedPropertyName(fieldSymbol);

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

        using var forwardedAttributes = ImmutableArrayBuilder<AttributeInfo>.Rent();

        // Gather attributes info
        foreach (var attribute in fieldSymbol.GetAttributes())
        {
            token.ThrowIfCancellationRequested();

            // Track the current attribute for forwarding if it is a validation attribute
            if (attribute.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ValidationAttribute") == true)
            {
                forwardedAttributes.Add(AttributeInfo.Create(attribute));
            }

            // Track the current attribute for forwarding if it is a Json Serialization attribute
            if (attribute.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.Text.Json.Serialization.JsonAttribute") == true)
            {
                forwardedAttributes.Add(AttributeInfo.Create(attribute));
            }

            // Also track the current attribute for forwarding if it is of any of the following types:
            if (attribute.AttributeClass?.HasOrInheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.UIHintAttribute") == true ||
                attribute.AttributeClass?.HasOrInheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute") == true ||
                attribute.AttributeClass?.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.DisplayAttribute") == true ||
                attribute.AttributeClass?.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.EditableAttribute") == true ||
                attribute.AttributeClass?.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.KeyAttribute") == true ||
                attribute.AttributeClass?.HasFullyQualifiedMetadataName("System.Runtime.Serialization.DataMemberAttribute") == true ||
                attribute.AttributeClass?.HasFullyQualifiedMetadataName("System.Runtime.Serialization.IgnoreDataMemberAttribute") == true)
            {
                forwardedAttributes.Add(AttributeInfo.Create(attribute));
            }
        }

        token.ThrowIfCancellationRequested();

        // Gather explicit forwarded attributes info
        foreach (var attributeList in fieldDeclaration.AttributeLists)
        {
            // Only look for attribute lists explicitly targeting the (generated) property. Roslyn will normally emit a
            // CS0657 warning (invalid target), but that is automatically suppressed by a dedicated diagnostic suppressor
            // that recognizes uses of this target specifically to support [ObservableAsProperty].
            if (attributeList.Target?.Identifier is not SyntaxToken(SyntaxKind.PropertyKeyword))
            {
                continue;
            }

            token.ThrowIfCancellationRequested();

            foreach (var attribute in attributeList.Attributes)
            {
                // Roslyn ignores attributes in an attribute list with an invalid target, so we can't get the AttributeData as usual.
                // To reconstruct all necessary attribute info to generate the serialized model, we use the following steps:
                //   - We try to get the attribute symbol from the semantic model, for the current attribute syntax. In case this is not
                //     available (in theory it shouldn't, but it can be), we try to get it from the candidate symbols list for the node.
                //     If there are no candidates or more than one, we just issue a diagnostic and stop processing the current attribute.
                //     The returned symbols might be method symbols (constructor attribute) so in that case we can get the declaring type.
                //   - We then go over each attribute argument expression and get the operation for it. This will still be available even
                //     though the rest of the attribute is not validated nor bound at all. From the operation we can still retrieve all
                //     constant values to build the AttributeInfo model. After all, attributes only support constant values, typeof(T)
                //     expressions, or arrays of either these two types, or of other arrays with the same rules, recursively.
                //   - From the syntax, we can also determine the identifier names for named attribute arguments, if any.
                // There is no need to validate anything here: the attribute will be forwarded as is, and then Roslyn will validate on the
                // generated property. Users will get the same validation they'd have had directly over the field. The only drawback is the
                // lack of IntelliSense when constructing attributes over the field, but this is the best we can do from this end anyway.
                if (!context.SemanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeTypeSymbol))
                {
                    builder.Add(
                            InvalidPropertyTargetedAttributeOnObservableAsPropertyField,
                            attribute,
                            fieldSymbol,
                            attribute.Name);
                    continue;
                }

                var attributeArguments = attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>();

                // Try to extract the forwarded attribute
                if (!AttributeInfo.TryCreate(attributeTypeSymbol, context.SemanticModel, attributeArguments, token, out var attributeInfo))
                {
                    builder.Add(
                            InvalidPropertyTargetedAttributeExpressionOnObservableAsPropertyField,
                            attribute,
                            fieldSymbol,
                            attribute.Name);
                    continue;
                }

                forwardedAttributes.Add(attributeInfo);
            }
        }

        token.ThrowIfCancellationRequested();

        // Get the nullability info for the property
        fieldSymbol.GetNullabilityInfo(
        context.SemanticModel,
        out var isReferenceTypeOrUnconstraindTypeParameter,
        out var includeMemberNotNullOnSetAccessor);

        token.ThrowIfCancellationRequested();
        var attributes = forwardedAttributes.ToImmutable();
        var forwardedPropertyAttributes = attributes.Select(static a => a.ToString()).ToImmutableArray();

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

    /// <summary>
    /// Get the generated property name for an input field.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>The generated property name for <paramref name="fieldSymbol"/>.</returns>
    private static string GetGeneratedPropertyName(IFieldSymbol fieldSymbol)
    {
        var propertyName = fieldSymbol.Name;

        if (propertyName.StartsWith("m_"))
        {
            propertyName = propertyName.Substring(2);
        }
        else if (propertyName.StartsWith("_"))
        {
            propertyName = propertyName.TrimStart('_');
        }

        return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
    }

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
}
