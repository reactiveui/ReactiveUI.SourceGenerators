// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Benchmarks.Shared;

namespace ReactiveUI.SourceGenerators.Runtime.Benchmarks;

/// <summary>The generated-code scenarios the EventPipe allocation harness measures.</summary>
internal static class RuntimeScenarios
{
    /// <summary>The operations each session runs; enough that one allocation tick is a small fraction of the total.</summary>
    private const int Operations = 2_000_000;

    /// <summary>
    /// The operations the collection-replacing session runs. Generated collection properties used to leak a handler per
    /// replacement, so each replacement cost more than the last; this count lets that code finish.
    /// </summary>
    private const int ReplaceCollectionOperations = 20_000;

    /// <summary>The operations the collection-changing session runs, after the replacements above have left their handlers.</summary>
    private const int ChangeCollectionOperations = 2_000;

    /// <summary>Creates one scenario per <see cref="GeneratedCodeBenchmarks"/> benchmark.</summary>
    /// <returns>The scenarios.</returns>
    internal static List<AllocationScenario> Create()
    {
        var benchmarks = new GeneratedCodeBenchmarks();
        return
        [
            new("ReplaceCollection", ReplaceCollectionOperations, benchmarks.Setup, () => benchmarks.ReplaceCollection()),
            new("ChangeCollection", ChangeCollectionOperations, static () => { }, () => benchmarks.ChangeCollection()),
            new("SetReactivePropertyWithAlsoNotify", Operations, static () => { }, () => benchmarks.SetReactivePropertyWithAlsoNotify()),
            new("SetGeneratedReactiveObjectProperty", Operations, static () => { }, () => benchmarks.SetGeneratedReactiveObjectProperty()),
            new("ReadCommand", Operations, static () => { }, () => benchmarks.ReadCommand()),
        ];
    }
}
