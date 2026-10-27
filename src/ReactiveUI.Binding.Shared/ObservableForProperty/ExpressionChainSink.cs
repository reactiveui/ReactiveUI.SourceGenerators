// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>
/// Observes the leaf of an expression member chain (<c>x.A.B.C</c>) and emits its value as an observed change
/// each time any link changes.
/// </summary>
/// <typeparam name="TSender">The root sender type surfaced on the emitted change.</typeparam>
/// <typeparam name="TValue">The leaf value type.</typeparam>
/// <remarks>
/// Each link is observed on the value the previous link produced, and the deeper links are re-attached when an
/// intermediate value changes. The current leaf value is emitted on subscribe unless the first value is skipped.
/// Nothing is emitted while an intermediate value is null. A leaf value that is not a <typeparamref name="TValue"/>
/// faults the observer with an <see cref="InvalidCastException"/>. The sequence never completes; observed values
/// carry the root object as their sender.
/// </remarks>
[DebuggerDisplay("ExpressionChainSink: {_expression}, Links = {_links.Length}, BeforeChange = {_beforeChange}, SkipInitial = {_skipInitial}, Distinct = {_isDistinct}")]
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
public sealed class ExpressionChainSink<TSender, TValue> : IObservable<IObservedChange<TSender, TValue>>
{
    /// <summary>The root object of the chain.</summary>
    private readonly TSender? _source;

    /// <summary>The full expression surfaced on the emitted change.</summary>
    private readonly Expression? _expression;

    /// <summary>The member-access links of the chain, in order.</summary>
    private readonly Expression[] _links;

    /// <summary>Whether values are observed before they change.</summary>
    private readonly bool _beforeChange;

    /// <summary>Whether the initial value is suppressed.</summary>
    private readonly bool _skipInitial;

    /// <summary>Whether consecutive equal leaf values are suppressed.</summary>
    private readonly bool _isDistinct;

    /// <summary>Whether the warning a property with no notification mechanism raises is suppressed.</summary>
    private readonly bool _suppressWarnings;

    /// <summary>Initializes a new instance of the <see cref="ExpressionChainSink{TSender, TValue}"/> class.</summary>
    /// <param name="source">The root object of the chain.</param>
    /// <param name="expression">The full expression surfaced on the emitted change.</param>
    /// <param name="links">The member-access links of the chain, in order.</param>
    /// <param name="beforeChange">Whether values are observed before they change.</param>
    /// <param name="skipInitial">Whether the first value the chain produces is dropped.</param>
    /// <param name="isDistinct">Whether consecutive equal leaf values are suppressed.</param>
    /// <param name="suppressWarnings">Whether the warning a property with no notification mechanism raises is suppressed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="links"/> is <see langword="null"/>.</exception>
    public ExpressionChainSink(
        TSender? source,
        Expression? expression,
        Expression[] links,
        bool beforeChange,
        bool skipInitial,
        bool isDistinct,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(links);
        _source = source;
        _expression = expression;
        _links = links;
        _beforeChange = beforeChange;
        _skipInitial = skipInitial;
        _isDistinct = isDistinct;
        _suppressWarnings = suppressWarnings;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<IObservedChange<TSender, TValue>> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sink = new Sink(
            observer,
            new ExpressionChainParameters<TSender>(
                _source,
                _expression,
                _links,
                _beforeChange,
                _skipInitial,
                _isDistinct,
                _suppressWarnings));
        sink.Run();
        return sink;
    }

    /// <summary>The running state of one chain subscription.</summary>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    private sealed class Sink : IDisposable
    {
        /// <summary>Serializes chain mutations and emission.</summary>
        private readonly Lock _gate = new();

        /// <summary>The observer receiving the leaf observed changes.</summary>
        private readonly IObserver<IObservedChange<TSender, TValue>> _downstream;

        /// <summary>The root object of the chain.</summary>
        private readonly TSender? _source;

        /// <summary>The full expression surfaced on the emitted change.</summary>
        private readonly Expression? _expression;

        /// <summary>The member-access links of the chain, in order.</summary>
        private readonly Expression[] _links;

        /// <summary>Whether values are observed before they change.</summary>
        private readonly bool _beforeChange;

        /// <summary>Whether consecutive equal leaf values are suppressed.</summary>
        private readonly bool _isDistinct;

        /// <summary>Whether the warning a property with no notification mechanism raises is suppressed.</summary>
        private readonly bool _suppressWarnings;

        /// <summary>The per-link watchers.</summary>
        private readonly Level[] _levels;

        /// <summary>Whether the next raw emission should be skipped (skip-initial).</summary>
        private bool _skipNext;

        /// <summary>Whether the next emission is dropped if it repeats the last one.</summary>
        /// <remarks>
        /// Set when the kicker emits, and cleared by the emission after it. A notification that races the
        /// subscribe-then-read window reports the value the kicker already pushed, and this collapses that
        /// one repeat whatever the distinct setting is.
        /// </remarks>
        private bool _suppressNextIfSameAsLast;

        /// <summary>The last emitted leaf value, used by the distinct gate.</summary>
        private TValue _last = default!;

        /// <summary>Whether <see cref="_last"/> holds a value yet.</summary>
        private bool _hasLast;

        /// <summary>Latched once this chain subscription has been disposed.</summary>
        private bool _disposed;

        /// <summary>Initializes a new instance of the <see cref="Sink"/> class.</summary>
        /// <param name="downstream">The observer receiving the leaf observed changes.</param>
        /// <param name="parameters">How the chain is to be observed.</param>
        public Sink(
            IObserver<IObservedChange<TSender, TValue>> downstream,
            in ExpressionChainParameters<TSender> parameters)
        {
            _downstream = downstream;
            _source = parameters.Source;
            _expression = parameters.Expression;
            _links = parameters.Links;
            _beforeChange = parameters.BeforeChange;
            _isDistinct = parameters.IsDistinct;
            _suppressWarnings = parameters.SuppressWarnings;
            _skipNext = parameters.SkipInitial;
            _levels = new Level[parameters.Links.Length];
        }

        /// <summary>Establishes the chain from the root value.</summary>
        /// <remarks>
        /// The levels are built here rather than in the constructor: each one captures the sink,
        /// and handing out <see langword="this"/> mid-construction would expose a half-built object.
        /// </remarks>
        public void Run()
        {
            lock (_gate)
            {
                for (var i = 0; i < _levels.Length; i++)
                {
                    _levels[i] = new(this, i, i == _levels.Length - 1);
                }

                if (_links.Length == 0)
                {
                    return;
                }

                _levels[0].SetParent(_source);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_gate)
            {
                _disposed = true;
                for (var i = 0; i < _levels.Length; i++)
                {
                    // Every slot is filled: Run takes this same lock and populates them all before the
                    // sink is handed out, so there is no window where one is still null here.
                    _levels[i].Dispose();
                }
            }
        }

        /// <summary>Sets the parent value of the level after <paramref name="level"/>.</summary>
        /// <param name="level">The level index that produced the value.</param>
        /// <param name="value">The value the link produced (the parent for the next level).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetNextParent(int level, object? value) => _levels[level + 1].SetParent(value);

        /// <summary>Handles a leaf raw emission: applies skip-initial, the non-null-parent filter, the cast, the kicker dedup and the distinct gate.</summary>
        /// <param name="parentMissing">Whether the leaf's parent was null.</param>
        /// <param name="value">The leaf value when the parent is present.</param>
        /// <param name="fromKicker">Whether this is the read that follows attaching a link subscription.</param>
        private void Emit(bool parentMissing, object? value, bool fromKicker = false)
        {
            if (_skipNext)
            {
                _skipNext = false;
                _suppressNextIfSameAsLast = false;
                return;
            }

            if (parentMissing)
            {
                return;
            }

            TValue typed;
            if (value is null)
            {
                typed = default!;
            }
            else if (value is TValue cast)
            {
                typed = cast;
            }
            else
            {
                _downstream.OnError(new InvalidCastException($"Unable to cast from {value.GetType()} to {typeof(TValue)}."));
                return;
            }

            if (ShouldSuppress(typed, fromKicker))
            {
                return;
            }

            // A notification that raced the subscribe-then-read window is queued behind the gate and will
            // report the value the kicker just pushed. Dropping the next equal value collapses that one
            // repeat without changing what a caller sees when the value genuinely changes twice.
            _suppressNextIfSameAsLast = fromKicker;
            _last = typed;
            _hasLast = true;
            _downstream.OnNext(new ObservedChange<TSender, TValue>(_source!, _expression, typed));
        }

        /// <summary>Decides whether an emission is dropped, by the kicker dedup or the distinct gate.</summary>
        /// <param name="typed">The leaf value about to be emitted.</param>
        /// <param name="fromKicker">Whether this is the read that follows attaching a link subscription.</param>
        /// <returns><see langword="true"/> when the value is not emitted.</returns>
        /// <remarks>The kicker itself is never dropped: it is the value the caller subscribed to receive.</remarks>
        private bool ShouldSuppress(TValue typed, bool fromKicker)
        {
            if (fromKicker)
            {
                return false;
            }

            var repeatsLast = _hasLast && EqualityComparer<TValue>.Default.Equals(typed, _last);
            if (_suppressNextIfSameAsLast)
            {
                _suppressNextIfSameAsLast = false;
                if (repeatsLast)
                {
                    return true;
                }
            }

            return _isDistinct && repeatsLast;
        }

        /// <summary>A single chain link's watcher: re-subscribes on parent change and reads the link's value.</summary>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        private sealed class Level : IDisposable
        {
            /// <summary>The owning chain sink.</summary>
            private readonly Sink _sink;

            /// <summary>This watcher's position in the chain.</summary>
            private readonly int _index;

            /// <summary>Whether this is the final link in the chain.</summary>
            private readonly bool _isLeaf;

            /// <summary>The current link-notification subscription; swapped on each re-parent.</summary>
            private readonly SwapDisposable _subscription = new();

            /// <summary>This link's value fetcher, compiled once, or <see langword="null"/> for an unsupported member.</summary>
            private readonly Func<object?, object?[]?, object?>? _getter;

            /// <summary>This link's index/argument array (non-null only for indexer links), cached once.</summary>
            private readonly object?[]? _arguments;

            /// <summary>Whether this level is subscribing to its link's notifications.</summary>
            /// <remarks>
            /// Only read under the gate, so only a notification the subscribing thread raises from inside
            /// <c>Subscribe</c> sees it set.
            /// </remarks>
            private bool _attaching;

            /// <summary>Initializes a new instance of the <see cref="Level"/> class.</summary>
            /// <param name="sink">The owning chain sink.</param>
            /// <param name="index">This watcher's position in the chain.</param>
            /// <param name="isLeaf">Whether this is the final link in the chain.</param>
            public Level(Sink sink, int index, bool isLeaf)
            {
                _sink = sink;
                _index = index;
                _isLeaf = isLeaf;
                _getter = ChainLinkReader.CreateGetter(sink._links[index]);
                _arguments = sink._links[index].GetArgumentsArray();
            }

            /// <summary>Re-establishes this watcher on a new parent value and propagates the current value downward.</summary>
            /// <param name="parent">The object this link is read from.</param>
            public void SetParent(object? parent)
            {
                if (parent is null)
                {
                    _subscription.Disposable = null;
                    if (_isLeaf)
                    {
                        _sink.Emit(parentMissing: true, null);
                    }
                    else
                    {
                        _sink.SetNextParent(_index, null);
                    }

                    return;
                }

                var link = _sink._links[_index];

                // Subscribe before reading, so a change between the two is reported rather than lost. The
                // caller holds the gate, so a notification that races this window queues behind it and
                // re-reports the value the kicker is about to push; the sink drops that one repeat. A
                // notification raised from inside Subscribe, such as the one a POCO link emits, is ignored:
                // the kicker reads the same value straight after.
                _attaching = true;
                _subscription.Disposable = ReactiveNotifyPropertyChangedMixins
                    .NotifyForProperty(parent, link, _sink._beforeChange, _sink._suppressWarnings)
                    .Subscribe(new Observer(this));
                _attaching = false;

                Push(ReadValue(parent), fromKicker: true);
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _subscription.Dispose();

            /// <summary>Handles a notification for this link by re-reading the value and propagating it.</summary>
            /// <param name="change">The notification (its value is read via reflection).</param>
            private void OnNotification(IObservedChange<object?, object?> change)
            {
                lock (_sink._gate)
                {
                    if (_sink._disposed || _attaching)
                    {
                        return;
                    }

                    Push(ReadValue(change.Sender));
                }
            }

            /// <summary>Forwards a link-subscription error to the downstream observer.</summary>
            /// <param name="error">The error to forward.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ForwardError(Exception error) => _sink._downstream.OnError(error);

            /// <summary>Reads the current value of this link from a parent using the cached fetcher.</summary>
            /// <param name="parent">The object the link is read from.</param>
            /// <returns>The link's current value, or the default when the parent is null.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private object? ReadValue(object? parent) =>
                ChainLinkReader.ReadValue(parent, _getter, _arguments, _sink._links[_index]);

            /// <summary>Forwards this link's value to the next level, or emits it at the leaf.</summary>
            /// <param name="value">The value this link produced.</param>
            /// <param name="fromKicker">Whether this is the read that follows attaching the subscription.</param>
            private void Push(object? value, bool fromKicker = false)
            {
                if (_isLeaf)
                {
                    _sink.Emit(parentMissing: false, value, fromKicker);
                }
                else
                {
                    _sink.SetNextParent(_index, value);
                }
            }

            /// <summary>Forwards a link's notifications back into the level.</summary>
            private sealed class Observer : IObserver<IObservedChange<object?, object?>>
            {
                /// <summary>The owning level.</summary>
                private readonly Level _level;

                /// <summary>Initializes a new instance of the <see cref="Observer"/> class.</summary>
                /// <param name="level">The owning level.</param>
                public Observer(Level level) => _level = level;

                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void OnNext(IObservedChange<object?, object?> value) => _level.OnNotification(value);

                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void OnError(Exception error) => _level.ForwardError(error);

                /// <inheritdoc/>
                public void OnCompleted()
                {
                }
            }
        }
    }
}
