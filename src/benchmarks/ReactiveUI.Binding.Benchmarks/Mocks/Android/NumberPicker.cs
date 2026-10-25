// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Android.Widget;

/// <summary>Provides a typed native value event with no fixture allocation per update.</summary>
public sealed class NumberPicker : Views.View
{
    /// <summary>Occurs after the numeric value changes.</summary>
    public event EventHandler? ValueChanged;

    /// <summary>Gets or sets the observed value.</summary>
    public int Value
    {
        get => field;
        set
        {
            field = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    } = -1;
}
