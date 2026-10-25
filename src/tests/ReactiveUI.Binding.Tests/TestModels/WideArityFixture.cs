// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>An object with enough notifying properties to reach every wide-arity observation overload.</summary>
public class WideArityFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets observed property 1.</summary>
    public string Value1
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value1)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value1)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 2.</summary>
    public string Value2
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value2)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value2)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 3.</summary>
    public string Value3
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value3)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value3)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 4.</summary>
    public string Value4
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value4)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value4)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 5.</summary>
    public string Value5
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value5)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value5)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 6.</summary>
    public string Value6
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value6)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value6)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 7.</summary>
    public string Value7
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value7)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value7)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 8.</summary>
    public string Value8
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value8)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value8)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 9.</summary>
    public string Value9
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value9)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value9)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 10.</summary>
    public string Value10
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value10)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value10)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 11.</summary>
    public string Value11
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value11)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value11)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 12.</summary>
    public string Value12
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value12)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value12)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 13.</summary>
    public string Value13
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value13)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value13)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 14.</summary>
    public string Value14
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value14)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value14)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 15.</summary>
    public string Value15
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value15)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value15)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 16.</summary>
    public string Value16
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, new(nameof(Value16)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value16)));
        }
    } = "a";
}
