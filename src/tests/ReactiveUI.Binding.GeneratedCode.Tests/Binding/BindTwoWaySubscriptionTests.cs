// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Checks that failed binding creation releases every attached notification handler.</summary>
public class BindTwoWaySubscriptionTests
{
    /// <summary>The value delivered before reverse subscription setup.</summary>
    private const string InitialText = "initial";

    /// <summary>The edit that must not cross a failed binding.</summary>
    private const string LaterText = "after failure";

    /// <summary>A failure on either endpoint leaves neither endpoint bound.</summary>
    /// <param name="rejectSource">Whether the source, rather than the target, rejects the subscription.</param>
    /// <param name="withConverters">Whether to use explicit conversion functions.</param>
    /// <returns>The asynchronous assertion operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task BindTwoWay_SubscriptionThrows_DetachesBothEndpoints([Matrix(false, true)] bool rejectSource, [Matrix(false, true)] bool withConverters)
    {
        var source = new Endpoint { Text = InitialText, RejectSubscription = rejectSource };
        var target = new Endpoint { RejectSubscription = !rejectSource };

        var createBinding = withConverters
            ? (Func<IDisposable>)(() => source.BindTwoWay(target, static x => x.Text, static x => x.Text, static value => value, static value => value))
            : () => source.BindTwoWay(target, static x => x.Text, static x => x.Text);

        await Assert.That(createBinding).Throws<InvalidOperationException>();

        await Assert.That(source.HasSubscribers).IsFalse();
        await Assert.That(target.HasSubscribers).IsFalse();
        var textAfterFailure = target.Text;
        source.Text = LaterText;
        await Assert.That(target.Text).IsEqualTo(textAfterFailure);
    }

    /// <summary>A failed runtime binding also releases the forward subscription.</summary>
    /// <param name="withConverters">Whether to use explicit conversion functions.</param>
    /// <returns>The asynchronous assertion operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task BindTwoWayUnsafe_TargetSubscriptionThrows_DetachesSource(bool withConverters)
    {
        var source = new Endpoint { Text = InitialText };
        var target = new Endpoint { RejectSubscription = true };

        var createBinding = withConverters
            ? (Func<IDisposable>)(() => source.BindTwoWayUnsafe(target, x => x.Text, x => x.Text, static value => value, static value => value))
            : () => source.BindTwoWayUnsafe(target, x => x.Text, x => x.Text);

        await Assert.That(createBinding).Throws<InvalidOperationException>();

        await Assert.That(source.HasSubscribers).IsFalse();
        source.Text = LaterText;
        await Assert.That(target.Text).IsEqualTo(InitialText);
    }

    /// <summary>A failed view-first runtime binding releases its view-model subscription.</summary>
    /// <returns>The asynchronous assertion operation.</returns>
    [Test]
    public async Task BindUnsafe_TargetSubscriptionThrows_DetachesSource()
    {
        var source = new Endpoint { Text = InitialText };
        var target = new Endpoint { RejectSubscription = true };

        await Assert.That(() => target.BindUnsafe(source, x => x.Text, x => x.Text))
            .Throws<InvalidOperationException>();

        await Assert.That(source.HasSubscribers).IsFalse();
        source.Text = LaterText;
        await Assert.That(target.Text).IsEqualTo(InitialText);
    }

    /// <summary>An endpoint that can reject notification subscriptions before attaching them.</summary>
    public sealed class Endpoint : INotifyPropertyChanged, IViewFor
    {
        /// <summary>The handlers attached to this endpoint.</summary>
        private PropertyChangedEventHandler? _handlers;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                if (RejectSubscription)
                {
                    throw new InvalidOperationException("The endpoint rejected the subscription.");
                }

                _handlers += value;
            }

            remove => _handlers -= value;
        }

        /// <inheritdoc/>
        public object? ViewModel { get; set; }

        /// <summary>Gets or sets a value indicating whether notification subscriptions fail.</summary>
        public bool RejectSubscription { get; set; }

        /// <summary>Gets a value indicating whether any notification handler remains attached.</summary>
        public bool HasSubscribers => _handlers is not null;

        /// <summary>Gets or sets the value synchronized by the binding.</summary>
        public string Text
        {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }

                field = value;
                _handlers?.Invoke(this, new(nameof(Text)));
            }
        } = string.Empty;
    }
}
