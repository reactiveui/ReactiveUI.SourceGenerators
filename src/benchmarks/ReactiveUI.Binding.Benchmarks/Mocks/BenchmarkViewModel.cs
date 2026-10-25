// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A view model used for benchmarking source-generated property observation and binding.</summary>
[DebuggerDisplay("BenchmarkViewModel: Name = {Name}, Age = {Age}")]
public class BenchmarkViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the name.</summary>
    public string Name
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Name)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Name)));
        }
    } = string.Empty;

    /// <summary>Gets or sets the age.</summary>
    public int Age
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Age)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Age)));
        }
    }

    /// <summary>Gets or sets the child view model.</summary>
    public BenchmarkChildViewModel Child
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
    } = new();

    /// <summary>Gets or sets the command an invocation or a command binding reaches.</summary>
    public ICommand? Run
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Run)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Run)));
        }
    }

    /// <summary>Gets or sets the stream a WhenAnyObservable call switches to.</summary>
    public IObservable<string>? Signal
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(Signal)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Signal)));
        }
    }

    /// <summary>Gets or sets the second stream a WhenAnyObservable call combines.</summary>
    public IObservable<string>? OtherSignal
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new(nameof(OtherSignal)));
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(OtherSignal)));
        }
    }
}
