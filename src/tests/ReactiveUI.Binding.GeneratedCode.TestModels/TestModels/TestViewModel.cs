// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A simple view model with Name and Age properties for testing WhenChanged/WhenChanging.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class TestViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the name.</summary>
    public string Name
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Name)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Name)));
        }
    } = string.Empty;

    /// <summary>Gets or sets the age.</summary>
    public int Age
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Age)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Age)));
        }
    }

    /// <summary>Gets or sets the nested child used by three-link observation tests.</summary>
    public TestViewModel? Child
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Child)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Child)));
        }
    }
}
