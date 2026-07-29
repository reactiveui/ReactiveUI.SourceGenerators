// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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

    /// <summary>The expected number of elements in a two-element array.</summary>
    private const int TwoElements = 2;

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

    /// <summary>The fifth value used by the tests.</summary>
    private const int FifthValue = 5;

    /// <summary>The sixth value used by the tests.</summary>
    private const int SixthValue = 6;

    /// <summary>The seventh value used by the tests.</summary>
    private const int SeventhValue = 7;

    /// <summary>The eighth value used by the tests.</summary>
    private const int EighthValue = 8;

    /// <summary>The ninth value used by the tests.</summary>
    private const int NinthValue = 9;

    /// <summary>The tenth value used by the tests.</summary>
    private const int TenthValue = 10;

    /// <summary>The twentieth value used by the tests.</summary>
    private const int TwentiethValue = 20;

    /// <summary>The thirtieth value used by the tests.</summary>
    private const int ThirtiethValue = 30;

    /// <summary>Two arrays with identical elements are equal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenSameElementsThenEqual()
    {
        var a = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();
        var b = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a != b).IsFalse();
    }

    /// <summary>Two arrays with different elements are not equal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDifferentElementsThenNotEqual()
    {
        var a = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();
        var b = ImmutableArray.Create(FirstValue, SecondValue, DifferentValue).AsEquatableArray();

        await Assert.That(a == b).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    /// <summary>Arrays with the same elements in different order are not equal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenDifferentOrderThenNotEqual()
    {
        var a = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();
        var b = ImmutableArray.Create(ThirdValue, SecondValue, FirstValue).AsEquatableArray();

        await Assert.That(a == b).IsFalse();
    }

    /// <summary>An empty array equals another empty array.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenBothEmptyThenEqual()
    {
        var a = ImmutableArray<int>.Empty.AsEquatableArray();
        var b = ImmutableArray<int>.Empty.AsEquatableArray();

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.IsEmpty).IsTrue();
    }

    /// <summary>An empty array is not equal to a non-empty array.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenOneEmptyThenNotEqual()
    {
        var a = ImmutableArray<int>.Empty.AsEquatableArray();
        var b = ImmutableArray.Create(FirstValue).AsEquatableArray();

        await Assert.That(a == b).IsFalse();
    }

    /// <summary>The indexer returns the element at the given position.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenIndexedThenReturnsCorrectElement()
    {
        var arr = ImmutableArray.Create(TenthValue, TwentiethValue, ThirtiethValue).AsEquatableArray();

        await Assert.That(arr[FirstIndex]).IsEqualTo(TenthValue);
        await Assert.That(arr[SecondIndex]).IsEqualTo(TwentiethValue);
        await Assert.That(arr[ThirdIndex]).IsEqualTo(ThirtiethValue);
    }

    /// <summary>Enumeration yields all elements in order.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEnumeratedThenYieldsAllElements()
    {
        var expected = new[] { FirstValue, SecondValue, ThirdValue };
        var arr = ImmutableArray.Create(expected).AsEquatableArray();
        var actual = new List<int>(arr);

        await Assert.That(actual.Count).IsEqualTo(ThreeElements);
        await Assert.That(actual[FirstIndex]).IsEqualTo(FirstValue);
        await Assert.That(actual[SecondIndex]).IsEqualTo(SecondValue);
        await Assert.That(actual[ThirdIndex]).IsEqualTo(ThirdValue);
    }

    /// <summary>Implicit conversion from ImmutableArray preserves elements.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenImplicitlyConvertedFromImmutableArrayThenPreservesElements()
    {
        EquatableArray<int> equatable = ImmutableArray.Create(FifthValue, SixthValue, SeventhValue);

        await Assert.That(equatable[FirstIndex]).IsEqualTo(FifthValue);
        await Assert.That(equatable[SecondIndex]).IsEqualTo(SixthValue);
        await Assert.That(equatable[ThirdIndex]).IsEqualTo(SeventhValue);
    }

    /// <summary>Implicit conversion to ImmutableArray preserves elements.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenImplicitlyConvertedToImmutableArrayThenPreservesElements()
    {
        ImmutableArray<int> immutable = ImmutableArray.Create(EighthValue, NinthValue).AsEquatableArray();

        await Assert.That(immutable.Length).IsEqualTo(TwoElements);
        await Assert.That(immutable[FirstIndex]).IsEqualTo(EighthValue);
        await Assert.That(immutable[SecondIndex]).IsEqualTo(NinthValue);
    }

    /// <summary>ToArray returns a mutable copy with the same elements.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenToArrayCalledThenReturnsMutableCopy()
    {
        var arr = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();
        var copy = arr.ToArray();

        await Assert.That(copy.Length).IsEqualTo(ThreeElements);
        await Assert.That(copy[FirstIndex]).IsEqualTo(FirstValue);
        await Assert.That(copy[ThirdIndex]).IsEqualTo(ThirdValue);
    }

    /// <summary>AsSpan returns a span over the elements.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAsSpanCalledThenSpanCoversElements()
    {
        var arr = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();
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
        var a = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();
        var b = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue).AsEquatableArray();

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    /// <summary>Equals(object) returns true when passed an equal EquatableArray.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualsObjectCalledWithEqualArrayThenReturnsTrue()
    {
        var a = ImmutableArray.Create(FirstValue, SecondValue).AsEquatableArray();
        object b = ImmutableArray.Create(FirstValue, SecondValue).AsEquatableArray();

        await Assert.That(a.Equals(b)).IsTrue();
    }

    /// <summary>Equals(object) returns false when passed null.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEqualsObjectCalledWithNullThenReturnsFalse()
    {
        var a = ImmutableArray.Create(FirstValue).AsEquatableArray();

        await Assert.That(a.Equals(null)).IsFalse();
    }

    /// <summary>AsImmutableArray round-trips back to ImmutableArray.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAsImmutableArrayCalledThenRoundTrips()
    {
        var source = ImmutableArray.Create("x", "y", "z");
        var equatable = source.AsEquatableArray();
        var roundTripped = equatable.AsImmutableArray();

        await Assert.That(roundTripped.SequenceEqual(source)).IsTrue();
    }

    /// <summary>FromImmutableArray static factory produces an equal instance to the extension method.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenCreatedViaFactoryThenEqualsExtensionMethod()
    {
        var immutable = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue);
        var viaExtension = immutable.AsEquatableArray();
        var viaFactory = EquatableArray<int>.FromImmutableArray(immutable);

        await Assert.That(viaExtension == viaFactory).IsTrue();
    }

    /// <summary>IEnumerable&lt;T&gt; explicit interface yields elements correctly.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenEnumeratedAsIEnumerableThenYieldsCorrectly()
    {
        IEnumerable<int> arr = ImmutableArray.Create(SeventhValue, EighthValue, NinthValue).AsEquatableArray();
        var list = new List<int>(arr);

        await Assert.That(list.Count).IsEqualTo(ThreeElements);
        await Assert.That(list[FirstIndex]).IsEqualTo(SeventhValue);
        await Assert.That(list[SecondIndex]).IsEqualTo(EighthValue);
        await Assert.That(list[ThirdIndex]).IsEqualTo(NinthValue);
    }
}
