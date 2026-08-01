// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Represents the generated namespace and syntax for a property attribute.</summary>
/// <param name="AttributeNamespace">The optional namespace containing the attribute.</param>
/// <param name="AttributeSyntax">The generated attribute syntax.</param>
internal sealed record PropertyAttributeData(string? AttributeNamespace, string AttributeSyntax);
