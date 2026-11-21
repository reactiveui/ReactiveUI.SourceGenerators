// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// Chooses between the observation the generator emitted for one link of a property chain and a registered
/// <see cref="ICreatesObservableForProperty"/> that outranks the mechanism it was built from.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class PluginObservationSource
{
    /// <summary>Returns the registration's observation of a property, or the generated one when none outranks it.</summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="expression">The property as the generated code names it.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked for this link.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="generated">The observation the generator emitted for this link.</param>
    /// <returns>
    /// The generated observation when no registration outranks <paramref name="generatedAffinity"/>; otherwise a
    /// <see cref="PluginPropertyObservable{T}"/> that does not filter consecutive equal values.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="generated"/> is <see langword="null"/>.</exception>
    public static IObservable<T> Choose<T>(
        object source,
        Expression expression,
        string propertyName,
        bool beforeChange,
        int generatedAffinity,
        Func<object, T?> getter,
        IObservable<T> generated)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(generated);

        var plugin = ObservationAffinityChecker.FindHigherAffinityPlugin(
            source.GetType(),
            propertyName,
            generatedAffinity,
            beforeChange);

        return plugin is null
            ? generated
            : new PluginPropertyObservable<T>(plugin, source, expression, propertyName, getter, beforeChange, false);
    }

    /// <summary>Returns the registration's observation of a property, or the generated one when none outranks it.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked for this link.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="expression">Builds the lambda naming the property; called only when a registration wins.</param>
    /// <param name="generated">The observation the generator emitted for this link.</param>
    /// <returns>
    /// The generated observation when no registration outranks <paramref name="generatedAffinity"/>; otherwise a
    /// <see cref="PluginPropertyObservable{T}"/> that does not filter consecutive equal values.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/>, <paramref name="getter"/>, <paramref name="expression"/> or <paramref name="generated"/> is <see langword="null"/>.
    /// </exception>
    public static IObservable<T> Choose<TOwner, T>(
        TOwner source,
        string propertyName,
        int generatedAffinity,
        bool beforeChange,
        Func<TOwner, T?> getter,
        Func<Expression<Func<TOwner, T>>> expression,
        IObservable<T> generated)
        where TOwner : notnull
    {
        ArgumentExceptionHelper.ThrowIfNull(generated);

        var plugin = FindPlugin(source, propertyName, generatedAffinity, beforeChange, getter, expression);
        return plugin is null
            ? generated
            : new PluginPropertyObservable<T>(plugin, source, expression().Body, propertyName, new OwnerGetter<TOwner, T>(getter).Read, beforeChange, false);
    }

    /// <summary>
    /// Returns the registration's observation of a property, without consecutive equal values, or the generated
    /// one when none outranks it.
    /// </summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked for this link.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="expression">Builds the lambda naming the property; called only when a registration wins.</param>
    /// <param name="generated">The observation the generator emitted for this link.</param>
    /// <returns>
    /// The generated observation when no registration outranks <paramref name="generatedAffinity"/>; otherwise a
    /// <see cref="PluginPropertyObservable{T}"/> that filters consecutive equal values.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/>, <paramref name="getter"/>, <paramref name="expression"/> or <paramref name="generated"/> is <see langword="null"/>.
    /// </exception>
    public static IObservable<T> ChooseDistinct<TOwner, T>(
        TOwner source,
        string propertyName,
        int generatedAffinity,
        bool beforeChange,
        Func<TOwner, T?> getter,
        Func<Expression<Func<TOwner, T>>> expression,
        IObservable<T> generated)
        where TOwner : notnull
    {
        ArgumentExceptionHelper.ThrowIfNull(generated);

        var plugin = FindPlugin(source, propertyName, generatedAffinity, beforeChange, getter, expression);
        return plugin is null
            ? generated
            : new PluginPropertyObservable<T>(plugin, source, expression().Body, propertyName, new OwnerGetter<TOwner, T>(getter).Read, beforeChange, true);
    }

    /// <summary>Finds a registration that outranks the generated observation of a property.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="expression">Builds the lambda naming the property.</param>
    /// <returns>The registration, or <see langword="null"/> when the generated observation ranks at least as high.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="getter"/> or <paramref name="expression"/> is <see langword="null"/>.</exception>
    private static ICreatesObservableForProperty? FindPlugin<TOwner, T>(
        TOwner source,
        string propertyName,
        int generatedAffinity,
        bool beforeChange,
        Func<TOwner, T?> getter,
        Func<Expression<Func<TOwner, T>>> expression)
        where TOwner : notnull
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        ArgumentExceptionHelper.ThrowIfNull(expression);

        return ObservationAffinityChecker.FindHigherAffinityPlugin(source.GetType(), propertyName, generatedAffinity, beforeChange);
    }

    /// <summary>Reads a property through its typed accessor for a registration, which hands its source over untyped.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="getter">Reads the current property value from the source.</param>
    private sealed class OwnerGetter<TOwner, T>(Func<TOwner, T?> getter)
    {
        /// <summary>Reads the property.</summary>
        /// <param name="source">The object the property is read from.</param>
        /// <returns>The property's value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Read(object source) => getter((TOwner)source);
    }
}
