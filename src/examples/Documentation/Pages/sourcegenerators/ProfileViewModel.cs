// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace ReactiveUI.Binding.Documentation.SourceGenerators;

/// <summary>
/// A view model whose properties and command ReactiveUI.SourceGenerators writes: <c>DisplayName</c> from a field marked
/// <c>[Reactive]</c>, and <c>SaveCommand</c> from a method marked <c>[ReactiveCommand]</c>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ProfileViewModel: DisplayName = {DisplayName}, Saves = {Saves}")]
public partial class ProfileViewModel : ReactiveObject
{
    /// <summary>The name shown for the profile, behind the generated <c>DisplayName</c> property.</summary>
    [Reactive]
    private string _displayName = "Ada";

    /// <summary>Gets how many times the profile was saved.</summary>
    public int Saves { get; private set; }

    /// <summary>Saves the profile, behind the generated <c>SaveCommand</c> property.</summary>
    [ReactiveCommand]
    private void Save() => Saves++;
}
