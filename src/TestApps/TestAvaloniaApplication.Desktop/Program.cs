// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using Avalonia;
using ReactiveUI.Avalonia;

namespace AvaloniaApplication1.Desktop;

/// <summary>The desktop entry point for the Avalonia sample application.</summary>
internal static class Program
{
    /// <summary>Starts the application with the classic desktop lifetime.</summary>
    /// <remarks>
    /// Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code
    /// before AppMain is called: things aren't initialized yet and stuff might break.
    /// </remarks>
    /// <param name="args">The command line arguments.</param>
    [STAThread]
    internal static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>Configures the Avalonia application builder with ReactiveUI support.</summary>
    /// <remarks>Don't remove; this is also used by the visual designer.</remarks>
    /// <returns>The configured <see cref="AppBuilder"/>.</returns>
    internal static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(static _ => { });
}
