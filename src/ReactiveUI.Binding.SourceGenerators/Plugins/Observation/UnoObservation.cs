// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes dependency properties exposed through Uno's Windows UI namespace.</summary>
internal static class UnoObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "UnoDP";

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } =
        DependencyPropertyObservationEmitter.CreatePlugin(Kind, "Windows.UI.Xaml");
}
