// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to an Avalonia object through the sequencer of the dispatcher that owns it.</summary>
internal sealed class AvaloniaViewThreadPlugin : IViewThreadPlugin
{
    /// <inheritdoc/>
    public string OwnerMetadataName => "Avalonia.AvaloniaObject";

    /// <inheritdoc/>
    public string InvokerMetadataName => "ReactiveUI.Binding.Avalonia.AvaloniaViewThreadInvoker";

    /// <inheritdoc/>
    public string ReactiveInvokerMetadataName => "ReactiveUI.Binding.Reactive.Avalonia.AvaloniaViewThreadInvoker";

    /// <inheritdoc/>
    public string PackageName => "ReactiveUI.Binding.Avalonia";
}
