// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for Apple <c>NSObject</c> types (KVO support).
/// Affinity: 15 (matches ReactiveUI's KVOObservableForProperty — highest affinity).
/// Supports both after-change and before-change notifications via KVO options.
/// Generates inline <c>__KVOObservable</c> using <c>NSObject.AddObserver</c> /
/// <c>NSObject.RemoveObserver</c> with compile-time resolved KVO key paths.
/// </summary>
/// <remarks>
/// <para>
/// KVO key paths are resolved at compile time from the .NET property name using the
/// standard naming convention: lowercase first character (e.g., <c>Text</c> → <c>"text"</c>).
/// Boolean properties use the <c>Is</c> prefix (e.g., <c>Enabled</c> → <c>"isEnabled"</c>).
/// </para>
/// <para>
/// The generated <c>__KVOObserver</c> class inherits <c>NSObject</c> and overrides
/// <c>ObserveValue</c> to forward KVO notifications, matching ReactiveUI's
/// <c>BlockObserveValueDelegate</c> pattern. A <c>GCHandle</c> pins the observer
/// for the subscription lifetime.
/// </para>
/// </remarks>
internal sealed class KVOObservationPlugin : IPlatformObservationPlugin
{
    /// <summary>
    /// The affinity score for the Apple KVO observation plugin
    /// (matches ReactiveUI's KVOObservableForProperty — highest affinity).
    /// </summary>
    private static readonly int KVOAffinity = BindingAffinity.Kvo;

    /// <inheritdoc/>
    public int Affinity => KVOAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "KVO";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) =>
        IsAMatch(classInfo) && CanObserveProperty(classInfo, propertyName) ? Affinity : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsNSObject;

    /// <inheritdoc/>
    /// <remarks>
    /// Key-value observing reaches the properties the Apple frameworks declare, because those are the ones the
    /// Obj-C runtime backs. A property an application adds to its own subclass of one of them is an ordinary
    /// CLR property that no key path resolves, so it falls through to whatever mechanism the type also carries.
    /// The runtime engine draws the same line by asking which assembly declares the member.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        ObservedProperties.Find(classInfo, propertyName) is { SymbolsInspected: true }
            ? PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null
            : !ObservedProperties.IsDeclaredByConsumer(classInfo, propertyName);

    /// <inheritdoc/>
    public PlatformObservationInfo? InspectProperty(INamedTypeSymbol owner, IPropertySymbol property)
    {
        if (!PlatformSymbols.DerivesFrom(owner, "Foundation.NSObject"))
        {
            return null;
        }

        var selector = property.GetMethod is { } getter ? ExportedSelector(getter) : null;
        selector ??= ExportedSelector(property);
        if (selector is null && IsNativeDeclaration(owner, property))
        {
            selector = KvoObservationEmitter.ToKvoKeyPath(property.Name, property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        return selector is null ? null : new(ObservationKind, Affinity, default, null, null, selector);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(SourceWriter sb, in ObservationExpression observation) =>
        KvoObservationEmitter.Emit(sb, observation.Source, observation.Segment, observation.SourceType, observation.BeforeChange, observation.Distinct);

    /// <summary>Recognizes properties declared by the assembly supplying NSObject.</summary>
    /// <param name="owner">The concrete observed type.</param>
    /// <param name="property">The selected property declaration.</param>
    /// <returns>True when the declaration belongs to the native framework.</returns>
    internal static bool IsNativeDeclaration(INamedTypeSymbol owner, IPropertySymbol property)
    {
        for (var current = owner; current is not null; current = current.BaseType)
        {
            if (NativeTypeIdentity.Matches(current, "Foundation.NSObject"))
            {
                return SymbolEqualityComparer.Default.Equals(property.ContainingAssembly, current.ContainingAssembly);
            }
        }

        return false;
    }

    /// <summary>Reads a Foundation export selector without loading the platform assembly.</summary>
    /// <param name="symbol">The property or getter carrying export metadata.</param>
    /// <returns>The getter selector, or null.</returns>
    private static string? ExportedSelector(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (NativeTypeIdentity.Matches(attribute.AttributeClass, "Foundation.ExportAttribute")
                && !attribute.ConstructorArguments.IsEmpty
                && attribute.ConstructorArguments[0].Value is string selector
                && selector.Length > 0 && selector.IndexOf(':') < 0)
            {
                return selector;
            }
        }

        return null;
    }
}
