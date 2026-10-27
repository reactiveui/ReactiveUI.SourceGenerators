// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.CommandBinding;
#else
namespace ReactiveUI.Binding.CommandBinding;
#endif

/// <summary>Executes a command with each value a sequence produces.</summary>
/// <remarks>
/// <para>
/// What an <c>InvokeCommand</c> does once the command is in hand, whether the generator resolved the command
/// property at compile time or the runtime engine read it from the expression. Both hand the command here rather
/// than writing the gating themselves, so a generated call site emits one call and the two paths cannot drift.
/// </para>
/// <para>
/// <see cref="ICommand.CanExecute"/> is asked at each emission rather than tracked from
/// <see cref="ICommand.CanExecuteChanged"/>: the value being offered is the command parameter, so only the
/// command can answer for that value, and an answer tracked from the event would have to be recomputed per value
/// anyway. A value the command refuses is dropped rather than held.
/// </para>
/// <para>
/// An error from a source sequence is rethrown on the thread that raised it, and completion is ignored.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandInvoker
{
    /// <summary>Executes one command with each value the sequence produces, skipping values the command refuses.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <param name="source">The sequence driving the executions.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>A disposable that, when disposed, stops executing the command.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="command"/> is null.</exception>
    public static IDisposable Invoke<T>(IObservable<T> source, ICommand command)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(command);

        return source.Subscribe(new FixedCommandObserver<T>(command));
    }

    /// <summary>Executes whichever command the observed sequence of commands last produced.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <param name="source">The sequence driving the executions.</param>
    /// <param name="commands">The command to execute, as the observed property produces it.</param>
    /// <returns>A disposable that, when disposed, stops executing and stops observing the property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="commands"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// The commands are subscribed first, so a command the sequence produces on subscription is latched before
    /// any value can arrive. Subscribing in the other order drops a value produced immediately.
    /// </para>
    /// <para>
    /// A null command - a property not yet assigned, or a path through an absent parent - drops the values
    /// offered while it stands, and a later command picks up from the next value. Replacing the command executes
    /// nothing by itself: values are what drive an execution, which is why the two sequences are not combined.
    /// </para>
    /// </remarks>
    public static IDisposable Invoke<T>(IObservable<T> source, IObservable<ICommand?> commands)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(commands);

        var latch = new CommandLatch();
        var commandSubscription = commands.Subscribe(latch);

        return new MultipleDisposable(commandSubscription, source.Subscribe(new LatchedCommandObserver<T>(latch)));
    }

    /// <summary>Offers each value to a command fixed for the lifetime of the subscription.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <param name="command">The command to execute.</param>
    private sealed class FixedCommandObserver<T>(ICommand command) : IObserver<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            if (!command.CanExecute(value))
            {
                return;
            }

            command.Execute(value);
        }
    }

    /// <summary>Holds the command an observed property last produced.</summary>
    /// <remarks>
    /// The command is written on whichever thread raised the property notification and read on whichever thread
    /// a value arrives on, which are not the same thread in the general case.
    /// </remarks>
    private sealed class CommandLatch : IObserver<ICommand?>
    {
        /// <summary>The command the property last produced.</summary>
        private ICommand? _command;

        /// <summary>Gets the command the property last produced.</summary>
        internal ICommand? Command => Volatile.Read(ref _command);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(ICommand? value) => Volatile.Write(ref _command, value);
    }

    /// <summary>Offers each value to whichever command the latch currently holds.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <param name="latch">The latch holding the command.</param>
    private sealed class LatchedCommandObserver<T>(CommandLatch latch) : IObserver<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            var command = latch.Command;
            if (command is null || !command.CanExecute(value))
            {
                return;
            }

            command.Execute(value);
        }
    }
}
