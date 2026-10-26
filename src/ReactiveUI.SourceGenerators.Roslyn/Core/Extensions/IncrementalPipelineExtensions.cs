// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Pipeline shapes the generators share: per-type grouping and diagnostics as their own output.</summary>
/// <remarks>
/// A generator that collects every model and writes every file from one callback rewrites every file whenever any model
/// changes. Grouping the collected models into one value per type, and registering the source output per value, lets
/// the driver compare each type's models with the last run's and skip the types that did not change.
/// </remarks>
internal static class IncrementalPipelineExtensions
{
    /// <summary>Provides extension members for the generator initialization context.</summary>
    /// <param name="context">The generator initialization context.</param>
    extension(in IncrementalGeneratorInitializationContext context)
    {
        /// <summary>Reports each result's diagnostics as their own output, apart from the sources.</summary>
        /// <typeparam name="T">The result's value type.</typeparam>
        /// <param name="results">The extraction results.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RegisterDiagnostics<T>(IncrementalValuesProvider<Result<T>> results)
            where T : IEquatable<T>? =>
            context.RegisterSourceOutput(
                results.Where(static result => !result.Errors.IsEmpty).Select(static (result, _) => result.Errors),
                static (context, errors) =>
                {
                    for (var i = 0; i < errors.Length; i++)
                    {
                        context.ReportDiagnostic(errors[i].ToDiagnostic());
                    }
                });
    }

    /// <summary>Provides extension members for a provider of models.</summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="models">The models.</param>
    extension<T>(IncrementalValuesProvider<T> models)
        where T : IEquatable<T>
    {
        /// <summary>Groups the models by the type they belong to, one value-equatable array per type.</summary>
        /// <param name="target">Reads a model's type.</param>
        /// <returns>One array per type, in the order the types were first seen.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IncrementalValuesProvider<EquatableArray<T>> GroupByTarget(Func<T, TargetInfo> target) =>
            models.Collect().SelectMany((all, _) => Group(all, target));
    }

    /// <summary>Groups models by their type's hint name, which is unique within a compilation.</summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="all">Every model; Roslyn's <c>Collect</c> returns it as an <see cref="ImmutableArray{T}"/>.</param>
    /// <param name="target">Reads a model's type.</param>
    /// <returns>One array per type, in first-seen order.</returns>
    private static EquatableArray<T>[] Group<T>(ImmutableArray<T> all, Func<T, TargetInfo> target)
        where T : IEquatable<T>
    {
        if (all.IsEmpty)
        {
            return [];
        }

        var indexByType = new Dictionary<string, int>(StringComparer.Ordinal);
        var groups = new List<List<T>>();
        foreach (var model in all)
        {
            var hintName = target(model).FileHintName;
            if (!indexByType.TryGetValue(hintName, out var index))
            {
                index = groups.Count;
                indexByType.Add(hintName, index);
                groups.Add([]);
            }

            groups[index].Add(model);
        }

        var result = new EquatableArray<T>[groups.Count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = new(groups[i].ToArray());
        }

        return result;
    }
}
