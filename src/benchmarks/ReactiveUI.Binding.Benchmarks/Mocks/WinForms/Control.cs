// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace System.Windows.Forms;

/// <summary>Provides layout and collection contracts without platform rendering costs.</summary>
public class Control
{
    /// <summary>Gets the number of completed layout batches.</summary>
    public long Layouts { get; private set; }

    /// <summary>Gets the number of outstanding layout suspensions.</summary>
    public int LayoutDepth { get; private set; }

    /// <summary>Starts a layout batch.</summary>
    public void SuspendLayout() => LayoutDepth++;

    /// <summary>Completes a layout batch.</summary>
    public void ResumeLayout()
    {
        LayoutDepth--;
        Layouts++;
    }

    /// <summary>Stores controls in a reusable collection buffer.</summary>
    public sealed class ControlCollection
    {
        /// <summary>Capacity reserved for the benchmark's fixed-size control inputs.</summary>
        private const int BufferCapacity = 8;

        /// <summary>Storage allocated before measurement.</summary>
        private readonly List<Control> _items = [with(BufferCapacity)];

        /// <summary>Initializes a new instance of the <see cref="ControlCollection"/> class.</summary>
        /// <param name="owner">The layout owner.</param>
        public ControlCollection(Control owner) => Owner = owner;

        /// <summary>Gets the layout owner.</summary>
        public Control Owner { get; }

        /// <summary>Gets the number of stored controls.</summary>
        public int Count => _items.Count;

        /// <summary>Empties the collection while retaining its capacity.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _items.Clear();

        /// <summary>Adds the supplied controls.</summary>
        /// <param name="controls">The incoming controls.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(Control[] controls) => _items.AddRange(controls);
    }
}
