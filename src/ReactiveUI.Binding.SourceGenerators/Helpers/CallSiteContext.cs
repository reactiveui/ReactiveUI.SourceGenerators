// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>A call site and the semantic model it is read with.</summary>
/// <param name="Node">The call site.</param>
/// <param name="SemanticModel">The semantic model that binds it.</param>
/// <remarks>
/// The model comes from the consumer's compilation, or from the copy that also declares the members ReactiveUI.SourceGenerators
/// adds, so a call site that binds one of those members can be read. It is never part of a pipeline's output.
/// </remarks>
internal readonly record struct CallSiteContext(SyntaxNode Node, SemanticModel SemanticModel)
{
    /// <summary>The semantic model of each file of each compilation copy, shared by every call site in the file.</summary>
    /// <remarks>
    /// A semantic model caches the members it binds. Call sites in one method share that cache.
    /// The table holds the models only while the compilation copy is alive.
    /// </remarks>
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, SemanticModel>> Models = new();

    /// <summary>Reads a call site from the compilation that declares ReactiveUI.SourceGenerators' members, when there is one.</summary>
    /// <param name="context">The syntax context the scan found the call site in.</param>
    /// <param name="sourceGenerators">The compilation with those members declared, or null when the consumer uses none.</param>
    /// <returns>The call site with the model to read it with.</returns>
    internal static CallSiteContext From(GeneratorSyntaxContext context, Compilation? sourceGenerators) =>
        new(context.Node, sourceGenerators is null ? context.SemanticModel : ModelFor(sourceGenerators, context.Node.SyntaxTree));

    /// <summary>Gets the one semantic model a compilation's file is read with.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="tree">The file.</param>
    /// <returns>The file's model, created on first use.</returns>
    /// <remarks>
    /// Transforms run in parallel. A semantic model supports queries from several threads.
    /// Two first readers can each create a model. Both receive the model the dictionary keeps.
    /// </remarks>
    internal static SemanticModel ModelFor(Compilation compilation, SyntaxTree tree)
    {
        var models = Models.GetValue(compilation, static _ => new());
        return models.TryGetValue(tree, out var model)
            ? model
            : models.GetOrAdd(tree, compilation.GetSemanticModel(tree));
    }
}
