// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Helpers;
#else
namespace ReactiveUI.Binding.Helpers;
#endif

/// <summary>Changes an array by publishing a changed copy, so a reader walks a stable set without a lock.</summary>
internal static class CopyOnWriteArray
{
    /// <summary>Publishes a copy of the array with <paramref name="item"/> added at the end.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="location">The field holding the array.</param>
    /// <param name="item">The item to add.</param>
    internal static void Add<T>(ref T[] location, T item)
    {
        T[] current;
        T[] updated;

        do
        {
            current = Volatile.Read(ref location);
            updated = new T[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[current.Length] = item;
        }
        while (!ReferenceEquals(Interlocked.CompareExchange(ref location, updated, current), current));
    }

    /// <summary>Publishes a copy of the array without the first occurrence of <paramref name="item"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="location">The field holding the array.</param>
    /// <param name="item">The item to remove. An item that is not there leaves the array alone.</param>
    internal static void Remove<T>(ref T[] location, T item)
    {
        T[] current;
        T[] updated;

        do
        {
            current = Volatile.Read(ref location);
            var index = Array.IndexOf(current, item);
            if (index < 0)
            {
                return;
            }

            updated = new T[current.Length - 1];
            Array.Copy(current, updated, index);
            Array.Copy(current, index + 1, updated, index, current.Length - index - 1);
        }
        while (!ReferenceEquals(Interlocked.CompareExchange(ref location, updated, current), current));
    }
}
