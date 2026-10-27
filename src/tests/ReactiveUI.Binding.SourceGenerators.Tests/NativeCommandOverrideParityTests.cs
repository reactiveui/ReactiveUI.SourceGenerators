// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Checks native command votes against scoped custom registrations.</summary>
public class NativeCommandOverrideParityTests
{
    /// <summary>The generated generic event score.</summary>
    private const int ExplicitEventAffinity = 4;

    /// <summary>The generated Android native score.</summary>
    private const int AndroidAffinity = 9;

    /// <summary>A native control that also offers command properties.</summary>
    private const string ViewSource = """
        public class BoundControl : Android.Views.View
        {
            public ICommand Command { get; set; }
            public object CommandParameter { get; set; }
        }
        public class View : IViewFor<Model>
        {
            public Model ViewModel { get; set; }
            object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Model)value; } }
            public BoundControl Control { get; } = new BoundControl();
        }
        """;

    /// <summary>A registration must beat the generated score and receives the selected event overload.</summary>
    /// <param name="score">The custom binder's score.</param>
    /// <param name="explicitEvent">Whether the caller selected Click explicitly.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(8, false)]
    [Arguments(9, false)]
    [Arguments(10, false)]
    [Arguments(3, true)]
    [Arguments(4, true)]
    [Arguments(5, true)]
    public async Task BindCommand_CustomProviderRequiresHigherAffinity(int score, bool explicitEvent)
    {
        var result = TestHelper.RunGenerator(Scenario(score, explicitEvent), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>Builds a custom registration with distinct default and named-event behavior.</summary>
    /// <param name="score">The registration's affinity.</param>
    /// <param name="explicitEvent">Whether the binding supplies an event name.</param>
    /// <returns>The executable consumer.</returns>
    private static string Scenario(int score, bool explicitEvent) => NativeCommandParityTests.Framework + ViewSource + $$"""
        public class Binder : ICreatesCommandBinding
        {
            public int Calls;
            public int Disposals;
            public string Event;
            public bool EventTarget;
            public int GetAffinityForObject<T>(bool hasEventTarget)
            {
                EventTarget = hasEventTarget;
                return typeof(T) == typeof(BoundControl) ? {{score}} : 0;
            }
            public IDisposable BindCommandToObject<T>(ICommand command, T target, IObservable<object> parameter) where T : class
                => Bind(command, "default");
            public IDisposable BindCommandToObject<T, TArgs>(ICommand command, T target, IObservable<object> parameter, string eventName) where T : class
                => Bind(command, eventName);
            public IDisposable BindCommandToObject<T, TArgs>(ICommand command, T target, IObservable<object> parameter,
                Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove) where T : class where TArgs : EventArgs
                => throw new InvalidOperationException("Unexpected delegate overload");
            private IDisposable Bind(ICommand command, string name)
            {
                Calls++;
                Event = name;
                return new ReactiveUI.Primitives.Disposables.ActionDisposable(() => Disposals++);
            }
        }
        public static class Usage
        {
            public static bool Run()
            {
                var binder = new Binder();
                using (var resolver = new Splat.ModernDependencyResolver())
                using (Splat.DependencyResolverMixins.WithResolver(resolver))
                {
                    resolver.Register<ICreatesCommandBinding>(() => binder);
                    var model = new Model();
                    var view = new View { ViewModel = model };
                    using (view.BindCommand(model, x => x.Command, x => x.Control{{(explicitEvent ? ", toEvent: \"Click\"" : string.Empty)}}))
                    {
                        view.Control.Raise();
                        if ({{score}} > {{(explicitEvent ? ExplicitEventAffinity : AndroidAffinity)}})
                        {
                            if (binder.Calls != 1 || model.Command.Count != 0 || binder.Event != "{{(explicitEvent ? "Click" : "default")}}") return false;
                        }
                        else if (binder.Calls != 0 || model.Command.Count != 1) return false;
                        if (binder.EventTarget != {{(explicitEvent ? "true" : "false")}}) return false;
                    }
                    return binder.Disposals == binder.Calls;
                }
            }
        }
        """;
}
