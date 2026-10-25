// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>What every API's per-call-site model tells the pipeline about how the call is claimed.</summary>
internal interface IClaimableCallSite
{
    /// <summary>Gets where the call site is, for a build that claims call sites with interceptors.</summary>
    InterceptorLocation Interceptor { get; }

    /// <summary>Gets the class the call's generated code is declared in, when only that class can name its types.</summary>
    HostedCall? Host { get; }
}
