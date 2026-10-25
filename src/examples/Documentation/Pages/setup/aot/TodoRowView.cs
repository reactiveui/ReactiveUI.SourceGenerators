// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Setup.Aot;

/// <summary>One row of the to-do list, with the two texts a row shows. It has no UI framework behind it, so the published program needs nothing else.</summary>
[System.Diagnostics.DebuggerDisplay("TodoRowView: TitleText = {TitleText}, StatusText = {StatusText}")]
public sealed class TodoRowView : ObservableObject
{
    /// <summary>Gets or sets the text that shows the title of the item.</summary>
    public string TitleText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the text that shows whether the item is finished.</summary>
    public string StatusText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
