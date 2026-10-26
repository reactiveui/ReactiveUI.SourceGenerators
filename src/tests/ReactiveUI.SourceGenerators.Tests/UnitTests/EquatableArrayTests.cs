// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="EquatableArray{T}"/>.</summary>
public sealed class EquatableArrayTests
{
    /// <summary>The first element index.</summary>
    private const int FirstIndex = 0;

    /// <summary>The second element index.</summary>
    private const int SecondIndex = 1;

    /// <summary>The third element index.</summary>
    private const int ThirdIndex = 2;

    /// <summary>The expected number of elements in a three-element array.</summary>
    private const int ThreeElements = 3;

    /// <summary>The first value used by the tests.</summary>
    private const int FirstValue = 1;

    /// <summary>The second value used by the tests.</summary>
    private const int SecondValue = 2;

    /// <summary>The third value used by the tests.</summary>
    private const int ThirdValue = 3;

    /// <summary>The value used to make an array unequal.</summary>
    private const int DifferentValue = 4;

    /// <summary>Two arrays with identical elements are equal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenSameElementsThenEqual()
    {
        var a = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var b = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a != b).IsFalse();
    }

    /// <summary>Two arrays with different elements are not equal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDifferentElementsThenNotEqual()
    {
        var a = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var b = new EquatableArray<int>([FirstValue, SecondValue, DifferentValue]);

        await Assert.That(a == b).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    /// <summary>Arrays with the same elements in different order are not equal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDifferentOrderThenNotEqual()
    {
        var a = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var b = new EquatableArray<int>([ThirdValue, SecondValue, FirstValue]);

        await Assert.That(a == b).IsFalse();
    }

    /// <summary>An empty array equals another empty array and a default one.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenBothEmptyThenEqual()
    {
        var a = new EquatableArray<int>([]);
        var b = default(EquatableArray<int>);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.IsEmpty).IsTrue();
        await Assert.That(b.Length).IsEqualTo(0);
        await Assert.That(b.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>An empty array is not equal to a non-empty array.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenOneEmptyThenNotEqual()
    {
        var a = new EquatableArray<int>([]);
        var b = new EquatableArray<int>([FirstValue]);

        await Assert.That(a == b).IsFalse();
        await Assert.That(b.Equals(a)).IsFalse();
    }

    /// <summary>The indexer returns the element at the given position.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenIndexedThenReturnsCorrectElement()
    {
        var arr = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);

        await Assert.That(arr[FirstIndex]).IsEqualTo(FirstValue);
        await Assert.That(arr[SecondIndex]).IsEqualTo(SecondValue);
        await Assert.That(arr[ThirdIndex]).IsEqualTo(ThirdValue);
    }

    /// <summary>A Roslyn array is wrapped without copying and keeps its elements.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenWrappedFromRoslynThenKeepsElements()
    {
        var wrapped = EquatableArray<int>.FromRoslyn([FirstValue, SecondValue]);
        var fromDefault = EquatableArray<int>.FromRoslyn(default);

        await Assert.That(wrapped.Length).IsEqualTo(SecondValue);
        await Assert.That(wrapped[SecondIndex]).IsEqualTo(SecondValue);
        await Assert.That(fromDefault.IsEmpty).IsTrue();
    }

    /// <summary>ToArray returns a copy that does not share storage.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToArrayCalledThenReturnsIndependentCopy()
    {
        var arr = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var copy = arr.ToArray();
        copy[FirstIndex] = DifferentValue;

        await Assert.That(copy.Length).IsEqualTo(ThreeElements);
        await Assert.That(arr[FirstIndex]).IsEqualTo(FirstValue);
        await Assert.That(default(EquatableArray<int>).ToArray().Length).IsEqualTo(0);
    }

    /// <summary>AsSpan returns a span over the elements.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAsSpanCalledThenSpanCoversElements()
    {
        var arr = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var span = arr.AsSpan();
        var length = span.Length;
        var middle = span[SecondIndex];

        await Assert.That(length).IsEqualTo(ThreeElements);
        await Assert.That(middle).IsEqualTo(SecondValue);
    }

    /// <summary>GetHashCode returns the same value for equal arrays.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualArraysThenSameHashCode()
    {
        var a = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var b = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>Equals(object) compares by value and rejects other types.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualsObjectCalledThenComparesByValue()
    {
        var a = new EquatableArray<int>([FirstValue, SecondValue]);
        object same = new EquatableArray<int>([FirstValue, SecondValue]);
        object different = new EquatableArray<int>([SecondValue]);

        await Assert.That(a.Equals(same)).IsTrue();
        await Assert.That(a.Equals(different)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }

    /// <summary>The IEnumerable implementation yields every element in order.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEnumeratedAsIEnumerableThenYieldsInOrder()
    {
        IEnumerable<int> arr = new EquatableArray<int>([FirstValue, SecondValue, ThirdValue]);
        var list = new List<int>(arr);

        await Assert.That(list.Count).IsEqualTo(ThreeElements);
        await Assert.That(list[FirstIndex]).IsEqualTo(FirstValue);
        await Assert.That(list[SecondIndex]).IsEqualTo(SecondValue);
        await Assert.That(list[ThirdIndex]).IsEqualTo(ThirdValue);
    }
}
