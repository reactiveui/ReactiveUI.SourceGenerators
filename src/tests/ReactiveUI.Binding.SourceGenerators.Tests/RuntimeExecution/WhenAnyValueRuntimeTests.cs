// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>Executes generated value projections through replaceable property paths.</summary>
public class WhenAnyValueRuntimeTests
{
    /// <summary>The notifying parent, child, and typed recorder used by the executable scenarios.</summary>
    private const string ScenarioTypes = """
        public sealed class Node : INotifyPropertyChanged
        {
            private int _value;
            public event PropertyChangedEventHandler PropertyChanged;
            public int Value
            {
                get => _value;
                set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
            }
        }

        public sealed class Model : INotifyPropertyChanged
        {
            private Node _child = new Node { Value = 5 };
            public event PropertyChangedEventHandler PropertyChanged;
            public Node Child
            {
                get => _child;
                set { _child = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child))); }
            }
        }

        public sealed class Recorder<T> : IObserver<T>
        {
            public readonly List<T> Values = new List<T>();
            public void OnNext(T value) => Values.Add(value);
            public void OnError(Exception error) => throw error;
            public void OnCompleted() { }
        }
        """;

    /// <summary>The projected integer values after initialization, a change, and parent replacement.</summary>
    private static readonly int[] ProjectedValues = [6, 10, 13];

    /// <summary>The projected text when the selector changes the observed value's type.</summary>
    private static readonly string[] ProjectedText = ["value:5", "value:9", "value:12"];

    /// <summary>A deep selector runs for each distinct leaf value and follows the replacement parent.</summary>
    /// <param name="reactive">Whether the consumer references the System.Reactive flavor.</param>
    /// <param name="stringify">Whether the selector changes the leaf value's type.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task DeepPropertySelector_ProjectsAndFollowsParentReplacement(
        [Matrix(false, true)] bool reactive,
        [Matrix(false, true)] bool stringify)
    {
        var runtimeNamespace = reactive ? "ReactiveUI.Binding.Reactive" : "ReactiveUI.Binding";
        var valueType = stringify ? "string" : "int";
        var selector = stringify ? "(int value) => \"value:\" + value" : "value => value + 1";
        var source = $$"""
            using System;
            using System.Collections.Generic;
            using System.ComponentModel;
            using {{runtimeNamespace}};

            namespace TestApp
            {
                {{ScenarioTypes}}

                public static class Usage
                {
                    public static {{valueType}}[] Run()
                    {
                        var model = new Model();
                        var observer = new Recorder<{{valueType}}>();
                        using (model.WhenAnyValue(x => x.Child.Value, {{selector}}).Subscribe(observer))
                        {
                            var previous = model.Child;
                            previous.Value = 9;
                            model.Child = new Node { Value = 9 };
                            previous.Value = 100;
                            model.Child.Value = 12;
                        }
                        model.Child.Value = 99;
                        return observer.Values.ToArray();
                    }
                }
            }
            """;
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, reactive);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var values = assembly.GetType("TestApp.Usage")!.GetMethod("Run")!.Invoke(null, null);
            if (stringify)
            {
                await Assert.That((string[])values!).IsEquivalentTo(ProjectedText);
            }
            else
            {
                await Assert.That((int[])values!).IsEquivalentTo(ProjectedValues);
            }
        }
        finally
        {
            context.Unload();
        }
    }
}
