// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>
/// The runtime view-first bindings follow the view model the view holds, as the generated ones do: a binding made
/// before the view has a view model waits for one, and replacing the view model moves the binding to the new one.
/// </summary>
[NotInParallel]
public class ViewModelRootedUnsafeBindingTests
{
    /// <summary>The first value carried across a binding.</summary>
    private const string FirstValue = "first";

    /// <summary>The value the replacement view model carries.</summary>
    private const string SecondValue = "second";

    /// <summary>The value typed into the view.</summary>
    private const string EditedValue = "edited";

    /// <summary>A one-way binding made while the view has no view model waits for one, then carries its value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBindUnsafe_NullViewModel_BindsOnceTheViewHasOne()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var view = new TypedStubView();

        using var binding = view.OneWayBindUnsafe((DispatchStubViewModel?)null, static vm => vm.Caption, static v => v.Caption);
        var beforeViewModel = view.Caption;

        view.ViewModel = new() { Caption = FirstValue };

        using (Assert.Multiple())
        {
            await Assert.That(beforeViewModel).IsEqualTo(TypedStubView.InitialCaption);
            await Assert.That(view.Caption).IsEqualTo(FirstValue);
        }
    }

    /// <summary>A two-way binding moves to a replacement view model in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindUnsafe_ViewModelReplaced_FollowsTheNewViewModel()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var first = new DispatchStubViewModel { Caption = FirstValue };
        var second = new DispatchStubViewModel { Caption = SecondValue };
        var view = new TypedStubView { ViewModel = first };

        using var binding = view.BindUnsafe(first, static vm => vm.Caption, static v => v.Caption);
        view.ViewModel = second;
        var afterReplacement = view.Caption;
        view.Caption = EditedValue;

        using (Assert.Multiple())
        {
            await Assert.That(afterReplacement).IsEqualTo(SecondValue);
            await Assert.That(second.Caption).IsEqualTo(EditedValue);
            await Assert.That(first.Caption).IsEqualTo(FirstValue);
        }
    }

    /// <summary>A triggered two-way binding writes back to the view model the view holds at the time.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindUnsafe_WithSignal_WritesToTheCurrentViewModel()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var first = new DispatchStubViewModel { Caption = FirstValue };
        var second = new DispatchStubViewModel { Caption = SecondValue };
        var view = new TypedStubView { ViewModel = first };
        var signal = new Primitives.Signals.Signal<bool>();

        using var binding = view.BindUnsafe(first, static vm => vm.Caption, static v => v.Caption, signal);
        view.ViewModel = second;
        view.Caption = EditedValue;
        signal.OnNext(true);

        using (Assert.Multiple())
        {
            await Assert.That(second.Caption).IsEqualTo(EditedValue);
            await Assert.That(first.Caption).IsEqualTo(FirstValue);
        }
    }

    /// <summary>A view that exposes a typed, notifying view model.</summary>
    internal sealed class TypedStubView : IViewFor<DispatchStubViewModel>, INotifyPropertyChanged
    {
        /// <summary>The caption before any binding writes it.</summary>
        internal const string InitialCaption = "initial";

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public DispatchStubViewModel? ViewModel
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(ViewModel)));
            }
        }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DispatchStubViewModel?)value;
        }

        /// <summary>Gets or sets the caption.</summary>
        public string Caption
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Caption)));
            }
        } = InitialCaption;
    }
}
