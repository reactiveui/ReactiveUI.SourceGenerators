// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenAnyValue.MultiPropertyTwelveProperties;

/// <summary>ViewModel with twelve observable properties (maximum standard overload).</summary>
public class WhenAnyFixture : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Value1"/>.</summary>
    private string _value1 = "1";

    /// <summary>The backing field for <see cref="Value2"/>.</summary>
    private string _value2 = string.Empty;

    /// <summary>The backing field for <see cref="Value3"/>.</summary>
    private string _value3 = "3";

    /// <summary>The backing field for <see cref="Value4"/>.</summary>
    private string _value4 = string.Empty;

    /// <summary>The backing field for <see cref="Value5"/>.</summary>
    private string _value5 = "5";

    /// <summary>The backing field for <see cref="Value6"/>.</summary>
    private string _value6 = string.Empty;

    /// <summary>The backing field for <see cref="Value7"/>.</summary>
    private string _value7 = "7";

    /// <summary>The backing field for <see cref="Value8"/>.</summary>
    private string _value8 = string.Empty;

    /// <summary>The backing field for <see cref="Value9"/>.</summary>
    private string _value9 = "9";

    /// <summary>The backing field for <see cref="Value10"/>.</summary>
    private string _value10 = string.Empty;

    /// <summary>The backing field for <see cref="Value11"/>.</summary>
    private string _value11 = "11";

    /// <summary>The backing field for <see cref="Value12"/>.</summary>
    private string _value12 = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets value 1.</summary>
    public string Value1
    {
        get => _value1;
        set
        {
            if (_value1 == value)
            {
                return;
            }

            _value1 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value1)));
        }
    }

    /// <summary>Gets or sets value 2.</summary>
    public string Value2
    {
        get => _value2;
        set
        {
            if (_value2 == value)
            {
                return;
            }

            _value2 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value2)));
        }
    }

    /// <summary>Gets or sets value 3.</summary>
    public string Value3
    {
        get => _value3;
        set
        {
            if (_value3 == value)
            {
                return;
            }

            _value3 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value3)));
        }
    }

    /// <summary>Gets or sets value 4.</summary>
    public string Value4
    {
        get => _value4;
        set
        {
            if (_value4 == value)
            {
                return;
            }

            _value4 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value4)));
        }
    }

    /// <summary>Gets or sets value 5.</summary>
    public string Value5
    {
        get => _value5;
        set
        {
            if (_value5 == value)
            {
                return;
            }

            _value5 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value5)));
        }
    }

    /// <summary>Gets or sets value 6.</summary>
    public string Value6
    {
        get => _value6;
        set
        {
            if (_value6 == value)
            {
                return;
            }

            _value6 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value6)));
        }
    }

    /// <summary>Gets or sets value 7.</summary>
    public string Value7
    {
        get => _value7;
        set
        {
            if (_value7 == value)
            {
                return;
            }

            _value7 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value7)));
        }
    }

    /// <summary>Gets or sets value 8.</summary>
    public string Value8
    {
        get => _value8;
        set
        {
            if (_value8 == value)
            {
                return;
            }

            _value8 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value8)));
        }
    }

    /// <summary>Gets or sets value 9.</summary>
    public string Value9
    {
        get => _value9;
        set
        {
            if (_value9 == value)
            {
                return;
            }

            _value9 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value9)));
        }
    }

    /// <summary>Gets or sets value 10.</summary>
    public string Value10
    {
        get => _value10;
        set
        {
            if (_value10 == value)
            {
                return;
            }

            _value10 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value10)));
        }
    }

    /// <summary>Gets or sets value 11.</summary>
    public string Value11
    {
        get => _value11;
        set
        {
            if (_value11 == value)
            {
                return;
            }

            _value11 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value11)));
        }
    }

    /// <summary>Gets or sets value 12.</summary>
    public string Value12
    {
        get => _value12;
        set
        {
            if (_value12 == value)
            {
                return;
            }

            _value12 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value12)));
        }
    }
}
