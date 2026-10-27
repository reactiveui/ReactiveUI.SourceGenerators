// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes the observation contracts shared by the native and managed providers.</summary>
public class ObservationContractsParityTests
{
    /// <summary>The parent reports replacements after the write; the leaf reports values before it.</summary>
    private const string BeforeChangeScenario = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        public class Leaf : INotifyPropertyChanging
        {
            private int _value;
            public event PropertyChangingEventHandler PropertyChanging;
            public int Value { get { return _value; } set { PropertyChanging?.Invoke(this, new PropertyChangingEventArgs("Value")); _value = value; } }
        }
        public class Model : INotifyPropertyChanged
        {
            private Leaf _child = new Leaf();
            public event PropertyChangedEventHandler PropertyChanged;
            public Leaf Child { get { return _child; } set { _child = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Child")); } }
        }
        public static class Usage
        {
            public static bool Run()
            {
                var model = new Model();
                var previous = model.Child;
                var observer = new Observer();
                using (model.WhenChanging(x => x.Child.Value).Subscribe(observer))
                {
                    model.Child = new Leaf { Value = 10 };
                    model.Child.Value = 11;
                    model.Child.Value = 12;
                    previous.Value = 99;
                    return observer.Value == 11;
                }
            }
            private sealed class Observer : IObserver<int>
            {
                public int Value;
                public void OnNext(int value) { Value = value; }
                public void OnError(Exception error) { throw error; }
                public void OnCompleted() {}
            }
        }
        """;

    /// <summary>Fallback observations read at subscription time and remain open.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Poco_ReadsWhenSubscribed()
    {
        const string source = """
            using System;
            using ReactiveUI.Binding;
            public class Model { public int Value { get; set; } }
            public static class Usage
            {
                public static bool Run()
                {
                    var model = new Model { Value = 1 };
                    var observation = model.WhenChanged(x => x.Value);
                    model.Value = 2;
                    var observer = new Observer();
                    using (observation.Subscribe(observer)) return observer.Value == 2 && !observer.Completed;
                }
                private sealed class Observer : IObserver<int>
                {
                    public int Value;
                    public bool Completed;
                    public void OnNext(int value) { Value = value; }
                    public void OnError(Exception error) { throw error; }
                    public void OnCompleted() { Completed = true; }
                }
            }
            """;
        await ExecutesSuccessfully(source);
    }

    /// <summary>A derived object's notification interface applies to inherited properties in generated bindings.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InheritedProperty_UsesObservedOwnersNotification()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;
            public class BaseModel { public int Value { get; set; } }
            public class Model : BaseModel, INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;
                public void Change(int value) { Value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value")); }
            }
            public class Target { public int Value { get; set; } }
            public static class Usage
            {
                public static bool Run()
                {
                    var model = new Model { Value = 1 };
                    var target = new Target();
                    using (model.BindOneWay(target, x => x.Value, x => x.Value))
                    {
                        model.Change(2);
                        return target.Value == 2;
                    }
                }
            }
            """;
        await ExecutesSuccessfully(source);
    }

    /// <summary>Before-change chains follow parent replacement and observe the leaf's prior value.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BeforeChangeChain_FollowsReplacement() => ExecutesSuccessfully(BeforeChangeScenario);

    /// <summary>Compiles and executes a generated consumer's assertions.</summary>
    /// <param name="source">The complete consumer source.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task ExecutesSuccessfully(string source)
    {
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Static | BindingFlags.Public)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }
}
