// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Covers the interaction registration a call site the generator could not read falls back to.</summary>
public class RuntimeInteractionFallbackTests
{
    /// <summary>The expression text reported when the observation faults.</summary>
    private const string BindingExpression = "x => x.Confirm";

    /// <summary>The registrations made after the interaction is replaced once: the original and the replacement.</summary>
    private const int RegistrationsAfterReplacement = 2;

    /// <summary>A null view model holds no interaction, so the handler is never registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WithNoViewModel_RegistersNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var registrations = 0;

        using var binding = RuntimeInteractionFallback.BindInteraction<DispatchStubViewModel, string, bool>(
            null,
            x => x.Confirm,
            _ =>
            {
                registrations++;
                return new UnregisteredHandler();
            },
            BindingExpression);

        await Assert.That(registrations).IsEqualTo(0);
    }

    /// <summary>A property holding no interaction registers nothing, and does not fault the observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WhenThePropertyHoldsNoInteraction_RegistersNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var registrations = 0;
        var viewModel = new DispatchStubViewModel { Confirm = null! };

        using var binding = RuntimeInteractionFallback.BindInteraction(
            viewModel,
            x => x.Confirm,
            _ =>
            {
                registrations++;
                return new UnregisteredHandler();
            },
            BindingExpression);

        await Assert.That(registrations).IsEqualTo(0);
    }

    /// <summary>Replacing the interaction disposes the registration made on the one it displaced.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WhenTheInteractionIsReplaced_DisposesTheDisplacedRegistration()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        List<TrackedHandler> handlers = [];
        var viewModel = new NotifyingViewModel { Confirm = new Interaction<string, bool>() };

        using var binding = RuntimeInteractionFallback.BindInteraction(
            viewModel,
            x => x.Confirm,
            _ =>
            {
                var handler = new TrackedHandler();
                handlers.Add(handler);
                return handler;
            },
            BindingExpression);

        viewModel.Confirm = new Interaction<string, bool>();

        await Assert.That(handlers.Count).IsEqualTo(RegistrationsAfterReplacement);
        await Assert.That(handlers[0].Disposals).IsEqualTo(1);
    }

    /// <summary>A view model that raises a change notification when its interaction is replaced.</summary>
    private sealed class NotifyingViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the interaction a binding names.</summary>
        public IInteraction<string, bool> Confirm
        {
            get => field;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Confirm)));
            }
        } = null!;
    }

    /// <summary>Counts how often it is disposed.</summary>
    private sealed class TrackedHandler : IDisposable
    {
        /// <summary>Gets how many times the handler has been disposed.</summary>
        public int Disposals { get; private set; }

        /// <inheritdoc/>
        public void Dispose() => Disposals++;
    }

    /// <summary>Stands in for the registration a handler would hand back.</summary>
    private sealed class UnregisteredHandler : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
