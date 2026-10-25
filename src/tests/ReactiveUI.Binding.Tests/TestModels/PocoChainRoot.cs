// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A root with no change notification that holds an object which notifies.</summary>
public class PocoChainRoot
{
    /// <summary>Gets the notifying object at the end of the chain.</summary>
    public TestFixture Leaf { get; } = new();
}
