// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace SharedScenarios.BindTwoWay.ReactiveObjectBoth;

/// <summary>Source ViewModel extending ReactiveObject.</summary>
public class MyViewModel : ReactiveObject
{
    /// <summary>The backing field for <see cref="Name"/>.</summary>
    private string _name = string.Empty;

    /// <summary>Gets or sets the name.</summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
}
