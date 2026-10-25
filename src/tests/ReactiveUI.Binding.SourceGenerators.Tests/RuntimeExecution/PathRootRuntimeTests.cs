// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// A generated path has to run from the lambda's parameter to the leaf. A link the generator cannot read, such as an
/// indexer, leaves the whole call to the runtime stub rather than generating the part after it; parentheses and
/// null-forgiving operators name no link and are read through.
/// </summary>
public class PathRootRuntimeTests
{
    /// <summary>The models and scenario entry points.</summary>
    private const string ScenarioSource = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public class Child : INotifyPropertyChanged
            {
                private string _name = "alpha";

                public event PropertyChangedEventHandler PropertyChanged;

                public string Name
                {
                    get { return _name; }
                    set { _name = value; var handler = PropertyChanged; if (handler != null) { handler(this, new PropertyChangedEventArgs("Name")); } }
                }
            }

            public class Parent : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public Child Child { get; } = new Child();

                public List<string> Items { get; } = new List<string> { "abc" };
            }

            public static class Usage
            {
                public static string ObserveThroughParentheses()
                {
                    var parent = new Parent();
                    var seen = "";
                    using (parent.WhenChanged(x => (x.Child).Name).Subscribe(new Sink(v => seen += "|" + v)))
                    {
                        parent.Child.Name = "beta";
                    }

                    return seen;
                }

                public static IObservable<int> ObserveThroughIndexer(Parent parent) => parent.WhenChanged(x => x.Items[0].Length);
            }

            public sealed class Sink : IObserver<string>
            {
                private readonly Action<string> _onNext;

                public Sink(Action<string> onNext) { _onNext = onNext; }

                public void OnNext(string value) { _onNext(value); }

                public void OnError(Exception error) { }

                public void OnCompleted() { }
            }
        }
        """;

    /// <summary>A parenthesized link is read through, so the whole path is observed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ParenthesizedPath_ObservesTheWholePath()
    {
        var result = TestHelper.RunGenerator(ScenarioSource, LanguageVersion.CSharp10);
        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod("ObserveThroughParentheses", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((string?)run.Invoke(null, null)).IsEqualTo("|alpha|beta");
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>A path through an indexer is left to the stub, not generated from the link after the indexer.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IndexerInPath_GeneratesNothingForThatCall()
    {
        var result = TestHelper.RunGenerator(ScenarioSource, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.GeneratedSourceDoesNotContain("WhenChangedDispatch.g.cs", "Length");
    }
}
