// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Documentation.SourceGenerators;

/// <summary>A screen for <see cref="ProfileViewModel"/>, with a name box and a save button.</summary>
[System.Diagnostics.DebuggerDisplay("ProfileView: NameText = {NameText}")]
public sealed class ProfileView : IViewFor<ProfileViewModel>, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public ProfileViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (ProfileViewModel?)value;
    }

    /// <summary>Gets or sets the text in the name box.</summary>
    public string? NameText
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameText)));
        }
    }

    /// <summary>Gets the save button.</summary>
    public SaveButton Save { get; } = new SaveButton();
}
