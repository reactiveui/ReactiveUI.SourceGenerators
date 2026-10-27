// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>A method symbol and the compilation it was resolved from.</summary>
/// <param name="MethodSymbol">The resolved method.</param>
/// <param name="Compilation">The compilation it belongs to.</param>
[DebuggerDisplay("MethodSymbolContext: {MethodSymbol.Name,nq}")]
internal readonly record struct MethodSymbolContext(IMethodSymbol MethodSymbol, Compilation Compilation);
