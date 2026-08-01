// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using CodeFixerHelpers = ReactiveUI.SourceGenerators.CodeFixers.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests the helper implementations used by the code-fix assembly.</summary>
public sealed class CodeFixerHelperTests
{
    /// <summary>The first test value.</summary>
    private const int FirstValue = 1;

    /// <summary>The second test value.</summary>
    private const int SecondValue = 2;

    /// <summary>The third test value.</summary>
    private const int ThirdValue = 3;

    /// <summary>The fourth test value.</summary>
    private const int FourthValue = 4;

    /// <summary>The fifth test value.</summary>
    private const int FifthValue = 5;

    /// <summary>The number of values that forces the builder to grow.</summary>
    private const int GrowthCount = 16;

    /// <summary>Equivalent hash states compare equally through typed and object equality.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task HashCode_WithEquivalentValues_UsesValueEquality()
    {
        var first = CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue, FifthValue);
        var second = CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue, FifthValue);
        object boxedSecond = second;
        IEquatable<CodeFixerHelpers.HashCode> equatable = first;

        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(equatable.Equals(second)).IsTrue();
        await Assert.That(first.Equals(boxedSecond)).IsTrue();
        await Assert.That(first.Equals(null)).IsFalse();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
        await Assert.That(first.ToHashCode()).IsEqualTo(second.ToHashCode());
    }

    /// <summary>Hash states retain value order and include null values consistently.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task HashCode_WithDifferentOrderOrNull_TracksTheAddedValues()
    {
        var ordered = CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue);
        var reordered = CreateHash(FourthValue, ThirdValue, SecondValue, FirstValue);
        var withNull = default(CodeFixerHelpers.HashCode);
        var withZero = default(CodeFixerHelpers.HashCode);

        withNull.Add<string?>(null);
        withZero.Add(0);

        await Assert.That(ordered.Equals(reordered)).IsFalse();
        await Assert.That(ordered.ToHashCode()).IsEqualTo(CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue).ToHashCode());
        await Assert.That(withNull.Equals(withZero)).IsTrue();
        await Assert.That(withNull.ToHashCode()).IsEqualTo(withZero.ToHashCode());
    }

    /// <summary>Equatable arrays use ordered element value equality for empty, single, and multiple values.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task EquatableArray_WithEmptySingleAndMultipleValues_UsesOrderedValueEquality()
    {
        var empty = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray(ImmutableArray<int>.Empty);
        var single = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray([FirstValue]);
        var multiple = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray([FirstValue, SecondValue, ThirdValue]);
        var reordered = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray([ThirdValue, SecondValue, FirstValue]);

        await Assert.That(empty.IsEmpty).IsTrue();
        await Assert.That(empty.Equals(CodeFixerHelpers.EquatableArray<int>.FromImmutableArray(ImmutableArray<int>.Empty))).IsTrue();
        await Assert.That(single[0]).IsEqualTo(FirstValue);
        await Assert.That(multiple.Equals(reordered)).IsFalse();
        await Assert.That(multiple.ToArray().SequenceEqual([FirstValue, SecondValue, ThirdValue])).IsTrue();
    }

    /// <summary>Equatable arrays support object equality, null, conversion, enumeration, and stable hashes for equal values.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task EquatableArray_WithEquivalentObject_RoundTripsAndHasEqualHashCode()
    {
        var source = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue);
        var array = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray(source);
        CodeFixerHelpers.EquatableArray<int> implicitlyConverted = source;
        var defaultArray = default(CodeFixerHelpers.EquatableArray<int>);
        object equivalent = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray(source);
        IEquatable<CodeFixerHelpers.EquatableArray<int>> equatable = array;
        ImmutableArray<int> roundTripped = array;
        IEnumerable<int> enumerable = array;
        var directEnumerator = array.GetEnumerator();
        var directlyEnumerated = new List<int>();
        while (directEnumerator.MoveNext())
        {
            directlyEnumerated.Add(directEnumerator.Current);
        }

        var nonGenericEnumerable = (System.Collections.IEnumerable)array;
        var nonGenericValues = CollectNonGenericValues(nonGenericEnumerable);

        await Assert.That(array.Equals(equivalent)).IsTrue();
        await Assert.That(array.Equals(null)).IsFalse();
        await Assert.That(equatable.Equals(array)).IsTrue();
        await Assert.That(implicitlyConverted == array).IsTrue();
        await Assert.That(implicitlyConverted != array).IsFalse();
        await Assert.That(defaultArray.GetHashCode()).IsEqualTo(0);
        await Assert.That(array.GetHashCode()).IsEqualTo(((CodeFixerHelpers.EquatableArray<int>)equivalent).GetHashCode());
        await Assert.That(roundTripped.SequenceEqual(source)).IsTrue();
        await Assert.That(enumerable.SequenceEqual(source)).IsTrue();
        await Assert.That(directlyEnumerated.SequenceEqual(source)).IsTrue();
        await Assert.That(nonGenericValues.SequenceEqual(source)).IsTrue();
    }

    /// <summary>The extension creates the same value as the factory.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task EquatableArrayExtension_WithImmutableArray_MatchesFactory()
    {
        var source = ImmutableArray.Create(FirstValue, SecondValue, ThirdValue);
        var fromExtension = CodeFixerHelpers.EquatableArrayExtensions.AsEquatableArray(source);
        var fromFactory = CodeFixerHelpers.EquatableArray<int>.FromImmutableArray(source);

        await Assert.That(fromExtension.Equals(fromFactory)).IsTrue();
    }

    /// <summary>A builder handles empty, single, growing, range, copy, and enumeration paths.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ImmutableArrayBuilder_WithValuesAndGrowth_PreservesContents()
    {
        ImmutableArray<int> empty;
        ImmutableArray<int> values;
        int[] copy;
        int count;
        int writtenLength;
        int[] enumeratedValues;
        int[] copiedValues;
        int[] nonGenericValues;
        bool isReadOnly;
        int collectionCount;
        ICollection<int> collection;
        using (var builder = CodeFixerHelpers.ImmutableArrayBuilder<int>.Rent())
        {
            empty = builder.ToImmutable();
            builder.Add(FirstValue);
            builder.AddRange([SecondValue, ThirdValue]);
            for (var value = FourthValue; value <= GrowthCount; value++)
            {
                builder.Add(value);
            }

            values = builder.ToImmutable();
            copy = builder.ToArray();
            count = builder.Count;
            writtenLength = builder.WrittenSpan.Length;
            enumeratedValues = [.. builder.AsEnumerable()];
            collection = (ICollection<int>)builder.AsEnumerable();
            collection.Add(GrowthCount + FirstValue);
            copiedValues = new int[collection.Count];
            collection.CopyTo(copiedValues, 0);
            nonGenericValues = CollectNonGenericValues((System.Collections.IEnumerable)builder.AsEnumerable());
            isReadOnly = collection.IsReadOnly;
            collectionCount = collection.Count;
        }

        await Assert.That(empty.IsEmpty).IsTrue();
        await Assert.That(count).IsEqualTo(GrowthCount);
        await Assert.That(writtenLength).IsEqualTo(GrowthCount);
        await Assert.That(values.SequenceEqual(Enumerable.Range(FirstValue, GrowthCount))).IsTrue();
        await Assert.That(copy.SequenceEqual(values)).IsTrue();
        await Assert.That(enumeratedValues.SequenceEqual(values)).IsTrue();
        await Assert.That(copiedValues.SequenceEqual(Enumerable.Range(FirstValue, GrowthCount + FirstValue))).IsTrue();
        await Assert.That(nonGenericValues.SequenceEqual(copiedValues)).IsTrue();
        await Assert.That(isReadOnly).IsTrue();
        await Assert.That(collectionCount).IsEqualTo(GrowthCount + FirstValue);
        await Assert.That(collection.Clear).Throws<NotSupportedException>();
        await Assert.That(() => collection.Contains(FirstValue)).Throws<NotSupportedException>();
        await Assert.That(() => collection.Remove(FirstValue)).Throws<NotSupportedException>();
    }

    /// <summary>A character builder returns its written text and supports interface disposal.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ImmutableArrayBuilder_WithCharacters_FormatsAndDisposesThroughInterface()
    {
        const string expected = "coverage";
        string actual;
        IDisposable disposable;
        var builder = CodeFixerHelpers.ImmutableArrayBuilder<char>.Rent();
        builder.AddRange(expected.AsSpan());
        actual = builder.ToString();
        disposable = (IDisposable)builder.AsEnumerable();
        disposable.Dispose();
        disposable.Dispose();
        builder.Dispose();

        await Assert.That(actual).IsEqualTo(expected);
    }

    /// <summary>Disposed builders and the unsupported collection operations report their intended failures.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ImmutableArrayBuilder_AfterDisposeAndUnsupportedCollectionCalls_ReportsErrors()
    {
        await Assert.That(DisposeBuilderTwice()).IsTrue();
        await Assert.That(AccessDisposedBuilder).Throws<NullReferenceException>();
        await Assert.That(UnsupportedCollectionOperation).Throws<NotSupportedException>();
    }

    /// <summary>Creates a hash from the provided values.</summary>
    /// <param name="values">The values to add.</param>
    /// <returns>The resulting hash state.</returns>
    private static CodeFixerHelpers.HashCode CreateHash(params int[] values)
    {
        var hash = default(CodeFixerHelpers.HashCode);
        foreach (var value in values)
        {
            hash.Add(value);
        }

        return hash;
    }

    /// <summary>Copies integer values from a non-generic enumerable without LINQ.</summary>
    /// <param name="source">The source values.</param>
    /// <returns>The copied integer values.</returns>
    private static int[] CollectNonGenericValues(System.Collections.IEnumerable source)
    {
        List<int> values = [];
        foreach (var value in source)
        {
            if (value is int integer)
            {
                values.Add(integer);
            }
        }

        return [.. values];
    }

    /// <summary>Accesses a disposed builder.</summary>
    private static void AccessDisposedBuilder()
    {
        var builder = CodeFixerHelpers.ImmutableArrayBuilder<int>.Rent();
        builder.Dispose();
        _ = builder.Count;
    }

    /// <summary>Disposes a builder twice.</summary>
    /// <returns><see langword="true"/> after both dispose calls complete.</returns>
    private static bool DisposeBuilderTwice()
    {
        var builder = CodeFixerHelpers.ImmutableArrayBuilder<int>.Rent();
        builder.Dispose();
        builder.Dispose();
        return true;
    }

    /// <summary>Invokes an unsupported operation exposed by a builder enumeration.</summary>
    private static void UnsupportedCollectionOperation()
    {
        using var builder = CodeFixerHelpers.ImmutableArrayBuilder<int>.Rent();
        ICollection<int> collection = (ICollection<int>)builder.AsEnumerable();
        _ = collection.Contains(FirstValue);
    }
}
