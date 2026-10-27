// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>An interaction that writes each question and answer to the console, as a bank keeps an audit trail of an approval.</summary>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
[System.Diagnostics.DebuggerDisplay("AuditedInteraction: Audited interaction")]
public sealed class AuditedInteraction<TInput, TOutput> : Interaction<TInput, TOutput>
{
    /// <inheritdoc/>
    public override async Task<TOutput> Handle(TInput input)
    {
        Console.WriteLine($"ask {input} of {GetHandlers().Length} handlers");
        return await base.Handle(input).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override IOutputContext<TInput, TOutput> GenerateContext(TInput input) => new AuditingOutputContext<TInput, TOutput>(base.GenerateContext(input));
}
