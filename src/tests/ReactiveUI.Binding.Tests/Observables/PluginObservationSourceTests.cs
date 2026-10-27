// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.Fallback;
using Splat;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Tests for <see cref="PluginObservationSource"/>, which decides whether one link of a generated chain is
/// observed by the mechanism it was generated from or by a registration that outranks it.
/// </summary>
/// <remarks>
/// The registered set is process-wide state, so these run on their own rather than alongside a test that
/// reads it.
/// </remarks>
[NotInParallel]
public class PluginObservationSourceTests
{
    /// <summary>The property these tests observe.</summary>
    private const string ObservedPropertyName = "Name";

    /// <summary>The value the fixture holds.</summary>
    private const string InitialName = "initial";

    /// <summary>An affinity no registration can beat.</summary>
    private const int UnbeatableAffinity = int.MaxValue;

    /// <summary>An affinity the registered plugin beats.</summary>
    private const int BeatableAffinity = 1;

    /// <summary>
    /// With nothing registered above the generated mechanism, the link keeps the observation it was built
    /// with. Wrapping it anyway would cost an allocation on every link of every chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Choose_WithNoRegistrationAboveTheGeneratedAffinity_KeepsTheGeneratedObservation()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var generated = new UnchangingPropertyObservable<string>(InitialName);

        var chosen = PluginObservationSource.Choose(
            fixture,
            NameExpression(),
            ObservedPropertyName,
            false,
            UnbeatableAffinity,
            static observed => ((ObservedFixture)observed).Name,
            generated);

        await Assert.That(chosen).IsSameReferenceAs(generated);
    }

    /// <summary>A registration that outranks the generated affinity drives the link instead.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Choose_WithARegistrationAboveTheGeneratedAffinity_ObservesThroughTheRegistration()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();

        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new WinningPlugin());
            ObservationAffinityChecker.Refresh();

            var fixture = new ObservedFixture { Name = InitialName };

            var chosen = PluginObservationSource.Choose(
                fixture,
                NameExpression(),
                ObservedPropertyName,
                false,
                BeatableAffinity,
                static observed => ((ObservedFixture)observed).Name,
                new UnchangingPropertyObservable<string>(InitialName));

            await Assert.That(chosen).IsTypeOf<PluginPropertyObservable<string>>();
        }
        finally
        {
            RuntimeObservationFallbackTests.EnsureInitialized();
            ObservationAffinityChecker.Refresh();
        }
    }

    /// <summary>Names the observed property the way generated code does, as a compiler-built expression.</summary>
    /// <returns>The body of a lambda naming the property.</returns>
    private static System.Linq.Expressions.Expression NameExpression()
    {
        Expression<Func<ObservedFixture, string>> property = fixture => fixture.Name;
        return property.Body;
    }

    /// <summary>An object with a property to observe. The registration reports it, so it notifies nothing.</summary>
    private sealed class ObservedFixture
    {
        /// <summary>Gets or sets the observed property.</summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>A registration that outranks the generated mechanism and reports nothing of its own.</summary>
    private sealed class WinningPlugin : ICreatesObservableForProperty, IObservable<IObservedChange<object, object?>>
    {
        /// <summary>The affinity this registration bids, above anything a generated link carries here.</summary>
        private const int WinningAffinity = 50;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) => WinningAffinity;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings) => this;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer) =>
            new NoSubscription();

        /// <summary>A subscription to a registration that never reports.</summary>
        private sealed class NoSubscription : IDisposable
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }
        }
    }
}
