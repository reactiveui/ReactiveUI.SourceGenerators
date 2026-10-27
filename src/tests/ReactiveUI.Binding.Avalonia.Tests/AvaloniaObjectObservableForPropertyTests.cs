// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>Tests for the Avalonia property observer, compiled once against each runtime flavour.</summary>
[NotInParallel]
public class AvaloniaObjectObservableForPropertyTests
{
    /// <summary>The affinity of a WPF dependency property, which a registered Avalonia property shares.</summary>
    private const int RegisteredPropertyAffinity = 4;

    /// <summary>The value the tests write to the observed property.</summary>
    private const string NewText = "first";

    /// <summary>The expression every notification carries in these tests.</summary>
    private static readonly Expression Carried = Expression.Constant(null);

    /// <summary>A type that is not an Avalonia object scores zero.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithANonAvaloniaType_IsZero() =>
        await Assert.That(new AvaloniaObjectObservableForProperty().GetAffinityForObject(typeof(string), "Length", false)).IsEqualTo(0);

    /// <summary>A registered Avalonia property scores the dependency-property affinity.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithARegisteredProperty_IsTheDependencyPropertyAffinity() =>
        await Assert.That(new AvaloniaObjectObservableForProperty().GetAffinityForObject(typeof(TestObject), nameof(TestObject.Text), false))
            .IsEqualTo(RegisteredPropertyAffinity);

    /// <summary>A CLR property that registers no Avalonia property scores zero.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithAPlainProperty_IsZero() =>
        await Assert.That(new AvaloniaObjectObservableForProperty().GetAffinityForObject(typeof(TestObject), nameof(TestObject.Plain), true))
            .IsEqualTo(0);

    /// <summary>A null sender is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_WithANullSender_Throws() =>
        await Assert.That(static () => new AvaloniaObjectObservableForProperty().GetNotificationForProperty(null!, Carried, "Text", false, false))
            .ThrowsExactly<ArgumentNullException>();

    /// <summary>A sender that is not an Avalonia object is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_WithANonAvaloniaSender_Throws() =>
        await Assert.That(static () => new AvaloniaObjectObservableForProperty().GetNotificationForProperty(new(), Carried, "Text", false, false))
            .ThrowsExactly<ArgumentException>();

    /// <summary>A property the type does not register is reported as missing, with or without the debug message.</summary>
    /// <param name="suppressWarnings">Whether the debug message is skipped.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GetNotificationForProperty_WithAnUnregisteredProperty_Throws(bool suppressWarnings)
    {
        TestObject? target = null;
        await AvaloniaTestSession.Run(() =>
        {
            target = new();
            return Task.CompletedTask;
        });

        await Assert.That(() => new AvaloniaObjectObservableForProperty().GetNotificationForProperty(target!, Carried, nameof(TestObject.Plain), false, suppressWarnings))
            .ThrowsExactly<MissingMemberException>();
    }

    /// <summary>A change to the observed property raises its new value; a change to another property raises nothing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task GetNotificationForProperty_RaisesOnlyTheObservedPropertysChanges() =>
        AvaloniaTestSession.Run(static async () =>
        {
            TestObject target = new();
            ListObserver<IObservedChange<object, object?>> observer = new();

            using (new AvaloniaObjectObservableForProperty().GetNotificationForProperty(target, Carried, nameof(TestObject.Text), false, false).Subscribe(observer))
            {
                target.Count = 1;
                target.Text = NewText;
            }

            await Assert.That(observer.Values.Count).IsEqualTo(1);
            await Assert.That(observer.Values[0].Value).IsEqualTo(NewText);
            await Assert.That(observer.Values[0].Sender).IsSameReferenceAs(target);
            await Assert.That(observer.Values[0].Expression).IsSameReferenceAs(Carried);
        });

    /// <summary>Disposing the subscription detaches it from the object.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task GetNotificationForProperty_AfterDisposal_RaisesNothing() =>
        AvaloniaTestSession.Run(static async () =>
        {
            TestObject target = new();
            ListObserver<IObservedChange<object, object?>> observer = new();

            new AvaloniaObjectObservableForProperty().GetNotificationForProperty(target, Carried, nameof(TestObject.Text), true, false).Subscribe(observer).Dispose();
            target.Text = NewText;

            await Assert.That(observer.Values).IsEmpty();
        });
}
