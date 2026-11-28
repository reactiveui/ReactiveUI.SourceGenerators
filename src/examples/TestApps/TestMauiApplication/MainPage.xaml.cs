// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MauiApp1;

/// <summary>The main page, whose <c>IViewFor</c> implementation is source generated.</summary>
/// <seealso cref="ContentPage" />
[ReactiveUI.SourceGenerators.IViewFor<MainViewModel>]
[System.Diagnostics.DebuggerDisplay("Counter page: {ViewModel}")]
public partial class MainPage : ContentPage
{
    /// <summary>Updates the button text while this page is visible.</summary>
    private IDisposable? _counterBinding;

    /// <summary>Connects the button to the increment command while the page is visible.</summary>
    private IDisposable? _commandBinding;

    /// <summary>Initializes a new instance of the <see cref="MainPage"/> class.</summary>
    public MainPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    /// <summary>Gets the button whose caption follows the view model.</summary>
    internal Button CounterButton => CounterBtn;

    /// <inheritdoc/>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _counterBinding ??= this.OneWayBind(ViewModel, static model => model.CounterText, static page => page.CounterButton.Text);
        _commandBinding ??= this.BindCommand(ViewModel, static model => model.IncrementCountCommand, static page => page.CounterButton);
    }

    /// <inheritdoc/>
    protected override void OnDisappearing()
    {
        _commandBinding?.Dispose();
        _commandBinding = null;
        _counterBinding?.Dispose();
        _counterBinding = null;
        base.OnDisappearing();
    }
}
