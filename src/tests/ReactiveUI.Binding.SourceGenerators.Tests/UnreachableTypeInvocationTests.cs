// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Call sites that name a private or protected nested type, in a build that writes dispatch overloads. Generated code
/// lives in a class of its own and cannot name such a type, so the generator writes nothing for the call rather than
/// code that fails the consumer's build. RXUIBIND015 then fails the build at the call.
/// </summary>
public class UnreachableTypeInvocationTests
{
    /// <summary>The <c>WhenChangedDispatch.g.cs</c> name these tests generate against.</summary>
    private const string WhenChangedDispatchName = "WhenChangedDispatch.g.cs";

    /// <summary>The <c>BindOneWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindOneWayDispatchName = "BindOneWayDispatch.g.cs";

    /// <summary>The <c>ToPropertyDispatch.g.cs</c> name these tests generate against.</summary>
    private const string ToPropertyDispatchName = "ToPropertyDispatch.g.cs";

    /// <summary>A WhenChanged call on a private nested type generates nothing, and the consumer still compiles.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_PrivateNestedReceiver_GeneratesNothing()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public static class Outer
                                  {
                                      public static IObservable<string?> Run() => new Vm().WhenChanged(x => x.Name);

                                      private sealed class Vm : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler? PropertyChanged;
                                          public string? Name { get; set; }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchName);
    }

    /// <summary>A path through a link of a private nested type generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_PrivateIntermediateLink_GeneratesNothing()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public sealed class Host : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      private Inner Child { get; } = new();

                                      public IObservable<string?> Run() => this.WhenChanged(x => x.Child.Name);

                                      private sealed class Inner : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler? PropertyChanged;
                                          public string? Name { get; set; }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchName);
    }

    /// <summary>A BindOneWay call onto a protected nested target generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ProtectedNestedTarget_GeneratesNothing()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class Source : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string? Name { get; set; }
                                  }

                                  public class Host
                                  {
                                      public IDisposable Run(Source source) => source.BindOneWay(new Target(), x => x.Name, x => x.Text);

                                      protected sealed class Target
                                      {
                                          public string? Text { get; set; }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchName);
    }

    /// <summary>A ToProperty call on a private nested source generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToProperty_PrivateNestedSource_GeneratesNothing()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public static class Outer
                                  {
                                      public static object Run(IObservable<string?> names) => new Vm(names);

                                      private sealed class Vm : INotifyPropertyChanged
                                      {
                                          private readonly ObservableAsPropertyHelper<string?> _name;

                                          public Vm(IObservable<string?> names) => _name = names.ToProperty(this, x => x.Name);

                                          public event PropertyChangedEventHandler? PropertyChanged;

                                          public string? Name => _name.Value;

                                          internal void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new(name));
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(ToPropertyDispatchName);
    }

    /// <summary>A BindTo onto a generic type closed over the calling method's type parameter generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_TargetClosedOverATypeParameter_GeneratesNothing()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public sealed class Source<TItem> : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public IReadOnlyList<TItem>? Data { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Run<TItem>(IObservable<IReadOnlyList<TItem>?> items) =>
                                          items.BindTo(new Source<TItem>(), static x => x.Data);
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource("BindToDispatch.g.cs");
    }

    /// <summary>An internal nested type is reachable, so the call still generates.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_InternalNestedReceiver_Generates()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public static class Outer
                                  {
                                      public static IObservable<string?> Run() => new Vm().WhenChanged(x => x.Name);

                                      internal sealed class Vm : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler? PropertyChanged;
                                          public string? Name { get; set; }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.HasGeneratedSource(WhenChangedDispatchName);
    }
}
