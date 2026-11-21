// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// A dependency object with a read-only dependency property, registered the way ReactiveUI #4547 does: the public
/// field is assigned from the key in the static constructor, and only the key can write the value.
/// </summary>
[DebuggerDisplay("ReadOnlyPropertyButton: IsButtonEnabled = {IsButtonEnabled}")]
public sealed class ReadOnlyPropertyButton : DependencyObject
{
    /// <summary>Identifies the read-only <see cref="IsButtonEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsButtonEnabledProperty;

    /// <summary>The key that writes <see cref="IsButtonEnabled"/>.</summary>
    internal static readonly DependencyPropertyKey IsButtonEnabledPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsButtonEnabled), typeof(bool), typeof(ReadOnlyPropertyButton), new(false));

    /// <summary>Initializes static members of the <see cref="ReadOnlyPropertyButton"/> class.</summary>
    static ReadOnlyPropertyButton() => IsButtonEnabledProperty = IsButtonEnabledPropertyKey.DependencyProperty;

    /// <summary>Gets a value indicating whether the button is enabled.</summary>
    public bool IsButtonEnabled
    {
        get => (bool)GetValue(IsButtonEnabledPropertyKey.DependencyProperty);
        private set => SetValue(IsButtonEnabledPropertyKey, value);
    }

    /// <summary>Writes <see cref="IsButtonEnabled"/> through its key.</summary>
    /// <param name="enabled">The value to write.</param>
    public void Enable(bool enabled) => IsButtonEnabled = enabled;
}
