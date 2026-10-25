// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>
///     Integration tests for the ConverterService.
///     Verifies end-to-end converter resolution with typed and fallback converters.
/// </summary>
public class ConverterServiceIntegrationTests
{
    /// <summary>The lowest affinity ranking used in these tests.</summary>
    private const int LowAffinity = 2;

    /// <summary>Affinity ranking below <see cref="DefaultAffinity"/>.</summary>
    private const int BelowDefaultAffinity = 3;

    /// <summary>The affinity a plain test converter reports unless a test needs a ranking.</summary>
    private const int DefaultAffinity = 5;

    /// <summary>Affinity ranking above <see cref="DefaultAffinity"/>.</summary>
    private const int AboveDefaultAffinity = 8;

    /// <summary>Affinity ranking that outranks every other converter registered in a test.</summary>
    private const int HighAffinity = 10;

    /// <summary>The highest affinity used, for the converter a test expects to win outright.</summary>
    private const int HighestAffinity = 100;

    /// <summary>Verifies that all three registries are accessible.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ConverterService_ShouldExposeAllRegistries()
    {
        // Arrange
        var service = new ConverterService();

        // Assert
        await Assert.That(service.TypedConverters).IsNotNull();
        await Assert.That(service.FallbackConverters).IsNotNull();
        await Assert.That(service.SetMethodConverters).IsNotNull();
    }

    /// <summary>Verifies that custom converters with high affinity can override defaults.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task CustomHighAffinityConverter_ShouldOverrideDefault()
    {
        // Arrange
        var service = new ConverterService();
        var defaultConverter = new TestTypedConverter<int, string>(LowAffinity);
        var customConverter = new TestTypedConverter<int, string>(HighestAffinity);

        service.TypedConverters.Register(defaultConverter);
        service.TypedConverters.Register(customConverter);

        // Act
        var result = service.ResolveConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(result).IsEqualTo(customConverter);
    }

    /// <summary>Verifies that fallback converters are used when no typed converter matches.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FallbackConverter_ShouldBeUsedWhenNoTypedMatch()
    {
        // Arrange
        var service = new ConverterService();
        var typedConverter = new TestTypedConverter<int, string>(DefaultAffinity);
        var fallbackConverter = new TestFallbackConverter(BelowDefaultAffinity);

        service.TypedConverters.Register(typedConverter);
        service.FallbackConverters.Register(fallbackConverter);

        // Act - Request different type pair (not int->string)
        var result = service.ResolveConverter(typeof(double), typeof(bool));

        // Assert - Fallback converter should be used
        await Assert.That(result).IsEqualTo(fallbackConverter);
    }

    /// <summary>Verifies that the highest affinity fallback converter is selected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task MultipleFallbackConverters_ShouldSelectHighestAffinity()
    {
        // Arrange
        var service = new ConverterService();
        var lowAffinity = new TestFallbackConverter(LowAffinity);
        var mediumAffinity = new TestFallbackConverter(DefaultAffinity);
        var highAffinity = new TestFallbackConverter(HighAffinity);

        service.FallbackConverters.Register(mediumAffinity);
        service.FallbackConverters.Register(lowAffinity);
        service.FallbackConverters.Register(highAffinity);

        // Act
        var result = service.ResolveConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(result).IsEqualTo(highAffinity);
    }

    /// <summary>Verifies that null is returned when no converter matches.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NoConverter_ShouldReturnNull()
    {
        // Arrange
        var service = new ConverterService();
        var converter = new TestTypedConverter<int, string>(DefaultAffinity);
        service.TypedConverters.Register(converter);

        // Act
        var result = service.ResolveConverter(typeof(double), typeof(bool));

        // Assert
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies end-to-end integration with real converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task RealConverters_ShouldResolveCorrectly()
    {
        // Arrange
        var service = new ConverterService();
        var intToString = new IntegerToStringTypeConverter();
        var stringToInt = new StringToIntegerTypeConverter();
        var equality = new EqualityTypeConverter();

        service.TypedConverters.Register(intToString);
        service.TypedConverters.Register(stringToInt);
        service.TypedConverters.Register(equality);

        // Act
        var result1 = service.ResolveConverter(typeof(int), typeof(string));
        var result2 = service.ResolveConverter(typeof(string), typeof(int));

        // Assert
        await Assert.That(result1).IsEqualTo(intToString);
        await Assert.That(result2).IsEqualTo(stringToInt);
    }

    /// <summary>Verifies that BindingConverters.Current works after being set.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BindingConverters_CurrentShouldBeAccessible()
    {
        // Arrange
        var service = new ConverterService();
        var converter = new TestTypedConverter<int, string>(DefaultAffinity);
        service.TypedConverters.Register(converter);

        // Act
        BindingConverters.SetService(service);
        var result = BindingConverters.Current.ResolveConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(result).IsEqualTo(converter);

        // Cleanup - reset to default
        BindingConverters.SetService(new());
    }

    /// <summary>Verifies that set-method converters can be registered and retrieved.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SetMethodConverter_ShouldBeRetrievable()
    {
        // Arrange
        var service = new ConverterService();
        var setMethodConverter = new TestSetMethodConverter(AboveDefaultAffinity);

        service.SetMethodConverters.Register(setMethodConverter);

        // Act
        var result = service.SetMethodConverters.TryGetConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(result).IsEqualTo(setMethodConverter);
    }

    /// <summary>Verifies that typed converters are selected before fallback converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TypedConverter_ShouldBePreferredOverFallback()
    {
        // Arrange
        var service = new ConverterService();
        var typedConverter = new TestTypedConverter<int, string>(LowAffinity);
        var fallbackConverter = new TestFallbackConverter(HighAffinity); // Higher affinity but should lose to typed

        service.TypedConverters.Register(typedConverter);
        service.FallbackConverters.Register(fallbackConverter);

        // Act
        var result = service.ResolveConverter(typeof(int), typeof(string));

        // Assert - Typed converter should win even with lower affinity
        await Assert.That(result).IsEqualTo(typedConverter);
    }

    /// <summary>Verifies that converters with affinity 0 are ignored in resolution.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ZeroAffinityConverter_ShouldBeIgnoredInResolution()
    {
        // Arrange
        var service = new ConverterService();
        var zeroAffinity = new TestTypedConverter<int, string>(0);
        var validAffinity = new TestTypedConverter<int, string>(LowAffinity);

        service.TypedConverters.Register(zeroAffinity);
        service.TypedConverters.Register(validAffinity);

        // Act
        var result = service.ResolveConverter(typeof(int), typeof(string));

        // Assert
        await Assert.That(result).IsEqualTo(validAffinity);
    }

    /// <summary>Test fallback converter for integration testing with configurable affinity.</summary>
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

    /// <summary>Test set-method converter for integration testing with configurable affinity.</summary>
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

    /// <summary>Test typed converter for integration testing with configurable affinity.</summary>
    /// <typeparam name="TFrom">The source type for conversion.</typeparam>
    /// <typeparam name="TTo">The target type for conversion.</typeparam>
    /// <param name="affinity">The affinity this converter reports.</param>
    private sealed class TestTypedConverter<TFrom, TTo>(int affinity) : BindingTypeConverter<TFrom, TTo>
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
}
