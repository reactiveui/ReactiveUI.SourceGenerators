// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.Extensions;

internal static class ContextExtensions
{
    internal static void GetForwardedAttributes(
        this in GeneratorAttributeSyntaxContext context,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        ISymbol symbol,
        in SyntaxList<AttributeListSyntax> attributeListSyntaxes,
        CancellationToken token,
        out ImmutableArray<string> forwardedAttributes)
    {
        using var forwardedAttributeBuilder = ImmutableArrayBuilder<AttributeInfo>.Rent();

        // Gather attributes info
        foreach (var attribute in symbol.GetAttributes())
        {
            token.ThrowIfCancellationRequested();

            // Track the current attribute for forwarding if it is a validation attribute
            if (attribute.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ValidationAttribute") == true)
            {
                forwardedAttributeBuilder.Add(AttributeInfo.Create(attribute));
            }

            // Track the current attribute for forwarding if it is a Json Serialization attribute
            if (attribute.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.Text.Json.Serialization.JsonAttribute") == true)
            {
                forwardedAttributeBuilder.Add(AttributeInfo.Create(attribute));
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
                forwardedAttributeBuilder.Add(AttributeInfo.Create(attribute));
            }
        }

        token.ThrowIfCancellationRequested();

        // Gather explicit forwarded attributes info
        foreach (var attributeList in attributeListSyntaxes)
        {
            // Only look for attribute lists explicitly targeting the (generated) property. Roslyn will normally emit a
            // CS0657 warning (invalid target), but that is automatically suppressed by a dedicated diagnostic suppressor
            // that recognizes uses of this target specifically to support [ObservableAsProperty].
            if (attributeList.Target?.Identifier is not SyntaxToken(SyntaxKind.PropertyKeyword) && attributeList.Target?.Identifier is not SyntaxToken(SyntaxKind.FieldKeyword))
            {
                continue;
            }

            token.ThrowIfCancellationRequested();

            foreach (var attribute in attributeList.Attributes)
            {
                if (!context.SemanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeTypeSymbol))
                {
                    builder.Add(
                            InvalidPropertyTargetedAttributeOnObservableAsPropertyField,
                            attribute,
                            symbol,
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
                            symbol,
                            attribute.Name);
                    continue;
                }

                forwardedAttributeBuilder.Add(attributeInfo);
            }
        }

        var attributes = forwardedAttributeBuilder.ToImmutable();
        forwardedAttributes = attributes.Select(static a => a.ToString()).ToImmutableArray();
    }
}
