// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Snapshot tests for platform-specific type detection (WPF, WinForms, Android, etc.).
/// Verifies that the generator correctly identifies base classes from various platforms
/// and includes them in the generated registration.
/// </summary>
public class PlatformDetectionSnapshotTests
{
    /// <summary>Verifies detection of WPF DependencyObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task WpfDependencyObject_Detected()
    {
        const string source = """
                              namespace System.Windows
                              {
                                  public class DependencyObject {}
                              }

                              namespace TestApp
                              {
                                  public class MyWpfControl : System.Windows.DependencyObject
                                  {
                                      public string Text { get; set; }
                                      public static readonly object TextProperty = new object();
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(PlatformDetectionSnapshotTests));
    }

    /// <summary>Verifies detection of WinForms Component.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task WinFormsComponent_Detected()
    {
        const string source = """
                              namespace System.ComponentModel
                              {
                                  public class Component {}
                              }

                              namespace TestApp
                              {
                                  public class MyWinFormsControl : System.ComponentModel.Component
                                  {
                                      public string Text { get; set; }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(PlatformDetectionSnapshotTests));
    }

    /// <summary>Verifies detection of Android View.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task AndroidView_Detected()
    {
        const string source = """
                              namespace Android.Views
                              {
                                  public class View {}
                              }

                              namespace TestApp
                              {
                                  public class MyAndroidView : Android.Views.View
                                  {
                                      public string Text { get; set; }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(PlatformDetectionSnapshotTests));
    }

    /// <summary>Verifies that an unused Apple type compiles on the minimum supported language version.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NSObject_Detected()
    {
        var source = ApplePlatformSource.TypeDetectionScenario();

        var result = await TestHelper.TestPassWithResult(source, typeof(PlatformDetectionSnapshotTests));
        await result.CompilationSucceeds();
    }
}
