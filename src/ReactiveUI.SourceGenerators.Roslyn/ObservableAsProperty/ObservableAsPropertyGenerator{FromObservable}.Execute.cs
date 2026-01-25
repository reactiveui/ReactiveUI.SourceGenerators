// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
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

        // Get the can InitialValue member, if any
        attributeData.TryGetNamedArgument("InitialValue", out string? initialValue);

        token.ThrowIfCancellationRequested();

        attributeData.TryGetNamedArgument("UseProtected", out bool useProtected);
        var useProtectedModifier = useProtected ? "protected" : "private";

        token.ThrowIfCancellationRequested();

        // Get the can ReadOnly member, if any
        attributeData.TryGetNamedArgument("ReadOnly", out bool? isReadonly);

        token.ThrowIfCancellationRequested();
        var compilation = context.SemanticModel.Compilation;

        if (context.TargetNode is MethodDeclarationSyntax methodSyntax)
        {
            var methodSymbol = (IMethodSymbol)symbol!;

            // Validate the target type
            if (!methodSymbol.IsTargetTypeValid())
            {
                diagnostics.Add(
                        InvalidReactiveObjectError,
                        methodSymbol,
                        methodSymbol.ContainingType,
                        methodSymbol.Name);
                return new(default, diagnostics.ToImmutable());
            }

            if (methodSymbol.Parameters.Length != 0)
            {
                diagnostics.Add(
                                ObservableAsPropertyMethodHasParametersError,
                                methodSymbol,
                                methodSymbol.Name);
                return new(default, diagnostics.ToImmutable());
            }

            var isObservable = methodSymbol.ReturnType.IsObservableReturnType();
            if (!isObservable)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();
            context.GetForwardedAttributes(
                diagnostics,
                methodSymbol,
                methodSyntax.AttributeLists,
                token,
                out var propertyAttributes);

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
                propertyAttributes,
                string.Empty,
                useProtectedModifier,
                initialValue),
                diagnostics.ToImmutable());
        }

        if (context.TargetNode is PropertyDeclarationSyntax propertySyntax)
        {
            if (symbol is not IPropertySymbol propertySymbol)
            {
                return default;
            }

            // Validate the target type
            if (!propertySymbol.IsTargetTypeValid())
            {
                diagnostics.Add(
                        InvalidReactiveObjectError,
                        propertySymbol,
                        propertySymbol.ContainingType,
                        propertySymbol.Name);
                return new(default, diagnostics.ToImmutable());
            }

            var observableType = string.Empty;
            var isNullableType = false;
            var isPartialProperty = false;

            token.ThrowIfCancellationRequested();
            context.GetForwardedAttributes(
                diagnostics,
                propertySymbol,
                propertySyntax.AttributeLists,
                token,
                out var propertyAttributes);

            token.ThrowIfCancellationRequested();

            if (propertySymbol.Type.IsObservableReturnType())
            {
                observableType = propertySymbol.Type is not INamedTypeSymbol typeSymbol
                    ? string.Empty
                    : typeSymbol.TypeArguments[0].GetFullyQualifiedNameWithNullabilityAnnotations();

                token.ThrowIfCancellationRequested();

                isNullableType = propertySymbol.Type is INamedTypeSymbol nullcheck && nullcheck.TypeArguments[0].IsNullableType();
            }
#if ROSYLN_412 || ROSYLN_500
            else
            {
                if (!propertySymbol.IsPartialDefinition || propertySymbol.IsStatic)
                {
                    return default;
                }

                // Validate the target type
                if (!propertySymbol.IsTargetTypeValid())
                {
                    diagnostics.Add(
                            InvalidReactiveObjectError,
                            propertySymbol,
                            propertySymbol.ContainingType,
                            propertySymbol.Name);
                    return new(default, diagnostics.ToImmutable());
                }

                token.ThrowIfCancellationRequested();

                isPartialProperty = true;

                var inheritance = propertySymbol.IsVirtual ? " virtual" : propertySymbol.IsOverride ? " override" : string.Empty;

                token.ThrowIfCancellationRequested();

                // Get the property type and name
                var typeNameWithNullabilityAnnotations = propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();

                // Get the field name
                var fieldName = propertySymbol.GetGeneratedFieldName();
                propertyName = propertySymbol.Name;

                // Check for names for collisions
                if (fieldName == propertyName)
                {
                    diagnostics.Add(
                        ReactivePropertyNameCollisionError,
                        propertySymbol,
                        propertySymbol.ContainingType,
                        propertySymbol.Name);
                    return new(default, diagnostics.ToImmutable());
                }

                var propertyDeclaration = (PropertyDeclarationSyntax)context.TargetNode!;

                token.ThrowIfCancellationRequested();

                context.GetForwardedAttributes(
                            diagnostics,
                            propertySymbol,
                            propertyDeclaration.AttributeLists,
                            token,
                            out var forwardedPropertyAttributes);

                token.ThrowIfCancellationRequested();

                observableType = "##FromPartialProperty##" + typeNameWithNullabilityAnnotations;
            }
#endif

            var isReadOnlyString = string.Empty;
            if (isPartialProperty)
            {
                isReadOnlyString = isReadonly == false ? string.Empty : "readonly";
            }

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
                propertyAttributes,
                isReadOnlyString,
                useProtectedModifier,
                initialValue),
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
        var propertyDeclarations = string.Join("\n", properties.Select(GetPropertySyntax));

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        {{propertyDeclarations}}

        {{GetPropertyInitiliser(properties)}}
    }
""";
    }

    private static string GetPropertySyntax(ObservableMethodInfo propertyInfo)
    {
        var propertyAttributes = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage.Concat(propertyInfo.ForwardedPropertyAttributes));
        var getterFieldIdentifierName = propertyInfo.GetGeneratedFieldName();
        var getterArrowExpression = propertyInfo.IsNullableType || propertyInfo.IsFromPartialProperty
            ? $"{getterFieldIdentifierName} = ({getterFieldIdentifierName}Helper == null ? {getterFieldIdentifierName} : {getterFieldIdentifierName}Helper.Value)"
            : $"{getterFieldIdentifierName} = {getterFieldIdentifierName}Helper?.Value ?? {getterFieldIdentifierName}";

        var isPartialProperty = string.Empty;
        var propertyType = propertyInfo.ObservableType;
        string? initVal;
        if (propertyType.EndsWith("##string") || propertyType.EndsWith("##string?"))
        {
            initVal = $""" = "{propertyInfo.InitialValue}";""";
        }
        else
        {
            initVal = $" = {propertyInfo.InitialValue};";
        }

        var initialValue = string.IsNullOrWhiteSpace(propertyInfo.InitialValue) ? ";" : initVal;
        if (propertyInfo.IsFromPartialProperty)
        {
            isPartialProperty = "partial ";
            propertyType = propertyInfo.PartialPropertyType;
        }

        var helperTypeName = $"{propertyInfo.AccessModifier} ReactiveUI.ObservableAsPropertyHelper<{propertyType}>?";

        // If the property is readonly, we need to change the helper to be non-nullable
        if (propertyInfo.IsReadOnly == "readonly")
        {
            helperTypeName = $"{propertyInfo.AccessModifier} readonly ReactiveUI.ObservableAsPropertyHelper<{propertyType}>";
        }

        return $$"""
/// <inheritdoc cref="{{propertyInfo.PropertyName}}"/>
        private {{propertyType}} {{getterFieldIdentifierName}}{{initialValue}}

        /// <inheritdoc cref="{{getterFieldIdentifierName}}Helper"/>
        {{helperTypeName}} {{getterFieldIdentifierName}}Helper;

        /// <inheritdoc cref="{{getterFieldIdentifierName}}"/>
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        {{propertyAttributes}}
        public {{isPartialProperty}}{{propertyType}} {{propertyInfo.PropertyName}} { get => {{getterArrowExpression}}; }
""";
    }

    private static string GetPropertyInitiliser(ObservableMethodInfo[] propertyInfos)
    {
        using var propertyInitilisers = ImmutableArrayBuilder<string>.Rent();

        foreach (var propertyInfo in propertyInfos)
        {
            if (propertyInfo.IsFromPartialProperty)
            {
                continue;
            }

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
