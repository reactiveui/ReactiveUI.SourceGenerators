// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests for <see cref="TypeDetectionExtractor"/> methods.</summary>
public class TypeDetectionExtractorTests
{
    /// <summary>The <c>MyViewModel</c> name these tests generate against.</summary>
    private const string MyViewModelName = "MyViewModel";

    /// <summary>The <c>Title</c> property these tests inspect.</summary>
    private const string TitlePropertyName = "Title";

    /// <summary>Verifies ExtractProperties extracts public properties from a type symbol.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_PublicProperties_ExtractsAll()
    {
        const int ExpectedPropertyCount = 2;
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                      public int Age { get; set; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyViewModelName);

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        await Assert.That(properties.Length).IsEqualTo(ExpectedPropertyCount);
    }

    /// <summary>Verifies ExtractProperties skips static properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_StaticProperty_Skipped()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public static string StaticProp { get; set; } = "";
                                      public string Name { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyViewModelName);

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        // Only Name should be extracted; StaticProp should be skipped
        await Assert.That(properties.Length).IsEqualTo(1);
        await Assert.That(properties[0].PropertyName).IsEqualTo("Name");
    }

    /// <summary>Verifies ExtractProperties skips write-only properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_WriteOnlyProperty_Skipped()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      private string _name = "";
                                      public string Name { set { _name = value; } }
                                      public string Title { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyViewModelName);

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        // Only Title should be extracted; Name (write-only) should be skipped
        await Assert.That(properties.Length).IsEqualTo(1);
        await Assert.That(properties[0].PropertyName).IsEqualTo(TitlePropertyName);
    }

    /// <summary>Verifies ExtractProperties includes indexer properties with IsIndexer flag.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_Indexer_Included()
    {
        const int ExpectedPropertyCount = 2;
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string this[int index] => "";
                                      public string Name { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyViewModelName);

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        // Both the indexer and Name are included; the indexer has IsIndexer = true
        var indexerProp = properties.FirstOrDefault(static p => p.IsIndexer);
        await Assert.That(properties.Length).IsEqualTo(ExpectedPropertyCount);
        await Assert.That(indexerProp).IsNotNull();
    }

    /// <summary>Verifies ExtractProperties detects dependency properties via companion static field heuristic.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_DependencyPropertyHeuristic_DetectsDP()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public static readonly object TitleProperty = new object();
                                      public string Title { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyViewModelName);

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        var titleProp = properties.First(static p => p.PropertyName == TitlePropertyName);
        await Assert.That(titleProp.IsDependencyProperty).IsTrue();
    }

    /// <summary>Verifies ExtractProperties records a companion change event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_ChangeEvent_Detected()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public class MyViewModel
                                  {
                                      public event EventHandler? TitleChanged;
                                      public string Title { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyViewModelName);

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        await Assert.That(properties.First(static p => p.PropertyName == TitlePropertyName).HasChangeEvent).IsTrue();
    }

    /// <summary>
    /// Verifies an Uno control, which implements <c>DependencyObject</c> as an interface on every head but Windows, is
    /// detected as a WinUI dependency object, and a type that does not implement it is not.
    /// </summary>
    /// <param name="typeName">The type to detect.</param>
    /// <param name="expected">Whether it is a WinUI dependency object.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("UnoControl", true)]
    [Arguments(MyViewModelName, false)]
    public async Task ExtractFromSymbol_UnoInterfaceDependencyObject_DetectsImplementers(string typeName, bool expected)
    {
        const string source = """
                              namespace Microsoft.UI.Xaml
                              {
                                  public interface DependencyObject { }
                              }

                              namespace TestApp
                              {
                                  public class UnoControl : Microsoft.UI.Xaml.DependencyObject
                                  {
                                      public string Title { get; set; } = "";
                                  }

                                  public class MyViewModel : System.IDisposable
                                  {
                                      public string Title { get; set; } = "";
                                      public void Dispose() { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, typeName);

        var info = TypeDetectionExtractor.ExtractFromSymbol(typeSymbol, compilation, default);

        await Assert.That(info.InheritsWinUIDependencyObject).IsEqualTo(expected);
    }

    /// <summary>Captures only the inherited view model needed to follow a view across assembly boundaries.</summary>
    /// <param name="baseMember">The view model declaration in the referenced base.</param>
    /// <param name="ownMember">The declaration that may hide the base member.</param>
    /// <param name="expectedType">The captured property type, or null when there is no readable property.</param>
    /// <param name="expectedPublic">Whether its getter is public.</param>
    /// <param name="expectedDeclared">Whether the consumer declares the property.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("public Model ViewModel { get; set; }", "", "global::External.Model", true, false)]
    [Arguments("public object ViewModel { get; set; }", "", "object", true, false)]
    [Arguments("public static Model ViewModel { get; set; }", "", null, false, false)]
    [Arguments("public Model ViewModel { private get; set; }", "", null, false, false)]
    [Arguments("public Model ViewModel { set { } }", "", null, false, false)]
    [Arguments("public Model ViewModel;", "", null, false, false)]
    [Arguments("", "", null, false, false)]
    [Arguments("public Model ViewModel { get; set; }", "public new object ViewModel { get; set; }", "object", true, true)]
    [Arguments("public Model ViewModel { get; set; }", "private new object ViewModel { get; set; }", "object", false, true)]
    [Arguments("public Model ViewModel { get; set; }", "public new static object ViewModel { get; set; }", null, false, false)]
    [Arguments("public Model ViewModel { get; set; }", "public new object ViewModel { set { } }", null, false, false)]
    [Arguments("public Model ViewModel { get; set; }", "public new object ViewModel;", null, false, false)]
    public async Task ExtractProperties_ReferencedViewModel_PreservesVisibleDeclaration(
        string baseMember,
        string ownMember,
        string? expectedType,
        bool expectedPublic,
        bool expectedDeclared)
    {
        var reference = TestHelper.CompileToReference(
            $$"""
              namespace External
              {
                  public class Model { }
                  public class ViewBase
                  {
                      {{baseMember}}
                      public string Unrelated { get; set; }
                  }
              }
              """,
            "ReferencedViews",
            null);
        var compilation = TestHelper.CreateCompilation(
            $$"""
              public class View : External.ViewBase
              {
                  {{ownMember}}
              }
              """,
            languageVersion: null,
            useReactiveRuntime: false,
            assemblyName: "Consumer",
            additionalReferences: [reference]);
        var typeSymbol = GetNamedTypeSymbol(compilation, "View");

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        await Assert.That(properties.Length).IsEqualTo(expectedType is null ? 0 : 1);
        if (expectedType is null)
        {
            return;
        }

        await Assert.That(properties[0].PropertyName).IsEqualTo("ViewModel");
        await Assert.That(properties[0].PropertyTypeFullName).IsEqualTo(expectedType);
        await Assert.That(properties[0].HasPublicGetter).IsEqualTo(expectedPublic);
        await Assert.That(properties[0].IsDeclaredByType).IsEqualTo(expectedDeclared);
    }

    /// <summary>Preserves the inherited view model's notification companion metadata.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperties_ReferencedViewModel_InspectsInheritedCompanions()
    {
        var reference = TestHelper.CompileToReference(
            """
            public class ViewBase : System.ComponentModel.Component
            {
                public object ViewModel { get; set; }
                public static readonly object ViewModelProperty = new object();
                public event System.EventHandler ViewModelChanged;
            }
            """,
            "ReferencedViewCompanions",
            null);
        var compilation = TestHelper.CreateCompilation(
            "public class View : ViewBase { }",
            languageVersion: null,
            useReactiveRuntime: false,
            assemblyName: "Consumer",
            additionalReferences: [reference]);
        var typeSymbol = GetNamedTypeSymbol(compilation, "View");

        var properties = TypeDetectionExtractor.ExtractProperties(typeSymbol, default);

        await Assert.That(properties.Length).IsEqualTo(1);
        await Assert.That(properties[0].IsDependencyProperty).IsTrue();
        await Assert.That(properties[0].HasChangeEvent).IsTrue();
        await Assert.That(properties[0].SymbolsInspected).IsTrue();
        await Assert.That(properties[0].PlatformObservations.Length).IsEqualTo(1);
    }

    /// <summary>Describes a write-only property without requiring a getter symbol.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractProperty_WriteOnlyMember_HasNoPublicGetter()
    {
        var compilation = TestHelper.CreateCompilation("public class Model { public string Value { set { } } }");
        var typeSymbol = GetNamedTypeSymbol(compilation, "Model");
        var property = (IPropertySymbol)typeSymbol.GetMembers("Value").Single();

        var metadata = TypeDetectionExtractor.ExtractProperty(typeSymbol, property);

        await Assert.That(metadata.HasPublicGetter).IsFalse();
        await Assert.That(metadata.SymbolsInspected).IsTrue();
    }

    /// <summary>Gets a named type symbol from a compilation.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="typeName">The type name.</param>
    /// <returns>The named type symbol.</returns>
    private static INamedTypeSymbol GetNamedTypeSymbol(Compilation compilation, string typeName)
    {
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == typeName);
        return (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDecl)!;
    }
}
