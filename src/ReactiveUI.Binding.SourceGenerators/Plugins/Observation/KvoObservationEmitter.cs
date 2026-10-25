// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits Apple KVO observations through the runtime's key-value observing observable.</summary>
internal static class KvoObservationEmitter
{
    /// <summary>Emits a typed native property observation.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="rootVar">The observed source variable.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="castTypeName">The concrete source type.</param>
    /// <param name="isBeforeChange">Whether to observe before the change.</param>
    /// <param name="includeStartWith">Whether equal consecutive values are suppressed.</param>
    internal static void Emit(
        SourceWriter sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        var keyPath = ResolveKeyPath(segment);
        _ = sb.Append(GeneratedTypeNames.OpenKvoProperty).Append(segment.PropertyTypeFullName).Append(">(").Append($"({GeneratedTypeNames.NSObject})")
            .Append(rootVar).Append(", ").Append('"').Append(keyPath).Append("\", ").Append($"({GeneratedTypeNames.NSObject} __o) => ((")
            .Append(castTypeName).Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(", ").Append(BoolLiteral(includeStartWith)).Append(", ")
            .Append(BoolLiteral(isBeforeChange)).Append(')');
    }

    /// <summary>
    /// Converts a .NET property name to a KVO key path using the standard naming convention.
    /// Boolean properties get an "Is" prefix unless they already start with "Is"
    /// (e.g., <c>Enabled</c> → <c>"isEnabled"</c>, but <c>IsEnabled</c> → <c>"isEnabled"</c>).
    /// All others: lowercase first character (e.g., <c>Text</c> → <c>"text"</c>).
    /// </summary>
    /// <param name="propertyName">The .NET property name.</param>
    /// <param name="propertyTypeFullName">The fully qualified property type (e.g., "bool", "string").</param>
    /// <returns>The KVO key path string.</returns>
    internal static string ToKvoKeyPath(string propertyName, string propertyTypeFullName)
    {
        if (propertyTypeFullName == "bool" && !propertyName.StartsWith("Is", StringComparison.Ordinal))
        {
            propertyName = $"Is{propertyName}";
        }

        return propertyName.Length == 0 ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }

    /// <summary>Renders a boolean as the lowercase C# literal text (<c>true</c>/<c>false</c>) for emission into generated source.</summary>
    /// <param name="value">The boolean value.</param>
    /// <returns><c>"true"</c> or <c>"false"</c>.</returns>
    private static string BoolLiteral(bool value) => value ? "true" : "false";

    /// <summary>Uses the exported getter selector verified by symbol inspection.</summary>
    /// <param name="segment">The property to observe.</param>
    /// <returns>The native key path, escaped for a generated string literal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ResolveKeyPath(PropertyPathSegment segment) =>
        CodeGeneratorHelpers.EscapeString(PlatformSymbols.Candidate(segment.DeclaringTypeInfo, segment.PropertyName, "KVO")?.KvoKeyPath
            ?? ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName));
}
