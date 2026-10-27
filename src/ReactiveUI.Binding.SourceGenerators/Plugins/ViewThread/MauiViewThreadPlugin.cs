// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a MAUI object through the dispatcher it carries.</summary>
internal sealed class MauiViewThreadPlugin : IViewThreadPlugin
{
    /// <inheritdoc/>
    public string OwnerMetadataName => "Microsoft.Maui.Controls.BindableObject";

    /// <inheritdoc/>
    public string InvokerMetadataName => "ReactiveUI.Binding.Maui.DispatcherViewThreadInvoker";

    /// <inheritdoc/>
    public string ReactiveInvokerMetadataName => "ReactiveUI.Binding.Reactive.Maui.DispatcherViewThreadInvoker";

    /// <inheritdoc/>
    public string PackageName => "ReactiveUI.Binding.Maui";
}
