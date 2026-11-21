// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>Tests observing a read-only <see cref="DependencyProperty"/>, which only its key can write (ReactiveUI #4547).</summary>
/// <remarks>
/// Serialized: the tests share one static <see cref="DependencyProperty"/>, and the value-changed subscriptions
/// on it live in a process-wide table keyed by that property.
/// </remarks>
[NotInParallel]
public class ReadOnlyDependencyPropertyTests
{
    /// <summary>A generated observation delivers the current value, then each value written through the key.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_ReadOnlyDependencyProperty_EmitsEachValueWrittenThroughTheKey()
    {
        var button = new ReadOnlyPropertyButton();
        var recorder = new ValueRecorder();

        using (button.WhenAnyValue(static x => x.IsButtonEnabled).Subscribe(recorder))
        {
            button.Enable(true);
            button.Enable(false);
        }

        button.Enable(true);

        await Assert.That(recorder.Values).IsEquivalentTo([false, true, false]);
    }

    /// <summary>The registered WPF observer scores a read-only dependency property and reports its changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisteredObserver_ReadOnlyDependencyProperty_ScoresAndNotifies()
    {
        var button = new ReadOnlyPropertyButton();
        var observer = new DependencyObjectObservableForProperty();
        var recorder = new ChangeRecorder();
        var expression = ((System.Linq.Expressions.Expression<Func<ReadOnlyPropertyButton, bool>>)(x => x.IsButtonEnabled)).Body;

        var affinity = observer.GetAffinityForObject(typeof(ReadOnlyPropertyButton), nameof(ReadOnlyPropertyButton.IsButtonEnabled), false);
        using (observer
            .GetNotificationForProperty(button, expression, nameof(ReadOnlyPropertyButton.IsButtonEnabled), false, false)
            .Subscribe(recorder))
        {
            button.Enable(true);
        }

        await Assert.That(affinity).IsEqualTo(BindingAffinity.WpfDependencyObject);
        await Assert.That(recorder.Count).IsEqualTo(1);
    }

    /// <summary>Records the values an observation delivers.</summary>
    private sealed class ValueRecorder : IObserver<bool>
    {
        /// <summary>Gets the values observed, in order.</summary>
        public List<bool> Values { get; } = [];

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(bool value) => Values.Add(value);

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }

    /// <summary>Counts the changes a registered observer reports.</summary>
    private sealed class ChangeRecorder : IObserver<IObservedChange<object, object?>>
    {
        /// <summary>Gets the number of changes observed.</summary>
        public int Count { get; private set; }

        /// <inheritdoc/>
        public void OnNext(IObservedChange<object, object?> value) => Count++;

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
