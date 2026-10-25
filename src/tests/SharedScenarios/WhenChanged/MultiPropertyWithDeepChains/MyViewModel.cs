// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanged.MultiPropertyWithDeepChains;

/// <summary>ViewModel with a shallow property and a deep property chain.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Name"/>.</summary>
    private string _name = string.Empty;

    /// <summary>The backing field for <see cref="Address"/>.</summary>
    private AddressModel _address = new AddressModel();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the name.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    /// <summary>Gets or sets the address.</summary>
    public AddressModel Address
    {
        get => _address;
        set
        {
            if (_address == value)
            {
                return;
            }

            _address = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
        }
    }
}
