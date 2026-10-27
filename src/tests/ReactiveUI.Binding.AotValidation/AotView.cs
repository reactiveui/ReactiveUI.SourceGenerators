// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>A view used for AOT binding validation.</summary>
public class AotView : INotifyPropertyChanged, IViewFor<AotViewModel>
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the view model this view displays.</summary>
    public AotViewModel? ViewModel
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewModel)));
        }
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (AotViewModel?)value;
    }

    /// <summary>Gets the button a command binds to.</summary>
    public AotButton SaveButton { get; } = new();

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(DisplayName)));
        }
    } = string.Empty;
}
