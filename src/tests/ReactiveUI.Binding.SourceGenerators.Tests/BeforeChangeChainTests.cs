// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers before-change observation across a chain whose intermediate notifies only before a change. Nothing
/// matches such a type on the after-change side, so the segment falls to the before-change observable directly
/// rather than to a mechanism chosen for the chain's root.
/// </summary>
public class BeforeChangeChainTests
{
    /// <summary>The dispatch file the WhenChanging call sites are generated into.</summary>
    private const string DispatchFileName = "WhenChangingDispatch.g.cs";

    /// <summary>The observable a segment falls to when its declaring type only notifies before a change.</summary>
    private const string ChangingObservable = "PropertyChangingObservable";

    /// <summary>A chain into a type that raises PropertyChanging and nothing else.</summary>
    private const string ChangingOnlyIntermediateSource = """
                                                          using System;
                                                          using System.ComponentModel;
                                                          using ReactiveUI.Binding;

                                                          namespace Consumer
                                                          {
                                                              public class Inner : INotifyPropertyChanging
                                                              {
                                                                  public event PropertyChangingEventHandler PropertyChanging;

                                                                  public string Name { get; set; } = "";
                                                              }

                                                              public class Outer : INotifyPropertyChanged, INotifyPropertyChanging
                                                              {
                                                                  public event PropertyChangedEventHandler PropertyChanged;

                                                                  public event PropertyChangingEventHandler PropertyChanging;

                                                                  public Inner Child { get; set; }

                                                                  public string Title { get; set; } = "";
                                                              }

                                                              public static class Usage
                                                              {
                                                                  public static IObservable<string> Single(Outer outer)
                                                                  {
                                                                      return outer.WhenChanging(x => x.Child.Name);
                                                                  }

                                                                  public static IObservable<PropertyValues<string, string>> Multiple(Outer outer)
                                                                  {
                                                                      return outer.WhenChanging(x => x.Child.Name, x => x.Title);
                                                                  }
                                                              }
                                                          }
                                                          """;

    /// <summary>
    /// A single-path chain observes the before-change intermediate through its own mechanism. The chain's root
    /// notifies both ways, so choosing from the root would reach for an after-change subscription the
    /// intermediate never offers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_ThroughAChangingOnlyIntermediate_ObservesItBeforeChange()
    {
        var result = TestHelper.RunGenerator(ChangingOnlyIntermediateSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, ChangingObservable);
    }

    /// <summary>The same holds when the chain is one of several paths combined into a single observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_ThroughAChangingOnlyIntermediateAlongsideAnotherPath_Compiles()
    {
        var result = TestHelper.RunGenerator(ChangingOnlyIntermediateSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, "__propObs");
    }
}
