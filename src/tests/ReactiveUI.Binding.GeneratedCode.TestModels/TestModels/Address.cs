// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>An address model for testing deep property chains. Implements both INotifyPropertyChanged and INotifyPropertyChanging.</summary>
public class Address : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the street.</summary>
    public string Street
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Street)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Street)));
        }
    } = string.Empty;

    /// <summary>Gets or sets the city.</summary>
    public string City
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(City)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(City)));
        }
    } = string.Empty;

    /// <summary>Gets or sets the zip code.</summary>
    public string ZipCode
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(ZipCode)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ZipCode)));
        }
    } = string.Empty;
}
