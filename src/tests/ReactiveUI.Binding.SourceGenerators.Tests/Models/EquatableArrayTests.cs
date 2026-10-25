// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Models;

/// <summary>Tests for <see cref="EquatableArray{T}"/> methods.</summary>
public class EquatableArrayTests
{
    /// <summary>The fully qualified name of the <c>Int32</c> type used by these tests.</summary>
    private const string Int32TypeName = "global::System.Int32";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the <c>global::T</c> type used by these tests.</summary>
    private const string GlobalTTypeName = "global::T";

    /// <summary>The fully qualified name of the <c>VM</c> type used by these tests.</summary>
    private const string VMTypeName = "global::TestApp.VM";

    /// <summary>Verifies ComputeHashCode returns 0 for null array.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_NullArray_ReturnsZero()
    {
        var result = EquatableArray<PropertyPathSegment>.ComputeHashCode(null);

        await Assert.That(result).IsEqualTo(0);
    }

    /// <summary>Verifies ComputeHashCode returns consistent hash for empty array.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_EmptyArray_ReturnsConsistentHash()
    {
        const int Expected = 17;
        var hash1 = EquatableArray<PropertyPathSegment>.ComputeHashCode([]);
        var hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode([]);

        await Assert.That(hash1).IsEqualTo(hash2);
        await Assert.That(hash1).IsEqualTo(Expected); // Hash seed with no elements
    }

    /// <summary>Verifies ComputeHashCode returns same hash for same content in different arrays.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_SameContent_ReturnsSameHash()
    {
        var seg = new PropertyPathSegment("Name", StringTypeName, VMTypeName, true, null);
        var arr1 = new[] { seg };
        var arr2 = new[] { new PropertyPathSegment("Name", StringTypeName, VMTypeName, true, null) };

        var hash1 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr1);
        var hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr2);

        await Assert.That(hash1).IsEqualTo(hash2);
    }

    /// <summary>Verifies ComputeHashCode returns different hash for different content.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_DifferentContent_ReturnsDifferentHash()
    {
        var arr1 = new[] { new PropertyPathSegment("Name", StringTypeName, VMTypeName, true, null) };
        var arr2 = new[] { new PropertyPathSegment("Age", Int32TypeName, VMTypeName, false, null) };

        var hash1 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr1);
        var hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr2);

        await Assert.That(hash1).IsNotEqualTo(hash2);
    }

    /// <summary>Verifies EquatableArray constructor caches hash code matching ComputeHashCode.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_CachesHashCode_MatchesComputeHashCode()
    {
        var seg = new PropertyPathSegment("Name", StringTypeName, VMTypeName, true, null);
        var arr = new[] { seg };

        var equatable = new EquatableArray<PropertyPathSegment>(arr);
        var expected = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr);

        await Assert.That(equatable.GetHashCode()).IsEqualTo(expected);
    }

    /// <summary>Verifies default-constructed EquatableArray has hash code 0.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultConstructed_HashCode_ReturnsZero() =>
        await Assert.That(default(EquatableArray<PropertyPathSegment>).GetHashCode()).IsEqualTo(0);

    /// <summary>Verifies equal arrays are equal via Equals method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_SameContent_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);
        var arr2 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);

        await Assert.That(arr1.Equals(arr2)).IsTrue();
    }

    /// <summary>Verifies different arrays are not equal via Equals method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_DifferentContent_ReturnsFalse()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);
        var arr2 = new EquatableArray<PropertyPathSegment>(
            [new("Age", Int32TypeName, VMTypeName, false, null)]);

        await Assert.That(arr1.Equals(arr2)).IsFalse();
    }

    /// <summary>Verifies default arrays are equal to each other.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_BothDefault_ReturnsTrue() =>
        await Assert.That(default(EquatableArray<PropertyPathSegment>)
                .Equals(default(EquatableArray<PropertyPathSegment>)))
            .IsTrue();

    /// <summary>Verifies Length returns the number of elements.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Length_MultipleElements_ReturnsCount()
    {
        const int ExpectedArrCount = 2;
        var arr = new EquatableArray<PropertyPathSegment>([
            new("A", StringTypeName, GlobalTTypeName, true, null),
            new("B", Int32TypeName, GlobalTTypeName, false, null)
        ]);

        await Assert.That(arr.Length).IsEqualTo(ExpectedArrCount);
    }

    /// <summary>Verifies Length returns 0 for default-constructed array.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Length_DefaultConstructed_ReturnsZero() =>
        await Assert.That(default(EquatableArray<PropertyPathSegment>).Length).IsEqualTo(0);

    /// <summary>
    /// Verifies ComputeHashCode handles null elements in the array by using 0 for their hash code.
    /// Covers EquatableArray.cs line 156 (array[i]?.GetHashCode() ?? 0 with null element).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComputeHashCode_NullElements_UsesZeroForNullHashCode()
    {
        var arr = new PropertyPathSegment?[] { null!, new("Name", StringTypeName, GlobalTTypeName, true, null), null! };

        // The hash should incorporate 0 for null elements and the actual hash for non-null
        var hash = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr!);

        // Verify it's deterministic (not crashing on null)
        var hash2 = EquatableArray<PropertyPathSegment>.ComputeHashCode(arr!);
        await Assert.That(hash).IsEqualTo(hash2);

        // Verify it differs from an array with all non-null elements
        var nonNullArr = new[]
        {
            new PropertyPathSegment("A", StringTypeName, GlobalTTypeName, true, null),
            new PropertyPathSegment("Name", StringTypeName, GlobalTTypeName, true, null),
            new PropertyPathSegment("B", StringTypeName, GlobalTTypeName, true, null),
        };

        var nonNullHash = EquatableArray<PropertyPathSegment>.ComputeHashCode(nonNullArr);
        await Assert.That(hash).IsNotEqualTo(nonNullHash);
    }

    /// <summary>Verifies operator== returns true for equal arrays.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OperatorEquals_SameContent_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, GlobalTTypeName, true, null)]);
        var arr2 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, GlobalTTypeName, true, null)]);

        await Assert.That(arr1 == arr2).IsTrue();
    }

    /// <summary>Verifies operator!= returns true for different arrays.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OperatorNotEquals_DifferentContent_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, GlobalTTypeName, true, null)]);
        var arr2 = new EquatableArray<PropertyPathSegment>(
            [new("Age", Int32TypeName, GlobalTTypeName, false, null)]);

        await Assert.That(arr1 != arr2).IsTrue();
    }

    /// <summary>Verifies the indexer returns the correct element at each position.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Indexer_ReturnsCorrectElement()
    {
        var seg0 = new PropertyPathSegment("Name", StringTypeName, VMTypeName, true, null);
        var seg1 = new PropertyPathSegment("Age", Int32TypeName, VMTypeName, false, null);
        var arr = new EquatableArray<PropertyPathSegment>([seg0, seg1]);

        await Assert.That(arr[0]).IsEqualTo(seg0);
        await Assert.That(arr[1]).IsEqualTo(seg1);
    }

    /// <summary>Verifies Equals(object) returns true when passed an equal EquatableArray boxed as object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_ObjectOverload_WithSameType_ReturnsTrue()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);
        object obj = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);

        await Assert.That(arr1.Equals(obj)).IsTrue();
    }

    /// <summary>Verifies Equals(object) returns false when passed a different type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_ObjectOverload_WithDifferentType_ReturnsFalse()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);

        await Assert.That(arr1.Equals("not an array")).IsFalse();
    }

    /// <summary>Verifies Equals(object) returns false when passed null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_ObjectOverload_WithNull_ReturnsFalse()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);

        await Assert.That(arr1.Equals((object?)null)).IsFalse();
    }

    /// <summary>Verifies a default array is not equal to a non-default array.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_DefaultVsNonDefault_ReturnsFalse()
    {
        var defaultArr = default(EquatableArray<PropertyPathSegment>);
        var nonDefaultArr = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);

        await Assert.That(defaultArr.Equals(nonDefaultArr)).IsFalse();
    }

    /// <summary>Verifies arrays with different lengths are not equal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_DifferentLengths_ReturnsFalse()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);
        var arr2 = new EquatableArray<PropertyPathSegment>([
            new("Name", StringTypeName, VMTypeName, true, null),
            new("Age", Int32TypeName, VMTypeName, false, null)
        ]);

        await Assert.That(arr1.Equals(arr2)).IsFalse();
    }

    /// <summary>Verifies the generic enumerator iterates all elements in order.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetEnumerator_IteratesAllElements()
    {
        const int ExpectedListCount = 3;
        var seg0 = new PropertyPathSegment("A", StringTypeName, VMTypeName, true, null);
        var seg1 = new PropertyPathSegment("B", Int32TypeName, VMTypeName, false, null);
        var seg2 = new PropertyPathSegment("C", "global::System.Boolean", VMTypeName, false, null);
        var arr = new EquatableArray<PropertyPathSegment>([seg0, seg1, seg2]);

        var list = arr.ToList();

        await Assert.That(list.Count).IsEqualTo(ExpectedListCount);
        await Assert.That(list[0]).IsEqualTo(seg0);
        await Assert.That(list[1]).IsEqualTo(seg1);
        await Assert.That(list[2]).IsEqualTo(seg2);
    }

    /// <summary>Verifies iterating a default-constructed array yields no elements.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetEnumerator_DefaultArray_IteratesNothing()
    {
        var arr = default(EquatableArray<PropertyPathSegment>);

        var count = 0;
        foreach (var item in arr)
        {
            count++;
        }

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>Verifies the non-generic IEnumerable.GetEnumerator works correctly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonGenericGetEnumerator_Works()
    {
        const int Expected = 2;
        var arr = new EquatableArray<PropertyPathSegment>([
            new("Name", StringTypeName, VMTypeName, true, null),
            new("Age", Int32TypeName, VMTypeName, false, null)
        ]);

        var enumerator = ((IEnumerable)arr).GetEnumerator();
        var count = 0;
        while (enumerator.MoveNext())
        {
            count++;
        }

        await Assert.That(count).IsEqualTo(Expected);
    }

    /// <summary>Verifies operator== returns true for two default-constructed instances.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OperatorEquals_BothDefault_ReturnsTrue() =>
        await Assert.That(default(EquatableArray<PropertyPathSegment>) == default(EquatableArray<PropertyPathSegment>))
            .IsTrue();

    /// <summary>Verifies operator!= returns false for arrays with the same content.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OperatorNotEquals_SameContent_ReturnsFalse()
    {
        var arr1 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);
        var arr2 = new EquatableArray<PropertyPathSegment>(
            [new("Name", StringTypeName, VMTypeName, true, null)]);

        await Assert.That(arr1 != arr2).IsFalse();
    }
}
