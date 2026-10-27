// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using ReactiveUI.Binding;
using ReactiveUI.Primitives.Concurrency;

namespace SharedScenarios.ToProperty.InitialValueDeferScheduler;

/// <summary>A partial view model whose derived property starts at a value and subscribes on first read.</summary>
public partial class MyViewModel : INotifyPropertyChanged
{
    /// <summary>Backs <see cref="FullName"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _fullName;

    /// <summary>Initializes a new instance of the <see cref="MyViewModel"/> class.</summary>
    /// <param name="names">The values <see cref="FullName"/> takes.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on.</param>
    public MyViewModel(IObservable<string> names, ISequencer? scheduler) =>
        _fullName = names.ToProperty(this, x => x.FullName, "(none)", true, scheduler);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest name.</summary>
    public string FullName => _fullName.Value;
}
