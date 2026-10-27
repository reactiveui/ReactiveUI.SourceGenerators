// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// The observation emission shared by every plugin that watches a type through
/// <see cref="System.ComponentModel.INotifyPropertyChanged"/> and
/// <see cref="System.ComponentModel.INotifyPropertyChanging"/>.
/// </summary>
/// <remarks>
/// A plain INPC type and an <c>IReactiveObject</c> differ only in which plugin claims them and at what
/// affinity - <c>IReactiveObject</c> implements both interfaces, so the code emitted for the two is the
/// same <c>PropertyObservable</c> / <c>PropertyChangingObservable</c> either way. The emission lives here
/// once so the two plugins stay in step.
/// </remarks>
internal static class NotifyPropertyEmitter
{
    /// <summary>Emits a shallow observation as an inline expression.</summary>
    /// <param name="sb">The writer, part way through the line the expression belongs to.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="castTypeName">The type the observed object is cast to.</param>
    /// <param name="isBeforeChange">True for before-change observation.</param>
    /// <param name="includeStartWith">Whether the observation emits the current value on subscribe.</param>
    internal static void EmitShallowObservation(
        SourceWriter sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        var read = GeneratedTypeNames.ReadProperty(segment, castTypeName, "__o");
        if (isBeforeChange)
        {
            _ = sb.Append($"new {GeneratedTypeNames.PropertyChangingObservable}<").Append(segment.PropertyTypeFullName).Append(">((")
                .Append($"{GeneratedTypeNames.INotifyPropertyChanging})").Append(rootVar).Append(", ").AppendQuoted(segment.PropertyName)
                .Append($", ({GeneratedTypeNames.INotifyPropertyChanging} __o) => ")
                .Append(read).Append(')');
            return;
        }

        _ = sb.Append($"new {GeneratedTypeNames.PropertyObservable}<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append(rootVar).Append(", ").AppendQuoted(segment.PropertyName)
            .Append($", ({GeneratedTypeNames.INotifyPropertyChanged} __o) => ").Append(read)
            .Append(", ").AppendLiteral(includeStartWith).Append(')');
    }
}
