// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Observes properties for code that another source generator writes, with the behaviour of <c>WhenAnyValue</c> and
/// without a generated binding.
/// </summary>
/// <remarks>
/// <para>
/// A source generator never sees another generator's output, so code a generator writes cannot rely on this library's
/// generator to bind a <c>WhenAnyValue</c> call. These methods take what that generator would have read from the call:
/// the property as a lambda, and a delegate that reads it. They use no reflection, and are safe to trim and to publish
/// ahead of time.
/// </para>
/// <para>
/// Hand-written code calls <c>WhenAnyValue</c> instead, which the generator binds.
/// </para>
/// <para>
/// The family observes one property, two properties, or two properties through a selector. <see cref="Then"/> continues a
/// path one property further, so <c>x =&gt; x.A.B</c> is <c>Create(source, x =&gt; x.A, x =&gt; x.A).Then(a =&gt; a.B, a =&gt; a.B)</c>.
/// <see cref="Switch"/> follows a property that holds an observable, as <c>WhenAnyObservable</c> does.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class ObservedProperty
{
    /// <summary>Observes one property: its current value, then each value it changes to, skipping repeats.</summary>
    /// <typeparam name="TSource">The type declaring the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="property">The property, as a lambda that names it: <c>x =&gt; x.Name</c>.</param>
    /// <param name="getter">A delegate that reads the property: <c>x =&gt; x.Name</c>.</param>
    /// <returns>The property's values.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="property"/> does not name a property or field of its parameter.</exception>
    /// <remarks>
    /// A source that implements <see cref="INotifyPropertyChanged"/> is followed through its change notifications.
    /// Another source's value is read once. A registered <see cref="ICreatesObservableForProperty"/> that scores higher
    /// for the type and property takes over, as it does for <c>WhenAnyValue</c>.
    /// </remarks>
    public static IObservable<TValue> Create<TSource, TValue>(
        TSource source,
        Expression<Func<TSource, TValue>> property,
        Func<TSource, TValue> getter)
        where TSource : class
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        return Observe(source, property, getter, true);
    }

    /// <summary>Observes two properties: their current values, then a pair each time either changes.</summary>
    /// <typeparam name="TSource">The type declaring the properties.</typeparam>
    /// <typeparam name="T1">The first property's type.</typeparam>
    /// <typeparam name="T2">The second property's type.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="property1">The first property, as a lambda that names it.</param>
    /// <param name="getter1">A delegate that reads the first property.</param>
    /// <param name="property2">The second property, as a lambda that names it.</param>
    /// <param name="getter2">A delegate that reads the second property.</param>
    /// <returns>The two properties' values.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException">A property lambda does not name a property or field of its parameter.</exception>
    [SuppressMessage("Design", "SST1472:Too many parameters", Justification = "parameter count is inherent to the API/overload under test")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<T1, T2>> Create<TSource, T1, T2>(
        TSource source,
        Expression<Func<TSource, T1>> property1,
        Func<TSource, T1> getter1,
        Expression<Func<TSource, T2>> property2,
        Func<TSource, T2> getter2)
        where TSource : class =>
        Create(source, property1, getter1, property2, getter2, static (value1, value2) => new PropertyValues<T1, T2>(value1, value2));

    /// <summary>Observes two properties and projects each pair of values through a selector.</summary>
    /// <typeparam name="TSource">The type declaring the properties.</typeparam>
    /// <typeparam name="T1">The first property's type.</typeparam>
    /// <typeparam name="T2">The second property's type.</typeparam>
    /// <typeparam name="TResult">The type the selector produces.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="property1">The first property, as a lambda that names it.</param>
    /// <param name="getter1">A delegate that reads the first property.</param>
    /// <param name="property2">The second property, as a lambda that names it.</param>
    /// <param name="getter2">A delegate that reads the second property.</param>
    /// <param name="selector">The projection applied to each pair of values.</param>
    /// <returns>The projected values.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException">A property lambda does not name a property or field of its parameter.</exception>
    [SuppressMessage("Design", "SST1472:Too many parameters", Justification = "parameter count is inherent to the API/overload under test")]
    public static IObservable<TResult> Create<TSource, T1, T2, TResult>(
        TSource source,
        Expression<Func<TSource, T1>> property1,
        Func<TSource, T1> getter1,
        Expression<Func<TSource, T2>> property2,
        Func<TSource, T2> getter2,
        Func<T1, T2, TResult> selector)
        where TSource : class
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(selector);
        return new CombineLatestSignal<T1, T2, TResult>(
            Observe(source, property1, getter1, true),
            Observe(source, property2, getter2, true),
            selector);
    }

    /// <summary>
    /// Continues a path one property further: the property of whatever object <paramref name="parent"/> currently
    /// holds, following each replacement of that object, as <c>WhenAnyValue(x =&gt; x.A.B)</c> does.
    /// </summary>
    /// <typeparam name="TParent">The type of the object the path has reached.</typeparam>
    /// <typeparam name="TValue">The next property's type.</typeparam>
    /// <param name="parent">The values of the path so far.</param>
    /// <param name="property">The next property, as a lambda that names it.</param>
    /// <param name="getter">A delegate that reads the next property.</param>
    /// <returns>The next property's values, skipping repeats. While the path holds null, nothing is produced.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="property"/> does not name a property or field of its parameter.</exception>
    public static IObservable<TValue> Then<TParent, TValue>(
        this IObservable<TParent?> parent,
        Expression<Func<TParent, TValue>> property,
        Func<TParent, TValue> getter)
        where TParent : class
    {
        ArgumentExceptionHelper.ThrowIfNull(parent);
        var name = ReadName(property);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        var link = new SwitchMapSignal<TParent?, TValue>(
            parent,
            owner => owner is null ? ImmutableEmptySignal<TValue>.Instance : Observe(owner, property, name, getter, false));
        return new UniqueSignal<TValue>(link, EqualityComparer<TValue>.Default);
    }

    /// <summary>
    /// Follows a property that holds an observable: produces what the latest observable it holds produces, as
    /// <c>WhenAnyObservable</c> does. While the property holds null, nothing is produced.
    /// </summary>
    /// <typeparam name="TValue">The type the held observables produce.</typeparam>
    /// <param name="source">The values of a property whose type is an observable.</param>
    /// <returns>The values of the observable the property currently holds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <remarks><c>WhenAnyObservable(x =&gt; x.Router!.CurrentViewModel!)</c> is <c>Create(...).Then(...).Switch()</c>.</remarks>
    public static IObservable<TValue> Switch<TValue>(this IObservable<IObservable<TValue>?> source)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        return new SwitchMapSignal<IObservable<TValue>?, TValue>(source, static inner => inner ?? ImmutableEmptySignal<TValue>.Instance);
    }

    /// <summary>Observes one property of one object, choosing the mechanism the way a generated binding does.</summary>
    /// <typeparam name="TSource">The type declaring the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="property">The property, as a lambda that names it.</param>
    /// <param name="getter">A delegate that reads the property.</param>
    /// <param name="distinct">Whether repeats of the same value are skipped.</param>
    /// <returns>The property's values.</returns>
    private static IObservable<TValue> Observe<TSource, TValue>(
        TSource source,
        Expression<Func<TSource, TValue>> property,
        Func<TSource, TValue> getter,
        bool distinct)
        where TSource : class
    {
        var name = ReadName(property);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        return Observe(source, property, name, getter, distinct);
    }

    /// <summary>Observes one property of one object whose name is already read.</summary>
    /// <typeparam name="TSource">The type declaring the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="property">The property, as a lambda that names it; a registered provider is handed its body.</param>
    /// <param name="name">The property's name.</param>
    /// <param name="getter">A delegate that reads the property.</param>
    /// <param name="distinct">Whether repeats of the same value are skipped.</param>
    /// <returns>The property's values.</returns>
    /// <remarks>
    /// A source with change notifications is observed through them at the affinity a generated binding gives
    /// <see cref="INotifyPropertyChanged"/>; any other source is read once. A registered provider that scores higher
    /// takes over in both cases.
    /// </remarks>
    private static IObservable<TValue> Observe<TSource, TValue>(
        TSource source,
        Expression<Func<TSource, TValue>> property,
        string name,
        Func<TSource, TValue> getter,
        bool distinct)
        where TSource : class
    {
        var notifying = source as INotifyPropertyChanged;
        var affinity = notifying is null ? 0 : BindingAffinity.Explicit;
        if (ObservationAffinityChecker.FindHigherAffinityPlugin(source.GetType(), name, affinity, false) is { } plugin)
        {
            return new PluginPropertyObservable<TValue>(plugin, source, property.Body, name, owner => getter((TSource)owner), false, distinct);
        }

        return notifying is null
            ? new UnchangingPropertyObservable<TValue>(getter(source))
            : new PropertyObservable<TValue>(notifying, name, owner => getter((TSource)owner), distinct);
    }

    /// <summary>Reads the name of the property a lambda names.</summary>
    /// <typeparam name="TSource">The lambda's parameter type.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="property">The lambda.</param>
    /// <returns>The property's name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
    /// <exception cref="ArgumentException">The lambda does not name a property or field of its parameter.</exception>
    private static string ReadName<TSource, TValue>(Expression<Func<TSource, TValue>> property)
    {
        ArgumentExceptionHelper.ThrowIfNull(property);
        var body = property.Body is UnaryExpression { NodeType: ExpressionType.Convert } conversion ? conversion.Operand : property.Body;
        return body is MemberExpression { Expression: ParameterExpression } member
            ? member.Member.Name
            : throw new ArgumentException("The lambda must name a property or field of its parameter, as in x => x.Name.", nameof(property));
    }
}
