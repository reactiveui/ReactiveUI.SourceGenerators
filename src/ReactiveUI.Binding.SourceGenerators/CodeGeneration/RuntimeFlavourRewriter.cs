// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Retargets generated code from the lean runtime library onto the System.Reactive flavour.</summary>
/// <remarks>
/// <para>
/// The two runtime packages are the same source compiled twice, and they share no type names: everything the
/// lean package puts in <c>ReactiveUI.Binding.*</c> the other puts in <c>ReactiveUI.Binding.Reactive.*</c>, and
/// the scheduler abstraction differs outright. Generated code that names the lean types does not compile at all
/// against the other package.
/// </para>
/// <para>
/// The emitters write the lean names, and this pass shifts them when the consumer references the other package.
/// It runs on the finished text rather than being threaded through the emitters because most of the generated
/// bodies are non-interpolated raw string literals, whose braces are the braces of the generated code - making
/// them interpolated to inject a namespace would mean escaping every one.
/// </para>
/// <para>
/// The shift is anchored on the names the runtime namespace actually declares, read from the referenced
/// assembly rather than listed here, so it cannot go stale as the runtime grows and it cannot touch a consumer
/// type that merely happens to sit under <c>ReactiveUI.Binding</c> - which is exactly what a consumer whose own
/// root namespace starts that way would otherwise hit.
/// </para>
/// </remarks>
internal static class RuntimeFlavourRewriter
{
    /// <summary>The prefix every generated reference to the lean runtime library starts with.</summary>
    private const string LeanPrefix = "global::ReactiveUI.Binding.";

    /// <summary>The prefix every generated reference to the lean Primitives stack starts with.</summary>
    private const string PrimitivesPrefix = "global::ReactiveUI.Primitives.";

    /// <summary>The segment the System.Reactive flavour inserts after the shared root.</summary>
    private const string ReactiveSegment = "Reactive.";

    /// <summary>How much headroom to leave for the segments the shift inserts, as a fraction of the source.</summary>
    private const int GrowthDivisor = 8;

    /// <summary>Retargets generated source onto the runtime flavour the consumer actually references.</summary>
    /// <param name="source">The generated source, written against the lean runtime library.</param>
    /// <param name="features">The consumer compilation's snapshot, naming the flavour and its members.</param>
    /// <returns>The source, unchanged for a lean consumer.</returns>
    internal static string Retarget(string source, in LanguageFeatures features)
    {
        if (!features.UsesReactiveRuntime)
        {
            return source;
        }

        var scheduled = source.Replace(Constants.LeanSchedulerTypeName, Constants.ReactiveSchedulerTypeName)
            .Replace(GeneratedTypeNames.ImmediateSequencer, "global::System.Reactive.Concurrency.Scheduler.Immediate");
        var runtime = ShiftNamespace(scheduled, LeanPrefix, features.RuntimeNamespaceMembers, matchWholePath: false);
        return ShiftNamespace(runtime, PrimitivesPrefix, features.PrimitivesNamespaceMembers, matchWholePath: true);
    }

    /// <summary>Inserts the flavour segment into every reference the shifted namespace declares.</summary>
    /// <param name="source">The generated source.</param>
    /// <param name="prefix">The lean prefix the references to shift start with.</param>
    /// <param name="shiftedNamespaceMembers">The names the shifted namespace declares.</param>
    /// <param name="matchWholePath">
    /// Whether a reference has to match a whole type path rather than its leading segment. A namespace split
    /// across a shared assembly and a flavoured one offers only some of its types under the shifted name, so
    /// the leading segment alone would shift the shared ones onto a namespace that does not declare them.
    /// </param>
    /// <returns>The shifted source.</returns>
    private static string ShiftNamespace(
        string source,
        string prefix,
        EquatableArray<string> shiftedNamespaceMembers,
        bool matchWholePath)
    {
        var index = source.IndexOf(prefix, StringComparison.Ordinal);
        if (index < 0)
        {
            return source;
        }

        var builder = PooledBuilder.Rent(source.Length + (source.Length / GrowthDivisor));
        var copiedTo = 0;

        while (index >= 0)
        {
            var segmentStart = index + prefix.Length;
            var segmentEnd = segmentStart;
            while (segmentEnd < source.Length && IsIdentifierCharacter(source[segmentEnd]))
            {
                segmentEnd++;
            }

            // A whole-path match walks on through the dots, so Advanced.SwitchMapSignal is told apart from
            // Signals.MapSignal, which the shared core declares and which therefore keeps its name.
            var pathEnd = matchWholePath ? ExtendThroughDots(source, segmentEnd) : segmentEnd;

            if (Declares(shiftedNamespaceMembers, source, segmentStart, pathEnd - segmentStart, matchWholePath)
                || (matchWholePath && DeclaresAnyPrefix(shiftedNamespaceMembers, source, segmentStart, pathEnd)))
            {
                _ = builder.Append(source, copiedTo, segmentStart - copiedTo).Append(ReactiveSegment);
                copiedTo = segmentStart;
            }

            index = source.IndexOf(prefix, segmentEnd, StringComparison.Ordinal);
        }

        return PooledBuilder.ToStringAndReturn(builder.Append(source, copiedTo, source.Length - copiedTo));
    }

    /// <summary>Determines whether the shifted namespace declares the name at the given span of the source.</summary>
    /// <param name="shiftedNamespaceMembers">The names the shifted namespace declares.</param>
    /// <param name="source">The generated source.</param>
    /// <param name="start">The index the name starts at.</param>
    /// <param name="length">The length of the name.</param>
    /// <param name="matchArity">Whether metadata generic arity must match the reference.</param>
    /// <returns><see langword="true"/> when the name is one the shifted namespace declares.</returns>
    private static bool Declares(EquatableArray<string> shiftedNamespaceMembers, string source, int start, int length, bool matchArity)
    {
        for (var i = 0; i < shiftedNamespaceMembers.Length; i++)
        {
            var member = shiftedNamespaceMembers[i];
            var marker = matchArity ? member.IndexOf('`') : -1;
            if ((marker < 0 ? member.Length : marker) == length && string.CompareOrdinal(member, 0, source, start, length) == 0
                && (!matchArity || TypeReferenceArity.Read(source, start + length) == TypeReferenceArity.FromMetadata(member, marker)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Extends a reference through the dotted segments that follow it.</summary>
    /// <param name="source">The generated source.</param>
    /// <param name="from">The index just past the first segment.</param>
    /// <returns>The index just past the last dotted segment.</returns>
    private static int ExtendThroughDots(string source, int from)
    {
        var pathEnd = from;
        while (pathEnd < source.Length && source[pathEnd] == '.')
        {
            var next = pathEnd + 1;
            while (next < source.Length && IsIdentifierCharacter(source[next]))
            {
                next++;
            }

            if (next == pathEnd + 1)
            {
                return pathEnd;
            }

            pathEnd = next;
        }

        return pathEnd;
    }

    /// <summary>
    /// Determines whether the shifted namespace declares any dotted prefix of the path at the given span, so a
    /// reference that reaches past the type name into its members still matches the type.
    /// </summary>
    /// <param name="shiftedNamespaceMembers">The type paths the shifted namespace declares.</param>
    /// <param name="source">The generated source.</param>
    /// <param name="start">The index the path starts at.</param>
    /// <param name="end">The index just past the end of the path.</param>
    /// <returns><see langword="true"/> when some prefix of the path is a declared type path.</returns>
    private static bool DeclaresAnyPrefix(
        EquatableArray<string> shiftedNamespaceMembers,
        string source,
        int start,
        int end)
    {
        for (var cut = end - 1; cut > start; cut--)
        {
            if (source[cut] != '.')
            {
                continue;
            }

            if (Declares(shiftedNamespaceMembers, source, start, cut - start, true))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a character can appear inside a C# identifier.</summary>
    /// <param name="character">The character to test.</param>
    /// <returns><see langword="true"/> when the character continues an identifier.</returns>
    private static bool IsIdentifierCharacter(char character) =>
        char.IsLetterOrDigit(character) || character == '_';
}
