// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Avalonia;
#else
namespace ReactiveUI.Binding.Avalonia;
#endif

/// <summary>Observes a property of an <c>AvaloniaObject</c> through the <c>AvaloniaProperty</c> registered under its name.</summary>
/// <remarks>
/// The observable raises after the value changes, and each notification carries the new value. Avalonia raises no
/// before-change notification, so a before-change request receives the same after-change observable.
/// </remarks>
public sealed class AvaloniaObjectObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>The affinity of a registered Avalonia property, the same as a WPF dependency property's.</summary>
    private static readonly int AvaloniaPropertyAffinity = BindingAffinity.WpfDependencyObject;

    /// <summary>Returns the Avalonia property affinity when the type registers an Avalonia property with the given name.</summary>
    /// <param name="type">The type that owns the property.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="beforeChanged">Ignored.</param>
    /// <returns>The Avalonia property affinity for an <c>AvaloniaObject</c> type that registers the property; otherwise zero.</returns>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        if (!typeof(AvaloniaObject).IsAssignableFrom(type))
        {
            return 0;
        }

        return FindProperty(type, propertyName) is not null ? AvaloniaPropertyAffinity : 0;
    }

    /// <summary>Returns an observable that raises whenever the Avalonia property changes on <paramref name="sender"/>.</summary>
    /// <param name="sender">The <c>AvaloniaObject</c> to observe.</param>
    /// <param name="expression">The expression carried on each notification.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="beforeChanged">Ignored; notifications are always after the change.</param>
    /// <param name="suppressWarnings"><see langword="true"/> to skip the debug message written when no property is found.</param>
    /// <returns>An observable that attaches to the object's property-changed event and detaches on disposal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="sender"/> is not an <c>AvaloniaObject</c>.</exception>
    /// <exception cref="MissingMemberException">The type registers no Avalonia property named <paramref name="propertyName"/>.</exception>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        System.Linq.Expressions.Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);

        if (sender is not AvaloniaObject avaloniaObject)
        {
            throw new ArgumentException("The sender is not an AvaloniaObject.", nameof(sender));
        }

        var type = sender.GetType();
        var property = FindProperty(type, propertyName);
        if (property is null)
        {
            if (!suppressWarnings)
            {
                Debug.WriteLine($"[ReactiveUI.Binding.Avalonia] Error: Couldn't find Avalonia property {propertyName} on {type.Name}");
            }

            throw new MissingMemberException(type.FullName, propertyName);
        }

        return new AnonymousObservable<IObservedChange<object, object?>>(observer =>
        {
            EventHandler<AvaloniaPropertyChangedEventArgs> handler = (_, args) =>
            {
                if (args.Property != property)
                {
                    return;
                }

                observer.OnNext(new ObservedChange<object, object?>(sender, expression, args.NewValue));
            };

            avaloniaObject.PropertyChanged += handler;
            return Scope.Create<AvaloniaPropertySubscription>(
                new(avaloniaObject, handler),
                static state => state.Sender.PropertyChanged -= state.Handler);
        });
    }

    /// <summary>Finds the Avalonia property registered on <paramref name="type"/> under <paramref name="propertyName"/>.</summary>
    /// <param name="type">The type to search.</param>
    /// <param name="propertyName">The property name; the comparison is case-sensitive.</param>
    /// <returns>The registered property, or null when the type registers none by that name.</returns>
    private static AvaloniaProperty? FindProperty(Type type, string propertyName)
    {
        var registered = AvaloniaPropertyRegistry.Instance.GetRegistered(type);
        for (var i = 0; i < registered.Count; i++)
        {
            if (string.Equals(registered[i].Name, propertyName, StringComparison.Ordinal))
            {
                return registered[i];
            }
        }

        return null;
    }
}
