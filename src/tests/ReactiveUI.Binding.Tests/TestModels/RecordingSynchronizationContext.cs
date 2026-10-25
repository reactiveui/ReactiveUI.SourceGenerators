// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A context that runs each posted callback inline and counts them, standing in for a host's main thread.</summary>
internal sealed class RecordingSynchronizationContext : SynchronizationContext
{
    /// <summary>Gets how many callbacks were posted.</summary>
    public int PostCount { get; private set; }

    /// <inheritdoc/>
    public override void Post(SendOrPostCallback d, object? state)
    {
        PostCount++;
        d(state);
    }
}
