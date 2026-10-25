// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Represents an interaction between collaborating application components.</summary>
/// <typeparam name="TInput">The interaction's input type.</typeparam>
/// <typeparam name="TOutput">The interaction's output type.</typeparam>
/// <remarks>
/// <para>
/// Interactions allow collaborating components in an application to ask each other questions. Typically,
/// interactions allow a view model to get the user's confirmation from the view before proceeding with
/// some operation.
/// </para>
/// <para>
/// By default, handlers are invoked in reverse order of registration. That is, handlers registered later
/// are given the opportunity to handle interactions before handlers that were registered earlier.
/// </para>
/// <para>
/// Note that handlers are not required to handle an interaction. They can choose to ignore it, leaving it
/// for some other handler to handle. The interaction's <see cref="Handle"/> method will throw an
/// <see cref="UnhandledInteractionException{TInput, TOutput}"/> if no handler handles the interaction.
/// </para>
/// </remarks>
[DebuggerDisplay("Interaction: Handlers = {_handlers.Length}")]
public class Interaction<TInput, TOutput> : IInteraction<TInput, TOutput>
{
    /// <summary>The scheduler each handler is invoked on, or null to invoke handlers on the calling thread.</summary>
    private readonly ISequencer? _handlerScheduler;

    /// <summary>
    /// The registered handlers, invoked in reverse order during <see cref="Handle"/>. The array is replaced
    /// rather than changed, so a question already being handled walks the set it started with.
    /// </summary>
    private Func<IInteractionContext<TInput, TOutput>, Task>[] _handlers = [];

    /// <summary>Initializes a new instance of the <see cref="Interaction{TInput, TOutput}"/> class that invokes handlers on the calling thread.</summary>
    public Interaction()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Interaction{TInput, TOutput}"/> class that invokes each handler on a scheduler.</summary>
    /// <param name="handlerScheduler">The scheduler each handler is invoked on, such as the main thread's; null invokes handlers on the calling thread.</param>
    public Interaction(ISequencer? handlerScheduler) => _handlerScheduler = handlerScheduler;

    /// <inheritdoc/>
    public IDisposable RegisterHandler(Action<IInteractionContext<TInput, TOutput>> handler)
    {
        ArgumentExceptionHelper.ThrowIfNull(handler);

        return RegisterHandler(context =>
        {
            handler(context);
            return Task.CompletedTask;
        });
    }

    /// <inheritdoc />
    public IDisposable RegisterHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler)
    {
        ArgumentExceptionHelper.ThrowIfNull(handler);

        AddHandler(handler);
        return new ActionDisposable(() => RemoveHandler(handler));
    }

    /// <summary>Registers a handler that finishes when the observable it returns completes.</summary>
    /// <typeparam name="TDontCare">The element type of the returned observable; the values are ignored.</typeparam>
    /// <param name="handler">The handler; the interaction moves to the next handler once the observable completes, and a fault in the observable faults <see cref="Handle"/>.</param>
    /// <returns>A disposable which, when disposed, unregisters the handler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
    public IDisposable RegisterHandler<TDontCare>(
        Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler)
    {
        ArgumentExceptionHelper.ThrowIfNull(handler);

        Task ContentHandler(IInteractionContext<TInput, TOutput> context)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = handler(context).Subscribe(new ObservableToTaskObserver<TDontCare>(tcs));
            return tcs.Task;
        }

        AddHandler(ContentHandler);
        return new ActionDisposable(() => RemoveHandler(ContentHandler));
    }

    /// <summary>Runs the handlers, latest registered first, until one sets an output, and returns that output.</summary>
    /// <param name="input">The input for the interaction.</param>
    /// <returns>A task that completes with the output the first handling handler set.</returns>
    /// <exception cref="UnhandledInteractionException{TInput, TOutput}">No handler set an output.</exception>
    public virtual async Task<TOutput> Handle(TInput input)
    {
        var context = GenerateContext(input);
        var handlers = Volatile.Read(ref _handlers);

        for (var i = handlers.Length - 1; i >= 0; i--)
        {
            await InvokeHandler(handlers[i], context).ConfigureAwait(false);
            if (context.IsHandled)
            {
                return context.GetOutput();
            }
        }

        throw new UnhandledInteractionException<TInput, TOutput>(this, input);
    }

    /// <summary>Asks the question as an observable: each subscription runs <see cref="Handle"/> once.</summary>
    /// <param name="input">The input for the interaction.</param>
    /// <returns>
    /// A cold observable that emits the output and completes, or fails with
    /// <see cref="UnhandledInteractionException{TInput, TOutput}"/> when no handler sets an output. Disposing the
    /// subscription drops the output; a handler that is already running still finishes.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<TOutput> WhenHandled(TInput input) =>
        new FromAsyncSignal<TOutput>(_ => Handle(input));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IObservable<TOutput> IInteraction<TInput, TOutput>.Handle(TInput input) => WhenHandled(input);

    /// <summary>Gets a copy of the registered handlers in order of registration.</summary>
    /// <returns>The registered handlers, earliest first.</returns>
    protected Func<IInteractionContext<TInput, TOutput>, Task>[] GetHandlers() => [.. Volatile.Read(ref _handlers)];

    /// <summary>Creates the context every handler receives for one call to <see cref="Handle"/>.</summary>
    /// <param name="input">The input passed to <see cref="Handle"/>.</param>
    /// <returns>A new interaction context carrying the input.</returns>
    protected virtual IOutputContext<TInput, TOutput> GenerateContext(TInput input) =>
        new InteractionContext<TInput, TOutput>(input);

    /// <summary>Invokes a handler on the handler scheduler, or inline when there is none.</summary>
    /// <param name="handler">The handler to invoke.</param>
    /// <param name="context">The context of the question being asked.</param>
    /// <returns>A task that completes when the handler finishes.</returns>
    private Task InvokeHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler, IOutputContext<TInput, TOutput> context)
    {
        if (_handlerScheduler is null)
        {
            return handler(context);
        }

        var call = new ScheduledHandlerCall(handler, context);
        _ = _handlerScheduler.Schedule(
            call,
            static (_, pending) =>
            {
                pending.Run();
                return EmptyDisposable.Instance;
            });
        return call.Completion;
    }

    /// <summary>Adds a handler to the registered set.</summary>
    /// <param name="handler">The handler to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler) =>
        CopyOnWriteArray.Add(ref _handlers, handler);

    /// <summary>Removes the first registration of a handler from the registered set.</summary>
    /// <param name="handler">The handler to remove.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveHandler(Func<IInteractionContext<TInput, TOutput>, Task> handler) =>
        CopyOnWriteArray.Remove(ref _handlers, handler);

    /// <summary>One handler call waiting for the handler scheduler to run it.</summary>
    /// <param name="handler">The handler to invoke.</param>
    /// <param name="context">The context of the question being asked.</param>
    private sealed class ScheduledHandlerCall(
        Func<IInteractionContext<TInput, TOutput>, Task> handler,
        IOutputContext<TInput, TOutput> context)
    {
        /// <summary>Completes with the handler's task once the scheduler has invoked the handler.</summary>
        private readonly TaskCompletionSource<Task> _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets a task that completes when the handler finishes.</summary>
        public Task Completion => _started.Task.Unwrap();

        /// <summary>Invokes the handler; a handler that throws faults <see cref="Completion"/>.</summary>
        public void Run()
        {
            Task running;
            try
            {
                running = handler(context);
            }
            catch (Exception ex)
            {
                _ = _started.TrySetException(ex);
                return;
            }

            _ = _started.TrySetResult(running);
        }
    }

    /// <summary>An observer that bridges an observable sequence to a <see cref="TaskCompletionSource{TResult}"/>, completing the task when the observable completes or faults.</summary>
    /// <typeparam name="T">The element type of the observable sequence.</typeparam>
    private sealed class ObservableToTaskObserver<T> : IObserver<T>
    {
        /// <summary>The task completion source that is signaled when the observable completes or errors.</summary>
        private readonly TaskCompletionSource<bool> _tcs;

        /// <summary>Initializes a new instance of the <see cref="ObservableToTaskObserver{T}"/> class.</summary>
        /// <param name="tcs">The task completion source to signal.</param>
        public ObservableToTaskObserver(TaskCompletionSource<bool> tcs) => _tcs = tcs;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => _tcs.TrySetResult(true);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => _tcs.TrySetException(error);

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            // Intentionally empty — we only care about completion/error.
        }
    }
}
