// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Value-equatable POCO representing a view-to-view-model mapping detected at compile time.
/// Produced by scanning <c>IViewFor&lt;T&gt;</c> implementations and <c>Map&lt;TVM,TView&gt;()</c> call sites.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="ViewModelFullyQualifiedName">The fully qualified name of the view model type (global:: prefixed).</param>
/// <param name="ViewFullyQualifiedName">The fully qualified name of the view type (global:: prefixed).</param>
/// <param name="HasParameterlessConstructor">Whether the view type has a parameterless constructor for direct instantiation.</param>
/// <param name="Contract">The contract string from <c>[ViewContract]</c>, or <see langword="null"/> for default registration.</param>
/// <param name="IsSingleInstance">Whether the view is marked with <c>[SingleInstanceView]</c> for cached singleton resolution.</param>
/// <param name="IsDeclaredByAttribute">
/// Whether the view was found through ReactiveUI.SourceGenerators' <c>[IViewFor]</c> rather than an <c>IViewFor&lt;T&gt;</c>
/// in its source. That generator adds the interface, which no other generator can see, so the resolver checks for it
/// at run time.
/// </param>
internal sealed record ViewRegistrationInfo(
    string ViewModelFullyQualifiedName,
    string ViewFullyQualifiedName,
    bool HasParameterlessConstructor,
    string? Contract,
    bool IsSingleInstance,
    bool IsDeclaredByAttribute = false);
