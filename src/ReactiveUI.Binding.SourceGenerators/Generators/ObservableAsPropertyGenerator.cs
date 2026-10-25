// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>Writes the body of each partial property marked <c>[ObservableAsProperty]</c>, and its helper field.</summary>
/// <remarks>
/// <para>
/// The attribute is a runtime type, so nothing is declared into the consumer's assembly: a friend assembly granted
/// <c>InternalsVisibleTo</c> never sees a second copy of it. The generator only adds members to a partial type the
/// consumer declared.
/// </para>
/// <para>
/// Only a partial property is accepted. Its declaration is the consumer's own source, so every generator in the build,
/// this one included, sees the property and can observe or bind it. A property that a generator writes from a field
/// would be invisible to the other generators, which is the failure this attribute exists to avoid.
/// </para>
/// </remarks>
internal static class ObservableAsPropertyGenerator
{
    /// <summary>The attribute's metadata name in the lean runtime.</summary>
    private const string LeanAttributeName = "ReactiveUI.Binding.ObservableAsPropertyAttribute";

    /// <summary>The attribute's metadata name in the System.Reactive runtime.</summary>
    private const string ReactiveAttributeName = "ReactiveUI.Binding.Reactive.ObservableAsPropertyAttribute";

    /// <summary>The helper type the generated field holds.</summary>
    private const string HelperType = GeneratedTypeNames.ObservableAsPropertyHelper;

    /// <summary>The prefix a fully qualified type name starts with.</summary>
    private const string GlobalPrefix = "global::";

    /// <summary>The suffix every generated file name ends with.</summary>
    private const string HintSuffix = ".ObservableAsProperties.g.cs";

    /// <summary>The type format a partial property's implementing part has to repeat exactly, nullable annotation included.</summary>
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>Registers the pipeline for both runtime flavours' attribute.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="languageFeatures">The consumer compilation's language-feature snapshot.</param>
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<LanguageFeatures> languageFeatures)
    {
        // Converted once and shared, so both flavours' registrations use the same two delegates.
        Func<SyntaxNode, CancellationToken, bool> isCandidate = IsCandidate;
        Func<GeneratorAttributeSyntaxContext, CancellationToken, ObservableAsPropertyInfo?> extract = Extract;
        var lean = context.SyntaxProvider.ForAttributeWithMetadataName(LeanAttributeName, isCandidate, extract).Collect();
        var reactive = context.SyntaxProvider.ForAttributeWithMetadataName(ReactiveAttributeName, isCandidate, extract).Collect();

        context.RegisterSourceOutput(
            lean.Combine(reactive).Combine(languageFeatures),
            static (ctx, data) => Emit(in ctx, data.Left.Left, data.Left.Right, data.Right));
    }

    /// <summary>Accepts only a partial property whose sole accessor is a bodiless getter, from syntax alone.</summary>
    /// <param name="node">The syntax node the attribute is on.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> for <c>partial T Name { get; }</c>.</returns>
    internal static bool IsCandidate(SyntaxNode node, CancellationToken ct) =>
        node is PropertyDeclarationSyntax
        {
            ExpressionBody: null,
            AccessorList.Accessors: [{ RawKind: (int)SyntaxKind.GetAccessorDeclaration, Body: null, ExpressionBody: null }],
        } property
        && property.Modifiers.Any(SyntaxKind.PartialKeyword);

    /// <summary>Reads a candidate property into its model.</summary>
    /// <param name="context">The attribute syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The model, or null when the property's type is not partial all the way out.</returns>
    internal static ObservableAsPropertyInfo? Extract(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        // A static property has no instance to hold its helper; the analyzer reports it.
        if (context.TargetSymbol is not IPropertySymbol { IsStatic: false, ContainingType: { } type } property
            || PartialTypeRaisePlugin.DescribeDeclaration(type) is not { } declaration)
        {
            return null;
        }

        var options = ObservableAsPropertyOptions.Read(context.Attributes[0]);
        var (initialValue, storesInitialValue) = InitialValueFor(property.Type, options.InitialValue);
        var syntax = (PropertyDeclarationSyntax)context.TargetNode;
        return new(
            declaration,
            HintNameFor(type),
            syntax.Modifiers.ToString(),
            property.Type.ToDisplayString(TypeFormat),
            property.Name,
            (options.UseProtected ? "protected" : "private") + (options.ReadOnly ? " readonly" : string.Empty),
            initialValue,
            storesInitialValue,
            storesInitialValue ? UsingsOf(syntax) : new([]));
    }

    /// <summary>Collects the directives an initial value expression was written against, in the order they appear.</summary>
    /// <param name="property">The property declaration.</param>
    /// <returns>The <c>extern alias</c> directives, then the <c>using</c> directives of the file and its namespaces.</returns>
    /// <remarks>
    /// The expression is copied into a generated file of its own, so it only binds the way it did in the consumer's file
    /// when that file's directives come with it. Namespace-level directives are lifted to the top of the generated file.
    /// </remarks>
    internal static EquatableArray<string> UsingsOf(PropertyDeclarationSyntax property)
    {
        var directives = new List<string>();
        var unit = (CompilationUnitSyntax)property.SyntaxTree.GetRoot();
        foreach (var alias in unit.Externs)
        {
            directives.Add(alias.ToString().Trim());
        }

        foreach (var directive in unit.Usings)
        {
            directives.Add(directive.ToString().Trim());
        }

        for (var node = property.Parent; node is not null; node = node.Parent)
        {
            if (node is BaseNamespaceDeclarationSyntax scope)
            {
                foreach (var directive in scope.Usings)
                {
                    directives.Add(directive.ToString().Trim());
                }
            }
        }

        return new([.. directives]);
    }

    /// <summary>Works out what a property returns before its helper is assigned.</summary>
    /// <param name="type">The property type.</param>
    /// <param name="initialValue">The attribute's <c>InitialValue</c>, or null.</param>
    /// <returns>
    /// The expression, or null for the type's default, and whether it is held in a backing field. A string property's
    /// text is its value, so it is written as a literal. Any other type's text is an expression, evaluated once.
    /// </returns>
    internal static (string? Expression, bool Stores) InitialValueFor(ITypeSymbol type, string? initialValue)
    {
        var isString = type.SpecialType == SpecialType.System_String;
        if (initialValue is not null)
        {
            return isString ? (SymbolDisplay.FormatLiteral(initialValue, true), false) : (initialValue, true);
        }

        // A non-nullable string never reports null, so it starts empty.
        return isString && type.NullableAnnotation != NullableAnnotation.Annotated
            ? ("global::System.String.Empty", false)
            : (null, false);
    }

    /// <summary>Writes one file per type, holding every marked property the type declares.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="lean">The properties marked with the lean runtime's attribute.</param>
    /// <param name="reactive">The properties marked with the System.Reactive runtime's attribute.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void Emit(
        in SourceProductionContext context,
        ImmutableArray<ObservableAsPropertyInfo?> lean,
        ImmutableArray<ObservableAsPropertyInfo?> reactive,
        in LanguageFeatures features)
    {
        var byType = new Dictionary<string, List<ObservableAsPropertyInfo>>(StringComparer.Ordinal);
        var order = new List<string>();
        Group(lean, byType, order);
        Group(reactive, byType, order);

        for (var i = 0; i < order.Count; i++)
        {
            var properties = byType[order[i]];
            CodeGeneratorHelpers.AddGeneratedSource(context, order[i], Generate(properties, features), features);
        }
    }

    /// <summary>Collects properties under the file their type is written to.</summary>
    /// <param name="properties">The extracted properties, with nulls for declined candidates.</param>
    /// <param name="byType">The properties per file.</param>
    /// <param name="order">The files in first-seen order, so output is deterministic.</param>
    private static void Group(
        ImmutableArray<ObservableAsPropertyInfo?> properties,
        Dictionary<string, List<ObservableAsPropertyInfo>> byType,
        List<string> order)
    {
        for (var i = 0; i < properties.Length; i++)
        {
            if (properties[i] is not { } property)
            {
                continue;
            }

            if (!byType.TryGetValue(property.HintName, out var list))
            {
                list = [];
                byType[property.HintName] = list;
                order.Add(property.HintName);
            }

            list.Add(property);
        }
    }

    /// <summary>Writes the partial declarations holding each property's helper field and body.</summary>
    /// <param name="properties">The properties one type declares.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <returns>The file's text.</returns>
    private static string Generate(List<ObservableAsPropertyInfo> properties, in LanguageFeatures features)
    {
        var declaration = properties[0].Declaration;
        var sb = SourceWriter.Rent(CodeGeneratorHelpers.PerInvocationBufferCapacity * properties.Count)
            .FileHeader(features.EmitGeneratedCodeMarkers, enableNullable: true)
            .BlankLine();

        if (AppendUsings(sb, properties))
        {
            _ = sb.BlankLine();
        }

        CodeGeneratorHelpers.OpenPartialDeclaration(sb, declaration);
        for (var i = 0; i < properties.Count; i++)
        {
            if (i > 0)
            {
                _ = sb.BlankLine();
            }

            AppendProperty(sb, properties[i]);
        }

        CodeGeneratorHelpers.ClosePartialDeclaration(sb, declaration);

        return sb.ToStringAndReturn();
    }

    /// <summary>Writes each directive the type's initial values need, once.</summary>
    /// <param name="sb">The writer, at the top of the file.</param>
    /// <param name="properties">The properties one type declares.</param>
    /// <returns><see langword="true"/> when any directive was written.</returns>
    private static bool AppendUsings(SourceWriter sb, List<ObservableAsPropertyInfo> properties)
    {
        HashSet<string>? written = null;
        for (var i = 0; i < properties.Count; i++)
        {
            var usings = properties[i].Usings;
            for (var j = 0; j < usings.Length; j++)
            {
                written ??= [with(StringComparer.Ordinal)];
                if (written.Add(usings[j]))
                {
                    _ = sb.Line(usings[j]);
                }
            }
        }

        return written is not null;
    }

    /// <summary>Writes one property's helper field and the body that reads it.</summary>
    /// <param name="sb">The writer, at the level of the type's members.</param>
    /// <param name="property">The property.</param>
    private static void AppendProperty(SourceWriter sb, ObservableAsPropertyInfo property)
    {
        var name = property.PropertyName;
        if (property.StoresInitialValue)
        {
            _ = sb.Append("/// <summary>The value <see cref=\"").Append(name).Line("\"/> returns until its helper is assigned.</summary>")
                .Append("private readonly ").Append(property.TypeFullName).Append(' ');
            _ = AppendFieldName(sb, name).Append(" = ").Append(property.InitialValue!).EndStatement()
                .BlankLine();
        }

        _ = sb.Append("/// <summary>Backs <see cref=\"").Append(name).Line("\"/>; assign it with <c>ToProperty</c>.</summary>")
            .Append(property.HelperModifiers).Append(' ').Append(HelperType).Append('<').Append(property.TypeFullName).Append(">? ");
        _ = AppendFieldName(sb, name).Append("Helper").EndStatement()
            .BlankLine()
            .Append(property.Modifiers).Append(' ').Append(property.TypeFullName).Append(' ').Append(name).Append(" => ");
        _ = AppendFieldName(sb, name).Append("Helper is null ? ");
        _ = property.StoresInitialValue
            ? AppendFieldName(sb, name)
            : sb.Append(property.InitialValue ?? "default!");
        _ = AppendFieldName(sb.Append(" : "), name).Line("Helper.Value;");
    }

    /// <summary>Appends a field name for a property, <c>_{name}</c> with the first letter lowered, without building a string for it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The builder, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SourceWriter AppendFieldName(SourceWriter sb, string propertyName) =>
        sb.Append('_').Append(char.ToLowerInvariant(propertyName[0])).Append(propertyName, 1, propertyName.Length - 1);

    /// <summary>Names a type's generated file from its metadata name, which is unique within the compilation.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The hint name.</returns>
    private static string HintNameFor(INamedTypeSymbol type)
    {
        // The display string is the one unavoidable allocation; the hint name is built from it in a pooled buffer.
        var display = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var name = new PooledStringBuilder(display.Length + HintSuffix.Length);
        for (var i = GlobalPrefix.Length; i < display.Length; i++)
        {
            var c = display[i];
            _ = c switch
            {
                '<' => name.Append('['),
                '>' => name.Append(']'),
                ' ' => name,
                _ => name.Append(c),
            };
        }

        return name.Append(HintSuffix).ToStringAndReturn();
    }
}
