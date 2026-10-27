// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding;

namespace SharedScenarios.BindInteraction.ObservableHandler;

/// <summary>ViewModel exposing a string-to-bool interaction.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Confirm"/>.</summary>
    private Interaction<string, bool> _confirm = new Interaction<string, bool>();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the confirmation interaction.</summary>
    public Interaction<string, bool> Confirm
    {
        get => _confirm;
        set
        {
            if (_confirm == value)
            {
                return;
            }

            _confirm = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Confirm)));
        }
    }
}
