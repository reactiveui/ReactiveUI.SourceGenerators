// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;

namespace ReactiveUI.Binding.Maui.Tests.XamlPage;

/// <summary>A page whose controls MAUI's XAML source generator declares, bound to <see cref="LoginViewModel"/>.</summary>
public partial class LoginPage : ContentPage, IViewFor<LoginViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="LoginPage"/> class.</summary>
    public LoginPage() => InitializeComponent();

    /// <inheritdoc/>
    public LoginViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (LoginViewModel?)value;
    }

    /// <summary>Binds the named controls to the view model.</summary>
    /// <returns>The bindings.</returns>
    public IDisposable[] BindControls() =>
    [
        this.Bind(ViewModel, static vm => vm.UserName, static v => v.UserName.Text),
        this.OneWayBind(ViewModel, static vm => vm.Status, static v => v.Status.Text),
        this.BindCommand(ViewModel, static vm => vm.Login, static v => v.LoginButton),
    ];
}
