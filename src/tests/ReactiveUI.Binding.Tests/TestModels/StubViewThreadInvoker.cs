// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>An invoker that claims the objects it is given and holds each posted callback until a test runs it.</summary>
internal sealed class StubViewThreadInvoker : IViewThreadInvoker
{
    /// <summary>The objects this invoker claims.</summary>
    private readonly object[] _claimed;

    /// <summary>The callbacks posted and not yet run.</summary>
    private readonly Queue<Action> _posted = new();

    /// <summary>Initializes a new instance of the <see cref="StubViewThreadInvoker"/> class.</summary>
    /// <param name="claimed">The objects this invoker claims.</param>
    public StubViewThreadInvoker(params object[] claimed) => _claimed = claimed;

    /// <summary>Gets or sets a value indicating whether the calling thread counts as the owning thread.</summary>
    public bool HasAccess { get; set; }

    /// <summary>Gets how many callbacks were posted.</summary>
    public int PostCount { get; private set; }

    /// <inheritdoc/>
    public bool Claims(object target) => Array.IndexOf(_claimed, target) >= 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess(object target) => HasAccess;

    /// <inheritdoc/>
    public void Post(object target, Action<object?> callback, object? state)
    {
        PostCount++;
        _posted.Enqueue(() => callback(state));
    }

    /// <summary>Runs the posted callbacks, including any posted while they run.</summary>
    internal void RunPosted()
    {
        while (_posted.Count > 0)
        {
            _posted.Dequeue()();
        }
    }
}
