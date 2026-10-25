// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Controls;

namespace ReactiveUI.Binding.Wpf.Tests.XamlPage;

/// <summary>
/// A WPF view whose controls are named in XAML. WPF's markup compiler declares their fields before the compiler runs, so
/// the binding generator sees them without reading the XAML.
/// </summary>
[System.Diagnostics.DebuggerDisplay("NamedControlsView: ViewModel = {ViewModel}")]
public partial class NamedControlsView : UserControl, IViewFor<NamedControlsViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="NamedControlsView"/> class.</summary>
    public NamedControlsView() => InitializeComponent();

    /// <inheritdoc/>
    public NamedControlsViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (NamedControlsViewModel?)value;
    }

    /// <summary>Binds the named controls to the view model.</summary>
    /// <returns>The bindings.</returns>
    public IDisposable[] BindControls() =>
    [
        this.Bind(ViewModel, static vm => vm.Name, static v => v.NameBox.Text),
        this.OneWayBind(ViewModel, static vm => vm.Name, static v => v.StatusText.Text),
    ];
}
