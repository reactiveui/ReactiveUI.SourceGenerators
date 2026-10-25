// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Covers the invoker that routes writes through the sequencer owning a view object's thread.</summary>
public class SequencerViewThreadInvokerTests
{
    /// <summary>An object of the claimed type is claimed, and anything else is not.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Claims_MatchesTheTargetType()
    {
        var invoker = new OwnedTargetInvoker();

        await Assert.That(invoker.Claims(new OwnedTarget(null))).IsTrue();
        await Assert.That(invoker.Claims(new())).IsFalse();
    }

    /// <summary>Access follows the owning sequencer's answer.</summary>
    /// <param name="hasAccess">Whether the sequencer grants the calling thread access.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task CheckAccess_WithAnOwningSequencer_FollowsTheSequencer(bool hasAccess)
    {
        var target = new OwnedTarget(new AffineSequencer { HasAccess = hasAccess });

        await Assert.That(new OwnedTargetInvoker().CheckAccess(target)).IsEqualTo(hasAccess);
    }

    /// <summary>An object no sequencer owns may be written from any thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_WithNoOwningSequencer_IsTrue() =>
        await Assert.That(new OwnedTargetInvoker().CheckAccess(new OwnedTarget(null))).IsTrue();

    /// <summary>A posted callback waits on the owning sequencer, then runs with its state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_WithAnOwningSequencer_RunsTheCallbackOnTheSequencer()
    {
        var sequencer = new AffineSequencer();
        var state = new object();
        object? received = null;

        new OwnedTargetInvoker().Post(new OwnedTarget(sequencer), s => received = s, state);
        var beforeRun = received;
        _ = sequencer.Inner.RunPending();

        await Assert.That(beforeRun).IsNull();
        await Assert.That(received).IsSameReferenceAs(state);
    }

    /// <summary>A callback for an object no sequencer owns runs inline.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_WithNoOwningSequencer_RunsTheCallbackInline()
    {
        var state = new object();
        object? received = null;

        new OwnedTargetInvoker().Post(new OwnedTarget(null), s => received = s, state);

        await Assert.That(received).IsSameReferenceAs(state);
    }

    /// <summary>A null callback is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_WithANullCallback_Throws() =>
        await Assert.That(static () => new OwnedTargetInvoker().Post(new OwnedTarget(null), null!, null)).ThrowsExactly<ArgumentNullException>();

    /// <summary>An invoker that reads the owning sequencer from the object.</summary>
    private sealed class OwnedTargetInvoker : SequencerViewThreadInvoker<OwnedTarget, AffineSequencer>
    {
        /// <inheritdoc/>
        protected override AffineSequencer? SequencerFor(OwnedTarget target) => target.Sequencer;
    }

    /// <summary>A manual sequencer that answers a set value for thread access.</summary>
    private sealed class AffineSequencer : ISequencer, IThreadAffineSequencer
    {
        /// <summary>Gets the sequencer that holds the scheduled work.</summary>
        public ManualSequencer Inner { get; } = new();

        /// <summary>Gets or sets a value indicating whether the calling thread owns the sequencer.</summary>
        public bool HasAccess { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset Now => Inner.Now;

        /// <inheritdoc/>
        public long Timestamp => Inner.Timestamp;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess() => HasAccess;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Schedule(IWorkItem item) => Inner.Schedule(item);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Schedule(IWorkItem item, long dueTimestamp) => Inner.Schedule(item, dueTimestamp);
    }

    /// <summary>A view object that carries the sequencer owning its thread, or none.</summary>
    /// <param name="Sequencer">The owning sequencer, or null when the object belongs to no thread.</param>
    private sealed record OwnedTarget(AffineSequencer? Sequencer);
}
