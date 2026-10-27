// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// Declares a property that reports <c>Loading</c> until its helper is assigned, with a helper field only the
/// constructor can assign.
/// </summary>
[System.Diagnostics.DebuggerDisplay("StatusViewModel: Status = {Status}")]
public sealed partial class StatusViewModel : INotifyPropertyChanged
{
    /// <summary>Initializes a new instance of the <see cref="StatusViewModel"/> class.</summary>
    /// <param name="statuses">The statuses to report, or null while there are none yet.</param>
    public StatusViewModel(IObservable<string>? statuses)
    {
        if (statuses is not null)
        {
            _statusHelper = statuses.ToProperty(this, static x => x.Status);
        }
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the current status.</summary>
    [ObservableAsProperty(InitialValue = "Loading", ReadOnly = true)]
    public partial string Status { get; }
}
