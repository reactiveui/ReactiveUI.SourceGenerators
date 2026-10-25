// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Identifies Cocoa's supported target/action hosts.</summary>
internal static class AppKitCommandSymbols
{
    /// <summary>Checks the native target property on a supported Cocoa control.</summary>
    /// <param name="control">The concrete control.</param>
    /// <returns>True when its target can be assigned directly.</returns>
    internal static bool CanBind(INamedTypeSymbol control) =>
        IsTargetHost(control) && NativeCommandMembers.HasWritableProperty(control, "Target", "Foundation.NSObject");

    /// <summary>Recognizes the native target/action hierarchies.</summary>
    /// <param name="control">The concrete control.</param>
    /// <returns>True for a supported Cocoa hierarchy.</returns>
    internal static bool IsTargetHost(INamedTypeSymbol control) =>
        NativeCommandMembers.DerivesFrom(control, "AppKit.NSControl")
        || NativeCommandMembers.DerivesFrom(control, "AppKit.NSCell")
        || NativeCommandMembers.DerivesFrom(control, "AppKit.NSMenu")
        || NativeCommandMembers.DerivesFrom(control, "AppKit.NSMenuItem")
        || NativeCommandMembers.DerivesFrom(control, "AppKit.NSToolbarItem");
}
