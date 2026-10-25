// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Foundation;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Receives key-value observing callbacks and forwards each to a delegate.</summary>
internal sealed class KvoCallbackObserver : NSObject
{
    /// <summary>Runs on each change the registration reports.</summary>
    private readonly Action _callback;

    /// <summary>Initializes a new instance of the <see cref="KvoCallbackObserver"/> class.</summary>
    /// <param name="callback">Runs on each change the registration reports.</param>
    public KvoCallbackObserver(Action callback) => _callback = callback;

    /// <inheritdoc/>
    public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context) => _callback();
}
