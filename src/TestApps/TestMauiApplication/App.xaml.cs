// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MauiApp1;

/// <summary>The MAUI application entry point that hosts the application shell.</summary>
/// <seealso cref="Microsoft.Maui.Controls.Application" />
public partial class App : Application
{
    /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>Creates the application window hosting the <see cref="AppShell"/>.</summary>
    /// <param name="activationState">The activation state supplied by the platform.</param>
    /// <returns>The window that displays the application shell.</returns>
    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
