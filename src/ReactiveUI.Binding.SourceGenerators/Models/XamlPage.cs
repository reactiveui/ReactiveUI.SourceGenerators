// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A XAML page whose named elements a XAML source generator turns into fields of its code-behind class.</summary>
/// <param name="ClassName">The code-behind class, from <c>x:Class</c>.</param>
/// <param name="Elements">The named elements, in document order.</param>
/// <param name="Root">
/// The type of the page's root element, which the XAML source generator makes the base class of the code-behind class
/// when the code-behind declares none. Null when the root's generic arguments cannot be read.
/// </param>
internal sealed record XamlPage(string ClassName, EquatableArray<XamlNamedElement> Elements, XamlTypeReference? Root = null);
