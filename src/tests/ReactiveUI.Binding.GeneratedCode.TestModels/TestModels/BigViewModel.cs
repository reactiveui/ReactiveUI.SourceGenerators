// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view model with 16 properties of mixed types plus a nested Address for testing
/// multi-property overloads and deep property chains.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class BigViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>Seed for <see cref="Prop2"/>; each seed is the property's own ordinal so no two values collide.</summary>
    private const int Prop2Seed = 2;

    /// <summary>Seed for <see cref="Prop3"/>.</summary>
    private const double Prop3Seed = 3.0;

    /// <summary>Seed for <see cref="Prop6"/>.</summary>
    private const int Prop6Seed = 6;

    /// <summary>Seed for <see cref="Prop7"/>.</summary>
    private const double Prop7Seed = 7.0;

    /// <summary>Seed for <see cref="Prop10"/>.</summary>
    private const int Prop10Seed = 10;

    /// <summary>Seed for <see cref="Prop11"/>.</summary>
    private const double Prop11Seed = 11.0;

    /// <summary>Seed for <see cref="Prop14"/>.</summary>
    private const int Prop14Seed = 14;

    /// <summary>Seed for <see cref="Prop15"/>.</summary>
    private const double Prop15Seed = 15.0;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets prop1 (string).</summary>
    public string Prop1
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop1)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop1)));
        }
    } = string.Empty;

    /// <summary>Gets or sets prop2 (int).</summary>
    public int Prop2
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop2)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop2)));
        }
    }

    /// <summary>Gets or sets prop3 (double).</summary>
    public double Prop3
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop3)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop3)));
        }
    }

    /// <summary>Gets or sets a value indicating whether prop4 is true.</summary>
    public bool Prop4
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop4)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop4)));
        }
    }

    /// <summary>Gets or sets prop5 (string).</summary>
    public string Prop5
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop5)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop5)));
        }
    } = string.Empty;

    /// <summary>Gets or sets prop6 (int).</summary>
    public int Prop6
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop6)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop6)));
        }
    }

    /// <summary>Gets or sets prop7 (double).</summary>
    public double Prop7
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop7)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop7)));
        }
    }

    /// <summary>Gets or sets a value indicating whether prop8 is true.</summary>
    public bool Prop8
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop8)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop8)));
        }
    }

    /// <summary>Gets or sets prop9 (string).</summary>
    public string Prop9
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop9)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop9)));
        }
    } = string.Empty;

    /// <summary>Gets or sets prop10 (int).</summary>
    public int Prop10
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop10)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop10)));
        }
    }

    /// <summary>Gets or sets prop11 (double).</summary>
    public double Prop11
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop11)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop11)));
        }
    }

    /// <summary>Gets or sets a value indicating whether prop12 is true.</summary>
    public bool Prop12
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop12)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop12)));
        }
    }

    /// <summary>Gets or sets prop13 (string).</summary>
    public string Prop13
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop13)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop13)));
        }
    } = string.Empty;

    /// <summary>Gets or sets prop14 (int).</summary>
    public int Prop14
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop14)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop14)));
        }
    }

    /// <summary>Gets or sets prop15 (double).</summary>
    public double Prop15
    {
        get => field;
        set
        {
            if (Math.Abs(field - value) <= double.Epsilon)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop15)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop15)));
        }
    }

    /// <summary>Gets or sets a value indicating whether prop16 is true.</summary>
    public bool Prop16
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Prop16)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Prop16)));
        }
    }

    /// <summary>Gets or sets the address for deep property chain testing.</summary>
    public Address Address
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Address)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Address)));
        }
    } = new();

    /// <summary>
    /// Creates an instance with every property set to a distinct non-default value, so a test observing
    /// any subset of the sixteen properties starts from a fully populated model.
    /// </summary>
    /// <returns>A populated view model.</returns>
    public static BigViewModel CreatePopulated() => new()
    {
        Prop1 = "a",
        Prop2 = Prop2Seed,
        Prop3 = Prop3Seed,
        Prop4 = true,
        Prop5 = "e",
        Prop6 = Prop6Seed,
        Prop7 = Prop7Seed,
        Prop8 = false,
        Prop9 = "i",
        Prop10 = Prop10Seed,
        Prop11 = Prop11Seed,
        Prop12 = true,
        Prop13 = "m",
        Prop14 = Prop14Seed,
        Prop15 = Prop15Seed,
        Prop16 = false,
    };
}
