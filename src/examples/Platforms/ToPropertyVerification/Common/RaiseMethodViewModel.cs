// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace ToPropertyVerification.Common;

/// <summary>
/// Backs <see cref="CountText"/> with the named-property overload and a plain initial value, and
/// <see cref="UpperLabel"/> with the selector overload and an initial-value factory.
/// </summary>
[System.Diagnostics.DebuggerDisplay("RaiseMethodViewModel: CountText = {CountText}, UpperLabel = {UpperLabel}")]
public sealed class RaiseMethodViewModel : RaiseMethodBase
{
    /// <summary>The initial value <see cref="CountText"/> takes before the first notification.</summary>
    private const string NoCountYet = "none";

    /// <summary>The initial value <see cref="UpperLabel"/>'s factory returns before the first notification.</summary>
    private const string PendingLabel = "PENDING";

    /// <summary>Backs <see cref="CountText"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _countTextHelper;

    /// <summary>Backs <see cref="UpperLabel"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _upperLabelHelper;

    /// <summary>Initializes a new instance of the <see cref="RaiseMethodViewModel"/> class.</summary>
    /// <param name="item">The item the view model follows.</param>
    public RaiseMethodViewModel(SourceItem item)
    {
        _countTextHelper = item.WhenChanged(static x => x.Count)
            .Select(static c => c.ToString(CultureInfo.InvariantCulture))
            .ToProperty(this, nameof(CountText), NoCountYet);
        _upperLabelHelper = item.WhenChanged(static x => x.Label)
            .Select(static l => l.ToUpperInvariant())
            .ToProperty(this, static x => x.UpperLabel, static () => PendingLabel);
    }

    /// <summary>Gets the followed item's count, formatted as text.</summary>
    public string CountText => _countTextHelper.Value;

    /// <summary>Gets the followed item's label, upper-cased.</summary>
    public string UpperLabel => _upperLabelHelper.Value;
}
