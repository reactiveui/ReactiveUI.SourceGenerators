// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Tests.TestExecutors;
using ReactiveUI.Binding.Tests.TestModels;
using TUnit.Core.Executors;
using ReactiveExtensions = ReactiveUI.Binding.Reactive.ReactiveUIBindingExtensions;
using ReactiveTrigger = ReactiveUI.Binding.Reactive.TriggerUpdate;

namespace ReactiveUI.Binding.Tests.Reactive;

/// <summary>Exercises all signal wirings against the System.Reactive runtime.</summary>
[TestExecutor<ReactiveBindingBuilderTestExecutor>]
public class ReactiveTriggerUpdateUnsafeBindingTests
{
    /// <summary>Both overloads initialize from the model, propagate in the chosen direction, and disconnect.</summary>
    /// <param name="direction">The signal direction.</param>
    /// <param name="hasStream">Whether a stream is supplied.</param>
    /// <param name="converters">Whether explicit converters are supplied.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task BindUnsafe_AllWirings_PropagatesAndDisposes(
        [Matrix(ReactiveTrigger.ViewToViewModel, ReactiveTrigger.ViewModelToView)] ReactiveTrigger direction,
        [Matrix(false, true)] bool hasStream,
        [Matrix(false, true)] bool converters)
    {
        const string initial = "model";
        const string edited = "edited";
        var model = new DispatchStubViewModel { Caption = initial };
        var view = new ReactiveView { Caption = "view" };
        using var signal = new Subject<int>();
        IObservable<int>? updates = hasStream ? signal : null;
        var binding = converters
            ? ReactiveExtensions.BindUnsafe(view, model, x => x.Caption, x => x.Caption, static x => x, static x => x, updates, direction)
            : ReactiveExtensions.BindUnsafe(view, model, x => x.Caption, x => x.Caption, updates, direction);
        using (binding)
        {
            await Assert.That(view.Caption).IsEqualTo(initial);
            if (direction == ReactiveTrigger.ViewToViewModel)
            {
                view.Caption = edited;
            }
            else
            {
                model.Caption = edited;
            }

            signal.OnNext(0);
            await Assert.That(view.Caption).IsEqualTo(edited);
            await Assert.That(model.Caption).IsEqualTo(edited);
        }

        model.Caption = initial;
        signal.OnNext(0);
        await Assert.That(view.Caption).IsEqualTo(edited);
        await Assert.That(signal.HasObservers).IsFalse();
    }

    /// <summary>A view exposing the Reactive runtime's view-model contract.</summary>
    private sealed class ReactiveView : DispatchStubView, global::ReactiveUI.Binding.Reactive.IViewFor;
}
