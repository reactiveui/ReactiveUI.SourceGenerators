// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Runs a binding's write on the thread that owns the object it lands on, through the platform's own dispatcher.</summary>
public interface IViewThreadInvoker
{
    /// <summary>Determines whether this invoker handles <paramref name="target"/>.</summary>
    /// <param name="target">The object a binding writes to.</param>
    /// <returns><see langword="true"/> when the object belongs to this invoker's platform; otherwise <see langword="false"/>.</returns>
    bool Claims(object target);

    /// <summary>Determines whether the calling thread may write to <paramref name="target"/> directly.</summary>
    /// <param name="target">An object this invoker claims.</param>
    /// <returns><see langword="true"/> on the owning thread, or when the object has no owning thread; otherwise <see langword="false"/>.</returns>
    bool CheckAccess(object target);

    /// <summary>Queues a callback on the thread that owns <paramref name="target"/>.</summary>
    /// <param name="target">An object this invoker claims.</param>
    /// <param name="callback">The callback to run.</param>
    /// <param name="state">The value handed to <paramref name="callback"/>.</param>
    void Post(object target, Action<object?> callback, object? state);
}
