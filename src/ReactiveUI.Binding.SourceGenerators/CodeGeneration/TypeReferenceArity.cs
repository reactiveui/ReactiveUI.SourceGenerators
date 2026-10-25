// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Distinguishes generic type references when runtime flavours share a simple type name.</summary>
internal static class TypeReferenceArity
{
    /// <summary>The numeric base of metadata arity suffixes.</summary>
    private const int DecimalBase = 10;

    /// <summary>Reads a CLR metadata name's generic arity without allocating a substring.</summary>
    /// <param name="name">The metadata name.</param>
    /// <param name="marker">The backtick position, or minus one for a non-generic type.</param>
    /// <returns>The declared arity.</returns>
    internal static int FromMetadata(string name, int marker)
    {
        if (marker < 0)
        {
            return 0;
        }

        var arity = 0;
        for (var i = marker + 1; i < name.Length; i++)
        {
            arity = (arity * DecimalBase) + name[i] - '0';
        }

        return arity;
    }

    /// <summary>Counts the type arguments immediately following a generated type name.</summary>
    /// <param name="source">The generated C#.</param>
    /// <param name="start">The position after the type name.</param>
    /// <returns>The reference's arity, or zero for a non-generic reference.</returns>
    internal static int Read(string source, int start) =>
        start < source.Length && source[start] == '<' ? ReadArguments(source, start) : 0;

    /// <summary>Ignores commas within nested generics, tuple elements and array ranks.</summary>
    /// <param name="source">The generated C#.</param>
    /// <param name="start">The opening angle bracket.</param>
    /// <returns>The number of outer type arguments.</returns>
    private static int ReadArguments(string source, int start)
    {
        var depth = 0;
        var groups = 0;
        var count = 1;
        for (var i = start; i < source.Length; i++)
        {
            switch (source[i])
            {
                case '<':
                {
                    depth++;
                    break;
                }

                case '>':
                {
                    depth--;
                    if (depth == 0)
                    {
                        return count;
                    }

                    break;
                }

                case '(' or '[':
                {
                    groups++;
                    break;
                }

                case ')' or ']':
                {
                    groups--;
                    break;
                }

                case ',' when depth == 1 && groups == 0:
                {
                    count++;
                    break;
                }
            }
        }

        return 0;
    }
}
