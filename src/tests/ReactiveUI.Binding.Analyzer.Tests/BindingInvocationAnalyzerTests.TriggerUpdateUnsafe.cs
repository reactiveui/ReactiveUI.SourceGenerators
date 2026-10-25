// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Signal-driven Unsafe bindings accept runtime expressions without generated-path diagnostics.</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>Both real Unsafe overloads accept stored expressions and paths the generator cannot inspect.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_TriggerOverloads_DoNotReportGeneratedPathDiagnostics()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;
            using ReactiveUI.Binding;

            public class Model
            {
                public string Name { get; set; } = "model";
                public string ReadName() => Name;
            }

            public class View : IViewFor
            {
                public object ViewModel { get; set; }
                public string Text { get; set; } = "view";

                public void Bind(Model model, IObservable<int> updates)
                {
                    Expression<Func<Model, string>> modelPath = value => value.Name;
                    Expression<Func<View, string>> viewPath = value => value.Text;
                    this.BindUnsafe(model, modelPath, viewPath, updates);
                    this.BindUnsafe(model, modelPath, viewPath, value => value, value => value, updates);
                    this.BindUnsafe(model, value => value.ReadName(), value => value.Text, updates);
                    this.BindUnsafe(model, value => value.ReadName(), value => value.Text,
                        value => value, value => value, updates, TriggerUpdate.ViewModelToView);
                }
            }
            """;
        var compilation = AnalyzerTestHelper.CreateCompilation(source)
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(ReactiveUIBindingExtensions).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Primitives.Concurrency.ISequencer).Assembly.Location));
        await Assert.That(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();

        var diagnostics = await compilation.WithAnalyzers([new BindingInvocationAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        await Assert.That(diagnostics).IsEmpty();
    }
}
