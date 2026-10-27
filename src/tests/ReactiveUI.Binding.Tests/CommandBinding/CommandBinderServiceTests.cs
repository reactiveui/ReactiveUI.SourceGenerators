// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.CommandBinding;

namespace ReactiveUI.Binding.Tests.CommandBinding;

/// <summary>Tests for the <see cref="CommandBinderService"/> class.</summary>
public class CommandBinderServiceTests
{
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
}
