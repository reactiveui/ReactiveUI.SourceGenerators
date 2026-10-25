// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>Tests for the converter registries.</summary>
public class ConverterRegistryTests
{
    /// <summary>A negative affinity, which means the converter does not apply.</summary>
    private const int NegativeAffinity = -5;

    /// <summary>The affinity a plain test converter reports unless a test needs a ranking.</summary>
    private const int DefaultAffinity = 5;

    /// <summary>Affinity ranking above <see cref="DefaultAffinity"/>.</summary>
    private const int AboveDefaultAffinity = 8;

    /// <summary>Affinity ranking that outranks every other converter registered in a test.</summary>
    private const int HighAffinity = 10;

    /// <summary>The number of concurrent read iterations in the thread-safety test.</summary>
    private const int ConcurrentReadIterations = 100;

    /// <summary>The iteration index at which a concurrent write is interleaved.</summary>
    private const int ConcurrentWriteIteration = 50;

    /// <summary>The expected number of converters after registering three.</summary>
    private const int ExpectedThreeConverters = 3;

    /// <summary>The expected number of converters after registering two.</summary>
    private const int ExpectedTwoConverters = 2;

    /// <summary>Verifies that the registry supports concurrent reads during registration.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ConcurrentReads_DuringRegistration_ShouldBeThreadSafe()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var converter1 = new TestConverter<int, string>(DefaultAffinity);
        var converter2 = new TestConverter<double, bool>(ExpectedThreeConverters);
        registry.Register(converter1);

        var readTasks = new List<Task<IBindingTypeConverter?>>();
        var writeTasks = new List<Task>();

        // Act - Start concurrent reads and writes
        for (var i = 0; i < ConcurrentReadIterations; i++)
        {
            // Concurrent reads
            readTasks.Add(Task.Run(() => registry.TryGetConverter(typeof(int), typeof(string))));

            // Concurrent write
            if (i == ConcurrentWriteIteration)
            {
                writeTasks.Add(Task.Run(() => registry.Register(converter2)));
            }
        }

        await Task.WhenAll(readTasks.Concat(writeTasks));

        // Assert - All reads should have completed successfully
        foreach (var task in readTasks)
        {
            var result = await task;
            await Assert.That(result).IsNotNull(); // Should always get converter1
        }

        // Verify both converters are registered
        var finalCheck1 = registry.TryGetConverter(typeof(int), typeof(string));
        var finalCheck2 = registry.TryGetConverter(typeof(double), typeof(bool));

        await Assert.That(finalCheck1).IsEqualTo(converter1);
        await Assert.That(finalCheck2).IsEqualTo(converter2);
    }

    /// <summary>Verifies that converters with negative affinity are ignored.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ConverterWithNegativeAffinity_ShouldBeIgnored()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var negativeAffinity = new TestConverter<int, string>(NegativeAffinity);
        var validAffinity = new TestConverter<int, string>(ExpectedTwoConverters);

        // Act
        registry.Register(negativeAffinity);
        registry.Register(validAffinity);

        var selected = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(selected).IsEqualTo(validAffinity);
    }

    /// <summary>Verifies that converters with affinity 0 are ignored.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ConverterWithZeroAffinity_ShouldBeIgnored()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var zeroAffinity = new TestConverter<int, string>(0);
        var validAffinity = new TestConverter<int, string>(ExpectedTwoConverters);

        // Act
        registry.Register(zeroAffinity);
        registry.Register(validAffinity);

        var selected = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(selected).IsEqualTo(validAffinity);
    }

    /// <summary>Verifies that an empty registry returns null.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task EmptyRegistry_ShouldReturnNull()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();

        // Act
        var result = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that fallback converter registry works correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FallbackRegistry_ShouldSelectHighestAffinity()
    {
        // Arrange
        var registry = new BindingFallbackConverterRegistry();
        var lowAffinity = new TestFallbackConverter(ExpectedTwoConverters);
        var highAffinity = new TestFallbackConverter(HighAffinity);

        // Act
        registry.Register(lowAffinity);
        registry.Register(highAffinity);

        var selected = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(selected).IsEqualTo(highAffinity);
    }

    /// <summary>Verifies that GetAllConverters returns all registered converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAllConverters_ShouldReturnAllRegistered()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var converter1 = new TestConverter<int, string>(DefaultAffinity);
        var converter2 = new TestConverter<double, bool>(ExpectedThreeConverters);
        var converter3 = new TestConverter<string, int>(ExpectedTwoConverters);

        // Act
        registry.Register(converter1);
        registry.Register(converter2);
        registry.Register(converter3);

        var allConverters = registry.GetAllConverters().ToList();

        // Assert
        await Assert.That(allConverters.Count).IsEqualTo(ExpectedThreeConverters);
        await Assert.That(allConverters).Contains(converter1);
        await Assert.That(allConverters).Contains(converter2);
        await Assert.That(allConverters).Contains(converter3);
    }

    /// <summary>Verifies that the highest-affinity converter is selected among several for the same type pair.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task MultipleConverters_ShouldSelectHighestAffinity()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var lowAffinity = new TestConverter<int, string>(ExpectedTwoConverters);
        var mediumAffinity = new TestConverter<int, string>(DefaultAffinity);
        var highAffinity = new TestConverter<int, string>(HighAffinity);

        // Act - register in random order
        registry.Register(mediumAffinity);
        registry.Register(lowAffinity);
        registry.Register(highAffinity);

        var selected = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(selected).IsEqualTo(highAffinity);
    }

    /// <summary>Verifies that requesting a non-existent type pair returns null.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NonExistentTypePair_ShouldReturnNull()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var converter = new TestConverter<int, string>(DefaultAffinity);
        registry.Register(converter);

        // Act
        var result = registry.TryGetConverter(typeof(double), typeof(bool));

        // Assert
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that a registered converter can be retrieved.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Register_AndRetrieve_ShouldReturnConverter()
    {
        // Arrange
        var registry = new BindingTypeConverterRegistry();
        var converter = new TestConverter<int, string>(DefaultAffinity);

        // Act
        registry.Register(converter);
        var retrieved = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved).IsEqualTo(converter);
    }

    /// <summary>Verifies that set-method converter registry works correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SetMethodRegistry_ShouldSelectHighestAffinity()
    {
        // Arrange
        var registry = new SetMethodBindingConverterRegistry();
        var lowAffinity = new TestSetMethodConverter(ExpectedTwoConverters);
        var highAffinity = new TestSetMethodConverter(AboveDefaultAffinity);

        // Act
        registry.Register(lowAffinity);
        registry.Register(highAffinity);

        var selected = registry.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(selected).IsEqualTo(highAffinity);
    }

    /// <summary>Verifies that SetMethodBindingConverterRegistry.TryGetConverter returns null when no converters are registered.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SetMethodRegistry_Empty_TryGetConverter_ReturnsNull()
    {
        var registry = new SetMethodBindingConverterRegistry();

        var result = registry.TryGetConverter(typeof(int), typeof(string));

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that SetMethodBindingConverterRegistry.GetAllConverters returns empty when no converters are registered.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SetMethodRegistry_Empty_GetAllConverters_ReturnsEmpty()
    {
        var registry = new SetMethodBindingConverterRegistry();

        var result = registry.GetAllConverters();

        await Assert.That(result.Count()).IsEqualTo(0);
    }

    /// <summary>Verifies that SetMethodBindingConverterRegistry.Register throws ArgumentNullException for null converter.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SetMethodRegistry_Register_Null_ThrowsArgumentNullException()
    {
        var registry = new SetMethodBindingConverterRegistry();

        var action = () => registry.Register(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that SetMethodBindingConverterRegistry.GetAllConverters returns all registered converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SetMethodRegistry_GetAllConverters_ReturnsAllRegistered()
    {
        var registry = new SetMethodBindingConverterRegistry();
        var converter1 = new TestSetMethodConverter(DefaultAffinity);
        var converter2 = new TestSetMethodConverter(ExpectedThreeConverters);

        registry.Register(converter1);
        registry.Register(converter2);

        var all = registry.GetAllConverters().ToList();
        await Assert.That(all.Count).IsEqualTo(ExpectedTwoConverters);
        await Assert.That(all).Contains(converter1);
        await Assert.That(all).Contains(converter2);
    }

    /// <summary>Verifies that GetAllConverters returns empty on a BindingTypeConverterRegistry nothing was registered with.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TypeConverterRegistry_Fresh_GetAllConverters_ReturnsEmpty()
    {
        var registry = new BindingTypeConverterRegistry();

        var result = registry.GetAllConverters();

        await Assert.That(result.Count()).IsEqualTo(0);
    }

    /// <summary>Verifies that GetAllConverters returns empty on a BindingFallbackConverterRegistry nothing was registered with.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FallbackRegistry_Fresh_GetAllConverters_ReturnsEmpty()
    {
        var registry = new BindingFallbackConverterRegistry();

        var result = registry.GetAllConverters();

        await Assert.That(result.Count()).IsEqualTo(0);
    }

    /// <summary>Test typed converter for registry tests with configurable affinity.</summary>
    /// <typeparam name="TFrom">The source type for conversion.</typeparam>
    /// <typeparam name="TTo">The target type for conversion.</typeparam>
    /// <param name="affinity">The affinity this converter reports.</param>
    private sealed class TestConverter<TFrom, TTo>(int affinity) : BindingTypeConverter<TFrom, TTo>
    {
        /// <inheritdoc/>
        public override int GetAffinityForObjects() => affinity;

        /// <inheritdoc/>
        public override bool TryConvert(TFrom? from, object? conversionHint, [NotNullWhen(true)] out TTo? result)
        {
            result = default;
            return false;
        }
    }

    /// <summary>Test fallback converter for registry tests with configurable affinity.</summary>
    /// <param name="baseAffinity">The affinity this converter reports.</param>
    private sealed class TestFallbackConverter(int baseAffinity) : IBindingFallbackConverter
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type fromType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type toType) => baseAffinity;

        /// <inheritdoc/>
        public bool TryConvert(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type fromType,
            object from,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type toType,
            object? conversionHint,
            [NotNullWhen(true)] out object? result)
        {
            result = null;
            return false;
        }
    }

    /// <summary>Test set-method converter for registry tests with configurable affinity.</summary>
    /// <param name="baseAffinity">The affinity this converter reports.</param>
    private sealed class TestSetMethodConverter(int baseAffinity) : ISetMethodBindingConverter
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects(Type? fromType, Type? toType) => baseAffinity;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments) => newValue;
    }
}
