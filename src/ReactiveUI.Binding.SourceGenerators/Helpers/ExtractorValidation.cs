// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Provides shared validation helpers extracted from the extractor classes.
/// These are internal methods that enable direct unit testing of guard-clause branches.
/// </summary>
internal static class ExtractorValidation
{
    /// <summary>The number of parameters on a standard .NET event handler delegate (sender, event args).</summary>
    private const int StandardEventHandlerParameterCount = 2;

    /// <summary>
    /// Checks whether a containing type name matches one of the recognized
    /// extension class names used by this generator (stub, scheduler, or generated).
    /// </summary>
    /// <param name="containingTypeName">The containing type name to check.</param>
    /// <returns><see langword="true"/> if the name is recognized; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsRecognizedExtensionClass(string? containingTypeName) =>
        containingTypeName is Constants.StubExtensionClassName
            or Constants.SchedulerExtensionClassName
            or Constants.GeneratedExtensionClassName
        || (containingTypeName is not null
            && containingTypeName.StartsWith($"{Constants.GeneratedExtensionClassName}_", StringComparison.Ordinal));

    /// <summary>
    /// Checks whether the type declaring an invoked method is one of the recognized extension classes
    /// used by this generator (stub, scheduler, or generated).
    /// </summary>
    /// <param name="containingType">The type containing the invoked method.</param>
    /// <returns><see langword="true"/> if the declaring class is recognized; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// An extension block declares its members in a synthesized grouping type rather than in the static class
    /// itself, so the class carrying the recognized name is one level further out. Which of the two a call site
    /// resolves to is decided by the compiler that hosts the generator rather than by anything in the consumer's
    /// project, and the two disagree across compiler versions, so both shapes are accepted. Rejecting the
    /// grouping type drops every call site of an API declared that way while leaving the rest generating.
    /// </remarks>
    internal static bool IsRecognizedExtensionClass(INamedTypeSymbol? containingType) =>
        containingType is not null
        && (IsRecognizedExtensionClass(containingType.Name)
            || (IsExtensionGroupingType(containingType)
                && IsRecognizedExtensionClass(containingType.ContainingType?.Name)));

    /// <summary>Checks whether an invocation has at least the required number of arguments.</summary>
    /// <param name="argumentCount">The actual argument count.</param>
    /// <param name="minimumRequired">The minimum required argument count.</param>
    /// <returns><see langword="true"/> if there are enough arguments; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasMinimumArguments(int argumentCount, int minimumRequired) =>
        argumentCount >= minimumRequired;

    /// <summary>Checks whether an immutable array of items is non-empty and should be processed.</summary>
    /// <typeparam name="T">The type of items in the array.</typeparam>
    /// <param name="items">The immutable array to check.</param>
    /// <returns><see langword="true"/> if the array has items; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasItems<T>(ImmutableArray<T> items) =>
        !items.IsDefaultOrEmpty;

    /// <summary>Extracts an <see cref="IMethodSymbol"/> from a <see cref="SymbolInfo"/>, returning null if the resolved symbol is not a method.</summary>
    /// <param name="symbolInfo">The symbol info from GetSymbolInfo.</param>
    /// <returns>The method symbol, or null.</returns>
    /// <remarks>
    /// Only a symbol the model actually resolved is used. Candidates are deliberately not consulted: a call the
    /// model could not narrow may have candidates of differing shape, and generating from one of those means
    /// reading a call's arguments against the wrong parameter list.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IMethodSymbol? ExtractMethodSymbol(SymbolInfo symbolInfo) =>
        symbolInfo.Symbol as IMethodSymbol;

    /// <summary>Determines whether generated code in the consumer's assembly can name a type.</summary>
    /// <param name="type">The type, which may be null.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when the type is closed and accessible from outside every type that declares it.</returns>
    /// <remarks>
    /// Generated overloads and interceptors are declared in a class of their own, so a private or protected
    /// nested type - or a generic closed over one - is out of their reach, and so is a type parameter of the
    /// calling code, alone or as a type argument of another type. An anonymous type has no name at all, and a
    /// file-local type has none outside its own file. The compiler counts both as accessible, so they are tested for
    /// separately. Naming one anyway fails the consumer's whole build over generated code they cannot edit.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsReachableFromGeneratedCode(ITypeSymbol? type, Compilation compilation) =>
        type is not null
        && !ContainsTypeParameter(type)
        && !ContainsNamelessType(type)
        && compilation.IsSymbolAccessibleWithin(type, compilation.Assembly);

    /// <summary>Determines whether a type is, or is built from, a type generated code has no name for.</summary>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true"/> when an anonymous or file-local type appears anywhere in the type.</returns>
    /// <remarks>Generated code sits in files of its own, so a file-local type is as nameless to it as an anonymous one.</remarks>
    internal static bool ContainsNamelessType(ITypeSymbol type) =>
        type switch
        {
            { IsAnonymousType: true } => true,
            IArrayTypeSymbol array => ContainsNamelessType(array.ElementType),
            INamedTypeSymbol named => named.IsFileLocal
                || AnyContainsNamelessType(named.TypeArguments)
                || (named.ContainingType is { } containing && ContainsNamelessType(containing)),
            _ => false,
        };

    /// <summary>Finds the one type argument a generic interceptor has to take in place of naming it.</summary>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="resultOrdinal">The position of the selector's result among the method's type arguments.</param>
    /// <returns>
    /// <see langword="true"/> when every type argument generated code cannot name is the selector's result, which an
    /// interceptor can take as a type parameter. <see langword="false"/> when another type argument is out of reach,
    /// or the method has no selector whose result is one of its own type parameters.
    /// </returns>
    /// <remarks>
    /// Generated code only passes the result along: the user's selector builds it and the observation hands it on.
    /// So an anonymous type, a private nested type, the caller's type parameter, or a type built from any of them can
    /// stay a type parameter there. The compiler closes the interceptor over whatever the call closed the result over.
    /// </remarks>
    internal static bool TryFindUnnameableResult(IMethodSymbol method, Compilation compilation, out int resultOrdinal)
    {
        resultOrdinal = FindSelectorResultOrdinal(method.OriginalDefinition);
        if (resultOrdinal < 0 || method.ContainingType.IsGenericType)
        {
            return false;
        }

        var typeArguments = method.TypeArguments;
        for (var i = 0; i < typeArguments.Length; i++)
        {
            if (i != resultOrdinal && !IsReachableFromGeneratedCode(typeArguments[i], compilation))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether a type is, or is built from, a type parameter.</summary>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true"/> when a type parameter appears anywhere in the type.</returns>
    internal static bool ContainsTypeParameter(ITypeSymbol type) =>
        type switch
        {
            ITypeParameterSymbol => true,
            IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
            INamedTypeSymbol named => AnyContainsTypeParameter(named.TypeArguments)
                || (named.ContainingType is { } containing && ContainsTypeParameter(containing)),
            _ => false,
        };

    /// <summary>Determines whether generated code can name every type an invoked binding method is closed over.</summary>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when every type argument is reachable.</returns>
    /// <remarks>
    /// A generated overload repeats the stub's signature with its type arguments substituted, and the stub's own
    /// types are public, so the type arguments are the only types the overload names that can be out of reach.
    /// </remarks>
    internal static bool NamesOnlyReachableTypes(IMethodSymbol method, Compilation compilation)
    {
        var typeArguments = method.TypeArguments;
        for (var i = 0; i < typeArguments.Length; i++)
        {
            if (!IsReachableFromGeneratedCode(typeArguments[i], compilation))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Gets the fully qualified display name of a type symbol, returning null if the symbol is null.</summary>
    /// <param name="type">The type symbol, which may be null.</param>
    /// <returns>The fully qualified type name, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetTypeDisplayName(ITypeSymbol? type) =>
        type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>Names a type only when a generated overload could declare a parameter of it.</summary>
    /// <param name="type">The type symbol, which may be null.</param>
    /// <returns>The fully qualified type name, or <see langword="null"/> when no overload could name it.</returns>
    /// <remarks>
    /// <para>
    /// A call made through a type parameter binds to whatever closes it, which the call site does not name. Writing the
    /// parameter's own name into code that is not generic over it puts an identifier no consumer declared into their
    /// build, so the whole compilation fails over generated code they cannot edit. <c>ReachScope.NameOf</c>
    /// names one only where the generated code is generic over it.
    /// </para>
    /// <para>
    /// A static type fails the same way and reaches here by a different route: a call written through the
    /// stub's declaring class - <c>ReactiveUIBindingExtensions.WhenChanged(vm, x => x.Name)</c> - puts that
    /// class where the observed object goes, and no member can declare a parameter of it or close a generic
    /// over it. Such a call resolves against that class's own members, so neither a generated overload nor an
    /// interceptor matching the call written on an instance is a candidate for it either way.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetDeclarableTypeDisplayName(ITypeSymbol? type) =>
        type is INamedTypeSymbol { IsStatic: false } named ? GetTypeDisplayName(named) : null;

    /// <summary>
    /// Searches method parameters for a selector or conversion function parameter
    /// and returns the fully qualified return type (the last type argument of the Func).
    /// </summary>
    /// <param name="parameters">The method parameters to search.</param>
    /// <param name="parameterNames">The parameter names to match (e.g. "selector", "conversionFunc").</param>
    /// <returns>The fully qualified return type, or null if no matching parameter was found.</returns>
    internal static string? FindSelectorReturnType(
        ImmutableArray<IParameterSymbol> parameters,
        params string[] parameterNames)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.Type is INamedTypeSymbol { TypeArguments.Length: > 0 } funcType)
            {
                for (var n = 0; n < parameterNames.Length; n++)
                {
                    if (parameter.Name == parameterNames[n])
                    {
                        return funcType.TypeArguments[funcType.TypeArguments.Length - 1]
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the event delegate type to its event args type.
    /// Returns "global::System.EventArgs" as a fallback when the delegate type
    /// is not a named type or has a non-standard parameter count.
    /// </summary>
    /// <param name="delegateType">The event's delegate type symbol.</param>
    /// <returns>The fully qualified event args type name.</returns>
    internal static string ResolveEventArgsType(ITypeSymbol? delegateType)
    {
        if (delegateType is not INamedTypeSymbol namedDelegateType)
        {
            return "global::System.EventArgs";
        }

        var invokeMethod = namedDelegateType.DelegateInvokeMethod;
        return invokeMethod is { Parameters.Length: StandardEventHandlerParameterCount }
            ? invokeMethod.Parameters[1].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : "global::System.EventArgs";
    }

    /// <summary>Checks whether a type is the synthesized grouping type that holds an extension block's members.</summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an extension grouping type; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The grouping type is unnamed when the declaration is read from source and is named <c>&lt;&gt;E__N</c>
    /// when it is read from metadata. Neither spelling is a legal C# identifier, so no type a consumer can
    /// declare is mistaken for one.
    /// </remarks>
    private static bool IsExtensionGroupingType(INamedTypeSymbol type) =>
        type.Name.Length == 0 || type.Name[0] == '<';

    /// <summary>Finds which of a method's own type parameters its selector returns.</summary>
    /// <param name="definition">The method as declared, with its type parameters unsubstituted.</param>
    /// <returns>The type parameter's ordinal, or -1 when the method has no selector returning one.</returns>
    private static int FindSelectorResultOrdinal(IMethodSymbol definition)
    {
        var parameters = definition.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] is { Name: "selector" or "conversionFunc", Type: INamedTypeSymbol { TypeArguments.Length: > 0 } func }
                && func.TypeArguments[func.TypeArguments.Length - 1] is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } result)
            {
                return result.Ordinal;
            }
        }

        return -1;
    }

    /// <summary>Determines whether any of a list of types is, or is built from, a type generated code has no name for.</summary>
    /// <param name="types">The types.</param>
    /// <returns><see langword="true"/> when an anonymous or file-local type appears in any of them.</returns>
    private static bool AnyContainsNamelessType(ImmutableArray<ITypeSymbol> types)
    {
        for (var i = 0; i < types.Length; i++)
        {
            if (ContainsNamelessType(types[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether any of a list of types is, or is built from, a type parameter.</summary>
    /// <param name="types">The types.</param>
    /// <returns><see langword="true"/> when a type parameter appears in any of them.</returns>
    private static bool AnyContainsTypeParameter(ImmutableArray<ITypeSymbol> types)
    {
        for (var i = 0; i < types.Length; i++)
        {
            if (ContainsTypeParameter(types[i]))
            {
                return true;
            }
        }

        return false;
    }
}
