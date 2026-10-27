// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI.Binding.Fallback;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Tests for the <see cref="CommandBindingAffinityChecker"/> class.</summary>
public class CommandBindingAffinityCheckerTests
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

    /// <summary>Verifies that when no plugins are registered, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_NoPluginsRegistered_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that when a registered plugin has lower affinity than the generated affinity, the method returns false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_PluginWithLowerAffinity_ReturnsFalse()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(LowerPluginAffinity));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

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
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(GeneratedAffinity));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

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
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(HigherPluginAffinity));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

            await Assert.That(result).IsTrue();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Verifies that the hasEventTarget parameter is correctly passed through to the plugin.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_HasEventTargetTrue_PassesThroughToPlugin()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            var plugin = new StubCommandBinding(HigherPluginAffinity, 0);
            AppLocator.Register<ICreatesCommandBinding>(() => plugin);

            var resultWithEvent = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, true);
            var resultWithoutEvent = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

            await Assert.That(resultWithEvent).IsTrue();
            await Assert.That(resultWithoutEvent).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>
    /// Verifies that when multiple plugins are registered and only one has higher affinity,
    /// the method returns true (short-circuits on first match).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasHigherAffinityPlugin_MultiplePlugins_OnlyOneHigher_ReturnsTrue()
    {
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(LowerPluginAffinity));
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(HigherPluginAffinity));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

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
        AppLocator.UnregisterAll<ICreatesCommandBinding>();
        try
        {
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(MinorPluginAffinity));
            AppLocator.Register<ICreatesCommandBinding>(static () => new StubCommandBinding(AlternatePluginAffinity));

            var result = CommandBindingAffinityChecker.HasHigherAffinityPlugin<StubControl>(GeneratedAffinity, false);

            await Assert.That(result).IsFalse();
        }
        finally
        {
            RestoreDefaultPlugins();
        }
    }

    /// <summary>Restores default plugins by re-initializing the binding infrastructure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RestoreDefaultPlugins() =>
        RuntimeObservationFallbackTests.EnsureInitialized();

    /// <summary>A stub control type used as a generic type argument in tests.</summary>
    [SuppressMessage("Design", "SST1436:Empty type", Justification = "Only used as a generic type argument; members would not be exercised.")]
    private sealed class StubControl;

    /// <summary>A stub implementation of <see cref="ICreatesCommandBinding"/> for testing.</summary>
    private sealed class StubCommandBinding : ICreatesCommandBinding
    {
        /// <summary>Thrown by stub members the affinity tests never call.</summary>
        private const string NotNeededForAffinityTests = "Not needed for affinity tests.";

        /// <summary>The affinity to return when hasEventTarget is true.</summary>
        private readonly int _hasEventAffinity;

        /// <summary>The affinity to return when hasEventTarget is false.</summary>
        private readonly int _noEventAffinity;

        /// <summary>Initializes a new instance of the <see cref="StubCommandBinding"/> class with the same affinity for both event and non-event targets.</summary>
        /// <param name="affinity">The affinity to return for all calls.</param>
        public StubCommandBinding(int affinity)
        {
            _hasEventAffinity = affinity;
            _noEventAffinity = affinity;
        }

        /// <summary>Initializes a new instance of the <see cref="StubCommandBinding"/> class with different affinities for event and non-event targets.</summary>
        /// <param name="hasEventAffinity">The affinity to return when hasEventTarget is true.</param>
        /// <param name="noEventAffinity">The affinity to return when hasEventTarget is false.</param>
        public StubCommandBinding(int hasEventAffinity, int noEventAffinity)
        {
            _hasEventAffinity = hasEventAffinity;
            _noEventAffinity = noEventAffinity;
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public int GetAffinityForObject<T>(bool hasEventTarget) =>
            hasEventTarget ? _hasEventAffinity : _noEventAffinity;

        /// <inheritdoc/>
        public IDisposable? BindCommandToObject<T>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter)
            where T : class =>
            throw new NotSupportedException(NotNeededForAffinityTests);

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            string eventName)
            where T : class =>
            throw new NotSupportedException(NotNeededForAffinityTests);

        /// <inheritdoc/>
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
            where T : class
            where TEventArgs : EventArgs =>
            throw new NotSupportedException(NotNeededForAffinityTests);
    }
}
