// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ToPropertyVerification.Common;

/// <summary>An ordinary <see cref="INotifyPropertyChanged"/> source that every scenario below follows with <c>WhenChanged</c>.</summary>
[System.Diagnostics.DebuggerDisplay("SourceItem: Count = {Count}, Label = {Label}")]
public sealed class SourceItem : INotifyPropertyChanged
{
    /// <summary>The label a new <see cref="SourceItem"/> starts with.</summary>
    private const string InitialLabel = "initial";

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets a count that scenarios push new values through.</summary>
    public int Count
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    /// <summary>Gets or sets a label that scenarios push new values through.</summary>
    public string Label
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
        }
    } = InitialLabel;
}
