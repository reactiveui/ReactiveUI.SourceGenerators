// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Common affinity scores shared by binding type converters, property observation factories,
/// command binders, and the source generator's observation/command plugins. A higher value
/// indicates a stronger match; zero means the candidate does not apply.
/// <para>
/// <see cref="DefaultInternalTypeConverter"/>, <see cref="DefaultEvent"/>, <see cref="Explicit"/> and
/// <see cref="ExactType"/> carry the same values as ReactiveUI's scores of those names; the platform-specific
/// scores are this library's own.
/// </para>
/// </summary>
public static class BindingAffinity
{
    /// <summary>The fallback affinity for the reflection-based POCO observer (lowest non-zero match).</summary>
    public static readonly int Fallback = 1;

    /// <summary>The affinity returned by the built-in value and string type converters.</summary>
    public static readonly int DefaultInternalTypeConverter = 2;

    /// <summary>The affinity for binding to a type's conventional default event.</summary>
    public static readonly int DefaultEvent = 3;

    /// <summary>The affinity for a WPF <c>DependencyObject</c> dependency-property match.</summary>
    public static readonly int WpfDependencyObject = 4;

    /// <summary>The affinity for an event-enabled control command binding.</summary>
    public static readonly int EventEnabledControl = 4;

    /// <summary>The affinity for an explicit or interface-based match, such as INotifyPropertyChanged or a named event.</summary>
    public static readonly int Explicit = 5;

    /// <summary>The affinity for a WinUI <c>DependencyObject</c> dependency-property match.</summary>
    public static readonly int WinUiDependencyObject = 6;

    /// <summary>The affinity for a WinForms event-based property observation match.</summary>
    public static readonly int WinFormsEvent = 8;

    /// <summary>The affinity for a strong, exact-type match, such as IReactiveObject.</summary>
    public static readonly int ExactType = 10;

    /// <summary>The affinity for an Apple KVO (<c>NSObject</c>) match.</summary>
    public static readonly int Kvo = 15;
}
