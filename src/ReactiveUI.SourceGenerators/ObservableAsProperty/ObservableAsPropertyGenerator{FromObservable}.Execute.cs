// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.ObservableAsProperty.Models;
using ReactiveUI.SourceGenerators.Reactive.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// Observable As Property From Observable Generator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ObservableAsPropertyGenerator
{
    private static Result<ObservableMethodInfo?>? GetObservableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var diagnostics = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        var attributeData = context.Attributes[0];

        // Get the can PropertyName member, if any
        attributeData.TryGetNamedArgument("PropertyName", out string? propertyName);

        token.ThrowIfCancellationRequested();
        var compilation = context.SemanticModel.Compilation;

        if (context.TargetNode is MethodDeclarationSyntax methodSyntax)
        {
            var methodSymbol = (IMethodSymbol)symbol!;
            if (methodSymbol.Parameters.Length != 0)
            {
                diagnostics.Add(
                                ObservableAsPropertyMethodHasParametersError,
                                methodSymbol,
                                methodSymbol.Name);
                return new(default, diagnostics.ToImmutable());
            }

            var isObservable = methodSymbol.ReturnType.IsObservableReturnType();

            token.ThrowIfCancellationRequested();

            methodSymbol.GatherForwardedAttributesFromMethod(
                context.SemanticModel,
                methodSyntax,
                token,
                out var attributes);
            var propertyAttributes = attributes.Select(x => x.ToString()).ToImmutableArray();

            token.ThrowIfCancellationRequested();

            var observableType = methodSymbol.ReturnType is not INamedTypeSymbol typeSymbol
                ? string.Empty
                : typeSymbol.TypeArguments[0].GetFullyQualifiedNameWithNullabilityAnnotations();

            var isNullableType = methodSymbol.ReturnType is INamedTypeSymbol nullcheck && nullcheck.TypeArguments[0].IsNullableType();

            token.ThrowIfCancellationRequested();

            // Get the containing type info
            var targetInfo = TargetInfo.From(methodSymbol.ContainingType);

            return new(
                new(
                targetInfo,
                methodSymbol.Name,
                methodSymbol.ReturnType.GetFullyQualifiedNameWithNullabilityAnnotations(),
                methodSymbol.Parameters.FirstOrDefault()?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                propertyName ?? (methodSymbol.Name + "Property"),
                observableType,
                isNullableType,
                false,
                propertyAttributes),
                diagnostics.ToImmutable());
        }

        if (context.TargetNode is PropertyDeclarationSyntax propertySyntax)
        {
            var propertySymbol = (IPropertySymbol)symbol!;
            var isObservable = propertySymbol.Type.IsObservableReturnType();

            token.ThrowIfCancellationRequested();

            propertySymbol.GatherForwardedAttributesFromProperty(
                context.SemanticModel,
                propertySyntax,
                token,
                out var attributes);
            var propertyAttributes = attributes.Select(x => x.ToString()).ToImmutableArray();

            token.ThrowIfCancellationRequested();

            var observableType = propertySymbol.Type is not INamedTypeSymbol typeSymbol
                ? string.Empty
                : typeSymbol.TypeArguments[0].GetFullyQualifiedNameWithNullabilityAnnotations();

            var isNullableType = propertySymbol.Type is INamedTypeSymbol nullcheck && nullcheck.TypeArguments[0].IsNullableType();

            token.ThrowIfCancellationRequested();

            // Get the containing type info
            var targetInfo = TargetInfo.From(propertySymbol.ContainingType);

            return new(
                new(
                targetInfo,
                propertySymbol.Name,
                propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations(),
                propertySymbol.Parameters.FirstOrDefault()?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                propertyName ?? (propertySymbol.Name + "Property"),
                observableType,
                isNullableType,
                true,
                propertyAttributes),
                diagnostics.ToImmutable());
        }

        return default;
    }

    private static string GenerateObservableSource(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, ObservableMethodInfo[] properties)
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
    private static string GenerateClassWithProperties(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, ObservableMethodInfo[] properties)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = string.Join("\n\r", properties.Select(GetPropertySyntax));

        return
$$"""
/// <summary>
    /// Partial class for the {{containingTypeName}} which contains ReactiveUI Observable As Property initialization.
    /// </summary>
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        {{propertyDeclarations}}

        {{GetPropertyInitiliser(properties)}}
    }
""";
    }

    private static string GetPropertySyntax(ObservableMethodInfo propertyInfo)
    {
        var propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedPropertyAttributes));
        var getterFieldIdentifierName = propertyInfo.GetGeneratedFieldName();
        var getterArrowExpression = propertyInfo.IsNullableType
            ? $"{getterFieldIdentifierName} = ({getterFieldIdentifierName}Helper == null ? {getterFieldIdentifierName} : {getterFieldIdentifierName}Helper.Value)"
            : $"{getterFieldIdentifierName} = {getterFieldIdentifierName}Helper?.Value ?? {getterFieldIdentifierName}";

        return $$"""
/// <inheritdoc cref="{{propertyInfo.PropertyName}}"/>
        private {{propertyInfo.ObservableType}} {{getterFieldIdentifierName}};

        /// <inheritdoc cref="{{getterFieldIdentifierName}}Helper"/>
        private ReactiveUI.ObservableAsPropertyHelper<{{propertyInfo.ObservableType}}>? {{getterFieldIdentifierName}}Helper;

        /// <inheritdoc cref="{{getterFieldIdentifierName}}"/>
        {{propertyAttributes}}
        public {{propertyInfo.ObservableType}} {{propertyInfo.PropertyName}} { get => {{getterArrowExpression}}; }
""";
    }

    private static string GetPropertyInitiliser(ObservableMethodInfo[] propertyInfos)
    {
        using var propertyInitilisers = ImmutableArrayBuilder<string>.Rent();

        foreach (var propertyInfo in propertyInfos)
        {
            var fieldIdentifierName = propertyInfo.GetGeneratedFieldName();
            if (propertyInfo.IsProperty)
            {
                propertyInitilisers.Add($"{fieldIdentifierName}Helper = {propertyInfo.MethodName}!.ToProperty(this, nameof({propertyInfo.PropertyName}));");
            }
            else
            {
                propertyInitilisers.Add($"{fieldIdentifierName}Helper = {propertyInfo.MethodName}()!.ToProperty(this, nameof({propertyInfo.PropertyName}));");
            }
        }

        return
$$"""
[global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected void InitializeOAPH()
        {
            {{string.Join("\n            ", propertyInitilisers.ToImmutable())}}
        }
""";
    }
}
