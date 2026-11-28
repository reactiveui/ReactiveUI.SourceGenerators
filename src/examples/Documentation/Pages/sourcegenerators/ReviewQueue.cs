// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using ReactiveUI.SourceGenerators;

namespace ReactiveUI.Binding.Documentation.SourceGenerators;

/// <summary>Tracks pending reviews and exposes completed reviews as a read-only list.</summary>
[System.Diagnostics.DebuggerDisplay("Reviews completed = {Completed.Count}")]
public partial class ReviewQueue : ReactiveObject
{
    /// <summary>The live list of completed reviews maintained by the reviewer.</summary>
    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<string> _completed;

    /// <summary>The batch whose changes notify observers of the generated property.</summary>
    [ReactiveCollection]
    private ObservableCollection<string>? _pending;

    /// <summary>Initializes a new instance of the <see cref="ReviewQueue"/> class.</summary>
    /// <param name="completed">The collection the reviewer updates on completion.</param>
    public ReviewQueue(ObservableCollection<string> completed) => _completed = new(completed);
}
