// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Converts a <see cref="Visibility"/> to a <see cref="bool"/>, true for Visible and false for Hidden or Collapsed; the conversion always succeeds.</summary>
/// <remarks>
/// A <see cref="BooleanToVisibilityHints.Inverse"/> conversion hint inverts the result. Any other hint is treated as
/// <see cref="BooleanToVisibilityHints.None"/>.
/// </remarks>
public sealed class VisibilityToBooleanTypeConverter : BindingTypeConverter<Visibility, bool>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override bool TryConvert(Visibility from, object? conversionHint, [NotNullWhen(true)] out bool result)
    {
        var hint = conversionHint is BooleanToVisibilityHints visibilityHint
            ? visibilityHint
            : BooleanToVisibilityHints.None;

        var isVisible = from == Visibility.Visible;
        result = (hint & BooleanToVisibilityHints.Inverse) != 0 ? !isVisible : isVisible;
        return true;
    }
}
