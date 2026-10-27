// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Resolves and emits sender-scoped Foundation notification subscriptions.</summary>
internal static class AppleNotificationEmitter
{
    /// <summary>Finds the public notification constant and its native notification center.</summary>
    /// <param name="owner">The concrete property owner.</param>
    /// <param name="memberName">The framework notification constant.</param>
    /// <returns>The fully qualified constant, or null when the contract is unavailable.</returns>
    internal static string? ResolveNotification(INamedTypeSymbol owner, string memberName)
    {
        var member = PlatformSymbols.FindMember(owner, memberName);
        return member is { IsStatic: true, DeclaredAccessibility: Accessibility.Public }
            && member.ContainingAssembly.GetTypeByMetadataName("Foundation.NSNotificationCenter") is not null
            ? $"{member.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{memberName}"
            : null;
    }

    /// <summary>Attaches a sender-filtered notification and releases both its registration and token.</summary>
    /// <param name="sb">The writer, inside the subscription callback.</param>
    /// <param name="segment">The property being observed.</param>
    /// <param name="info">The selected notification constant.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendSubscription(SourceWriter sb, PropertyPathSegment segment, PlatformObservationInfo info) =>
        sb.BeginVar("__token").Append("global::Foundation.NSNotificationCenter.DefaultCenter.AddObserver(")
            .Append(info.NotificationName).Line(", __notification => __notify(), __source);")
            .Line($"return new {GeneratedTypeNames.ActionDisposable}(() =>")
            .OpenBlock()
            .Line("global::Foundation.NSNotificationCenter.DefaultCenter.RemoveObserver(__token);")
            .Line("__token.Dispose();")
            .CloseBlock(");");
}
