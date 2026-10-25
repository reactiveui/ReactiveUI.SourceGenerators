// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes native command routes, enabled state, replacement and disposal.</summary>
public class NativeCommandParityTests
{
    /// <summary>The framework command contracts exercised by the generated consumer.</summary>
    internal const string Framework = """
        using System;
        using System.ComponentModel;
        using System.Reflection;
        using System.Windows.Input;
        using ReactiveUI.Binding;
        namespace Foundation
        {
            public class NSObject : IDisposable { public void Dispose() {} }
            public class ExportAttribute : Attribute
            {
                public string Selector { get; }
                public ExportAttribute(string selector) { Selector = selector; }
            }
        }
        namespace ObjCRuntime
        {
            public class Selector
            {
                public string Name { get; }
                public Selector(string name) { Name = name; }
            }
        }
        namespace Android.Views
        {
            public class View
            {
                public event EventHandler Click;
                public bool Enabled { get; set; }
                public bool HasBinding => Click != null;
                public void Raise() => Click?.Invoke(this, EventArgs.Empty);
            }
        }
        namespace UIKit
        {
            public enum UIControlEvent { TouchUpInside }
            public class UIControl : Foundation.NSObject
            {
                private event EventHandler _targets;
                public event EventHandler TouchUpInside;
                public bool Enabled { get; set; }
                public bool HasBinding => _targets != null || TouchUpInside != null;
                public void AddTarget(EventHandler handler, UIControlEvent kind) { _targets += handler; }
                public void RemoveTarget(EventHandler handler, UIControlEvent kind) { _targets -= handler; }
                public virtual void Raise() => _targets?.Invoke(this, EventArgs.Empty);
            }
            public class UIRefreshControl : UIControl
            {
                public event EventHandler ValueChanged;
                public new bool HasBinding => ValueChanged != null || base.HasBinding;
                public override void Raise() => ValueChanged?.Invoke(this, EventArgs.Empty);
            }
            public class UIBarButtonItem : Foundation.NSObject
            {
                public event EventHandler Clicked;
                public bool Enabled { get; set; }
                public bool HasBinding => Clicked != null;
                public void Raise() => Clicked?.Invoke(this, EventArgs.Empty);
            }
        }
        namespace AppKit
        {
            public class ActionHost : Foundation.NSObject
            {
                public Foundation.NSObject Target { get; set; }
                public ObjCRuntime.Selector Action { get; set; }
                public bool Enabled { get; set; }
                public bool HasBinding => Target != null;
                public void Raise()
                {
                    if (Target == null || Action == null) return;
                    foreach (var method in Target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        if (method.GetCustomAttribute<Foundation.ExportAttribute>()?.Selector == Action.Name)
                            method.Invoke(Target, new object[] { this });
                }
            }
            public class NSControl : ActionHost {}
            public class NSCell : ActionHost {}
            public class NSMenu : ActionHost {}
            public class NSMenuItem : ActionHost {}
            public class NSToolbarItem : ActionHost {}
        }
        namespace ReactiveUI.Binding.CommandBinding
        {
            public sealed class AppKitCommandTarget : Foundation.NSObject
            {
                private readonly ICommand _command;
                private readonly Func<object> _parameter;
                public AppKitCommandTarget(ICommand command, Func<object> parameter)
                {
                    _command = command;
                    _parameter = parameter;
                    IsEnabled = command.CanExecute(null);
                }
                public bool IsEnabled { get; set; }
                [Foundation.Export("theAction:")]
                public void Execute(Foundation.NSObject sender)
                {
                    var parameter = _parameter();
                    if (_command.CanExecute(parameter)) _command.Execute(parameter);
                }
                [Foundation.Export("validateMenuItem:")]
                public bool ValidateMenuItem(AppKit.NSMenuItem item) => IsEnabled;
            }
        }
        public class Command : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public int Count { get; private set; }
            public object Parameter { get; private set; }
            public bool Allowed { get; private set; } = true;
            public bool CanExecute(object parameter) => Allowed;
            public void Execute(object parameter) { Count++; Parameter = parameter; }
            public void SetAllowed(bool value) { Allowed = value; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
        }
        public class Model : INotifyPropertyChanged
        {
            private Command _command = new Command();
            private int _parameter;
            public event PropertyChangedEventHandler PropertyChanged;
            public Command Command { get { return _command; } set { _command = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Command")); } }
            public int Parameter { get { return _parameter; } set { _parameter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Parameter")); } }
        }
        public class Parameters : IObservable<int>
        {
            private event Action<int> _next;
            public bool HasObservers => _next != null;
            public void Send(int value) => _next?.Invoke(value);
            public IDisposable Subscribe(IObserver<int> observer)
            {
                _next += observer.OnNext;
                return new ReactiveUI.Primitives.Disposables.ActionDisposable(() => _next -= observer.OnNext);
            }
        }
        """;

    /// <summary>The native route executes commands and detaches both replaced and disposed controls.</summary>
    /// <param name="type">The native control type.</param>
    /// <param name="affinity">The native binder's score.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Android.Views.View", 9)]
    [Arguments("UIKit.UIControl", 9)]
    [Arguments("UIKit.UIRefreshControl", 10)]
    [Arguments("UIKit.UIBarButtonItem", 10)]
    [Arguments("AppKit.NSControl", 4)]
    [Arguments("AppKit.NSCell", 4)]
    [Arguments("AppKit.NSMenu", 4)]
    [Arguments("AppKit.NSMenuItem", 4)]
    [Arguments("AppKit.NSToolbarItem", 4)]
    public async Task BindCommand_ExecutesNativeContract(string type, int affinity)
    {
        var result = TestHelper.RunGenerator(Scenario(type), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("BindCommandDispatch.g.cs", $">({affinity}, false)");
        await AssertRuns(result);
    }

    /// <summary>Native commands read current typed parameters and release their subscriptions.</summary>
    /// <param name="type">The native control type.</param>
    /// <param name="expression">Whether the parameter is a property expression.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Android.Views.View", false)]
    [Arguments("UIKit.UIControl", false)]
    [Arguments("UIKit.UIRefreshControl", false)]
    [Arguments("UIKit.UIBarButtonItem", false)]
    [Arguments("AppKit.NSControl", false)]
    [Arguments("Android.Views.View", true)]
    [Arguments("UIKit.UIControl", true)]
    [Arguments("UIKit.UIRefreshControl", true)]
    [Arguments("UIKit.UIBarButtonItem", true)]
    [Arguments("AppKit.NSControl", true)]
    public async Task BindCommand_TracksParameters(string type, bool expression)
    {
        var result = TestHelper.RunGenerator(ParameterScenario(type, expression), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await AssertRuns(result);
    }

    /// <summary>The System.Reactive package executes the same native command behavior.</summary>
    /// <param name="type">The native control type.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Android.Views.View")]
    [Arguments("UIKit.UIControl")]
    [Arguments("AppKit.NSControl")]
    public async Task BindCommand_ReactiveRuntimeExecutesNativeContract(string type)
    {
        var source = Scenario(type)
            .Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal)
            .Replace("namespace ReactiveUI.Binding.CommandBinding", "namespace ReactiveUI.Binding.Reactive.CommandBinding", StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, true);
        await result.CompilationSucceeds();
        await AssertRuns(result);
    }

    /// <summary>Runs the consumer's behavior checks in its collectible assembly.</summary>
    /// <param name="result">The compiled binding consumer.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task AssertRuns(GeneratorTestResult result)
    {
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

    /// <summary>Builds a consumer with either a hot stream or an observed property parameter.</summary>
    /// <param name="type">The native control.</param>
    /// <param name="expression">Whether the property supplies parameters.</param>
    /// <returns>The executable consumer.</returns>
    private static string ParameterScenario(string type, bool expression) => Framework + $$"""
        public class View : IViewFor<Model>
        {
            public Model ViewModel { get; set; }
            object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Model)value; } }
            public {{type}} Control { get; } = new {{type}}();
        }
        public static class Usage
        {
            public static bool Run()
            {
                var model = new Model { Parameter = 7 };
                var view = new View { ViewModel = model };
                var parameters = new Parameters();
                using (view.BindCommand(model, x => x.Command, x => x.Control, {{(expression ? "x => x.Parameter" : "parameters")}}))
                {
                    view.Control.Raise();
                    if (!object.Equals(model.Command.Parameter, {{(expression ? "(object)7" : "null")}})) return false;
                    model.Parameter = 42;
                    parameters.Send(42);
                    view.Control.Raise();
                    if (!object.Equals(model.Command.Parameter, 42)) return false;
                    var previous = model.Command;
                    model.Command = new Command();
                    model.Parameter = 43;
                    parameters.Send(43);
                    view.Control.Raise();
                    if (previous.Count != 2 || model.Command.Count != 1 || !object.Equals(model.Command.Parameter, 43)) return false;
                }
                return !parameters.HasObservers && !view.Control.HasBinding;
            }
        }
        """;

    /// <summary>Builds a view that can replace its control and command independently.</summary>
    /// <param name="type">The native control type.</param>
    /// <returns>The executable consumer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Scenario(string type) => Framework + $$"""
        public class View : IViewFor<Model>, INotifyPropertyChanged
        {
            private {{type}} _control = new {{type}}();
            public Model ViewModel { get; set; }
            object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Model)value; } }
            public event PropertyChangedEventHandler PropertyChanged;
            public {{type}} Control { get { return _control; } set { _control = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Control")); } }
        }
        public static class Usage
        {
            public static bool Run()
            {
                var model = new Model();
                var command = model.Command;
                var view = new View { ViewModel = model };
                var first = view.Control;
                var second = new {{type}}();
                var replacement = new Command();
                using (view.BindCommand(model, x => x.Command, x => x.Control))
                {
                    if (!first.Enabled) return false;
                    first.Raise();
                    if (command.Count != 1 || command.Parameter != null) return false;
                    command.SetAllowed(false);
                    first.Raise();
                    if (first.Enabled || command.Count != 1) return false;
                    model.Command = replacement;
                    first.Raise();
                    if (!first.Enabled || replacement.Count != 1) return false;
                    view.Control = second;
                    first.Raise();
                    if (first.HasBinding || replacement.Count != 1) return false;
                    second.Raise();
                    if (replacement.Count != 2) return false;
                }
                second.Raise();
                return !second.HasBinding && replacement.Count == 2;
            }
        }
        """;
}
