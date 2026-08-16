// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>Observable As Property From Observable Generator.</summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ObservableAsPropertyGenerator
{
    /// <summary>Creates metadata for an ObservableAsProperty-annotated method or property.</summary>
    /// <param name="context">The attribute syntax context for the source member.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The member metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ObservableMethodInfo?>? GetObservableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var diagnostics = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        var attributeData = context.Attributes[0];

        // Get the can PropertyName member, if any
        _ = attributeData.TryGetNamedArgument("PropertyName", out string? propertyName);

        // Get the can InitialValue member, if any
        _ = attributeData.TryGetNamedArgument("InitialValue", out string? initialValue);

        token.ThrowIfCancellationRequested();

        _ = attributeData.TryGetNamedArgument("UseProtected", out bool useProtected);
        var useProtectedModifier = useProtected ? "protected" : "private";

        token.ThrowIfCancellationRequested();

        // Get the can ReadOnly member, if any
        _ = attributeData.TryGetNamedArgument("ReadOnly", out bool? isReadonly);

        token.ThrowIfCancellationRequested();

        var settings = new ObservablePropertySettings(propertyName, initialValue, useProtectedModifier, isReadonly);
        return context.TargetNode switch
        {
            MethodDeclarationSyntax methodSyntax when symbol is IMethodSymbol methodSymbol => GetObservableMethodInfo(
                context,
                methodSyntax,
                methodSymbol,
                propertyName,
                initialValue,
                useProtectedModifier,
                token),
            PropertyDeclarationSyntax propertySyntax when symbol is IPropertySymbol propertySymbol => GetObservablePropertyInfo(
                context,
                propertySyntax,
                propertySymbol,
                in settings,
                diagnostics,
                token),
            _ => default,
        };
    }

    /// <summary>Creates metadata for an ObservableAsProperty-annotated property.</summary>
    /// <param name="context">The attribute syntax context for the property.</param>
    /// <param name="propertySyntax">The property declaration syntax.</param>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="settings">The settings declared by the attribute.</param>
    /// <param name="diagnostics">The builder that receives diagnostics.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The property metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ObservableMethodInfo?>? GetObservablePropertyInfo(
        in GeneratorAttributeSyntaxContext context,
        PropertyDeclarationSyntax propertySyntax,
        IPropertySymbol propertySymbol,
        in ObservablePropertySettings settings,
        ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        CancellationToken token)
    {
        if (!propertySymbol.IsTargetTypeValid())
        {
            diagnostics.Add(InvalidReactiveObjectError, propertySymbol, propertySymbol.ContainingType, propertySymbol.Name);
            return new(default, diagnostics.ToImmutable());
        }

        token.ThrowIfCancellationRequested();
        context.GetForwardedAttributes(diagnostics, propertySymbol, propertySyntax.AttributeLists, token, out var propertyAttributes);
        token.ThrowIfCancellationRequested();

        return propertySymbol.Type.IsObservableReturnType()
            ? CreateObservablePropertyInfo(
                propertySymbol,
                in settings,
                propertyAttributes,
                diagnostics)

#if ROSYLN_412 || ROSYLN_500
            : GetPartialObservablePropertyInfo(
                context,
                propertySyntax,
                propertySymbol,
                in settings,
                propertyAttributes,
                diagnostics,
                token);
#else
            : default;
#endif
    }

    /// <summary>Creates the metadata for a property that returns an observable.</summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="settings">The settings declared by the attribute.</param>
    /// <param name="propertyAttributes">The attributes to forward to the generated property.</param>
    /// <param name="diagnostics">The builder that receives diagnostics.</param>
    /// <returns>The property metadata and diagnostics.</returns>
    private static Result<ObservableMethodInfo?> CreateObservablePropertyInfo(
        IPropertySymbol propertySymbol,
        in ObservablePropertySettings settings,
        ImmutableArray<string> propertyAttributes,
        ImmutableArrayBuilder<DiagnosticInfo> diagnostics)
    {
        var observableType = GetObservableElementType(propertySymbol.Type);
        var isNullableType = IsObservableElementNullable(propertySymbol.Type);
        var targetInfo = TargetInfo.From(propertySymbol.ContainingType);
        return new(
            new(
                targetInfo,
                propertySymbol.Name,
                propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations(),
                propertySymbol.Parameters.FirstOrDefault()?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                settings.PropertyName ?? $"{propertySymbol.Name}Property",
                observableType,
                isNullableType,
                true,
                propertyAttributes,
                string.Empty,
                settings.UseProtectedModifier,
                settings.InitialValue),
            diagnostics.ToImmutable());
    }

    /// <summary>Gets the element type of an observable property.</summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The fully qualified observable element type, or an empty string when unavailable.</returns>
    private static string GetObservableElementType(ITypeSymbol propertyType) =>
        propertyType is INamedTypeSymbol typeSymbol
            ? typeSymbol.TypeArguments[0].GetFullyQualifiedNameWithNullabilityAnnotations()
            : string.Empty;

    /// <summary>Determines whether the element type of an observable property permits null.</summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns><see langword="true"/> when the observable element type permits null; otherwise, <see langword="false"/>.</returns>
    private static bool IsObservableElementNullable(ITypeSymbol propertyType) =>
        propertyType is INamedTypeSymbol typeSymbol && typeSymbol.TypeArguments[0].IsNullableType();

#if ROSYLN_412 || ROSYLN_500
    /// <summary>Creates metadata for a partial property that supplies its own observable value.</summary>
    /// <param name="context">The attribute syntax context for the property.</param>
    /// <param name="propertySyntax">The property declaration syntax.</param>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="settings">The settings declared by the attribute.</param>
    /// <param name="propertyAttributes">The attributes to forward to the generated property.</param>
    /// <param name="diagnostics">The builder that receives diagnostics.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The property metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ObservableMethodInfo?>? GetPartialObservablePropertyInfo(
        in GeneratorAttributeSyntaxContext context,
        PropertyDeclarationSyntax propertySyntax,
        IPropertySymbol propertySymbol,
        in ObservablePropertySettings settings,
        ImmutableArray<string> propertyAttributes,
        ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        CancellationToken token)
    {
        if (!propertySymbol.IsPartialDefinition || propertySymbol.IsStatic)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();
        var propertyType = propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldName = propertySymbol.GetGeneratedFieldName();
        var generatedPropertyName = propertySymbol.Name;
        if (fieldName == generatedPropertyName)
        {
            diagnostics.Add(
                ReactivePropertyNameCollisionError,
                propertySymbol,
                propertySymbol.ContainingType,
                propertySymbol.Name);
            return new(default, diagnostics.ToImmutable());
        }

        token.ThrowIfCancellationRequested();
        context.GetForwardedAttributes(diagnostics, propertySymbol, propertySyntax.AttributeLists, token, out _);
        token.ThrowIfCancellationRequested();

        var isReadOnly = settings.IsReadOnly == false ? string.Empty : "readonly";
        var targetInfo = TargetInfo.From(propertySymbol.ContainingType);
        return new(
            new(
                targetInfo,
                propertySymbol.Name,
                propertyType,
                propertySymbol.Parameters.FirstOrDefault()?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                generatedPropertyName,
                $"##FromPartialProperty##{propertyType}",
                false,
                true,
                propertyAttributes,
                isReadOnly,
                settings.UseProtectedModifier,
                settings.InitialValue),
            diagnostics.ToImmutable());
    }
#endif

    /// <summary>Creates metadata for an ObservableAsProperty-annotated method.</summary>
    /// <param name="context">The attribute syntax context for the method.</param>
    /// <param name="methodSyntax">The method declaration syntax.</param>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="propertyName">The optional generated property name.</param>
    /// <param name="initialValue">The optional generated property initial value.</param>
    /// <param name="useProtectedModifier">The generated helper accessibility.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The method metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ObservableMethodInfo?>? GetObservableMethodInfo(
        in GeneratorAttributeSyntaxContext context,
        MethodDeclarationSyntax methodSyntax,
        IMethodSymbol methodSymbol,
        string? propertyName,
        string? initialValue,
        string useProtectedModifier,
        CancellationToken token)
    {
        using var diagnostics = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        if (!methodSymbol.IsTargetTypeValid())
        {
            diagnostics.Add(InvalidReactiveObjectError, methodSymbol, methodSymbol.ContainingType, methodSymbol.Name);
            return new(default, diagnostics.ToImmutable());
        }

        if (!methodSymbol.Parameters.IsEmpty)
        {
            diagnostics.Add(ObservableAsPropertyMethodHasParametersError, methodSymbol, methodSymbol.Name);
            return new(default, diagnostics.ToImmutable());
        }

        if (!methodSymbol.ReturnType.IsObservableReturnType())
        {
            return default;
        }

        context.GetForwardedAttributes(diagnostics, methodSymbol, methodSyntax.AttributeLists, token, out var propertyAttributes);
        var observableType = methodSymbol.ReturnType is INamedTypeSymbol typeSymbol
            ? typeSymbol.TypeArguments[0].GetFullyQualifiedNameWithNullabilityAnnotations()
            : string.Empty;
        var isNullableType = methodSymbol.ReturnType is INamedTypeSymbol nullcheck && nullcheck.TypeArguments[0].IsNullableType();
        var targetInfo = TargetInfo.From(methodSymbol.ContainingType);
        return new(
            new(
                targetInfo,
                methodSymbol.Name,
                methodSymbol.ReturnType.GetFullyQualifiedNameWithNullabilityAnnotations(),
                methodSymbol.Parameters.FirstOrDefault()?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                propertyName ?? $"{methodSymbol.Name}Property",
                observableType,
                isNullableType,
                false,
                propertyAttributes,
                string.Empty,
                useProtectedModifier,
                initialValue),
            diagnostics.ToImmutable());
    }

    /// <summary>Generates the complete source document for method-backed observable properties.</summary>
    /// <param name="containingTypeName">The name of the containing type.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing type visibility.</param>
    /// <param name="containingType">The containing type kind.</param>
    /// <param name="properties">The generated property metadata.</param>
    /// <param name="integration">The selected ReactiveUI integration metadata.</param>
    /// <returns>The generated source document.</returns>
    private static string GenerateObservableSource(
        string containingTypeName,
        string containingNamespace,
        string containingClassVisibility,
        string containingType,
        ObservableMethodInfo[] properties,
        ReactiveUiIntegration integration)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(GetParentInfos(properties));

        var classes = GenerateClassWithProperties(containingTypeName, containingClassVisibility, containingType, properties, integration.DeclarationNamespace);

        return
$$"""
// <auto-generated/>
{{integration.UsingDirectives}}

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

    /// <summary>Generates the source code.</summary>
    /// <param name="containingTypeName">The contain type name.</param>
    /// <param name="containingClassVisibility">The containing class visibility.</param>
    /// <param name="containingType">The containing type.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="reactiveUiNamespace">The namespace containing the selected ReactiveUI implementation types.</param>
    /// <returns>The value.</returns>
    private static string GenerateClassWithProperties(
        string containingTypeName,
        string containingClassVisibility,
        string containingType,
        ObservableMethodInfo[] properties,
        string reactiveUiNamespace)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = GetPropertyDeclarations(properties, reactiveUiNamespace);

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        {{propertyDeclarations}}

        {{GetPropertyInitiliser(properties)}}
    }
""";
    }

    /// <summary>Generates a property declaration for an observable source member.</summary>
    /// <param name="propertyInfo">The source member metadata.</param>
    /// <param name="reactiveUiNamespace">The ReactiveUI implementation namespace.</param>
    /// <returns>The generated property declaration.</returns>
    private static string GetPropertySyntax(ObservableMethodInfo propertyInfo, string reactiveUiNamespace)
    {
        var propertyAttributes = GetPropertyAttributes(propertyInfo);
        var getterFieldIdentifierName = propertyInfo.GetGeneratedFieldName();
        var getterArrowExpression = propertyInfo.IsNullableType || propertyInfo.IsFromPartialProperty
            ? $"{getterFieldIdentifierName} = ({getterFieldIdentifierName}Helper == null ? {getterFieldIdentifierName} : {getterFieldIdentifierName}Helper.Value)"
            : $"{getterFieldIdentifierName} = {getterFieldIdentifierName}Helper?.Value ?? {getterFieldIdentifierName}";

        var isPartialProperty = string.Empty;
        var propertyType = propertyInfo.ObservableType;
        var initialValue = GetInitialValueSyntax(propertyType, propertyInfo.InitialValue);
        if (propertyInfo.IsFromPartialProperty)
        {
            isPartialProperty = "partial ";
            propertyType = propertyInfo.PartialPropertyType;
        }

        var helperTypeName = $"{propertyInfo.AccessModifier} {reactiveUiNamespace}.ObservableAsPropertyHelper<{propertyType}>?";

        // If the property is readonly, we need to change the helper to be non-nullable
        if (propertyInfo.IsReadOnly == "readonly")
        {
            helperTypeName = $"{propertyInfo.AccessModifier} readonly {reactiveUiNamespace}.ObservableAsPropertyHelper<{propertyType}>";
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

    /// <summary>Formats an optional initial value for an observable-backed generated field.</summary>
    /// <param name="propertyType">The generated property type or partial-property marker.</param>
    /// <param name="initialValue">The configured initial value.</param>
    /// <returns>The field initializer suffix.</returns>
    private static string GetInitialValueSyntax(string propertyType, string? initialValue)
    {
        var isNullableStringProperty = propertyType is "string?"
            || propertyType.EndsWith("##string?", System.StringComparison.Ordinal);
        var isStringProperty = isNullableStringProperty
            || propertyType is "string"
            || propertyType.EndsWith("##string", System.StringComparison.Ordinal);

        if (isStringProperty)
        {
            // A non nullable string field is initialised to an empty string when no initial value is
            // supplied, so the generated field never holds null. Empty and whitespace values are valid
            // string literals and are emitted as written.
            if (initialValue is not null)
            {
                return $" = {Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(initialValue, quote: true)};";
            }

            return isNullableStringProperty ? ";" : " = string.Empty;";
        }

        return string.IsNullOrWhiteSpace(initialValue)
            ? ";"
            : $" = {initialValue};";
    }

    /// <summary>Generates the initialization method for observable property helpers.</summary>
    /// <param name="propertyInfos">The observable property metadata.</param>
    /// <returns>The generated initialization method.</returns>
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

    /// <summary>Gets parent information for each generated property.</summary>
    /// <param name="properties">The generated property metadata.</param>
    /// <returns>The parent information for the generated properties.</returns>
    private static TargetInfo?[] GetParentInfos(ObservableMethodInfo[] properties)
    {
        var parentInfos = new TargetInfo?[properties.Length];
        for (var index = 0; index < properties.Length; index++)
        {
            parentInfos[index] = properties[index].TargetInfo.ParentInfo;
        }

        return parentInfos;
    }

    /// <summary>Gets the generated property declarations.</summary>
    /// <param name="properties">The generated property metadata.</param>
    /// <param name="reactiveUiNamespace">The ReactiveUI implementation namespace.</param>
    /// <returns>The generated property declarations.</returns>
    private static string GetPropertyDeclarations(ObservableMethodInfo[] properties, string reactiveUiNamespace)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < properties.Length; index++)
        {
            if (index > 0)
            {
                _ = builder.Append('\n');
            }

            _ = builder.Append(GetPropertySyntax(properties[index], reactiveUiNamespace));
        }

        return builder.ToString();
    }

    /// <summary>Gets attributes applied to a generated property.</summary>
    /// <param name="propertyInfo">The source member metadata.</param>
    /// <returns>The property attributes separated by generated-code indentation.</returns>
    private static string GetPropertyAttributes(ObservableMethodInfo propertyInfo)
    {
        var builder = new StringBuilder();
        foreach (var attribute in AttributeDefinitions.ExcludeFromCodeCoverage)
        {
            AppendObservablePropertyAttribute(builder, attribute);
        }

        foreach (var attribute in propertyInfo.ForwardedPropertyAttributes.AsImmutableArray())
        {
            AppendObservablePropertyAttribute(builder, attribute);
        }

        return builder.ToString();
    }

    /// <summary>Appends a generated property attribute with the required separator.</summary>
    /// <param name="builder">The destination builder.</param>
    /// <param name="attribute">The attribute source.</param>
    private static void AppendObservablePropertyAttribute(StringBuilder builder, string attribute)
    {
        if (builder.Length > 0)
        {
            _ = builder.Append("\n        ");
        }

        _ = builder.Append(attribute);
    }

    /// <summary>Groups the settings declared by an ObservableAsProperty attribute.</summary>
    /// <param name="PropertyName">The optional generated property name.</param>
    /// <param name="InitialValue">The optional generated property initial value.</param>
    /// <param name="UseProtectedModifier">The generated helper accessibility.</param>
    /// <param name="IsReadOnly">Whether a partial generated property is read-only.</param>
    private readonly record struct ObservablePropertySettings(
        string? PropertyName,
        string? InitialValue,
        string UseProtectedModifier,
        bool? IsReadOnly);
}
