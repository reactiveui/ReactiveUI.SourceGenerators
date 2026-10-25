// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Mixins;
using Splat.Builder;

namespace ReactiveUI.Binding.Tests.TestExecutors;

/// <summary>
/// Helper class that provides ReactiveUI.Binding builder lifecycle management for test executors.
/// Handles state reset, builder configuration, and cleanup between tests.
/// </summary>
public sealed class BindingBuilderTestHelper
{
    /// <summary>Initializes the builder with custom configuration. Resets builder state and configures using the provided action.</summary>
    /// <param name="configureBuilder">
    /// Action to configure the builder. Should call <c>.WithCoreServices()</c> at minimum.
    /// <c>.BuildApp()</c> is called automatically after the action.
    /// </param>
    public static void Initialize(Action<IReactiveUIBindingBuilder> configureBuilder)
    {
        ArgumentNullException.ThrowIfNull(configureBuilder);

        // Force-reset any previous builder state
        RxBindingBuilder.ResetForTesting();
        AppBuilder.ResetBuilderStateForTests();

        // Create builder and apply custom configuration
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        configureBuilder(builder);

        // Build the app with configured services
        _ = builder.BuildApp();
    }

    /// <summary>Cleans up builder state and restores a clean environment for the next test.</summary>
    public static void CleanUp()
    {
        // Reset generated view dispatch to avoid cross-test contamination
        DefaultViewLocator.ResetGeneratedViewDispatchForTesting();

        // Reset builder state
        RxBindingBuilder.ResetForTesting();

        // Rebuild with core services to ensure clean state for next test
        _ = RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}
