// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Reads an API's generated file back, so its members can move into the caller's partial class.</summary>
/// <remarks>
/// Every API writes its file through the same header and footer, one member per line and four spaces per level. The
/// class's body is therefore the lines between its opening brace and the closing brace at its own indentation. While
/// reading, each interception attribute is removed and remembered against the interceptor that follows it, and that
/// interceptor loses the <c>this</c> on its receiver, because an extension method cannot sit in a nested class.
/// </remarks>
internal static class HostedSourceReader
{
    /// <summary>One level of indentation.</summary>
    private const string Level = "    ";

    /// <summary>Reads an API's file.</summary>
    /// <param name="source">The file.</param>
    /// <param name="className">The name of the class the API writes its members in.</param>
    /// <param name="read">The members, claims and trailing text.</param>
    /// <returns><see langword="true"/> when the file has the shape every API writes.</returns>
    internal static bool TryRead(string source, string className, out HostedSource read)
    {
        read = default;
        var lines = source.Split('\n');
        if (!TryFindClassBody(lines, className, out var open, out var close, out var namespaceClose))
        {
            return false;
        }

        var memberIndent = $"{Indentation(lines[open])}{Level}";
        var claims = new ClaimReader();
        var members = new List<string>(close - open);
        for (var i = open + 1; i < close; i++)
        {
            var line = lines[i].StartsWith(memberIndent, StringComparison.Ordinal)
                ? lines[i].Substring(memberIndent.Length)
                : lines[i].TrimStart();
            if (claims.TryKeep(line, out var kept))
            {
                members.Add(kept);
            }
        }

        read = new(members, claims.Claims, Trailing(lines, namespaceClose));
        return true;
    }

    /// <summary>Finds the generated class's braces and the namespace's closing brace.</summary>
    /// <param name="lines">The file's lines.</param>
    /// <param name="className">The name of the class the API writes its members in.</param>
    /// <param name="open">The line of the class's opening brace.</param>
    /// <param name="close">The line of the class's closing brace.</param>
    /// <param name="namespaceClose">The line of the namespace's closing brace.</param>
    /// <returns><see langword="true"/> when all three were found.</returns>
    private static bool TryFindClassBody(string[] lines, string className, out int open, out int close, out int namespaceClose)
    {
        var header = $"internal static partial class {className}";
        open = -1;
        close = -1;
        namespaceClose = -1;
        for (var i = 0; i + 1 < lines.Length && open < 0; i++)
        {
            if (lines[i].Trim() == header && lines[i + 1].Trim() == "{")
            {
                open = i + 1;
            }
        }

        if (open < 0)
        {
            return false;
        }

        close = Array.IndexOf(lines, $"{Indentation(lines[open])}}}", open + 1);
        namespaceClose = close < 0 ? -1 : Array.IndexOf(lines, "}", close + 1);
        return namespaceClose >= 0;
    }

    /// <summary>Reads the whitespace a line starts with.</summary>
    /// <param name="line">The line.</param>
    /// <returns>The leading whitespace.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Indentation(string line) => line.Substring(0, line.Length - line.TrimStart().Length);

    /// <summary>Joins whatever the file declares after the generated namespace.</summary>
    /// <param name="lines">The file's lines.</param>
    /// <param name="namespaceClose">The line of the namespace's closing brace.</param>
    /// <returns>The trailing text, or an empty string.</returns>
    private static string Trailing(string[] lines, int namespaceClose) =>
        namespaceClose + 1 < lines.Length
            ? string.Join("\n", lines, namespaceClose + 1, lines.Length - namespaceClose - 1).Trim()
            : string.Empty;

    /// <summary>Removes interception attributes and remembers which interceptor each one claimed a call site for.</summary>
    private sealed class ClaimReader
    {
        /// <summary>The start of an interception attribute, as the API emitters write it.</summary>
        private const string AttributeStart = "[global::System.Runtime.CompilerServices.InterceptsLocation(";

        /// <summary>The prefix every interceptor an API writes carries.</summary>
        private const string InterceptorPrefix = "__Intercept_";

        /// <summary>The modifier that makes a method an extension method.</summary>
        private const string ReceiverModifier = "this ";

        /// <summary>The receiver modifier as it reads when the parameter list starts on the method's line.</summary>
        private const string InlineReceiverModifier = "(this ";

        /// <summary>The data of the attributes read since the last interceptor.</summary>
        private readonly List<string> _pending = [];

        /// <summary>Whether the last interceptor's receiver still carries <c>this</c>.</summary>
        private bool _stripping;

        /// <summary>Gets the interceptor each call site's data names.</summary>
        internal Dictionary<string, string> Claims { get; } = [with(StringComparer.Ordinal)];

        /// <summary>Reads one member line.</summary>
        /// <param name="line">The line, without the class's indentation.</param>
        /// <param name="kept">The line to keep, rewritten when it is part of an interceptor's declaration.</param>
        /// <returns><see langword="false"/> when the line is an interception attribute and is dropped.</returns>
        internal bool TryKeep(string line, out string kept)
        {
            kept = line;
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith(AttributeStart, StringComparison.Ordinal))
            {
                _pending.Add(ReadData(trimmed));
                return false;
            }

            var nameStart = _pending.Count > 0 ? trimmed.IndexOf(InterceptorPrefix, StringComparison.Ordinal) : -1;
            if (nameStart >= 0)
            {
                var name = ReadName(trimmed, nameStart);
                foreach (var data in _pending)
                {
                    Claims[data] = name;
                }

                _pending.Clear();
                _stripping = true;
            }

            if (_stripping)
            {
                kept = StripReceiver(line);
            }

            return true;
        }

        /// <summary>Reads the data an interception attribute names.</summary>
        /// <param name="attribute">The attribute line.</param>
        /// <returns>The data, the text between its quotes.</returns>
        private static string ReadData(string attribute)
        {
            var start = attribute.IndexOf('"') + 1;
            return attribute.Substring(start, attribute.LastIndexOf('"') - start);
        }

        /// <summary>Reads an interceptor's name from its declaration line.</summary>
        /// <param name="declaration">The declaration line.</param>
        /// <param name="start">Where the name starts.</param>
        /// <returns>The name.</returns>
        private static string ReadName(string declaration, int start)
        {
            var end = start;
            while (end < declaration.Length && (char.IsLetterOrDigit(declaration[end]) || declaration[end] == '_'))
            {
                end++;
            }

            return declaration.Substring(start, end - start);
        }

        /// <summary>Removes the <c>this</c> from the interceptor's receiver, once.</summary>
        /// <param name="line">A line of the interceptor's declaration.</param>
        /// <returns>The line, without the modifier.</returns>
        private string StripReceiver(string line)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith(ReceiverModifier, StringComparison.Ordinal))
            {
                _stripping = false;
                return line.Remove(line.Length - trimmed.Length, ReceiverModifier.Length);
            }

            var inline = line.IndexOf(InlineReceiverModifier, StringComparison.Ordinal);
            if (inline < 0)
            {
                return line;
            }

            _stripping = false;
            return line.Remove(inline + 1, ReceiverModifier.Length);
        }
    }
}
