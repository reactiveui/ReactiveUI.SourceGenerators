// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Reads the named elements of a XAML page that a XAML source generator turns into fields.</summary>
/// <remarks>
/// <para>
/// MAUI and Avalonia declare a field for each named element with a source generator, so no other generator sees the
/// field. Both pass their XAML files to every generator as additional files, marked with item metadata. This reads the
/// files they mark, the way their generators do: the class from <c>x:Class</c>, a field per <c>x:Name</c> (and per
/// <c>Name</c> in Avalonia), and its accessibility from <c>x:FieldModifier</c> or the platform's default.
/// </para>
/// <para>
/// WPF and WinUI declare their fields in files their build writes before the compiler runs, which every generator sees
/// already, so their XAML is not read.
/// </para>
/// </remarks>
internal static class XamlPageReader
{
    /// <summary>The XAML language namespace, which declares <c>x:Class</c>, <c>x:Name</c> and <c>x:FieldModifier</c>.</summary>
    private const string XamlLanguageNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>The 2009 spelling of the XAML language namespace, which declares the same attributes.</summary>
    private const string XamlLanguageNamespace2009 = "http://schemas.microsoft.com/winfx/2009/xaml";

    /// <summary>The item metadata MAUI marks its XAML files with.</summary>
    private const string MauiKindKey = "build_metadata.AdditionalFiles.GenKind";

    /// <summary>The item metadata Avalonia marks its XAML files with.</summary>
    private const string AvaloniaItemGroupKey = "build_metadata.AdditionalFiles.SourceItemGroup";

    /// <summary>The build property that turns Avalonia's name generator off.</summary>
    private const string AvaloniaEnabledKey = "build_property.AvaloniaNameGeneratorIsEnabled";

    /// <summary>The build property that sets the accessibility of Avalonia's generated fields.</summary>
    private const string AvaloniaModifierKey = "build_property.AvaloniaNameGeneratorDefaultFieldModifier";

    /// <summary>The accessibility MAUI gives a field with no <c>x:FieldModifier</c>.</summary>
    private const string MauiDefaultAccessibility = "private";

    /// <summary>The accessibility Avalonia gives a field when the build does not set one.</summary>
    private const string AvaloniaDefaultAccessibility = "internal";

    /// <summary>The accessibility each <c>x:FieldModifier</c> spelling names; spellings are matched in any case.</summary>
    /// <remarks>A generator cannot count on the frozen collections, which older compilers do not load.</remarks>
    private static readonly (string Spelling, string Accessibility)[] Modifiers =
    [
        ("Public", "public"),
        ("Internal", AvaloniaDefaultAccessibility),
        ("NotPublic", AvaloniaDefaultAccessibility),
        ("Assembly", AvaloniaDefaultAccessibility),
        ("Protected", "protected"),
        ("Family", "protected"),
        ("ProtectedInternal", "protected internal"),
        ("Protected Internal", "protected internal"),
        ("FamOrAssem", "protected internal"),
        ("PrivateProtected", "private protected"),
        ("Private Protected", "private protected"),
        ("FamAndAssem", "private protected"),
        ("Private", MauiDefaultAccessibility),
    ];

    /// <summary>Reads a XAML page, when a XAML source generator reads it too.</summary>
    /// <param name="file">The additional file.</param>
    /// <param name="options">The file's item metadata.</param>
    /// <param name="globalOptions">The build's properties.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The page, or null when no XAML source generator reads the file or it declares no class.</returns>
    internal static XamlPage? Read(AdditionalText file, AnalyzerConfigOptions options, AnalyzerConfigOptions globalOptions, CancellationToken ct)
    {
        var avalonia = IsAvaloniaPage(options, globalOptions);
        if (!avalonia && !IsMauiPage(options))
        {
            return null;
        }

        var text = file.GetText(ct)?.ToString();
        return text is null ? null : Parse(text, avalonia, avalonia ? AvaloniaAccessibility(globalOptions) : MauiDefaultAccessibility);
    }

    /// <summary>Reads the class and the named elements of a XAML document.</summary>
    /// <param name="text">The document.</param>
    /// <param name="readsName">Whether a plain <c>Name</c> attribute names an element, as it does in Avalonia.</param>
    /// <param name="defaultAccessibility">The accessibility of a field with no <c>x:FieldModifier</c>.</param>
    /// <returns>The page, or null when the document is not well formed or declares no class.</returns>
    internal static XamlPage? Parse(string text, bool readsName, string defaultAccessibility)
    {
        XElement root;
        try
        {
            root = XDocument.Parse(text).Root!;
        }
        catch (XmlException)
        {
            // A page being edited is often not well formed. It gains its fields again once it is.
            return null;
        }

        if (LanguageAttribute(root, "Class")?.Value is not { Length: > 0 } className)
        {
            return null;
        }

        var elements = new List<XamlNamedElement>();
        foreach (var child in root.Elements())
        {
            Collect(child, readsName, defaultAccessibility, elements);
        }

        return new(className, new([.. elements]), ReadRoot(root));
    }

    /// <summary>Maps an <c>x:FieldModifier</c> value to accessibility keywords.</summary>
    /// <param name="modifier">The value, in any case.</param>
    /// <param name="defaultAccessibility">The accessibility when the value names none.</param>
    /// <returns>The accessibility keywords.</returns>
    internal static string Accessibility(string? modifier, string defaultAccessibility)
    {
        var spelling = modifier?.Trim();
        foreach (var (candidate, accessibility) in Modifiers)
        {
            if (string.Equals(candidate, spelling, StringComparison.OrdinalIgnoreCase))
            {
                return accessibility;
            }
        }

        return defaultAccessibility;
    }

    /// <summary>Reads the type of a page's root element, with the generic arguments <c>x:TypeArguments</c> gives it.</summary>
    /// <param name="root">The root element.</param>
    /// <returns>The type, or null when an argument is not a prefixed type name.</returns>
    private static XamlTypeReference? ReadRoot(XElement root)
    {
        List<XamlTypeReference> arguments = [];
        if (LanguageAttribute(root, "TypeArguments")?.Value is { } text)
        {
            foreach (var argument in text.Split(','))
            {
                if (ReadTypeArgument(root, argument.Trim()) is not { } reference)
                {
                    return null;
                }

                arguments.Add(reference);
            }
        }

        return new(root.Name.NamespaceName, root.Name.LocalName, new([.. arguments]));
    }

    /// <summary>Reads one generic argument, written as a type name with an optional XML namespace prefix.</summary>
    /// <param name="root">The root element, whose namespace declarations the prefix is looked up in.</param>
    /// <param name="argument">The argument, such as <c>vm:LoginViewModel</c>.</param>
    /// <returns>The type, or null when the prefix is unknown or the name is empty.</returns>
    private static XamlTypeReference? ReadTypeArgument(XElement root, string argument)
    {
        var colon = argument.IndexOf(':');
        if (colon == 0)
        {
            return null;
        }

        var name = colon < 0 ? argument : argument[(colon + 1)..];
        var xmlNamespace = colon < 0
            ? root.GetDefaultNamespace()
            : root.GetNamespaceOfPrefix(argument[..colon]);

        return name.Length == 0 || xmlNamespace is null ? null : new(xmlNamespace.NamespaceName, name, new([]));
    }

    /// <summary>Collects the named elements of an element and its children, skipping templates.</summary>
    /// <param name="element">The element.</param>
    /// <param name="readsName">Whether a plain <c>Name</c> attribute names an element.</param>
    /// <param name="defaultAccessibility">The accessibility of a field with no <c>x:FieldModifier</c>.</param>
    /// <param name="elements">The list to add to.</param>
    /// <remarks>
    /// A template's elements are created each time the template is applied, so they get no field. A property element,
    /// such as <c>Grid.ColumnDefinitions</c>, is not a type: its children are still collected.
    /// </remarks>
    private static void Collect(XElement element, bool readsName, string defaultAccessibility, List<XamlNamedElement> elements)
    {
        var localName = element.Name.LocalName;
        if (localName.EndsWith("Template", StringComparison.Ordinal))
        {
            return;
        }

        if (localName.IndexOf('.') < 0 && NameOf(element, readsName) is { } name)
        {
            elements.Add(new(
                name,
                Accessibility(LanguageAttribute(element, "FieldModifier")?.Value, defaultAccessibility),
                element.Name.NamespaceName,
                localName));
        }

        foreach (var child in element.Elements())
        {
            Collect(child, readsName, defaultAccessibility, elements);
        }
    }

    /// <summary>Reads the name an element is given.</summary>
    /// <param name="element">The element.</param>
    /// <param name="readsName">Whether a plain <c>Name</c> attribute names an element.</param>
    /// <returns>The name, or null when the element has none or its type is generic.</returns>
    private static string? NameOf(XElement element, bool readsName)
    {
        if (LanguageAttribute(element, "TypeArguments") is not null)
        {
            return null;
        }

        var name = LanguageAttribute(element, "Name")?.Value
            ?? (readsName ? element.Attribute("Name")?.Value : null);
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    /// <summary>Reads an attribute of the XAML language namespace, under either of its spellings.</summary>
    /// <param name="element">The element.</param>
    /// <param name="localName">The attribute's name without its prefix.</param>
    /// <returns>The attribute, or null when the element has none.</returns>
    private static XAttribute? LanguageAttribute(XElement element, string localName) =>
        element.Attribute(XName.Get(localName, XamlLanguageNamespace))
        ?? element.Attribute(XName.Get(localName, XamlLanguageNamespace2009));

    /// <summary>Determines whether MAUI's generator reads a file.</summary>
    /// <param name="options">The file's item metadata.</param>
    /// <returns><see langword="true"/> for a file MAUI marks as XAML.</returns>
    private static bool IsMauiPage(AnalyzerConfigOptions options) =>
        options.TryGetValue(MauiKindKey, out var kind) && string.Equals(kind, "Xaml", StringComparison.OrdinalIgnoreCase);

    /// <summary>Determines whether Avalonia's name generator reads a file.</summary>
    /// <param name="options">The file's item metadata.</param>
    /// <param name="globalOptions">The build's properties.</param>
    /// <returns><see langword="true"/> for a file Avalonia marks as XAML, while its name generator is on.</returns>
    private static bool IsAvaloniaPage(AnalyzerConfigOptions options, AnalyzerConfigOptions globalOptions) =>
        options.TryGetValue(AvaloniaItemGroupKey, out var group)
        && string.Equals(group, "AvaloniaXaml", StringComparison.OrdinalIgnoreCase)
        && !(globalOptions.TryGetValue(AvaloniaEnabledKey, out var enabled) && string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase));

    /// <summary>Reads the accessibility Avalonia gives a field with no <c>x:FieldModifier</c>.</summary>
    /// <param name="globalOptions">The build's properties.</param>
    /// <returns>The accessibility keywords.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AvaloniaAccessibility(AnalyzerConfigOptions globalOptions) =>
        Accessibility(globalOptions.TryGetValue(AvaloniaModifierKey, out var modifier) ? modifier : null, AvaloniaDefaultAccessibility);
}
