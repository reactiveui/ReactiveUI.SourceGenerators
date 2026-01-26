// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace ReactiveUI.SourceGenerator.Tests;

internal static class TestCompilationReferences
{
    internal static ImmutableArray<MetadataReference> CreateDefault()
    {
        // Use assemblies already referenced by the test project to create a compilation that
        // can resolve ReactiveUI types used in the in-memory source strings.
        var root = typeof(ReactiveUI.SourceGenerators.CodeFixers.PropertyToReactiveFieldAnalyzer).Assembly;

        var assemblies = new HashSet<Assembly>
        {
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            root,
        };

        // Load the dependency closure for the analyzer assembly to ensure ReactiveUI is present.
        TryAdd(root.GetName(), assemblies);

        // Also add currently loaded assemblies (helps when running under different test hosts).
        foreach (var assemblyName in AppDomain.CurrentDomain.GetAssemblies()
                     .Select(static a => a.GetName())
                     .DistinctBy(static a => a.FullName))
        {
            TryAdd(assemblyName, assemblies);
        }

        return assemblies
            .Where(static a => !string.IsNullOrWhiteSpace(a.Location))
            .Select(static a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
            .ToImmutableArray();
    }

    private static void TryAdd(AssemblyName assemblyName, HashSet<Assembly> set)
    {
        try
        {
            var loaded = Assembly.Load(assemblyName);

            if (!string.IsNullOrWhiteSpace(loaded.Location))
            {
                set.Add(loaded);
            }

            foreach (var referenced in loaded.GetReferencedAssemblies())
            {
                try
                {
                    var referencedLoaded = Assembly.Load(referenced);

                    if (!string.IsNullOrWhiteSpace(referencedLoaded.Location))
                    {
                        set.Add(referencedLoaded);
                    }
                }
                catch
                {
                    // Best-effort only.
                }
            }
        }
        catch
        {
            // Best-effort only.
        }
    }
}
