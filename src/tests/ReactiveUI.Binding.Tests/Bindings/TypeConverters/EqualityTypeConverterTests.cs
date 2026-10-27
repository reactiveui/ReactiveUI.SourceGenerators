// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the EqualityTypeConverter which compares objects for equality.</summary>
public class EqualityTypeConverterTests
{
    /// <summary>A value deliberately different from SampleValue, so equality fails.</summary>
    private const int DifferentValue = 43;

    /// <summary>A sample value used by these tests.</summary>
    private const int SampleValue = 42;

    /// <summary>Verifies FromType ReturnsObjectType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsObjectType()
    {
        var converter = new EqualityTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(object));
    }

    /// <summary>Verifies GetAffinityForObjects Returns1.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns1()
    {
        var converter = new EqualityTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(1);
    }

    /// <summary>Verifies ToType ReturnsBoolType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsBoolType()
    {
        var converter = new EqualityTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(bool));
    }

    /// <summary>Verifies TryConvertTyped BothNull ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_BothNull_ReturnsTrue()
    {
        var converter = new EqualityTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsTrue();
    }

    /// <summary>Verifies TryConvertTyped DifferentIntegers ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_DifferentIntegers_ReturnsFalse()
    {
        var converter = new EqualityTypeConverter();
        const int obj = 42;

        var result = converter.TryConvertTyped(obj, DifferentValue, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsFalse();
    }

    /// <summary>Verifies TryConvertTyped DifferentTypes ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_DifferentTypes_ReturnsFalse()
    {
        var converter = new EqualityTypeConverter();
        const string obj = "42";

        var result = converter.TryConvertTyped(obj, SampleValue, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsFalse();
    }

    /// <summary>Verifies TryConvertTyped DifferentValues ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_DifferentValues_ReturnsFalse()
    {
        var converter = new EqualityTypeConverter();
        const string obj = "test";

        var result = converter.TryConvertTyped(obj, "other", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsFalse();
    }

    /// <summary>Verifies TryConvertTyped EqualIntegers ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_EqualIntegers_ReturnsTrue()
    {
        var converter = new EqualityTypeConverter();
        const int obj = 42;

        var result = converter.TryConvertTyped(obj, SampleValue, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsTrue();
    }

    /// <summary>Verifies TryConvertTyped EqualStrings ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_EqualStrings_ReturnsTrue()
    {
        var converter = new EqualityTypeConverter();
        const string obj = "hello";

        var result = converter.TryConvertTyped(obj, "hello", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsTrue();
    }

    /// <summary>Verifies TryConvertTyped EqualValues ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_EqualValues_ReturnsTrue()
    {
        var converter = new EqualityTypeConverter();
        const string obj = "test";

        var result = converter.TryConvertTyped(obj, "test", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsTrue();
    }

    /// <summary>Verifies TryConvertTyped NoConversionHint UseNullComparison.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NoConversionHint_UseNullComparison()
    {
        var converter = new EqualityTypeConverter();
        const string obj = "test";

        var result = converter.TryConvertTyped(obj, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsFalse();
    }

    /// <summary>Verifies TryConvertTyped OneNull ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_OneNull_ReturnsFalse()
    {
        var converter = new EqualityTypeConverter();

        var result1 = converter.TryConvertTyped("test", null, out var output1);
        var result2 = converter.TryConvertTyped(null, "test", out var output2);

        await Assert.That(result1).IsTrue();
        await Assert.That((bool)output1!).IsFalse();
        await Assert.That(result2).IsTrue();
        await Assert.That((bool)output2!).IsFalse();
    }

    /// <summary>Verifies TryConvertTyped ReferenceEquality ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ReferenceEquality_ReturnsTrue()
    {
        var converter = new EqualityTypeConverter();
        var obj = new object();

        var result = converter.TryConvertTyped(obj, obj, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsTrue();
    }

    /// <summary>Verifies TryConvertTyped ValueEquality ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValueEquality_ReturnsTrue()
    {
        var converter = new EqualityTypeConverter();
        var obj = new { Value = "test" };
        var other = new { Value = "test" };

        var result = converter.TryConvertTyped(obj, other, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That((bool)output!).IsTrue();
    }
}
