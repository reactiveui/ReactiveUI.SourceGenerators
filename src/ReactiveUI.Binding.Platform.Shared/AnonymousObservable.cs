// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// An observable whose subscription logic is supplied as a delegate, for the platform observers that
/// hook an event on subscribe and hand back the unhook as the subscription.
/// </summary>
/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
/// <remarks>
/// Each platform assembly compiles its own internal copy, so this stays off the public surface of every
/// package and out of the seam: it names no scheduler and no notification type.
/// </remarks>
internal sealed class AnonymousObservable<T> : IObservable<T>
{
    /// <summary>Produces the subscription for an observer, returning the resource that tears it down.</summary>
    private readonly Func<IObserver<T>, IDisposable> _subscribe;

    /// <summary>Initializes a new instance of the <see cref="AnonymousObservable{T}"/> class.</summary>
    /// <param name="subscribe">Subscribes an observer and returns the resource that tears the subscription down.</param>
    public AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
    {
        ArgumentExceptionHelper.ThrowIfNull(subscribe);
        _subscribe = subscribe;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);
        return _subscribe(observer);
    }
}
