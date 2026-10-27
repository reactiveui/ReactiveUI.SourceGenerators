// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>A partial view model whose derived properties are declared with <c>[ObservableAsProperty]</c>.</summary>
public partial class SummaryViewModel : INotifyPropertyChanged
{
    /// <summary>Initializes a new instance of the <see cref="SummaryViewModel"/> class.</summary>
    /// <param name="names">The values <see cref="FullName"/> takes.</param>
    /// <param name="ages">The values <see cref="Age"/> takes.</param>
    public SummaryViewModel(IObservable<string?> names, IObservable<int?> ages)
    {
        _fullNameHelper = names.ToProperty(this, static x => x.FullName);
        _ageHelper = ages.ToProperty(this, static x => x.Age);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest name.</summary>
    [ObservableAsProperty]
    public partial string? FullName { get; }

    /// <summary>Gets the latest age.</summary>
    [ObservableAsProperty]
    public partial int? Age { get; }
}
