// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A call site whose generated code moves out of a class of its own, and the signature of the method it calls.</summary>
/// <param name="HostTypeFullName">
/// The fully qualified name of the caller's class the generated code moves into, or null when it stays in the generated
/// class and only becomes generic.
/// </param>
/// <param name="Declaration">The partial declarations that reach the caller's class, outermost first, or null.</param>
/// <param name="TypeParameters">The interceptor's type parameters, one for each of the called method's, in order.</param>
/// <param name="Constraints">The interceptor's constraint clauses, each starting with <c>where</c>.</param>
/// <param name="DeclaredReturnType">The called method's return type, written with the interceptor's type parameters. Every binding API returns a value.</param>
/// <param name="ClosedReturnType">The return type this call closes it to.</param>
/// <param name="Parameters">The called method's parameters, the receiver first.</param>
/// <remarks>
/// <para>
/// A private or protected nested type or member can only be named inside the class that declares it. When that class
/// is partial, the generator adds the call's generated code to it. A call built from the caller's type parameters gets
/// its code wrapped in a class generic over them. Either way the interceptor still has to sit in the generated
/// namespace, so it is generic: it repeats the called method's signature with its own type parameters, and forwards to
/// an entry point beside the moved code, which casts to the closed types the call was made with.
/// </para>
/// <para>
/// Strings only, so the model compares by value and carries no symbol.
/// </para>
/// </remarks>
internal sealed record HostedCall(
    string? HostTypeFullName,
    PartialTypeDeclaration? Declaration,
    EquatableArray<string> TypeParameters,
    EquatableArray<string> Constraints,
    string DeclaredReturnType,
    string ClosedReturnType,
    EquatableArray<HostedParameter> Parameters)
{
    /// <summary>Gets the caller's type parameters the moved code is generic over, by their own names.</summary>
    internal EquatableArray<string> WrapperTypeParameters { get; init; } = new([]);

    /// <summary>Gets the constraint clauses of the caller's type parameters, as the caller declares them.</summary>
    internal EquatableArray<string> WrapperConstraints { get; init; } = new([]);

    /// <summary>Gets the interceptor type parameter that hands on each of the caller's, in the same order.</summary>
    internal EquatableArray<string> WrapperTypeArguments { get; init; } = new([]);

    /// <summary>Gets a value indicating whether the return type has to be cast from the closed type to the declared one.</summary>
    internal bool ReturnNeedsCast => DeclaredReturnType != ClosedReturnType;

    /// <summary>Gets a value indicating whether the moved code is generic over the caller's type parameters.</summary>
    internal bool IsGeneric => WrapperTypeParameters.Length > 0;
}
