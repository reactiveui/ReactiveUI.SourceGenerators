// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The partial declaration a raise accessor is emitted into, from the outermost type to the owning type.</summary>
/// <param name="Namespace">The namespace the outermost type sits in, or null for the global namespace.</param>
/// <param name="TypeHeaders">One <c>partial class Name&lt;T&gt;</c> header per type, outermost first.</param>
internal sealed record PartialTypeDeclaration(string? Namespace, EquatableArray<string> TypeHeaders);
