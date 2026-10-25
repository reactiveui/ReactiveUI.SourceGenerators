// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A view naming the properties the binding dispatch stubs bind against.</summary>
public class DispatchStubView : IViewFor, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public object? ViewModel { get; set; }

    /// <summary>Gets the control a command binding names.</summary>
    public DispatchStubControl Control { get; } = new();

    /// <summary>Gets the control that no registered command binder reaches.</summary>
    public UnclaimedStubControl Surface { get; } = new();

    /// <summary>Gets or sets the bound text.</summary>
    public string Caption
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Caption)));
        }
    } = "a";
}
