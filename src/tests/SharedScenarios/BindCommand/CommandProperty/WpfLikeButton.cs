// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace SharedScenarios.BindCommand.CommandProperty;

/// <summary>A WPF-like button with Command and CommandParameter properties.</summary>
public class WpfLikeButton
{
    /// <summary>Gets or sets the command.</summary>
    public ICommand? Command { get; set; }

    /// <summary>Gets or sets the command parameter.</summary>
    public object? CommandParameter { get; set; }
}
