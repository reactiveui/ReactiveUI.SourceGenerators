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

    /// <summary>
    /// With nothing registered above the generated mechanism, the deferred forms keep the generated observation
    /// and never build the expression.
    /// </summary>
    /// <param name="distinct">Whether the form that filters equal values is called.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ChooseDeferred_WithNoRegistrationAboveTheGeneratedAffinity_BuildsNoExpression(bool distinct)
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var generated = new UnchangingPropertyObservable<string>(InitialName);
        var built = 0;

        var chosen = ChooseDeferred(distinct, fixture, UnbeatableAffinity, () => CountedNameLambda(ref built), generated);

        await Assert.That(chosen).IsSameReferenceAs(generated);
        await Assert.That(built).IsEqualTo(0);
    }

    /// <summary>A registration that outranks the generated affinity drives the link, and the expression is built once.</summary>
    /// <param name="distinct">Whether the form that filters equal values is called.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ChooseDeferred_WithARegistrationAboveTheGeneratedAffinity_ObservesThroughTheRegistration(bool distinct)
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();

        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new WinningPlugin());
            ObservationAffinityChecker.Refresh();

            var fixture = new ObservedFixture { Name = InitialName };
            var built = 0;

            var chosen = ChooseDeferred(
                distinct,
                fixture,
                BeatableAffinity,
                () => CountedNameLambda(ref built),
                new UnchangingPropertyObservable<string>(InitialName));

            await Assert.That(chosen).IsTypeOf<PluginPropertyObservable<string>>();
            await Assert.That(built).IsEqualTo(1);
        }
        finally
        {
            RuntimeObservationFallbackTests.EnsureInitialized();
            ObservationAffinityChecker.Refresh();
        }
    }

    /// <summary>The deferred forms require the source, the expression and the generated observation.</summary>
    /// <param name="distinct">Whether the form that filters equal values is called.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ChooseDeferred_NullArgument_Throws(bool distinct)
    {
        var fixture = new ObservedFixture();
        var generated = new UnchangingPropertyObservable<string>(InitialName);

        await Assert.That(() => ChooseDeferred(distinct, null!, BeatableAffinity, NameLambda, generated)).Throws<ArgumentNullException>();
        await Assert.That(() => ChooseDeferred(distinct, fixture, BeatableAffinity, null!, generated)).Throws<ArgumentNullException>();
        await Assert.That(() => ChooseDeferred(distinct, fixture, BeatableAffinity, NameLambda, null!)).Throws<ArgumentNullException>();
        await Assert.That(() => ChooseDeferred(distinct, fixture, BeatableAffinity, NameLambda, generated, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>A winning registration reads the property through the typed accessor the generated code passed.</summary>
    /// <param name="distinct">Whether the form that filters equal values is called.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ChooseDeferred_WithAWinningRegistration_ReadsThroughTheTypedAccessor(bool distinct)
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();

        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new WinningPlugin());
            ObservationAffinityChecker.Refresh();

            var fixture = new ObservedFixture { Name = InitialName };
            var observer = new TestModels.RecordingObserver<string>();

            using var subscription = ChooseDeferred(
                distinct,
                fixture,
                BeatableAffinity,
                NameLambda,
                new UnchangingPropertyObservable<string>("generated")).Subscribe(observer);

            await Assert.That(observer.Values).Contains(InitialName);
        }
        finally
        {
            RuntimeObservationFallbackTests.EnsureInitialized();
            ObservationAffinityChecker.Refresh();
        }
    }

    /// <summary>Calls one of the two forms that build the expression only when a registration wins.</summary>
    /// <param name="distinct">Whether to call the form that filters equal values.</param>
    /// <param name="fixture">The observed object.</param>
    /// <param name="generatedAffinity">The affinity the generated observation carries.</param>
    /// <param name="expression">Builds the lambda naming the property.</param>
    /// <param name="generated">The generated observation.</param>
    /// <param name="getter">Reads the property, or the default typed accessor when omitted.</param>
    /// <returns>The chosen observation.</returns>
    private static IObservable<string> ChooseDeferred(
        bool distinct,
        ObservedFixture fixture,
        int generatedAffinity,
        Func<Expression<Func<ObservedFixture, string>>> expression,
        IObservable<string> generated,
        Func<ObservedFixture, string?> getter) =>
        distinct
            ? PluginObservationSource.ChooseDistinct(fixture, ObservedPropertyName, generatedAffinity, false, getter, expression, generated)
            : PluginObservationSource.Choose(fixture, ObservedPropertyName, generatedAffinity, false, getter, expression, generated);

    /// <summary>Calls one of the two deferred forms with the fixture's own typed accessor.</summary>
    /// <param name="distinct">Whether to call the form that filters equal values.</param>
    /// <param name="fixture">The observed object.</param>
    /// <param name="generatedAffinity">The affinity the generated observation carries.</param>
    /// <param name="expression">Builds the lambda naming the property.</param>
    /// <param name="generated">The generated observation.</param>
    /// <returns>The chosen observation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<string> ChooseDeferred(
        bool distinct,
        ObservedFixture fixture,
        int generatedAffinity,
        Func<Expression<Func<ObservedFixture, string>>> expression,
        IObservable<string> generated) =>
        ChooseDeferred(distinct, fixture, generatedAffinity, expression, generated, static observed => observed.Name);

    /// <summary>Names the observed property the way generated code does, as a lambda the compiler turns into a tree.</summary>
    /// <returns>The lambda naming the property.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<ObservedFixture, string>> NameLambda() => static fixture => fixture.Name;

    /// <summary>Builds the property's lambda, counting the call.</summary>
    /// <param name="built">How many times the lambda has been built.</param>
    /// <returns>The lambda naming the property.</returns>
    private static Expression<Func<ObservedFixture, string>> CountedNameLambda(ref int built)
    {
        built++;
        return NameLambda();
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
