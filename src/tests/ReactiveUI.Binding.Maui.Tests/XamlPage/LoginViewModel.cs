// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace ReactiveUI.Binding.Maui.Tests.XamlPage;

/// <summary>A view model for <see cref="LoginPage"/>.</summary>
[System.Diagnostics.DebuggerDisplay("LoginViewModel: UserName = {UserName}, Status = {Status}")]
public sealed class LoginViewModel : INotifyPropertyChanged
{
    /// <summary>Initializes a new instance of the <see cref="LoginViewModel"/> class.</summary>
    public LoginViewModel() => Login = new Command(() => Status = $"Welcome {UserName}");

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

    /// <summary>Gets the status shown on the page.</summary>
    public string? Status
    {
        get;
        private set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    /// <summary>Gets the command that logs in.</summary>
    public ICommand Login { get; }
}
