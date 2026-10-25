// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

/// <summary>Selects the strongest way to raise a type's change notifications from generated code.</summary>
internal static class PropertyRaisePluginRegistry
{
    /// <summary>The mechanisms, strongest first, so the first that applies is the one that wins.</summary>
    /// <remarks>
    /// Declared in descending affinity so selection can stop at the first match. That order also puts the cheap
    /// tests first: the ReactiveUI test is one interface lookup, the raise-method test walks member names, and only
    /// the partial-type test reads syntax.
    /// </remarks>
    private static readonly IPropertyRaisePlugin[] Plugins =
    [
        new ReactiveObjectRaisePlugin(), // Affinity 10 - ReactiveUI's IReactiveObject
        new RaiseMethodRaisePlugin(), // Affinity 5 - a raise method generated code can call
        new PartialTypeRaisePlugin(), // Affinity 1 - a partial type the generator can add a member to
    ];

    /// <summary>The selection per type symbol, which lives as long as the compilation that owns the symbol.</summary>
    /// <remarks>
    /// A view model backs several properties, and every one of its call sites asks about the same type. A symbol
    /// belongs to one compilation, so the answer cannot go stale; the box lets a type that nothing can raise be
    /// remembered too.
    /// </remarks>
    private static readonly ConditionalWeakTable<INamedTypeSymbol, StrongBox<PropertyRaiseInfo?>> Selections = new();

    /// <summary>Selects how generated code raises a type's change notifications.</summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The strongest applicable raise description, or null when generated code cannot raise them.</returns>
    internal static PropertyRaiseInfo? Select(INamedTypeSymbol type, Compilation compilation)
    {
        if (Selections.TryGetValue(type, out var cached))
        {
            return cached.Value;
        }

        var selection = SelectUncached(type, compilation);
        _ = Selections.GetValue(type, _ => new(selection));
        return selection;
    }

    /// <summary>Asks each mechanism in turn, strongest first.</summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The first applicable raise description, or null.</returns>
    private static PropertyRaiseInfo? SelectUncached(INamedTypeSymbol type, Compilation compilation)
    {
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Plugins[i].Select(type, compilation) is { } raise)
            {
                return raise;
            }
        }

        return null;
    }
}
