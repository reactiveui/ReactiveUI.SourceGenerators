// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Decides where a call site's generated code is declared, so that it can name every type and member the call uses.</summary>
/// <remarks>
/// <para>
/// Generated code normally sits in a class of its own, which can only name closed types visible outside the classes
/// that declare them. On a build that intercepts, a call that needs more gets its code moved:
/// </para>
/// <list type="bullet">
/// <item>
/// A call built from the caller's type parameters moves into a generic class whose type parameters stand in for them.
/// Each of the caller's type parameters has to be one of the called method's own type arguments, so the interceptor can
/// hand it on.
/// </item>
/// <item>
/// A call that names a private or protected type or member moves into the caller's own partial class, which can name
/// it. That class has to be partial, together with every class it is nested in, and none of them may be generic.
/// </item>
/// </list>
/// <para>
/// An interceptor in the generated namespace claims the call. It repeats the called method's signature with its type
/// parameters, so it names nothing out of reach, and forwards to an entry point next to the moved code. A call that
/// meets none of these rules stays out of reach, and an error fails the build at the call.
/// </para>
/// </remarks>
internal static class CallSiteHosting
{
    /// <summary>The name prefix of the interceptor's own type parameters, which never clash with the caller's.</summary>
    private const string TypeParameterPrefix = "__S";

    /// <summary>The format the hosted signature is written in, which keeps nullable reference annotations.</summary>
    private static readonly SymbolDisplayFormat SignatureFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>Resolves the scope a call site's generated code is written in.</summary>
    /// <param name="context">The call site and its semantic model.</param>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="scope">The scope the generated code can name types in, which may move while the call is read.</param>
    /// <returns><see langword="true"/> when generated code can name every type argument of the call.</returns>
    internal static bool TryResolve(in CallSiteContext context, IMethodSymbol method, CancellationToken ct, out ReachScope scope)
    {
        scope = ReachScope.ForCall(context, method, ct);
        return scope.CanNameAll(method.TypeArguments);
    }

    /// <summary>Determines whether every type parameter a type is built from is one of a list.</summary>
    /// <param name="type">The type.</param>
    /// <param name="typeParameters">The type parameters in scope.</param>
    /// <returns><see langword="true"/> when the type names no other type parameter.</returns>
    internal static bool UsesOnly(ITypeSymbol type, ImmutableArray<ITypeParameterSymbol> typeParameters) =>
        type switch
        {
            ITypeParameterSymbol parameter => IndexOf(typeParameters, parameter) >= 0,
            IArrayTypeSymbol array => UsesOnly(array.ElementType, typeParameters),
            INamedTypeSymbol named => AllUseOnly(named.TypeArguments, typeParameters)
                && (named.ContainingType is not { } containing || UsesOnly(containing, typeParameters)),
            _ => true,
        };

    /// <summary>Moves a call's generated code out of a class of its own.</summary>
    /// <param name="context">The call site and its semantic model.</param>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="intoCaller">Whether the code moves into the caller's own partial class, rather than only a generic class.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Where the code moved to, or null when the call cannot move there.</returns>
    internal static MovedScope? TryMove(in CallSiteContext context, IMethodSymbol method, bool intoCaller, CancellationToken ct)
    {
        if (CallerTypeParameters(method) is not { } callerTypeParameters || (!intoCaller && callerTypeParameters.IsEmpty))
        {
            return null;
        }

        INamedTypeSymbol? hostType = null;
        PartialTypeDeclaration? declaration = null;
        if (intoCaller)
        {
            hostType = FindHostType(context, context.SemanticModel.Compilation, ct);
            declaration = hostType is null ? null : PartialTypeRaisePlugin.DescribeDeclaration(hostType);
            if (declaration is null)
            {
                return null;
            }
        }

        return Describe(method, hostType, declaration, callerTypeParameters) is { } call
            ? new MovedScope(hostType, callerTypeParameters, call)
            : null;
    }

    /// <summary>Writes the constraint clauses of a list of type parameters, with each type parameter renamed as given.</summary>
    /// <param name="typeParameters">The type parameters, in order.</param>
    /// <param name="names">The name each type parameter, and each type parameter its constraints name, is written as.</param>
    /// <returns>The clauses, each starting with <c>where</c>; a type parameter with no constraint gets none.</returns>
    /// <remarks>
    /// Generic code that repeats a parameter type such as <c>IViewFor&lt;TViewModel&gt;</c> only accepts a type argument
    /// that meets the same constraints, so the constraints are repeated too.
    /// </remarks>
    internal static EquatableArray<string> ConstraintClauses(
        ImmutableArray<ITypeParameterSymbol> typeParameters,
        Dictionary<ITypeParameterSymbol, string> names)
    {
        var clauses = new List<string>(typeParameters.Length);
        foreach (var typeParameter in typeParameters)
        {
            if (TypeParameterConstraints.Of(typeParameter, names).Format() is { } constraints)
            {
                clauses.Add($"where {names[typeParameter]} : {constraints}");
            }
        }

        return new([.. clauses]);
    }

    /// <summary>Writes a type, with each type parameter in a map written by its mapped name.</summary>
    /// <param name="type">The type.</param>
    /// <param name="names">The names to write type parameters by.</param>
    /// <returns>The type's text.</returns>
    internal static string Render(ITypeSymbol type, Dictionary<ITypeParameterSymbol, string> names)
    {
        var text = new PooledStringBuilder();
        foreach (var part in type.ToDisplayParts(SignatureFormat))
        {
            _ = part.Symbol is ITypeParameterSymbol parameter && names.TryGetValue(parameter, out var name)
                ? text.Append(name)
                : text.Append(part.ToString());
        }

        return text.ToStringAndReturn();
    }

    /// <summary>Collects the caller's type parameters a call is closed over, when each can be handed on by the interceptor.</summary>
    /// <param name="method">The resolved binding method.</param>
    /// <returns>
    /// The type parameters in the order the call first names them, or null when one of them is not itself one of the
    /// method's type arguments, or when their constraints name a type parameter outside the list.
    /// </returns>
    private static ImmutableArray<ITypeParameterSymbol>? CallerTypeParameters(IMethodSymbol method)
    {
        var found = new List<ITypeParameterSymbol>();
        foreach (var argument in method.TypeArguments)
        {
            Collect(argument, found);
        }

        var collected = found.ToImmutableArray();
        foreach (var parameter in collected)
        {
            if (IndexOf(method.TypeArguments, parameter) < 0)
            {
                return null;
            }

            foreach (var constraint in parameter.ConstraintTypes)
            {
                if (!UsesOnly(constraint, collected))
                {
                    return null;
                }
            }
        }

        return collected;
    }

    /// <summary>Adds every type parameter a type is built from to a list, once each.</summary>
    /// <param name="type">The type.</param>
    /// <param name="found">The list.</param>
    private static void Collect(ITypeSymbol type, List<ITypeParameterSymbol> found)
    {
        if (type is ITypeParameterSymbol parameter)
        {
            AddOnce(found, parameter);
        }
        else if (type is IArrayTypeSymbol array)
        {
            Collect(array.ElementType, found);
        }
        else if (type is INamedTypeSymbol named)
        {
            foreach (var argument in named.TypeArguments)
            {
                Collect(argument, found);
            }

            if (named.ContainingType is { } containing)
            {
                Collect(containing, found);
            }
        }
    }

    /// <summary>Adds a type parameter to a list unless it is already there.</summary>
    /// <param name="found">The list.</param>
    /// <param name="parameter">The type parameter.</param>
    private static void AddOnce(List<ITypeParameterSymbol> found, ITypeParameterSymbol parameter)
    {
        for (var i = 0; i < found.Count; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(found[i], parameter))
            {
                return;
            }
        }

        found.Add(parameter);
    }

    /// <summary>Finds a symbol in a list, by symbol equality.</summary>
    /// <typeparam name="T">The kind of symbol.</typeparam>
    /// <param name="symbols">The list.</param>
    /// <param name="symbol">The symbol.</param>
    /// <returns>Its position, or -1.</returns>
    private static int IndexOf<T>(ImmutableArray<T> symbols, ISymbol symbol)
        where T : ISymbol
    {
        for (var i = 0; i < symbols.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(symbols[i], symbol))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Determines whether every type in a list names only the given type parameters.</summary>
    /// <param name="types">The types.</param>
    /// <param name="typeParameters">The type parameters in scope.</param>
    /// <returns><see langword="true"/> when they do.</returns>
    private static bool AllUseOnly(ImmutableArray<ITypeSymbol> types, ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        for (var i = 0; i < types.Length; i++)
        {
            if (!UsesOnly(types[i], typeParameters))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Finds the innermost class around a call site that the generated namespace can reach.</summary>
    /// <param name="context">The call site and its semantic model.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The class, or null when it is file-local or a generic class encloses the call before it is found.</returns>
    /// <remarks>
    /// A binding call always sits in a member, so it always has a class around it, and the outermost class is visible
    /// to its own assembly. A private class nested in the caller's class is skipped outward: the interceptor has to
    /// call into the class it picks, so that class has to be visible from the generated namespace.
    /// </remarks>
    private static INamedTypeSymbol? FindHostType(in CallSiteContext context, Compilation compilation, CancellationToken ct)
    {
        var type = context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart, ct)!.ContainingType;
        while (!type.IsGenericType && !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            type = type.ContainingType;
        }

        // A file-local class cannot be extended from a generated file, which is never the same file.
        return type.IsGenericType || ExtractorValidation.ContainsNamelessType(type) ? null : type;
    }

    /// <summary>Describes the method a moved call site calls, in the terms its generic interceptor needs.</summary>
    /// <param name="method">The resolved binding method, as the call closes it.</param>
    /// <param name="hostType">The caller's class the code moves into, or null for the generated class.</param>
    /// <param name="declaration">The partial declarations that reach the caller's class.</param>
    /// <param name="callerTypeParameters">The caller's type parameters the moved code is generic over.</param>
    /// <returns>The description, or null when the method is not a classic extension method.</returns>
    /// <remarks>
    /// A member of an extension block is not a classic extension method, and an interceptor for it would have to take
    /// the block's type parameters as well, so it does not move.
    /// </remarks>
    private static HostedCall? Describe(
        IMethodSymbol method,
        INamedTypeSymbol? hostType,
        PartialTypeDeclaration? declaration,
        ImmutableArray<ITypeParameterSymbol> callerTypeParameters)
    {
        var closed = method.GetConstructedReducedFrom() ?? method;
        var definition = closed.OriginalDefinition;
        if (!definition.IsExtensionMethod)
        {
            return null;
        }

        // The interceptor's type parameters are renamed so they never clash with the caller's, which the moved code keeps.
        var stubNames = new Dictionary<ITypeParameterSymbol, string>(SymbolEqualityComparer.Default);
        var typeParameters = new string[definition.TypeParameters.Length];
        for (var i = 0; i < typeParameters.Length; i++)
        {
            typeParameters[i] = $"{TypeParameterPrefix}{i}";
            stubNames[definition.TypeParameters[i]] = typeParameters[i];
        }

        // Each caller type parameter is handed on as the interceptor type parameter the call closed over it.
        var callerNames = new Dictionary<ITypeParameterSymbol, string>(SymbolEqualityComparer.Default);
        var handedOn = new Dictionary<ITypeParameterSymbol, string>(SymbolEqualityComparer.Default);
        var wrapperParameters = new string[callerTypeParameters.Length];
        var wrapperArguments = new string[callerTypeParameters.Length];
        for (var i = 0; i < callerTypeParameters.Length; i++)
        {
            wrapperParameters[i] = callerTypeParameters[i].Name;
            wrapperArguments[i] = typeParameters[IndexOf(closed.TypeArguments, callerTypeParameters[i])];
            callerNames[callerTypeParameters[i]] = wrapperParameters[i];
            handedOn[callerTypeParameters[i]] = wrapperArguments[i];
        }

        var parameters = new HostedParameter[definition.Parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            parameters[i] = new(
                definition.Parameters[i].Name,
                Render(definition.Parameters[i].Type, stubNames),
                Render(closed.Parameters[i].Type, callerNames));
        }

        return new(
            hostType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            declaration,
            new(typeParameters),
            InterceptorConstraints(definition, closed, stubNames, handedOn),
            Render(definition.ReturnType, stubNames),
            Render(closed.ReturnType, callerNames),
            new(parameters))
        {
            WrapperTypeParameters = new(wrapperParameters),
            WrapperConstraints = ConstraintClauses(callerTypeParameters, callerNames),
            WrapperTypeArguments = new(wrapperArguments),
        };
    }

    /// <summary>Writes the interceptor's constraint clauses: the called method's, and those of each caller type parameter it hands on.</summary>
    /// <param name="definition">The called method as declared.</param>
    /// <param name="closed">The called method as the call closes it.</param>
    /// <param name="stubNames">The interceptor's name for each of the method's type parameters.</param>
    /// <param name="handedOn">The interceptor's name for each caller type parameter.</param>
    /// <returns>The clauses.</returns>
    /// <remarks>
    /// The moved code is generic over the caller's type parameters with their constraints, so the interceptor type
    /// parameter that stands in for one has to meet them too.
    /// </remarks>
    private static EquatableArray<string> InterceptorConstraints(
        IMethodSymbol definition,
        IMethodSymbol closed,
        Dictionary<ITypeParameterSymbol, string> stubNames,
        Dictionary<ITypeParameterSymbol, string> handedOn)
    {
        var clauses = new List<string>(definition.TypeParameters.Length);
        for (var i = 0; i < definition.TypeParameters.Length; i++)
        {
            var constraints = TypeParameterConstraints.Of(definition.TypeParameters[i], stubNames);
            if (closed.TypeArguments[i] is ITypeParameterSymbol caller)
            {
                constraints = constraints.With(TypeParameterConstraints.Of(caller, handedOn));
            }

            if (constraints.Format() is { } text)
            {
                clauses.Add($"where {stubNames[definition.TypeParameters[i]]} : {text}");
            }
        }

        return new([.. clauses]);
    }

    /// <summary>Where a call's generated code moved to.</summary>
    /// <param name="Host">The caller's class, or null when the code stays in the generated class.</param>
    /// <param name="TypeParameters">The caller's type parameters the moved code is generic over.</param>
    /// <param name="Call">The call, described for the interceptor that claims it.</param>
    internal sealed record MovedScope(INamedTypeSymbol? Host, ImmutableArray<ITypeParameterSymbol> TypeParameters, HostedCall Call);

    /// <summary>The constraints on one type parameter, as written in a <c>where</c> clause.</summary>
    /// <param name="Kind">The kind constraint, such as <c>class</c> or <c>struct</c>, or null.</param>
    /// <param name="Types">The constraint types, written out.</param>
    /// <param name="HasConstructor">Whether the type parameter has a constructor constraint.</param>
    internal sealed record TypeParameterConstraints(string? Kind, List<string> Types, bool HasConstructor)
    {
        /// <summary>Reads a type parameter's constraints.</summary>
        /// <param name="typeParameter">The type parameter.</param>
        /// <param name="names">The names to write type parameters by.</param>
        /// <returns>The constraints.</returns>
        internal static TypeParameterConstraints Of(ITypeParameterSymbol typeParameter, Dictionary<ITypeParameterSymbol, string> names)
        {
            var types = new List<string>(typeParameter.ConstraintTypes.Length);
            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                types.Add(Render(constraintType, names));
            }

            return new(KindOf(typeParameter), types, typeParameter.HasConstructorConstraint);
        }

        /// <summary>Combines these constraints with another set on the same type argument.</summary>
        /// <param name="other">The other set.</param>
        /// <returns>The combined set.</returns>
        /// <remarks>
        /// The type argument already meets both, because the call compiled, so the sets never conflict. A plain class
        /// constraint is kept over an annotated or a not-null one, which it implies.
        /// </remarks>
        internal TypeParameterConstraints With(TypeParameterConstraints other)
        {
            var types = new List<string>(Types);
            foreach (var type in other.Types)
            {
                if (!types.Contains(type))
                {
                    types.Add(type);
                }
            }

            return new(CombineKinds(Kind, other.Kind), types, HasConstructor || other.HasConstructor);
        }

        /// <summary>Writes the constraints in the order C# requires.</summary>
        /// <returns>The constraints, comma separated, or null when there are none.</returns>
        internal string? Format()
        {
            var parts = new List<string>();
            if (Kind is not null)
            {
                parts.Add(Kind);
            }

            parts.AddRange(Types);

            // A value type always has a constructor, and C# refuses the constraint beside struct or unmanaged.
            if (HasConstructor && Kind is not ("struct" or "unmanaged"))
            {
                parts.Add("new()");
            }

            return parts.Count == 0 ? null : string.Join(", ", parts);
        }

        /// <summary>Picks the kind constraint that implies the other.</summary>
        /// <param name="first">The first kind, or null.</param>
        /// <param name="second">The second kind, or null.</param>
        /// <returns>The stricter kind.</returns>
        private static string? CombineKinds(string? first, string? second)
        {
            if (first is null || first == second)
            {
                return second ?? first;
            }

            return second is null || first is not ("class?" or "notnull") ? first : second;
        }

        /// <summary>Reads a type parameter's kind constraint.</summary>
        /// <param name="typeParameter">The type parameter.</param>
        /// <returns>The constraint, or null when it has none.</returns>
        private static string? KindOf(ITypeParameterSymbol typeParameter) =>
            typeParameter switch
            {
                { HasReferenceTypeConstraint: true } => typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class",
                { HasUnmanagedTypeConstraint: true } => "unmanaged",
                { HasValueTypeConstraint: true } => "struct",
                { HasNotNullConstraint: true } => "notnull",
                _ => null,
            };
    }
}
