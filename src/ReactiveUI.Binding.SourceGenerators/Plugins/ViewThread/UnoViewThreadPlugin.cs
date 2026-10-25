// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Writes to an Uno Platform object through the sequencer of the dispatcher queue that owns it.</summary>
/// <remarks>
/// Uno and WinUI share <c>Microsoft.UI.Xaml.DependencyObject</c>. The invoker is named only when the compilation
/// references ReactiveUI.Binding.Uno, and a WinUI binding without it is not reported, since that package is for Uno.
/// </remarks>
internal sealed class UnoViewThreadPlugin : IViewThreadPlugin
{
    /// <inheritdoc/>
    public string OwnerMetadataName => "Microsoft.UI.Xaml.DependencyObject";

    /// <inheritdoc/>
    public string InvokerMetadataName => "ReactiveUI.Binding.Uno.UnoViewThreadInvoker";

    /// <inheritdoc/>
    public string ReactiveInvokerMetadataName => "ReactiveUI.Binding.Reactive.Uno.UnoViewThreadInvoker";

    /// <inheritdoc/>
    public string? PackageName => null;
}
