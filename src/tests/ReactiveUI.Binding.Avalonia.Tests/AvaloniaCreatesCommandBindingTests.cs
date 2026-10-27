// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>Tests for the Avalonia command binder, compiled once against each runtime flavour.</summary>
[NotInParallel]
public class AvaloniaCreatesCommandBindingTests
{
    /// <summary>The affinity of a binding through a named routed event.</summary>
    private const int EventTargetAffinity = 6;

    /// <summary>The affinity of a binding through a command source's Command property.</summary>
    private const int CommandSourceAffinity = 10;

    /// <summary>The routed event the event bindings name.</summary>
    private const string ClickEvent = "Click";

    /// <summary>The parameter the command runs with while it can execute.</summary>
    private const string FirstParameter = "first";

    /// <summary>The parameter pushed while the command cannot execute.</summary>
    private const string SecondParameter = "second";

    /// <summary>The parameters a command bound until disposal runs with.</summary>
    private static readonly object?[] OnlyFirstParameter = [FirstParameter];

    /// <summary>A type that is not an input element scores zero.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithANonInputElement_IsZero() =>
        await Assert.That(new AvaloniaCreatesCommandBinding().GetAffinityForObject<TestObject>(false)).IsEqualTo(0);

    /// <summary>An input element bound through an event scores the event affinity.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithAnEvent_IsTheEventAffinity() =>
        await Assert.That(new AvaloniaCreatesCommandBinding().GetAffinityForObject<Border>(true)).IsEqualTo(EventTargetAffinity);

    /// <summary>A command source bound without an event scores the command source affinity.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithACommandSource_IsTheCommandSourceAffinity() =>
        await Assert.That(new AvaloniaCreatesCommandBinding().GetAffinityForObject<Button>(false)).IsEqualTo(CommandSourceAffinity);

    /// <summary>An input element that is not a command source, bound without an event, scores zero.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObject_WithAnInputElementThatIsNotACommandSource_IsZero() =>
        await Assert.That(new AvaloniaCreatesCommandBinding().GetAffinityForObject<Border>(false)).IsEqualTo(0);

    /// <summary>A null target binds nothing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BindCommandToObject_WithANullTarget_BindsNothing() =>
        await Assert.That(new AvaloniaCreatesCommandBinding().BindCommandToObject<Button>(new RecordingCommand(), null, new ValueSource())).IsNull();

    /// <summary>Clearing the command of something that is not a command source binds nothing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_WithANullCommandAndNoCommandSource_BindsNothing() =>
        AvaloniaTestSession.Run(static async () =>
            await Assert.That(new AvaloniaCreatesCommandBinding().BindCommandToObject(null, new Border(), new ValueSource())).IsNull());

    /// <summary>A command bound to something that is not a command source is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_WithACommandAndNoCommandSource_Throws() =>
        AvaloniaTestSession.Run(static async () =>
        {
            Border border = new();

            await Assert.That(() => new AvaloniaCreatesCommandBinding().BindCommandToObject(new RecordingCommand(), border, new ValueSource()))
                .ThrowsExactly<InvalidOperationException>();
        });

    /// <summary>A null command clears the command source's command.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_WithANullCommand_ClearsTheCommand() =>
        AvaloniaTestSession.Run(static async () =>
        {
            Button button = new() { Command = new RecordingCommand() };

            var binding = new AvaloniaCreatesCommandBinding().BindCommandToObject(null, button, new ValueSource());

            await Assert.That(binding).IsNotNull();
            await Assert.That(button.Command).IsNull();
        });

    /// <summary>A command sets the command source's command, and its parameter follows the stream until disposal.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_WithACommand_SetsTheCommandAndFollowsTheParameter() =>
        AvaloniaTestSession.Run(static async () =>
        {
            Button button = new();
            RecordingCommand command = new();
            ValueSource parameters = new();

            var binding = new AvaloniaCreatesCommandBinding().BindCommandToObject(command, button, parameters)!;
            parameters.Push(FirstParameter);

            await Assert.That(button.Command).IsSameReferenceAs(command);
            await Assert.That(button.CommandParameter).IsEqualTo(FirstParameter);

            binding.Dispose();

            await Assert.That(button.Command).IsNull();
            await Assert.That(parameters.HasObserver).IsFalse();
        });

    /// <summary>Disposing leaves a command that something else set since.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_AfterTheCommandIsReplaced_DisposalLeavesTheReplacement() =>
        AvaloniaTestSession.Run(static async () =>
        {
            Button button = new();
            RecordingCommand replacement = new();

            var binding = new AvaloniaCreatesCommandBinding().BindCommandToObject(new RecordingCommand(), button, new ValueSource())!;
            button.Command = replacement;
            binding.Dispose();

            await Assert.That(button.Command).IsSameReferenceAs(replacement);
        });

    /// <summary>An event binding with a null command or target binds nothing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_ToAnEventWithANullCommandOrTarget_BindsNothing() =>
        AvaloniaTestSession.Run(static async () =>
        {
            AvaloniaCreatesCommandBinding binder = new();

            await Assert.That(binder.BindCommandToObject<Button, RoutedEventArgs>(null, new(), new ValueSource(), ClickEvent)).IsNull();
            await Assert.That(binder.BindCommandToObject<Button, RoutedEventArgs>(new RecordingCommand(), null, new ValueSource(), ClickEvent)).IsNull();
        });

    /// <summary>An event binding onto something that is not an input element is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_ToAnEventOnANonInputElement_Throws() =>
        AvaloniaTestSession.Run(static async () =>
        {
            TestObject target = new();

            await Assert.That(() => new AvaloniaCreatesCommandBinding().BindCommandToObject<TestObject, RoutedEventArgs>(new RecordingCommand(), target, new ValueSource(), ClickEvent))
                .ThrowsExactly<InvalidOperationException>();
        });

    /// <summary>An event name no type in the hierarchy registers is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_ToAnUnknownEvent_Throws() =>
        AvaloniaTestSession.Run(static async () =>
        {
            Button button = new();

            await Assert.That(() => new AvaloniaCreatesCommandBinding().BindCommandToObject<Button, RoutedEventArgs>(new RecordingCommand(), button, new ValueSource(), "NoSuchEvent"))
                .ThrowsExactly<InvalidOperationException>();
        });

    /// <summary>An event whose arguments are not the requested type is not a match.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_ToAnEventWithOtherArguments_Throws() =>
        AvaloniaTestSession.Run(static async () =>
        {
            Button button = new();

            await Assert.That(() => new AvaloniaCreatesCommandBinding().BindCommandToObject<Button, PointerPressedEventArgs>(new RecordingCommand(), button, new ValueSource(), ClickEvent))
                .ThrowsExactly<InvalidOperationException>();
        });

    /// <summary>
    /// An event a base type registers executes the command with the latest parameter while it can execute, keeps
    /// IsEnabled in step with CanExecute, and stops when disposed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_ToABaseTypesEvent_ExecutesTheCommandUntilDisposed() =>
        AvaloniaTestSession.Run(static async () =>
        {
            DerivedButton button = new();
            RecordingCommand command = new();
            ValueSource parameters = new();

            var binding = new AvaloniaCreatesCommandBinding().BindCommandToObject<DerivedButton, RoutedEventArgs>(command, button, parameters, ClickEvent)!;
            parameters.Push(FirstParameter);
            button.RaiseEvent(new(Button.ClickEvent));

            command.Enabled = false;
            parameters.Push(SecondParameter);
            var enabledWhileBlocked = button.IsEnabled;
            button.RaiseEvent(new(Button.ClickEvent));

            parameters.Complete();
            binding.Dispose();
            command.Enabled = true;
            button.RaiseEvent(new(Button.ClickEvent));

            await Assert.That(command.Executed).IsEquivalentTo(OnlyFirstParameter);
            await Assert.That(enabledWhileBlocked).IsFalse();
            await Assert.That(button.IsEnabled).IsTrue();
        });

    /// <summary>An error from the parameter stream is rethrown to the caller that raised it.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommandToObject_ToAnEvent_RethrowsAParameterError() =>
        AvaloniaTestSession.Run(static async () =>
        {
            ValueSource parameters = new();
            using var binding = new AvaloniaCreatesCommandBinding().BindCommandToObject<Button, RoutedEventArgs>(new RecordingCommand(), new(), parameters, ClickEvent)!;

            await Assert.That(() => parameters.Fail(new FormatException())).ThrowsExactly<FormatException>();
        });

    /// <summary>A handler binding rejects null attach and detach delegates.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BindCommandToObject_WithHandlers_RejectsNullDelegates()
    {
        AvaloniaCreatesCommandBinding binder = new();
        EventSource source = new();

        await Assert.That(() => binder.BindCommandToObject<EventSource, EventArgs>(new RecordingCommand(), source, new ValueSource(), null!, h => source.Fired -= h))
            .ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => binder.BindCommandToObject<EventSource, EventArgs>(new RecordingCommand(), source, new ValueSource(), h => source.Fired += h, null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>A handler binding with a null command or target binds nothing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BindCommandToObject_WithHandlersAndANullCommandOrTarget_BindsNothing()
    {
        AvaloniaCreatesCommandBinding binder = new();
        EventSource source = new();

        await Assert.That(binder.BindCommandToObject<EventSource, EventArgs>(null, source, new ValueSource(), h => source.Fired += h, h => source.Fired -= h)).IsNull();
        await Assert.That(binder.BindCommandToObject<EventSource, EventArgs>(new RecordingCommand(), null, new ValueSource(), h => source.Fired += h, h => source.Fired -= h)).IsNull();
    }

    /// <summary>A handler binding executes the command with the latest parameter while it can execute, and detaches when disposed.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BindCommandToObject_WithHandlers_ExecutesTheCommandUntilDisposed()
    {
        EventSource source = new();
        RecordingCommand command = new();
        ValueSource parameters = new();

        var binding = new AvaloniaCreatesCommandBinding().BindCommandToObject<EventSource, EventArgs>(command, source, parameters, h => source.Fired += h, h => source.Fired -= h)!;
        parameters.Push(FirstParameter);
        source.Raise();
        command.Enabled = false;
        source.Raise();
        parameters.Complete();
        binding.Dispose();

        await Assert.That(command.Executed).IsEquivalentTo(OnlyFirstParameter);
        await Assert.That(source.HasHandlers).IsFalse();
        await Assert.That(parameters.HasObserver).IsFalse();
    }

    /// <summary>A handler binding rethrows an error from the parameter stream.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BindCommandToObject_WithHandlers_RethrowsAParameterError()
    {
        EventSource source = new();
        ValueSource parameters = new();
        using var binding = new AvaloniaCreatesCommandBinding()
            .BindCommandToObject<EventSource, EventArgs>(new RecordingCommand(), source, parameters, h => source.Fired += h, h => source.Fired -= h)!;

        await Assert.That(() => parameters.Fail(new FormatException())).ThrowsExactly<FormatException>();
    }

    /// <summary>A button type that registers no routed events of its own, so its events come from a base type.</summary>
    public sealed class DerivedButton : Button
    {
    }
}
