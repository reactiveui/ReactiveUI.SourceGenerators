// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Identifies a verified native command route.</summary>
internal enum NativeCommandKind
{
    /// <summary>Android View.Click with enabled-state synchronization.</summary>
    AndroidClick = 0,

    /// <summary>UIKit UIControl target/action touch handling.</summary>
    UIKitTouch = 1,

    /// <summary>A UIKit control-specific event.</summary>
    UIKitEvent = 2,

    /// <summary>Cocoa Target and Action properties.</summary>
    AppKitTargetAction = 3,
}
