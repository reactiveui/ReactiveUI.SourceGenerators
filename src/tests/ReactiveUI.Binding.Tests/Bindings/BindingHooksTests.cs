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

    /// <summary>Clears every registration and the cached set, so a test starts from nothing registered.</summary>
    private static void ResetHooks()
    {
        AppLocator.UnregisterAll<IPropertyBindingHook>();
        BindingHooks.Refresh();
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
}
