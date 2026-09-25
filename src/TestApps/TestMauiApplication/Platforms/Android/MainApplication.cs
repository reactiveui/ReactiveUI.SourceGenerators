// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Android.App;
using Android.Runtime;

namespace MauiApp1;

/// <summary>The Android application class that bootstraps the MAUI application.</summary>
/// <seealso cref="Microsoft.Maui.MauiApplication" />
[Application]
public class MainApplication : MauiApplication
{
    /// <summary>Initializes a new instance of the <see cref="MainApplication"/> class.</summary>
    /// <param name="handle">The handle.</param>
    /// <param name="ownership">The ownership.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <summary>
    /// When overridden in a derived class, creates the <see cref="T:Microsoft.Maui.Hosting.MauiApp" /> to be used in this application.
    /// Typically a <see cref="T:Microsoft.Maui.Hosting.MauiApp" /> is created by calling <see cref="M:Microsoft.Maui.Hosting.MauiApp.CreateBuilder(System.Boolean)" />, configuring
    /// the returned <see cref="T:Microsoft.Maui.Hosting.MauiAppBuilder" />, and returning the built app by calling <see cref="M:Microsoft.Maui.Hosting.MauiAppBuilder.Build" />.
    /// </summary>
    /// <returns>
    /// The built <see cref="T:Microsoft.Maui.Hosting.MauiApp" />.
    /// </returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
