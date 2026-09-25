// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Syntax-only checks that let a transform skip semantic work it would find empty.</summary>
internal static class SyntaxNodeExtensions
{
    /// <summary>Provides extension members for a syntax node.</summary>
    /// <param name="syntaxNode">The syntax node to extend.</param>
    extension(SyntaxNode syntaxNode)
    {
        /// <summary>Determines whether a declaration carries a documentation comment, from its leading trivia alone.</summary>
        /// <returns>Whether the declaration has a <c>///</c> or <c>/** */</c> documentation comment.</returns>
        /// <remarks>
        /// Asking the symbol for its documentation parses and formats the comment's XML, allocating even when there is
        /// none; reading the trivia first skips that for the common undocumented member.
        /// </remarks>
        internal bool HasDocumentationComment()
        {
            foreach (var trivia in syntaxNode.GetLeadingTrivia())
            {
                if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
