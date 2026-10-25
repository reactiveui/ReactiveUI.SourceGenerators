// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

/// <summary>Emits typed collection mutation and isolates winning custom set-method providers.</summary>
internal static class CollectionSetMethodEmitter
{
    /// <summary>The local a write callback holds the collection it mutates in.</summary>
    private const string CollectionLocal = "__collection";

    /// <summary>Names the local holding one walked intermediate of the path, before its index.</summary>
    private const string ParentLocalPrefix = "__setParent";

    /// <summary>Subscribes after scheduling so collection operations run on the owning thread.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="write">The concrete collection write.</param>
    /// <param name="source">The scheduled input stream.</param>
    internal static void EmitSubscription(SourceWriter sb, in SetMethodEmission write, string source)
    {
        var targetType = write.Path[write.Path.Length - 1].PropertyTypeFullName;
        AppendSelection(sb, write, targetType);
        if (write.ReportChanges)
        {
            _ = sb.BeginVar("__setChanges").Append("new global::ReactiveUI.Primitives.Signals.Signal<").Append(targetType).Line(">();");
        }

        _ = sb.BeginVar("__setSubscription").Append(GeneratedTypeNames.BindingErrors).Append(".Subscribe(").Append(source).Line(", value =>")
            .OpenBlock();
        AppendTarget(sb, write);
        _ = sb.If("__setConverter != null")
            .BeginVar("__result").Append('(').Append(targetType).Line(")__setConverter.PerformSet(__collection, value, null);");
        if (write.ReportChanges)
        {
            _ = sb.Line("__setChanges.OnNext(__result);");
        }

        _ = sb.Line("return;").CloseBlock();
        AppendMutation(sb, write.Mechanism);
        if (write.ReportChanges)
        {
            _ = sb.Line("__setChanges.OnNext(__collection);");
        }

        _ = sb.CloseBlockInline().Append(", ").AppendQuoted(write.Expression).Line(");");
    }

    /// <summary>Resolves the custom winner once, retaining native ties.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="write">The concrete write.</param>
    /// <param name="targetType">The collection type.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendSelection(SourceWriter sb, in SetMethodEmission write, string targetType) =>
        sb.BeginVar("__setConverter").Append("global::ReactiveUI.Binding.BindingConverters.Current.SetMethodConverters.TryGetConverter(typeof(")
            .Append(write.SourceType).Append("), typeof(").Append(targetType).Line("));")
            .BeginIf().Append("__setConverter != null && __setConverter.GetAffinityForObjects(typeof(")
            .Append(write.SourceType).Append("), typeof(").Append(targetType).Append(")) <= ").Append(write.Mechanism.Affinity).CloseCondition()
            .Line("__setConverter = null;")
            .CloseBlock();

    /// <summary>Reads each path link once and skips a temporarily missing collection owner.</summary>
    /// <param name="sb">The writer, inside the write callback.</param>
    /// <param name="write">The target path.</param>
    private static void AppendTarget(SourceWriter sb, in SetMethodEmission write)
    {
        var parent = write.Root;
        for (var i = 0; i < write.Path.Length; i++)
        {
            var local = i == write.Path.Length - 1 ? CollectionLocal : ParentLocalPrefix + i.ToString(CultureInfo.InvariantCulture);
            _ = sb.BeginVar(local).Append(CodeGeneratorHelpers.AppendSegmentRead(parent, write.Path[i])).EndStatement()
                .BeginIf().Append(local).Append(" == null").CloseCondition()
                .Line("return;")
                .CloseBlock();
            parent = local;
        }
    }

    /// <summary>Balances layout suspension even when enumeration or a native collection call fails.</summary>
    /// <param name="sb">The writer, inside the write callback.</param>
    /// <param name="mechanism">The selected collection mechanism.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendMutation(SourceWriter sb, SetMethodInfo mechanism) =>
        sb.BeginVar("__layoutOwner").Append("__collection.").Append(mechanism.LayoutOwner).EndStatement()
            .Line("__layoutOwner.SuspendLayout();")
            .Try()
            .Line("__collection.Clear();")
            .Append("__collection.AddRange(")
            .Append(mechanism.SourceIsArray ? "value" : $"global::System.Linq.Enumerable.ToArray<{GeneratedTypeNames.WinFormsControl}>(value)")
            .Line(");")
            .CloseBlock()
            .Finally()
            .Line("__layoutOwner.ResumeLayout();")
            .CloseBlock();
}
