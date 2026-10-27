// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A control no registered command binder reaches.</summary>
/// <remarks>
/// The runtime library ships no command binder of its own, so a control nothing claims is what a command
/// binding meets when the host registered no binder that fits. The binding resolves to nothing rather than
/// faulting, and this control is how that outcome is reached without unregistering anything global.
/// </remarks>
public class UnclaimedStubControl
{
    /// <summary>Gets or sets the control's text.</summary>
    public string Text { get; set; } = "a";
}
