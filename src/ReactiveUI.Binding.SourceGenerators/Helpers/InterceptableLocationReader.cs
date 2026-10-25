// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Reads the call-site description an interceptor has to name.</summary>
/// <remarks>
/// The only place either build asks the compiler about interception. The 4.13 build has the API and answers;
/// the baseline build has no such API to call and answers that it described nothing, which every caller
/// already handles because a call site can be undescribable on a new compiler too.
/// </remarks>
internal static class InterceptableLocationReader
{
    /// <summary>Gets a value indicating whether this build can describe a call site at all.</summary>
    /// <remarks>
    /// Fixed when the generator is compiled, not read from the consumer: which of the two builds is loaded is
    /// decided by the compiler's own version, through the analyzer slot the package was resolved from.
    /// </remarks>
    internal static bool IsSupported =>
#if ROSLYN_4_13
        true;
#else
        false;
#endif

    /// <summary>Determines whether the consumer has opted the generated namespace into interception.</summary>
    /// <param name="parseOptions">The consumer's parse options, which carry the opt-in the build set.</param>
    /// <returns><see langword="true"/> when an interceptor emitted here would be honoured.</returns>
    /// <remarks>
    /// Interception is refused outright for a namespace the project did not list, so emitting one without the
    /// opt-in turns every call site into a build error. The package's own props lists the generated namespace,
    /// which is why this is normally true; a consumer who clears the property gets the dispatch overloads back.
    /// A listed namespace covers the ones nested under it, so a prefix counts as a match.
    /// </remarks>
    internal static bool IsOptedIn(ParseOptions parseOptions)
    {
        if (!parseOptions.Features.TryGetValue(Constants.InterceptorsNamespacesFeature, out var namespaces)
            || string.IsNullOrEmpty(namespaces))
        {
            return false;
        }

        foreach (var candidate in namespaces.Split(';'))
        {
            var trimmed = candidate.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (string.Equals(trimmed, Constants.InterceptorNamespace, StringComparison.Ordinal)
                || (Constants.InterceptorNamespace.Length > trimmed.Length
                    && Constants.InterceptorNamespace[trimmed.Length] == '.'
                    && Constants.InterceptorNamespace.StartsWith(trimmed, StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether an interceptor emitted for this compilation would be honoured.</summary>
    /// <param name="parseOptions">The consumer's parse options, which carry the opt-in the build set.</param>
    /// <returns><see langword="true"/> when this build can describe a call site and the project listed the namespace.</returns>
    /// <remarks>
    /// The one question both the generator and the analyzer ask before deciding whether the dispatch overloads
    /// still matter, so the two answer it the same way. Which build is loaded settles the first half outright,
    /// which is why the baseline never reads the options at all.
    /// </remarks>
    internal static bool IsInterceptionEnabled(ParseOptions parseOptions)
    {
#if ROSLYN_4_13
        return IsOptedIn(parseOptions);
#else

        // The baseline compiler describes no call site, so there is no interceptor for a listing to honour.
        _ = parseOptions;
        return false;
#endif
    }

    /// <summary>Describes a call site, when the host compiler can.</summary>
    /// <param name="semanticModel">The model the invocation was bound in.</param>
    /// <param name="invocation">The call site.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The description, or a location reporting that none was produced.</returns>
    internal static InterceptorLocation Read(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
#if ROSLYN_4_13
        var location = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetInterceptableLocation(
            semanticModel,
            invocation,
            cancellationToken);

        return location is null ? default : new InterceptorLocation(location.Version, location.Data);
#else

        // The baseline compiler cannot describe a call site, so nothing here is interceptable. The arguments
        // are read so the two builds keep one signature rather than diverging on what they accept.
        _ = semanticModel;
        _ = invocation;
        _ = cancellationToken;
        return default;
#endif
    }
}
