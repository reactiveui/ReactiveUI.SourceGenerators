// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Observes property chains that are only known as expressions at run time.</summary>
/// <remarks>
/// These overloads take an <see cref="Expression"/> the caller built rather than a lambda the compiler can
/// read, so no generator resolves them: the chain is always walked by reflection, and the overloads are
/// annotated as not trim- or AOT-safe.
/// <para>
/// The chain walking is the engine the runtime observation fallback uses: it resolves each link,
/// re-subscribes a deeper link when an intermediate value changes, and filters duplicates.
/// Each arity differs only in how many chains it combines.
/// </para>
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Why every overload here is unsafe to trim.</summary>
    private const string DynamicChainRequiresUnreferencedCode =
        "Evaluates expression-based member chains via reflection; members may be trimmed.";

    /// <summary>Observes 1 property chain named by a run-time expression and emits the selector applied to its observed change, on subscription and after each change.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Func<IObservedChange<TSender, object?>, TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, selector, true);

    /// <summary>Observes 1 property chain named by a run-time expression and emits the selector applied to its observed change, on subscription and after each change.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Func<IObservedChange<TSender, object?>, TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1);
        return new MapSignal<IObservedChange<TSender, object?>, TRet>(chains[0], selector);
    }

    /// <summary>Observes 2 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, selector, true);

    /// <summary>Observes 2 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2);
        return CombineLatestObservable.Create(chains[0], chains[1], selector);
    }

    /// <summary>Observes 3 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, selector, true);

    /// <summary>Observes 3 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], selector);
    }

    /// <summary>Observes 4 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, selector, true);

    /// <summary>Observes 4 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], selector);
    }

    /// <summary>Observes 5 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, selector, true);

    /// <summary>Observes 5 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], selector);
    }

    /// <summary>Observes 6 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, selector, true);

    /// <summary>Observes 6 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], selector);
    }

    /// <summary>Observes 7 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, selector, true);

    /// <summary>Observes 7 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6, property7);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], chains[6], selector);
    }

    /// <summary>Observes 8 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, selector, true);

    /// <summary>Observes 8 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6, property7, property8);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], chains[6], chains[7], selector);
    }

    /// <summary>Observes 9 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, selector, true);

    /// <summary>Observes 9 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6, property7, property8, property9);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], chains[6], chains[7], chains[8], selector);
    }

    /// <summary>Observes 10 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, selector, true);

    /// <summary>Observes 10 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6, property7, property8, property9, property10);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], chains[6], chains[7], chains[8], chains[9], selector);
    }

    /// <summary>Observes 11 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, selector, true);

    /// <summary>Observes 11 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], chains[6], chains[7], chains[8], chains[9], chains[10], selector);
    }

    /// <summary>Observes 12 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="property12">An expression naming property 12.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Expression? property12,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, selector, true);

    /// <summary>Observes 12 property chains named by run-time expressions and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="property12">An expression naming property 12.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of selector results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> or <paramref name="selector"/> is null.</exception>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Expression? property12,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        var chains = Chains(sender, selector, isDistinct, property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12);
        return CombineLatestObservable.Create(chains[0], chains[1], chains[2], chains[3], chains[4], chains[5], chains[6], chains[7], chains[8], chains[9], chains[10], chains[11], selector);
    }

    /// <summary>Subscribes to every chain an overload was handed, after refusing a missing argument.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="selector">The projection the caller supplied, which is required.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <param name="properties">The expressions naming the chains.</param>
    /// <returns>One observation per chain, in the order the chains were given.</returns>
    /// <remarks>
    /// Every arity funnels its argument checks and its subscriptions through here, so each overload is the
    /// signature and nothing else. The array is built once when the observable is created rather than per
    /// notification, so a higher arity costs one allocation at subscription and nothing while it reports.
    /// </remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    private static IObservable<IObservedChange<TSender, object?>>[] Chains<TSender>(
        TSender sender,
        object selector,
        bool isDistinct,
        params Expression?[] properties)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var chains = new IObservable<IObservedChange<TSender, object?>>[properties.Length];
        for (var i = 0; i < properties.Length; i++)
        {
            chains[i] = ObserveDynamicChain(sender, properties[i], isDistinct);
        }

        return chains;
    }

    /// <summary>Subscribes to one property chain named by a run-time expression.</summary>
    /// <typeparam name="TSender">The type of the object the chain is rooted on.</typeparam>
    /// <param name="sender">The object the chain is rooted on.</param>
    /// <param name="property">An expression naming the chain.</param>
    /// <param name="isDistinct">Whether the chain reports only when its value changes.</param>
    /// <returns>An observable of the chain's observed changes.</returns>
    /// <remarks>Every arity funnels through here, so the reflection is written once.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<IObservedChange<TSender, object?>> ObserveDynamicChain<TSender>(
        TSender sender,
        Expression? property,
        bool isDistinct)
        where TSender : class =>
        sender.SubscribeToExpressionChain<TSender, object?>(property, false, false, isDistinct);
}
