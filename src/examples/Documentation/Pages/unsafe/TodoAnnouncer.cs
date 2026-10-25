// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Announces what happened to the to-do list. Each announcement stream is replaced when its source changes.</summary>
[System.Diagnostics.DebuggerDisplay("TodoAnnouncer")]
public sealed class TodoAnnouncer : ObservableObject
{
    /// <summary>Gets or sets the stream of ordinary announcements.</summary>
    public IObservable<string> Latest
    {
        get;
        set => SetProperty(ref field, value);
    } = Signal.Never<string>();

    /// <summary>Gets or sets the stream of urgent announcements.</summary>
    public IObservable<string> Urgent
    {
        get;
        set => SetProperty(ref field, value);
    } = Signal.Never<string>();
}
