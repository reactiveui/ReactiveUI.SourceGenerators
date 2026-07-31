// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="ImmutableArrayBuilder{T}"/>.</summary>
public sealed class ImmutableArrayBuilderTests
{
    /// <summary>The expected answer value.</summary>
    private const int Answer = 42;

    /// <summary>The first item index.</summary>
    private const int FirstIndex = 0;

    /// <summary>The second item index.</summary>
    private const int SecondIndex = 1;

    /// <summary>The count for two items.</summary>
    private const int TwoItems = 2;

    /// <summary>The count for three items.</summary>
    private const int ThreeItems = 3;

    /// <summary>The count for four items.</summary>
    private const int FourItems = 4;

    /// <summary>The count for five items.</summary>
    private const int FiveItems = 5;

    /// <summary>The first WrittenSpan test value.</summary>
    private const int Seven = 7;

    /// <summary>The second WrittenSpan test value.</summary>
    private const int Eight = 8;

    /// <summary>The first ordered test value.</summary>
    private const int Ten = 10;

    /// <summary>The second ordered test value.</summary>
    private const int Twenty = 20;

    /// <summary>The third ordered test value.</summary>
    private const int Thirty = 30;

    /// <summary>The final index in a hundred item collection.</summary>
    private const int NinetyNine = 99;

    /// <summary>The collection growth count.</summary>
    private const int OneHundred = 100;

    /// <summary>The second enumerable test value.</summary>
    private const int TwoHundred = 200;

    /// <summary>A freshly rented builder starts with Count == 0.</summary>
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

    /// <summary>Adding a single item increments Count to 1.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenItemAddedThenCountIncrements()
    {
        int count;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(Answer);
            count = builder.Count;
        }

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>Multiple adds are reflected in Count.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenMultipleItemsAddedThenCountMatchesAdded()
    {
        int count;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(1);
            builder.Add(TwoItems);
            builder.Add(ThreeItems);
            count = builder.Count;
        }

        await Assert.That(count).IsEqualTo(ThreeItems);
    }

    /// <summary>ToImmutable returns an array containing all added items in order.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToImmutableCalledThenContainsAddedItems()
    {
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(Ten);
            builder.Add(Twenty);
            builder.Add(Thirty);
            result = builder.ToImmutable();
        }

        await Assert.That(result.Length).IsEqualTo(ThreeItems);
        await Assert.That(result[FirstIndex]).IsEqualTo(Ten);
        await Assert.That(result[SecondIndex]).IsEqualTo(Twenty);
        await Assert.That(result[TwoItems]).IsEqualTo(Thirty);
    }

    /// <summary>ToArray returns a mutable array with the same elements.</summary>
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

        await Assert.That(result.Length).IsEqualTo(TwoItems);
        await Assert.That(result[FirstIndex]).IsEqualTo("a");
        await Assert.That(result[SecondIndex]).IsEqualTo("b");
    }

    /// <summary>AddRange appends all items from the span.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAddRangeCalledThenAllItemsAppended()
    {
        int count;
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            ReadOnlySpan<int> items = [1, TwoItems, ThreeItems, FourItems, FiveItems];
            builder.AddRange(items);
            count = builder.Count;
            result = builder.ToImmutable();
        }

        await Assert.That(count).IsEqualTo(FiveItems);
        await Assert.That(result[FourItems]).IsEqualTo(FiveItems);
    }

    /// <summary>WrittenSpan reflects the items added so far.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenWrittenSpanAccessedThenReflectsCurrentItems()
    {
        int length;
        int first;
        int second;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(Seven);
            builder.Add(Eight);
            var span = builder.WrittenSpan;
            length = span.Length;
            first = span[FirstIndex];
            second = span[SecondIndex];
        }

        await Assert.That(length).IsEqualTo(TwoItems);
        await Assert.That(first).IsEqualTo(Seven);
        await Assert.That(second).IsEqualTo(Eight);
    }

    /// <summary>AsEnumerable returns an IEnumerable containing all added items.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAsEnumerableCalledThenYieldsAllItems()
    {
        List<int> list;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(OneHundred);
            builder.Add(TwoHundred);
            list = [.. builder.AsEnumerable()];
        }

        await Assert.That(list.Count).IsEqualTo(TwoItems);
        await Assert.That(list[FirstIndex]).IsEqualTo(OneHundred);
        await Assert.That(list[SecondIndex]).IsEqualTo(TwoHundred);
    }

    /// <summary>Generic and non-generic enumerators traverse the writer iterator directly.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEnumeratorsUsedDirectlyThenTheyTraverseEveryItem()
    {
        var genericItems = new int[TwoItems];
        var genericIndex = 0;
        var nonGenericCount = 0;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            builder.Add(Seven);
            builder.Add(Eight);
            var enumerable = builder.AsEnumerable();
            foreach (var item in enumerable)
            {
                genericItems[genericIndex] = item;
                genericIndex++;
            }

            foreach (var item in (System.Collections.IEnumerable)enumerable)
            {
                if (item is int)
                {
                    nonGenericCount++;
                }
            }
        }

        await Assert.That(genericItems).IsEquivalentTo([Seven, Eight]);
        await Assert.That(nonGenericCount).IsEqualTo(TwoItems);
    }

    /// <summary>Builder can hold more than the initial capacity (pool growth).</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenManyItemsAddedThenBuilderGrowsCorrectly()
    {
        int count;
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            for (var i = 0; i < OneHundred; i++)
            {
                builder.Add(i);
            }

            count = builder.Count;
            result = builder.ToImmutable();
        }

        await Assert.That(count).IsEqualTo(OneHundred);
        await Assert.That(result[NinetyNine]).IsEqualTo(NinetyNine);
    }

    /// <summary>ToImmutable on an empty builder returns an empty ImmutableArray.</summary>
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

    /// <summary>ToString returns the WrittenSpan string representation without throwing.</summary>
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

    /// <summary>Dispose can be called multiple times without throwing.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDisposedTwiceThenDoesNotThrow()
    {
        var builder = ImmutableArrayBuilder<int>.Rent();
        builder.Add(1);
        var completed = false;
        builder.Dispose();
        builder.Dispose();
        completed = true;
        await Assert.That(completed).IsTrue();
    }

    /// <summary>AddRange followed by Add correctly appends items in order.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAddRangeThenAddThenOrderPreserved()
    {
        ImmutableArray<int> result;
        using (var builder = ImmutableArrayBuilder<int>.Rent())
        {
            ReadOnlySpan<int> range = [1, TwoItems, ThreeItems];
            builder.AddRange(range);
            builder.Add(FourItems);
            result = builder.ToImmutable();
        }

        await Assert.That(result.Length).IsEqualTo(FourItems);
        await Assert.That(result[ThreeItems]).IsEqualTo(FourItems);
    }
}
