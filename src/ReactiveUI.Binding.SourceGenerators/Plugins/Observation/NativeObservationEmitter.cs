// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits a typed observation around a platform's native attachment statements.</summary>
internal static class NativeObservationEmitter
{
    /// <summary>Combines a concrete getter with a selected native subscription.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="observation">The typed source and property.</param>
    /// <param name="kind">The selected mechanism's identity.</param>
    /// <param name="emitSubscription">The mechanism's static subscription emitter.</param>
    internal static void Emit(
        SourceWriter sb,
        in ObservationExpression observation,
        string kind,
        Action<SourceWriter, PropertyPathSegment, PlatformObservationInfo> emitSubscription)
    {
        var segment = observation.Segment;
        var info = PlatformSymbols.Candidate(segment.DeclaringTypeInfo, segment.PropertyName, kind);
        if (observation.BeforeChange || info is null)
        {
            _ = UnchangingObservationEmitter.AppendExpression(sb, observation.Source, segment, observation.SourceType);
            return;
        }

        _ = sb.Append(GeneratedTypeNames.OpenCallbackProperty).Append(observation.SourceType).Append(", ")
            .Append(segment.PropertyTypeFullName).Append(">((").Append(observation.SourceType).Append(')').Append(observation.Source)
            .Line(", (__source, __notify) =>")
            .Indent()
            .OpenBlock();
        emitSubscription(sb, segment, info);
        _ = sb.CloseBlockInline().Append(", __source => __source.").Append(segment.PropertyName)
            .Append(", ").AppendLiteral(observation.Distinct).Append(')')
            .Outdent();
    }
}
