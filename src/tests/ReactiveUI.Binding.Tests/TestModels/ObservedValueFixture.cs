// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A minimal observable object exposing a single value. Its own type is what keys the notification
/// factory lookup, so tests that need a specific notifier resolved for them observe this rather than a
/// fixture other tests already share.
/// </summary>
public class ObservedValueFixture : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the observed value.</summary>
    public string Value
    {
        get => field;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }

    = string.Empty;

    /// <summary>Raises a change notification for <see cref="Value"/> without altering it.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaiseValueChanged() => PropertyChanged?.Invoke(this, new(nameof(Value)));
}
