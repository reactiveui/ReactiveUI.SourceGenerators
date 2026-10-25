// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Turns the named elements of XAML pages into field declarations of their code-behind classes.</summary>
/// <remarks>
/// An element's XML namespace names its type's CLR namespace directly (<c>clr-namespace:</c> or <c>using:</c>), or is a
/// URI that assemblies map to CLR namespaces with <c>XmlnsDefinition</c> attributes. MAUI and Avalonia each declare their
/// own <c>XmlnsDefinitionAttribute</c>, so the attribute is matched by name.
/// </remarks>
internal static class XamlMemberResolver
{
    /// <summary>The name of the attribute that maps an XML namespace URI to a CLR namespace.</summary>
    private const string XmlnsDefinitionName = "XmlnsDefinitionAttribute";

    /// <summary>The prefix of an XML namespace that names a CLR namespace, as WPF and MAUI write it.</summary>
    private const string ClrNamespacePrefix = "clr-namespace:";

    /// <summary>The prefix of an XML namespace that names a CLR namespace, as Avalonia writes it.</summary>
    private const string UsingPrefix = "using:";

    /// <summary>Declares the fields the named elements of each page become.</summary>
    /// <param name="pages">The pages.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A field per named element whose class and type resolve and that the class does not already declare.</returns>
    internal static ImmutableArray<SourceGeneratorsMember?> Resolve(ImmutableArray<XamlPage> pages, Compilation compilation, CancellationToken ct)
    {
        if (pages.IsEmpty)
        {
            return ImmutableArray<SourceGeneratorsMember?>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<SourceGeneratorsMember?>();
        Dictionary<string, List<string>>? namespaces = null;
        foreach (var page in pages)
        {
            ct.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(page.ClassName) is not { } owner
                || PartialTypeRaisePlugin.DescribeDeclaration(owner) is not { } declaration)
            {
                continue;
            }

            foreach (var element in page.Elements)
            {
                // A field the class declares already, as the XAML compilers that run before the compiler write them,
                // is seen as it is; declaring it again would make it ambiguous.
                if (!owner.GetMembers(element.Name).IsEmpty
                    || FindType(element, compilation, ref namespaces) is not { } type)
                {
                    continue;
                }

                builder.Add(new(
                    declaration,
                    null,
                    $"{element.Accessibility} {type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {element.Name};"));
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>Determines whether an attribute maps an XML namespace URI to a CLR namespace.</summary>
    /// <param name="attributeClass">The attribute's class, which is null when it could not be read.</param>
    /// <returns><see langword="true"/> for any class named <c>XmlnsDefinitionAttribute</c>.</returns>
    internal static bool IsXmlnsDefinition(INamedTypeSymbol? attributeClass) =>
        attributeClass is { Name: XmlnsDefinitionName };

    /// <summary>Finds the type an element names.</summary>
    /// <param name="element">The element.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="namespaces">The CLR namespaces each XML namespace URI maps to, read on first use.</param>
    /// <returns>The type, or null when it does not resolve.</returns>
    private static INamedTypeSymbol? FindType(XamlNamedElement element, Compilation compilation, ref Dictionary<string, List<string>>? namespaces)
    {
        if (ClrNamespace(element.XmlNamespace) is { } clrNamespace)
        {
            return compilation.GetTypeByMetadataName($"{clrNamespace}.{element.TypeName}");
        }

        namespaces ??= ReadXmlnsDefinitions(compilation);
        if (!namespaces.TryGetValue(element.XmlNamespace, out var candidates))
        {
            return null;
        }

        foreach (var candidate in candidates)
        {
            if (compilation.GetTypeByMetadataName($"{candidate}.{element.TypeName}") is { } type)
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>Reads the CLR namespace an XML namespace names directly.</summary>
    /// <param name="xmlNamespace">The XML namespace.</param>
    /// <returns>The CLR namespace, or null when the XML namespace is a URI.</returns>
    private static string? ClrNamespace(string xmlNamespace)
    {
        int prefix;
        if (xmlNamespace.StartsWith(ClrNamespacePrefix, StringComparison.Ordinal))
        {
            prefix = ClrNamespacePrefix.Length;
        }
        else if (xmlNamespace.StartsWith(UsingPrefix, StringComparison.Ordinal))
        {
            prefix = UsingPrefix.Length;
        }
        else
        {
            return null;
        }

        var end = xmlNamespace.IndexOf(';', prefix);
        return end < 0 ? xmlNamespace[prefix..] : xmlNamespace[prefix..end];
    }

    /// <summary>Reads every <c>XmlnsDefinition</c> the compilation and its references declare.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The CLR namespaces each XML namespace URI maps to, in the order they are declared.</returns>
    private static Dictionary<string, List<string>> ReadXmlnsDefinitions(Compilation compilation)
    {
        var map = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        AddDefinitions(compilation.Assembly, map);
        foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            AddDefinitions(reference, map);
        }

        return map;
    }

    /// <summary>Adds the <c>XmlnsDefinition</c> attributes one assembly declares.</summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="map">The map to add to.</param>
    private static void AddDefinitions(IAssemblySymbol assembly, Dictionary<string, List<string>> map)
    {
        foreach (var attribute in assembly.GetAttributes())
        {
            if (!IsXmlnsDefinition(attribute.AttributeClass)
                || attribute.ConstructorArguments is not [{ Value: string uri }, { Value: string clrNamespace }, ..])
            {
                continue;
            }

            if (!map.TryGetValue(uri, out var list))
            {
                list = [];
                map.Add(uri, list);
            }

            list.Add(clrNamespace);
        }
    }
}
