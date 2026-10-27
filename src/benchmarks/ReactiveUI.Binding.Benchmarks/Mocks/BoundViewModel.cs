// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Provides replaceable commands and interactions with cached property notifications.</summary>
public sealed class BoundViewModel : INotifyPropertyChanged
{
    /// <summary>The command replacement notification.</summary>
    private static readonly PropertyChangedEventArgs CommandChanged = new(nameof(Command));

    /// <summary>The interaction replacement notification.</summary>
    private static readonly PropertyChangedEventArgs InteractionChanged = new(nameof(Question));

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the command the view's control receives.</summary>
    public ICommand? Command
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, CommandChanged);
        }
    }

    /// <summary>Gets or sets the interaction whose handler belongs to the view.</summary>
    public Interaction<int, int> Question
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, InteractionChanged);
        }
    } = new();
}
