// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="EquatableArray{T}"/>.
/// </summary>
public sealed class EquatableArrayTests
{
    /// <summary>
    /// Two arrays with identical elements are equal.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenSameElementsThenEqual()
    {
        var a = ImmutableArray.Create(1, 2, 3).AsEquatableArray();
        var b = ImmutableArray.Create(1, 2, 3).AsEquatableArray();

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a != b).IsFalse();
    }

    /// <summary>
    /// Two arrays with different elements are not equal.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDifferentElementsThenNotEqual()
    {
        var a = ImmutableArray.Create(1, 2, 3).AsEquatableArray();
        var b = ImmutableArray.Create(1, 2, 4).AsEquatableArray();

        await Assert.That(a == b).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    /// <summary>
    /// Arrays with the same elements in different order are not equal.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDifferentOrderThenNotEqual()
    {
        var a = ImmutableArray.Create(1, 2, 3).AsEquatableArray();
        var b = ImmutableArray.Create(3, 2, 1).AsEquatableArray();

        await Assert.That(a == b).IsFalse();
    }

    /// <summary>
    /// An empty array equals another empty array.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenBothEmptyThenEqual()
    {
        var a = ImmutableArray<int>.Empty.AsEquatableArray();
        var b = ImmutableArray<int>.Empty.AsEquatableArray();

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.IsEmpty).IsTrue();
    }

    /// <summary>
    /// An empty array is not equal to a non-empty array.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenOneEmptyThenNotEqual()
    {
        var a = ImmutableArray<int>.Empty.AsEquatableArray();
        var b = ImmutableArray.Create(1).AsEquatableArray();

        await Assert.That(a == b).IsFalse();
    }

    /// <summary>
    /// The indexer returns the element at the given position.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenIndexedThenReturnsCorrectElement()
    {
        var arr = ImmutableArray.Create(10, 20, 30).AsEquatableArray();

        await Assert.That(arr[0]).IsEqualTo(10);
        await Assert.That(arr[1]).IsEqualTo(20);
        await Assert.That(arr[2]).IsEqualTo(30);
    }

    /// <summary>
    /// Enumeration yields all elements in order.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEnumeratedThenYieldsAllElements()
    {
        var expected = new[] { 1, 2, 3 };
        var arr = ImmutableArray.Create(expected).AsEquatableArray();

        var actual = arr.ToList();

        await Assert.That(actual.Count).IsEqualTo(3);
        await Assert.That(actual[0]).IsEqualTo(1);
        await Assert.That(actual[1]).IsEqualTo(2);
        await Assert.That(actual[2]).IsEqualTo(3);
    }

    /// <summary>
    /// Implicit conversion from ImmutableArray preserves elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenImplicitlyConvertedFromImmutableArrayThenPreservesElements()
    {
        var immutable = ImmutableArray.Create(5, 6, 7);
        EquatableArray<int> equatable = immutable;

        await Assert.That(equatable[0]).IsEqualTo(5);
        await Assert.That(equatable[1]).IsEqualTo(6);
        await Assert.That(equatable[2]).IsEqualTo(7);
    }

    /// <summary>
    /// Implicit conversion to ImmutableArray preserves elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenImplicitlyConvertedToImmutableArrayThenPreservesElements()
    {
        var equatable = ImmutableArray.Create(8, 9).AsEquatableArray();
        ImmutableArray<int> immutable = equatable;

        await Assert.That(immutable.Length).IsEqualTo(2);
        await Assert.That(immutable[0]).IsEqualTo(8);
        await Assert.That(immutable[1]).IsEqualTo(9);
    }

    /// <summary>
    /// ToArray returns a mutable copy with the same elements.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToArrayCalledThenReturnsMutableCopy()
    {
        var arr = ImmutableArray.Create(1, 2, 3).AsEquatableArray();
        var copy = arr.ToArray();

        await Assert.That(copy.Length).IsEqualTo(3);
        await Assert.That(copy[0]).IsEqualTo(1);
        await Assert.That(copy[2]).IsEqualTo(3);
    }

    /// <summary>
    /// AsSpan returns a span over the elements.
    /// </summary>
    [Test]
    public void WhenAsSpanCalledThenSpanCoversElements()
    {
        var arr = ImmutableArray.Create(1, 2, 3).AsEquatableArray();
        var span = arr.AsSpan();
        var length = span.Length;
        var mid = span[1];

        if (length != 3)
        {
            throw new InvalidOperationException($"Expected span length 3, got {length}.");
        }

        if (mid != 2)
        {
            throw new InvalidOperationException($"Expected span[1] == 2, got {mid}.");
        }
    }

    /// <summary>
    /// GetHashCode returns the same value for equal arrays.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualArraysThenSameHashCode()
    {
        var a = ImmutableArray.Create(1, 2, 3).AsEquatableArray();
        var b = ImmutableArray.Create(1, 2, 3).AsEquatableArray();

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>
    /// Equals(object) returns true when passed an equal EquatableArray.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualsObjectCalledWithEqualArrayThenReturnsTrue()
    {
        var a = ImmutableArray.Create(1, 2).AsEquatableArray();
        object b = ImmutableArray.Create(1, 2).AsEquatableArray();

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>
    /// Equals(object) returns false when passed null.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualsObjectCalledWithNullThenReturnsFalse()
    {
        var a = ImmutableArray.Create(1).AsEquatableArray();

        await Assert.That(a.Equals(null)).IsFalse();
    }

    /// <summary>
    /// AsImmutableArray round-trips back to ImmutableArray.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAsImmutableArrayCalledThenRoundTrips()
    {
        var source = ImmutableArray.Create("x", "y", "z");
        var equatable = source.AsEquatableArray();
        var roundTripped = equatable.AsImmutableArray();

        await Assert.That(roundTripped.SequenceEqual(source)).IsTrue();
    }

    /// <summary>
    /// FromImmutableArray static factory produces an equal instance to the extension method.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenCreatedViaFactoryThenEqualsExtensionMethod()
    {
        var immutable = ImmutableArray.Create(1, 2, 3);
        var viaExtension = immutable.AsEquatableArray();
        var viaFactory = EquatableArray<int>.FromImmutableArray(immutable);

        await Assert.That(viaExtension == viaFactory).IsTrue();
    }

    /// <summary>
    /// IEnumerable&lt;T&gt; explicit interface yields elements correctly.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEnumeratedAsIEnumerableThenYieldsCorrectly()
    {
        IEnumerable<int> arr = ImmutableArray.Create(7, 8, 9).AsEquatableArray();
        var list = arr.ToList();

        await Assert.That(list.Count).IsEqualTo(3);
        await Assert.That(list[0]).IsEqualTo(7);
        await Assert.That(list[1]).IsEqualTo(8);
        await Assert.That(list[2]).IsEqualTo(9);
    }
}
