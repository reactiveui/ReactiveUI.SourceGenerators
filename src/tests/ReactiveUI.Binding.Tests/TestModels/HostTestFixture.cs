// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A host test fixture that contains nested observable and non-observable child objects.
/// Used for testing deep property chains.
/// </summary>
public class HostTestFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the child test fixture.</summary>
    public TestFixture? Child
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            OnPropertyChanging();
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets some other parameter.</summary>
    public int SomeOtherParam
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            OnPropertyChanging();
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the non-observable child.</summary>
    public NonObservableTestFixture? PocoChild
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            OnPropertyChanging();
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Raises the PropertyChanging event.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new(propertyName));

    /// <summary>Raises the PropertyChanged event.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new(propertyName));
}
