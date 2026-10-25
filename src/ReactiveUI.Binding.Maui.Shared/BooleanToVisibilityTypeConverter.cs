// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if WINUI_TARGET
using Visibility = Microsoft.UI.Xaml.Visibility;
#else
using Visibility = Microsoft.Maui.Visibility;
#endif

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>Converts a <see cref="bool"/> to <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/>; the conversion always succeeds.</summary>
/// <remarks>
/// A <see cref="BooleanToVisibilityHints"/> conversion hint inverts the mapping or, in the MAUI build, selects
/// <c>Hidden</c> for the value that is not visible. Any other hint is treated as
/// <see cref="BooleanToVisibilityHints.None"/>.
/// </remarks>
public sealed class BooleanToVisibilityTypeConverter : BindingTypeConverter<bool, Visibility>
{
    /// <inheritdoc/>
    public override int GetAffinityForObjects() => BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override bool TryConvert(bool from, object? conversionHint, [NotNullWhen(true)] out Visibility result)
    {
        var hint = conversionHint is BooleanToVisibilityHints visibilityHint
            ? visibilityHint
            : BooleanToVisibilityHints.None;

        var value = (hint & BooleanToVisibilityHints.Inverse) != 0 ? !from : from;

#if !WINUI_TARGET
        var notVisible = (hint & BooleanToVisibilityHints.UseHidden) != 0
            ? Visibility.Hidden
            : Visibility.Collapsed;
#else
        const Visibility notVisible = Visibility.Collapsed;
#endif

        result = value ? Visibility.Visible : notVisible;
        return true;
    }
}
