// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>The delivery contracts exercised by the observation benchmarks.</summary>
public enum ObservationKind
{
    /// <summary>Re-reads the property after a change.</summary>
    Changed = 0,

    /// <summary>Captures the property before a write.</summary>
    Changing = 1,

    /// <summary>Re-reads the property after a custom provider's notification.</summary>
    Plugin = 2,
}
