// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using TUnit.Core.Interfaces;

namespace ReactiveUI.Binding.Tests.TestExecutors;

/// <summary>
/// Base test executor that provides ReactiveUI.Binding builder lifecycle management
/// with customizable registration hooks. Derived executors can override
/// <see cref="ConfigureAppBuilder"/> to add custom builder registrations.
/// </summary>
/// <remarks>
/// This base class uses <see cref="BindingBuilderTestHelper"/> to handle:
/// <list type="bullet">
/// <item><description>Builder state reset before and after test execution.</description></item>
/// <item><description>Core services registration.</description></item>
/// <item><description>Generated view dispatch cleanup after test completion.</description></item>
/// <item><description>Customizable builder configuration via virtual method.</description></item>
/// </list>
/// </remarks>
public class BaseBindingBuilderTestExecutor : ITestExecutor
{
    /// <inheritdoc />
    public virtual async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        BindingBuilderTestHelper.Initialize(builder => ConfigureAppBuilder(builder, context));

        try
        {
            await action();
        }
        finally
        {
            BindingBuilderTestHelper.CleanUp();
        }
    }

    /// <summary>
    /// Configures the builder with custom registrations and services.
    /// Derived classes should override this method to add their specific configuration.
    /// </summary>
    /// <param name="builder">The ReactiveUI.Binding builder to configure.</param>
    /// <param name="context">The test context for storing state.</param>
    /// <remarks>
    /// The base implementation registers core services only. Derived classes should call the base method
    /// or ensure they register core services if they override without calling base.
    /// </remarks>
    protected virtual void ConfigureAppBuilder(IReactiveUIBindingBuilder builder, TestContext context)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(context);

        _ = builder.WithCoreServices();
    }
}
