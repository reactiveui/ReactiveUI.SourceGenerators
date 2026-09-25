// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;

namespace AvaloniaApplication1;

/// <summary>The Avalonia application that creates the main window or view.</summary>
/// <seealso cref="Avalonia.Application" />
public class App : Application
{
    /// <summary>Initializes the application by loading XAML etc.</summary>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>Creates the main window or view once the framework has initialized.</summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = new MainViewModel() };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
