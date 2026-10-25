// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>Tests for the internal <see cref="BindingTypeConverterRegistry.CloneRegistryShallow"/> method.</summary>
public class BindingTypeConverterRegistryTests
{
    /// <summary>Verifies that CloneRegistryShallow returns an empty dictionary for empty input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_EmptyDictionary_ReturnsEmpty()
    {
        var source = new Dictionary<ConverterTypePair, List<IBindingTypeConverter>>();

        var clone = BindingTypeConverterRegistry.CloneRegistryShallow(source);

        await Assert.That(clone.Count).IsEqualTo(0);
    }

    /// <summary>Verifies that CloneRegistryShallow returns a shallow copy with the same entries.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_WithEntries_ReturnsShallowCopy()
    {
        var converter = new TestConverter(typeof(string), typeof(int));
        var list = new List<IBindingTypeConverter> { converter };
        var source = new Dictionary<ConverterTypePair, List<IBindingTypeConverter>> { [new ConverterTypePair(typeof(string), typeof(int))] = list };

        var clone = BindingTypeConverterRegistry.CloneRegistryShallow(source);

        await Assert.That(clone.Count).IsEqualTo(1);
        await Assert.That(clone.ContainsKey(new(typeof(string), typeof(int)))).IsTrue();

        var clonedList = clone[new ConverterTypePair(typeof(string), typeof(int))];
        await Assert.That(clonedList.Count).IsEqualTo(1);
        await Assert.That(ReferenceEquals(clonedList, list)).IsTrue();
    }

    /// <summary>Verifies that modifying the clone does not affect the original dictionary.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_ModifyingClone_DoesNotAffectOriginal()
    {
        var converter = new TestConverter(typeof(string), typeof(int));
        var list = new List<IBindingTypeConverter> { converter };
        var source = new Dictionary<ConverterTypePair, List<IBindingTypeConverter>> { [new ConverterTypePair(typeof(string), typeof(int))] = list };

        var clone = BindingTypeConverterRegistry.CloneRegistryShallow(source);

        // Remove entry from clone
        _ = clone.Remove(new(typeof(string), typeof(int)));

        // Original should still have the entry
        await Assert.That(source.Count).IsEqualTo(1);
        await Assert.That(clone.Count).IsEqualTo(0);
    }

    /// <summary>Verifies that CloneRegistryShallow throws ArgumentNullException for null input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloneRegistryShallow_NullInput_ThrowsArgumentNullException() =>
        await Assert.That(static () => BindingTypeConverterRegistry.CloneRegistryShallow(null!))
            .ThrowsExactly<ArgumentNullException>();

    /// <summary>Minimal test implementation of <see cref="IBindingTypeConverter"/>.</summary>
    /// <param name="fromType">The source type this converter claims to handle.</param>
    /// <param name="toType">The target type this converter claims to handle.</param>
    private sealed class TestConverter(Type fromType, Type toType) : IBindingTypeConverter
    {
        /// <summary>Affinity returned by this stub converter indicating a strong match.</summary>
        private const int StubAffinity = 2;

        /// <inheritdoc/>
        public Type FromType { get; } = fromType;

        /// <inheritdoc/>
        public Type ToType { get; } = toType;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects() => StubAffinity;

        /// <inheritdoc/>
        public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
        {
            result = null;
            return false;
        }
    }
}
