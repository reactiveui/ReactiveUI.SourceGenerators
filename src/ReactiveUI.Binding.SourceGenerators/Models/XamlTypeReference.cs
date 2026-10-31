// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A type named in XAML, with the generic arguments <c>x:TypeArguments</c> gives it.</summary>
/// <param name="XmlNamespace">The XML namespace the type is declared in.</param>
/// <param name="TypeName">The type name, without its namespace and its generic arity.</param>
/// <param name="TypeArguments">The generic arguments, in order; empty for a type that is not generic.</param>
internal sealed record XamlTypeReference(string XmlNamespace, string TypeName, EquatableArray<XamlTypeReference> TypeArguments);
