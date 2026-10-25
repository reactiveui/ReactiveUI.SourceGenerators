// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

[assembly: NotInParallel]

namespace ReactiveUI.Binding.GeneratedCode.Tests;

/// <summary>Assembly-level setup for the generated code test project.</summary>
public static class AssemblySetup
{
    /// <summary>Initializes the RxBinding builder for generated code tests.</summary>
    [Before(Assembly)]
    public static void Initialize()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }
}
