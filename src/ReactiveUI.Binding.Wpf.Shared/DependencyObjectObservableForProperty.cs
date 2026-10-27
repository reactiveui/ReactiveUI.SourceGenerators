// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Windows;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Observes a WPF <c>DependencyObject</c> property through its <c>{PropertyName}Property</c> dependency property.</summary>
/// <remarks>
/// The observable raises after the value changes, and each notification carries no value; the observer reads the property.
/// A before-change request receives the same after-change observable.
/// </remarks>
public class DependencyObjectObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>Returns the WPF dependency-object affinity when the type declares a public static <c>{propertyName}Property</c> field.</summary>
    /// <param name="type">The type that owns the property.</param>
    /// <param name="propertyName">The property name, without the <c>Property</c> suffix.</param>
    /// <param name="beforeChanged">Ignored.</param>
    /// <returns><see cref="BindingAffinity.WpfDependencyObject"/> for a <c>DependencyObject</c> type with such a field; otherwise zero.</returns>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        if (!typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        {
            return 0;
        }

        return GetDependencyProperty(type, propertyName) is not null ? BindingAffinity.WpfDependencyObject : 0;
    }

    /// <summary>Returns an observable that raises whenever the dependency property changes on <paramref name="sender"/>.</summary>
    /// <param name="sender">The <c>DependencyObject</c> to observe.</param>
    /// <param name="expression">The expression carried on each notification.</param>
    /// <param name="propertyName">The property name, without the <c>Property</c> suffix.</param>
    /// <param name="beforeChanged">Ignored; notifications are always after the change.</param>
    /// <param name="suppressWarnings"><see langword="true"/> to skip the debug message written when no descriptor is found.</param>
    /// <returns>An observable that subscribes to the descriptor's value-changed event and unsubscribes on disposal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> is null.</exception>
    /// <exception cref="ArgumentException">The type declares no dependency property for <paramref name="propertyName"/>.</exception>
    /// <exception cref="InvalidOperationException">WPF supplies no descriptor for the dependency property.</exception>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        System.Linq.Expressions.Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);

        var type = sender.GetType();

        var dependencyProperty = GetDependencyProperty(type, propertyName) ?? throw new ArgumentException(
            $"The property {propertyName} does not have a dependency property.",
            nameof(propertyName));
        var dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(dependencyProperty, type);

        if (dependencyPropertyDescriptor is null)
        {
            if (!suppressWarnings)
            {
                Debug.WriteLine(
                    $"[ReactiveUI.Binding.Wpf] Error: Couldn't find dependency property {propertyName} on {type.Name}");
            }

            throw new InvalidOperationException($"Couldn't find dependency property {propertyName} on {type.Name}");
        }

        return new AnonymousObservable<IObservedChange<object, object?>>(subj =>
        {
            var handler = new EventHandler((_, _) =>
                subj.OnNext(new ObservedChange<object, object?>(sender, expression, default)));

            dependencyPropertyDescriptor.AddValueChanged(sender, handler);
            return Scope.Create<DependencyPropertySubscription>(
                new(dependencyPropertyDescriptor, sender, handler),
                static state => state.Descriptor.RemoveValueChanged(state.Sender, state.Handler));
        });
    }

    /// <summary>Retrieves the <see cref="DependencyProperty"/> associated with the specified property name on the given type, if available.</summary>
    /// <param name="type">The type to search for the dependency property.</param>
    /// <param name="propertyName">The name of the property for which the dependency property is sought.</param>
    /// <returns>
    /// The <see cref="DependencyProperty"/> associated with the specified property name,
    /// or <c>null</c> if no matching dependency property is found.
    /// </returns>
    internal static DependencyProperty? GetDependencyProperty(Type type, string propertyName)
    {
        var fi = Array.Find(
            type.GetTypeInfo().GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public),
            x => x.Name == $"{propertyName}Property" && x.IsStatic);

        return (DependencyProperty?)fi?.GetValue(null);
    }
}
