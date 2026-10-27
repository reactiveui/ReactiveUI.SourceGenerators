// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Default implementation of <see cref="IReactiveBinding{TView, TValue}"/> used by generated view-first bindings.</summary>
/// <typeparam name="TView">The type of the view.</typeparam>
/// <typeparam name="TValue">The type of the bound value.</typeparam>
[DebuggerDisplay("ReactiveBinding: {Direction} binding on {View}")]
public sealed class ReactiveBinding<TView, TValue> : IReactiveBinding<TView, TValue>
    where TView : IViewFor
{
    /// <summary>The underlying subscription that is disposed when this binding is disposed.</summary>
    private readonly IDisposable _subscription;

    /// <summary>Tracks whether this instance has been disposed (0 = not disposed, 1 = disposed).</summary>
    private int _disposed;

    /// <summary>Initializes a new instance of the <see cref="ReactiveBinding{TView, TValue}"/> class.</summary>
    /// <param name="view">The view that is bound.</param>
    /// <param name="changed">An observable that signals when the binding value changes.</param>
    /// <param name="direction">The direction of the binding.</param>
    /// <param name="subscription">The underlying subscription to dispose when the binding is disposed.</param>
    public ReactiveBinding(
        TView view,
        IObservable<TValue> changed,
        BindingDirection direction,
        IDisposable subscription)
    {
        View = view;
        Changed = changed;
        Direction = direction;
        _subscription = subscription;
    }

    /// <summary>Gets the view model expression, which is always null because a generated binding carries no expression.</summary>
    public Expression? ViewModelExpression => null;

    /// <inheritdoc/>
    public TView View { get; }

    /// <summary>Gets the view expression, which is always null because a generated binding carries no expression.</summary>
    public Expression? ViewExpression => null;

    /// <inheritdoc/>
    public IObservable<TValue> Changed { get; }

    /// <inheritdoc/>
    public BindingDirection Direction { get; }

    /// <summary>Disposes the underlying subscription on the first call; later calls do nothing.</summary>
    public void Dispose()
    {
        if (!TrySetDisposed())
        {
            return;
        }

        _subscription.Dispose();
    }

    /// <summary>Atomically marks this instance as disposed.</summary>
    /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TrySetDisposed() => Interlocked.Exchange(ref _disposed, 1) == 0;
}
