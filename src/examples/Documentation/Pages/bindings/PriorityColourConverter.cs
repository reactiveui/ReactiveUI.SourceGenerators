// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Graphics;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>
/// Turns the priority of a to-do item into the colour of its badge. The conversion hint <c>"dark"</c> picks the
/// palette for a dark theme; any other hint, or none, picks the light palette.
/// </summary>
[System.Diagnostics.DebuggerDisplay("PriorityColourConverter: Priority colour converter")]
public sealed class PriorityColourConverter : BindingTypeConverter<TodoPriority, Color>
{
    /// <summary>The hint that asks for the dark palette.</summary>
    private const string DarkTheme = "dark";

    /// <summary>The affinity that outranks the converters the library registers.</summary>
    private const int OverridingAffinity = 100;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => OverridingAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(TodoPriority from, object? conversionHint, [NotNullWhen(true)] out Color? result)
    {
        bool dark = conversionHint is DarkTheme;

        result = Color.FromArgb(from switch
        {
            TodoPriority.High => dark ? "#EF5350" : "#C62828",
            TodoPriority.Normal => dark ? "#BDBDBD" : "#616161",
            _ => dark ? "#66BB6A" : "#2E7D32",
        });

        return true;
    }
}
