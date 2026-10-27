// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>How generated code raises a type's change notifications for a property it backs.</summary>
/// <param name="Mechanism">The plugin that chose this mechanism.</param>
/// <param name="Changed">Raises the after-change notification.</param>
/// <param name="Changing">Raises the before-change notification, or null when the type offers none.</param>
/// <param name="Accessor">
/// The partial declaration the raise calls are emitted into, when they can only run inside the type; null when
/// generated code can make them directly.
/// </param>
internal sealed record PropertyRaiseInfo(
    string Mechanism,
    PropertyRaiseCall Changed,
    PropertyRaiseCall? Changing,
    PartialTypeDeclaration? Accessor);
