// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Covers the command binding a call site the generator could not read falls back to.</summary>
public class RuntimeCommandBindingFallbackTests
{
    /// <summary>The expression text reported when the observation faults.</summary>
    private const string BindingExpression = "x => x.Run";

    /// <summary>A null view model holds no command to bind, so the parameter stream stays unsubscribed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithNoViewModel_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var parameters = new ManualObservable<object?>();

        using var binding = RuntimeCommandBindingFallback.BindCommand<
            DispatchStubView,
            DispatchStubViewModel,
            ICommand,
            DispatchStubControl>(
            new(),
            null,
            x => x.Run,
            x => x.Control,
            parameters,
            null,
            BindingExpression);

        await Assert.That(parameters.Observer).IsNull();
    }

    /// <summary>A control no registered binder reaches leaves the command unbound rather than faulting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithAControlNoBinderReaches_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        var viewModel = new DispatchStubViewModel { Run = command };

        using var binding = RuntimeCommandBindingFallback.BindCommand<
            DispatchStubView,
            DispatchStubViewModel,
            ICommand,
            UnclaimedStubControl>(
            new(),
            viewModel,
            x => x.Run,
            x => x.Surface,
            new ManualObservable<object?>(),
            null,
            BindingExpression);

        await Assert.That(command.LastParameter).IsNull();
    }

    /// <summary>Naming an event does not find a binder for a control no binder reaches either.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithANamedEventAndAControlNoBinderReaches_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        var viewModel = new DispatchStubViewModel { Run = command };

        using var binding = RuntimeCommandBindingFallback.BindCommand<
            DispatchStubView,
            DispatchStubViewModel,
            ICommand,
            UnclaimedStubControl>(
            new(),
            viewModel,
            x => x.Run,
            x => x.Surface,
            new ManualObservable<object?>(),
            nameof(DispatchStubControl.Click),
            BindingExpression);

        await Assert.That(command.LastParameter).IsNull();
    }

    /// <summary>A change notification on a link of the control chain rebinds without leaving the old binding attached.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WhenAChainLinkRaisesChanged_KeepsOneActiveBinding()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        var viewModel = new DispatchStubViewModel { Run = command };
        var view = new ChainedView();

        using var binding = RuntimeCommandBindingFallback.BindCommand(
            view,
            viewModel,
            x => x.Run,
            x => x.Editor.Properties.Button,
            new ManualObservable<object?>(),
            null,
            BindingExpression);

        view.Editor.RaisePropertiesChanged();
        view.Editor.RaisePropertiesChanged();
        view.Editor.Properties.Button.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Disposing the binding detaches the command even after a link rebound it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WhenDisposedAfterARebind_DetachesTheCommand()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        var viewModel = new DispatchStubViewModel { Run = command };
        var view = new ChainedView();

        var binding = RuntimeCommandBindingFallback.BindCommand(
            view,
            viewModel,
            x => x.Run,
            x => x.Editor.Properties.Button,
            new ManualObservable<object?>(),
            null,
            BindingExpression);

        view.Editor.RaisePropertiesChanged();
        view.Editor.RaisePropertiesChanged();
        binding.Dispose();
        view.Editor.Properties.Button.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>A view whose control sits behind a notifying chain.</summary>
    private sealed class ChainedView : IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }

        /// <summary>Gets the editor holding the control.</summary>
        public ChainedEditor Editor { get; } = new();
    }

    /// <summary>An editor that reports its properties changed without replacing them.</summary>
    private sealed class ChainedEditor : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets the properties holding the control.</summary>
        public ChainedProperties Properties { get; } = new();

        /// <summary>Raises a change notification for <see cref="Properties"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertiesChanged() => PropertyChanged?.Invoke(this, new(nameof(Properties)));
    }

    /// <summary>Holds the control a command binds to.</summary>
    private sealed class ChainedProperties
    {
        /// <summary>Gets the control.</summary>
        public DispatchStubControl Button { get; } = new();
    }
}
