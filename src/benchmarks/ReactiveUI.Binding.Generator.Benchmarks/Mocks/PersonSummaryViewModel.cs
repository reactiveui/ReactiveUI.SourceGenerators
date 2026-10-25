// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Generator.Benchmarks.Mocks;

/// <summary>A view model whose derived properties are backed by observables through ToProperty.</summary>
/// <remarks>Its public event-args raise method is how generated code raises its notifications.</remarks>
public sealed class PersonSummaryViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>Backs <see cref="DisplayName"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _displayName;

    /// <summary>Backs <see cref="Age"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _age;

    /// <summary>Backs <see cref="IsAdult"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isAdult;

    /// <summary>Initializes a new instance of the <see cref="PersonSummaryViewModel"/> class.</summary>
    /// <param name="names">The values <see cref="DisplayName"/> takes.</param>
    /// <param name="ages">The values <see cref="Age"/> takes.</param>
    /// <param name="adults">The values <see cref="IsAdult"/> takes.</param>
    public PersonSummaryViewModel(IObservable<string> names, IObservable<int> ages, IObservable<bool> adults)
    {
        _displayName = names.ToProperty(this, static x => x.DisplayName);
        _age = ages.ToProperty(this, nameof(Age), -1);
        _isAdult = adults.ToProperty(this, static x => x.IsAdult, false, true);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the display name.</summary>
    public string DisplayName => _displayName.Value;

    /// <summary>Gets the age.</summary>
    public int Age => _age.Value;

    /// <summary>Gets a value indicating whether the person is an adult.</summary>
    public bool IsAdult => _isAdult.Value;

    /// <summary>Raises <see cref="PropertyChanged"/>.</summary>
    /// <param name="args">The event arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

    /// <inheritdoc/>
    public void Dispose()
    {
        _displayName.Dispose();
        _age.Dispose();
        _isAdult.Dispose();
    }
}
