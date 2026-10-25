// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>Tests for the <see cref="RxBindingBuilder"/> static factory class.</summary>
public class RxBindingBuilderTests
{
    /// <summary>Verifies that CreateReactiveUIBindingBuilder returns a non-null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateReactiveUIBindingBuilder_ReturnsBuilder()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        await Assert.That(builder).IsNotNull();
    }

    /// <summary>Verifies that EnsureInitialized throws when not initialized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EnsureInitialized_NotInitialized_ThrowsInvalidOperationException()
    {
        RxBindingBuilder.ResetForTesting();

        await Assert.That(RxBindingBuilder.EnsureInitialized)
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that EnsureInitialized does not throw after BuildApp has been called.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EnsureInitialized_AfterBuildApp_DoesNotThrow()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();

        await Assert.That(RxBindingBuilder.EnsureInitialized).ThrowsNothing();
    }

    /// <summary>Verifies that ResetForTesting resets the initialization state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResetForTesting_ResetsInitializationState()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();

        // Should not throw
        RxBindingBuilder.EnsureInitialized();

        // Reset
        RxBindingBuilder.ResetForTesting();

        // Should throw again
        await Assert.That(RxBindingBuilder.EnsureInitialized)
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that calling WithCoreServices twice does not throw and is idempotent.
    /// Covers the _coreRegistered=true path in ReactiveUIBindingBuilder.WithCoreServices.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_CalledTwice_IsIdempotent()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.WithCoreServices(); // Second call should be a no-op
        _ = builder.BuildApp();

        await Assert.That(RxBindingBuilder.EnsureInitialized).ThrowsNothing();
    }

    /// <summary>
    /// Verifies that the extension method overload CreateReactiveUIBindingBuilder(resolver) returns a non-null builder.
    /// Covers lines 41-47 in RxBindingBuilder.cs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateReactiveUIBindingBuilder_WithResolver_ReturnsBuilder()
    {
        RxBindingBuilder.ResetForTesting();

        var resolver = new ModernDependencyResolver();
        var builder = resolver.CreateReactiveUIBindingBuilder();

        await Assert.That(builder).IsNotNull();
    }

    /// <summary>Verifies that the extension method overload throws ArgumentNullException when resolver is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateReactiveUIBindingBuilder_NullResolver_ThrowsArgumentNullException()
    {
        var action = static () => ((IMutableDependencyResolver)null!).CreateReactiveUIBindingBuilder();

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that CreateReactiveUIBindingBuilder falls back to AppLocator.Current
    /// when the resolver implements IMutableDependencyResolver but NOT IReadonlyDependencyResolver.
    /// Covers the fallback branch on RxBindingBuilder.cs line 51 where the "as IReadonlyDependencyResolver" cast fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateReactiveUIBindingBuilder_ResolverNotReadonly_FallsBackToAppLocator()
    {
        RxBindingBuilder.ResetForTesting();

        var resolver = new MutableOnlyResolver();
        var builder = resolver.CreateReactiveUIBindingBuilder();

        await Assert.That(builder).IsNotNull();
    }

    /// <summary>
    /// A stub resolver that implements <see cref="IMutableDependencyResolver"/> but NOT
    /// <see cref="IReadonlyDependencyResolver"/>. Used to test the fallback branch where
    /// the "as IReadonlyDependencyResolver" cast returns null.
    /// </summary>
    private sealed class MutableOnlyResolver : IMutableDependencyResolver
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasRegistration(Type? serviceType) => false;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasRegistration(Type? serviceType, string? contract) => false;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasRegistration<T>() => false;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasRegistration<T>(string? contract) => false;

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory, string? contract)
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public void UnregisterCurrent<T>()
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public void UnregisterCurrent<T>(string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public void UnregisterAll<T>()
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public void UnregisterAll<T>(string? contract)
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) =>
            Disposable.Empty;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable
            ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) =>
            Disposable.Empty;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) =>
            Disposable.Empty;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) =>
            Disposable.Empty;

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value, string? contract)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory, string? contract)
            where T : class
        {
        }
    }
}
