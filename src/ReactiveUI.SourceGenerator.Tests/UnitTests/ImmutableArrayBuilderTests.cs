// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="ImmutableArrayBuilder{T}"/>.
/// </summary>
public sealed class ImmutableArrayBuilderTests
{
    /// <summary>
    /// A freshly rented builder starts with Count == 0.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenRentedThenCountIsZero()
    {
        int count;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            count = builder.Count;
        }

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>
    /// Adding a single item increments Count to 1.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenItemAddedThenCountIncrements()
    {
        int count;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(42);
            count = builder.Count;
        }

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>
    /// Multiple adds are reflected in Count.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenMultipleItemsAddedThenCountMatchesAdded()
    {
        int count;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(1);
            builder.Add(2);
            builder.Add(3);
            count = builder.Count;
        }

        await Assert.That(count).IsEqualTo(3);
    }

    /// <summary>
    /// ToImmutable returns an array containing all added items in order.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToImmutableCalledThenContainsAddedItems()
    {
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(10);
            builder.Add(20);
            builder.Add(30);
            result = builder.ToImmutable();
        }

        await Assert.That(result.Length).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(10);
        await Assert.That(result[1]).IsEqualTo(20);
        await Assert.That(result[2]).IsEqualTo(30);
    }

    /// <summary>
    /// ToArray returns a mutable array with the same elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToArrayCalledThenReturnsMutableArray()
    {
        string[] result;
        using (var builder = ImmutableArrayBuilder<string>.Rent())
        {
            builder.Add("a");
            builder.Add("b");
            result = builder.ToArray();
        }

        await Assert.That(result.Length).IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo("a");
        await Assert.That(result[1]).IsEqualTo("b");
    }

    /// <summary>
    /// AddRange appends all items from the span.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAddRangeCalledThenAllItemsAppended()
    {
        int count;
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            ReadOnlySpan<int> items = [1, 2, 3, 4, 5];
            builder.AddRange(items);
            count = builder.Count;
            result = builder.ToImmutable();
        }

        await Assert.That(count).IsEqualTo(5);
        await Assert.That(result[4]).IsEqualTo(5);
    }

    /// <summary>
    /// WrittenSpan reflects the items added so far.
    /// </summary>
    [Test]
    public void WhenWrittenSpanAccessedThenReflectsCurrentItems()
    {
        int length;
        int first;
        int second;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(7);
            builder.Add(8);
            var span = builder.WrittenSpan;
            length = span.Length;
            first = span[0];
            second = span[1];
        }

        if (length != 2)
        {
            throw new InvalidOperationException($"Expected WrittenSpan.Length 2, got {length}.");
        }

        if (first != 7 || second != 8)
        {
            throw new InvalidOperationException($"Expected 7, 8 but got {first}, {second}.");
        }
    }

    /// <summary>
    /// AsEnumerable returns an IEnumerable containing all added items.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAsEnumerableCalledThenYieldsAllItems()
    {
        List<int> list;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(100);
            builder.Add(200);
            list = builder.AsEnumerable().ToList();
        }

        await Assert.That(list.Count).IsEqualTo(2);
        await Assert.That(list[0]).IsEqualTo(100);
        await Assert.That(list[1]).IsEqualTo(200);
    }

    /// <summary>
    /// Builder can hold more than the initial capacity (pool growth).
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenManyItemsAddedThenBuilderGrowsCorrectly()
    {
        int count;
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            for (var i = 0; i < 100; i++)
            {
                builder.Add(i);
            }

            count = builder.Count;
            result = builder.ToImmutable();
        }

        await Assert.That(count).IsEqualTo(100);
        await Assert.That(result[99]).IsEqualTo(99);
    }

    /// <summary>
    /// ToImmutable on an empty builder returns an empty ImmutableArray.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEmptyThenToImmutableReturnsEmpty()
    {
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            result = builder.ToImmutable();
        }

        await Assert.That(result.IsEmpty).IsTrue();
    }

    /// <summary>
    /// ToString returns the WrittenSpan string representation without throwing.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToStringCalledThenDoesNotThrow()
    {
        string result;
        using (var builder = ImmutableArrayBuilder<char>.Rent())
        {
            builder.Add('H');
            builder.Add('i');
            result = builder.ToString();
        }

        await Assert.That(result).IsNotNull();
    }

    /// <summary>
    /// Dispose can be called multiple times without throwing.
    /// </summary>
    [Test]
    public void WhenDisposedTwiceThenDoesNotThrow()
    {
        var builder = ImmutableArrayBuilder<int>.Rent();
        builder.Add(1);
        builder.Dispose();
        builder.Dispose();
    }

    /// <summary>
    /// AddRange followed by Add correctly appends items in order.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAddRangeThenAddThenOrderPreserved()
    {
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            ReadOnlySpan<int> range = [1, 2, 3];
            builder.AddRange(range);
            builder.Add(4);
            result = builder.ToImmutable();
        }

        await Assert.That(result.Length).IsEqualTo(4);
        await Assert.That(result[3]).IsEqualTo(4);
    }
}
