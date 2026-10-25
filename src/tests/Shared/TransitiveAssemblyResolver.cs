// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;

namespace ReactiveUI.Binding.Tests.Shared;

/// <summary>
/// Resolves transitive assembly references whose requested version does not match the
/// version deployed to the test output directory, by falling back to a simple-name lookup.
/// <para>
/// This keeps the test suite resilient to NuGet package updates that change an assembly's
/// version independently of the libraries compiled against it. For example, Splat 19.4.0
/// ships <c>Splat.Logging.dll</c> with assembly version <c>19.0.0.0</c>, while the pinned
/// ReactiveUI 23.2.27 was compiled against <c>Splat.Logging, Version=19.3.0.0</c>. The
/// default loader cannot roll a <c>19.3.0.0</c> request down to the <c>19.0.0.0</c> assembly,
/// so it is resolved here by name instead of forcing the package version to stay aligned.
/// </para>
/// </summary>
internal static class TransitiveAssemblyResolver
{
    /// <summary>Guards against registering the resolver more than once.</summary>
    private static int _registered;

    /// <summary>
    /// Registers the resolver on the default load context. Runs automatically when the test
    /// assembly is loaded, before any test executes.
    /// </summary>
    [ModuleInitializer]
    internal static void Register()
    {
        if (Interlocked.Exchange(ref _registered, 1) != 0)
        {
            return;
        }

        AssemblyLoadContext.Default.Resolving += ResolveByName;
    }

    /// <summary>Resolves an assembly by its simple name when the default, version-strict load fails.</summary>
    /// <param name="context">The load context raising the resolve request.</param>
    /// <param name="name">The requested assembly name, including the version that could not be found.</param>
    /// <returns>A matching assembly resolved by simple name, or <see langword="null"/> if none is available.</returns>
    private static Assembly? ResolveByName(AssemblyLoadContext context, AssemblyName name)
    {
        if (string.IsNullOrEmpty(name.Name))
        {
            return null;
        }

        // Prefer an assembly with the same simple name that is already loaded, so every
        // consumer shares a single instance regardless of the version each requested.
        foreach (var loaded in context.Assemblies)
        {
            if (string.Equals(loaded.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase))
            {
                return loaded;
            }
        }

        // Otherwise load the deployed copy from the test output directory, ignoring the
        // requested version (which may be higher than what the package actually shipped).
        var candidate = Path.Combine(AppContext.BaseDirectory, $"{name.Name}.dll");
        return File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
    }
}
