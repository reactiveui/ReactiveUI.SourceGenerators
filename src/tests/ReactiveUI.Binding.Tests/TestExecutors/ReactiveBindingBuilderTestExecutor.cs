// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using TUnit.Core.Interfaces;
using ReactiveBuilder = ReactiveUI.Binding.Reactive.Builder.RxBindingBuilder;

namespace ReactiveUI.Binding.Tests.TestExecutors;

/// <summary>Scopes the System.Reactive binding services to one test.</summary>
public sealed class ReactiveBindingBuilderTestExecutor : ITestExecutor
{
    /// <inheritdoc/>
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        ReactiveBuilder.ResetForTesting();
        var builder = ReactiveBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
        try
        {
            await action();
        }
        finally
        {
            ReactiveBuilder.ResetForTesting();
            BindingBuilderTestHelper.CleanUp();
        }
    }
}
