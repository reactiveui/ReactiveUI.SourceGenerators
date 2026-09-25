// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
#if ROSYLN_412 || ROSYLN_500
using Microsoft.CodeAnalysis.CSharp;
#endif
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
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

    /// <summary>The <c>GeneratedCode</c> attribute stamped on every generated property, built once.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);
#if ROSYLN_412 || ROSYLN_500
    /// <summary>Gets metadata for an attributed partial property.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The property metadata, or <see langword="null"/> when the property is unsupported.</returns>
    private static Result<PropertyInfo?>? GetPropertyInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        var attributeData = context.Attributes[0];

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
            context.TargetNode.HasDocumentationComment() ? GetXmlDocumentation(propertySymbol, token) : string.Empty);
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

        // The first and the last two lines are the <member> envelope around the documentation.
        const int XmlMemberEnvelopeLineCount = 2;
        var builder = PooledBuilder.Rent(xmlDocumentation.Length);
        for (var index = 1; index < lines.Length - XmlMemberEnvelopeLineCount; index++)
        {
            if (index > 1)
            {
                _ = builder.Append('\n');
            }

            _ = builder.Append("/// ").Append(lines[index].Trim());
        }

        return PooledBuilder.ToStringAndReturn(builder);
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
        var attributeData = context.Attributes[0];

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

    /// <summary>Generates the partial declaration holding one type's reactive properties.</summary>
    /// <param name="properties">The properties the type declares, all sharing one <see cref="TargetInfo"/>.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The file's text.</returns>
    private static string GenerateSource(EquatableArray<PropertyInfo> properties, ReactiveUiIntegration integration)
    {
        var target = properties[0].TargetInfo;
        var writer = SourceWriter.Rent()
            .AutoGenerated()
            .Lines(integration.UsingDirectives)
            .BlankLine()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(target.TargetNamespace) + writer.OpenContainingTypes(target.ParentInfo);
        _ = writer.OpenPartialType(target);
        for (var i = 0; i < properties.Count; i++)
        {
            if (i > 0)
            {
                _ = writer.BlankLine();
            }

            WriteProperty(writer, properties[i]);
        }

        return writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Writes one reactive property, and the backing field a partial property needs below C# 14.</summary>
    /// <param name="writer">The writer, at the level of the type's members.</param>
    /// <param name="propertyInfo">The property.</param>
    private static void WriteProperty(SourceWriter writer, PropertyInfo propertyInfo)
    {
        var getFieldName = propertyInfo.FieldName;
        var setFieldName = getFieldName == "value" ? "this.value" : getFieldName;

        if (propertyInfo.IsProperty && getFieldName != "field")
        {
            WriteLines(writer, propertyInfo.ForwardedAttributes);
            _ = writer.Append("private ").Append(propertyInfo.TypeNameWithNullabilityAnnotations).Append(' ').Append(getFieldName).EndStatement();
        }

        WriteDocumentation(writer, propertyInfo, getFieldName);
        _ = writer.Line(GeneratedCodeAttribute).ExcludeFromCodeCoverage();
        if (!propertyInfo.IsProperty)
        {
            WriteLines(writer, propertyInfo.ForwardedAttributes);
        }

        _ = writer.Append(propertyInfo.PropertyAccessModifier).Append(propertyInfo.Inheritance).Append(' ').Append(propertyInfo.UseRequired);
        if (propertyInfo.IsProperty)
        {
            _ = writer.Append("partial ");
        }

        _ = writer.Append(propertyInfo.TypeNameWithNullabilityAnnotations).Append(' ').Line(propertyInfo.PropertyName)
            .OpenBlock()
            .Append("get => ").Append(getFieldName).EndStatement();

        if (propertyInfo.IncludeMemberNotNullOnSetAccessor || propertyInfo.IsReferenceTypeOrUnconstrainedTypeParameter)
        {
            _ = writer.Append("[global::System.Diagnostics.CodeAnalysis.MemberNotNull(\"").Append(setFieldName).Line("\")]");
        }

        _ = writer.Line(propertyInfo.SetAccessModifier)
            .OpenBlock()
            .Append("this.RaiseAndSetIfChanged(ref ").Append(setFieldName).Line(", value);");

        foreach (var propertyName in propertyInfo.AlsoNotify.AsImmutableArray())
        {
            _ = writer.Append("this.RaisePropertyChanged(nameof(").Append(propertyName).Line("));");
        }

        _ = writer.CloseBlock().CloseBlock();
    }

    /// <summary>Writes a property's documentation: the partial property's own, or an <c>inheritdoc</c> of its source member.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="propertyInfo">The property.</param>
    /// <param name="getFieldName">The field the getter reads.</param>
    private static void WriteDocumentation(SourceWriter writer, PropertyInfo propertyInfo, string getFieldName)
    {
        if (!propertyInfo.IsProperty)
        {
            _ = writer.InheritDoc(getFieldName);
            return;
        }

        _ = string.IsNullOrWhiteSpace(propertyInfo.XmlComment)
            ? writer.InheritDoc(propertyInfo.PropertyName)
            : writer.Lines(propertyInfo.XmlComment!);
    }

    /// <summary>Writes each line on its own line at the writer's level.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="lines">The lines, such as forwarded attributes.</param>
    private static void WriteLines(SourceWriter writer, EquatableArray<string> lines)
    {
        foreach (var line in lines.AsImmutableArray())
        {
            _ = writer.Line(line);
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

        // Read in place rather than through an iterator: the names arrive as the params array's elements.
        foreach (var argument in attributeData.ConstructorArguments)
        {
            if (argument.Kind != TypedConstantKind.Array)
            {
                AddAlsoNotifyValue(builder, argument.Value as string, propertyName);
                continue;
            }

            foreach (var item in argument.Values)
            {
                AddAlsoNotifyValue(builder, item.Value as string, propertyName);
            }
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
            // Named arguments such as SetModifier are settings, never notified property names; evaluating them
            // would make the compiler bind and flow-analyse each one.
            if (argument.NameEquals is not null)
            {
                continue;
            }

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
