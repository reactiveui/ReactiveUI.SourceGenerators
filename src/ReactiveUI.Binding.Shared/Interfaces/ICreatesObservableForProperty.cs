// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Provides property change notifications for the types it supports; the registration with the highest affinity for
/// a type and property is the one used.
/// </summary>
public interface ICreatesObservableForProperty
{
    /// <summary>Returns how well this implementation can observe a property of a type.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property name being observed.</param>
    /// <param name="beforeChanged">Whether before-change (PropertyChanging) is requested.</param>
    /// <returns>A positive score when the property can be observed, where a higher score wins; zero or negative when it cannot.</returns>
    int GetAffinityForObject(Type type, string propertyName, bool beforeChanged);

    /// <summary>Creates an observable that emits each time the specified property changes.</summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="beforeChanged">Whether to observe before-change events.</param>
    /// <param name="suppressWarnings">Whether to suppress the warning written for a property that cannot notify.</param>
    /// <returns>An observable of observed changes; callers read the current value from <paramref name="sender"/> rather than from the emitted change.</returns>
    IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings);
}
