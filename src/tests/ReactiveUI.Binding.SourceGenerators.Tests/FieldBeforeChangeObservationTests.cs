// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers before-change observation of a field on a type that raises <c>PropertyChanging</c>. A field has no
/// mechanism of its own, so the observation falls to the before-change observable the owning type offers.
/// </summary>
public class FieldBeforeChangeObservationTests
{
    /// <summary>The dispatch file the WhenChanging call sites are generated into.</summary>
    private const string DispatchFileName = "WhenChangingDispatch.g.cs";

    /// <summary>The observable a field on a before-change type falls to.</summary>
    private const string ChangingObservable = "PropertyChangingObservable";

    /// <summary>A type that notifies before a change, observed through one of its fields.</summary>
    private const string FieldSource = """
                                       using System;
                                       using System.ComponentModel;
                                       using ReactiveUI.Binding;

                                       namespace Consumer
                                       {
                                           public class Model : INotifyPropertyChanged, INotifyPropertyChanging
                                           {
                                               public string Label = "";

                                               public event PropertyChangedEventHandler PropertyChanged;

                                               public event PropertyChangingEventHandler PropertyChanging;

                                               public string Title { get; set; } = "";
                                           }

                                           public static class Usage
                                           {
                                               public static IObservable<string> Single(Model model)
                                               {
                                                   return model.WhenChanging(x => x.Label);
                                               }

                                               public static IObservable<PropertyValues<string, string>> Multiple(Model model)
                                               {
                                                   return model.WhenChanging(x => x.Label, x => x.Title);
                                               }
                                           }
                                       }
                                       """;

    /// <summary>A single field, a field with a selector and a field among several paths each observe before the change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_FieldOnBeforeChangeType_ObservesBeforeChange()
    {
        var result = TestHelper.RunGenerator(FieldSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, ChangingObservable);
    }
}
