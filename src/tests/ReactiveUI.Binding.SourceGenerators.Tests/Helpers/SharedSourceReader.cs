// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Reads shared scenario source files from the test output directory.
/// These files are copied via MSBuild None items with CopyToOutputDirectory.
/// </summary>
internal static partial class SharedSourceReader
{
    /// <summary>
    /// Reads all source files from a shared scenario directory and merges them
    /// into a single valid C# compilation unit with deduplicated usings
    /// and a single namespace block.
    /// </summary>
    /// <param name="scenarioPath">The path relative to the SharedScenarios root (e.g. "WhenChanged/SinglePropertyINPC").</param>
    /// <returns>A single merged source text containing all types from the scenario.</returns>
    internal static string ReadScenario(string scenarioPath)
    {
        var dir = Path.Combine(FindRoot(), scenarioPath);
        var files = Directory.GetFiles(dir, "*.cs");
        Array.Sort(files);

        var copyrightLines = new List<string>();
        var usingDirectives = new LinkedHashSet<string>();
        string? namespaceName = null;
        var typeBlocks = new List<string>();

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            ParseFile(lines, copyrightLines, usingDirectives, ref namespaceName, typeBlocks);
        }

        var sb = new StringBuilder();

        // Copyright header
        foreach (var line in copyrightLines)
        {
            _ = sb.AppendLine(line);
        }

        _ = sb.AppendLine();

        // Deduplicated using directives (preserve blank line separators between groups)
        var usingList = usingDirectives.ToList();
        string? prevPrefix = null;
        foreach (var u in usingList)
        {
            var currentPrefix = ExtractUsingPrefix(u);
            if (prevPrefix is not null && currentPrefix != prevPrefix)
            {
                _ = sb.AppendLine();
            }

            _ = sb.AppendLine(u);
            prevPrefix = currentPrefix;
        }

        _ = sb.AppendLine();

        // Single namespace block with all type blocks
        _ = sb.Append("namespace ").Append(namespaceName).AppendLine();
        _ = sb.AppendLine("{");

        for (var i = 0; i < typeBlocks.Count; i++)
        {
            if (i > 0)
            {
                _ = sb.AppendLine();
            }

            _ = sb.AppendLine(typeBlocks[i]);
        }

        _ = sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>Lists every shared scenario, so a test can sweep all of them without a hand-kept list.</summary>
    /// <returns>The scenario paths, relative to the shared scenario root, in a stable order.</returns>
    internal static IReadOnlyList<string> EnumerateScenarioPaths()
    {
        var root = FindRoot();
        var scenarios = new List<string>();

        foreach (var directory in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            if (Directory.GetFiles(directory, "*.cs").Length > 0)
            {
                scenarios.Add(Path.GetRelativePath(root, directory).Replace('\\', '/'));
            }
        }

        scenarios.Sort(StringComparer.Ordinal);
        return scenarios;
    }

    /// <summary>Locates the shared scenario root in the test output directory.</summary>
    /// <returns>The absolute path to the shared scenario root.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FindRoot() =>
        Path.Combine(Path.GetDirectoryName(typeof(SharedSourceReader).Assembly.Location)!, "SharedScenarios");

    /// <summary>Parses a source file's lines to extract copyright headers, usings, and type blocks.</summary>
    /// <param name="lines">The lines of the file to parse.</param>
    /// <param name="copyrightLines">The list to which copyright lines will be added.</param>
    /// <param name="usingDirectives">The set to which using directives will be added.</param>
    /// <param name="namespaceName">A reference to the shared namespace name.</param>
    /// <param name="typeBlocks">The list to which extracted type blocks will be added.</param>
    private static void ParseFile(
        string[] lines,
        List<string> copyrightLines,
        LinkedHashSet<string> usingDirectives,
        ref string? namespaceName,
        List<string> typeBlocks)
    {
        var i = 0;
        CollectCopyright(lines, ref i, copyrightLines);
        CollectUsings(lines, ref i, usingDirectives);
        var isFileScoped = ReadNamespace(lines, ref i, ref namespaceName);

        string? body;
        if (isFileScoped)
        {
            // File-scoped namespace ("namespace X;") has no braces: every remaining line is a top-level
            // member of the namespace, so take the rest of the file verbatim.
            body = ExtractRemainingBody(lines, ref i);
        }
        else
        {
            SkipToBodyStart(lines, ref i);
            body = ExtractTypeBlock(lines, ref i);
        }

        if (body is null)
        {
            return;
        }

        typeBlocks.Add(body);
    }

    /// <summary>Collects up to the first three copyright comment lines from the file header.</summary>
    /// <param name="lines">The file lines.</param>
    /// <param name="i">The current line index, advanced past the header.</param>
    /// <param name="copyrightLines">The list to which copyright lines are added.</param>
    private static void CollectCopyright(string[] lines, ref int i, List<string> copyrightLines)
    {
        while (i < lines.Length
               && (lines[i].StartsWith("//", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(lines[i])))
        {
            if (copyrightLines.Count < 3 && lines[i].StartsWith("//", StringComparison.Ordinal))
            {
                copyrightLines.Add(lines[i]);
            }

            i++;
        }
    }

    /// <summary>Collects using directives, skipping blank lines, until a non-using, non-blank line is reached.</summary>
    /// <param name="lines">The file lines.</param>
    /// <param name="i">The current line index, advanced past the using block.</param>
    /// <param name="usingDirectives">The set to which using directives are added.</param>
    private static void CollectUsings(string[] lines, ref int i, LinkedHashSet<string> usingDirectives)
    {
        while (i < lines.Length)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith("using ", StringComparison.Ordinal))
            {
                usingDirectives.Add(trimmed);
                i++;
            }
            else if (string.IsNullOrWhiteSpace(trimmed))
            {
                i++;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>Reads the namespace declaration if present, capturing the name on first encounter.</summary>
    /// <param name="lines">The file lines.</param>
    /// <param name="i">The current line index, advanced past the namespace line when matched.</param>
    /// <param name="namespaceName">A reference to the shared namespace name.</param>
    /// <returns><see langword="true"/> if the namespace is file-scoped (<c>namespace X;</c>); otherwise <see langword="false"/>.</returns>
    private static bool ReadNamespace(string[] lines, ref int i, ref string? namespaceName)
    {
        if (i >= lines.Length)
        {
            return false;
        }

        var namespaceMatch = NamespaceRegex().Match(lines[i]);
        if (!namespaceMatch.Success)
        {
            return false;
        }

        namespaceName ??= namespaceMatch.Groups[1].Value;
        var isFileScoped = lines[i].TrimEnd().EndsWith(';');
        i++;
        return isFileScoped;
    }

    /// <summary>
    /// Returns all remaining lines (the top-level members of a file-scoped namespace), trimming leading
    /// and trailing blank lines.
    /// </summary>
    /// <param name="lines">The file lines.</param>
    /// <param name="i">The current line index, advanced to the end of the file.</param>
    /// <returns>The trimmed body text, or <c>null</c> if there is no content.</returns>
    private static string? ExtractRemainingBody(string[] lines, ref int i)
    {
        var bodyStart = i;
        var bodyEnd = lines.Length - 1;
        i = lines.Length;

        while (bodyStart <= bodyEnd && string.IsNullOrWhiteSpace(lines[bodyStart]))
        {
            bodyStart++;
        }

        while (bodyEnd >= bodyStart && string.IsNullOrWhiteSpace(lines[bodyEnd]))
        {
            bodyEnd--;
        }

        return bodyStart > bodyEnd ? null : string.Join(Environment.NewLine, lines[bodyStart..(bodyEnd + 1)]);
    }

    /// <summary>Advances past the namespace's opening brace to the first body line.</summary>
    /// <param name="lines">The file lines.</param>
    /// <param name="i">The current line index, advanced past the opening brace.</param>
    private static void SkipToBodyStart(string[] lines, ref int i)
    {
        while (i < lines.Length && lines[i].Trim() != "{")
        {
            i++;
        }

        i++; // move past the opening brace
    }

    /// <summary>
    /// Collects the namespace body (all type declarations) until the final closing brace,
    /// trimming leading and trailing blank lines.
    /// </summary>
    /// <param name="lines">The file lines.</param>
    /// <param name="i">The current line index, advanced through the body.</param>
    /// <returns>The trimmed body text, or <c>null</c> if the body is empty.</returns>
    private static string? ExtractTypeBlock(string[] lines, ref int i)
    {
        var bodyStart = i;
        var braceDepth = 1;
        var bodyEnd = bodyStart;
        while (i < lines.Length && braceDepth > 0)
        {
            braceDepth = UpdateBraceDepth(lines[i], braceDepth);
            if (braceDepth <= 0)
            {
                continue;
            }

            bodyEnd = i;
            i++;
        }

        while (bodyStart <= bodyEnd && string.IsNullOrWhiteSpace(lines[bodyStart]))
        {
            bodyStart++;
        }

        while (bodyEnd >= bodyStart && string.IsNullOrWhiteSpace(lines[bodyEnd]))
        {
            bodyEnd--;
        }

        return bodyStart > bodyEnd ? null : string.Join(Environment.NewLine, lines[bodyStart..(bodyEnd + 1)]);
    }

    /// <summary>Updates the running brace depth for a single line, stopping at the matching close brace.</summary>
    /// <param name="line">The line to scan.</param>
    /// <param name="braceDepth">The current brace depth.</param>
    /// <returns>The updated brace depth (zero once the matching close brace is found).</returns>
    private static int UpdateBraceDepth(string line, int braceDepth)
    {
        foreach (var ch in line)
        {
            if (ch == '{')
            {
                braceDepth++;
            }
            else if (ch == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    break;
                }
            }
        }

        return braceDepth;
    }

    /// <summary>Extracts the prefix from a using directive for grouping.</summary>
    /// <param name="usingDirective">The using directive.</param>
    /// <returns>The prefix string.</returns>
    private static string ExtractUsingPrefix(string usingDirective)
    {
        // Group usings by top-level namespace (e.g., "System" vs "ReactiveUI")
        var match = UsingPrefixRegex().Match(usingDirective);
        return match.Success ? match.Groups[1].Value : usingDirective;
    }

    /// <summary>
    /// Generates a regex for matching namespace declarations.
    /// </summary>
    /// <returns>The generated regex.</returns>
    [GeneratedRegex(@"^\s*namespace\s+([\w.]+)")]
    private static partial Regex NamespaceRegex();

    /// <summary>
    /// Generates a regex for matching using directives and extracting the top-level namespace.
    /// </summary>
    /// <returns>The generated regex.</returns>
    [GeneratedRegex(@"^using\s+(?:static\s+)?(\w+)")]
    private static partial Regex UsingPrefixRegex();

    /// <summary>A set that preserves insertion order while preventing duplicates.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    private sealed class LinkedHashSet<T> : IEnumerable<T>
        where T : notnull
    {
        /// <summary>The hash set used for O(1) duplicate checks.</summary>
        private readonly HashSet<T> _set = [];

        /// <summary>The list used for preserving insertion order.</summary>
        private readonly List<T> _list = [];

        /// <summary>Adds an item to the set if it's not already present.</summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            if (!_set.Add(item))
            {
                return;
            }

            _list.Add(item);
        }

        /// <summary>Converts the set to a list.</summary>
        /// <returns>A new list containing the set elements in insertion order.</returns>
        public List<T> ToList() => [.. _list];

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
