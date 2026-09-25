// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        internal void RegisterDiagnostics<T>(IncrementalValuesProvider<Result<T>> results)
            where T : IEquatable<T>? =>
            context.RegisterSourceOutput(
                results.Where(static result => !result.Errors.IsEmpty).Select(static (result, _) => result.Errors),
                static (context, errors) =>
                {
                    foreach (var error in errors.AsImmutableArray())
                    {
                        context.ReportDiagnostic(error.ToDiagnostic());
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
        internal IncrementalValuesProvider<EquatableArray<T>> GroupByTarget(Func<T, TargetInfo> target) =>
            models.Collect().SelectMany((all, _) => Group(all, target));
    }

    /// <summary>Groups models by their type's hint name, which is unique within a compilation.</summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="all">Every model.</param>
    /// <param name="target">Reads a model's type.</param>
    /// <returns>One array per type, in first-seen order.</returns>
    private static ImmutableArray<EquatableArray<T>> Group<T>(ImmutableArray<T> all, Func<T, TargetInfo> target)
        where T : IEquatable<T>
    {
        if (all.IsEmpty)
        {
            return ImmutableArray<EquatableArray<T>>.Empty;
        }

        var indexByType = new Dictionary<string, int>(StringComparer.Ordinal);
        var groups = new List<ImmutableArray<T>.Builder>();
        foreach (var model in all)
        {
            var hintName = target(model).FileHintName;
            if (!indexByType.TryGetValue(hintName, out var index))
            {
                index = groups.Count;
                indexByType.Add(hintName, index);
                groups.Add(ImmutableArray.CreateBuilder<T>());
            }

            groups[index].Add(model);
        }

        var result = ImmutableArray.CreateBuilder<EquatableArray<T>>(groups.Count);
        foreach (var group in groups)
        {
            result.Add(group.ToImmutable());
        }

        return result.MoveToImmutable();
    }
}
