// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>An object with enough notifying properties to observe every dynamic-chain arity.</summary>
/// <remarks>Public because the chain is walked by reflection, which reads public members.</remarks>
public class DynamicChainFixture : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets observed property 1.</summary>
    public string P1
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P1)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 2.</summary>
    public string P2
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P2)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 3.</summary>
    public string P3
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P3)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 4.</summary>
    public string P4
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P4)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 5.</summary>
    public string P5
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P5)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 6.</summary>
    public string P6
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P6)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 7.</summary>
    public string P7
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P7)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 8.</summary>
    public string P8
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P8)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 9.</summary>
    public string P9
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P9)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 10.</summary>
    public string P10
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P10)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 11.</summary>
    public string P11
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P11)));
        }
    } = "a";

    /// <summary>Gets or sets observed property 12.</summary>
    public string P12
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(P12)));
        }
    } = "a";

    /// <summary>Gets or sets the intermediate link an observed chain passes through.</summary>
    public DynamicChainChild? Child
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Child)));
        }
    }
}
