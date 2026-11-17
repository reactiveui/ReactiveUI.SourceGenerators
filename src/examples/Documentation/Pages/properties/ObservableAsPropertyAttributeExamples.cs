// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>Shows a partial property declared with <see cref="ObservableAsPropertyAttribute"/>.</summary>
public static partial class ObservableAsPropertyAttributeExamples
{
    /// <summary>Constructs <see cref="SummaryViewModel"/> and reads both of its generated properties.</summary>
    public static void DeclareAPropertyWithTheAttribute()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };
        SummaryViewModel summary = new SummaryViewModel(item);

        Console.WriteLine(summary.Title);
        Console.WriteLine(summary.IsDone);

        item.IsDone = true;

        Console.WriteLine(summary.IsDone);
    }

    /// <summary>Reads a property before and after its helper is assigned: until then it returns its initial value.</summary>
    public static void StartFromAnInitialValue()
    {
        StatusViewModel waiting = new StatusViewModel(null);
        StatusViewModel ready = new StatusViewModel(Signal.Return("Ready"));

        Console.WriteLine(waiting.Status);
        Console.WriteLine(ready.Status);

        // Output:
        // Loading
        // Ready
    }

    /// <summary>Reads each replacement helper's value from its synchronous change notification.</summary>
    public static void ReplaceAHelperWhileObserved()
    {
        using ReplaceableStatus? status = new();
        string? observed = null;
        status.PropertyChanged += (_, _) => observed = status.Status;

        status.Connect(Signal.Return("Connected"));
        Console.WriteLine(observed);
        status.Connect(Signal.Return("Disconnected"));
        Console.WriteLine(observed);

        // Output:
        // Connected
        // Disconnected
    }

    /// <summary>Replaces its property's source when a new connection starts.</summary>
    [System.Diagnostics.DebuggerDisplay("Status = {Status}")]
    public sealed partial class ReplaceableStatus : INotifyPropertyChanged, IDisposable
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets the latest value from the current connection.</summary>
        [ObservableAsProperty]
        public partial string? Status { get; }

        /// <summary>Disposes the previous helper and follows a new source.</summary>
        /// <param name="source">The connection's status values.</param>
        public void Connect(IObservable<string?> source)
        {
            _statusHelper?.Dispose();
            _statusHelper = source.ToProperty(this, static x => x.Status);
        }

        /// <inheritdoc/>
        public void Dispose() => _statusHelper?.Dispose();
    }
}
