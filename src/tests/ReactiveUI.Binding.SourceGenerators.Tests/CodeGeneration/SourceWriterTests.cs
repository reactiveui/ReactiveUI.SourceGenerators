// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Covers the writer every generated file is built through. It owns the layout of the generated code, so a
/// defect here shows up in every file at once: a line indented at the wrong depth, whitespace left on a blank
/// line, or one file's text leaking into the next through the reused buffer.
/// </summary>
public class SourceWriterTests
{
    /// <summary>Enough lines to force the buffer to grow several times over.</summary>
    private const int GrowthLineCount = 2_000;

    /// <summary>Where the copied part of a string starts.</summary>
    private const int RangeStart = 2;

    /// <summary>How many characters of a string are copied.</summary>
    private const int RangeCount = 3;

    /// <summary>A depth past the writer's precomputed indentation table.</summary>
    private const int DeepLevel = 20;

    /// <summary>An empty writer renders as the empty string, at level zero.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Empty_RendersAsEmpty()
    {
        var writer = new SourceWriter();

        await Assert.That(writer.Length).IsEqualTo(0);
        await Assert.That(writer.Level).IsEqualTo(0);
        await Assert.That(writer.ToString()).IsEqualTo(string.Empty);
    }

    /// <summary>A line is indented by four spaces per level, once, when its first character is written.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Append_IndentsOnlyTheStartOfALine()
    {
        var writer = new SourceWriter().Indent().Indent();

        var rendered = writer.Append("var x").Append(" = ").Append('1').Line(";").ToString();

        await Assert.That(rendered).IsEqualTo("        var x = 1;\n");
    }

    /// <summary>A blank line carries no indentation, whatever the level.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BlankLine_CarriesNoWhitespace()
    {
        var writer = new SourceWriter().Indent().Indent().Indent();

        var rendered = writer.Line("a").BlankLine().Line("b").ToString();

        await Assert.That(rendered).IsEqualTo("            a\n\n            b\n");
    }

    /// <summary>A level change mid-line applies from the next line, never to the line already started.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LevelChange_MidLine_AppliesFromTheNextLine()
    {
        var writer = new SourceWriter().Indent();

        var rendered = writer.Append("call(").Indent().EndLine().Line("argument);").Outdent().Line("done;").ToString();

        await Assert.That(rendered).IsEqualTo("    call(\n        argument);\n    done;\n");
    }

    /// <summary>Moving shallower than the outermost level is a bug in the emitter, so it throws rather than writing past it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Outdent_AtTheOutermostLevel_Throws()
    {
        var writer = new SourceWriter();

        await Assert.That(() => writer.Outdent()).Throws<InvalidOperationException>();
    }

    /// <summary>A block opens a level for its body and closes back to the level it opened at.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Blocks_NestAndCloseAtTheirOwnLevel()
    {
        var writer = new SourceWriter();

        var rendered = writer.Line("outer").OpenBlock().Line("inner").OpenBlock().Line("body;").CloseBlock().CloseBlock(");").ToString();

        await Assert.That(rendered).IsEqualTo("outer\n{\n    inner\n    {\n        body;\n    }\n});\n");
        await Assert.That(writer.Level).IsEqualTo(0);
    }

    /// <summary>An inline close leaves the line open, for a caller to finish the statement the block sat in.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CloseBlockInline_LeavesTheLineOpen()
    {
        var writer = new SourceWriter();

        var rendered = writer.Line("x =>").OpenBlock().Line("y;").CloseBlockInline().Append(", \"x\"").Line(");").ToString();

        await Assert.That(rendered).IsEqualTo("x =>\n{\n    y;\n}, \"x\");\n");
    }

    /// <summary>Levels past the precomputed indentation table indent just as deep.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepLevels_IndentFourSpacesPerLevel()
    {
        var writer = new SourceWriter();
        for (var i = 0; i < DeepLevel; i++)
        {
            _ = writer.Indent();
        }

        var rendered = writer.Line("x").ToString();

        await Assert.That(rendered).IsEqualTo($"{new string(' ', DeepLevel * SourceWriter.IndentWidth)}x\n");
    }

    /// <summary>A null or empty append writes nothing, not even the indentation of an otherwise empty line.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendingNothing_WritesNothing()
    {
        var writer = new SourceWriter().Indent();

        var rendered = writer.Append((string?)null).Append(string.Empty).Append("abc", 1, 0).EndLine().ToString();

        await Assert.That(rendered).IsEqualTo("\n");
    }

    /// <summary>Part of a string is copied exactly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendRange_CopiesThePart()
    {
        var rendered = new SourceWriter().Append("abcdef", RangeStart, RangeCount).ToString();

        await Assert.That(rendered).IsEqualTo("cde");
    }

    /// <summary>Integers render exactly as the framework renders them, including the extremes.</summary>
    /// <param name="value">The value to render.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(0)]
    [Arguments(7)]
    [Arguments(-7)]
    [Arguments(1_234_567)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    public async Task AppendInteger_MatchesTheInvariantRendering(int value)
    {
        var rendered = new SourceWriter().Append(value).ToString();

        await Assert.That(rendered).IsEqualTo(value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>A boolean renders as the C# literal, not as the framework's capitalised name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendLiteral_WritesTheCSharpLiteral()
    {
        var rendered = new SourceWriter().AppendLiteral(true).Append(' ').AppendLiteral(false).ToString();

        await Assert.That(rendered).IsEqualTo("true false");
    }

    /// <summary>Backslashes and quotes are escaped for a regular string literal; everything else is copied.</summary>
    /// <param name="raw">The raw text.</param>
    /// <param name="escaped">The escaped rendering.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Name", "x => x.Name")]
    [Arguments("a\"b", "a\\\"b")]
    [Arguments("C:\\src\\a.cs", "C:\\\\src\\\\a.cs")]
    [Arguments("\"\\", "\\\"\\\\")]
    [Arguments("", "")]
    public async Task AppendEscaped_EscapesBackslashesAndQuotes(string raw, string escaped)
    {
        var rendered = new SourceWriter().AppendEscaped(raw).ToString();

        await Assert.That(rendered).IsEqualTo(escaped);
        await Assert.That(rendered).IsEqualTo(CodeGeneratorHelpers.EscapeString(raw));
    }

    /// <summary>A quoted literal is the escaped text between quotes, indented like any other line start.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendQuoted_WritesARegularStringLiteral()
    {
        var rendered = new SourceWriter().Indent().AppendQuoted("say \"hi\"").ToString();

        await Assert.That(rendered).IsEqualTo("    \"say \\\"hi\\\"\"");
    }

    /// <summary>A block of lines keeps its own relative indentation on top of the writer's level.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Lines_IndentEachLineFromTheWritersLevel()
    {
        var writer = new SourceWriter().Indent();

        var rendered = writer.Lines("class C\n{\n    int x;\n\n}").ToString();

        await Assert.That(rendered).IsEqualTo("    class C\n    {\n        int x;\n\n    }\n");
    }

    /// <summary>A block written with Windows line endings renders the same as one written with Unix ones.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Lines_WithCarriageReturns_RenderLineFeedsOnly()
    {
        var unix = new SourceWriter().Lines("a\nb\n").ToString();
        var windows = new SourceWriter().Lines("a\r\nb\r\n").ToString();

        await Assert.That(windows).IsEqualTo(unix);
        await Assert.That(windows).IsEqualTo("a\nb\n");
    }

    /// <summary>Output larger than the starting buffer grows it without losing or reordering anything.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LargeOutput_GrowsTheBufferIntact()
    {
        var writer = new SourceWriter().Indent();
        for (var i = 0; i < GrowthLineCount; i++)
        {
            _ = writer.Append("line ").Append(i).EndLine();
        }

        var lines = writer.ToString().Split('\n');

        await Assert.That(lines.Length).IsEqualTo(GrowthLineCount + 1);
        await Assert.That(lines[0]).IsEqualTo("    line 0");
        await Assert.That(lines[GrowthLineCount - 1]).IsEqualTo($"    line {GrowthLineCount - 1}");
    }

    /// <summary>A writer rented after another was handed back starts empty and at level zero, whatever the last one left.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rent_AfterReturn_StartsEmptyAtLevelZero()
    {
        var first = SourceWriter.Rent();
        _ = first.Indent().Append("left over");
        var text = first.ToStringAndReturn();

        var second = SourceWriter.Rent();
        var secondLength = second.Length;
        var secondLevel = second.Level;
        var rendered = second.Line("fresh").ToStringAndReturn();

        await Assert.That(text).IsEqualTo("    left over");
        await Assert.That(secondLength).IsEqualTo(0);
        await Assert.That(secondLevel).IsEqualTo(0);
        await Assert.That(rendered).IsEqualTo("fresh\n");
    }

    /// <summary>A second rent while the first is still out gets a writer of its own, so nested emitters never share text.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rent_WhileRented_KeepsTheTextApart()
    {
        var outer = SourceWriter.Rent();
        var inner = SourceWriter.Rent();

        _ = outer.Line("outer");
        _ = inner.Line("inner");
        var innerText = inner.ToStringAndReturn();
        var outerText = outer.ToStringAndReturn();

        await Assert.That(innerText).IsEqualTo("inner\n");
        await Assert.That(outerText).IsEqualTo("outer\n");
    }

    /// <summary>A writer handed back without materializing leaves nothing behind for the next rent.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Return_WithoutMaterializing_LeavesNothingBehind()
    {
        const int oversized = 512 * 1024;
        var large = SourceWriter.Rent(oversized);
        _ = large.Line("big");
        large.Return();

        var next = SourceWriter.Rent();
        var length = next.Length;
        next.Return();

        await Assert.That(length).IsEqualTo(0);
    }

    /// <summary>Reset empties the writer and returns it to the outermost level, ready for another file.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Reset_EmptiesAndReturnsToLevelZero()
    {
        var writer = new SourceWriter().Indent().Append("partial");

        _ = writer.Reset();

        await Assert.That(writer.Length).IsEqualTo(0);
        await Assert.That(writer.Level).IsEqualTo(0);
        await Assert.That(writer.Line("x").ToString()).IsEqualTo("x\n");
    }
}
