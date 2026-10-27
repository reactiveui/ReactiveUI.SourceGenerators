// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Where a call's selector result sits when generated code takes it as a type parameter instead of naming it.</summary>
/// <param name="Arity">How many type arguments the called method takes, or zero when every type is named.</param>
/// <param name="Ordinal">The position of the selector's result among those type arguments.</param>
/// <remarks>
/// A selector can return an anonymous type, a private nested type, or a type built from one. Generated code cannot
/// write any of those. It only passes the result along, so the observation method is generic over it, and so is the
/// interceptor that claims the call. An interceptor's type parameters have to line up with the called method's, which
/// is why the arity and the position travel with the call site.
/// </remarks>
internal readonly record struct GenericResult(int Arity, int Ordinal)
{
    /// <summary>The name generated code gives the result's type parameter.</summary>
    internal const string TypeParameterName = "__TResult";

    /// <summary>Gets a value indicating whether generated code takes the result as a type parameter.</summary>
    internal bool IsGeneric => Arity > 0;
}
