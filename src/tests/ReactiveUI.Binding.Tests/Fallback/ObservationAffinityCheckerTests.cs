// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Fallback;
using Splat;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Tests for the <see cref="ObservationAffinityChecker"/> class.</summary>
public class ObservationAffinityCheckerTests
{
    /// <summary>The affinity the source generator's own selection is assumed to have.</summary>
    private const int GeneratedAffinity = 10;

    /// <summary>A plugin affinity above <see cref="GeneratedAffinity"/>, so the plugin wins.</summary>
    private const int HigherPluginAffinity = 20;

    /// <summary>A plugin affinity below <see cref="GeneratedAffinity"/>, so the generated binding wins.</summary>
    private const int LowerPluginAffinity = 5;

    /// <summary>A second, still-losing plugin affinity used when two plugins are registered.</summary>
    private const int MinorPluginAffinity = 3;

    /// <summary>A second plugin affinity used to check which of two plugins is selected.</summary>
    private const int AlternatePluginAffinity = 7;

    /// <summary>The property these tests ask about.</summary>
    private const string ObservedPropertyName = "Length";

    /// <summary>A property a property-scoped plugin does not reach.</summary>
    private const string UnscoredPropertyName = "Other";

    /// <summary>Verifies that passing a null type throws <see cref="ArgumentNullException"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NullType_ThrowsArgumentNullException()
    {
        var action = static () =>
            ObservationAffinityChecker.HasHigherAffinityPlugin(null!, ObservedPropertyName, GeneratedAffinity, false);
        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that passing a null property name throws <see cref="ArgumentNullException"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NullPropertyName_ThrowsArgumentNullException()
    {
        var action = static () =>
            ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), null!, GeneratedAffinity, false);
        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// The property name reaches the plugin. Every mechanism-specific plugin scores a type and a property
    /// together and answers 0 for a property its mechanism does not reach, so a plugin asked without one
    /// could never win.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginScoresOneProperty_OnlyThatPropertyIsTaken()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(
                static () => new PropertyScopedObservableForProperty(ObservedPropertyName, HigherPluginAffinity));

            ObservationAffinityChecker.Refresh();

            var matching = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);
            var other = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), UnscoredPropertyName, GeneratedAffinity, false);

            await Assert.That(matching).IsTrue();
            await Assert.That(other).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when no plugins are registered, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NoPluginsRegistered_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            ObservationAffinityChecker.Refresh();

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when all registered plugins have lower affinity than the generated affinity, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithLowerAffinity_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(LowerPluginAffinity));

            ObservationAffinityChecker.Refresh();

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when a registered plugin has equal affinity to the generated affinity,
    /// the method returns false (only strictly higher affinity wins).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithEqualAffinity_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(GeneratedAffinity));

            ObservationAffinityChecker.Refresh();

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when a registered plugin has higher affinity than the generated affinity, the method returns true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithHigherAffinity_ReturnsTrue()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(HigherPluginAffinity));

            ObservationAffinityChecker.Refresh();

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that the beforeChanged parameter is correctly passed through to the plugin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_BeforeChangedTrue_PassesThroughToPlugin()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var plugin = new StubObservableForProperty(HigherPluginAffinity, 0);
            AppLocator.Register<ICreatesObservableForProperty>(() => plugin);

            ObservationAffinityChecker.Refresh();

            var resultBeforeChanged = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, true);
            var resultAfterChanged = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(resultBeforeChanged).IsTrue();
            await Assert.That(resultAfterChanged).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that a stronger provider wins among several registered providers.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_MultiplePlugins_OnlyOneHigher_ReturnsTrue()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(LowerPluginAffinity));
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(HigherPluginAffinity));

            ObservationAffinityChecker.Refresh();

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when multiple plugins are registered and none has higher affinity, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_MultiplePlugins_NoneHigher_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(MinorPluginAffinity));
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(AlternatePluginAffinity));

            ObservationAffinityChecker.Refresh();

            var result = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that a plugin registered after the first resolve is picked up once the cache is dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginRegisteredAfterFirstResolve_IsSeenAfterRefresh()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            ObservationAffinityChecker.Refresh();
            var beforeRegistration = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(HigherPluginAffinity));

            // The resolved set is kept, so the new registration is invisible until the cache is dropped.
            var beforeRefresh = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            ObservationAffinityChecker.Refresh();
            var afterRefresh = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(beforeRegistration).IsFalse();
            await Assert.That(beforeRefresh).IsFalse();
            await Assert.That(afterRefresh).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Generated code asks for the registration itself, so a winner is handed back rather than a flag.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_RegistrationOutranksTheGenerated_HandsBackThatRegistration()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(HigherPluginAffinity));
            ObservationAffinityChecker.Refresh();

            var found = ObservationAffinityChecker.FindHigherAffinityPlugin(
                typeof(string),
                ObservedPropertyName,
                GeneratedAffinity,
                false);

            await Assert.That(found).IsNotNull();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>A registration scoring no better than the generated mechanism leaves the observation alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_NoRegistrationOutranksTheGenerated_HandsBackNothing()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(LowerPluginAffinity));
            ObservationAffinityChecker.Refresh();

            var found = ObservationAffinityChecker.FindHigherAffinityPlugin(
                typeof(string),
                ObservedPropertyName,
                GeneratedAffinity,
                false);

            await Assert.That(found).IsNull();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>The observed type is required to score a registration against it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_NullType_ThrowsArgumentNullException()
    {
        var action = static () =>
            ObservationAffinityChecker.FindHigherAffinityPlugin(null!, ObservedPropertyName, GeneratedAffinity, false);
        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>The property name is required, because a registration scores a type and a property together.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_NullPropertyName_ThrowsArgumentNullException()
    {
        var action = static () =>
            ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), null!, GeneratedAffinity, false);
        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Repeated selections reuse the custom score while comparing each generated affinity independently.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_RepeatedSelection_ScoresOnceAcrossGeneratedAffinities()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var plugin = new StubObservableForProperty(HigherPluginAffinity);
            AppLocator.Register<ICreatesObservableForProperty>(() => plugin);
            ObservationAffinityChecker.Refresh();

            var winner = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);
            var tie = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, HigherPluginAffinity, false);
            var repeated = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(winner).IsSameReferenceAs(plugin);
            await Assert.That(tie).IsNull();
            await Assert.That(repeated).IsTrue();
            await Assert.That(plugin.AffinityCallCount).IsEqualTo(1);
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Types, properties and notification timing each receive independent cached scores.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_DifferentObservationKeys_ScoresEachOnce()
    {
        const int repetitions = 2;
        const int distinctObservationCount = 4;
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var plugin = new StubObservableForProperty(HigherPluginAffinity);
            AppLocator.Register<ICreatesObservableForProperty>(() => plugin);
            ObservationAffinityChecker.Refresh();

            for (var repeat = 0; repeat < repetitions; repeat++)
            {
                _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);
                _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(object), ObservedPropertyName, GeneratedAffinity, false);
                _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), UnscoredPropertyName, GeneratedAffinity, false);
                _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, true);
            }

            await Assert.That(plugin.AffinityCallCount).IsEqualTo(distinctObservationCount);
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Refresh invalidates scored selections even when the registered provider instances are unchanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_Refresh_ScoresRegistrationsAgain()
    {
        const int expectedAffinityCalls = 2;
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var plugin = new StubObservableForProperty(LowerPluginAffinity);
            AppLocator.Register<ICreatesObservableForProperty>(() => plugin);
            ObservationAffinityChecker.Refresh();

            _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);
            _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);
            ObservationAffinityChecker.Refresh();
            _ = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(plugin.AffinityCallCount).IsEqualTo(expectedAffinityCalls);
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>The highest custom score wins regardless of registration order.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_MultipleWinners_ReturnsHighestAffinity()
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        try
        {
            var highest = new StubObservableForProperty(HigherPluginAffinity);
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(GeneratedAffinity));
            AppLocator.Register<ICreatesObservableForProperty>(() => highest);
            AppLocator.Register<ICreatesObservableForProperty>(static () => new StubObservableForProperty(AlternatePluginAffinity));
            ObservationAffinityChecker.Refresh();

            var winner = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, LowerPluginAffinity, false);

            await Assert.That(winner).IsSameReferenceAs(highest);
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>A resolution overlapping refresh cannot publish its registrations into the refreshed cache.</summary>
    /// <param name="cancellationToken">Cancels the synchronization if the test is interrupted.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindHigherAffinityPlugin_RefreshDuringResolution_DiscardsStaleRegistrations(CancellationToken cancellationToken)
    {
        AppLocator.UnregisterAll<ICreatesObservableForProperty>();
        using var releaseResolution = new ManualResetEventSlim();
        var resolutionStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ICreatesObservableForProperty?>? pending = null;
        try
        {
            var stale = new StubObservableForProperty(HigherPluginAffinity);
            var current = new StubObservableForProperty(HigherPluginAffinity);
            var resolutionCount = 0;
            AppLocator.Register<ICreatesObservableForProperty>(() =>
            {
                if (Interlocked.Increment(ref resolutionCount) == 1)
                {
                    resolutionStarted.SetResult(true);
                    releaseResolution.Wait(cancellationToken);
                    return stale;
                }

                return current;
            });
            ObservationAffinityChecker.Refresh();
            pending = Task.Run(
                static () => ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false),
                cancellationToken);
            _ = await resolutionStarted.Task.WaitAsync(cancellationToken);

            ObservationAffinityChecker.Refresh();
            releaseResolution.Set();
            _ = await pending;
            var winner = ObservationAffinityChecker.FindHigherAffinityPlugin(typeof(string), ObservedPropertyName, GeneratedAffinity, false);

            await Assert.That(winner).IsSameReferenceAs(current);
        }
        finally
        {
            releaseResolution.Set();
            try
            {
                if (pending is not null)
                {
                    _ = await pending;
                }
            }
            finally
            {
                RestoreDefaultPlugins();
            }
        }
    }

    /// <summary>Restores default plugins by re-initializing the binding infrastructure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RestoreDefaultPlugins()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        ObservationAffinityChecker.Refresh();
    }

    /// <summary>A plugin that reaches one property, the way the WPF, WinUI, WinForms and KVO plugins do.</summary>
    /// <param name="scopedProperty">The only property this plugin scores.</param>
    /// <param name="affinity">The affinity returned for that property.</param>
    private sealed class PropertyScopedObservableForProperty(string scopedProperty, int affinity) : ICreatesObservableForProperty
    {
        /// <inheritdoc/>
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
            string.Equals(propertyName, scopedProperty, StringComparison.Ordinal) ? affinity : 0;

        /// <inheritdoc/>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings) =>
            throw new NotSupportedException("Not needed for affinity tests.");
    }

    /// <summary>A stub implementation of <see cref="ICreatesObservableForProperty"/> for testing.</summary>
    private sealed class StubObservableForProperty : ICreatesObservableForProperty
    {
        /// <summary>The affinity to return when beforeChanged is true.</summary>
        private readonly int _beforeChangedAffinity;

        /// <summary>The affinity to return when beforeChanged is false.</summary>
        private readonly int _afterChangedAffinity;

        /// <summary>Initializes a new instance of the <see cref="StubObservableForProperty"/> class with the same affinity for both before and after change.</summary>
        /// <param name="affinity">The affinity to return for all calls.</param>
        public StubObservableForProperty(int affinity)
        {
            _beforeChangedAffinity = affinity;
            _afterChangedAffinity = affinity;
        }

        /// <summary>Initializes a new instance of the <see cref="StubObservableForProperty"/> class with different affinities for before and after change.</summary>
        /// <param name="beforeChangedAffinity">The affinity to return when beforeChanged is true.</param>
        /// <param name="afterChangedAffinity">The affinity to return when beforeChanged is false.</param>
        public StubObservableForProperty(int beforeChangedAffinity, int afterChangedAffinity)
        {
            _beforeChangedAffinity = beforeChangedAffinity;
            _afterChangedAffinity = afterChangedAffinity;
        }

        /// <summary>Gets the number of times this registration has been scored.</summary>
        public int AffinityCallCount { get; private set; }

        /// <inheritdoc/>
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
        {
            AffinityCallCount++;
            return beforeChanged ? _beforeChangedAffinity : _afterChangedAffinity;
        }

        /// <inheritdoc/>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings) =>
            throw new NotSupportedException("Not needed for affinity tests.");
    }
}
