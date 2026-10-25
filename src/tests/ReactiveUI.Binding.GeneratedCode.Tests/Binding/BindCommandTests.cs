// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Tests that the source-generator-generated BindCommand code works correctly at runtime.</summary>
public class BindCommandTests
{
    /// <summary>The expression-parameter test item value.</summary>
    private const string TestItem = "TestItem";

    /// <summary>The observable command-parameter value.</summary>
    private const string ObsParam = "obs-param";

    /// <summary>The command parameter the observable is moved to.</summary>
    private const string ReplacementValue = "updated";

    /// <summary>The number of clicks performed in the multiple-clicks test.</summary>
    private const int ExpectedClickCount = 3;

    /// <summary>Verifies that clicking the button executes the bound command (command set before binding).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_CommandSetBeforeBinding_ExecutesOnClick()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.BasicNoParam(vm, view);

        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that clicking the button executes the bound command (command set after binding).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_CommandSetAfterBinding_ExecutesOnClick()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var command = new TrackingCommand();

        using var binding = BindCommandScenarios.BasicNoParam(vm, view);

        vm.Save = command;
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that when the command is null, clicking the button does not throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_NullCommand_ClickDoesNotThrow()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();

        using var binding = BindCommandScenarios.BasicNoParam(vm, view);

        await Assert.That((Action)view.SaveButton.PerformClick).ThrowsNothing();
    }

    /// <summary>Verifies that when the command changes, the new command is executed on click.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_CommandChanges_NewCommandExecutes()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var first = new TrackingCommand();
        var second = new TrackingCommand();

        vm.Save = first;
        using var binding = BindCommandScenarios.BasicNoParam(vm, view);

        view.SaveButton.PerformClick();
        await Assert.That(first.ExecuteCount).IsEqualTo(1);

        vm.Save = second;
        view.SaveButton.PerformClick();

        await Assert.That(second.ExecuteCount).IsEqualTo(1);
        await Assert.That(first.ExecuteCount).IsEqualTo(1); // no longer wired
    }

    /// <summary>Verifies that disposing the binding stops command execution on click.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_Dispose_StopsExecution()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        var binding = BindCommandScenarios.BasicNoParam(vm, view);
        view.SaveButton.PerformClick();
        await Assert.That(command.ExecuteCount).IsEqualTo(1);

        binding.Dispose();
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that when CanExecute returns false, the command is not executed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_CanExecuteFalse_CommandNotExecuted()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var command = new TrackingCommand { CanExecuteResult = false };
        vm.Save = command;

        using var binding = BindCommandScenarios.BasicNoParam(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Verifies that the expression parameter is passed to the command on click.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionParam_PassesParameterToCommand()
    {
        var vm = new SharedScenarios.BindCommand.ExpressionParam.MyViewModel { CurrentItem = TestItem };
        var view = new SharedScenarios.BindCommand.ExpressionParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.ExpressionParam(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo(TestItem);
    }

    /// <summary>
    /// Verifies that the expression parameter is captured at command-bind time (when Save changes),
    /// not at click time. This reflects the generated binder path used at runtime.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionParam_ParameterCapturedAtCommandBindTime()
    {
        var vm = new SharedScenarios.BindCommand.ExpressionParam.MyViewModel { CurrentItem = "Initial" };
        var view = new SharedScenarios.BindCommand.ExpressionParam.MyView();
        var command = new TrackingCommand();

        // Binding created with no command yet
        using var binding = BindCommandScenarios.ExpressionParam(vm, view);

        // Change CurrentItem and then set command — parameter captured at this moment
        vm.CurrentItem = "AtCommandSetTime";
        vm.Save = command;

        view.SaveButton.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo("AtCommandSetTime");
    }

    /// <summary>Verifies that the observable parameter is passed to the command on click.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableParam_PassesParameterToCommand()
    {
        var vm = new SharedScenarios.BindCommand.ObservableParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.ObservableParam.MyView();
        var command = new TrackingCommand();
        var paramSubject = new System.Reactive.Subjects.BehaviorSubject<string>("observable-param");
        vm.Save = command;

        using var binding = BindCommandScenarios.ObservableParam(vm, view, paramSubject);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo("observable-param");
    }

    /// <summary>Verifies that clicking the button 3 times results in 3 executions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_MultipleClicks_ExecutesMultipleTimes()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.BasicNoParam(vm, view);

        view.SaveButton.PerformClick();
        view.SaveButton.PerformClick();
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(ExpectedClickCount);
    }

    /// <summary>Verifies that setting command then null, then clicking does not throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_CommandSetToNull_ClickDoesNotThrow()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.BasicNoParam(vm, view);

        vm.Save = null;
        await Assert.That((Action)view.SaveButton.PerformClick).ThrowsNothing();
        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Verifies that pushing a new value to the observable parameter is used on click.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableParam_ParameterUpdates_UsesLatestValue()
    {
        var vm = new SharedScenarios.BindCommand.ObservableParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.ObservableParam.MyView();
        var command = new TrackingCommand();
        var paramSubject = new System.Reactive.Subjects.BehaviorSubject<string>("first");
        vm.Save = command;

        using var binding = BindCommandScenarios.ObservableParam(vm, view, paramSubject);

        paramSubject.OnNext("latest");
        view.SaveButton.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo("latest");
    }

    /// <summary>Verifies that a custom event fires and the command executes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CustomEvent_ExecutesOnCustomEvent()
    {
        var vm = new SharedScenarios.BindCommand.CustomEvent.MyViewModel();
        var view = new SharedScenarios.BindCommand.CustomEvent.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.CustomEvent(vm, view);

        view.SaveButton.PerformMouseUp();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that a deep command path (Child.SaveCommand) executes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepCommandPath_CommandExecutes()
    {
        var child = new SharedScenarios.BindCommand.DeepCommandPath.ChildViewModel();
        var vm = new SharedScenarios.BindCommand.DeepCommandPath.MyViewModel { Child = child };
        var view = new SharedScenarios.BindCommand.DeepCommandPath.MyView();
        var command = new TrackingCommand();
        child.SaveCommand = command;

        using var binding = BindCommandScenarios.DeepCommandPath(vm, view);

        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that when the child is replaced, the new child's command is wired.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepCommandPath_ChildReplaced_NewCommandExecutes()
    {
        var oldChild = new SharedScenarios.BindCommand.DeepCommandPath.ChildViewModel();
        var vm = new SharedScenarios.BindCommand.DeepCommandPath.MyViewModel { Child = oldChild };
        var view = new SharedScenarios.BindCommand.DeepCommandPath.MyView();
        var oldCommand = new TrackingCommand();
        oldChild.SaveCommand = oldCommand;

        using var binding = BindCommandScenarios.DeepCommandPath(vm, view);

        var newChild = new SharedScenarios.BindCommand.DeepCommandPath.ChildViewModel();
        var newCommand = new TrackingCommand();
        newChild.SaveCommand = newCommand;
        vm.Child = newChild;

        view.SaveButton.PerformClick();

        await Assert.That(newCommand.ExecuteCount).IsEqualTo(1);
        await Assert.That(oldCommand.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Verifies that disposing twice does not throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam_DoubleDispose_DoesNotThrow()
    {
        var vm = new SharedScenarios.BindCommand.BasicNoParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.BasicNoParam.MyView();
        vm.Save = new TrackingCommand();

        var binding = BindCommandScenarios.BasicNoParam(vm, view);
        binding.Dispose();

        await Assert.That((Action)binding.Dispose).ThrowsNothing();
    }

    // ── EventEnabled (Click + Enabled, no Command property) ─────────────
    /// <summary>Verifies that clicking executes the command via event+Enabled binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabled_ClickExecutesCommand()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabled.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabled.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.EventEnabled(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that the Enabled property is synchronized with CanExecute.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabled_EnabledSyncedWithCanExecute()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabled.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabled.MyView();
        vm.Save = new TrackingCommand { CanExecuteResult = false };

        using var binding = BindCommandScenarios.EventEnabled(vm, view);

        await Assert.That(view.SaveButton.Enabled).IsFalse();
    }

    /// <summary>Verifies that the Enabled property becomes false when the command is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabled_NullCommand_DisablesControl()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabled.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabled.MyView();

        using var binding = BindCommandScenarios.EventEnabled(vm, view);

        await Assert.That(view.SaveButton.Enabled).IsFalse();
    }

    /// <summary>Verifies that disposing the binding stops command execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabled_Dispose_StopsExecution()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabled.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabled.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        var binding = BindCommandScenarios.EventEnabled(vm, view);
        view.SaveButton.PerformClick();
        await Assert.That(command.ExecuteCount).IsEqualTo(1);

        binding.Dispose();
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies that CanExecute=false prevents execution via event+Enabled binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabled_CanExecuteFalse_DoesNotExecute()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabled.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabled.MyView();
        var command = new TrackingCommand { CanExecuteResult = false };
        vm.Save = command;

        using var binding = BindCommandScenarios.EventEnabled(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Verifies event+Enabled binding with an expression parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledExprParam_PassesParameter()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabledExprParam.MyViewModel { CurrentItem = TestItem };
        var view = new SharedScenarios.BindCommand.EventEnabledExprParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.EventEnabledExprParam(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo(TestItem);
    }

    /// <summary>Verifies event+Enabled binding with an observable parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledObsParam_PassesParameter()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabledObsParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabledObsParam.MyView();
        var command = new TrackingCommand();
        var paramSubject = new System.Reactive.Subjects.BehaviorSubject<string>(ObsParam);
        vm.Save = command;

        using var binding = BindCommandScenarios.EventEnabledObsParam(vm, view, paramSubject);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo(ObsParam);
    }

    /// <summary>Verifies event+Enabled binding with observable parameter updates reactively.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledObsParam_ParameterUpdates()
    {
        var vm = new SharedScenarios.BindCommand.EventEnabledObsParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.EventEnabledObsParam.MyView();
        var command = new TrackingCommand();
        var paramSubject = new System.Reactive.Subjects.BehaviorSubject<string>("initial");
        vm.Save = command;

        using var binding = BindCommandScenarios.EventEnabledObsParam(vm, view, paramSubject);

        paramSubject.OnNext(ReplacementValue);
        view.SaveButton.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo(ReplacementValue);
    }

    // ── CommandProperty (Command + CommandParameter, no event) ──────────
    /// <summary>Verifies that binding sets the Command property on the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_SetsCommandOnControl()
    {
        var vm = new SharedScenarios.BindCommand.CommandProperty.MyViewModel();
        var view = new SharedScenarios.BindCommand.CommandProperty.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.CommandProperty(vm, view);

        await Assert.That(view.SaveButton.Command).IsEqualTo(command);
    }

    /// <summary>Verifies that setting the command after binding updates the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_CommandSetAfterBinding_UpdatesControl()
    {
        var vm = new SharedScenarios.BindCommand.CommandProperty.MyViewModel();
        var view = new SharedScenarios.BindCommand.CommandProperty.MyView();
        var command = new TrackingCommand();

        using var binding = BindCommandScenarios.CommandProperty(vm, view);

        vm.Save = command;

        await Assert.That(view.SaveButton.Command).IsEqualTo(command);
    }

    /// <summary>Verifies that changing the command updates the control's Command property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_CommandChanges_ControlTakesTheReplacement()
    {
        var vm = new SharedScenarios.BindCommand.CommandProperty.MyViewModel();
        var view = new SharedScenarios.BindCommand.CommandProperty.MyView();
        var first = new TrackingCommand();
        var second = new TrackingCommand();
        vm.Save = first;

        using var binding = BindCommandScenarios.CommandProperty(vm, view);
        await Assert.That(view.SaveButton.Command).IsEqualTo(first);

        vm.Save = second;
        await Assert.That(view.SaveButton.Command).IsEqualTo(second);
    }

    /// <summary>Verifies that null command results in null on the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_NullCommand_SetsControlCommandNull()
    {
        var vm = new SharedScenarios.BindCommand.CommandProperty.MyViewModel();
        var view = new SharedScenarios.BindCommand.CommandProperty.MyView();

        using var binding = BindCommandScenarios.CommandProperty(vm, view);

        await Assert.That(view.SaveButton.Command).IsNull();
    }

    /// <summary>Verifies that expression parameter sets CommandParameter on the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyExprParam_SetsCommandParameter()
    {
        var vm = new SharedScenarios.BindCommand.CommandPropertyExprParam.MyViewModel { CurrentItem = TestItem };
        var view = new SharedScenarios.BindCommand.CommandPropertyExprParam.MyView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = BindCommandScenarios.CommandPropertyExprParam(vm, view);

        await Assert.That(view.SaveButton.Command).IsEqualTo(command);
        await Assert.That(view.SaveButton.CommandParameter).IsEqualTo(TestItem);
    }

    /// <summary>The control follows the parameter property, not the value it held when the command arrived.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyExprParam_ParameterPropertyChanges_UpdatesCommandParameter()
    {
        var vm = new SharedScenarios.BindCommand.CommandPropertyExprParam.MyViewModel { CurrentItem = TestItem };
        var view = new SharedScenarios.BindCommand.CommandPropertyExprParam.MyView();
        vm.Save = new TrackingCommand();

        using var binding = BindCommandScenarios.CommandPropertyExprParam(vm, view);

        vm.CurrentItem = ReplacementValue;

        await Assert.That(view.SaveButton.CommandParameter).IsEqualTo(ReplacementValue);
    }

    /// <summary>Verifies that observable parameter sets CommandParameter on the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyObsParam_SetsCommandParameter()
    {
        var vm = new SharedScenarios.BindCommand.CommandPropertyObsParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.CommandPropertyObsParam.MyView();
        var command = new TrackingCommand();
        var paramSubject = new System.Reactive.Subjects.BehaviorSubject<string>(ObsParam);
        vm.Save = command;

        using var binding = BindCommandScenarios.CommandPropertyObsParam(vm, view, paramSubject);

        await Assert.That(view.SaveButton.Command).IsEqualTo(command);
        await Assert.That(view.SaveButton.CommandParameter).IsEqualTo(ObsParam);
    }

    /// <summary>Verifies that the observable parameter updates CommandParameter reactively.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyObsParam_ParameterUpdates()
    {
        var vm = new SharedScenarios.BindCommand.CommandPropertyObsParam.MyViewModel();
        var view = new SharedScenarios.BindCommand.CommandPropertyObsParam.MyView();
        var command = new TrackingCommand();
        var paramSubject = new System.Reactive.Subjects.BehaviorSubject<string>("initial");
        vm.Save = command;

        using var binding = BindCommandScenarios.CommandPropertyObsParam(vm, view, paramSubject);

        paramSubject.OnNext(ReplacementValue);

        await Assert.That(view.SaveButton.CommandParameter).IsEqualTo(ReplacementValue);
    }

    /// <summary>A simple <see cref="ICommand"/> implementation that tracks invocations for testing.</summary>
    private sealed class TrackingCommand : ICommand
    {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { /* CanExecute never changes for this command. */ }
            remove { /* CanExecute never changes for this command. */ }
        }

        /// <summary>Gets or sets a value indicating whether <see cref="CanExecute"/> returns <see langword="true"/>.</summary>
        public bool CanExecuteResult { get; set; } = true;

        /// <summary>Gets the number of times <see cref="Execute"/> has been called.</summary>
        public int ExecuteCount { get; private set; }

        /// <summary>Gets the parameter passed to the most recent <see cref="Execute"/> call.</summary>
        public object? LastParameter { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter) => CanExecuteResult;

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }
    }
}
