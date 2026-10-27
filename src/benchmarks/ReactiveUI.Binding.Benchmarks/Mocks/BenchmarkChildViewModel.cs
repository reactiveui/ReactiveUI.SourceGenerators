// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A child view model for deep chain benchmarks.</summary>
[DebuggerDisplay("BenchmarkChildViewModel: Value = {Value}")]
public class BenchmarkChildViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the value.</summary>
    public string Value
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    } = string.Empty;
}
