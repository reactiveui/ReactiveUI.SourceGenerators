// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A namespace still to be walked, and the dotted prefix it was reached by.</summary>
/// <param name="Namespace">The namespace to walk.</param>
/// <param name="Prefix">The dotted prefix naming it.</param>
[DebuggerDisplay("NamespacePath: {Prefix,nq}")]
internal readonly record struct NamespacePath(INamespaceSymbol Namespace, string Prefix);
