// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A fixture with 12 string properties for testing multi-property WhenAnyValue overloads.
/// Implements INotifyPropertyChanged.
/// </summary>
public class WhenAnyTestFixture : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets value1.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value1)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value2.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value2)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value3.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value3)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value4.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value4)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value5.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value5)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value6.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value6)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value7.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value7)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value8.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value8)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value9.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value9)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value10.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value10)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value11.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value11)));
        }
    } = string.Empty;

    /// <summary>Gets or sets value12.</summary>
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
            PropertyChanged?.Invoke(this, new(nameof(Value12)));
        }
    } = string.Empty;
}
