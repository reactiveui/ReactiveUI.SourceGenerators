// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A test fixture implementing INotifyPropertyChanged and INotifyPropertyChanging
/// with various property types for comprehensive testing.
/// </summary>
public class TestFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets a non-null string property.</summary>
    public string IsNotNullString
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
    } = "Foo";

    /// <summary>Gets or sets a single-word string property.</summary>
    public string IsOnlyOneWord
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
    } = "Baz";

    /// <summary>Gets or sets a property without change notification behavior.</summary>
    public string PocoProperty
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
    } = string.Empty;

    /// <summary>Gets or sets a nullable integer property.</summary>
    public int? NullableInt
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

    /// <summary>Gets or sets a property that uses expression-based raise and set.</summary>
    public string UsesExprRaiseSet
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
    } = string.Empty;

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
