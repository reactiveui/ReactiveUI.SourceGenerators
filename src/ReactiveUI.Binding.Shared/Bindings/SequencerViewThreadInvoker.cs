// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Routes writes to a view object through the sequencer that owns the object's thread.</summary>
/// <typeparam name="TTarget">The type of view object this invoker claims.</typeparam>
/// <typeparam name="TSequencer">The sequencer type that owns those objects' threads.</typeparam>
/// <remarks>
/// A platform supplies <see cref="SequencerFor"/>, which names the sequencer that owns an object. The sequencer
/// answers whether the calling thread owns it, and queues work onto that thread. An object with no owning sequencer
/// belongs to no thread, so every thread may write to it.
/// </remarks>
public abstract class SequencerViewThreadInvoker<TTarget, TSequencer> : IViewThreadInvoker
    where TTarget : class
    where TSequencer : class, ISequencer, IThreadAffineSequencer
{
    /// <summary>Returns whether <paramref name="target"/> is a <typeparamref name="TTarget"/>.</summary>
    /// <param name="target">The object a binding is about to write to.</param>
    /// <returns><see langword="true"/> when this invoker routes writes to <paramref name="target"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Claims(object target) => target is TTarget;

    /// <summary>Returns whether the calling thread owns the sequencer of <paramref name="target"/>.</summary>
    /// <param name="target">A <typeparamref name="TTarget"/>; any other type throws <see cref="InvalidCastException"/>.</param>
    /// <returns><see langword="true"/> when the calling thread may write to <paramref name="target"/> now.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess(object target) => SequencerFor((TTarget)target)?.CheckAccess() ?? true;

    /// <summary>Queues <paramref name="callback"/> on the sequencer of <paramref name="target"/>, or runs it inline when the target has none.</summary>
    /// <param name="target">A <typeparamref name="TTarget"/>; any other type throws <see cref="InvalidCastException"/>.</param>
    /// <param name="callback">The callback to run.</param>
    /// <param name="state">The value passed to <paramref name="callback"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is null.</exception>
    public void Post(object target, Action<object?> callback, object? state)
    {
        ArgumentExceptionHelper.ThrowIfNull(callback);

        var sequencer = SequencerFor((TTarget)target);
        if (sequencer is null)
        {
            callback(state);
            return;
        }

        _ = sequencer.Schedule(
            new PostedCallback(callback, state),
            static (_, posted) =>
            {
                posted.Callback(posted.State);
                return EmptyDisposable.Instance;
            });
    }

    /// <summary>Returns the sequencer that owns the thread of <paramref name="target"/>.</summary>
    /// <param name="target">The object a binding writes to.</param>
    /// <returns>The owning sequencer, or null when <paramref name="target"/> belongs to no thread.</returns>
    protected abstract TSequencer? SequencerFor(TTarget target);

    /// <summary>A callback and the value it is called with.</summary>
    /// <param name="Callback">The callback to run.</param>
    /// <param name="State">The value passed to <paramref name="Callback"/>.</param>
    private readonly record struct PostedCallback(Action<object?> Callback, object? State);
}
