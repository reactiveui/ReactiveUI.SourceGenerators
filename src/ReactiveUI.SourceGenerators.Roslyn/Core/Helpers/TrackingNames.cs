// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>The names the generators' pipeline steps are tracked under, so tests can assert what the driver cached.</summary>
internal static class TrackingNames
{
    /// <summary>The models extracted from <c>[Reactive]</c> fields.</summary>
    internal const string ReactiveFields = nameof(ReactiveFields);

    /// <summary>The <c>[Reactive]</c> field models grouped per type.</summary>
    internal const string ReactiveFieldTypes = nameof(ReactiveFieldTypes);

    /// <summary>The models extracted from <c>[Reactive]</c> partial properties.</summary>
    internal const string ReactivePartialProperties = nameof(ReactivePartialProperties);

    /// <summary>The <c>[Reactive]</c> partial property models grouped per type.</summary>
    internal const string ReactivePartialPropertyTypes = nameof(ReactivePartialPropertyTypes);

    /// <summary>The models extracted from <c>[ReactiveCommand]</c> methods.</summary>
    internal const string ReactiveCommands = nameof(ReactiveCommands);

    /// <summary>The <c>[ReactiveCommand]</c> models grouped per type.</summary>
    internal const string ReactiveCommandTypes = nameof(ReactiveCommandTypes);

    /// <summary>The models extracted from <c>[BindableDerivedList]</c> fields.</summary>
    internal const string BindableDerivedLists = nameof(BindableDerivedLists);

    /// <summary>The <c>[BindableDerivedList]</c> models grouped per type.</summary>
    internal const string BindableDerivedListTypes = nameof(BindableDerivedListTypes);

    /// <summary>The models extracted from <c>[ReactiveCollection]</c> fields.</summary>
    internal const string ReactiveCollections = nameof(ReactiveCollections);

    /// <summary>The <c>[ReactiveCollection]</c> models grouped per type.</summary>
    internal const string ReactiveCollectionTypes = nameof(ReactiveCollectionTypes);

    /// <summary>The <c>[IReactiveObject]</c> models grouped per type.</summary>
    internal const string ReactiveObjectTypes = nameof(ReactiveObjectTypes);

    /// <summary>The <c>[RoutedControlHost]</c> models.</summary>
    internal const string RoutedControlHosts = nameof(RoutedControlHosts);

    /// <summary>The <c>[ViewModelControlHost]</c> models.</summary>
    internal const string ViewModelControlHosts = nameof(ViewModelControlHosts);
}
