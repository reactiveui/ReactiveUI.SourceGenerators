// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Tests for <see cref="BindingHooks"/>, which lets a registered <see cref="IPropertyBindingHook"/> refuse a
/// binding. Registration is global, so each test restores the empty set it started from.
/// </summary>
[NotInParallel]
public class BindingHooksTests
{
    /// <summary>An empty set of observed changes, which is what a hook is handed for a bound object.</summary>
    private static readonly Func<IObservedChange<object, object>[]> NoChanges = static () => [];

    /// <summary>With nothing registered, the guard is false so a caller never builds the closures.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Any_WithNoHookRegistered_IsFalse()
    {
        ResetHooks();

        await Assert.That(BindingHooks.Any).IsFalse();
    }

    /// <summary>With nothing registered, nothing can refuse a binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_WithNoHookRegistered_AllowsTheBinding()
    {
        ResetHooks();

        var allowed = BindingHooks.ShouldBind(new(), new(), NoChanges, NoChanges, BindingDirection.OneWay);

        await Assert.That(allowed).IsTrue();
    }

    /// <summary>A registered hook is seen once the set is refreshed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Any_AfterRegisteringAHook_IsTrue()
    {
        ResetHooks();
        try
        {
            AppLocator.Register<IPropertyBindingHook>(static () => new StubHook(true));
            BindingHooks.Refresh();

            await Assert.That(BindingHooks.Any).IsTrue();
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>A hook that approves lets the binding through.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_WhenTheHookApproves_AllowsTheBinding()
    {
        ResetHooks();
        try
        {
            AppLocator.Register<IPropertyBindingHook>(static () => new StubHook(true));
            BindingHooks.Refresh();

            var allowed = BindingHooks.ShouldBind(new(), new(), NoChanges, NoChanges, BindingDirection.TwoWay);

            await Assert.That(allowed).IsTrue();
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>A hook that refuses cancels the binding, which is the whole point of the interface.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_WhenTheHookRefuses_CancelsTheBinding()
    {
        ResetHooks();
        try
        {
            AppLocator.Register<IPropertyBindingHook>(static () => new StubHook(false));
            BindingHooks.Refresh();

            var allowed = BindingHooks.ShouldBind(new(), new(), NoChanges, NoChanges, BindingDirection.OneWay);

            await Assert.That(allowed).IsFalse();
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>One refusal is enough, whatever the other hooks say.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_WhenOneOfSeveralHooksRefuses_CancelsTheBinding()
    {
        ResetHooks();
        try
        {
            AppLocator.Register<IPropertyBindingHook>(static () => new StubHook(true));
            AppLocator.Register<IPropertyBindingHook>(static () => new StubHook(false));
            BindingHooks.Refresh();

            var allowed = BindingHooks.ShouldBind(new(), new(), NoChanges, NoChanges, BindingDirection.OneWay);

            await Assert.That(allowed).IsFalse();
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>The bound target is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_NullTarget_Throws() =>
        await Assert.That(static () => BindingHooks.ShouldBind(new(), null!, NoChanges, NoChanges, BindingDirection.OneWay))
            .Throws<ArgumentNullException>();

    /// <summary>The source reader is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_NullSourceReader_Throws() =>
        await Assert.That(static () => BindingHooks.ShouldBind(new(), new(), null!, NoChanges, BindingDirection.OneWay))
            .Throws<ArgumentNullException>();

    /// <summary>The target reader is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_NullTargetReader_Throws() =>
        await Assert.That(static () => BindingHooks.ShouldBind(new(), new(), NoChanges, null!, BindingDirection.OneWay))
            .Throws<ArgumentNullException>();

    /// <summary>A hook reading a property path gets one change per property, with its owner, expression and value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_WithPropertyPaths_HandsTheHookEachPropertyRead()
    {
        ResetHooks();
        try
        {
            var hook = new ReadingHook();
            AppLocator.Register<IPropertyBindingHook>(() => hook);
            BindingHooks.Refresh();
            var leaf = new PathNode("leaf", null);
            var root = new PathNode("root", leaf);
            var target = new PathNode("target", null);

            var allowed = BindingHooks.ShouldBind(root, target, ChildNamePath(), NamePath(), BindingDirection.OneWay);

            await Assert.That(allowed).IsTrue();
            await Assert.That(hook.Source.Length).IsEqualTo(ChildNamePath().Length);
            await Assert.That(hook.Source[0].Sender).IsSameReferenceAs(root);
            await Assert.That(hook.Source[0].Value).IsSameReferenceAs(leaf);
            await Assert.That(hook.Source[1].Sender).IsSameReferenceAs(leaf);
            await Assert.That(hook.Source[1].Value).IsEqualTo("leaf");
            await Assert.That(hook.Source[1].Expression!.ToString()).IsEqualTo("node.Name");
            await Assert.That(hook.Target.Length).IsEqualTo(1);
            await Assert.That(hook.Target[0].Value).IsEqualTo("target");
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>A path stops at a null owner, and a null root reads nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_WithPropertyPaths_StopsAtANullOwner()
    {
        ResetHooks();
        try
        {
            var hook = new ReadingHook();
            AppLocator.Register<IPropertyBindingHook>(() => hook);
            BindingHooks.Refresh();
            var root = new PathNode("root", null);

            _ = BindingHooks.ShouldBind(null, root, NamePath(), ChildNamePath(), BindingDirection.TwoWay);

            await Assert.That(hook.Source).IsEmpty();
            await Assert.That(hook.Target.Length).IsEqualTo(1);
            await Assert.That(hook.Target[0].Sender).IsSameReferenceAs(root);
        }
        finally
        {
            ResetHooks();
        }
    }

    /// <summary>Both property paths are required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShouldBind_NullPropertyPath_Throws()
    {
        await Assert.That(static () => BindingHooks.ShouldBind(new(), new(), null!, NamePath(), BindingDirection.OneWay))
            .Throws<ArgumentNullException>();
        await Assert.That(static () => BindingHooks.ShouldBind(new(), new(), NamePath(), (BindingHookProperty[])null!, BindingDirection.OneWay))
            .Throws<ArgumentNullException>();
    }

    /// <summary>A hook property needs both of its functions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingHookProperty_NullFunction_Throws()
    {
        await Assert.That(static () => BindingHookProperty.Create<PathNode, string>(null!, static () => static node => node.Name))
            .Throws<ArgumentNullException>();
        await Assert.That(static () => BindingHookProperty.Create<PathNode, string>(static node => node.Name, null!))
            .Throws<ArgumentNullException>();
    }

    /// <summary>Describes the path <c>Name</c>, the way generated code does.</summary>
    /// <returns>The path.</returns>
    private static BindingHookProperty[] NamePath() =>
    [
        BindingHookProperty.Create<PathNode, string>(static node => node.Name, static () => static node => node.Name),
    ];

    /// <summary>Describes the path <c>Child.Name</c>, the way generated code does.</summary>
    /// <returns>The path.</returns>
    private static BindingHookProperty[] ChildNamePath() =>
    [
        BindingHookProperty.Create<PathNode, PathNode?>(static node => node.Child, static () => static node => node.Child),
        .. NamePath(),
    ];

    /// <summary>Clears every registration and the cached set, so a test starts from nothing registered.</summary>
    private static void ResetHooks()
    {
        AppLocator.UnregisterAll<IPropertyBindingHook>();
        BindingHooks.Refresh();
    }

    /// <summary>A hook that reads both sides and keeps what it read.</summary>
    private sealed class ReadingHook : IPropertyBindingHook
    {
        /// <summary>Gets the changes read from the source side.</summary>
        public IObservedChange<object, object>[] Source { get; private set; } = [];

        /// <summary>Gets the changes read from the target side.</summary>
        public IObservedChange<object, object>[] Target { get; private set; } = [];

        /// <inheritdoc/>
        public bool ExecuteHook(
            object? source,
            object target,
            Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
            Func<IObservedChange<object, object>[]> getCurrentViewProperties,
            BindingDirection direction)
        {
            Source = getCurrentViewModelProperties();
            Target = getCurrentViewProperties();
            return true;
        }
    }

    /// <summary>A hook that answers the same way every time.</summary>
    /// <param name="allow">What the hook answers.</param>
    private sealed class StubHook(bool allow) : IPropertyBindingHook
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExecuteHook(
            object? source,
            object target,
            Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
            Func<IObservedChange<object, object>[]> getCurrentViewProperties,
            BindingDirection direction) => allow;
    }

    /// <summary>An object with a property to read and a child to walk to.</summary>
    /// <param name="Name">The node's name.</param>
    /// <param name="Child">The next node, or null at the end of the chain.</param>
    private sealed record PathNode(string Name, PathNode? Child);
}
