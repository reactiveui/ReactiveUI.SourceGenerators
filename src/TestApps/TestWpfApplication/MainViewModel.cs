// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.SourceGenerators;

namespace WpfApp1;

/// <summary>Represents the primary view model for the WPF sample application.</summary>
public partial class MainViewModel : ReactiveObject
{
    /// <summary>Provides the scheduler used by the generated command.</summary>
    private readonly ISequencer _scheduler = RxSchedulers.MainThreadScheduler;

    /// <summary>Stores the editable display name.</summary>
    [Reactive]
    private string? _name;

    /// <summary>Saves the current display name.</summary>
    [ReactiveCommand(OutputScheduler = nameof(_scheduler))]
    private void Save() => Name = Name?.Trim();
}
