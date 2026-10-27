// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Subjects;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A partial view model whose helpers are assigned after listeners attach, and whose first property follows the second,
/// so each helper raises notifications while it is still being created.
/// </summary>
public partial class StartupViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the property that follows a source.</summary>
    [ObservableAsProperty]
    public partial bool Shown { get; }

    /// <summary>Gets the property whose helper is created first and follows <see cref="Mirrored"/>.</summary>
    [ObservableAsProperty]
    public partial int Total { get; }

    /// <summary>Gets the property whose helper is created second.</summary>
    [ObservableAsProperty]
    public partial int Mirrored { get; }

    /// <summary>Backs <see cref="Shown"/> with a source, as a view model does when it is activated.</summary>
    /// <param name="source">The source.</param>
    public void Show(IObservable<bool> source) => _shownHelper = source.ToProperty(this, static x => x.Shown);

    /// <summary>Backs <see cref="Total"/> first, then <see cref="Mirrored"/> which it follows.</summary>
    /// <param name="values">The values <see cref="Mirrored"/> takes.</param>
    public void Follow(IObservable<int> values)
    {
        var totals = new BehaviorSubject<int>(0);
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Mirrored))
            {
                totals.OnNext(Mirrored);
            }
        };

        _totalHelper = totals.ToProperty(this, static x => x.Total);
        _mirroredHelper = values.ToProperty(this, static x => x.Mirrored);
    }
}
