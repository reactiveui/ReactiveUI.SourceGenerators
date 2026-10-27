// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A test fixture with 12 value properties for multi-property WhenAnyValue testing.</summary>
public class WhenAnyTestFixture : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets value 1.</summary>
    public string Value1
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

    /// <summary>Gets or sets value 2.</summary>
    public string Value2
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

    /// <summary>Gets or sets value 3.</summary>
    public string Value3
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

    /// <summary>Gets or sets value 4.</summary>
    public string Value4
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

    /// <summary>Gets or sets value 5.</summary>
    public string Value5
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

    /// <summary>Gets or sets value 6.</summary>
    public string Value6
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

    /// <summary>Gets or sets value 7.</summary>
    public string Value7
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

    /// <summary>Gets or sets value 8.</summary>
    public string Value8
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

    /// <summary>Gets or sets value 9.</summary>
    public string Value9
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

    /// <summary>Gets or sets value 10.</summary>
    public string Value10
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

    /// <summary>Gets or sets value 11.</summary>
    public string Value11
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

    /// <summary>Gets or sets value 12.</summary>
    public string Value12
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
