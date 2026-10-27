// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// A view model that is not <see langword="partial"/> and inherits only a <see langword="protected"/> raise method.
/// Generated code cannot call that method, so <c>ToProperty</c> reports RXUIBIND012 here and <c>ToPropertyUnsafe</c>
/// backs the property instead.
/// </summary>
[DebuggerDisplay("TodoHeadlineViewModel: Headline = {Headline}")]
public sealed class TodoHeadlineViewModel : ObservableObject
{
    /// <summary>Backs <see cref="Headline"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _headline;

    /// <summary>Initializes a new instance of the <see cref="TodoHeadlineViewModel"/> class.</summary>
    /// <param name="titles">The titles the headline follows.</param>
    public TodoHeadlineViewModel(IObservable<string> titles) =>
        _headline = titles.ToPropertyUnsafe(this, x => x.Headline);

    /// <summary>Gets the latest title.</summary>
    public string Headline => _headline.Value;
}
