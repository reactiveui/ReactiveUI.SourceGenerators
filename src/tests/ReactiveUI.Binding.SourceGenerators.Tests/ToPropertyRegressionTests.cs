// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Regressions reported against ReactiveUI.SourceGenerators and ReactiveUI, checked against this generator.</summary>
public class ToPropertyRegressionTests
{
    /// <summary>The dispatch file ToProperty generates.</summary>
    private const string DispatchName = "ToPropertyDispatch.g.cs";

    /// <summary>A library granting the consumer InternalsVisibleTo, which uses both features itself.</summary>
    private const string LibrarySource = """
                           using System;
                           using System.ComponentModel;
                           using System.Runtime.CompilerServices;
                           using ReactiveUI.Binding;

                           [assembly: InternalsVisibleTo("TestAssembly")]

                           namespace Library
                           {
                               public partial class LibraryViewModel : INotifyPropertyChanged
                               {
                                   public LibraryViewModel(IObservable<string> names) => _nameHelper = names.ToProperty(this, x => x.Name);

                                   public event PropertyChangedEventHandler? PropertyChanged;

                                   [ObservableAsProperty]
                                   public partial string Name { get; }
                               }
                           }
                           """;

    /// <summary>A consumer of that library, which uses both features too.</summary>
    private const string ConsumerSource = """
                            using System;
                            using System.ComponentModel;
                            using ReactiveUI.Binding;

                            namespace Consumer
                            {
                                public partial class ConsumerViewModel : INotifyPropertyChanged
                                {
                                    public ConsumerViewModel(IObservable<string> names)
                                    {
                                        _nameHelper = names.ToProperty(this, x => x.Name);
                                        Inner = new Library.LibraryViewModel(names);
                                    }

                                    public event PropertyChangedEventHandler? PropertyChanged;

                                    public Library.LibraryViewModel Inner { get; }

                                    [ObservableAsProperty]
                                    public partial string Name { get; }
                                }
                            }
                            """;

    /// <summary>
    /// A type that implements IReactiveObject itself, with no ReactiveObject base, still raises through ReactiveUI's
    /// extensions (ReactiveUI.SourceGenerators #224 and #467, ReactiveUI #1199).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandWrittenReactiveObject_RaisesThroughReactiveUi()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public abstract class CustomBase : global::ReactiveUI.IReactiveObject
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public event PropertyChangingEventHandler? PropertyChanging;

                                      void global::ReactiveUI.IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

                                      void global::ReactiveUI.IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);
                                  }

                                  public class WidgetViewModel : CustomBase
                                  {
                                      private readonly ObservableAsPropertyHelper<string> _title;

                                      public WidgetViewModel(IObservable<string> titles) => _title = titles.ToProperty(this, x => x.Title);

                                      public string Title => _title.Value;
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.GeneratedSourceContains(DispatchName, "IReactiveObjectExtensions.RaisePropertyChanged");
        await result.CompilationSucceeds();
    }

    /// <summary>
    /// A string initial value for a string property is not mistaken for a caller-information argument: a string name
    /// takes no caller information at all, and a selector's initial-value overload has priority from C# 13. Below
    /// C# 13 the selector call names its initial value.
    /// </summary>
    /// <param name="version">The C# version the consumer compiles with.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(LanguageVersion.CSharp10)]
    [Arguments(LanguageVersion.CSharp13)]
    public async Task StringInitialValue_IsNotAmbiguous(LanguageVersion version)
    {
        var selector = version >= LanguageVersion.CSharp13
            ? "        _title = names.ToProperty(this, x => x.Title, \"(untitled)\");\n"
            : "        _title = names.ToProperty(this, x => x.Title, initialValue: \"(untitled)\");\n";
        var source = $$"""
                     using System;
                     using System.ComponentModel;
                     using ReactiveUI.Binding;

                     namespace TestApp
                     {
                         public partial class MyViewModel : INotifyPropertyChanged
                         {
                             private readonly ObservableAsPropertyHelper<string> _name;

                             private readonly ObservableAsPropertyHelper<string> _title;

                             public MyViewModel(IObservable<string> names)
                             {
                                 _name = names.ToProperty(this, nameof(Name), "(none)");
                     {{selector}}            }

                             public event PropertyChangedEventHandler? PropertyChanged;

                             public string Name => _name.Value;

                             public string Title => _title?.Value ?? "";
                         }
                     }
                     """;

        var result = TestHelper.RunGenerator(source, version);
        await result.GeneratedSourceContains(DispatchName, "property == \"Name\"");
        await result.CompilationSucceeds();
    }

    /// <summary>
    /// Two assemblies joined by InternalsVisibleTo that both use ToProperty and [ObservableAsProperty] compile, since
    /// neither declares a type the other can see twice (ReactiveUI.SourceGenerators #111).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InternalsVisibleToFriends_BothGenerating_Compile()
    {
        var libraryResult = TestHelper.RunGenerator(
            TestHelper.CreateCompilation(LibrarySource, LanguageVersion.CSharp13, false, "Library", []),
            LanguageVersion.CSharp13,
            "Library",
            true);
        await libraryResult.CompilationSucceeds();

        var consumerCompilation = TestHelper.CreateCompilation(
            ConsumerSource,
            LanguageVersion.CSharp13,
            false,
            "TestAssembly",
            [libraryResult.OutputCompilation.ToMetadataReference()]);
        var consumerResult = TestHelper.RunGenerator(consumerCompilation, LanguageVersion.CSharp13, "Consumer", true);
        await consumerResult.CompilationSucceeds();
    }
}
