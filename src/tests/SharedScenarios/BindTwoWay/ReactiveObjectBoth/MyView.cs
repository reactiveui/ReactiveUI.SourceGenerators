// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace SharedScenarios.BindTwoWay.ReactiveObjectBoth;

/// <summary>Target View extending ReactiveObject.</summary>
public class MyView : ReactiveObject
{
    /// <summary>The backing field for <see cref="NameText"/>.</summary>
    private string _nameText = string.Empty;

    /// <summary>Gets or sets the name text.</summary>
    public string NameText
    {
        get => _nameText;
        set => this.RaiseAndSetIfChanged(ref _nameText, value);
    }
}
