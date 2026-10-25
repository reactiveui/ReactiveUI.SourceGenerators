// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>One parameter of a method a hosted call site calls.</summary>
/// <param name="Name">The parameter's name.</param>
/// <param name="DeclaredType">The parameter's type, written with the method's type parameters.</param>
/// <param name="ClosedType">The type the call closes it to, which may name a private nested type.</param>
internal readonly record struct HostedParameter(string Name, string DeclaredType, string ClosedType)
{
    /// <summary>Gets a value indicating whether the argument has to be cast from the declared type to the closed one.</summary>
    internal bool NeedsCast => DeclaredType != ClosedType;
}
