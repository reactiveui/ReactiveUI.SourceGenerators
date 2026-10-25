// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A view model holding a decimal amount that raises a change on every write and refuses to be written without end.</summary>
public class AmountViewModel : INotifyPropertyChanged
{
    /// <summary>The number of writes after which the view model reports that its binding never settles.</summary>
    private const int WriteLimit = 1_000;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the number of writes to <see cref="Amount"/>.</summary>
    public int Writes { get; private set; }

    /// <summary>Gets or sets the amount.</summary>
    public decimal Amount
    {
        get => field;
        set
        {
            Writes++;
            if (Writes > WriteLimit)
            {
                throw new InvalidOperationException($"The binding kept writing: {value}");
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Amount)));
        }
    }
}
