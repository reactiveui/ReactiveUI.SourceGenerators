// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts notification capabilities from property-owner symbols.</summary>
internal static class TypeDetectionExtractor
{
    /// <summary>The name a view exposes its view model under.</summary>
    private const string ViewModelPropertyName = "ViewModel";

    /// <summary>Reads a type's notification mechanisms and observable properties from its symbol.</summary>
    /// <param name="typeSymbol">The type to inspect, declared in this compilation or referenced from another.</param>
    /// <param name="compilation">The compilation the type is resolved against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ClassBindingInfo POCO for the type.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ClassBindingInfo ExtractFromSymbol(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        CancellationToken ct) =>
        CreateTypeInfo(typeSymbol, compilation, ExtractProperties(typeSymbol, ct), ct);

    /// <summary>Captures the concrete owner's notification interfaces and one inherited or declared property.</summary>
    /// <param name="owner">The type through which the property is read.</param>
    /// <param name="property">The bound property symbol.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The owner's capabilities with property-specific native candidates.</returns>
    internal static ClassBindingInfo ExtractPropertyOwner(
        INamedTypeSymbol owner,
        IPropertySymbol property,
        Compilation compilation,
        CancellationToken ct)
    {
        var propertyInfo = ExtractProperty(owner, property);
        var properties = property.Name != ViewModelPropertyName && FindViewModelProperty(owner) is { } viewModel
            ? new EquatableArray<ObservablePropertyInfo>([propertyInfo, ExtractProperty(owner, viewModel)])
            : new EquatableArray<ObservablePropertyInfo>([propertyInfo]);
        return CreateTypeInfo(owner, compilation, properties, ct);
    }

    /// <summary>Captures the concrete owner of a field link: its notification interfaces and its view model property.</summary>
    /// <param name="owner">The type through which the field is read.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The owner's capabilities, listing its view model property when it exposes one.</returns>
    /// <remarks>
    /// A view binding usually starts at a field, a control named in XAML or by a designer, as in
    /// <c>v =&gt; v.NameBox.Text</c>. The view model property is recorded so the binding follows the view model the view
    /// currently holds, as it does when the path starts at a property.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ClassBindingInfo ExtractFieldOwner(
        INamedTypeSymbol owner,
        Compilation compilation,
        CancellationToken ct) =>
        CreateTypeInfo(
            owner,
            compilation,
            FindViewModelProperty(owner) is { } viewModel
                ? new EquatableArray<ObservablePropertyInfo>([ExtractProperty(owner, viewModel)])
                : new EquatableArray<ObservablePropertyInfo>([]),
            ct);

    /// <summary>Reads one property's native candidates while its owner symbols are available.</summary>
    /// <param name="owner">The concrete property owner.</param>
    /// <param name="property">The selected declaration.</param>
    /// <returns>Property-specific observation metadata.</returns>
    internal static ObservablePropertyInfo ExtractProperty(INamedTypeSymbol owner, IPropertySymbol property)
    {
        var companion = PlatformSymbols.FindMember(owner, $"{property.Name}Property");
        return new(
            property.Name,
            property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            property.GetMethod?.DeclaredAccessibility == Accessibility.Public,
            property.IsIndexer,
            companion is IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true },
            PlatformSymbols.FindEvent(owner, $"{property.Name}Changed") is not null,
            SymbolEqualityComparer.Default.Equals(owner, property.ContainingType),
            ObservationPluginRegistry.InspectProperty(owner, property),
            true);
    }

    /// <summary>Combines owner capabilities with the property metadata needed by the caller.</summary>
    /// <param name="typeSymbol">The concrete owner type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="properties">The inspected property metadata.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Value-equatable owner information.</returns>
    internal static ClassBindingInfo CreateTypeInfo(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        EquatableArray<ObservablePropertyInfo> properties,
        CancellationToken ct)
    {
        var wellKnown = SymbolHelpers.GetWellKnownSymbols(compilation);

        // Walk AllInterfaces (includes inherited interfaces)
        DetectImplementedInterfaces(
            typeSymbol,
            wellKnown,
            ct,
            out var implementsIReactiveObject,
            out var implementsINPC,
            out var implementsINPChanging);

        // Walk base type chain for platform detection
        var platform = DetectPlatformBaseTypes(typeSymbol, wellKnown, ct);

        return new(
            typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            typeSymbol.MetadataName,
            implementsIReactiveObject,
            implementsINPC,
            implementsINPChanging,
            platform.InheritsWpfDependencyObject,
            platform.InheritsWinUIDependencyObject,
            platform.InheritsNSObject,
            platform.InheritsWinFormsComponent,
            platform.InheritsAndroidView,
            properties);
    }

    /// <summary>Extracts the properties from a named type symbol.</summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of observable property info.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static EquatableArray<ObservablePropertyInfo> ExtractProperties(
        INamedTypeSymbol typeSymbol,
        CancellationToken ct)
    {
        const int TypicalObservablePropertyCount = 16;

        var properties = new List<ObservablePropertyInfo>(TypicalObservablePropertyCount);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var members = CollectMembersThroughOwnBases(typeSymbol, ct);

        for (var i = 0; i < members.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (members[i] is not IPropertySymbol property)
            {
                continue;
            }

            if (property.IsStatic || property.IsWriteOnly || !seen.Add(property.Name))
            {
                continue;
            }

            FindCompanionMembers(members, property.Name, out var isDependencyProperty, out var hasChangeEvent);

            properties.Add(new(
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                property.GetMethod!.DeclaredAccessibility == Accessibility.Public,
                property.IsIndexer,
                isDependencyProperty,
                hasChangeEvent,
                SymbolEqualityComparer.Default.Equals(property.ContainingType, typeSymbol),
                ObservationPluginRegistry.InspectProperty(typeSymbol, property),
                true));
        }

        // View model replacement must also follow a view model declared by a referenced view base.
        if (!seen.Contains(ViewModelPropertyName) && FindViewModelProperty(typeSymbol) is { } viewModel)
        {
            properties.Add(ExtractProperty(typeSymbol, viewModel));
        }

        return new([.. properties]);
    }

    /// <summary>Finds the public instance view model property a view exposes, through its base classes.</summary>
    /// <param name="owner">The type to search.</param>
    /// <returns>The view model property, or null when the type exposes none.</returns>
    private static IPropertySymbol? FindViewModelProperty(INamedTypeSymbol owner) =>
        PlatformSymbols.FindMember(owner, ViewModelPropertyName) is IPropertySymbol { IsStatic: false, GetMethod.DeclaredAccessibility: Accessibility.Public } viewModel
            ? viewModel
            : null;

    /// <summary>Finds the dependency-property field and change event a property notifies through.</summary>
    /// <param name="members">The members of the type and of the bases its own assembly declares.</param>
    /// <param name="propertyName">The property being described.</param>
    /// <param name="isDependencyProperty">Set when a companion <c>{PropertyName}Property</c> field is declared.</param>
    /// <param name="hasChangeEvent">Set when a companion <c>{PropertyName}Changed</c> event is declared.</param>
    /// <remarks>
    /// A mechanism the type carries does not settle how any one property notifies: the dependency property
    /// field and the change event are declared per property, so both are recorded per property.
    /// </remarks>
    private static void FindCompanionMembers(
        List<ISymbol> members,
        string propertyName,
        out bool isDependencyProperty,
        out bool hasChangeEvent)
    {
        isDependencyProperty = false;
        hasChangeEvent = false;

        var dependencyPropertyName = $"{propertyName}Property";
        var changeEventName = $"{propertyName}Changed";

        for (var i = 0; i < members.Count; i++)
        {
            var member = members[i];

            if (member is IFieldSymbol { IsStatic: true } && member.Name == dependencyPropertyName)
            {
                isDependencyProperty = true;
            }
            else if (member is IEventSymbol && member.Name == changeEventName)
            {
                hasChangeEvent = true;
            }
        }
    }

    /// <summary>Collects the members of a type and of the bases its own assembly declares.</summary>
    /// <param name="typeSymbol">The type to collect from.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The members, most-derived first.</returns>
    /// <remarks>
    /// A property a type inherits is still a property a call site can name, and whether it notifies is settled
    /// by the base that declares it. Reading only the type's own members leaves every inherited property
    /// unknown, which the mechanism predicates have to treat as reachable - so a plain property inherited from
    /// a base with no dependency-property field and no change event would still be observed as though it had
    /// one.
    /// <para>
    /// The walk stops at the assembly boundary. A base the consumer wrote is worth reading and cheap; a
    /// platform base is neither, and its properties genuinely are backed by the mechanism the type advertises,
    /// which is what the unknown answer already assumes.
    /// </para>
    /// </remarks>
    private static List<ISymbol> CollectMembersThroughOwnBases(INamedTypeSymbol typeSymbol, CancellationToken ct)
    {
        const int TypicalMemberCount = 32;

        var members = new List<ISymbol>(TypicalMemberCount);
        var assembly = typeSymbol.ContainingAssembly;

        for (var type = typeSymbol; type is not null; type = type.BaseType)
        {
            ct.ThrowIfCancellationRequested();

            if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, assembly))
            {
                break;
            }

            members.AddRange(type.GetMembers());
        }

        return members;
    }

    /// <summary>Walks <c>AllInterfaces</c> to detect notification interfaces.</summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="wellKnown">The cached well-known symbols.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="implementsIReactiveObject">Set to true if the type implements IReactiveObject.</param>
    /// <param name="implementsINPC">Set to true if the type implements INotifyPropertyChanged.</param>
    /// <param name="implementsINPChanging">Set to true if the type implements INotifyPropertyChanging.</param>
    private static void DetectImplementedInterfaces(
        INamedTypeSymbol typeSymbol,
        SymbolHelpers.WellKnownSymbolsBox wellKnown,
        CancellationToken ct,
        out bool implementsIReactiveObject,
        out bool implementsINPC,
        out bool implementsINPChanging)
    {
        implementsIReactiveObject = false;
        implementsINPC = false;
        implementsINPChanging = false;

        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var iface = allInterfaces[i];

            if (wellKnown.IReactiveObject is not null
                && SymbolEqualityComparer.Default.Equals(iface, wellKnown.IReactiveObject))
            {
                implementsIReactiveObject = true;
            }

            if (wellKnown.INPC is not null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.INPC))
            {
                implementsINPC = true;
            }

            if (wellKnown.INPChanging is not null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.INPChanging))
            {
                implementsINPChanging = true;
            }
        }
    }

    /// <summary>Walks the base type chain to detect platform-specific base types (WPF/WinUI/KVO/WinForms/Android).</summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="wellKnown">The cached well-known symbols.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detected platform base-type flags.</returns>
    private static PlatformBaseTypeFlags DetectPlatformBaseTypes(
        INamedTypeSymbol typeSymbol,
        SymbolHelpers.WellKnownSymbolsBox wellKnown,
        CancellationToken ct)
    {
        var wpf = false;
        var winui = false;
        var ns = false;
        var winforms = false;
        var android = false;

        var baseType = typeSymbol;
        while (baseType is not null)
        {
            ct.ThrowIfCancellationRequested();

            wpf = wpf || Matches(baseType, wellKnown.WpfDependencyObject);
            winui = winui || Matches(baseType, wellKnown.WinUIDependencyObject);
            ns = ns || Matches(baseType, wellKnown.NSObject);
            winforms = winforms || Matches(baseType, wellKnown.WinFormsComponent);
            android = android || Matches(baseType, wellKnown.AndroidView);

            baseType = baseType.BaseType;
        }

        // Uno declares DependencyObject as an interface on its non-Windows heads, so its controls implement it.
        winui = winui || ImplementsInterface(typeSymbol, wellKnown.WinUIDependencyObject);

        return new(wpf, winui, ns, winforms, android);
    }

    /// <summary>Determines whether a type implements a (possibly null) interface.</summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="candidate">The interface; null, or a type that is not an interface, is no match.</param>
    /// <returns><c>true</c> if the candidate is an interface the type implements; otherwise, <c>false</c>.</returns>
    private static bool ImplementsInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol? candidate)
    {
        if (candidate is not { TypeKind: TypeKind.Interface })
        {
            return false;
        }

        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(allInterfaces[i], candidate))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether <paramref name="symbol"/> equals the (possibly null) candidate symbol.</summary>
    /// <param name="symbol">The symbol to compare.</param>
    /// <param name="candidate">The candidate symbol; null is treated as no match.</param>
    /// <returns><c>true</c> if the candidate is non-null and equal; otherwise, <c>false</c>.</returns>
    private static bool Matches(INamedTypeSymbol symbol, INamedTypeSymbol? candidate) =>
        candidate is not null && SymbolEqualityComparer.Default.Equals(symbol, candidate);

    /// <summary>Platform-specific base-type detection flags for a class.</summary>
    /// <param name="InheritsWpfDependencyObject">Whether the type inherits a WPF DependencyObject.</param>
    /// <param name="InheritsWinUIDependencyObject">Whether the type inherits a WinUI DependencyObject.</param>
    /// <param name="InheritsNSObject">Whether the type inherits an Apple NSObject.</param>
    /// <param name="InheritsWinFormsComponent">Whether the type inherits a WinForms Component.</param>
    /// <param name="InheritsAndroidView">Whether the type inherits an Android View.</param>
    private readonly record struct PlatformBaseTypeFlags(
        bool InheritsWpfDependencyObject,
        bool InheritsWinUIDependencyObject,
        bool InheritsNSObject,
        bool InheritsWinFormsComponent,
        bool InheritsAndroidView);
}
