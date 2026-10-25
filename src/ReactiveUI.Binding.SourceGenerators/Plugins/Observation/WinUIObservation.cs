// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes WinUI dependency properties through native callback tokens.</summary>
internal static class WinUIObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "WinUIDP";

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } =
        DependencyPropertyObservationEmitter.CreatePlugin(Kind, "Microsoft.UI.Xaml");
}
