// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if WINUI_TARGET
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.UI.Xaml;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>Observes a WinUI <c>DependencyObject</c> property through its <c>{PropertyName}Property</c> static field or property.</summary>
/// <remarks>
/// A before-change request, or a property with no dependency property, is observed as a plain object property instead.
/// Each notification carries no value; the observer reads the property.
/// </remarks>
[RequiresUnreferencedCode("Uses reflection to find DependencyProperty static fields/properties.")]
public class DependencyObjectObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>Message logged when a caller asks for before-change notifications a DependencyProperty cannot provide.</summary>
    private static readonly CompositeFormat BeforeChangedUnsupportedFormat = CompositeFormat.Parse(
        "[ReactiveUI.Binding.Maui] Tried to bind DO {0}.{1}, but DPs can't do beforeChanged. Binding as POCO object");

    /// <summary>Message logged when the requested property is not backed by a DependencyProperty.</summary>
    private static readonly CompositeFormat DependencyPropertyMissingFormat = CompositeFormat.Parse(
        "[ReactiveUI.Binding.Maui] Tried to bind DO {0}.{1}, but DP doesn't exist. Binding as POCO object");

    /// <summary>Returns the WinUI dependency-object affinity when the type declares a <c>{propertyName}Property</c> static member.</summary>
    /// <param name="type">The type that owns the property.</param>
    /// <param name="propertyName">The property name, without the <c>Property</c> suffix.</param>
    /// <param name="beforeChanged">Ignored.</param>
    /// <returns><see cref="BindingAffinity.WinUiDependencyObject"/> for a <c>DependencyObject</c> type with such a member; otherwise zero.</returns>
    [RequiresUnreferencedCode("Uses reflection to find DependencyProperty.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        if (!typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        {
            return 0;
        }

        return GetDependencyPropertyFetcher(type, propertyName) is null ? 0 : BindingAffinity.WinUiDependencyObject;
    }

    /// <summary>Returns an observable that raises whenever the dependency property changes on <paramref name="sender"/>.</summary>
    /// <param name="sender">The <c>DependencyObject</c> to observe.</param>
    /// <param name="expression">The expression carried on each notification.</param>
    /// <param name="propertyName">The property name, without the <c>Property</c> suffix.</param>
    /// <param name="beforeChanged"><see langword="true"/> observes the property as a plain object property, since a dependency property has no before-change notification.</param>
    /// <param name="suppressWarnings">Passed through to the plain-object observer when it is used.</param>
    /// <returns>An observable that registers a property-changed callback and unregisters it on disposal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="sender"/> is not a <c>DependencyObject</c>.</exception>
    [RequiresUnreferencedCode("Uses reflection to find DependencyProperty.")]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);

        if (sender is not DependencyObject depSender)
        {
            throw new ArgumentException("The sender must be a DependencyObject", nameof(sender));
        }

        var type = sender.GetType();

        if (beforeChanged)
        {
            Debug.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    BeforeChangedUnsupportedFormat,
                    type.FullName,
                    propertyName));

            var ret = new POCOObservableForProperty();
            return ret.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings);
        }

        var dependencyPropertyFetcher = GetDependencyPropertyFetcher(type, propertyName);
        if (dependencyPropertyFetcher is null)
        {
            Debug.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    DependencyPropertyMissingFormat,
                    type.FullName,
                    propertyName));

            var ret = new POCOObservableForProperty();
            return ret.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings);
        }

        return new AnonymousObservable<IObservedChange<object, object?>>(subj =>
        {
            var handler = new DependencyPropertyChangedCallback((_, _) =>
                subj.OnNext(new ObservedChange<object, object?>(sender, expression, default)));

            var dependencyProperty = dependencyPropertyFetcher();
            var token = depSender.RegisterPropertyChangedCallback(dependencyProperty, handler);
            return Scope.Create<DependencyPropertyRegistration>(
                new(depSender, dependencyProperty, token),
                static state => state.Sender.UnregisterPropertyChangedCallback(state.Property, state.Token));
        });
    }

    /// <summary>Walks the type hierarchy to find a static property with the given name.</summary>
    /// <param name="typeInfo">The type info to search.</param>
    /// <param name="propertyName">The property name to find.</param>
    /// <returns>The property info if found; otherwise, null.</returns>
    [RequiresUnreferencedCode("Uses reflection to walk type hierarchy.")]
    internal static PropertyInfo? ActuallyGetProperty(TypeInfo typeInfo, string propertyName)
    {
        var current = typeInfo;
        while (current is not null)
        {
            var ret = current.GetDeclaredProperty(propertyName);
            if (ret is not null && ret.GetMethod?.IsStatic == true)
            {
                return ret;
            }

            current = current.BaseType?.GetTypeInfo();
        }

        return null;
    }

    /// <summary>Walks the type hierarchy to find a static field with the given name.</summary>
    /// <param name="typeInfo">The type info to search.</param>
    /// <param name="propertyName">The field name to find.</param>
    /// <returns>The field info if found; otherwise, null.</returns>
    [RequiresUnreferencedCode("Uses reflection to walk type hierarchy.")]
    internal static FieldInfo? ActuallyGetField(TypeInfo typeInfo, string propertyName)
    {
        var current = typeInfo;
        while (current is not null)
        {
            var ret = current.GetDeclaredField(propertyName);
            if (ret?.IsStatic == true)
            {
                return ret;
            }

            current = current.BaseType?.GetTypeInfo();
        }

        return null;
    }

    /// <summary>Gets a function that returns the DependencyProperty for the given property name.</summary>
    /// <param name="type">The type to search for the DependencyProperty.</param>
    /// <param name="propertyName">The property name whose DependencyProperty to find.</param>
    /// <returns>A function returning the DependencyProperty, or null if not found.</returns>
    [RequiresUnreferencedCode("Uses reflection to find DependencyProperty static fields/properties.")]
    internal static Func<DependencyProperty>? GetDependencyPropertyFetcher(Type type, string propertyName)
    {
        var typeInfo = type.GetTypeInfo();

        // Look for the DependencyProperty attached to this property name
        var pi = ActuallyGetProperty(typeInfo, $"{propertyName}Property");
        if (pi is not null)
        {
            var value = pi.GetValue(null);

            return value is null ? null : () => (DependencyProperty)value;
        }

        var fi = ActuallyGetField(typeInfo, $"{propertyName}Property");
        if (fi is not null)
        {
            var value = fi.GetValue(null);

            return value is null ? null : () => (DependencyProperty)value;
        }

        return null;
    }
}
#endif
