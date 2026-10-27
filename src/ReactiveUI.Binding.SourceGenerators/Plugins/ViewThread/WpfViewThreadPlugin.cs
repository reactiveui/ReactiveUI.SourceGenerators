// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a WPF object on the thread its dispatcher owns.</summary>
internal sealed class WpfViewThreadPlugin : IViewThreadPlugin
{
    /// <inheritdoc/>
    public string OwnerMetadataName => "System.Windows.Threading.DispatcherObject";

    /// <inheritdoc/>
    public string InvokerMetadataName => "ReactiveUI.Binding.Wpf.DispatcherViewThreadInvoker";

    /// <inheritdoc/>
    public string ReactiveInvokerMetadataName => "ReactiveUI.Binding.Reactive.Wpf.DispatcherViewThreadInvoker";

    /// <inheritdoc/>
    public string PackageName => "ReactiveUI.Binding.Wpf";
}
