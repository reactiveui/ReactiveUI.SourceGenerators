// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    internal static EquatableArray<SourceGeneratorsMember> Resolve(XamlPage[] pages, Compilation compilation, CancellationToken ct)
    {
        if (pages.Length == 0)
        {
            return new([]);
        }

        List<SourceGeneratorsMember> builder = [];
        Dictionary<string, List<string>>? namespaces = null;
        foreach (var page in pages)
        {
            ct.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(page.ClassName) is not { } owner
                || PartialTypeRaisePlugin.DescribeDeclaration(owner) is not { } declaration)
            {
                continue;
            }

            // The XAML source generator makes the root element's type the base of a code-behind class that names none,
            // in a file this generator cannot read. Declaring the base lets a view's ViewModel and IViewFor<T> be seen.
            if (owner.BaseType is { SpecialType: SpecialType.System_Object }
                && FindRootType(page.Root, compilation, ref namespaces) is { } root)
            {
                builder.Add(new(declaration, root.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), null));
            }

            foreach (var element in page.Elements)
            {
                // A field the class declares already, as the XAML compilers that run before the compiler write them,
                // is seen as it is; declaring it again would make it ambiguous.
                if (!owner.GetMembers(element.Name).IsEmpty
                    || FindType(element.XmlNamespace, element.TypeName, compilation, ref namespaces) is not { } type)
                {
                    continue;
                }

                builder.Add(new(
                    declaration,
                    null,
                    $"{element.Accessibility} {type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {element.Name};"));
            }
        }

        return new([.. builder]);
    }

    /// <summary>Determines whether an attribute maps an XML namespace URI to a CLR namespace.</summary>
    /// <param name="attributeClass">The attribute's class, which is null when it could not be read.</param>
    /// <returns><see langword="true"/> for any class named <c>XmlnsDefinitionAttribute</c>.</returns>
    internal static bool IsXmlnsDefinition(INamedTypeSymbol? attributeClass) =>
        attributeClass is { Name: XmlnsDefinitionName };

    /// <summary>Finds the type a page's root element names, closed over its <c>x:TypeArguments</c>.</summary>
    /// <param name="root">The root element's type, or null when it could not be read.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="namespaces">The CLR namespaces each XML namespace URI maps to, read on first use.</param>
    /// <returns>The type, or null when it or one of its arguments does not resolve.</returns>
    private static INamedTypeSymbol? FindRootType(XamlTypeReference? root, Compilation compilation, ref Dictionary<string, List<string>>? namespaces)
    {
        if (root is null)
        {
            return null;
        }

        var arity = root.TypeArguments.Length;
        var metadataName = arity == 0 ? root.TypeName : $"{root.TypeName}`{arity}";
        if (FindType(root.XmlNamespace, metadataName, compilation, ref namespaces) is not { } type || type.Arity != arity)
        {
            return null;
        }

        if (arity == 0)
        {
            return type;
        }

        var arguments = new ITypeSymbol[arity];
        for (var i = 0; i < arity; i++)
        {
            var argument = root.TypeArguments[i];
            if (FindType(argument.XmlNamespace, argument.TypeName, compilation, ref namespaces) is not { } resolved)
            {
                return null;
            }

            arguments[i] = resolved;
        }

        return type.Construct(arguments);
    }

    /// <summary>Finds a type by the XML namespace and metadata name XAML gives it.</summary>
    /// <param name="xmlNamespace">The XML namespace the type is declared in.</param>
    /// <param name="metadataName">The type's name without its namespace, with its generic arity when it has one.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="namespaces">The CLR namespaces each XML namespace URI maps to, read on first use.</param>
    /// <returns>The type, or null when it does not resolve.</returns>
    private static INamedTypeSymbol? FindType(string xmlNamespace, string metadataName, Compilation compilation, ref Dictionary<string, List<string>>? namespaces)
    {
        if (ClrNamespace(xmlNamespace) is { } clrNamespace)
        {
            return compilation.GetTypeByMetadataName($"{clrNamespace}.{metadataName}");
        }

        namespaces ??= ReadXmlnsDefinitions(compilation);
        if (!namespaces.TryGetValue(xmlNamespace, out var candidates))
        {
            return null;
        }

        foreach (var candidate in candidates)
        {
            if (compilation.GetTypeByMetadataName($"{candidate}.{metadataName}") is { } type)
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
