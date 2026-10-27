// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A host fixture with a Child property for testing deep property chain observation.
/// Implements both INotifyPropertyChanged and INotifyPropertyChanging.
/// </summary>
public class HostTestFixture : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the child view model for deep chain testing.</summary>
    public TestViewModel? Child
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Child)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Child)));
        }
    }
}
