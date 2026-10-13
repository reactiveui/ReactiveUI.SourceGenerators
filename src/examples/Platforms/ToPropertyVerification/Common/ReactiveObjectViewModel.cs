// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// ReactiveUI's namespace is not imported, so its own ToProperty mixin never competes with this library's; its
// types are named in full instead.
namespace ToPropertyVerification.Common;

/// <summary>
/// A ReactiveUI view model, raised through ReactiveUI's own public raise extensions (<c>IReactiveObject</c>).
/// Backs <see cref="Count"/> with the selector overload, and <see cref="Label"/> with the selector
/// overload that returns its helper through an <see langword="out"/> parameter and a plain initial value.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ReactiveObjectViewModel: Count = {Count}, Label = {Label}")]
public sealed class ReactiveObjectViewModel : global::ReactiveUI.ReactiveObject
{
    /// <summary>The plain initial value <see cref="Label"/> starts at.</summary>
    private const string InitialLabel = "start";

    /// <summary>Backs <see cref="Count"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _countHelper;

    /// <summary>Backs <see cref="Label"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _labelHelper;

    /// <summary>Backs the reflection-based property.</summary>
    private readonly ObservableAsPropertyHelper<int> _unsafeCountHelper;

    /// <summary>Initializes a new instance of the <see cref="ReactiveObjectViewModel"/> class.</summary>
    /// <param name="item">The item the view model follows.</param>
    public ReactiveObjectViewModel(SourceItem item)
    {
        _countHelper = item.WhenChanged(static x => x.Count).ToProperty(this, static x => x.Count);

        _ = item.WhenChanged(static x => x.Label).ToProperty(this, static x => x.Label, out _labelHelper, InitialLabel);

        _unsafeCountHelper = item.WhenChanged(static x => x.Count).ToPropertyUnsafe(this, static x => x.UnsafeCount);
    }

    /// <summary>Gets the followed item's count.</summary>
    public int Count => _countHelper.Value;

    /// <summary>Gets the followed item's label.</summary>
    public string Label => _labelHelper.Value;

    /// <summary>Gets the followed count through the reflection-based overload.</summary>
    public int UnsafeCount => _unsafeCountHelper.Value;
}
