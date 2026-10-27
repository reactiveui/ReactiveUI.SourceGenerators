// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits WPF dependency-property observation with a typed getter.</summary>
internal static class WpfObservationEmitter
{
    /// <summary>Closes the descriptor lookup and subscribes the generated handler.</summary>
    private const string AddValueChangedCall = ")).AddValueChanged(";

    /// <summary>Closes the descriptor lookup and unsubscribes the generated handler.</summary>
    private const string RemoveValueChangedCall = ")).RemoveValueChanged(";

    /// <summary>Emits a typed native property observation.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="rootVar">The observed source variable.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="castTypeName">The concrete source type.</param>
    /// <param name="includeStartWith">Whether equal consecutive values are suppressed.</param>
    internal static void Emit(
        SourceWriter sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool includeStartWith) =>
        _ = sb.Append($"new {GeneratedTypeNames.Observables}.EventObservable<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append("__h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(").Append(castTypeName).Append('.')
            .Append(segment.PropertyName).Append("Property,").Append(" typeof(").Append(castTypeName).Append(AddValueChangedCall).Append(rootVar)
            .Append(", __h), ").Append("__h => global::System.ComponentModel.DependencyPropertyDescriptor.FromProperty(").Append(castTypeName).Append('.')
            .Append(segment.PropertyName).Append("Property,").Append(" typeof(").Append(castTypeName).Append(RemoveValueChangedCall).Append(rootVar)
            .Append(", __h), ").Append("() => ((").Append(castTypeName).Append(')').Append(rootVar).Append(").").Append(segment.PropertyName).Append(", ")
            .AppendLiteral(includeStartWith).Append(')');
}
