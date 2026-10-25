// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>A view model used for AOT validation of source-generated property observation and binding.</summary>
public class AotViewModel : INotifyPropertyChanged, INotifyPropertyChanging
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

    /// <summary>Gets or sets the child view model.</summary>
    public AotChildViewModel Child
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
    } = new();

    /// <summary>Gets or sets the stream a WhenAnyObservable call switches to.</summary>
    public IObservable<string>? Signal
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Signal)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Signal)));
        }
    }

    /// <summary>Gets or sets the command a stream of values is invoked against.</summary>
    public ICommand? Save
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Save)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Save)));
        }
    }
}
