// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

/// <summary>Raises a ReactiveUI object's notifications through ReactiveUI's own public raise extensions.</summary>
/// <remarks>
/// <para>
/// ReactiveUI routes a raise through per-object state: that is what honours
/// <c>SuppressChangeNotifications</c> and <c>DelayChangeNotifications</c> and feeds the <c>Changed</c> and
/// <c>Changing</c> observables. <c>IReactiveObject.RaisePropertyChanged(args)</c> would skip all of it, so the
/// generated code calls the public <c>IReactiveObjectExtensions.RaisePropertyChanged</c> extension instead, which is
/// the same path ReactiveUI's own <c>ToProperty</c> takes.
/// </para>
/// <para>
/// The two ReactiveUI flavours keep that state in different assemblies, so the extension class is taken from the
/// assembly that declares the object's ReactiveUI base type. Nothing here references ReactiveUI: every type is found
/// by metadata name.
/// </para>
/// </remarks>
internal sealed class ReactiveObjectRaisePlugin : IPropertyRaisePlugin
{
    /// <summary>The name this mechanism is recorded under.</summary>
    internal const string MechanismName = "ReactiveObject";

    /// <summary>The ReactiveUI extension raising <c>PropertyChanged</c>.</summary>
    private const string RaiseChangedName = "RaisePropertyChanged";

    /// <summary>The ReactiveUI extension raising <c>PropertyChanging</c>.</summary>
    private const string RaiseChangingName = "RaisePropertyChanging";

    /// <summary>The raise description per ReactiveUI extension class, which lives as long as its compilation.</summary>
    private static readonly ConditionalWeakTable<INamedTypeSymbol, PropertyRaiseInfo> RaiseByExtensions = new();

    /// <inheritdoc/>
    public int Affinity => BindingAffinity.ExactType;

    /// <inheritdoc/>
    public PropertyRaiseInfo? Select(INamedTypeSymbol type, Compilation compilation)
    {
        var reactiveObject = compilation.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName);
        if (reactiveObject is null || !Implements(type, reactiveObject))
        {
            return null;
        }

        // Every ReactiveUI type that reaches the same extension class raises the same way, so the description and
        // the class name inside it are built once per extension symbol rather than once per call site.
        var extensions = FindExtensions(type, compilation);
        return extensions is null
            || !HasAccessibleMethod(extensions, RaiseChangedName, compilation)
            || !HasAccessibleMethod(extensions, RaiseChangingName, compilation)
            ? null
            : RaiseByExtensions.GetValue(extensions, static e => Describe(e.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
    }

    /// <summary>Describes the two static raise calls on one extension class.</summary>
    /// <param name="owner">The fully qualified extension class.</param>
    /// <returns>The raise description.</returns>
    private static PropertyRaiseInfo Describe(string owner) =>
        new(
            MechanismName,
            new(PropertyRaiseCallKind.StaticMethod, RaiseChangedName, PropertyRaiseArgumentKind.PropertyName, owner),
            new PropertyRaiseCall(PropertyRaiseCallKind.StaticMethod, RaiseChangingName, PropertyRaiseArgumentKind.PropertyName, owner),
            null);

    /// <summary>Determines whether a type implements an interface.</summary>
    /// <param name="type">The type to judge.</param>
    /// <param name="interfaceType">The interface.</param>
    /// <returns><see langword="true"/> when the type is or implements the interface.</returns>
    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol interfaceType)
    {
        if (SymbolEqualityComparer.Default.Equals(type, interfaceType))
        {
            return true;
        }

        var interfaces = type.AllInterfaces;
        for (var i = 0; i < interfaces.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(interfaces[i], interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds the ReactiveUI extension class that owns the object's notification state.</summary>
    /// <param name="type">The ReactiveUI object type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The extension class, or null when no ReactiveUI assembly declaring it is referenced.</returns>
    private static INamedTypeSymbol? FindExtensions(INamedTypeSymbol type, Compilation compilation)
    {
        // A ReactiveObject subclass reaches the assembly that declares ReactiveObject, which is the flavour whose
        // state the object uses. A hand-written IReactiveObject reaches no ReactiveUI base, and takes either.
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            var assembly = current.ContainingAssembly;
            if (assembly is null)
            {
                continue;
            }

            var declared = assembly.GetTypeByMetadataName(Constants.IReactiveObjectExtensionsMetadataName)
                ?? assembly.GetTypeByMetadataName(Constants.ReactiveIReactiveObjectExtensionsMetadataName);
            if (declared is not null)
            {
                return declared;
            }
        }

        return compilation.GetTypeByMetadataName(Constants.IReactiveObjectExtensionsMetadataName)
            ?? compilation.GetTypeByMetadataName(Constants.ReactiveIReactiveObjectExtensionsMetadataName);
    }

    /// <summary>Determines whether a static class declares a method the consumer can call.</summary>
    /// <param name="type">The class.</param>
    /// <param name="name">The method name.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when an accessible static method of that name exists.</returns>
    private static bool HasAccessibleMethod(INamedTypeSymbol type, string name, Compilation compilation)
    {
        var members = type.GetMembers(name);
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i] is IMethodSymbol { IsStatic: true } method
                && compilation.IsSymbolAccessibleWithin(method, compilation.Assembly))
            {
                return true;
            }
        }

        return false;
    }
}
