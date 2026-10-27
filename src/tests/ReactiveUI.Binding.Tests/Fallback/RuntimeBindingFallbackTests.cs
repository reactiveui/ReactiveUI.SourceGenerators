// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Tests for the <see cref="RuntimeBindingFallback"/> class.</summary>
public class RuntimeBindingFallbackTests
{
    /// <summary>The value the source starts with.</summary>
    private const string InitialValue = "Initial";

    /// <summary>The value assigned to trigger a notification.</summary>
    private const string NotifyingValue = "Changed";

    /// <summary>The age the source starts with, used by the converting binding.</summary>
    private const int InitialAge = 30;

    /// <summary>The age assigned to trigger a notification on the converting binding.</summary>
    private const int ChangedAge = 41;

    /// <summary>The expression text reported when a write faults.</summary>
    private const string BindingExpression = "x => x.Name";

    /// <summary>The age written back from the target side of a two-way binding.</summary>
    private const int ReverseEditedAge = 55;

    /// <summary>The text form of <see cref="ReverseEditedAge"/>, as the target side holds it.</summary>
    private const string ReverseEditedAgeText = "55";

    /// <summary>Verifies that the binding writes the initial value and every later change onto the target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_SameType_WritesInitialValueAndChanges()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Name = InitialValue };
        var target = new TestViewModel();

        using var binding = RuntimeBindingFallback.BindOneWay(
            source,
            target,
            x => x.Name,
            x => x.Name,
            null,
            BindingExpression);

        var afterBind = target.Name;
        source.Name = NotifyingValue;

        await Assert.That(afterBind).IsEqualTo(InitialValue);
        await Assert.That(target.Name).IsEqualTo(NotifyingValue);
    }

    /// <summary>Verifies that the binding stops writing once it is disposed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_AfterDispose_StopsWriting()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Name = InitialValue };
        var target = new TestViewModel();

        var binding = RuntimeBindingFallback.BindOneWay(
            source,
            target,
            x => x.Name,
            x => x.Name,
            null,
            BindingExpression);

        binding.Dispose();
        source.Name = NotifyingValue;

        await Assert.That(target.Name).IsEqualTo(InitialValue);
    }

    /// <summary>Verifies that a conversion is applied to every value written to the target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithConversion_WritesConvertedValue()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Age = InitialAge };
        var target = new TestViewModel();

        using var binding = RuntimeBindingFallback.BindOneWay(
            source,
            target,
            x => x.Age,
            x => x.Name,
            static age => age.ToString(System.Globalization.CultureInfo.InvariantCulture),
            null,
            BindingExpression);

        var afterBind = target.Name;
        source.Age = ChangedAge;

        await Assert.That(afterBind).IsEqualTo("30");
        await Assert.That(target.Name).IsEqualTo("41");
    }

    /// <summary>Verifies that an explicit sequencer is used to deliver the write.</summary>
    /// <param name="immediate">Whether to use the immediate scheduler or queue on the current thread.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task BindOneWay_WithSequencer_WritesTarget(bool immediate)
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Name = InitialValue };
        var target = new TestViewModel();

        using var binding = RuntimeBindingFallback.BindOneWay(
            source,
            target,
            x => x.Name,
            x => x.Name,
            immediate ? Sequencer.Immediate : Sequencer.CurrentThread,
            BindingExpression);

        source.Name = NotifyingValue;

        await Assert.That(target.Name).IsEqualTo(NotifyingValue);
    }

    /// <summary>Verifies that a null target is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_NullTarget_ThrowsArgumentNullException()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Name = InitialValue };

        var action = () => RuntimeBindingFallback.BindOneWay(
            source,
            (TestViewModel)null!,
            x => x.Name,
            x => x.Name,
            null,
            BindingExpression);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that a null target property expression is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_NullTargetProperty_ThrowsArgumentNullException()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Name = InitialValue };
        var target = new TestViewModel();

        var action = () => RuntimeBindingFallback.BindOneWay(
            source,
            target,
            x => x.Name,
            null!,
            null,
            BindingExpression);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that a null conversion is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_NullConversion_ThrowsArgumentNullException()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Age = InitialAge };
        var target = new TestViewModel();

        var action = () => RuntimeBindingFallback.BindOneWay(
            source,
            target,
            x => x.Age,
            x => x.Name,
            (Func<int, string>)null!,
            null,
            BindingExpression);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that a two-way binding carries edits in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_SameType_CarriesBothDirections()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Name = InitialValue };
        var target = new TestViewModel();

        using var binding = RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            x => x.Name,
            x => x.Name,
            null,
            BindingExpression);

        var afterBind = target.Name;
        source.Name = NotifyingValue;
        var afterSourceEdit = target.Name;
        target.Name = "FromTarget";

        await Assert.That(afterBind).IsEqualTo(InitialValue);
        await Assert.That(afterSourceEdit).IsEqualTo(NotifyingValue);
        await Assert.That(source.Name).IsEqualTo("FromTarget");
    }

    /// <summary>Verifies that a two-way binding converts in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_WithConverters_ConvertsBothDirections()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Age = InitialAge };
        var target = new TestViewModel();

        using var binding = RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            x => x.Age,
            x => x.Name,
            TwoWayConverters.Create<int, string>(
                static age => age.ToString(System.Globalization.CultureInfo.InvariantCulture),
                static text => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture)),
            null,
            BindingExpression);

        var afterBind = target.Name;
        target.Name = ReverseEditedAgeText;

        await Assert.That(afterBind).IsEqualTo("30");
        await Assert.That(source.Age).IsEqualTo(ReverseEditedAge);
    }

    /// <summary>Verifies that a null converter pair is rejected by the two-way binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_NullConverters_ThrowsArgumentNullException()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new TestViewModel { Age = InitialAge };
        var target = new TestViewModel();

        var action = () => RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            x => x.Age,
            x => x.Name,
            (TwoWayConverterPair<int, string>)null!,
            null,
            BindingExpression);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the view-first one-way binding writes the view and reports its direction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_SameType_WritesViewAndReportsDirection()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var viewModel = new TestViewModel { Name = InitialValue };
        var view = new BindingTestView();

        using var binding = RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            x => x.Name,
            x => x.Caption,
            null,
            BindingExpression);

        viewModel.Name = NotifyingValue;

        await Assert.That(view.Caption).IsEqualTo(NotifyingValue);
        await Assert.That(binding.Direction).IsEqualTo(BindingDirection.OneWay);
        await Assert.That(binding.View).IsSameReferenceAs(view);
    }

    /// <summary>Verifies that the view-first one-way binding applies its conversion.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_WithConversion_WritesConvertedView()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var viewModel = new TestViewModel { Age = InitialAge };
        var view = new BindingTestView();

        using var binding = RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            x => x.Age,
            x => x.Caption,
            static age => age.ToString(System.Globalization.CultureInfo.InvariantCulture),
            null,
            BindingExpression);

        viewModel.Age = ChangedAge;

        await Assert.That(view.Caption).IsEqualTo("41");
    }

    /// <summary>Verifies that a null conversion is rejected by the view-first one-way binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_NullConversion_ThrowsArgumentNullException()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var viewModel = new TestViewModel { Age = InitialAge };
        var view = new BindingTestView();

        var action = () => RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            x => x.Age,
            x => x.Caption,
            (Func<int, string>)null!,
            null,
            BindingExpression);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the view-first two-way binding carries both directions and tags which side moved.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SameType_CarriesBothDirectionsAndTagsChanges()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var viewModel = new TestViewModel { Name = InitialValue };
        var view = new BindingTestView();
        var changes = new List<BindingChange>();

        using var binding = RuntimeBindingFallback.Bind(
            view,
            viewModel,
            x => x.Name,
            x => x.Caption,
            null,
            BindingExpression);

        using var changeSubscription = binding.Changed.Subscribe(changes.Add);

        viewModel.Name = NotifyingValue;
        var afterViewModelEdit = view.Caption;
        view.Caption = "FromView";

        await Assert.That(afterViewModelEdit).IsEqualTo(NotifyingValue);
        await Assert.That(viewModel.Name).IsEqualTo("FromView");
        await Assert.That(binding.Direction).IsEqualTo(BindingDirection.TwoWay);
        await Assert.That(changes.Exists(static c => c.FromViewModel)).IsTrue();
        await Assert.That(changes.Exists(static c => !c.FromViewModel)).IsTrue();
    }

    /// <summary>Verifies that the view-first two-way binding converts in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_WithConverters_ConvertsBothDirections()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var viewModel = new TestViewModel { Age = InitialAge };
        var view = new BindingTestView();

        using var binding = RuntimeBindingFallback.Bind(
            view,
            viewModel,
            x => x.Age,
            x => x.Caption,
            TwoWayConverters.Create<int, string>(
                static age => age.ToString(System.Globalization.CultureInfo.InvariantCulture),
                static text => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture)),
            null,
            BindingExpression);

        var afterBind = view.Caption;
        view.Caption = ReverseEditedAgeText;

        await Assert.That(afterBind).IsEqualTo("30");
        await Assert.That(viewModel.Age).IsEqualTo(ReverseEditedAge);
    }

    /// <summary>Verifies that a null converter pair is rejected by the view-first two-way binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_NullConverters_ThrowsArgumentNullException()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var viewModel = new TestViewModel { Age = InitialAge };
        var view = new BindingTestView();

        var action = () => RuntimeBindingFallback.Bind(
            view,
            viewModel,
            x => x.Age,
            x => x.Caption,
            (TwoWayConverterPair<int, string>)null!,
            null,
            BindingExpression);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that a null target leaves the source unsubscribed rather than faulting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_NullTarget_LeavesTheSourceUnsubscribed()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var source = new ManualObservable<string>();

        using var binding = RuntimeBindingFallback.BindTo<string, TestViewModel, string>(
            source,
            null,
            x => x.Name,
            null,
            null,
            null,
            BindingExpression);

        await Assert.That(source.Observer).IsNull();
    }

    /// <summary>A view with one observable property, for the view-first binding tests.</summary>
    private sealed class BindingTestView : IViewFor, INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public object? ViewModel { get; set; }

        /// <summary>Gets or sets the bound caption.</summary>
        public string Caption
        {
            get;
            set
            {
                if (string.Equals(field, value, StringComparison.Ordinal))
                {
                    return;
                }

                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Caption)));
            }
        }

        = string.Empty;
    }
}
