// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;

namespace ReactiveUI.Binding.Generator.Benchmarks.Mocks;

/// <summary>A view with the controls the mock bindings write to.</summary>
public class PersonView : IViewFor<PersonViewModel>, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public PersonViewModel? ViewModel
    {
        get => field;
        set => SetField(ref field, value, nameof(ViewModel));
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (PersonViewModel?)value;
    }

    /// <summary>Gets or sets the text showing the name.</summary>
    public string NameText
    {
        get => field;
        set => SetField(ref field, value, nameof(NameText));
    } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the active box is checked.</summary>
    public bool IsActiveValue
    {
        get => field;
        set => SetField(ref field, value, nameof(IsActiveValue));
    }

    /// <summary>Gets the button that runs the save command.</summary>
    public SaveButton SaveButton { get; } = new();

    /// <summary>Writes a property's value, raising the change event after the write.</summary>
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

        storage = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
