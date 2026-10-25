// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Writes the one thing every intercepted call site needs, whichever API it belongs to: the attribute naming
/// the call, and the declaration of that attribute.
/// </summary>
/// <remarks>
/// An interceptor redirects a call the compiler has already bound, so nothing about it depends on which API is
/// being generated - only on where the call is and what it should run instead. That is why every generator
/// shares this rather than growing its own copy, and why the tier needs none of the namespace placement the
/// dispatch overloads do: lookup never sees an interceptor.
/// </remarks>
internal static class InterceptorEmitter
{
    /// <summary>The attribute a generated method carries to claim a call site.</summary>
    private const string AttributeName = "global::System.Runtime.CompilerServices.InterceptsLocation";

    /// <summary>Room for the attribute declaration, which is a fixed block of text.</summary>
    private const int DeclarationCapacity = 640;

    /// <summary>Writes the parameter list one API's members declare, closing it.</summary>
    /// <param name="builder">The writer, inside the parameter list.</param>
    /// <param name="first">The invocation whose types the parameters are written from.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    internal delegate void ParameterListWriter(SourceWriter builder, InvocationInfo first, in LanguageFeatures features);

    /// <summary>Writes the attribute for every call site one generated method claims, then opens that method's parameter list.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="builder">The writer, at the class's member level; left inside the parameter list.</param>
    /// <param name="callSites">The call sites the method claims.</param>
    /// <param name="locationOf">Reads where a call site is.</param>
    /// <param name="declarationOpen">The method's declaration up to its name suffix, e.g. its modifiers, return type and name prefix.</param>
    /// <param name="suffix">The suffix naming the body the call sites reach.</param>
    internal static void AppendClaimingMethodOpen<T>(
        SourceWriter builder,
        List<T> callSites,
        Func<T, InterceptorLocation> locationOf,
        string declarationOpen,
        string suffix)
    {
        for (var i = 0; i < callSites.Count; i++)
        {
            AppendAttribute(builder, locationOf(callSites[i]));
        }

        _ = builder.Append(declarationOpen).Append(suffix).OpenParameterList();
    }

    /// <summary>Writes the attribute that binds a generated method to one call site.</summary>
    /// <param name="builder">The writer, at the class's member level.</param>
    /// <param name="location">The call site the compiler described.</param>
    internal static void AppendAttribute(SourceWriter builder, in InterceptorLocation location) =>
        _ = builder.Append('[')
            .Append(AttributeName)
            .Append('(')
            .Append(location.Version)
            .Append(", \"")
            .Append(location.Data)
            .Line("\")]");

    /// <summary>Appends the interception attribute's declaration to a generated file that applies it.</summary>
    /// <param name="source">The generated file.</param>
    /// <returns>The file, with a file-local attribute declaration when it intercepts a call site.</returns>
    /// <remarks>
    /// The attribute is not part of any framework, so the compilation that intercepts has to declare it. Each file
    /// that applies it declares its own file-local copy: a shared internal declaration would be one type visible to
    /// every assembly granted <c>InternalsVisibleTo</c>, colliding with that assembly's own.
    /// </remarks>
    internal static string AppendAttributeDeclaration(string source) =>
        source.IndexOf($"{AttributeName}(", StringComparison.Ordinal) < 0
            ? source
            : $"{source}\n{BuildAttributeDeclaration()}";

    /// <summary>Builds the file-local declaration of the interception attribute.</summary>
    /// <returns>The source declaring the attribute.</returns>
    /// <remarks>Written to the rules the oldest intercepting consumer parses: no file-scoped namespace.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildAttributeDeclaration() =>
        SourceWriter.Rent(DeclarationCapacity)
            .OpenNamespace("System.Runtime.CompilerServices")
            .Summary("Binds a generated method to the call site it replaces.")
            .Attribute($"{GeneratedTypeNames.AttributeUsage}({GeneratedTypeNames.AttributeTargets}.Method, AllowMultiple = true)")
            .OpenType($"file sealed class InterceptsLocationAttribute : {GeneratedTypeNames.Attribute}")
            .Summary("Initializes a new instance of the <see cref=\"InterceptsLocationAttribute\"/> class.")
            .DocLine("<param name=\"version\">The encoding of <paramref name=\"data\"/>.</param>")
            .DocLine("<param name=\"data\">The call site being replaced.</param>")
            .Line("public InterceptsLocationAttribute(int version, string data)")
            .OpenBlock()
            .Line("_ = version;")
            .Line("_ = data;")
            .CloseBlock()
            .CloseBlock()
            .CloseBlock()
            .ToStringAndReturn();

    /// <summary>Emits one interceptor per generated body, claiming every call site that reaches it.</summary>
    /// <param name="builder">The writer, at the class's member level.</param>
    /// <param name="group">The type group whose call sites are being claimed.</param>
    /// <param name="methodPrefix">The method name prefix the generated bodies carry.</param>
    /// <param name="suffixOf">Names the body a call site reaches.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot, handed to the writer.</param>
    /// <param name="appendParameterList">Writes the whole parameter list, closing it.</param>
    /// <remarks>
    /// The grouping and the attribute are the whole of what every API shares here, so they live in one place
    /// and each API supplies only the signature it is being called with. Call sites that reach one body are
    /// claimed by one method carrying an attribute each, which is what the attribute allowing repeats is for.
    /// <para>
    /// The feature snapshot travels as an argument rather than being closed over, so the writer each API hands
    /// in captures nothing and the compiler caches one delegate for the whole compilation.
    /// </para>
    /// </remarks>
    internal static void GenerateInterceptors(
        SourceWriter builder,
        ObservationCodeGenerator.TypeGroup group,
        string methodPrefix,
        Func<InvocationInfo, string> suffixOf,
        in LanguageFeatures features,
        ParameterListWriter appendParameterList)
    {
        foreach (var entry in GroupCallSitesByBody(group, suffixOf))
        {
            var first = entry.Value[0];

            foreach (var callSite in entry.Value)
            {
                AppendAttribute(builder, callSite.Interceptor);
            }

            _ = builder.Append($"internal static {GeneratedTypeNames.IObservable}<").Append(first.ReturnTypeFullName)
                .Append("> __Intercept_").Append(methodPrefix).Append('_').Append(entry.Key);
            AppendTypeParameterList(builder, first.GenericResult);
            _ = builder.OpenParameterList();

            appendParameterList(builder, first, in features);

            _ = builder.Indent()
                .Append("=> __").Append(methodPrefix).Append('_').Append(entry.Key)
                .Append("(objectToMonitor").Append(first.HasSelector ? ", selector" : string.Empty).Line(");")
                .Outdent()
                .BlankLine();
        }
    }

    /// <summary>Writes the type parameters of an interceptor that is generic over its call's selector result.</summary>
    /// <param name="builder">The writer, just after the interceptor's name.</param>
    /// <param name="genericResult">Where the result sits among the called method's type arguments.</param>
    /// <remarks>
    /// The compiler substitutes the call's type arguments into a generic interceptor before it compares the two
    /// signatures, so the interceptor takes as many type parameters as the called method. Only the result's is used:
    /// every other parameter is written with the type the call was closed over, which is the same after substitution.
    /// </remarks>
    internal static void AppendTypeParameterList(SourceWriter builder, in GenericResult genericResult)
    {
        if (!genericResult.IsGeneric)
        {
            return;
        }

        _ = builder.Append('<');
        for (var i = 0; i < genericResult.Arity; i++)
        {
            if (i > 0)
            {
                _ = builder.Append(", ");
            }

            _ = i == genericResult.Ordinal
                ? builder.Append(GenericResult.TypeParameterName)
                : builder.Append("__T").Append(i);
        }

        _ = builder.Append('>');
    }

    /// <summary>Writes the type parameter of an observation method that is generic over its selector result.</summary>
    /// <param name="builder">The writer, just after the method's name.</param>
    /// <param name="genericResult">Where the result sits, or a default value when every type is named.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendResultTypeParameter(SourceWriter builder, in GenericResult genericResult)
    {
        if (genericResult.IsGeneric)
        {
            _ = builder.Append('<').Append(GenericResult.TypeParameterName).Append('>');
        }
    }

    /// <summary>Drops the call sites only an interceptor can claim, when the build writes dispatch overloads instead.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="invocations">The call sites of one API.</param>
    /// <param name="genericResultOf">Reads where a call site's result sits when generated code cannot name it.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <returns>The call sites the build can claim.</returns>
    /// <remarks>
    /// A dispatch overload has to name its result type, so a call whose result generated code cannot name gets no
    /// overload. It reaches the runtime stub instead, and RXUIBIND015 fails the build at the call.
    /// </remarks>
    internal static ImmutableArray<T> ClaimableBy<T>(
        ImmutableArray<T> invocations,
        Func<T, GenericResult> genericResultOf,
        in LanguageFeatures features)
    {
        if (features.SupportsInterceptors || invocations.IsDefaultOrEmpty)
        {
            return invocations;
        }

        var builder = ImmutableArray.CreateBuilder<T>(invocations.Length);
        for (var i = 0; i < invocations.Length; i++)
        {
            if (!genericResultOf(invocations[i]).IsGeneric)
            {
                builder.Add(invocations[i]);
            }
        }

        return builder.Count == invocations.Length ? invocations : builder.ToImmutable();
    }

    /// <summary>Gathers call sites under the generated method each of them reaches.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="invocations">The call sites of one group.</param>
    /// <param name="locationOf">Reads where a call site is.</param>
    /// <param name="suffixOf">Names the generated method a call site reaches.</param>
    /// <returns>Each generated method, against every call site that resolves to it.</returns>
    /// <remarks>
    /// Every API claims its call sites the same way, so the gathering is written once over whatever model the
    /// API happens to carry. A call site the compiler declined to describe is left out: nothing can claim it,
    /// and it keeps whatever the call already resolved to.
    /// </remarks>
    internal static Dictionary<string, List<T>> GroupCallSites<T>(
        IReadOnlyList<T> invocations,
        Func<T, InterceptorLocation> locationOf,
        Func<T, string> suffixOf)
    {
        var claimed = new Dictionary<string, List<T>>(StringComparer.Ordinal);
        for (var i = 0; i < invocations.Count; i++)
        {
            var invocation = invocations[i];
            if (!locationOf(invocation).IsAvailable)
            {
                continue;
            }

            var suffix = suffixOf(invocation);
            if (!claimed.TryGetValue(suffix, out var callSites))
            {
                callSites = [];
                claimed[suffix] = callSites;
            }

            callSites.Add(invocation);
        }

        return claimed;
    }

    /// <summary>Gathers the call sites of a group under the body each of them reaches.</summary>
    /// <param name="group">The type group whose call sites are being gathered.</param>
    /// <param name="suffixOf">Names the body a call site reaches.</param>
    /// <returns>Each generated body, against every call site that resolves to it.</returns>
    /// <remarks>
    /// A call site the compiler declined to describe is left out: nothing can claim it, and it keeps whatever
    /// the call already resolved to.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<string, List<InvocationInfo>> GroupCallSitesByBody(
        ObservationCodeGenerator.TypeGroup group,
        Func<InvocationInfo, string> suffixOf) =>
        GroupCallSites(group.Invocations, static x => x.Interceptor, suffixOf);
}
