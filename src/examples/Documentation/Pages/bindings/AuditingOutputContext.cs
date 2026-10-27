// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Passes every call on to an interaction context and writes each answer to the console.</summary>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
/// <param name="inner">The context that holds the input and the answer.</param>
[System.Diagnostics.DebuggerDisplay("AuditingOutputContext: Input = {Input}, IsHandled = {IsHandled}")]
public sealed class AuditingOutputContext<TInput, TOutput>(IOutputContext<TInput, TOutput> inner) : IOutputContext<TInput, TOutput>
{
    /// <inheritdoc/>
    public TInput Input => inner.Input;

    /// <inheritdoc/>
    public bool IsHandled => inner.IsHandled;

    /// <inheritdoc/>
    public void SetOutput(TOutput output)
    {
        Console.WriteLine($"answer {output}");
        inner.SetOutput(output);
    }

    /// <inheritdoc/>
    public TOutput GetOutput() => inner.GetOutput();
}
