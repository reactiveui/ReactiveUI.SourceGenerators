// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Maui;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates the MAUI converters between a boolean and a Visibility.</summary>
public static class MauiVisibilityExamples
{
    /// <summary>Shows a completed-task mark for a finished to-do item and hides it for an open one.</summary>
    public static void ConvertIsDoneToVisibility()
    {
        BooleanToVisibilityTypeConverter converter = new BooleanToVisibilityTypeConverter();

        _ = converter.TryConvert(true, conversionHint: null, out var doneMark);
        Console.WriteLine(doneMark);

        _ = converter.TryConvert(false, BooleanToVisibilityHints.UseHidden, out var hiddenMark);
        Console.WriteLine(hiddenMark);

        _ = converter.TryConvert(true, BooleanToVisibilityHints.Inverse, out var openMark);
        Console.WriteLine(openMark);

        // Output:
        // Visible
        // Hidden
        // Collapsed
    }

    /// <summary>Reads whether a completed-task mark is showing; only Visible counts as showing.</summary>
    public static void ConvertVisibilityToIsDone()
    {
        VisibilityToBooleanTypeConverter converter = new VisibilityToBooleanTypeConverter();

        _ = converter.TryConvert(Microsoft.Maui.Visibility.Visible, conversionHint: null, out var isDone);
        Console.WriteLine(isDone);

        _ = converter.TryConvert(Microsoft.Maui.Visibility.Collapsed, BooleanToVisibilityHints.Inverse, out var isOpen);
        Console.WriteLine(isOpen);

        // Output:
        // True
        // True
    }
}
