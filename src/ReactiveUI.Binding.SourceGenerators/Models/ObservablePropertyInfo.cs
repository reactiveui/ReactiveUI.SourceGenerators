// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Property-level value-equatable POCO for the incremental generator pipeline.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="PropertyName">The name of the property.</param>
/// <param name="PropertyTypeFullName">The fully qualified type of the property.</param>
/// <param name="HasPublicGetter">Whether the property has a public getter.</param>
/// <param name="IsIndexer">Whether the property is an indexer.</param>
/// <param name="IsDependencyProperty">Whether the property is a dependency property (WPF/WinUI).</param>
/// <param name="HasChangeEvent">
/// Whether the declaring type also declares a <c>{PropertyName}Changed</c> event, which is the convention the
/// component mechanism observes a property through.
/// </param>
/// <param name="IsDeclaredByType">
/// Whether the type this record was collected for declares the property itself, rather than inheriting it from
/// a base in the same assembly. Key-value observing turns on this question - the Obj-C runtime backs what its
/// own frameworks declare, not what an application adds to a subclass - so an inherited property has to stay
/// distinguishable from a declared one even though both are now recorded.
/// </param>
/// <param name="PlatformObservations">Eligible platform plugins and their verified notification members.</param>
/// <param name="SymbolsInspected">Whether eligibility was checked against compiler symbols.</param>
internal sealed record ObservablePropertyInfo(
    string PropertyName,
    string PropertyTypeFullName,
    bool HasPublicGetter,
    bool IsIndexer,
    bool IsDependencyProperty,
    bool HasChangeEvent,
    bool IsDeclaredByType = true,
    EquatableArray<PlatformObservationInfo> PlatformObservations = default,
    bool SymbolsInspected = false);
