// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.CommandBinding;

namespace ReactiveUI.Binding.Tests.CommandBinding;

/// <summary>Tests for the <see cref="CommandBinderService"/> class.</summary>
[NotInParallel]
public class CommandBinderServiceTests
{
    /// <summary>The event the named-event form binds.</summary>
    private const string EventName = "Click";

    /// <summary>A generated affinity the recording binder outranks.</summary>
    private const int LowerAffinity = 3;

    /// <summary>The command parameter the tests send.</summary>
    private const int ParameterValue = 7;

    /// <summary>
    /// Verifies that GetBinder returns null when only core services are registered
    /// (no runtime command binders — known patterns handled by source generator).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBinder_CoreServicesOnly_ReturnsNull()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();

        var binder = CommandBinderService.GetBinder<ClickableButton>(false);

        await Assert.That(binder).IsNull();
    }

    /// <summary>
    /// Verifies that GetBinder returns null when no matching binder supports the type.
    /// Uses a fresh resolver to avoid state from other tests.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBinder_NoMatchingBinders_ReturnsNull()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();

        // PlainObject has no default events and no Command/CommandParameter properties
        var binder = CommandBinderService.GetBinder<PlainObject>(false);

        await Assert.That(binder).IsNull();
    }

    /// <summary>Verifies that GetBinder prefers higher affinity when custom binder is registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBinder_CustomBinder_PrefersHigherAffinity()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();

        var customBinder = new HighAffinityBinder();
        _ = builder.WithCommandBinder(customBinder);
        _ = builder.BuildApp();

        var binder = CommandBinderService.GetBinder<ClickableButton>(false);

        await Assert.That(binder).IsNotNull();
        await Assert.That(ReferenceEquals(binder, customBinder)).IsTrue();
    }

    /// <summary>With no binder above the generated affinity, a custom binding is not made, for either form.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCustom_NoBinderAboveTheGeneratedAffinity_ReturnsNull()
    {
        RegisterBinder(new RecordingBinder());
        var commands = new TestModels.ManualObservable<ICommand?>();

        var byDefaultEvent = CommandBinderService.BindCustom<ClickableButton, int>(new(), commands, null, RecordingBinder.Affinity);
        var byNamedEvent = CommandBinderService.BindCustom<ClickableButton, int, EventArgs>(new(), commands, null, RecordingBinder.Affinity, EventName);

        await Assert.That(byDefaultEvent).IsNull();
        await Assert.That(byNamedEvent).IsNull();
        await Assert.That(commands.Observer).IsNull();
    }

    /// <summary>A binder above the generated affinity binds each command, releasing the one it replaces.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCustom_WithAHigherBinder_BindsEachCommandAndReleasesTheLast()
    {
        var binder = new RecordingBinder();
        RegisterBinder(binder);
        var button = new ClickableButton();
        var commands = new TestModels.ManualObservable<ICommand?>();
        var parameters = new TestModels.ManualObservable<int>();
        var first = new TestModels.RecordingStubCommand();

        var binding = CommandBinderService.BindCustom(button, commands, parameters, LowerAffinity);
        commands.Observer!.OnNext(first);
        parameters.Observer!.OnNext(ParameterValue);
        commands.Observer.OnNext(null);

        await Assert.That(binding).IsNotNull();
        await Assert.That(binder.Commands).IsEquivalentTo((ICommand?[])[first, null]);
        await Assert.That(binder.Targets[0]).IsSameReferenceAs(button);
        await Assert.That(binder.EventNames).IsEquivalentTo((string?[])[null, null]);
        await Assert.That(binder.Parameters).IsEquivalentTo((object?[])[ParameterValue]);
        await Assert.That(binder.Released).IsEqualTo(1);
    }

    /// <summary>The named-event form hands the binder the event, and a binding with no parameter supplies none.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCustom_WithANamedEvent_HandsTheBinderTheEvent()
    {
        var binder = new RecordingBinder();
        RegisterBinder(binder);
        var commands = new TestModels.ManualObservable<ICommand?>();
        var command = new TestModels.RecordingStubCommand();

        using var binding = CommandBinderService.BindCustom<ClickableButton, int, EventArgs>(new(), commands, null, LowerAffinity, EventName);
        commands.Observer!.OnNext(command);

        await Assert.That(binder.EventNames).IsEquivalentTo((string?[])[EventName]);
        await Assert.That(binder.Parameters).IsEmpty();
    }

    /// <summary>Disposing a custom binding releases the command it currently binds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCustom_WhenDisposed_ReleasesTheCurrentCommand()
    {
        var binder = new RecordingBinder();
        RegisterBinder(binder);
        var commands = new TestModels.ManualObservable<ICommand?>();

        var binding = CommandBinderService.BindCustom<ClickableButton, int>(new(), commands, null, LowerAffinity);
        commands.Observer!.OnNext(new TestModels.RecordingStubCommand());
        binding!.Dispose();

        await Assert.That(binder.Released).IsEqualTo(1);
    }

    /// <summary>Both forms require the commands.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCustom_NullCommands_Throws()
    {
        await Assert.That(static () => CommandBinderService.BindCustom<ClickableButton, int>(new(), null!, null, LowerAffinity))
            .Throws<ArgumentNullException>();
        await Assert.That(static () => CommandBinderService.BindCustom<ClickableButton, int, EventArgs>(new(), null!, null, LowerAffinity, EventName))
            .Throws<ArgumentNullException>();
    }

    /// <summary>Starts a fresh application with one command binder registered.</summary>
    /// <param name="binder">The binder to register.</param>
    private static void RegisterBinder(ICreatesCommandBinding binder)
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.WithCommandBinder(binder);
        _ = builder.BuildApp();
    }

    /// <summary>A mock control with a Click event for testing event-based command binder selection.</summary>
    private sealed class ClickableButton
    {
#pragma warning disable CS0067
        /// <summary>Occurs when the button is clicked.</summary>
        [SuppressMessage("Design", "SST2407:Event is never raised", Justification = "The binder only needs the event to exist; the test never raises it.")]
        private event EventHandler? Click;
#pragma warning restore CS0067
    }

    /// <summary>A plain object with no events or command properties for testing null binder scenarios.</summary>
    [SuppressMessage("Design", "SST1436:Empty type", Justification = "Having no members is the condition under test.")]
    private sealed class PlainObject;

    /// <summary>A custom command binder with high affinity (10) for testing binder priority selection.</summary>
    private sealed class HighAffinityBinder : ICreatesCommandBinding
    {
        /// <summary>The high affinity score reported by this binder.</summary>
        private const int HighAffinity = 10;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObject<T>(bool hasEventTarget) => HighAffinity;

        /// <inheritdoc/>
        [RequiresUnreferencedCode("Test stub")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
            where T : class => null;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [RequiresUnreferencedCode("Test stub")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            string eventName)
            where T : class => null;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
            where T : class
            where TEventArgs : EventArgs => null;
    }

    /// <summary>A command binder that records what it was asked to bind and what it released.</summary>
    private sealed class RecordingBinder : ICreatesCommandBinding
    {
        /// <summary>The affinity this binder bids.</summary>
        public const int Affinity = 10;

        /// <summary>Gets the commands bound, in order.</summary>
        public List<ICommand?> Commands { get; } = [];

        /// <summary>Gets the controls bound, in order.</summary>
        public List<object?> Targets { get; } = [];

        /// <summary>Gets the event named for each binding, or null for the default event.</summary>
        public List<string?> EventNames { get; } = [];

        /// <summary>Gets the parameter values received.</summary>
        public List<object?> Parameters { get; } = [];

        /// <summary>Gets how many bindings were released.</summary>
        public int Released { get; private set; }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObject<T>(bool hasEventTarget) => Affinity;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
            where T : class => Record(command, target, commandParameter, null);

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            string eventName)
            where T : class => Record(command, target, commandParameter, eventName);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
            where T : class
            where TEventArgs : EventArgs => null;

        /// <summary>Records one binding request, binding nothing for a null command.</summary>
        /// <param name="command">The command to bind.</param>
        /// <param name="target">The control.</param>
        /// <param name="commandParameter">The parameter values.</param>
        /// <param name="eventName">The named event, or null for the default event.</param>
        /// <returns>What releases the binding, or null when there is no command.</returns>
        private Release? Record(ICommand? command, object? target, IObservable<object?> commandParameter, string? eventName)
        {
            Commands.Add(command);
            Targets.Add(target);
            EventNames.Add(eventName);
            return command is null
                ? null
                : new(this, commandParameter.Subscribe(new TestModels.RecordingObserver<object?>(Parameters.Add)));
        }

        /// <summary>Releases one recorded binding.</summary>
        /// <param name="owner">The binder that made the binding.</param>
        /// <param name="parameters">The subscription to the parameter values.</param>
        private sealed class Release(RecordingBinder owner, IDisposable parameters) : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose()
            {
                parameters.Dispose();
                owner.Released++;
            }
        }
    }
}
