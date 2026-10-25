// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestExecutors;

/// <summary>
/// Test executor that sets up ReactiveUI.Binding builder isolation for test duration.
/// Ensures builder state is reset before and after each test.
/// </summary>
/// <remarks>
/// This executor uses the default <see cref="BaseBindingBuilderTestExecutor"/> behavior,
/// which registers core services only. For custom registrations, derive from
/// <see cref="BaseBindingBuilderTestExecutor"/> and override
/// <see cref="BaseBindingBuilderTestExecutor.ConfigureAppBuilder"/>.
/// </remarks>
public class BindingBuilderTestExecutor : BaseBindingBuilderTestExecutor
{
    // No additional configuration needed — base class handles everything.
}
