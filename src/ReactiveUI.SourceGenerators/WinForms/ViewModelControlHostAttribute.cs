// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>Generates a WinForms control that hosts the view for its view model.</summary>
/// <param name="baseType">The fully qualified name of the control type the generated host derives from.</param>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ViewModelControlHostAttribute(string? baseType) : Attribute
{
    /// <summary>Gets the fully qualified name of the control type the generated host derives from.</summary>
    public string? BaseType { get; } = baseType;
}
