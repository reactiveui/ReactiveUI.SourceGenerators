// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A plain old CLR object with no change notification support.</summary>
public class PocoModel
{
    /// <summary>Gets or sets the value.</summary>
    public string Value { get; set; } = string.Empty;
}
