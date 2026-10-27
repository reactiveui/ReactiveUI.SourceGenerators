// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Indicates that an interaction has gone unhandled.</summary>
/// <typeparam name="TInput">The type of the interaction's input.</typeparam>
/// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
[DebuggerDisplay("UnhandledInteractionException: Input = {Input}, Interaction = {Interaction}")]
public class UnhandledInteractionException<TInput, TOutput> : Exception
{
    /// <summary>Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class for an interaction no handler handled.</summary>
    /// <param name="interaction">The interaction that no handler handled.</param>
    /// <param name="input">The input into the interaction.</param>
    public UnhandledInteractionException(Interaction<TInput, TOutput> interaction, TInput input)
        : this("Failed to find a registration for an Interaction.")
    {
        Interaction = interaction;
        Input = input;
    }

    /// <summary>Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.</summary>
    public UnhandledInteractionException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.</summary>
    /// <param name="message">A message about the exception.</param>
    public UnhandledInteractionException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UnhandledInteractionException{TInput, TOutput}"/> class.</summary>
    /// <param name="message">A message about the exception.</param>
    /// <param name="innerException">Any other exception that caused the issue.</param>
    public UnhandledInteractionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Gets the interaction that was not handled, or null when the exception was created without one.</summary>
    public Interaction<TInput, TOutput>? Interaction { get; }

    /// <summary>Gets the input for the interaction that was not handled, or the default value when the exception was created without one.</summary>
    public TInput Input { get; } = default!;
}
