// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extends <see cref="IInteractionContext{TInput, TOutput}"/> with the ability to retrieve the output.</summary>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
public interface IOutputContext<out TInput, TOutput> : IInteractionContext<TInput, TOutput>
{
    /// <summary>Gets the output of the interaction.</summary>
    /// <returns>The output.</returns>
    /// <exception cref="InvalidOperationException">If the output has not been set.</exception>
    TOutput GetOutput();
}
