// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Tests that the source-generator-generated BindInteraction code works correctly at runtime.</summary>
public class BindInteractionTests
{
    /// <summary>The interaction input value used across the handler tests.</summary>
    private const string Question = "question";

    /// <summary>The number of sequential Handle calls in the multiple-calls test.</summary>
    private const int HandleCallCount = 5;

    /// <summary>Verifies that a task-based handler is registered and produces the expected output.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler_Handle_ReturnsExpectedOutput()
    {
        var vm = new SharedScenarios.BindInteraction.TaskHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.TaskHandler.MyView();

        using var binding = BindInteractionScenarios.TaskHandler(vm, view);

        var result = await vm.Confirm.Handle(Question);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that an observable-based handler is registered and produces the expected output.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableHandler_Handle_ReturnsExpectedOutput()
    {
        var vm = new SharedScenarios.BindInteraction.ObservableHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.ObservableHandler.MyView();

        using var binding = BindInteractionScenarios.ObservableHandler(vm, view);

        var result = await vm.Confirm.Handle(Question);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that disposing the binding unregisters the handler so subsequent Handle calls throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler_Dispose_UnregistersHandler()
    {
        var vm = new SharedScenarios.BindInteraction.TaskHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.TaskHandler.MyView();

        var binding = BindInteractionScenarios.TaskHandler(vm, view);
        binding.Dispose();

        await Assert.That(() => vm.Confirm.Handle(Question))
            .ThrowsExactly<UnhandledInteractionException<string, bool>>();
    }

    /// <summary>Verifies that when the VM's interaction property changes, the handler follows the new interaction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler_InteractionPropertyChanges_HandlerFollowsNewInteraction()
    {
        var vm = new SharedScenarios.BindInteraction.TaskHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.TaskHandler.MyView();
        var originalInteraction = vm.Confirm;

        using var binding = BindInteractionScenarios.TaskHandler(vm, view);

        // Swap the interaction
        var newInteraction = new Interaction<string, bool>();
        vm.Confirm = newInteraction;

        // Old interaction should no longer have the handler
        await Assert.That(() => originalInteraction.Handle("old"))
            .ThrowsExactly<UnhandledInteractionException<string, bool>>();

        // New interaction should have the handler
        var result = await newInteraction.Handle("new");
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that a deep property path (Child.Confirm) interaction binding works.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyPath_Handle_ReturnsExpectedOutput()
    {
        var vm = new SharedScenarios.BindInteraction.DeepPropertyPath.MyViewModel { Child = new() };
        var view = new SharedScenarios.BindInteraction.DeepPropertyPath.MyView();

        using var binding = BindInteractionScenarios.DeepPropertyPath(vm, view);

        var result = await vm.Child!.Confirm.Handle(Question);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that when Child is replaced, the handler follows the new child's interaction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyPath_ChildReplaced_HandlerFollowsNewChild()
    {
        var originalChild = new SharedScenarios.BindInteraction.DeepPropertyPath.ChildViewModel();
        var vm = new SharedScenarios.BindInteraction.DeepPropertyPath.MyViewModel { Child = originalChild };
        var view = new SharedScenarios.BindInteraction.DeepPropertyPath.MyView();

        using var binding = BindInteractionScenarios.DeepPropertyPath(vm, view);

        var newChild = new SharedScenarios.BindInteraction.DeepPropertyPath.ChildViewModel();
        vm.Child = newChild;

        // Old child's interaction is no longer handled
        await Assert.That(() => originalChild.Confirm.Handle("old"))
            .ThrowsExactly<UnhandledInteractionException<string, bool>>();

        // New child's interaction is handled
        var result = await newChild.Confirm.Handle("new");
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that disposing the observable handler binding unregisters the handler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableHandler_Dispose_UnregistersHandler()
    {
        var vm = new SharedScenarios.BindInteraction.ObservableHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.ObservableHandler.MyView();

        var binding = BindInteractionScenarios.ObservableHandler(vm, view);
        binding.Dispose();

        await Assert.That(() => vm.Confirm.Handle(Question))
            .ThrowsExactly<UnhandledInteractionException<string, bool>>();
    }

    /// <summary>Verifies that when the VM's interaction property changes, the observable handler follows the new interaction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableHandler_InteractionPropertyChanges_HandlerFollows()
    {
        var vm = new SharedScenarios.BindInteraction.ObservableHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.ObservableHandler.MyView();
        var originalInteraction = vm.Confirm;

        using var binding = BindInteractionScenarios.ObservableHandler(vm, view);

        var newInteraction = new Interaction<string, bool>();
        vm.Confirm = newInteraction;

        // Old interaction should no longer have the handler
        await Assert.That(() => originalInteraction.Handle("old"))
            .ThrowsExactly<UnhandledInteractionException<string, bool>>();

        // New interaction should have the handler
        var result = await newInteraction.Handle("new");
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that setting child to null on a deep path unregisters the handler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyPath_ChildSetToNull_HandlerUnregisters()
    {
        var child = new SharedScenarios.BindInteraction.DeepPropertyPath.ChildViewModel();
        var vm = new SharedScenarios.BindInteraction.DeepPropertyPath.MyViewModel { Child = child };
        var view = new SharedScenarios.BindInteraction.DeepPropertyPath.MyView();

        using var binding = BindInteractionScenarios.DeepPropertyPath(vm, view);

        // Handler works initially
        var result = await child.Confirm.Handle(Question);
        await Assert.That(result).IsTrue();

        // Setting child to null should unregister
        vm.Child = null;

        await Assert.That(() => child.Confirm.Handle(Question))
            .ThrowsExactly<UnhandledInteractionException<string, bool>>();
    }

    /// <summary>Verifies that disposing twice does not throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler_DoubleDispose_DoesNotThrow()
    {
        var vm = new SharedScenarios.BindInteraction.TaskHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.TaskHandler.MyView();

        var binding = BindInteractionScenarios.TaskHandler(vm, view);
        binding.Dispose();

        await Assert.That((Action)binding.Dispose).ThrowsNothing();
    }

    /// <summary>Verifies that the interaction binding handles multiple sequential Handle calls correctly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler_MultipleHandleCalls_AllReturnExpectedOutput()
    {
        var vm = new SharedScenarios.BindInteraction.TaskHandler.MyViewModel();
        var view = new SharedScenarios.BindInteraction.TaskHandler.MyView();

        using var binding = BindInteractionScenarios.TaskHandler(vm, view);

        for (var i = 0; i < HandleCallCount; i++)
        {
            var result = await vm.Confirm.Handle($"question-{i}");
            await Assert.That(result).IsTrue();
        }
    }
}
