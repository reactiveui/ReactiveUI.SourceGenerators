// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Covers matching a binding target's type to the runtime invoker a generated binding routes through.</summary>
public class ViewThreadPluginRegistryTests
{
    /// <summary>The WPF runtime invoker.</summary>
    private const string WpfInvoker = "global::ReactiveUI.Binding.Wpf.DispatcherViewThreadInvoker";

    /// <summary>The metadata name of a WPF view.</summary>
    private const string WpfView = "TestApp.WpfView";

    /// <summary>The metadata name of a type from no platform.</summary>
    private const string PlainViewModel = "TestApp.PlainViewModel";

    /// <summary>
    /// A compilation that declares one type from each platform, plus a type from none, with each platform's runtime
    /// invoker. The WinForms invoker is the System.Reactive flavour's, so either flavour's invoker counts.
    /// </summary>
    private const string PlatformSource = """
                                          namespace System.Windows.Threading
                                          {
                                              public class DispatcherObject { }
                                          }

                                          namespace System.Windows.Forms
                                          {
                                              public class Control { }
                                          }

                                          namespace Microsoft.Maui.Controls
                                          {
                                              public class BindableObject { }
                                          }

                                          namespace ReactiveUI.Binding.Wpf
                                          {
                                              public sealed class DispatcherViewThreadInvoker { }
                                          }

                                          namespace ReactiveUI.Binding.Reactive.WinForms
                                          {
                                              public sealed class ControlViewThreadInvoker { }
                                          }

                                          namespace ReactiveUI.Binding.Maui
                                          {
                                              public sealed class DispatcherViewThreadInvoker { }
                                          }

                                          namespace Avalonia
                                          {
                                              public class AvaloniaObject { }
                                          }

                                          namespace ReactiveUI.Binding.Avalonia
                                          {
                                              public sealed class AvaloniaViewThreadInvoker { }
                                          }

                                          namespace Microsoft.UI.Xaml
                                          {
                                              public class DependencyObject { }
                                          }

                                          namespace ReactiveUI.Binding.Uno
                                          {
                                              public sealed class UnoViewThreadInvoker { }
                                          }

                                          namespace TestApp
                                          {
                                              public class WpfView : System.Windows.Threading.DispatcherObject { }

                                              public class DerivedWpfView : WpfView { }

                                              public class WinFormsView : System.Windows.Forms.Control { }

                                              public class MauiView : Microsoft.Maui.Controls.BindableObject { }

                                              public class AvaloniaView : Avalonia.AvaloniaObject { }

                                              public class UnoView : Microsoft.UI.Xaml.DependencyObject { }

                                              public class PlainViewModel { }
                                          }
                                          """;

    /// <summary>A compilation that references WPF but not the runtime package carrying its invoker.</summary>
    private const string PlatformWithoutRuntimeSource = """
                                                        namespace System.Windows.Threading
                                                        {
                                                            public class DispatcherObject { }
                                                        }

                                                        namespace Avalonia
                                                        {
                                                            public class AvaloniaObject { }
                                                        }

                                                        namespace Microsoft.UI.Xaml
                                                        {
                                                            public class DependencyObject { }
                                                        }

                                                        namespace TestApp
                                                        {
                                                            public class WpfView : System.Windows.Threading.DispatcherObject { }

                                                            public class AvaloniaView : Avalonia.AvaloniaObject { }

                                                            public class WinUIView : Microsoft.UI.Xaml.DependencyObject { }
                                                        }
                                                        """;

    /// <summary>
    /// A compilation shaped like an Uno head other than Windows, where <c>DependencyObject</c> is an interface that
    /// each control implements.
    /// </summary>
    private const string UnoInterfaceSource = """
                                              namespace Microsoft.UI.Xaml
                                              {
                                                  public interface DependencyObject { }
                                              }

                                              namespace ReactiveUI.Binding.Uno
                                              {
                                                  public sealed class UnoViewThreadInvoker { }
                                              }

                                              namespace TestApp
                                              {
                                                  public interface IUnrelated { }

                                                  public class UnoControl : Microsoft.UI.Xaml.DependencyObject { }

                                                  public class DerivedUnoControl : UnoControl { }

                                                  public class UnrelatedViewModel : IUnrelated { }
                                              }
                                              """;

    /// <summary>A compilation that references no platform.</summary>
    private const string PlainSource = """
                                       namespace TestApp
                                       {
                                           public class PlainViewModel { }
                                       }
                                       """;

    /// <summary>
    /// A type from a platform, directly or through a base class, names that platform's runtime invoker in the flavour
    /// the compilation references.
    /// </summary>
    /// <param name="metadataName">The type to look up.</param>
    /// <param name="expected">The runtime invoker it should route through.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(WpfView, WpfInvoker)]
    [Arguments("TestApp.DerivedWpfView", WpfInvoker)]
    [Arguments("TestApp.WinFormsView", "global::ReactiveUI.Binding.Reactive.WinForms.ControlViewThreadInvoker")]
    [Arguments("TestApp.MauiView", "global::ReactiveUI.Binding.Maui.DispatcherViewThreadInvoker")]
    [Arguments("TestApp.AvaloniaView", "global::ReactiveUI.Binding.Avalonia.AvaloniaViewThreadInvoker")]
    [Arguments("TestApp.UnoView", "global::ReactiveUI.Binding.Uno.UnoViewThreadInvoker")]
    public async Task InvokerFor_WithATypeFromAPlatform_NamesItsInvoker(string metadataName, string expected)
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(metadataName), compilation))
            .IsEqualTo(expected);
    }

    /// <summary>A type from no platform carries no invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WithATypeFromNoPlatform_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(PlainViewModel), compilation))
            .IsNull();
    }

    /// <summary>With no type there is nothing to match.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WithNoType_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(null, compilation)).IsNull();
    }

    /// <summary>In a compilation that references no platform, no type carries an invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WhenTheCompilationReferencesNoPlatform_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlainSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(PlainViewModel), compilation))
            .IsNull();
    }

    /// <summary>A platform type whose runtime invoker is out of reach names the package that ships it.</summary>
    /// <param name="metadataName">The type to look up.</param>
    /// <param name="expected">The package that ships the type's invoker.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(WpfView, "ReactiveUI.Binding.Wpf")]
    [Arguments("TestApp.AvaloniaView", "ReactiveUI.Binding.Avalonia")]
    public async Task MissingPackageFor_WhenTheRuntimeInvokerIsMissing_NamesThePackage(string metadataName, string expected)
    {
        var compilation = TestHelper.CreateCompilation(PlatformWithoutRuntimeSource);

        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(compilation.GetTypeByMetadataName(metadataName), compilation))
            .IsEqualTo(expected);
    }

    /// <summary>Nothing is missing for a platform whose invoker resolves, for a type from no platform, or for no type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MissingPackageFor_WhenNothingIsMissing_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformSource);

        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(compilation.GetTypeByMetadataName(WpfView), compilation)).IsNull();
        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(compilation.GetTypeByMetadataName(PlainViewModel), compilation)).IsNull();
        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(null, compilation)).IsNull();
    }

    /// <summary>
    /// A WinUI object without ReactiveUI.Binding.Uno carries no invoker and names no missing package: that package is
    /// for Uno, and WinUI shares its dependency object type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIObject_WithoutTheUnoPackage_CarriesNoInvokerAndReportsNothing()
    {
        var compilation = TestHelper.CreateCompilation(PlatformWithoutRuntimeSource);
        var winUIView = compilation.GetTypeByMetadataName("TestApp.WinUIView");

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(winUIView, compilation)).IsNull();
        await Assert.That(ViewThreadPluginRegistry.MissingPackageFor(winUIView, compilation)).IsNull();
    }

    /// <summary>
    /// On an Uno head other than Windows, a control implements <c>DependencyObject</c> rather than deriving from it, and
    /// still names the Uno invoker; a type that implements some other interface names none.
    /// </summary>
    /// <param name="metadataName">The type to look up.</param>
    /// <param name="expected">The runtime invoker it should route through, or null for none.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("TestApp.UnoControl", "global::ReactiveUI.Binding.Uno.UnoViewThreadInvoker")]
    [Arguments("TestApp.DerivedUnoControl", "global::ReactiveUI.Binding.Uno.UnoViewThreadInvoker")]
    [Arguments("TestApp.UnrelatedViewModel", null)]
    public async Task InvokerFor_WithAnUnoInterfaceDependencyObject_NamesTheUnoInvoker(string metadataName, string? expected)
    {
        var compilation = TestHelper.CreateCompilation(UnoInterfaceSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(metadataName), compilation))
            .IsEqualTo(expected);
    }

    /// <summary>A platform type whose runtime invoker is out of reach carries no invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokerFor_WhenTheRuntimeInvokerIsMissing_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(PlatformWithoutRuntimeSource);

        await Assert.That(ViewThreadPluginRegistry.InvokerFor(compilation.GetTypeByMetadataName(WpfView), compilation))
            .IsNull();
    }
}
