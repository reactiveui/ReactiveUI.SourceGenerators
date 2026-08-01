// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="SyntaxToken"/> type.</summary>
internal static class SyntaxTokenExtensions
{
    /// <summary>Provides extension members for a syntax token.</summary>
    /// <param name="syntaxToken">The syntax token to extend.</param>
    extension(in SyntaxToken syntaxToken)
    {
        /// <summary>Deconstructs this token into its <see cref="SyntaxKind"/> value.</summary>
        /// <param name="syntaxKind">The resulting <see cref="SyntaxKind"/> value.</param>
        internal void Deconstruct(out SyntaxKind syntaxKind) => syntaxKind = syntaxToken.Kind();
    }
}
