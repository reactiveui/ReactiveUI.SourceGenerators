// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A XAML element with a name, which a XAML source generator turns into a field of the page's class.</summary>
/// <param name="Name">The field's name.</param>
/// <param name="Accessibility">The field's accessibility keywords.</param>
/// <param name="XmlNamespace">The XML namespace the element's type is declared in.</param>
/// <param name="TypeName">The element's type name, without its namespace.</param>
internal sealed record XamlNamedElement(string Name, string Accessibility, string XmlNamespace, string TypeName);
