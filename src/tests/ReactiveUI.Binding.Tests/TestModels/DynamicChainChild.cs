// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>An object reached through an intermediate link of an observed chain.</summary>
/// <remarks>Public because the chain is walked by reflection, which reads public members.</remarks>
public class DynamicChainChild : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the observed value.</summary>
    public string Name
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Name)));
        }
    } = "a";
}
