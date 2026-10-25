// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Covers the symbol checks the platform observation plugins share.</summary>
public class PlatformSymbolsTests
{
    /// <summary>The metadata name of the WinUI and Uno dependency object.</summary>
    private const string DependencyObjectName = "Microsoft.UI.Xaml.DependencyObject";

    /// <summary>The metadata name of the WinUI and Uno dependency property.</summary>
    private const string DependencyPropertyName = "Microsoft.UI.Xaml.DependencyProperty";

    /// <summary>
    /// A compilation with Uno's interface <c>DependencyObject</c>, a control that implements it with a dependency property
    /// exposed as a static property, a class-based control, and a plain type.
    /// </summary>
    private const string Source = """
                                  namespace Microsoft.UI.Xaml
                                  {
                                      public class DependencyProperty { }
                                      public interface DependencyObject { }
                                  }

                                  namespace Windows.UI.Xaml
                                  {
                                      public class DependencyObject { }
                                  }

                                  namespace TestApp
                                  {
                                      public class UnoControl : Microsoft.UI.Xaml.DependencyObject
                                      {
                                          public static Microsoft.UI.Xaml.DependencyProperty TitleProperty { get; } = new Microsoft.UI.Xaml.DependencyProperty();
                                          public string Title { get; set; } = "";
                                      }

                                      public class ClassControl : Windows.UI.Xaml.DependencyObject { }

                                      public class PlainViewModel : System.IDisposable
                                      {
                                          public string Title { get; set; } = "";
                                          public void Dispose() { }
                                      }
                                  }
                                  """;

    /// <summary>
    /// A type derives from or implements a framework type: a base class matches, an implemented interface matches, and a
    /// type with neither does not.
    /// </summary>
    /// <param name="typeName">The type to check.</param>
    /// <param name="frameworkType">The framework type name.</param>
    /// <param name="expected">Whether the type derives from or implements it.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("TestApp.ClassControl", "Windows.UI.Xaml.DependencyObject", true)]
    [Arguments("TestApp.UnoControl", DependencyObjectName, true)]
    [Arguments("TestApp.PlainViewModel", DependencyObjectName, false)]
    public async Task DerivesFromOrImplements_MatchesBaseClassesAndInterfaces(string typeName, string frameworkType, bool expected)
    {
        var compilation = TestHelper.CreateCompilation(Source);

        await Assert.That(PlatformSymbols.DerivesFromOrImplements(compilation.GetTypeByMetadataName(typeName)!, frameworkType))
            .IsEqualTo(expected);
    }

    /// <summary>A dependency property exposed as a public static property counts, as a static field does.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasDependencyProperty_WithAStaticProperty_ReturnsTrue()
    {
        var compilation = TestHelper.CreateCompilation(Source);

        await Assert.That(PlatformSymbols.HasDependencyProperty(compilation.GetTypeByMetadataName("TestApp.UnoControl")!, "Title", DependencyPropertyName))
            .IsTrue();
    }

    /// <summary>
    /// A type offers a mechanism when one of its properties is eligible for it, and a type none of whose properties is
    /// eligible does not.
    /// </summary>
    /// <param name="typeName">The type to check.</param>
    /// <param name="expected">Whether any property offers the WinUI dependency-property mechanism.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("TestApp.UnoControl", true)]
    [Arguments("TestApp.PlainViewModel", false)]
    public async Task HasCandidate_ReportsWhetherAnyPropertyIsEligible(string typeName, bool expected)
    {
        var compilation = TestHelper.CreateCompilation(Source);
        var info = TypeDetectionExtractor.ExtractFromSymbol(compilation.GetTypeByMetadataName(typeName)!, compilation, default);

        await Assert.That(PlatformSymbols.HasCandidate(info, WinUIObservation.Kind)).IsEqualTo(expected);
    }
}
