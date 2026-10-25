// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Tests.TestExecutors;
using ReactiveUI.Binding.Tests.TestModels;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Tests the signal-driven Unsafe binding surface.</summary>
[TestExecutor<BindingBuilderTestExecutor>]
public class TriggerUpdateUnsafeBindingTests
{
    /// <summary>The view model's initial value.</summary>
    private const string ModelValue = "model";

    /// <summary>The view's initial value.</summary>
    private const string ViewValue = "view";

    /// <summary>A user edit on the view side.</summary>
    private const string EditedValue = "edited";

    /// <summary>The view-first API exposes both converter and registry-based signal overloads.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ExposesBothSignalOverloads()
    {
        const int signalOverloadTypeParameters = 5;
        const int expectedOverloads = 2;
        var overloads = typeof(ReactiveUIBindingExtensions).GetMethods()
            .Where(static method => method.Name == "BindUnsafe" && method.GetGenericArguments().Length == signalOverloadTypeParameters);

        await Assert.That(overloads).Count().IsEqualTo(expectedOverloads);
    }

    /// <summary>Every wiring initializes the view from the view model.</summary>
    /// <param name="direction">The stream's update direction.</param>
    /// <param name="hasStream">Whether a stream is supplied.</param>
    /// <param name="converters">Whether explicit conversion delegates are supplied.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task BindUnsafe_AllWirings_InitializesFromViewModel(
        [Matrix(TriggerUpdate.ViewToViewModel, TriggerUpdate.ViewModelToView)] TriggerUpdate direction,
        [Matrix(false, true)] bool hasStream,
        [Matrix(false, true)] bool converters)
    {
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        using var signal = new Subject<int>();
        IObservable<int>? updates = hasStream ? signal : null;
        using var binding = converters
            ? view.BindUnsafe(model, x => x.Caption, x => x.Caption, static x => x, static x => x, updates, direction)
            : view.BindUnsafe(model, x => x.Caption, x => x.Caption, updates, direction);

        await Assert.That(view.Caption).IsEqualTo(ModelValue);
        await Assert.That(model.Caption).IsEqualTo(ModelValue);
        await Assert.That(binding.Direction).IsEqualTo(BindingDirection.TwoWay);
    }

    /// <summary>The default signal direction replaces the view's own notifications.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ViewSignal_ReplacesViewNotifications()
    {
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        using var signal = new Subject<int>();
        using var binding = view.BindUnsafe(model, x => x.Caption, x => x.Caption, signal);
        var changes = new List<BindingChange>();
        using var observer = binding.Changed.Subscribe(changes.Add);

        view.Caption = EditedValue;
        await Assert.That(model.Caption).IsEqualTo(ModelValue);
        signal.OnNext(0);

        await Assert.That(model.Caption).IsEqualTo(EditedValue);
        await Assert.That(changes).Count().IsEqualTo(1);
        await Assert.That(changes[0].FromViewModel).IsFalse();
        await Assert.That(changes[0].Value).IsEqualTo(EditedValue);
    }

    /// <summary>A model-directed stream drives later model writes while view notifications participate.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ModelSignal_RequiresSignalAfterInitialModelNotification()
    {
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        using var signal = new Subject<int>();
        using var binding = view.BindUnsafe(model, x => x.Caption, x => x.Caption, signal, TriggerUpdate.ViewModelToView);
        var changes = new List<BindingChange>();
        using var observer = binding.Changed.Subscribe(changes.Add);

        model.Caption = "first";
        model.Caption = "second";
        await Assert.That(view.Caption).IsEqualTo(ModelValue);
        signal.OnNext(0);
        await Assert.That(view.Caption).IsEqualTo("second");
        await Assert.That(changes).Count().IsEqualTo(1);
        await Assert.That(changes[0].FromViewModel).IsTrue();

        view.Caption = EditedValue;
        await Assert.That(model.Caption).IsEqualTo(EditedValue);
    }

    /// <summary>With no supplied stream, property notifications drive both directions.</summary>
    /// <param name="direction">The selected trigger direction.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(TriggerUpdate.ViewToViewModel)]
    [Arguments(TriggerUpdate.ViewModelToView)]
    public async Task BindUnsafe_WithoutSignal_ObservesBothProperties(TriggerUpdate direction)
    {
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        using var binding = view.BindUnsafe(model, x => x.Caption, x => x.Caption, (IObservable<int>?)null, direction);

        model.Caption = "forward";
        await Assert.That(view.Caption).IsEqualTo("forward");
        view.Caption = EditedValue;
        await Assert.That(model.Caption).IsEqualTo(EditedValue);
    }

    /// <summary>A dispatched signal reads the source's value when the view thread delivers it.</summary>
    /// <param name="direction">The stream's update direction.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(TriggerUpdate.ViewToViewModel)]
    [Arguments(TriggerUpdate.ViewModelToView)]
    public async Task BindUnsafe_DispatchedSignal_ReadsAtDelivery(TriggerUpdate direction)
    {
        const string deliveryValue = "delivery";
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        var invoker = new StubViewThreadInvoker(view) { HasAccess = true };
        Locator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(invoker);
        ViewThreadInvokers.Refresh();
        try
        {
            using var signal = new Subject<int>();
            using var binding = view.BindUnsafe(model, x => x.Caption, x => x.Caption, signal, direction);
            invoker.HasAccess = false;
            if (direction == TriggerUpdate.ViewModelToView)
            {
                model.Caption = "queued";
                signal.OnNext(0);
                model.Caption = deliveryValue;
                await Assert.That(view.Caption).IsEqualTo(ModelValue);
            }
            else
            {
                view.Caption = "queued";
                signal.OnNext(0);
                view.Caption = deliveryValue;
                await Assert.That(model.Caption).IsEqualTo(ModelValue);
            }

            await Assert.That(invoker.PostCount).IsEqualTo(1);
            invoker.RunPosted();
            await Assert.That(view.Caption).IsEqualTo(deliveryValue);
            await Assert.That(model.Caption).IsEqualTo(deliveryValue);
        }
        finally
        {
            ViewThreadInvokers.Refresh();
        }
    }

    /// <summary>A view edit queued before initialization cannot reverse the initial write's direction.</summary>
    /// <param name="direction">The stream's update direction.</param>
    /// <param name="hasStream">Whether a stream is supplied.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task BindUnsafe_ViewEditsBeforeInitialDispatch_InitializesFromViewModel(
        [Matrix(TriggerUpdate.ViewToViewModel, TriggerUpdate.ViewModelToView)] TriggerUpdate direction,
        [Matrix(false, true)] bool hasStream)
    {
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        var invoker = new StubViewThreadInvoker(view);
        Locator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(invoker);
        ViewThreadInvokers.Refresh();
        try
        {
            using var signal = new Subject<int>();
            IObservable<int>? updates = hasStream ? signal : null;
            using var binding = view.BindUnsafe(model, x => x.Caption, x => x.Caption, updates, direction);
            view.Caption = EditedValue;
            signal.OnNext(0);
            invoker.RunPosted();

            await Assert.That(model.Caption).IsEqualTo(ModelValue);
            await Assert.That(view.Caption).IsEqualTo(ModelValue);
        }
        finally
        {
            ViewThreadInvokers.Refresh();
        }
    }

    /// <summary>The equality check compares converted values and avoids an echo write.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ConvertedValueEqualsDestination_SkipsWrite()
    {
        var model = new DispatchStubViewModel { Caption = "same " };
        var view = new DispatchStubView { Caption = "same" };
        var writes = 0;
        view.PropertyChanged += (_, _) => writes++;
        using var binding = view.BindUnsafe(
            model,
            x => x.Caption,
            x => x.Caption,
            static value => value.Trim(),
            static value => $"{value} ",
            (IObservable<int>?)null);
        var changes = new List<BindingChange>();
        using var observer = binding.Changed.Subscribe(changes.Add);

        model.Caption = "same  ";
        await Assert.That(writes).IsEqualTo(0);
        await Assert.That(changes).IsEmpty();
        view.Caption = EditedValue;
        await Assert.That(model.Caption).IsEqualTo($"{EditedValue} ");
        await Assert.That(writes).IsEqualTo(1);
        await Assert.That(changes).Count().IsEqualTo(1);
    }

    /// <summary>Disposal detaches the stream and stops property writes in both directions.</summary>
    /// <param name="direction">The stream's update direction.</param>
    /// <param name="hasStream">Whether a stream is supplied.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task Dispose_AllWirings_StopsBothDirections(
        [Matrix(TriggerUpdate.ViewToViewModel, TriggerUpdate.ViewModelToView)] TriggerUpdate direction,
        [Matrix(false, true)] bool hasStream)
    {
        var model = new DispatchStubViewModel { Caption = ModelValue };
        var view = new DispatchStubView { Caption = ViewValue };
        using var signal = new Subject<int>();
        IObservable<int>? updates = hasStream ? signal : null;
        var binding = view.BindUnsafe(model, x => x.Caption, x => x.Caption, updates, direction);
        var completed = false;
        using var observer = binding.Changed.Subscribe(static _ => { }, () => completed = true);

        binding.Dispose();
        binding.Dispose();
        model.Caption = "after disposal";
        await Assert.That(view.Caption).IsEqualTo(ModelValue);
        view.Caption = EditedValue;
        signal.OnNext(0);
        await Assert.That(model.Caption).IsEqualTo("after disposal");
        await Assert.That(signal.HasObservers).IsFalse();
        await Assert.That(completed).IsTrue();
    }

    /// <summary>A missing model parent suppresses writes until the path becomes available.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ModelParentMissing_ResumesWhenParentAppears()
    {
        var model = new TestViewModel();
        var view = new DispatchStubView { Caption = ViewValue };
        using var binding = view.BindUnsafe(model, x => x.Address!.City, x => x.Caption, (IObservable<int>?)null);
        await Assert.That(view.Caption).IsEqualTo(ViewValue);

        model.Address = new TestAddress { City = ModelValue };
        await Assert.That(view.Caption).IsEqualTo(ModelValue);
        model.Address = null;
        view.Caption = EditedValue;
        await Assert.That(model.Address).IsNull();
        model.Address = new TestAddress { City = ModelValue };
        await Assert.That(view.Caption).IsEqualTo(ModelValue);
    }

    /// <summary>A missing view parent receives model initialization when the view path becomes available.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ViewParentMissing_InitializesWhenParentAppears()
    {
        var model = new TestViewModel { Name = ModelValue };
        var view = new ChainView();
        using var binding = view.BindUnsafe(model, x => x.Name, x => x.Address!.City, (IObservable<int>?)null);
        await Assert.That(view.Address).IsNull();

        view.Address = new TestAddress { City = ViewValue };
        await Assert.That(view.Address.City).IsEqualTo(ModelValue);
        await Assert.That(model.Name).IsEqualTo(ModelValue);
    }

    /// <summary>A type pair with no registered conversion cannot overwrite either property.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindUnsafe_ConversionUnavailable_SkipsBothDirections()
    {
        const int initialAge = 11;
        const int editedAge = 22;
        var address = new TestAddress { City = ModelValue };
        var model = new TestViewModel { Address = address };
        var view = new ChainView { Age = initialAge };
        using var binding = view.BindUnsafe(model, x => x.Address, x => x.Age, (IObservable<int>?)null);
        await Assert.That(view.Age).IsEqualTo(initialAge);

        view.Age = editedAge;
        await Assert.That(model.Address).IsSameReferenceAs(address);
        await Assert.That(view.Age).IsEqualTo(editedAge);
    }

    /// <summary>A view whose bound properties include a replaceable child path.</summary>
    private sealed class ChainView : TestViewModel, IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }
}
