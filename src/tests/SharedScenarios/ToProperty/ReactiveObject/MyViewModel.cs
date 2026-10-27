// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.ToProperty.ReactiveObject;

/// <summary>A ReactiveUI view model; its notifications go through ReactiveUI's own raise path.</summary>
public class MyViewModel : global::ReactiveUI.ReactiveObject
{
    /// <summary>Backs <see cref="FullName"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _fullName;

    /// <summary>Initializes a new instance of the <see cref="MyViewModel"/> class.</summary>
    /// <param name="names">The values <see cref="FullName"/> takes.</param>
    public MyViewModel(IObservable<string> names) => _fullName = names.ToProperty(this, x => x.FullName);

    /// <summary>Gets the latest name.</summary>
    public string FullName => _fullName.Value;
}
