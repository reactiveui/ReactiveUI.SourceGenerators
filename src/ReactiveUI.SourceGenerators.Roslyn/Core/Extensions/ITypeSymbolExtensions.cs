// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="ITypeSymbol"/> type.</summary>
internal static class ITypeSymbolExtensions
{
    /// <summary>The root namespace of the framework types the classifiers look for.</summary>
    private const string SystemNamespace = "System";

    /// <summary>The name of <c>System.IObservable&lt;T&gt;</c>.</summary>
    private const string ObservableTypeName = "IObservable";

    /// <summary>Provides metadata-name and hierarchy operations for a type symbol.</summary>
    /// <param name="typeSymbol">The type symbol receiving the extension operation.</param>
    extension(ITypeSymbol typeSymbol)
    {
    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol is or inherits from <paramref name="name"/>.</returns>
    internal bool HasOrInheritsFromFullyQualifiedMetadataName(string name)
    {
        for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol inherits from <paramref name="name"/>.</returns>
    internal bool InheritsFromFullyQualifiedMetadataName(string name)
    {
        var baseType = typeSymbol.BaseType;

        while (baseType is not null)
        {
            if (baseType.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> implements a specified interface.</summary>
    /// <param name="name">The full name of the interface to check for inheritance.</param>
    /// <returns>Whether the type symbol implements <paramref name="name"/>.</returns>
    internal bool ImplementsFullyQualifiedMetadataName(string name)
    {
        foreach (var implementedInterface in typeSymbol.AllInterfaces)
        {
            if (implementedInterface.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol is or inherits from <paramref name="name"/>.</returns>
    internal bool HasOrInheritsFromFullyQualifiedMetadataNameStartingWith(string name)
    {
        for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.ContainsFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol inherits from <paramref name="name"/>.</returns>
    internal bool InheritsFromFullyQualifiedMetadataNameStartingWith(string name)
    {
        var baseType = typeSymbol.BaseType;

        while (baseType is not null)
        {
            if (baseType.ContainsFullyQualifiedMetadataName(name))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits a specified attribute.</summary>
    /// <param name="name">The name of the attribute to look for.</param>
    /// <returns>Whether the type symbol has an attribute with the specified type name.</returns>
    internal bool HasOrInheritsAttributeWithFullyQualifiedMetadataName(string name)
    {
        for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.HasAttributeWithFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks whether or not a given type symbol has a specified fully qualified metadata name.</summary>
    /// <param name="name">The full name to check.</param>
    /// <returns>Whether the type symbol has a full name equal to <paramref name="name"/>.</returns>
    /// <remarks>
    /// The name is compared segment by segment against the symbol chain rather than built first: this runs for every
    /// base type and interface a validity check walks, so building the name would allocate for each of them.
    /// </remarks>
    internal bool HasFullyQualifiedMetadataName(string name)
    {
        var position = 0;
        return MatchMetadataName(typeSymbol, name, ref position) && position == name.Length;
    }

    /// <summary>Checks whether a type symbol's metadata name contains a given value.</summary>
    /// <param name="name">The value to find in the full metadata name.</param>
    /// <returns>Whether the metadata name contains <paramref name="name"/>.</returns>
    internal bool ContainsFullyQualifiedMetadataName(string name)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        AppendFullyQualifiedMetadataName(typeSymbol, builder);

        return builder.WrittenSpan.IndexOf(name.AsSpan()) >= 0;
    }

    /// <summary>Gets the fully qualified metadata name for a given <see cref="ITypeSymbol"/> instance.</summary>
    /// <returns>The fully qualified metadata name for the type symbol.</returns>
    internal string GetFullyQualifiedMetadataName()
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        AppendFullyQualifiedMetadataName(typeSymbol, builder);

        return builder.ToString();
    }

    }

    /// <summary>Provides null-tolerant type-classification operations.</summary>
    /// <param name="typeSymbol">The type symbol receiving the extension operation.</param>
    extension(ITypeSymbol? typeSymbol)
    {
    /// <summary>Determines whether a type symbol represents a task return type.</summary>
    /// <returns>Whether the type symbol or a base type is <c>System.Threading.Tasks.Task</c>.</returns>
    /// <remarks>Compares names structurally rather than through display strings, so no string is built per base type.</remarks>
    internal bool IsTaskReturnType()
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (current is INamedTypeSymbol { Name: "Task", Arity: 0, ContainingType: null } task
                && IsNamespace(task.ContainingNamespace, "Tasks", "Threading", SystemNamespace))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type symbol represents an observable return type.</summary>
    /// <returns>Whether the type symbol or a base type mentions <c>System.IObservable</c> anywhere in its name.</returns>
    /// <remarks>
    /// Matches what the fully qualified display string containing <c>global::System.IObservable</c> matched - the type,
    /// its type arguments, array elements and containing types - without building the string.
    /// </remarks>
    internal bool IsObservableReturnType()
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (MentionsType(
                    current,
                    static named => named.Name.StartsWith(ObservableTypeName, StringComparison.Ordinal) && IsNamespace(named.ContainingNamespace, SystemNamespace)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type symbol represents the configured scheduler API.</summary>
    /// <param name="api">The ReactiveUI API family to inspect.</param>
    /// <returns>Whether the type symbol or a base type represents that scheduler API.</returns>
    internal bool IsSchedulerType(ReactiveUiApi api)
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (current is not INamedTypeSymbol { Arity: 0, ContainingType: null } named)
            {
                continue;
            }

            var matches = api == ReactiveUiApi.Primitives
                ? named.Name == "ISequencer" && IsNamespace(named.ContainingNamespace, "Concurrency", "Primitives", "ReactiveUI")
                : named.Name == "IScheduler" && IsNamespace(named.ContainingNamespace, "Concurrency", "Reactive", SystemNamespace);
            if (matches)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type symbol represents an observable Boolean value.</summary>
    /// <returns>Whether the type symbol or a base type mentions <c>System.IObservable&lt;bool&gt;</c> anywhere in its name.</returns>
    internal bool IsObservableBoolType()
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (MentionsType(
                    current,
                    static named => named is { Name: ObservableTypeName, TypeArguments: [{ SpecialType: SpecialType.System_Boolean }] }
                        && IsNamespace(named.ContainingNamespace, SystemNamespace)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether [is nullable type].</summary>
    /// <returns>
    ///   <c>true</c> if [is nullable type] [the specified type symbol]; otherwise, <c>false</c>.
    /// </returns>
    internal bool IsNullableType() => typeSymbol?.NullableAnnotation == NullableAnnotation.Annotated;

    /// <summary>Gets the type produced by a task-like generic return type.</summary>
    /// <param name="compilation">The current compilation.</param>
    /// <returns>The task result type, or <see cref="SpecialType.System_Void"/> when no result exists.</returns>
    internal ITypeSymbol GetTaskReturnType(Compilation compilation) => typeSymbol switch
    {
        INamedTypeSymbol { TypeArguments.Length: 1 } namedTypeSymbol => namedTypeSymbol.TypeArguments[0],
        _ => compilation.GetSpecialType(SpecialType.System_Void)
    };

    }

    /// <summary>Matches the start of a name against the metadata name a symbol would build, without building it.</summary>
    /// <param name="current">The symbol, walked outermost namespace first.</param>
    /// <param name="name">The name to compare with.</param>
    /// <param name="position">The number of characters of <paramref name="name"/> matched so far.</param>
    /// <returns><see langword="false"/> at the first character that differs; otherwise <see langword="true"/>.</returns>
    /// <remarks>Mirrors <see cref="AppendFullyQualifiedMetadataName"/>, character for character.</remarks>
    private static bool MatchMetadataName(ISymbol? current, string name, ref int position) => current switch
    {
        INamespaceSymbol namespaceSymbol => MatchNamespaceName(namespaceSymbol, name, ref position),
        ITypeSymbol typeSymbol => MatchTypeName(typeSymbol, name, ref position),
        _ => true,
    };

    /// <summary>Matches the start of a name against a namespace's dotted name.</summary>
    /// <param name="namespaceSymbol">The namespace.</param>
    /// <param name="name">The name to compare with.</param>
    /// <param name="position">The number of characters of <paramref name="name"/> matched so far.</param>
    /// <returns><see langword="false"/> at the first character that differs; otherwise <see langword="true"/>.</returns>
    private static bool MatchNamespaceName(INamespaceSymbol namespaceSymbol, string name, ref int position) =>
        namespaceSymbol.IsGlobalNamespace
        || ((namespaceSymbol.ContainingNamespace.IsGlobalNamespace
                || (MatchNamespaceName(namespaceSymbol.ContainingNamespace, name, ref position) && MatchCharacter('.', name, ref position)))
            && MatchText(namespaceSymbol.MetadataName, name, ref position));

    /// <summary>Matches the start of a name against a type's metadata name, containing types joined by <c>+</c>.</summary>
    /// <param name="currentType">The type.</param>
    /// <param name="name">The name to compare with.</param>
    /// <param name="position">The number of characters of <paramref name="name"/> matched so far.</param>
    /// <returns><see langword="false"/> at the first character that differs; otherwise <see langword="true"/>.</returns>
    private static bool MatchTypeName(ITypeSymbol currentType, string name, ref int position)
    {
        var containerMatches = currentType.ContainingSymbol switch
        {
            ITypeSymbol containingType => MatchMetadataName(containingType, name, ref position) && MatchCharacter('+', name, ref position),
            INamespaceSymbol { IsGlobalNamespace: false } containingNamespace => MatchMetadataName(containingNamespace, name, ref position) && MatchCharacter('.', name, ref position),
            _ => true,
        };

        return containerMatches && MatchText(currentType.MetadataName, name, ref position);
    }

    /// <summary>Matches one segment of a metadata name; once the name is exhausted, everything further matches.</summary>
    /// <param name="text">The segment.</param>
    /// <param name="name">The name to compare with.</param>
    /// <param name="position">The number of characters of <paramref name="name"/> matched so far.</param>
    /// <returns>Whether the segment matches.</returns>
    private static bool MatchText(string text, string name, ref int position)
    {
        var count = Math.Min(text.Length, name.Length - position);
        if (count > 0 && string.CompareOrdinal(text, 0, name, position, count) != 0)
        {
            return false;
        }

        position += count;
        return true;
    }

    /// <summary>Matches one separator character of a metadata name; once the name is exhausted, everything further matches.</summary>
    /// <param name="character">The separator.</param>
    /// <param name="name">The name to compare with.</param>
    /// <param name="position">The number of characters of <paramref name="name"/> matched so far.</param>
    /// <returns>Whether the separator matches.</returns>
    private static bool MatchCharacter(char character, string name, ref int position)
    {
        if (position >= name.Length)
        {
            return true;
        }

        if (name[position] != character)
        {
            return false;
        }

        position++;
        return true;
    }

    /// <summary>Appends the fully qualified metadata name for a given symbol to a target builder.</summary>
    /// <param name="symbol">The input <see cref="ITypeSymbol"/> instance.</param>
    /// <param name="builder">The target <see cref="ImmutableArrayBuilder{T}"/> instance.</param>
    private static void AppendFullyQualifiedMetadataName(ITypeSymbol symbol, ImmutableArrayBuilder<char> builder)
    {
        static void BuildFrom(ISymbol? current, ImmutableArrayBuilder<char> target)
        {
            if (current is INamespaceSymbol namespaceSymbol)
            {
                if (!namespaceSymbol.IsGlobalNamespace)
                {
                    if (!namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
                    {
                        BuildFrom(namespaceSymbol.ContainingNamespace, target);
                        target.Add('.');
                    }

                    target.AddRange(namespaceSymbol.MetadataName.AsSpan());
                }

                return;
            }

            if (current is not ITypeSymbol currentType)
            {
                return;
            }

            if (currentType.ContainingSymbol is ITypeSymbol containingType)
            {
                BuildFrom(containingType, target);
                target.Add('+');
            }
            else if (currentType.ContainingSymbol is INamespaceSymbol containingNamespace && !containingNamespace.IsGlobalNamespace)
            {
                BuildFrom(containingNamespace, target);
                target.Add('.');
            }

            target.AddRange(currentType.MetadataName.AsSpan());
        }

        BuildFrom(symbol, builder);
    }

    /// <summary>Determines whether a type, or any type named in its display form, satisfies a check.</summary>
    /// <param name="type">The type.</param>
    /// <param name="check">The check applied to each named type.</param>
    /// <returns>Whether any named type in the type's display form satisfies the check.</returns>
    /// <remarks>
    /// Walks what a fully qualified display string would spell out: the type itself, its containing types, its type
    /// arguments (tuple elements included) and array and pointer element types.
    /// </remarks>
    private static bool MentionsType(ITypeSymbol type, Func<INamedTypeSymbol, bool> check) => type switch
    {
        INamedTypeSymbol named => check(named)
            || (named.ContainingType is { } containing && MentionsType(containing, check))
            || AnyTypeArgument(named, check),
        IArrayTypeSymbol array => MentionsType(array.ElementType, check),
        IPointerTypeSymbol pointer => MentionsType(pointer.PointedAtType, check),
        _ => false,
    };

    /// <summary>Determines whether any type argument of a named type satisfies a check.</summary>
    /// <param name="named">The named type.</param>
    /// <param name="check">The check applied to each named type.</param>
    /// <returns>Whether any type argument, at any depth, satisfies the check.</returns>
    private static bool AnyTypeArgument(INamedTypeSymbol named, Func<INamedTypeSymbol, bool> check)
    {
        foreach (var argument in named.TypeArguments)
        {
            if (MentionsType(argument, check))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a namespace is exactly a top-level namespace.</summary>
    /// <param name="namespaceSymbol">The namespace.</param>
    /// <param name="name">The namespace's name.</param>
    /// <returns>Whether the namespace matches.</returns>
    private static bool IsNamespace(INamespaceSymbol? namespaceSymbol, string name) =>
        namespaceSymbol is { IsGlobalNamespace: false, ContainingNamespace.IsGlobalNamespace: true } && namespaceSymbol.Name == name;

    /// <summary>Determines whether a namespace is exactly a two-part dotted name.</summary>
    /// <param name="namespaceSymbol">The namespace.</param>
    /// <param name="inner">The innermost segment.</param>
    /// <param name="outer">The outermost segment.</param>
    /// <returns>Whether the namespace matches.</returns>
    private static bool IsNamespace(INamespaceSymbol? namespaceSymbol, string inner, string outer) =>
        namespaceSymbol is { IsGlobalNamespace: false } && namespaceSymbol.Name == inner && IsNamespace(namespaceSymbol.ContainingNamespace, outer);

    /// <summary>Determines whether a namespace is exactly a three-part dotted name.</summary>
    /// <param name="namespaceSymbol">The namespace.</param>
    /// <param name="inner">The innermost segment.</param>
    /// <param name="middle">The middle segment.</param>
    /// <param name="outer">The outermost segment.</param>
    /// <returns>Whether the namespace matches.</returns>
    private static bool IsNamespace(INamespaceSymbol? namespaceSymbol, string inner, string middle, string outer) =>
        namespaceSymbol is { IsGlobalNamespace: false } && namespaceSymbol.Name == inner && IsNamespace(namespaceSymbol.ContainingNamespace, middle, outer);
}
