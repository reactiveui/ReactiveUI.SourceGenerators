// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ToPropertyVerification.Common;

/// <summary>
/// Wraps a virtual-time sequencer so the scenarios that pass an explicit <c>ISequencer</c> to <c>ToProperty</c>
/// can prove delivery waits for the sequencer to run.
/// </summary>
[System.Diagnostics.DebuggerDisplay("TestClock")]
public sealed class TestClock
{
    /// <summary>The virtual-time sequencer backing <see cref="Sequencer"/>.</summary>
    private readonly VirtualClock _scheduler = new();

    /// <summary>Gets the sequencer scenarios pass to <c>ToProperty</c>.</summary>
    public ISequencer Sequencer => _scheduler;

    /// <summary>Runs every action the sequencer has queued so far.</summary>
    public void RunPending()
    {
        _scheduler.Start();
        _scheduler.Stop();
    }
}
