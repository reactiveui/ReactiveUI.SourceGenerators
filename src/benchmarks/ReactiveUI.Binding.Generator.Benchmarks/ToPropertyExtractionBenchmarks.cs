// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.Generator.Benchmarks.Support;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>
/// Measures the two checks a ToProperty call site can be turned away by, so the order the extractor runs them in is
/// backed by numbers: the selector's shape, read from syntax, and the call's resolution, which asks the semantic model.
/// </summary>
/// <remarks>
/// Each benchmark visits the same call sites. The shape check is what the extractor runs first; the resolution is what
/// it would have to run for every call site if it asked the model first. A fresh semantic model per invocation keeps
/// the model's binding cache from serving one iteration with the previous one's work.
/// </remarks>
public class ToPropertyExtractionBenchmarks
{
    /// <summary>The number of call sites in the corpus.</summary>
    private const int CallSiteCount = 200;

    /// <summary>The compilation holding the call sites.</summary>
    private Compilation _compilation = null!;

    /// <summary>The syntax tree holding the call sites.</summary>
    private SyntaxTree _tree = null!;

    /// <summary>The ToProperty invocations, found once so neither benchmark measures the scan.</summary>
    private InvocationExpressionSyntax[] _invocations = [];

    /// <summary>Builds a corpus of ToProperty call sites whose selectors reach past the source object.</summary>
    [GlobalSetup]
    public void Setup()
    {
        var source = new StringBuilder()
            .AppendLine("using System;")
            .AppendLine("using System.ComponentModel;")
            .AppendLine("using ReactiveUI.Binding;")
            .AppendLine("namespace Corpus;")
            .AppendLine("public partial class Vm : INotifyPropertyChanged")
            .AppendLine("{")
            .AppendLine("    public event PropertyChangedEventHandler? PropertyChanged;")
            .AppendLine("    public string Name { get; set; } = \"\";")
            .AppendLine("    public void Wire(IObservable<int> lengths)")
            .AppendLine("    {");
        for (var i = 0; i < CallSiteCount; i++)
        {
            _ = source.AppendLine("        lengths.ToProperty(this, x => x.Name.Length);");
        }

        _ = source.AppendLine("    }").AppendLine("}");

        _tree = CSharpSyntaxTree.ParseText(source.ToString(), GeneratorHarness.ParseOptions(false));
        _compilation = GeneratorHarness.BuildCompilation(false).AddSyntaxTrees(_tree);
        var invocations = new List<InvocationExpressionSyntax>(CallSiteCount);
        foreach (var descendant in _tree.GetRoot().DescendantNodes())
        {
            if (descendant is InvocationExpressionSyntax invocation)
            {
                invocations.Add(invocation);
            }
        }

        _invocations = [.. invocations];
    }

    /// <summary>Reads each selector's shape from syntax, the check the extractor runs before asking the model.</summary>
    /// <returns>The number of call sites the shape check turned away.</returns>
    [Benchmark(Baseline = true, Description = "Syntax: selector shape")]
    public int SyntaxShape()
    {
        var rejected = 0;
        for (var i = 0; i < _invocations.Length; i++)
        {
            if (!IsDirectMemberSelector(_invocations[i].ArgumentList.Arguments[1].Expression))
            {
                rejected++;
            }
        }

        return rejected;
    }

    /// <summary>Resolves each call through the semantic model, what the extractor would pay if it asked the model first.</summary>
    /// <returns>The number of call sites that resolved to a method.</returns>
    [Benchmark(Description = "Semantic: resolve the call")]
    public int SemanticResolve()
    {
        var model = _compilation.GetSemanticModel(_tree);
        var resolved = 0;
        for (var i = 0; i < _invocations.Length; i++)
        {
            if (model.GetSymbolInfo(_invocations[i]).Symbol is IMethodSymbol)
            {
                resolved++;
            }
        }

        return resolved;
    }

    /// <summary>The shape test the extractor makes: one member read straight off the lambda's own parameter.</summary>
    /// <param name="expression">The property argument.</param>
    /// <returns><see langword="true"/> for <c>x =&gt; x.Property</c>.</returns>
    private static bool IsDirectMemberSelector(ExpressionSyntax expression) =>
        expression is SimpleLambdaExpressionSyntax
        {
            Body: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax receiver },
        } lambda
        && receiver.Identifier.ValueText == lambda.Parameter.Identifier.ValueText;
}
