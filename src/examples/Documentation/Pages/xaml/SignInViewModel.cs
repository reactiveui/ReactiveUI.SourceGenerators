// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace ReactiveUI.Binding.Documentation.Xaml;

/// <summary>A sign-in form: a user name, a status line, and a command that signs in.</summary>
[System.Diagnostics.DebuggerDisplay("SignInViewModel: UserName = {UserName}, Status = {Status}")]
public sealed class SignInViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the user name.</summary>
    public string? UserName
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserName)));
        }
    }

    /// <summary>Gets the status line.</summary>
    public string? Status
    {
        get;
        private set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    /// <summary>Gets the command that signs in as <see cref="UserName"/>.</summary>
    public ICommand SignIn => field ??= new Command(() => Status = $"Signed in as {UserName}");
}
