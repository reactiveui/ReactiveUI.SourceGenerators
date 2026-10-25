// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits concrete native event delegates and their matching removal.</summary>
internal static class NativeEventSubscriptionEmitter
{
    /// <summary>Attaches the events a mechanism verified for one property, for a plugin that observes through events alone.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="info">The selected native mechanism, which carries the verified events.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendSubscription(SourceWriter sb, PropertyPathSegment segment, PlatformObservationInfo info) =>
        Append(sb, info.Events);

    /// <summary>Attaches every event required by the selected property contract.</summary>
    /// <param name="sb">The writer, inside the subscription callback.</param>
    /// <param name="events">The verified event names and delegate types.</param>
    internal static void Append(SourceWriter sb, EquatableArray<NotificationEventInfo> events)
    {
        for (var i = 0; i < events.Length; i++)
        {
            _ = sb.Append(events[i].HandlerType).Append(" __handler").Append(i)
                .Line(" = (__sender, __args) => __notify();")
                .Append("__source.").Append(events[i].Name).Append(" += __handler").Append(i).EndStatement();
        }

        _ = sb.Line($"return new {GeneratedTypeNames.ActionDisposable}(() =>")
            .OpenBlock();
        for (var i = 0; i < events.Length; i++)
        {
            _ = sb.Append("__source.").Append(events[i].Name).Append(" -= __handler").Append(i).EndStatement();
        }

        _ = sb.CloseBlock(");");
    }
}
