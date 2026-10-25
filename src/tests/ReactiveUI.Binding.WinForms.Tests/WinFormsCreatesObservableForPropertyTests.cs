// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>Tests for the WinForms property observer.</summary>
public class WinFormsCreatesObservableForPropertyTests
{
    /// <summary>The number of notifications expected when two changes are raised.</summary>
    private const int TwoNotifications = 2;

    /// <summary>An expression standing in for the observed property path.</summary>
    private static readonly Expression NameExpression =
        ((Expression<Func<ComponentWithChangedEvent, string>>)(x => x.Name)).Body;

    /// <summary>Verifies that a component exposing a matching Changed event is claimed.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_ComponentWithChangedEvent_ClaimsTheProperty()
    {
        var observer = new WinFormsCreatesObservableForProperty();

        await Assert.That(observer.GetAffinityForObject(typeof(ComponentWithChangedEvent), "Name", false))
            .IsEqualTo(BindingAffinity.WinFormsEvent);
    }

    /// <summary>Verifies that before-change observation is declined; WinForms events fire after the change.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_BeforeChanged_IsDeclined()
    {
        var observer = new WinFormsCreatesObservableForProperty();

        await Assert.That(observer.GetAffinityForObject(typeof(ComponentWithChangedEvent), "Name", true))
            .IsEqualTo(0);
    }

    /// <summary>Verifies that a type which is not a component is declined.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_NonComponent_IsDeclined()
    {
        var observer = new WinFormsCreatesObservableForProperty();

        await Assert.That(observer.GetAffinityForObject(typeof(PlainModel), "Name", false)).IsEqualTo(0);
    }

    /// <summary>Verifies that a component without the matching Changed event is declined.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_ComponentWithoutMatchingEvent_IsDeclined()
    {
        var observer = new WinFormsCreatesObservableForProperty();

        await Assert.That(observer.GetAffinityForObject(typeof(ComponentWithChangedEvent), "Untracked", false))
            .IsEqualTo(0);
    }

    /// <summary>Verifies that raising the Changed event pushes a notification.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_RaisesOnTheChangedEvent()
    {
        var observer = new WinFormsCreatesObservableForProperty();
        using var component = new ComponentWithChangedEvent();
        var received = 0;

        using var subscription = observer
            .GetNotificationForProperty(component, NameExpression, "Name", false, false)
            .Subscribe(new CountingObserver(() => received++));

        component.Name = "first";
        component.Name = "second";

        await Assert.That(received).IsEqualTo(TwoNotifications);
    }

    /// <summary>Verifies that the subscription detaches from the event on disposal.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_DisposalDetachesTheHandler()
    {
        var observer = new WinFormsCreatesObservableForProperty();
        using var component = new ComponentWithChangedEvent();
        var received = 0;

        var subscription = observer
            .GetNotificationForProperty(component, NameExpression, "Name", false, false)
            .Subscribe(new CountingObserver(() => received++));

        component.Name = "first";
        subscription.Dispose();
        component.Name = "second";

        await Assert.That(received).IsEqualTo(1);
    }

    /// <summary>Verifies that a null sender is rejected.</summary>
    [Test]
    public void GetNotificationForProperty_NullSender_ThrowsArgumentNullException()
    {
        var observer = new WinFormsCreatesObservableForProperty();

        _ = Assert.Throws<ArgumentNullException>(
            () => observer.GetNotificationForProperty(null!, NameExpression, "Name", false, false));
    }

    /// <summary>Verifies that a property with no matching Changed event is rejected.</summary>
    [Test]
    public void GetNotificationForProperty_WithoutMatchingEvent_ThrowsArgumentException()
    {
        var observer = new WinFormsCreatesObservableForProperty();
        using var component = new ComponentWithChangedEvent();

        _ = Assert.Throws<ArgumentException>(
            () => observer.GetNotificationForProperty(component, NameExpression, "Untracked", false, false));
    }

    /// <summary>A public component raising the <c>{PropertyName}Changed</c> event, which the observer looks up among public members only.</summary>
    public sealed class ComponentWithChangedEvent : Component
    {
        /// <summary>Raised after <see cref="Name"/> changes.</summary>
        public event EventHandler? NameChanged;

        /// <summary>Gets or sets a value whose changes are announced by <see cref="NameChanged"/>.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Name
        {
            get;
            set
            {
                field = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
            }
        } = string.Empty;

        /// <summary>Gets a value with no corresponding Changed event.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Untracked { get; } = string.Empty;
    }

    /// <summary>A type that is not a component, so WinForms observation should decline it.</summary>
    public sealed class PlainModel
    {
        /// <summary>Gets a value that no WinForms event announces.</summary>
        public string Name { get; } = string.Empty;
    }

    /// <summary>An observer that counts the notifications it receives.</summary>
    /// <param name="onNext">Invoked for each notification.</param>
    public sealed class CountingObserver(Action onNext) : IObserver<IObservedChange<object, object?>>
    {
        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => throw error;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(IObservedChange<object, object?> value) => onNext();
    }
}
