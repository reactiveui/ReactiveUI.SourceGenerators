// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
#if ROSYLN_412 || ROSYLN_500
using Microsoft.CodeAnalysis.CSharp;
#endif
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates ReactiveUI-compatible properties from attributed fields and partial properties.</summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ReactiveGenerator
{
    /// <summary>Gets the fully-qualified name emitted in generated-code attributes.</summary>
    internal static readonly string GeneratorName = typeof(ReactiveGenerator).FullName!;

    /// <summary>Gets the generator assembly version emitted in generated-code attributes.</summary>
    internal static readonly string GeneratorVersion = typeof(ReactiveGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The access-modifier value for an internal setter.</summary>
    private const int InternalSetModifier = 2;

    /// <summary>The access-modifier value for a private setter.</summary>
    private const int PrivateSetModifier = 3;

    /// <summary>The access-modifier value for a protected-internal setter.</summary>
    private const int ProtectedInternalSetModifier = 4;

    /// <summary>The access-modifier value for a private-protected setter.</summary>
    private const int PrivateProtectedSetModifier = 5;

    /// <summary>The access-modifier value for an init-only setter.</summary>
    private const int InitSetModifier = 6;

    /// <summary>The inheritance value for an override property.</summary>
    private const int OverrideInheritanceModifier = 2;

    /// <summary>The inheritance value for a new property.</summary>
    private const int NewInheritanceModifier = 3;

#if ROSYLN_412 || ROSYLN_500
    /// <summary>Gets metadata for an attributed partial property.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The property metadata, or <see langword="null"/> when the property is unsupported.</returns>
    private static Result<PropertyInfo?>? GetPropertyInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType, out var attributeData))
        {
            return default;
        }

        if (symbol is not IPropertySymbol propertySymbol || !propertySymbol.IsPartialDefinition || propertySymbol.IsStatic)
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

        return new(CreatePartialPropertyInfo(context, propertySymbol, attributeData, builder, token), builder.ToImmutable());
    }

    /// <summary>Creates metadata for a valid attributed partial property.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="propertySymbol">The attributed property symbol.</param>
    /// <param name="attributeData">The reactive attribute.</param>
    /// <param name="builder">The diagnostic builder.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The generated property metadata.</returns>
    private static PropertyInfo CreatePartialPropertyInfo(
        in GeneratorAttributeSyntaxContext context,
        IPropertySymbol propertySymbol,
        AttributeData attributeData,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var propertyAccessModifier = GetAccessibilityText(propertySymbol.DeclaredAccessibility);
        var setAccessModifier = GetSetAccessModifier(propertySymbol, propertyAccessModifier);
        var inheritance = GetPropertyInheritance(propertySymbol);
        var fieldName = GetPartialPropertyFieldName(context, propertySymbol);
        propertySymbol.GetNullabilityInfo(context.SemanticModel, out var isReferenceTypeOrUnconstraindTypeParameter, out var includeMemberNotNullOnSetAccessor);
        context.GetForwardedAttributes(builder, propertySymbol, ((PropertyDeclarationSyntax)context.TargetNode).AttributeLists, token, out var forwardedAttributesString);
        token.ThrowIfCancellationRequested();
        return new(
            TargetInfo.From(propertySymbol.ContainingType),
            propertySymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations(),
            fieldName,
            propertySymbol.Name,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedAttributesString,
            setAccessModifier,
            inheritance,
            propertySymbol.IsRequired ? "required " : string.Empty,
            true,
            propertyAccessModifier,
            GetAlsoNotifyValues(attributeData, propertySymbol.Name, context.SemanticModel, token),
            GetXmlDocumentation(propertySymbol, token));
    }

    /// <summary>Gets normalized C# accessibility text.</summary>
    /// <param name="accessibility">The Roslyn accessibility value.</param>
    /// <returns>The corresponding C# accessibility text.</returns>
    private static string GetAccessibilityText(Accessibility accessibility)
    {
        var text = accessibility.ToString().ToLowerInvariant();
        return text.Contains("protectedandinternal", StringComparison.Ordinal)
            ? "private protected"
            : text.Replace("and", " ").Replace("or", " ");
    }

    /// <summary>Gets the generated setter modifier for a partial property.</summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="propertyAccessModifier">The generated property modifier.</param>
    /// <returns>The setter modifier text.</returns>
    private static string GetSetAccessModifier(IPropertySymbol propertySymbol, string propertyAccessModifier)
    {
        var setAccessModifier = $"{GetAccessibilityText(propertySymbol.SetMethod?.DeclaredAccessibility ?? Accessibility.Public)} set";
        return setAccessModifier == "public set" || setAccessModifier == $"{propertyAccessModifier} set"
            ? "set"
            : setAccessModifier;
    }

    /// <summary>Gets the inheritance modifier for a partial property.</summary>
    /// <param name="propertySymbol">The partial property symbol.</param>
    /// <returns>The inheritance modifier text.</returns>
    private static string GetPropertyInheritance(IPropertySymbol propertySymbol)
    {
        if (propertySymbol.IsVirtual)
        {
            return " virtual";
        }

        return propertySymbol.IsOverride ? " override" : string.Empty;
    }

    /// <summary>Gets the backing-field name used for an attributed partial property.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <returns>The field name supported by the target language version.</returns>
    private static string GetPartialPropertyFieldName(in GeneratorAttributeSyntaxContext context, IPropertySymbol propertySymbol) =>
        context.SemanticModel.Compilation is CSharpCompilation { LanguageVersion: > LanguageVersion.CSharp13 }
            ? "field"
            : propertySymbol.GetGeneratedFieldName();

    /// <summary>Formats symbol XML documentation for insertion into generated source.</summary>
    /// <param name="symbol">The documented symbol.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The formatted documentation, or an empty string.</returns>
    private static string GetXmlDocumentation(ISymbol symbol, CancellationToken token)
    {
        var xmlDocumentation = symbol.GetDocumentationCommentXml(cancellationToken: token) ?? string.Empty;
        if (xmlDocumentation.Length == 0)
        {
            return string.Empty;
        }

        var lines = xmlDocumentation.Split('\n');
        if (lines.Length < 3)
        {
            return string.Empty;
        }

        var formattedDocumentation = new System.Text.StringBuilder();
        const int XmlMemberEnvelopeLineCount = 2;
        for (var index = 1; index < lines.Length - XmlMemberEnvelopeLineCount; index++)
        {
            _ = formattedDocumentation.Append("        /// ")
                .AppendLine(lines[index].TrimStart());
        }

        return formattedDocumentation.ToString().TrimEnd();
    }
#endif

    /// <summary>Gets the observable method information.</summary>
    /// <param name="context">The context.</param>
    /// <param name="token">The token.</param>
    /// <returns>
    /// The value.
    /// </returns>
    private static Result<PropertyInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        if (!context.TargetSymbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveAttributeType, out var attributeData))
        {
            return default;
        }

        if (context.TargetSymbol is not IFieldSymbol fieldSymbol || !fieldSymbol.IsTargetTypeValid())
        {
            if (context.TargetSymbol is not IFieldSymbol invalidFieldSymbol)
            {
                return default;
            }

            builder.Add(
                    InvalidReactiveObjectError,
                    invalidFieldSymbol,
                    invalidFieldSymbol.ContainingType,
                    invalidFieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        var propertyInfo = CreateFieldPropertyInfo(context, fieldSymbol, attributeData, builder, token);
        return propertyInfo is null ? new(default, builder.ToImmutable()) : new(propertyInfo, builder.ToImmutable());
    }

    /// <summary>Creates metadata for a valid attributed field.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="fieldSymbol">The attributed field symbol.</param>
    /// <param name="attributeData">The reactive attribute.</param>
    /// <param name="builder">The diagnostic builder.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The generated property metadata, or <see langword="null"/> for a name collision.</returns>
    private static PropertyInfo? CreateFieldPropertyInfo(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        AttributeData attributeData,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        var propertyName = fieldSymbol.GetGeneratedPropertyName();
        if (fieldSymbol.Name == propertyName)
        {
            builder.Add(ReactivePropertyNameCollisionError, fieldSymbol, fieldSymbol.ContainingType, fieldSymbol.Name);
            return null;
        }

        token.ThrowIfCancellationRequested();
        fieldSymbol.GetNullabilityInfo(context.SemanticModel, out var isReferenceTypeOrUnconstraindTypeParameter, out var includeMemberNotNullOnSetAccessor);
        context.GetForwardedAttributes(builder, fieldSymbol, ((FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!).AttributeLists, token, out var forwardedAttributesString);
        token.ThrowIfCancellationRequested();
        return new(
            TargetInfo.From(fieldSymbol.ContainingType),
            fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations(),
            fieldSymbol.Name,
            propertyName,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedAttributesString,
            GetFieldSetAccessModifier(attributeData),
            GetFieldInheritance(attributeData),
            attributeData.GetNamedArgument<bool>("UseRequired") ? "required " : string.Empty,
            false,
            "public",
            GetAlsoNotifyValues(attributeData, propertyName, context.SemanticModel, token),
            string.Empty);
    }

    /// <summary>Gets the generated setter modifier for an attributed field.</summary>
    /// <param name="attributeData">The reactive attribute.</param>
    /// <returns>The setter modifier text.</returns>
    private static string GetFieldSetAccessModifier(AttributeData attributeData) =>
        attributeData.GetNamedArgument<int>("SetModifier") switch
        {
            1 => "protected set",
            InternalSetModifier => "internal set",
            PrivateSetModifier => "private set",
            ProtectedInternalSetModifier => "protected internal set",
            PrivateProtectedSetModifier => "private protected set",
            InitSetModifier => "init",
            _ => "set",
        };

    /// <summary>Gets the generated inheritance modifier for an attributed field.</summary>
    /// <param name="attributeData">The reactive attribute.</param>
    /// <returns>The inheritance modifier text.</returns>
    private static string GetFieldInheritance(AttributeData attributeData) =>
        attributeData.GetNamedArgument<int>("Inheritance") switch
        {
            1 => " virtual",
            OverrideInheritanceModifier => " override",
            NewInheritanceModifier => " new",
            _ => string.Empty,
        };

    /// <summary>Generates the source code.</summary>
    /// <param name="containingTypeName">The contain type name.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing class visibility.</param>
    /// <param name="containingType">The containing type.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The value.</returns>
    private static string GenerateSource(
        string containingTypeName,
        string containingNamespace,
        string containingClassVisibility,
        string containingType,
        PropertyInfo[] properties,
        ReactiveUiIntegration integration)
    {
        var parentTypes = new TargetInfo?[properties.Length];
        for (var index = 0; index < properties.Length; index++)
        {
            parentTypes[index] = properties[index].TargetInfo.ParentInfo;
        }

        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(parentTypes);

        var classes = GenerateClassWithProperties(containingTypeName, containingClassVisibility, containingType, properties);

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
    /// <returns>The value.</returns>
    private static string GenerateClassWithProperties(string containingTypeName, string containingClassVisibility, string containingType, PropertyInfo[] properties)
    {
        var propertyDeclarationsBuilder = new System.Text.StringBuilder();
        foreach (var property in properties)
        {
            if (propertyDeclarationsBuilder.Length > 0)
            {
                _ = propertyDeclarationsBuilder.AppendLine();
            }

            _ = propertyDeclarationsBuilder.Append(GetPropertySyntax(property));
        }

        var propertyDeclarations = propertyDeclarationsBuilder.ToString();

        return
$$"""
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
{{propertyDeclarations}}
    }
""";
    }

    /// <summary>Generates property declarations for the given observable method information.</summary>
    /// <param name="propertyInfo">Metadata about the observable property.</param>
    /// <returns>A string containing the generated code for the property.</returns>
    private static string GetPropertySyntax(PropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyName is null)
        {
            return string.Empty;
        }

        var partialModifier = propertyInfo.IsProperty ? "partial " : string.Empty;
        var getFieldName = propertyInfo.FieldName;
        var setFieldName = getFieldName == "value" ? "this.value" : getFieldName;
        var memberNotNullAttribute = GetMemberNotNullAttribute(propertyInfo, setFieldName);
        var propertyDeclaration = GetPropertyDeclaration(propertyInfo, partialModifier);
        var openingBrace = memberNotNullAttribute.Length > 0
            && propertyInfo.TypeNameWithNullabilityAnnotations.EndsWith("?", StringComparison.Ordinal)
            ? "{ "
            : "{";
        return $$"""
        {{GetFieldSyntax(propertyInfo)}}
{{GetDocumentationSyntax(propertyInfo, getFieldName)}}
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        {{GetPropertyAttributes(propertyInfo)}}
        {{propertyDeclaration}}
        {{openingBrace}}
            get => {{getFieldName}};
{{memberNotNullAttribute}}            {{propertyInfo.SetAccessModifier}}
            {
                this.RaiseAndSetIfChanged(ref {{setFieldName}}, value);{{GetAlsoNotifyStatements(propertyInfo.AlsoNotify)}}
            }
        }
""";
    }

    /// <summary>Gets the optional backing-field declaration for a generated property.</summary>
    /// <param name="propertyInfo">The generated property metadata.</param>
    /// <returns>The field declaration, or an empty string.</returns>
    private static string GetFieldSyntax(PropertyInfo propertyInfo) =>
        !propertyInfo.IsProperty || propertyInfo.FieldName == "field"
            ? string.Empty
            : $$"""
{{JoinIndentedLines(propertyInfo.ForwardedAttributes)}}
        private {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.FieldName}};
""";

    /// <summary>Gets the declaration line for a generated property.</summary>
    /// <param name="propertyInfo">The generated property metadata.</param>
    /// <param name="partialModifier">The partial modifier, when applicable.</param>
    /// <returns>The generated property declaration.</returns>
    private static string GetPropertyDeclaration(PropertyInfo propertyInfo, string partialModifier)
    {
        var modifiers = $"{propertyInfo.PropertyAccessModifier}{propertyInfo.Inheritance} {propertyInfo.UseRequired}{partialModifier}";
        return $"{modifiers}{propertyInfo.TypeNameWithNullabilityAnnotations} {propertyInfo.PropertyName}";
    }

    /// <summary>Gets the documentation declaration for a generated property.</summary>
    /// <param name="propertyInfo">The generated property metadata.</param>
    /// <param name="getFieldName">The generated getter field name.</param>
    /// <returns>The documentation declaration.</returns>
    private static string GetDocumentationSyntax(PropertyInfo propertyInfo, string getFieldName)
    {
        if (!propertyInfo.IsProperty)
        {
            return $$"""        /// <inheritdoc cref="{{getFieldName}}"/>""";
        }

        return string.IsNullOrWhiteSpace(propertyInfo.XmlComment)
            ? $$"""        /// <inheritdoc cref="{{propertyInfo.PropertyName}}"/>"""
            : propertyInfo.XmlComment!;
    }

    /// <summary>Gets the attributes applied to a generated property.</summary>
    /// <param name="propertyInfo">The generated property metadata.</param>
    /// <returns>The formatted attribute list.</returns>
    private static string GetPropertyAttributes(PropertyInfo propertyInfo)
    {
        var builder = new System.Text.StringBuilder();
        AppendIndentedLines(builder, AttributeDefinitions.ExcludeFromCodeCoverage);
        if (!propertyInfo.IsProperty)
        {
            AppendIndentedLines(builder, propertyInfo.ForwardedAttributes);
        }

        return builder.ToString();
    }

    /// <summary>Gets the optional member-not-null accessor attribute.</summary>
    /// <param name="propertyInfo">The generated property metadata.</param>
    /// <param name="setFieldName">The field assigned by the setter.</param>
    /// <returns>The formatted attribute prefix, or an empty string.</returns>
    private static string GetMemberNotNullAttribute(PropertyInfo propertyInfo, string setFieldName) =>
        propertyInfo.IncludeMemberNotNullOnSetAccessor || propertyInfo.IsReferenceTypeOrUnconstrainedTypeParameter
            ? $"            [global::System.Diagnostics.CodeAnalysis.MemberNotNull(\"{setFieldName}\")]\n"
            : string.Empty;

    /// <summary>Gets property-changed statements for additional notifications.</summary>
    /// <param name="alsoNotify">The additional property names.</param>
    /// <returns>The generated statements.</returns>
    private static string GetAlsoNotifyStatements(EquatableArray<string> alsoNotify)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var propertyName in alsoNotify.AsImmutableArray())
        {
            _ = builder.Append("\n                this.RaisePropertyChanged(nameof(")
                .Append(propertyName)
                .Append("));");
        }

        return builder.ToString();
    }

    /// <summary>Joins source lines with generated-property indentation.</summary>
    /// <param name="lines">The source lines.</param>
    /// <returns>The joined and indented lines.</returns>
    private static string JoinIndentedLines(IEnumerable<string> lines)
    {
        var builder = new System.Text.StringBuilder();
        AppendIndentedLines(builder, lines);
        return builder.ToString();
    }

    /// <summary>Appends generated-property source lines with indentation.</summary>
    /// <param name="builder">The destination builder.</param>
    /// <param name="lines">The source lines.</param>
    private static void AppendIndentedLines(System.Text.StringBuilder builder, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (builder.Length > 0)
            {
                _ = builder.Append("\n        ");
            }

            _ = builder.Append(line);
        }
    }

    /// <summary>Gets the additional property names notified by a reactive property update.</summary>
    /// <param name="attributeData">The reactive attribute.</param>
    /// <param name="propertyName">The generated property name.</param>
    /// <param name="semanticModel">The semantic model for syntax fallback.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The additional property names.</returns>
    private static EquatableArray<string> GetAlsoNotifyValues(AttributeData attributeData, string propertyName, SemanticModel semanticModel, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<string>.Rent();
        foreach (var notify in attributeData.GetConstructorArguments<string>())
        {
            AddAlsoNotifyValue(builder, notify, propertyName);
        }

        if (builder.Count == 0 && attributeData.ApplicationSyntaxReference?.GetSyntax(token) is AttributeSyntax attributeSyntax)
        {
            AddAlsoNotifySyntaxArguments(builder, attributeSyntax, propertyName, semanticModel, token);
        }

        return builder.ToImmutable();
    }

    /// <summary>Adds valid notification values represented in attribute syntax.</summary>
    /// <param name="builder">The destination builder.</param>
    /// <param name="attributeSyntax">The attribute syntax.</param>
    /// <param name="propertyName">The generated property name.</param>
    /// <param name="semanticModel">The semantic model for constant lookup.</param>
    /// <param name="token">The cancellation token.</param>
    private static void AddAlsoNotifySyntaxArguments(
        ImmutableArrayBuilder<string> builder,
        AttributeSyntax attributeSyntax,
        string propertyName,
        SemanticModel semanticModel,
        CancellationToken token)
    {
        if (attributeSyntax.ArgumentList is not { Arguments: var arguments })
        {
            return;
        }

        foreach (var argument in arguments)
        {
            var constantValue = semanticModel.GetConstantValue(argument.Expression, token);
            AddAlsoNotifyValue(builder, constantValue.HasValue ? constantValue.Value as string : null, propertyName);
        }
    }

    /// <summary>Adds one valid additional-notification value.</summary>
    /// <param name="builder">The destination builder.</param>
    /// <param name="value">The candidate property name.</param>
    /// <param name="propertyName">The generated property name.</param>
    private static void AddAlsoNotifyValue(ImmutableArrayBuilder<string> builder, string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, propertyName, StringComparison.Ordinal))
        {
            return;
        }

        builder.Add(value!);
    }
}
