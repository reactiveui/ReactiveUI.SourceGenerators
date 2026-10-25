// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to a WinForms control on the thread that created its handle.</summary>
internal sealed class WinFormsViewThreadPlugin : IViewThreadPlugin
{
    /// <inheritdoc/>
    public string OwnerMetadataName => "System.Windows.Forms.Control";

    /// <inheritdoc/>
    public string InvokerMetadataName => "ReactiveUI.Binding.WinForms.ControlViewThreadInvoker";

    /// <inheritdoc/>
    public string ReactiveInvokerMetadataName => "ReactiveUI.Binding.Reactive.WinForms.ControlViewThreadInvoker";

    /// <inheritdoc/>
    public string PackageName => "ReactiveUI.Binding.WinForms";
}
