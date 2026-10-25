// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>
/// Extension methods that observe a property, by name or through an expression chain, using the
/// <see cref="ICreatesObservableForProperty"/> registrations from the service locator. Generated code does not call them.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode(
    "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
public static class ReactiveNotifyPropertyChangedMixins
{
    /// <summary>Number of resolved factories kept in the MRU cache.</summary>
    private const int NotifyFactoryCacheSize = 64;

    /// <summary>Appended to the "no factory found" message; the only way to hit it is a misconfigured locator.</summary>
    private const string BrokenLocatorAdvice =
        "This should never happen, your service locator is probably broken. Please make sure you have registered ICreatesObservableForProperty implementations.";

    /// <summary>MRU cache that maps (sender type, property name, before-change flag) to the best <see cref="ICreatesObservableForProperty"/> implementation for that combination.</summary>
    /// <remarks>
    /// The entry is supplied through the context argument rather than looked up here, so only a
    /// resolution that actually found an implementation is ever stored. A failure is deliberately not
    /// stored: the locator can still be empty the first time a property is observed — an observation
    /// running ahead of application startup, or a test suite whose registration belongs to a later
    /// test — and a stored failure would outlive the registration that fixes it, leaving that sender
    /// and property permanently unobservable.
    /// </remarks>
    private static readonly MemoizingMRUCache<
        NotifyFactoryKey,
        ICreatesObservableForProperty>
        NotifyFactoryCache =
            new(
                static (_, resolved) => (ICreatesObservableForProperty)resolved!,
                NotifyFactoryCacheSize);

    /// <summary>Provides ObservableForProperty extension members for <paramref name="item"/>.</summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <param name="item">The source object to observe properties of.</param>
    extension<TSender>(TSender? item)
    {
        /// <summary>Observes a property by name after it changes, emitting its current value first and dropping consecutive equal values.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="propertyName">The property name to observe.</param>
        /// <returns>An observable of the changes for the property.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode(
            "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            string propertyName) =>
            ObservableForProperty<TSender, TValue>(item, propertyName, false, true, true);

        /// <summary>Observes a property by name after it changes, dropping consecutive equal values and optionally skipping the current value emitted on subscription.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="propertyName">The property name to observe.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <returns>An observable of the changes for the property.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode(
            "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            string propertyName,
            bool skipInitial) =>
            ObservableForProperty<TSender, TValue>(item, propertyName, false, skipInitial, true);

        /// <summary>Observes a property by name through the highest-affinity <see cref="ICreatesObservableForProperty"/> registered for the source's runtime type.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="propertyName">The property name to observe.</param>
        /// <param name="beforeChange">If true, the Observable will notify immediately before a property is going to change.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <returns>An observable of the changes for the property; each value is read by reflection from the public instance property, or is the default when that is missing or null.</returns>
        /// <exception cref="ArgumentNullException">The source object or <paramref name="propertyName"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// No registered <see cref="ICreatesObservableForProperty"/> bids a positive affinity for
        /// <paramref name="propertyName"/> on the source object's type.
        /// </exception>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode(
            "Creating Expressions requires unreferenced code because the members being referenced by the Expression may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            string propertyName,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct)
        {
            ArgumentExceptionHelper.ThrowIfNull(item);
            ArgumentExceptionHelper.ThrowIfNull(propertyName);

            var parameter = Expression.Parameter(typeof(TSender), "x");
            Expression expr;
            try
            {
                expr = Expression.Property(parameter, propertyName);
            }
            catch
            {
                expr = parameter;
            }

            var factory = ResolveNotifyFactory(new(item!.GetType(), propertyName, beforeChange))
                          ?? throw new InvalidOperationException(
                              $"Could not find a ICreatesObservableForProperty for {item.GetType()} property {propertyName}. {BrokenLocatorAdvice}");

            static TValue GetCurrentValue(object sender, string name)
            {
                var t = sender.GetType();
                var prop = t.GetProperty(
                    name,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                if (prop is null)
                {
                    return default!;
                }

                var propertyValue = prop.GetValue(sender);
                if (propertyValue is null)
                {
                    return default!;
                }

                return propertyValue is TValue typedValue ? typedValue : (TValue)propertyValue;
            }

            // Single fused sink: emits the initial value (unless skipped), then re-reads and emits on each
            // notification, applying the distinct gate inline.
            var notifications = factory.GetNotificationForProperty(item!, expr, propertyName, beforeChange);
            return new ObservableForPropertySink<TSender, TValue>(
                item!,
                expr,
                notifications,
                () => GetCurrentValue(item!, propertyName),
                skipInitial,
                isDistinct);
        }

        /// <summary>Observes the property an expression points at after it changes, emitting its current value first and dropping consecutive equal values.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="property">An Expression representing the property.</param>
        /// <returns>An observable of the changes for the property.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            Expression<Func<TSender, TValue>> property) =>
            ObservableForProperty(item, property, false, true, true);

        /// <summary>Observes the property an expression points at after it changes, dropping consecutive equal values and optionally skipping the current value emitted on subscription.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="property">An Expression representing the property.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <returns>An observable of the changes for the property.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            Expression<Func<TSender, TValue>> property,
            bool skipInitial) =>
            ObservableForProperty(item, property, false, skipInitial, true);

        /// <summary>Observes the property, or the member chain, an expression points at.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="property">An Expression representing the property.</param>
        /// <param name="beforeChange">If true, the Observable will notify immediately before a property is going to change.</param>
        /// <param name="skipInitial">If true, the Observable will not notify with the initial value.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <returns>An observable of the changes for the last link of the chain.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        public IObservable<IObservedChange<TSender, TValue>> ObservableForProperty<TValue>(
            Expression<Func<TSender, TValue>> property,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct)
        {
            ArgumentExceptionHelper.ThrowIfNull(property);

            return SubscribeToExpressionChain<TSender, TValue>(
                item,
                property.Body,
                beforeChange,
                skipInitial,
                isDistinct);
        }

        /// <summary>Observes an expression's member chain after each link changes, emitting the end value first and dropping consecutive equal values.</summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression) =>
            SubscribeToExpressionChain<TSender, TValue>(item, expression, false, true, true);

        /// <summary>Observes an expression's member chain after each link changes, dropping consecutive equal values and optionally skipping the end value emitted on subscription.</summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression,
            bool skipInitial) =>
            SubscribeToExpressionChain<TSender, TValue>(item, expression, false, skipInitial, true);

        /// <summary>
        /// Observes each property in an expression's member chain, re-subscribing deeper links when an
        /// intermediate value changes, and reports the value at the end of the chain.
        /// </summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <param name="beforeChange">If we are interested in notifications before the property value is changed.</param>
        /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct) =>
            CreateExpressionChain<TSender, TValue>(item, expression, beforeChange, skipInitial, isDistinct, false);

        /// <summary>
        /// Observes each property in an expression's member chain, re-subscribing deeper links when an
        /// intermediate value changes, and reports the value at the end of the chain.
        /// </summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <param name="beforeChange">If we are interested in notifications before the property value is changed.</param>
        /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <param name="suppressWarnings">If set to true, a property that cannot notify is observed quietly.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the observed shape.")]
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<TSender, TValue>> SubscribeToExpressionChain<TValue>(
            Expression? expression,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct,
            bool suppressWarnings) =>
            CreateExpressionChain<TSender, TValue>(item, expression, beforeChange, skipInitial, isDistinct, suppressWarnings);

        /// <summary>Builds the chain observation both overloads hand back.</summary>
        /// <typeparam name="TValue">The end value we want to subscribe to.</typeparam>
        /// <param name="source">The root object of the chain.</param>
        /// <param name="expression">An expression which will point towards the property.</param>
        /// <param name="beforeChange">If we are interested in notifications before the property value is changed.</param>
        /// <param name="skipInitial">If we don't want to get a notification about the default value of the property.</param>
        /// <param name="isDistinct">If set to true, values are filtered with DistinctUntilChanged.</param>
        /// <param name="suppressWarnings">If set to true, a property that cannot notify is observed quietly.</param>
        /// <returns>An observable which notifies about observed changes.</returns>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        private static ExpressionChainSink<TSender, TValue> CreateExpressionChain<TValue>(
            TSender? source,
            Expression? expression,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct,
            bool suppressWarnings)
        {
            // Single fused switching engine: one watcher per link, re-subscribing deeper links when an
            // intermediate value changes, with skip-initial, the non-null-parent filter, the cast to TValue,
            // and the distinct gate applied inline.
            var links = new List<Expression>(Reflection.Rewrite(expression).GetExpressionChain()).ToArray();
            return new(
                source,
                expression,
                links,
                beforeChange,
                skipInitial,
                isDistinct,
                suppressWarnings);
        }

    }

    /// <summary>Creates an observable that tracks nested property changes for a single link in an expression chain.</summary>
    /// <param name="expression">The expression representing the current property in the chain.</param>
    /// <param name="sourceChange">The observed change from the previous link in the chain.</param>
    /// <param name="beforeChange">If <see langword="true"/>, subscribes to before-change notifications.</param>
    /// <returns>An observable of observed changes for the current property link.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static IObservable<IObservedChange<object?, object?>> NestedObservedChanges(
        Expression expression,
        IObservedChange<object?, object?> sourceChange,
        bool beforeChange)
    {
        var kicker = new ObservedChange<object?, object?>(sourceChange.Value, expression, default);

        return sourceChange.Value is null
            ? new ImmediateReturnSignal<IObservedChange<object?, object?>>(kicker)
            : new MapSignal<IObservedChange<object?, object?>, IObservedChange<object?, object?>>(
                new LeadSignal<IObservedChange<object?, object?>>(
                    NotifyForProperty(sourceChange.Value, expression, beforeChange),
                    kicker),
                static IObservedChange<object?, object?> (x) =>
                    new ObservedChange<object?, object?>(x.Sender, x.Expression, x.GetValueOrDefault()));
    }

    /// <summary>
    /// Gets property change notifications for a single property on an object by resolving the
    /// best <see cref="ICreatesObservableForProperty"/> implementation from the service locator.
    /// </summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property to observe.</param>
    /// <param name="beforeChange">If <see langword="true"/>, subscribes to before-change notifications.</param>
    /// <returns>An observable of observed changes for the property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> does not point at a member, so no property name can be resolved from it.</exception>
    /// <exception cref="InvalidOperationException">No registered <see cref="ICreatesObservableForProperty"/> bids a positive affinity for the property on <paramref name="sender"/>'s type.</exception>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IObservable<IObservedChange<object?, object?>> NotifyForProperty(
        object sender,
        Expression expression,
        bool beforeChange) =>
        NotifyForProperty(sender, expression, beforeChange, false);

    /// <summary>
    /// Gets property change notifications for a single property on an object by resolving the
    /// best <see cref="ICreatesObservableForProperty"/> implementation from the service locator.
    /// </summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property to observe.</param>
    /// <param name="beforeChange">If <see langword="true"/>, subscribes to before-change notifications.</param>
    /// <param name="suppressWarnings">If <see langword="true"/>, the plugin stays quiet about a property that cannot notify.</param>
    /// <returns>An observable of observed changes for the property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> does not point at a member, so no property name can be resolved from it.</exception>
    /// <exception cref="InvalidOperationException">No registered <see cref="ICreatesObservableForProperty"/> bids a positive affinity for the property on <paramref name="sender"/>'s type.</exception>
    /// <remarks>
    /// The suppression reaches the plugin rather than being dropped here. A caller that already knows the
    /// property cannot notify - a binding that chose this path deliberately - would otherwise be warned once
    /// per subscription about something it cannot act on.
    /// </remarks>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static IObservable<IObservedChange<object?, object?>> NotifyForProperty(
        object sender,
        Expression expression,
        bool beforeChange,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        var memberInfo = expression.GetMemberInfo() ?? throw new ArgumentException(
            "The expression does not have valid member info",
            nameof(expression));
        var propertyName = memberInfo.Name;
        var result = ResolveNotifyFactory(new(sender.GetType(), propertyName, beforeChange));

        return result switch
        {
            null => throw new InvalidOperationException(
                $"Could not find a ICreatesObservableForProperty for {sender.GetType()} property {propertyName}. {BrokenLocatorAdvice}"),
            _ => result.GetNotificationForProperty(sender, expression, propertyName, beforeChange, suppressWarnings)
        };
    }

    /// <summary>
    /// Gets the highest-affinity <see cref="ICreatesObservableForProperty"/> for a sender type and
    /// property, consulting the service locator only when the combination has not already resolved.
    /// </summary>
    /// <param name="key">The sender type, property name and before-change flag being resolved.</param>
    /// <returns>The best implementation, or <see langword="null"/> when nothing bids a positive affinity.</returns>
    /// <remarks>
    /// The key is three fields wide, past the two registers the JIT would pass it in, so it travels by
    /// reference rather than being copied onto the stack for every resolution.
    /// </remarks>
    private static ICreatesObservableForProperty? ResolveNotifyFactory(
        in NotifyFactoryKey key)
    {
        if (NotifyFactoryCache.TryGet(key, out var memoized))
        {
            return memoized;
        }

        var bestScore = 0;
        ICreatesObservableForProperty? best = null;
        foreach (var candidate in AppLocator.Current.GetServices<ICreatesObservableForProperty>())
        {
            var score = candidate.GetAffinityForObject(key.SenderType, key.PropertyName, key.BeforeChange);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            best = candidate;
        }

        return best is null ? null : NotifyFactoryCache.Get(key, best);
    }
}
