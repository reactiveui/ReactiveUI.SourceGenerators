// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A member ReactiveUI.SourceGenerators adds to a consumer type, described so call sites can bind to it.</summary>
/// <param name="Declaration">The partial declarations that reach the type the member is added to.</param>
/// <param name="Interface">The fully qualified interface the type gains, or null when the member is a property.</param>
/// <param name="Property">The property's declaration, or null when the type only gains an interface.</param>
/// <remarks>
/// The property is declared with the name, type and accessibility ReactiveUI.SourceGenerators gives it. Its accessors
/// have bodies that never run: the declaration only exists for call sites to bind against.
/// </remarks>
internal sealed record SourceGeneratorsMember(PartialTypeDeclaration Declaration, string? Interface, string? Property);
