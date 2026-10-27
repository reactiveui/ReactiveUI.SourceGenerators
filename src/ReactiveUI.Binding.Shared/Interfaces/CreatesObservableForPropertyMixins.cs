// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Overloads of <see cref="ICreatesObservableForProperty"/> members that default the before-change and warning flags to <see langword="false"/>.</summary>
public static class CreatesObservableForPropertyMixins
{
    /// <summary>Provides GetAffinityForObject extension members for <paramref name="factory"/>.</summary>
    /// <param name="factory">The observation factory.</param>
    extension(ICreatesObservableForProperty factory)
    {
        /// <summary>Returns the affinity for after-change observation of the specified property.</summary>
        /// <param name="type">The type being observed.</param>
        /// <param name="propertyName">The property name being observed.</param>
        /// <returns>The affinity score. Positive means supported.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
        public int GetAffinityForObject(Type type, string propertyName)
        {
            ArgumentExceptionHelper.ThrowIfNull(factory);
            return factory.GetAffinityForObject(type, propertyName, false);
        }

        /// <summary>Creates an observable that fires after the specified property changes, without suppressing warnings.</summary>
        /// <param name="sender">The object to observe.</param>
        /// <param name="expression">The expression identifying the property.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>An observable of observed changes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            Expression expression,
            string propertyName)
        {
            ArgumentExceptionHelper.ThrowIfNull(factory);
            return factory.GetNotificationForProperty(sender, expression, propertyName, false, false);
        }

        /// <summary>Creates an observable that fires when the specified property changes, without suppressing warnings.</summary>
        /// <param name="sender">The object to observe.</param>
        /// <param name="expression">The expression identifying the property.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="beforeChanged">Whether to observe before-change events.</param>
        /// <returns>An observable of observed changes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            Expression expression,
            string propertyName,
            bool beforeChanged)
        {
            ArgumentExceptionHelper.ThrowIfNull(factory);
            return factory.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, false);
        }
    }
}
