// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>A person with an account on the server. Each property raises <c>PropertyChanged</c>.</summary>
[System.Diagnostics.DebuggerDisplay("User: {Login}")]
public sealed class User : ObservableObject
{
    /// <summary>Gets or sets the account name, such as <c>priya-nair</c>.</summary>
    public string Login
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the name the person goes by.</summary>
    public string DisplayName
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Creates an independent copy, as the server returns a new object for each response.</summary>
    /// <returns>A copy with the same values.</returns>
    public User Clone() => new() { Login = Login, DisplayName = DisplayName };
}
