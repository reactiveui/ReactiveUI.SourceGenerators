// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view with 16 properties matching BigViewModel types for testing bindings.
/// Implements INotifyPropertyChanged only (views don't need INotifyPropertyChanging).
/// </summary>
public class BigView : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets view prop1 (string).</summary>
    public string ViewProp1
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp1)));
        }
    } = string.Empty;

    /// <summary>Gets or sets view prop2 (int).</summary>
    public int ViewProp2
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp2)));
        }
    }

    /// <summary>Gets or sets view prop3 (double).</summary>
    public double ViewProp3
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp3)));
        }
    }

    /// <summary>Gets or sets a value indicating whether view prop4 is true.</summary>
    public bool ViewProp4
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp4)));
        }
    }

    /// <summary>Gets or sets view prop5 (string).</summary>
    public string ViewProp5
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp5)));
        }
    } = string.Empty;

    /// <summary>Gets or sets view prop6 (int).</summary>
    public int ViewProp6
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp6)));
        }
    }

    /// <summary>Gets or sets view prop7 (double).</summary>
    public double ViewProp7
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp7)));
        }
    }

    /// <summary>Gets or sets a value indicating whether view prop8 is true.</summary>
    public bool ViewProp8
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp8)));
        }
    }

    /// <summary>Gets or sets view prop9 (string).</summary>
    public string ViewProp9
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp9)));
        }
    } = string.Empty;

    /// <summary>Gets or sets view prop10 (int).</summary>
    public int ViewProp10
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp10)));
        }
    }

    /// <summary>Gets or sets view prop11 (double).</summary>
    public double ViewProp11
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp11)));
        }
    }

    /// <summary>Gets or sets a value indicating whether view prop12 is true.</summary>
    public bool ViewProp12
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp12)));
        }
    }

    /// <summary>Gets or sets view prop13 (string).</summary>
    public string ViewProp13
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp13)));
        }
    } = string.Empty;

    /// <summary>Gets or sets view prop14 (int).</summary>
    public int ViewProp14
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp14)));
        }
    }

    /// <summary>Gets or sets view prop15 (double).</summary>
    public double ViewProp15
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp15)));
        }
    }

    /// <summary>Gets or sets a value indicating whether view prop16 is true.</summary>
    public bool ViewProp16
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewProp16)));
        }
    }
}
