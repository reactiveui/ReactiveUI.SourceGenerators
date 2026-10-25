// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Conversion hints, passed as the conversion hint, that change how a boolean maps to a <c>Visibility</c>.</summary>
[Flags]
public enum BooleanToVisibilityHints
{
    /// <summary>True is Visible and false is Collapsed.</summary>
    None = 0,

    /// <summary>Swaps the mapping, so true is not visible and false is Visible.</summary>
    Inverse = 1 << 1,

    /// <summary>Uses Hidden rather than Collapsed as the value that is not visible.</summary>
    UseHidden = 1 << 2,
}
