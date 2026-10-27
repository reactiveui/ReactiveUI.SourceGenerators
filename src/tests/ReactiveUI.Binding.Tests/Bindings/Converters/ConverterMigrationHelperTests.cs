// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>
///     Tests for the ConverterMigrationHelper which assists in migrating from Splat-based
///     converter registration to the new ConverterService-based system.
/// </summary>
public class ConverterMigrationHelperTests
{
    /// <summary>Affinity ranking below <see cref="DefaultAffinity"/>.</summary>
    private const int BelowDefaultAffinity = 3;

    /// <summary>The affinity a plain test converter reports unless a test needs a ranking.</summary>
    private const int DefaultAffinity = 5;

    /// <summary>The expected number of converters extracted when two are registered.</summary>
    private const int ExpectedTwoConverters = 2;

    /// <summary>Verifies that ExtractConverters throws when resolver is null.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldThrowArgumentNullException_WhenResolverIsNull() =>
        await Assert.That(static () => ConverterMigrationHelper.ExtractConverters(null!))
            .Throws<ArgumentException>();

    /// <summary>Verifies that ExtractConverters returns empty lists when no converters registered.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldReturnEmptyLists_WhenNoConvertersRegistered()
    {
        // Arrange
        var resolver = new TestDependencyResolver();

        // Act
        var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

        // Assert
        await Assert.That(typed).IsEmpty();
        await Assert.That(fallback).IsEmpty();
        await Assert.That(setMethod).IsEmpty();
    }

    /// <summary>Verifies that ExtractConverters extracts typed converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldExtractTypedConverters()
    {
        // Arrange
        var converter1 = new TestTypedConverter<int, string>(DefaultAffinity);
        var converter2 = new TestTypedConverter<double, bool>(BelowDefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingTypeConverter>(converter1);
        resolver.RegisterService<IBindingTypeConverter>(converter2);

        // Act
        var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

        // Assert
        await Assert.That(typed).Count().IsEqualTo(ExpectedTwoConverters);
        await Assert.That(typed).Contains(converter1);
        await Assert.That(typed).Contains(converter2);
        await Assert.That(fallback).IsEmpty();
        await Assert.That(setMethod).IsEmpty();
    }

    /// <summary>Verifies that ExtractConverters extracts fallback converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldExtractFallbackConverters()
    {
        // Arrange
        var converter1 = new TestFallbackConverter(DefaultAffinity);
        var converter2 = new TestFallbackConverter(BelowDefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingFallbackConverter>(converter1);
        resolver.RegisterService<IBindingFallbackConverter>(converter2);

        // Act
        var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

        // Assert
        await Assert.That(typed).IsEmpty();
        await Assert.That(fallback).Count().IsEqualTo(ExpectedTwoConverters);
        await Assert.That(fallback).Contains(converter1);
        await Assert.That(fallback).Contains(converter2);
        await Assert.That(setMethod).IsEmpty();
    }

    /// <summary>Verifies that ExtractConverters extracts set-method converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldExtractSetMethodConverters()
    {
        // Arrange
        var converter1 = new TestSetMethodConverter(DefaultAffinity);
        var converter2 = new TestSetMethodConverter(BelowDefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<ISetMethodBindingConverter>(converter1);
        resolver.RegisterService<ISetMethodBindingConverter>(converter2);

        // Act
        var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

        // Assert
        await Assert.That(typed).IsEmpty();
        await Assert.That(fallback).IsEmpty();
        await Assert.That(setMethod).Count().IsEqualTo(ExpectedTwoConverters);
        await Assert.That(setMethod).Contains(converter1);
        await Assert.That(setMethod).Contains(converter2);
    }

    /// <summary>Verifies that ExtractConverters extracts all converter types.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldExtractAllConverterTypes()
    {
        // Arrange
        var typedConverter = new TestTypedConverter<int, string>(DefaultAffinity);
        var fallbackConverter = new TestFallbackConverter(BelowDefaultAffinity);
        var setMethodConverter = new TestSetMethodConverter(ExpectedTwoConverters);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingTypeConverter>(typedConverter);
        resolver.RegisterService<IBindingFallbackConverter>(fallbackConverter);
        resolver.RegisterService<ISetMethodBindingConverter>(setMethodConverter);

        // Act
        var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

        // Assert
        await Assert.That(typed).Count().IsEqualTo(1);
        await Assert.That(typed).Contains(typedConverter);
        await Assert.That(fallback).Count().IsEqualTo(1);
        await Assert.That(fallback).Contains(fallbackConverter);
        await Assert.That(setMethod).Count().IsEqualTo(1);
        await Assert.That(setMethod).Contains(setMethodConverter);
    }

    /// <summary>Verifies that ExtractConverters filters out null converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ExtractConverters_ShouldFilterOutNullConverters()
    {
        // Arrange
        var converter = new TestTypedConverter<int, string>(DefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingTypeConverter>(converter);
        resolver.RegisterService<IBindingTypeConverter>(null!);

        // Act
        var (typed, _, _) = ConverterMigrationHelper.ExtractConverters(resolver);

        // Assert - Should only contain the non-null converter
        await Assert.That(typed).Count().IsEqualTo(1);
        await Assert.That(typed).Contains(converter);
    }

    /// <summary>Verifies that ImportFrom throws when converter service is null.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldThrowArgumentNullException_WhenConverterServiceIsNull()
    {
        // Arrange
        var resolver = new TestDependencyResolver();

        // Act & Assert
        await Assert.That(() => ((ConverterService)null!).ImportFrom(resolver))
            .Throws<ArgumentException>();
    }

    /// <summary>Verifies that ImportFrom throws when resolver is null.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldThrowArgumentNullException_WhenResolverIsNull()
    {
        // Arrange
        var service = new ConverterService();

        // Act & Assert
        await Assert.That(() => service.ImportFrom(null!))
            .Throws<ArgumentException>();
    }

    /// <summary>Verifies that ImportFrom imports typed converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldImportTypedConverters()
    {
        // Arrange
        var converter1 = new TestTypedConverter<int, string>(DefaultAffinity);
        var converter2 = new TestTypedConverter<double, bool>(BelowDefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingTypeConverter>(converter1);
        resolver.RegisterService<IBindingTypeConverter>(converter2);
        var service = new ConverterService();

        // Act
        service.ImportFrom(resolver);

        // Assert
        var result1 = service.ResolveConverter(typeof(int), typeof(string));
        var result2 = service.ResolveConverter(typeof(double), typeof(bool));
        await Assert.That(result1).IsEqualTo(converter1);
        await Assert.That(result2).IsEqualTo(converter2);
    }

    /// <summary>Verifies that ImportFrom imports fallback converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldImportFallbackConverters()
    {
        // Arrange
        var converter = new TestFallbackConverter(DefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingFallbackConverter>(converter);
        var service = new ConverterService();

        // Act
        service.ImportFrom(resolver);

        // Assert - Fallback converters should be used when no typed converter matches
        var result = service.ResolveConverter(typeof(int), typeof(string));
        await Assert.That(result).IsEqualTo(converter);
    }

    /// <summary>Verifies that ImportFrom imports set-method converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldImportSetMethodConverters()
    {
        // Arrange
        var converter = new TestSetMethodConverter(DefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<ISetMethodBindingConverter>(converter);
        var service = new ConverterService();

        // Act
        service.ImportFrom(resolver);

        // Assert
        var result = service.ResolveSetMethodConverter(typeof(int), typeof(string));
        await Assert.That(result).IsEqualTo(converter);
    }

    /// <summary>Verifies that ImportFrom imports all converter types.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldImportAllConverterTypes()
    {
        // Arrange
        var typedConverter = new TestTypedConverter<int, string>(DefaultAffinity);
        var fallbackConverter = new TestFallbackConverter(BelowDefaultAffinity);
        var setMethodConverter = new TestSetMethodConverter(ExpectedTwoConverters);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingTypeConverter>(typedConverter);
        resolver.RegisterService<IBindingFallbackConverter>(fallbackConverter);
        resolver.RegisterService<ISetMethodBindingConverter>(setMethodConverter);
        var service = new ConverterService();

        // Act
        service.ImportFrom(resolver);

        // Assert
        var typedResult = service.ResolveConverter(typeof(int), typeof(string));
        var setMethodResult = service.ResolveSetMethodConverter(typeof(int), typeof(string));

        await Assert.That(typedResult).IsEqualTo(typedConverter);
        await Assert.That(setMethodResult).IsEqualTo(setMethodConverter);
    }

    /// <summary>Verifies that ImportFrom does not import null converters.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ImportFrom_ShouldNotImportNullConverters()
    {
        // Arrange
        var converter = new TestTypedConverter<int, string>(DefaultAffinity);
        var resolver = new TestDependencyResolver();
        resolver.RegisterService<IBindingTypeConverter>(converter);
        resolver.RegisterService<IBindingTypeConverter>(null!);
        var service = new ConverterService();

        // Act
        service.ImportFrom(resolver);

        // Assert - Should only import the non-null converter
        var result = service.ResolveConverter(typeof(int), typeof(string));
        await Assert.That(result).IsEqualTo(converter);
    }

    /// <summary>Test dependency resolver for testing converter extraction.</summary>
    private sealed class TestDependencyResolver : IReadonlyDependencyResolver
    {
        /// <summary>Registered services for resolution.</summary>
        private readonly List<object> _services = [];

        /// <summary>Registers a service instance for later resolution.</summary>
        /// <typeparam name="T">The service type to register.</typeparam>
        /// <param name="service">The service instance to register.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterService<T>(T? service) => _services.Add(service!);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? GetService(Type? serviceType) => _services.FirstOrDefault();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? GetService(Type? serviceType, string? contract) => _services.FirstOrDefault();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetService<T>() => _services.OfType<T>().FirstOrDefault();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetService<T>(string? contract) => _services.OfType<T>().FirstOrDefault();

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType) => _services.Where(static s => s is not null)!;

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract) =>
            _services.Where(static s => s is not null)!;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetServices<T>() => _services.OfType<T>();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetServices<T>(string? contract) => _services.OfType<T>();
    }

    /// <summary>Test typed converter for testing purposes.</summary>
    /// <typeparam name="TFrom">The source type for conversion.</typeparam>
    /// <typeparam name="TTo">The target type for conversion.</typeparam>
    /// <param name="affinity">The affinity this converter reports.</param>
    private sealed class TestTypedConverter<TFrom, TTo>(int affinity) : BindingTypeConverter<TFrom, TTo>
    {
        /// <inheritdoc/>
        public override int GetAffinityForObjects() => affinity;

        /// <inheritdoc/>
        public override bool TryConvert(TFrom? from, object? conversionHint, [NotNullWhen(true)] out TTo result)
        {
            result = default!;
            return false;
        }
    }

    /// <summary>Test fallback converter for testing purposes.</summary>
    /// <param name="affinity">The affinity this converter reports.</param>
    private sealed class TestFallbackConverter(int affinity) : IBindingFallbackConverter
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects(Type fromType, Type toType) => affinity;

        /// <inheritdoc/>
        public bool TryConvert(
            Type fromType,
            object from,
            Type toType,
            object? conversionHint,
            [NotNullWhen(true)] out object? result)
        {
            result = null;
            return false;
        }
    }

    /// <summary>Test set method converter for testing purposes.</summary>
    /// <param name="affinity">The affinity this converter reports.</param>
    private sealed class TestSetMethodConverter(int affinity) : ISetMethodBindingConverter
    {
        /// <summary>
        /// Represents the affinity value of the <see cref="TestSetMethodConverter"/>.
        /// This value influences how strongly the converter is preferred for handling specific type conversions
        /// when multiple converters are available. Higher values indicate higher affinity.
        /// </summary>
        private readonly int _affinity = affinity;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObjects(Type? fromType, Type? toType) => _affinity;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments) => null;
    }
}
