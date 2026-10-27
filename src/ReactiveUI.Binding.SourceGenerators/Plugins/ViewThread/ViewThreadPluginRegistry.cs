// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Matches a binding target's type to the runtime invoker a generated binding routes its writes through.</summary>
internal static class ViewThreadPluginRegistry
{
    /// <summary>The plugins, one per UI platform.</summary>
    private static readonly IViewThreadPlugin[] Plugins =
    [
        new WpfViewThreadPlugin(),
        new WinFormsViewThreadPlugin(),
        new MauiViewThreadPlugin(),
        new AvaloniaViewThreadPlugin(),
        new UnoViewThreadPlugin(),
    ];

    /// <summary>The platform types each compilation resolves, in plugin order.</summary>
    private static readonly ConditionalWeakTable<Compilation, OwnerTypes> OwnerCache = new();

    /// <summary>Finds the invoker a generated binding routes its writes through for a target type.</summary>
    /// <param name="type">The target's type, or null when the model could not name it.</param>
    /// <param name="compilation">The compilation the type belongs to.</param>
    /// <returns>
    /// The fully qualified runtime invoker, or null when the type belongs to no supported platform or the compilation
    /// does not reference that platform's runtime package.
    /// </returns>
    internal static string? InvokerFor(ITypeSymbol? type, Compilation compilation) =>
        FindPlatform(type, compilation) is { } index ? GetOwners(compilation).Invokers[index] : null;

    /// <summary>Finds the platform package a target type needs but the compilation does not reference.</summary>
    /// <param name="type">The target's type, or null when the model could not name it.</param>
    /// <param name="compilation">The compilation the type belongs to.</param>
    /// <returns>
    /// The package that ships the platform's invoker, or null when the type belongs to no supported platform or the
    /// compilation already references that platform's invoker.
    /// </returns>
    internal static string? MissingPackageFor(ITypeSymbol? type, Compilation compilation) =>
        FindPlatform(type, compilation) is { } index && GetOwners(compilation).Invokers[index] is null
            ? Plugins[index].PackageName
            : null;

    /// <summary>Finds which platform a type belongs to, directly, through a base class or through an interface.</summary>
    /// <param name="type">The type, or null.</param>
    /// <param name="compilation">The compilation the type belongs to.</param>
    /// <returns>The plugin index, or null when the type belongs to no supported platform.</returns>
    /// <remarks>Uno declares <c>DependencyObject</c> as an interface on its non-Windows heads.</remarks>
    private static int? FindPlatform(ITypeSymbol? type, Compilation compilation)
    {
        var owners = GetOwners(compilation);
        if (type is null || !owners.AnyResolved)
        {
            return null;
        }

        for (var current = type; current is not null; current = current.BaseType)
        {
            if (IndexOf(current, owners) is { } index)
            {
                return index;
            }
        }

        var interfaces = type.AllInterfaces;
        for (var i = 0; i < interfaces.Length; i++)
        {
            if (IndexOf(interfaces[i], owners) is { } index)
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>Finds the plugin whose platform type is the given type.</summary>
    /// <param name="type">The type to look up.</param>
    /// <param name="owners">The platform types the compilation resolves.</param>
    /// <returns>The plugin index, or null when the type is no platform's type.</returns>
    private static int? IndexOf(ITypeSymbol type, OwnerTypes owners)
    {
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(type, owners.Types[i]))
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>Resolves the platform types a compilation references, once per compilation.</summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>The resolved types, in plugin order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OwnerTypes GetOwners(Compilation compilation) =>
        OwnerCache.GetValue(compilation, static c => new(c));

    /// <summary>The platform types one compilation resolves, and their invokers, in plugin order.</summary>
    private sealed class OwnerTypes
    {
        /// <summary>Initializes a new instance of the <see cref="OwnerTypes"/> class.</summary>
        /// <param name="compilation">The compilation to resolve the types in.</param>
        /// <remarks>
        /// The invoker is named by the flavour that resolved, because the platform packages are separate assemblies
        /// that retargeting cannot see into. A platform whose owner resolves without an invoker is still recorded, so
        /// a binding onto it can be reported rather than silently left unmarshalled.
        /// </remarks>
        public OwnerTypes(Compilation compilation)
        {
            Types = new INamedTypeSymbol?[Plugins.Length];
            Invokers = new string?[Plugins.Length];
            for (var i = 0; i < Plugins.Length; i++)
            {
                var plugin = Plugins[i];
                Types[i] = compilation.GetTypeByMetadataName(plugin.OwnerMetadataName);
                Invokers[i] = Types[i] is not null && ResolveInvoker(compilation, plugin) is { } invoker ? $"global::{invoker}" : null;
                AnyResolved |= Types[i] is not null;
            }
        }

        /// <summary>Gets the resolved types; an entry is null when the compilation does not reference that platform.</summary>
        public INamedTypeSymbol?[] Types { get; }

        /// <summary>Gets the fully qualified invoker each platform routes through, or null where none resolves.</summary>
        public string?[] Invokers { get; }

        /// <summary>Gets a value indicating whether the compilation references any supported platform.</summary>
        public bool AnyResolved { get; }

        /// <summary>Finds which flavour of a platform's invoker the compilation references.</summary>
        /// <param name="compilation">The compilation to inspect.</param>
        /// <param name="plugin">The platform.</param>
        /// <returns>The metadata name of the invoker that resolves, or null when neither does.</returns>
        private static string? ResolveInvoker(Compilation compilation, IViewThreadPlugin plugin)
        {
            if (compilation.GetTypeByMetadataName(plugin.InvokerMetadataName) is not null)
            {
                return plugin.InvokerMetadataName;
            }

            return compilation.GetTypeByMetadataName(plugin.ReactiveInvokerMetadataName) is not null
                ? plugin.ReactiveInvokerMetadataName
                : null;
        }
    }
}
