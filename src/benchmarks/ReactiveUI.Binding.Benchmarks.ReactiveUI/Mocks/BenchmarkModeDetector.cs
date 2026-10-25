// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Splat;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Tells ReactiveUI it is running outside an application, as a benchmark process does.</summary>
internal sealed class BenchmarkModeDetector : IModeDetector
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool? InUnitTestRunner() => true;
}
