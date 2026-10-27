// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Identifies UIKit's touch and control-specific command routes.</summary>
internal static class UIKitCommandSymbols
{
    /// <summary>Checks the native touch target methods and enabled property.</summary>
    /// <param name="control">The concrete control.</param>
    /// <returns>True when UIKit can attach a typed touch handler.</returns>
    internal static bool CanBindTouch(INamedTypeSymbol control) =>
        NativeCommandMembers.DerivesFrom(control, "UIKit.UIControl")
        && NativeCommandMembers.HasWritableProperty(control, "Enabled", "bool")
        && NativeCommandMembers.HasTargetMethod(control, "AddTarget")
        && NativeCommandMembers.HasTargetMethod(control, "RemoveTarget");

    /// <summary>Finds the verified specialized command event.</summary>
    /// <param name="control">The concrete control.</param>
    /// <returns>The specialized event name, or null.</returns>
    internal static string? ControlEvent(INamedTypeSymbol control)
    {
        string? name = null;
        if (NativeCommandMembers.DerivesFrom(control, "UIKit.UIRefreshControl"))
        {
            name = "ValueChanged";
        }
        else if (NativeCommandMembers.DerivesFrom(control, "UIKit.UIBarButtonItem"))
        {
            name = "Clicked";
        }

        return name is not null && NativeCommandMembers.HasWritableProperty(control, "Enabled", "bool") && NativeCommandMembers.HasEvent(control, name)
            ? name
            : null;
    }
}
