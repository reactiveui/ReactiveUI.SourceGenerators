// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A fixture observed once while the locator is still empty and again after registration.
/// It is used by no other test, so the factory-resolution cache entry for its properties is
/// created solely by the test that exercises that sequence.
/// </summary>
public class LateRegistrationFixture : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the value observed through the by-name overload.</summary>
    public string Name
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>Gets or sets the value observed through the expression-chain path.</summary>
    /// <remarks>
    /// A resolution that succeeds is memoized for the lifetime of the process, so each test that
    /// observes this fixture before registration needs a property of its own to start from an
    /// unresolved cache entry.
    /// </remarks>
    public string Title
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>Raises the PropertyChanged event.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new(propertyName));
}
