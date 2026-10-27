// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A view model that only the lookups <c>ViewLocatorBenchmark</c> registers resolve.</summary>
public sealed class ViewLocatorBenchmarkViewModel
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
}
