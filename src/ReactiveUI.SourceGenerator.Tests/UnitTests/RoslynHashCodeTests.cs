// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias RoslynSourceGenerator;

using RoslynHashCode = RoslynSourceGenerator::System.HashCode;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests the Roslyn-targeted <c>System.HashCode</c> polyfill.</summary>
public sealed class RoslynHashCodeTests
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

    /// <summary>The sixth test value.</summary>
    private const int SixthValue = 6;

    /// <summary>The seventh test value.</summary>
    private const int SeventhValue = 7;

    /// <summary>The eighth test value.</summary>
    private const int EighthValue = 8;

    /// <summary>Equivalent sequences produce value-equal hash states.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task EquivalentSequences_AreEqualAndProduceEqualHashes()
    {
        var first = CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue, FifthValue, SixthValue, SeventhValue, EighthValue);
        var second = CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue, FifthValue, SixthValue, SeventhValue, EighthValue);
        object boxedSecond = second;

        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(first.Equals(boxedSecond)).IsTrue();
        await Assert.That(first.Equals(null)).IsFalse();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
        await Assert.That(first.ToHashCode()).IsEqualTo(second.ToHashCode());
    }

    /// <summary>Queue sizes, null values, and differing states are handled.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task DifferentSequences_ExerciseEveryQueuePosition()
    {
        var empty = CreateHash();
        var one = CreateHash(FirstValue);
        var two = CreateHash(FirstValue, SecondValue);
        var three = CreateHash(FirstValue, SecondValue, ThirdValue);
        var four = CreateHash(FirstValue, SecondValue, ThirdValue, FourthValue);
        var withNull = default(RoslynHashCode);
        var withZero = default(RoslynHashCode);

        withNull.Add<string?>(null);
        withZero.Add(0);

        await Assert.That(empty.ToHashCode()).IsEqualTo(empty.GetHashCode());
        await Assert.That(one.ToHashCode()).IsEqualTo(one.GetHashCode());
        await Assert.That(two.ToHashCode()).IsEqualTo(two.GetHashCode());
        await Assert.That(three.ToHashCode()).IsEqualTo(three.GetHashCode());
        await Assert.That(four.ToHashCode()).IsEqualTo(four.GetHashCode());
        await Assert.That(withNull.Equals(withZero)).IsTrue();
        await Assert.That(withNull.ToHashCode()).IsEqualTo(withZero.ToHashCode());
        await Assert.That(four.Equals(CreateHash(FourthValue, ThirdValue, SecondValue, FirstValue))).IsFalse();
        await Assert.That(four.Equals("not a hash state")).IsFalse();
    }

    /// <summary>Creates a Roslyn hash state from an ordered sequence.</summary>
    /// <param name="values">Values to add.</param>
    /// <returns>The populated hash state.</returns>
    private static RoslynHashCode CreateHash(params int[] values)
    {
        var hash = default(RoslynHashCode);

        foreach (var value in values)
        {
            hash.Add(value);
        }

        return hash;
    }
}
