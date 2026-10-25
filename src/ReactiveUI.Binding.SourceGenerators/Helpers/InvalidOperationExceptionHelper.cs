// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Throws <see cref="InvalidOperationException"/> for defensive guards that should never
/// be reached in practice. Excluded from code coverage because the branches are unreachable
/// under normal Roslyn semantic analysis.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class InvalidOperationExceptionHelper
{
    /// <summary>
    /// Returns the value if non-null; otherwise throws <see cref="InvalidOperationException"/>.
    /// Used to replace <c>if (x == null) return null;</c> defensive guards in extractors
    /// so the null-check branch lives inside an <c>[ExcludeFromCodeCoverage]</c> method.
    /// </summary>
    /// <typeparam name="T">The non-nullable value type or reference type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="context">A description of what was unexpectedly null.</param>
    /// <returns>The non-null <paramref name="value"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="value"/> is null.</exception>
    internal static T EnsureNotNull<T>(T? value, string context)
        where T : class =>
        value ?? throw new InvalidOperationException($"Unexpected null in source generator pipeline: {context}");

    /// <summary>Returns the value if non-null and non-empty; otherwise throws <see cref="InvalidOperationException"/>.</summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="context">A description of what was unexpectedly null or empty.</param>
    /// <returns>The non-null, non-empty <paramref name="value"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="value"/> is null or empty.</exception>
    internal static string EnsureNotNullOrEmpty(string? value, string context) =>
        string.IsNullOrEmpty(value)
            ? throw new InvalidOperationException($"Unexpected null or empty in source generator pipeline: {context}")
            : value!;

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the argument count is less than the minimum.
    /// Used to replace <c>if (!HasMinimumArguments(...)) return null;</c> defensive guards in extractors
    /// where the syntax predicate guarantees sufficient arguments.
    /// </summary>
    /// <param name="count">The actual argument count.</param>
    /// <param name="minimum">The minimum required count.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="count"/> is less than <paramref name="minimum"/>.</exception>
    internal static void EnsureMinimumArguments(int count, int minimum)
    {
        if (count >= minimum)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Expected at least {minimum} arguments but found {count} in source generator pipeline");
    }
}
