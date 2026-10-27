// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A simple view with DisplayName and DisplayAge properties for testing bindings.
/// Implements INotifyPropertyChanged and IViewFor for view-first binding compat aliases.
/// </summary>
public class TestView : INotifyPropertyChanged, IViewFor
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public object? ViewModel { get; set; }

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

    /// <summary>Gets or sets the display age.</summary>
    public int DisplayAge
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(DisplayAge)));
        }
    }
}
