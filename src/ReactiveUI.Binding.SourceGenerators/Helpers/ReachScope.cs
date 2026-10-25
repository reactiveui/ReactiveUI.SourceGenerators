// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Where a call site's generated code is declared, which decides the types and members it can name.</summary>
/// <remarks>
/// <para>
/// Most generated code sits in a class of its own in the consumer's assembly. On a build that intercepts, a call that
/// needs more moves, the first time it needs it:
/// </para>
/// <list type="number">
/// <item>into a generic class whose type parameters stand in for the caller's, when the call is built from them;</item>
/// <item>into the caller's own partial class, when the call names a private or protected type or member.</item>
/// </list>
/// <para>
/// A move happens while the call's types and paths are read, so a call that needs neither costs nothing more. Never part
/// of a pipeline's output.
/// </para>
/// </remarks>
internal sealed class ReachScope
{
    /// <summary>The step a scope that cannot move any further is at.</summary>
    private const int LastStep = 2;

    /// <summary>The call site and its semantic model, for a scope that can move.</summary>
    private readonly CallSiteContext _context;

    /// <summary>The resolved binding method, or null for a scope that never moves.</summary>
    private readonly IMethodSymbol? _method;

    /// <summary>Cancels the reads a move makes.</summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>How far the scope has moved: 0 not at all, 1 into a generic class, 2 into the caller's class.</summary>
    private int _step;

    /// <summary>Initializes a new instance of the <see cref="ReachScope"/> class.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="context">The call site and its semantic model.</param>
    /// <param name="method">The resolved binding method, or null for a scope that never moves.</param>
    /// <param name="cancellationToken">Cancels the reads a move makes.</param>
    private ReachScope(Compilation compilation, in CallSiteContext context, IMethodSymbol? method, CancellationToken cancellationToken)
    {
        Compilation = compilation;
        _context = context;
        _method = method;
        _cancellationToken = cancellationToken;
        _step = method is not null && InterceptableLocationReader.IsSupported ? 0 : LastStep;
    }

    /// <summary>Gets the consumer compilation.</summary>
    internal Compilation Compilation { get; }

    /// <summary>Gets the caller's class the generated code is declared in, or null when it sits in a class of its own.</summary>
    internal INamedTypeSymbol? Host { get; private set; }

    /// <summary>Gets the caller's type parameters the generated code is generic over.</summary>
    internal ImmutableArray<ITypeParameterSymbol> TypeParameters { get; private set; } = ImmutableArray<ITypeParameterSymbol>.Empty;

    /// <summary>Gets the call, described for the interceptor that claims it from the moved code, or null when it did not move.</summary>
    internal HostedCall? Call { get; private set; }

    /// <summary>Creates a scope that never moves, for code declared in a class of its own.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The scope.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReachScope Assembly(Compilation compilation) => new(compilation, default, null, default);

    /// <summary>Creates a scope for one call site, which may move when the call needs it.</summary>
    /// <param name="context">The call site and its semantic model.</param>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="cancellationToken">Cancels the reads a move makes.</param>
    /// <returns>The scope.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReachScope ForCall(in CallSiteContext context, IMethodSymbol method, CancellationToken cancellationToken) =>
        new(context.SemanticModel.Compilation, context, method, cancellationToken);

    /// <summary>Stops the scope from moving, for a call whose generated code has to stay in a class of its own.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Pin() => _step = LastStep;

    /// <summary>Determines whether code in this scope can name a type, moving the scope if that makes it possible.</summary>
    /// <param name="type">The type, which may be null.</param>
    /// <returns><see langword="true"/> when the type is closed over names in scope, has a name, and is accessible here.</returns>
    internal bool CanName(ITypeSymbol? type)
    {
        while (!CanNameHere(type))
        {
            if (!Move())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether code in this scope can name every type in a list, moving the scope if need be.</summary>
    /// <param name="types">The types.</param>
    /// <returns><see langword="true"/> when every type can be named here.</returns>
    internal bool CanNameAll(ImmutableArray<ITypeSymbol> types)
    {
        for (var i = 0; i < types.Length; i++)
        {
            if (!CanName(types[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Names a type a generated member declares a parameter of, when code in this scope can.</summary>
    /// <param name="type">The type, which may be null.</param>
    /// <returns>The fully qualified name, or null when no generated member could declare a parameter of it.</returns>
    /// <remarks>
    /// The calling code's type parameter is named only when the generated code is generic over it, moving the scope there
    /// if it can.
    /// </remarks>
    internal string? NameOf(ITypeSymbol? type)
    {
        if (type is not ITypeParameterSymbol)
        {
            return ExtractorValidation.GetDeclarableTypeDisplayName(type);
        }

        return CanName(type) ? ExtractorValidation.GetTypeDisplayName(type) : null;
    }

    /// <summary>Determines whether code in this scope can read a member, moving the scope into the caller's class if need be.</summary>
    /// <param name="member">The property or field.</param>
    /// <param name="owner">The type the member is read through.</param>
    /// <returns><see langword="true"/> when the member is visible to the assembly, or to the caller's class.</returns>
    internal bool CanRead(ISymbol member, ITypeSymbol owner)
    {
        if (member.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
        {
            return true;
        }

        while (Host is null)
        {
            if (!Move())
            {
                return false;
            }
        }

        return Compilation.IsSymbolAccessibleWithin(member, Host, owner);
    }

    /// <summary>Takes the next step out of a class of its own, when the call allows it.</summary>
    /// <returns><see langword="true"/> when the scope moved.</returns>
    private bool Move()
    {
        while (_step < LastStep)
        {
            _step++;
            if (CallSiteHosting.TryMove(_context, _method!, _step == LastStep, _cancellationToken) is not { } moved)
            {
                continue;
            }

            Host = moved.Host;
            TypeParameters = moved.TypeParameters;
            Call = moved.Call;
            return true;
        }

        return false;
    }

    /// <summary>Determines whether code in this scope, as it stands, can name a type.</summary>
    /// <param name="type">The type, which may be null.</param>
    /// <returns><see langword="true"/> when it can.</returns>
    private bool CanNameHere(ITypeSymbol? type) =>
        type is not null
        && !ExtractorValidation.ContainsNamelessType(type)
        && CallSiteHosting.UsesOnly(type, TypeParameters)
        && Compilation.IsSymbolAccessibleWithin(type, (ISymbol?)Host ?? Compilation.Assembly);
}
