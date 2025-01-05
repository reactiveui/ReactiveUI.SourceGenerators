// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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

        if (!IsTargetTypeValid(fieldSymbol))
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
            1 => "protected ",
            2 => "internal ",
            3 => "private ",
            4 => "internal protected ",
            5 => "private protected ",
            _ => string.Empty,
        };

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
            if (attributeData.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.Text.Json.Serialization.JsonAttribute") == true)
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
        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;

        // Gather explicit forwarded attributes info
        foreach (var attributeList in fieldDeclaration.AttributeLists)
        {
            // Only look for attribute lists explicitly targeting the (generated) property. Roslyn will normally emit a
            // CS0657 warning (invalid target), but that is automatically suppressed by a dedicated diagnostic suppressor
            // that recognizes uses of this target specifically to support [Reactive].
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
                            InvalidPropertyTargetedAttributeOnReactiveField,
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
                            InvalidPropertyTargetedAttributeExpressionOnReactiveField,
                            attribute,
                            fieldSymbol,
                            attribute.Name);
                    continue;
                }

                forwardedAttributes.Add(attributeInfo);
            }
        }

        var forwardedAttributesString = forwardedAttributes.ToImmutable().Select(x => x.ToString()).ToImmutableArray();
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
            inheritance),
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
        var propertyDeclarations = string.Join("\n\r", properties.Select(GetPropertySyntax));

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

        var propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedAttributes));

        if (propertyInfo.IncludeMemberNotNullOnSetAccessor)
        {
            return
$$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        {{propertyInfo.TargetInfo.TargetVisibility}}{{propertyInfo.Inheritance}} {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}}
        { 
            get => {{propertyInfo.FieldName}};
            [global::System.Diagnostics.CodeAnalysis.MemberNotNull("{{propertyInfo.FieldName}}")]
            {{propertyInfo.AccessModifier}}set => this.RaiseAndSetIfChanged(ref {{propertyInfo.FieldName}}, value);
        }
""";
        }

        return
$$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        {{propertyInfo.TargetInfo.TargetVisibility}}{{propertyInfo.Inheritance}} {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} { get => {{propertyInfo.FieldName}}; {{propertyInfo.AccessModifier}}set => this.RaiseAndSetIfChanged(ref {{propertyInfo.FieldName}}, value); }
""";
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
