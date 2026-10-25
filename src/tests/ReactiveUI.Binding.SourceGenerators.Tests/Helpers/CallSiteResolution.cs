// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Resolves what a call in the consumer source actually binds to after the generator has run. Which method wins
/// is the whole point of the generated overloads, and it is invisible to text assertions on the generated files:
/// an overload can be emitted perfectly and still lose to the runtime stub.
/// </summary>
internal static class CallSiteResolution
{
    /// <summary>Finds the method a named call in the consumer's own source binds to.</summary>
    /// <param name="result">The generator run to inspect.</param>
    /// <param name="methodName">The name of the invoked method.</param>
    /// <returns>The resolved method, or <see langword="null"/> when the call does not resolve.</returns>
    internal static async Task<IMethodSymbol?> ResolveAsync(GeneratorTestResult result, string methodName)
    {
        // The consumer's own tree is the only one the generator did not add, so it is the one that came in with
        // the compilation. Generated trees all carry a hint-name path; the input tree's path is empty.
        var tree = result.OutputCompilation.SyntaxTrees.First(static t => t.FilePath.Length == 0);
        var root = await tree.GetRootAsync();

        var invocation = root
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(i => i.Expression is MemberAccessExpressionSyntax access
                && string.Equals(access.Name.Identifier.ValueText, methodName, StringComparison.Ordinal));

        return result.OutputCompilation.GetSemanticModel(tree).GetSymbolInfo(invocation).Symbol as IMethodSymbol;
    }
}
