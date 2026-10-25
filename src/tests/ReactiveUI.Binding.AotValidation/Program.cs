// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>
/// Entry point that exercises the source-generated bindings to confirm they work under Native AOT.
/// Each scenario is a dedicated method so the validation surface is easy to read and extend.
/// </summary>
internal static class Program
{
    /// <summary>A sample name value used across the WhenChanged scenarios.</summary>
    private const string InitialName = "Alice";

    /// <summary>The replacement string value used to assert change propagation.</summary>
    private const string ReplacementName = "Updated";

    /// <summary>The initial source name value used in the binding scenarios.</summary>
    private const string SourceName = "Source";

    /// <summary>The initial age value used in the two-property scenario.</summary>
    private const int InitialAge = 30;

    /// <summary>The replacement age value used to assert change propagation.</summary>
    private const int ReplacementAge = 31;

    /// <summary>The value set before the binding is disposed, which must survive the disposal.</summary>
    private const string BeforeDisposal = "Before";

    /// <summary>The number of executions expected after the initial value and one change.</summary>
    private const int ExpectedTwoExecutions = 2;

    /// <summary>
    /// Sink for scenario results. This validation harness runs standalone under Native AOT with no host
    /// or logging infrastructure, so its report goes straight to the process output stream.
    /// </summary>
    private static readonly TextWriter _output = Console.Out;

    /// <summary>The number of scenarios that have passed.</summary>
    private static int _passed;

    /// <summary>The number of scenarios that have failed.</summary>
    private static int _failed;

    /// <summary>Runs every AOT binding validation scenario and reports the aggregate result.</summary>
    /// <returns>Zero if every scenario passed; otherwise, one.</returns>
    internal static int Main()
    {
        ValidateWhenChangedSingleProperty();
        ValidateWhenChangedDeepChain();
        ValidateWhenChangedTwoProperties();
        ValidateWhenChangedOnViewType();
        ValidateBindOneWay();
        ValidateBindTwoWay();
        ValidateBindOneWayDisposal();
        ValidateOneWayBind();
        ValidateBind();
        ValidateInvokeCommand();
        ValidateBindCommand();
        ValidateBindCommandWithParameter();
        ValidateBindCommandToNamedEvent();
        ValidateWhenAny();
        ValidateWhenAnyObservable();
        ValidateResolveViewFromObject();

        Report(string.Empty);
        Report($"AOT Validation: {_passed} passed, {_failed} failed");
        return _failed > 0 ? 1 : 0;
    }

    /// <summary>WhenChanged on a single property emits the initial value and subsequent changes.</summary>
    private static void ValidateWhenChangedSingleProperty()
    {
        var vm = new AotViewModel { Name = InitialName };
        string? last = null;
        using var sub = vm.WhenChanged(x => x.Name).Subscribe(v => last = v);
        AssertEqual("WhenChanged initial", InitialName, last);
        vm.Name = "Bob";
        AssertEqual("WhenChanged after set", "Bob", last);
    }

    /// <summary>WhenChanged across a deep property chain tracks changes to the leaf value.</summary>
    private static void ValidateWhenChangedDeepChain()
    {
        var vm = new AotViewModel();
        vm.Child.Value = "Deep";
        string? last = null;
        using var sub = vm.WhenChanged(x => x.Child.Value).Subscribe(v => last = v);
        AssertEqual("WhenChanged deep initial", "Deep", last);
        vm.Child.Value = "Deeper";
        AssertEqual("WhenChanged deep after set", "Deeper", last);
    }

    /// <summary>WhenChanged on two properties emits a tuple and updates when either changes.</summary>
    private static void ValidateWhenChangedTwoProperties()
    {
        var vm = new AotViewModel { Name = InitialName, Age = InitialAge };
        PropertyValues<string, int> last = default;
        using var sub = vm.WhenChanged(x => x.Name, x => x.Age).Subscribe(v => last = v);
        AssertEqual("WhenChanged two-prop name", InitialName, last.Property1);
        AssertEqual("WhenChanged two-prop age", InitialAge, last.Property2);
        vm.Age = ReplacementAge;
        AssertEqual("WhenChanged two-prop age update", ReplacementAge, last.Property2);
    }

    /// <summary>WhenChanged on the view type produces a dispatch entry required by BindTwoWay.</summary>
    private static void ValidateWhenChangedOnViewType()
    {
        var view = new AotView { DisplayName = "ViewVal" };
        string? last = null;
        using var sub = view.WhenChanged(x => x.DisplayName).Subscribe(v => last = v);
        AssertEqual("WhenChanged on view initial", "ViewVal", last);
        view.DisplayName = ReplacementName;
        AssertEqual("WhenChanged on view after set", ReplacementName, last);
    }

    /// <summary>BindOneWay propagates source changes to the target property.</summary>
    private static void ValidateBindOneWay()
    {
        var source = new AotViewModel { Name = SourceName };
        var target = new AotView();
        using var binding = source.BindOneWay(target, x => x.Name, x => x.DisplayName);
        AssertEqual("BindOneWay initial", SourceName, target.DisplayName);
        source.Name = ReplacementName;
        AssertEqual("BindOneWay after set", ReplacementName, target.DisplayName);
    }

    /// <summary>BindTwoWay propagates changes in both directions between source and target.</summary>
    private static void ValidateBindTwoWay()
    {
        var source = new AotViewModel { Name = SourceName };
        var target = new AotView();
        using var binding = source.BindTwoWay(target, x => x.Name, x => x.DisplayName);
        AssertEqual("BindTwoWay initial", SourceName, target.DisplayName);
        source.Name = "FromSource";
        AssertEqual("BindTwoWay source→target", "FromSource", target.DisplayName);
        target.DisplayName = "FromTarget";
        AssertEqual("BindTwoWay target→source", "FromTarget", source.Name);
    }

    /// <summary>BindOneWay stops propagating once the binding is disposed.</summary>
    private static void ValidateBindOneWayDisposal()
    {
        var source = new AotViewModel { Name = BeforeDisposal };
        var target = new AotView();
        var binding = source.BindOneWay(target, x => x.Name, x => x.DisplayName);
        AssertEqual("BindOneWay pre-dispose", BeforeDisposal, target.DisplayName);
        binding.Dispose();
        source.Name = "After";
        AssertEqual("BindOneWay post-dispose unchanged", BeforeDisposal, target.DisplayName);
    }

    /// <summary>OneWayBind, which observes the view model through the view, propagates changes to the view.</summary>
    private static void ValidateOneWayBind()
    {
        var viewModel = new AotViewModel { Name = SourceName };
        var view = new AotView { ViewModel = viewModel };
        using var binding = view.OneWayBind(viewModel, x => x.Name, x => x.DisplayName);
        AssertEqual("OneWayBind initial", SourceName, view.DisplayName);
        viewModel.Name = ReplacementName;
        AssertEqual("OneWayBind after set", ReplacementName, view.DisplayName);
    }

    /// <summary>Bind, which observes both sides through the view, propagates changes in both directions.</summary>
    private static void ValidateBind()
    {
        var viewModel = new AotViewModel { Name = SourceName };
        var view = new AotView { ViewModel = viewModel };
        using var binding = view.Bind(viewModel, x => x.Name, x => x.DisplayName);
        AssertEqual("Bind initial", SourceName, view.DisplayName);
        viewModel.Name = "FromViewModel";
        AssertEqual("Bind view model to view", "FromViewModel", view.DisplayName);
        view.DisplayName = "FromView";
        AssertEqual("Bind view to view model", "FromView", viewModel.Name);
    }

    /// <summary>InvokeCommand executes the command a view model holds with each observed value.</summary>
    private static void ValidateInvokeCommand()
    {
        var viewModel = new AotViewModel { Name = InitialName };
        var command = new AotCommand();
        viewModel.Save = command;

        using var invocation = viewModel.WhenChanged(x => x.Name).InvokeCommand(viewModel, x => x.Save);
        AssertEqual("InvokeCommand initial", 1, command.ExecuteCount);
        AssertEqual("InvokeCommand initial parameter", InitialName, command.LastParameter as string);

        viewModel.Name = ReplacementName;
        AssertEqual("InvokeCommand after set", ExpectedTwoExecutions, command.ExecuteCount);
        AssertEqual("InvokeCommand parameter after set", ReplacementName, command.LastParameter as string);
    }

    /// <summary>BindCommand executes the view model's command when the control raises its Click event.</summary>
    private static void ValidateBindCommand()
    {
        var command = new AotCommand();
        var viewModel = new AotViewModel { Save = command };
        var view = new AotView { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.Save, v => v.SaveButton);
        view.SaveButton.PerformClick();
        AssertEqual("BindCommand click", 1, command.ExecuteCount);
    }

    /// <summary>BindCommand passes the latest value of a parameter observable to the command.</summary>
    private static void ValidateBindCommandWithParameter()
    {
        var command = new AotCommand();
        var viewModel = new AotViewModel { Save = command };
        var view = new AotView { ViewModel = viewModel, DisplayName = InitialName };

        using var binding = view.BindCommand(viewModel, x => x.Save, v => v.SaveButton, view.WhenChanged(v => v.DisplayName));
        view.SaveButton.PerformClick();
        AssertEqual("BindCommand parameter", InitialName, command.LastParameter as string);
    }

    /// <summary>BindCommand binds to the event the call names instead of Click.</summary>
    private static void ValidateBindCommandToNamedEvent()
    {
        var command = new AotCommand();
        var viewModel = new AotViewModel { Save = command };
        var view = new AotView { ViewModel = viewModel };

        using var binding = view.BindCommand(viewModel, x => x.Save, v => v.SaveButton, nameof(AotButton.Pressed));
        view.SaveButton.PerformClick();
        view.SaveButton.PerformPress();
        AssertEqual("BindCommand named event", 1, command.ExecuteCount);
    }

    /// <summary>WhenAny reports the observed change, and its selector projects it.</summary>
    private static void ValidateWhenAny()
    {
        var vm = new AotViewModel { Name = InitialName };
        string? last = null;
        using var sub = vm.WhenAny(x => x.Name, static c => c.Value).Subscribe(v => last = v);
        AssertEqual("WhenAny initial", InitialName, last);
        vm.Name = ReplacementName;
        AssertEqual("WhenAny after set", ReplacementName, last);
    }

    /// <summary>WhenAnyObservable switches to the stream the observed property holds.</summary>
    private static void ValidateWhenAnyObservable()
    {
        var stream = new System.Reactive.Subjects.Subject<string>();
        var vm = new AotViewModel { Signal = stream };
        string? last = null;
        using var sub = vm.WhenAnyObservable(x => x.Signal!).Subscribe(v => last = v);
        stream.OnNext(InitialName);
        AssertEqual("WhenAnyObservable first value", InitialName, last);
        stream.OnNext(ReplacementName);
        AssertEqual("WhenAnyObservable second value", ReplacementName, last);
    }

    /// <summary>ResolveView on a view model held as an object finds the generated view and a mapping keyed by the runtime type.</summary>
    private static void ValidateResolveViewFromObject()
    {
        const string MappedContract = "mapped";
        object viewModel = new AotViewModel();
        var locator = new DefaultViewLocator();
        var mappedView = new AotView();
        locator.Map<AotViewModel>(() => mappedView, MappedContract);

        var generated = locator.ResolveView(viewModel);
        var mapped = locator.ResolveView(viewModel, MappedContract);

        AssertEqual("ResolveView(object) generated view", true, generated is AotView && ReferenceEquals(generated.ViewModel, viewModel));
        AssertEqual("ResolveView(object) mapped view", true, ReferenceEquals(mapped, mappedView) && ReferenceEquals(mappedView.ViewModel, viewModel));
    }

    /// <summary>Compares an expected and actual value, recording a pass or failure to the console.</summary>
    /// <typeparam name="T">The value type being compared.</typeparam>
    /// <param name="label">A human-readable label for the scenario.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value produced by the binding.</param>
    private static void AssertEqual<T>(string label, T expected, T? actual)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            Report($"  PASS: {label}");
            _passed++;
        }
        else
        {
            Report($"  FAIL: {label} - expected '{expected}', got '{actual}'");
            _failed++;
        }
    }

    /// <summary>Writes a single line of the validation report to the process output stream.</summary>
    /// <param name="message">The line to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Report(string message) => _output.WriteLine(message);
}
