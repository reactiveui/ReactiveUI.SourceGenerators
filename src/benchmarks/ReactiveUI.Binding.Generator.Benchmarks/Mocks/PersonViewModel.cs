// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace ReactiveUI.Binding.Generator.Benchmarks.Mocks;

/// <summary>A view model with the property shapes the mock bindings read: text, a number, a flag, a nested object and a command.</summary>
public class PersonViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the person's name.</summary>
    public string Name
    {
        get => field;
        set => SetField(ref field, value, nameof(Name));
    } = string.Empty;

    /// <summary>Gets or sets the person's age.</summary>
    public int Age
    {
        get => field;
        set => SetField(ref field, value, nameof(Age));
    }

    /// <summary>Gets or sets a value indicating whether the person is active.</summary>
    public bool IsActive
    {
        get => field;
        set => SetField(ref field, value, nameof(IsActive));
    }

    /// <summary>Gets or sets the person's address.</summary>
    public AddressViewModel Address
    {
        get => field;
        set => SetField(ref field, value, nameof(Address));
    } = new();

    /// <summary>Gets or sets the command that saves the person.</summary>
    public ICommand? Save
    {
        get => field;
        set => SetField(ref field, value, nameof(Save));
    }

    /// <summary>Writes a property's value, raising the change events around the write.</summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="storage">The property's backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property being written.</param>
    private void SetField<T>(ref T storage, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return;
        }

        PropertyChanging?.Invoke(this, new(propertyName));
        storage = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
