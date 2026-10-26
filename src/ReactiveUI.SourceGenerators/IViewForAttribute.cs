// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Makes a partial UI control implement <c>IViewFor&lt;TViewModel&gt;</c> for a view model named by its type name.</summary>
/// <param name="viewModelType">The view model's type name; use the full name including the namespace when it is not in scope.</param>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("IViewForAttribute: ViewModelType = {ViewModelType}")]
public sealed class IViewForAttribute(string? viewModelType) : Attribute
{
    /// <summary>Gets the view model's type name.</summary>
    public string? ViewModelType { get; } = viewModelType;
}
