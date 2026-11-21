// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Registers a view with <c>GeneratedViewRegistrations.RegisterGeneratedViews</c> without runtime assembly scanning.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
[DebuggerDisplay("RegisterViewAttribute: Contract = {Contract}, IsSingleton = {IsSingleton}")]
public sealed class RegisterViewAttribute : Attribute
{
    /// <summary>Gets or sets the optional view contract.</summary>
    public string? Contract { get; set; }

    /// <summary>Gets or sets a value indicating whether to register the view as a singleton.</summary>
    public bool IsSingleton { get; set; }
}
