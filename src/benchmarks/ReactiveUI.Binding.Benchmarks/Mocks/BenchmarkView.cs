// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A view used for binding benchmarks. Implements <see cref="IViewFor{T}"/> to support ReactiveUI's expression-tree-based binding APIs.</summary>
[DebuggerDisplay("BenchmarkView: DisplayName = {DisplayName}, DisplayAge = {DisplayAge}")]
public class BenchmarkView : IViewFor<BenchmarkViewModel>, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(DisplayName)));
        }
    } = string.Empty;

    /// <summary>Gets or sets the display age.</summary>
    public int DisplayAge
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(DisplayAge)));
        }
    }

    /// <inheritdoc/>
    public BenchmarkViewModel? ViewModel
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewModel)));
        }
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (BenchmarkViewModel?)value;
    }
}
