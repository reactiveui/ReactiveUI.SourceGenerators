// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// Tests for the WPF observation factory that watches a <see cref="DependencyProperty"/>. Every
/// dependency object here is created and mutated before the first await, because a dependency object
/// has thread affinity and an await may resume the test on another thread.
/// </summary>
/// <remarks>
/// Serialized: these share one static <see cref="DependencyProperty"/>, and the value-changed
/// subscription that <c>DependencyPropertyDescriptor.AddValueChanged</c> installs lives in a
/// process-wide table keyed by that property. Run in parallel they race on it, which shows up as an
/// occasional failure rather than a consistent one.
/// </remarks>
[NotInParallel]
public class DependencyObjectObservableForPropertyTests
{
    /// <summary>The value written to trigger the first change notification.</summary>
    private const string FirstValue = "first";

    /// <summary>The value written after the subscription has been disposed.</summary>
    private const string SecondValue = "second";

    /// <summary>The property name behind the dependency property another type owns.</summary>
    private const string GhostPropertyName = "Ghost";

    /// <summary>Verifies that a dependency-property-backed property scores the WPF affinity.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_DependencyProperty_ScoresAsWpfDependencyObject() =>
        await Assert.That(new DependencyObjectObservableForProperty()
                .GetAffinityForObject(typeof(Fixture), nameof(Fixture.Value), false))
            .IsEqualTo(BindingAffinity.WpfDependencyObject);

    /// <summary>Verifies that a type which is not a dependency object does not match at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_NonDependencyObject_ScoresZero() =>
        await Assert.That(new DependencyObjectObservableForProperty()
                .GetAffinityForObject(typeof(string), nameof(string.Length), false))
            .IsEqualTo(0);

    /// <summary>Verifies that a dependency object without a matching dependency property does not match.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_PropertyWithoutDependencyProperty_ScoresZero() =>
        await Assert.That(new DependencyObjectObservableForProperty()
                .GetAffinityForObject(typeof(Fixture), nameof(Fixture.Plain), false))
            .IsEqualTo(0);

    /// <summary>Verifies that changing the dependency property notifies the subscriber.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_EmitsWhenTheDependencyPropertyChanges()
    {
        var target = new Fixture();
        var recorder = new Recorder();

        using (new DependencyObjectObservableForProperty()
            .GetNotificationForProperty(target, ValueExpression(), nameof(Fixture.Value), false, false)
            .Subscribe(recorder))
        {
            target.Value = FirstValue;
        }

        await Assert.That(recorder.Count).IsEqualTo(1);
        await Assert.That(recorder.Last!.Sender).IsSameReferenceAs(target);
    }

    /// <summary>
    /// Verifies that disposing the subscription unhooks the value-changed handler. This is the teardown
    /// the observation hands back, and without it the descriptor keeps the target alive and emitting.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_StopsEmittingOnceDisposed()
    {
        var target = new Fixture();
        var recorder = new Recorder();

        var subscription = new DependencyObjectObservableForProperty()
            .GetNotificationForProperty(target, ValueExpression(), nameof(Fixture.Value), false, false)
            .Subscribe(recorder);

        target.Value = FirstValue;
        subscription.Dispose();
        target.Value = SecondValue;

        await Assert.That(recorder.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that observing a property with no dependency property behind it is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_PropertyWithoutDependencyProperty_Throws()
    {
        var target = new Fixture();

        await Assert.That(() => new DependencyObjectObservableForProperty()
                .GetNotificationForProperty(target, ValueExpression(), nameof(Fixture.Plain), false, false))
            .Throws<ArgumentException>();
    }

    /// <summary>
    /// A dependency property with no CLR accessor and no attached accessor has no descriptor to hook a
    /// value-changed handler onto, so the observation is rejected rather than silently going quiet.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_UndescribedDependencyProperty_Throws()
    {
        var target = new Undescribed();

        await Assert.That(() => new DependencyObjectObservableForProperty()
                .GetNotificationForProperty(target, ValueExpression(), GhostPropertyName, false, false))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// The same rejection, with the diagnostic write suppressed. Suppression silences the trace the binder
    /// writes; it never turns an unobservable property into an observable one.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_SuppressingWarnings_StillRejectsAnUndescribedProperty()
    {
        var target = new Undescribed();

        await Assert.That(() => new DependencyObjectObservableForProperty()
                .GetNotificationForProperty(target, ValueExpression(), GhostPropertyName, false, true))
            .Throws<InvalidOperationException>();
    }

    /// <summary>Verifies that a null sender is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NullSender_Throws() =>
        await Assert.That(static () => new DependencyObjectObservableForProperty()
                .GetNotificationForProperty(null!, ValueExpression(), nameof(Fixture.Value), false, false))
            .Throws<ArgumentNullException>();

    /// <summary>Builds the member expression the observation carries on its observed changes.</summary>
    /// <returns>The body of an expression selecting <see cref="Fixture.Value"/>.</returns>
    /// <remarks>Fully qualified: <c>System.Windows</c> declares an <c>Expression</c> of its own.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static System.Linq.Expressions.Expression ValueExpression() =>
        ((System.Linq.Expressions.Expression<Func<Fixture, string?>>)(x => x.Value)).Body;

    /// <summary>A minimal dependency object, so the tests do not need a visual tree or an STA thread.</summary>
    private sealed class Fixture : DependencyObject
    {
        /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(Fixture),
            new(default(string)));

        /// <summary>Gets or sets the observed dependency property.</summary>
        public string? Value
        {
            get => (string?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>Gets a plain property with no dependency property behind it.</summary>
        public string Plain => nameof(Plain);
    }

    /// <summary>Registers a dependency property that neither a CLR accessor nor an attached one reaches.</summary>
    private sealed class Undescribed : DependencyObject
    {
        /// <summary>Identifies a dependency property with no property or attached accessor behind it.</summary>
        public static readonly DependencyProperty GhostProperty = DependencyProperty.Register(
            GhostPropertyName,
            typeof(string),
            typeof(Undescribed),
            new(default(string)));
    }

    /// <summary>Records the observed changes a subscription delivers.</summary>
    private sealed class Recorder : IObserver<IObservedChange<object, object?>>
    {
        /// <summary>Gets the number of changes observed.</summary>
        public int Count { get; private set; }

        /// <summary>Gets the most recently observed change.</summary>
        public IObservedChange<object, object?>? Last { get; private set; }

        /// <inheritdoc/>
        public void OnNext(IObservedChange<object, object?> value)
        {
            Count++;
            Last = value;
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }
}
