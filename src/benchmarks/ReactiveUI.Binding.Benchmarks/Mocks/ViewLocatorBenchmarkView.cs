// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>The view a lookup registered by <c>ViewLocatorBenchmark</c> returns, which accepts any view model.</summary>
public sealed class ViewLocatorBenchmarkView : IViewFor
{
    /// <inheritdoc/>
    public object? ViewModel { get; set; }
}
