// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Checks the layout of every generated file the snapshot tests store. The writer indents from structure, so a
/// brace written at the wrong depth or a line indented by hand is a defect in an emitter that no single snapshot
/// review would reliably catch.
/// </summary>
public class GeneratedLayoutTests
{
    /// <summary>The spaces one indentation level adds.</summary>
    private const int IndentWidth = 4;

    /// <summary>The first line of a snapshot, which names the generated file rather than being part of it.</summary>
    private const string HintNamePrefix = "//HintName:";

    /// <summary>
    /// Every closing brace lines up with the brace that opened its block, every line inside a block is indented
    /// deeper than that block's braces in whole levels, and no line carries trailing whitespace.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <remarks>
    /// A block passed as an argument - a lambda body - sits at its argument's continuation level rather than at
    /// the statement's, so braces are checked against each other rather than against a fixed depth.
    /// </remarks>
    [Test]
    public async Task EverySnapshot_IsIndentedByItsStructure()
    {
        var problems = new List<string>();
        var checkedFiles = 0;

        foreach (var file in GeneratorSnapshot.VerifiedSnapshots)
        {
            checkedFiles++;
            CheckLayout(Path.GetFileName(file), await File.ReadAllLinesAsync(file), problems);
        }

        await Assert.That(checkedFiles).IsGreaterThan(0);
        await Assert.That(problems).IsEmpty();
    }

    /// <summary>Records every layout problem in one generated file.</summary>
    /// <param name="name">The snapshot's file name.</param>
    /// <param name="lines">The snapshot's lines.</param>
    /// <param name="problems">Where problems are recorded.</param>
    private static void CheckLayout(string name, string[] lines, List<string> problems)
    {
        var openBraces = new Stack<int>();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Length == 0 || line.StartsWith(HintNamePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (DescribeProblem(line, openBraces) is { } problem)
            {
                problems.Add($"{name}:{i + 1} {problem}");
            }
        }

        if (openBraces.Count != 0)
        {
            problems.Add($"{name}: {openBraces.Count} block(s) left open");
        }
    }

    /// <summary>Checks one non-empty line against the blocks it sits in, opening or closing one where it does.</summary>
    /// <param name="line">The line.</param>
    /// <param name="openBraces">The indentation of each open block's brace, innermost on top.</param>
    /// <returns>The problem with the line, or null when it is laid out correctly.</returns>
    private static string? DescribeProblem(string line, Stack<int> openBraces)
    {
        var content = line.TrimStart(' ');
        var indent = line.Length - content.Length;

        if (char.IsWhiteSpace(line[^1]))
        {
            return "trailing whitespace";
        }

        if (indent % IndentWidth != 0)
        {
            return $"indented {indent}, not a whole number of levels: {content}";
        }

        if (content[0] == '}')
        {
            if (openBraces.Count == 0)
            {
                return $"closes a block that was never opened: {content}";
            }

            var opened = openBraces.Pop();
            return indent == opened ? null : $"closing brace at {indent} does not line up with its opening brace at {opened}: {content}";
        }

        var blockIndent = openBraces.Count == 0 ? -IndentWidth : openBraces.Peek();
        if (indent <= blockIndent)
        {
            return $"indented {indent}, no deeper than its block's brace at {blockIndent}: {content}";
        }

        if (content == "{")
        {
            openBraces.Push(indent);
        }

        return null;
    }
}
