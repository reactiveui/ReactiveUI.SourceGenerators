// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>How a binding reacts when the sequence feeding its write faults.</summary>
/// <remarks>
/// A faulted write has no caller stack to surface on: it arrives on whichever thread raised the change
/// notification. Every fault is logged against the bound expression. A fault that carries an inner exception,
/// a setter that threw, is rethrown as a <see cref="TargetInvocationException"/>. A fault with none is the
/// sequence itself ending in error, and is only logged. Completion is ignored.
/// </remarks>
public static class BindingErrors
{
    /// <summary>Subscribes a binding's write to its source, applying the binding fault contract.</summary>
    /// <typeparam name="T">The type of the value written.</typeparam>
    /// <param name="source">The sequence feeding the write.</param>
    /// <param name="onNext">The write itself.</param>
    /// <param name="bindingExpression">The bound expression, named in the log entry and the rethrown exception.</param>
    /// <returns>A disposable that, when disposed, disconnects the write.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IDisposable Subscribe<T>(IObservable<T> source, Action<T> onNext, string bindingExpression)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(onNext);

        return source.Subscribe(new BindingObserver<T>(onNext, bindingExpression));
    }

    /// <summary>Applies the write, and the fault contract when the sequence errors.</summary>
    /// <typeparam name="T">The type of the value written.</typeparam>
    /// <param name="onNext">The write itself.</param>
    /// <param name="bindingExpression">The bound expression, named when a fault is reported.</param>
    private sealed class BindingObserver<T>(Action<T> onNext, string bindingExpression) : IObserver<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        /// <exception cref="TargetInvocationException">The fault carried an inner exception.</exception>
        public void OnError(Exception error)
        {
            LogHost.Default.Error(error, $"{bindingExpression} Binding received an Exception!");

            if (error.InnerException is null)
            {
                return;
            }

            throw new TargetInvocationException(
                $"{bindingExpression} Binding received an Exception!",
                error.InnerException);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value) => onNext(value);
    }
}
