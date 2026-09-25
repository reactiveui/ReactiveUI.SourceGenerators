// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Compares a generator run's output with the snapshots stored in the test project.</summary>
/// <remarks>
/// <para>
/// Each generated file is stored as <c>{folder}/{type}.{method}#{hint name}.verified.cs</c>: a <c>//HintName:</c>
/// line, then the generated text. Output that differs from its snapshot, or has none, is written beside it as
/// <c>*.received.cs</c> and fails the test, as does a snapshot the run no longer produces.
/// </para>
/// <para>
/// Set <c>ACCEPT_SNAPSHOTS=1</c> to write every output over its snapshot and delete snapshots nothing produces.
/// </para>
/// </remarks>
internal static class GeneratorSnapshot
{
    /// <summary>The environment variable that makes a run write its output over the snapshots.</summary>
    private const string AcceptVariable = "ACCEPT_SNAPSHOTS";

    /// <summary>The assembly metadata key the test project records its snapshot root under.</summary>
    private const string DirectoryMetadataKey = "GeneratorSnapshotDirectory";

    /// <summary>The suffix of a stored snapshot.</summary>
    private const string VerifiedSuffix = ".verified.cs";

    /// <summary>The suffix of the output written beside a snapshot it does not match.</summary>
    private const string ReceivedSuffix = ".received.cs";

    /// <summary>
    /// A generated line carrying the generator's assembly version. It changes with every build of the package, so it
    /// is left out of the snapshots.
    /// </summary>
    private const string GeneratedCodeAttributeLine = "[global::System.CodeDom.Compiler.GeneratedCode(\"";

    /// <summary>The suffix a test class name ends with, dropped from the snapshot name.</summary>
    private const string TestClassSuffix = "Tests";

    /// <summary>The suffix every generated hint name ends with, dropped from the snapshot name.</summary>
    private const string HintSuffix = ".g.cs";

    /// <summary>The encoding snapshots are written in: UTF-8 without a byte order mark, as every file in the repository is.</summary>
    private static readonly UTF8Encoding SnapshotEncoding = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// The words shortened in a snapshot's method and hint segments, longest first so a longer word is never split by a
    /// shorter one. They keep snapshot paths well inside the Windows path limit.
    /// </summary>
    private static readonly (string Word, string Abbreviation)[] Abbreviations =
    [
        ("ReactiveUI.SourceGenerators.", string.Empty),
        ("CancellationToken", "Ct"),
        ("ReactiveCommand", "RxCmd"),
        ("ReactiveObject", "RxObj"),
        ("ReactiveCollection", "RxColl"),
        ("BindableDerivedList", "Bdl"),
        ("ObservableCollection", "ObsColl"),
        ("ReadOnly", "Ro"),
        ("Observable", "Obs"),
        ("Properties", "Props"),
        ("Property", "Prop"),
        ("Reactive", "Rx"),
        ("Attribute", "Attr"),
        ("Collection", "Coll"),
        ("Command", "Cmd"),
        ("Generic", "Gen"),
        ("Generates", "Gen"),
        ("Nullable", "Null"),
        ("Parameter", "Param"),
        ("Namespace", "Ns"),
        ("Multiple", "Multi"),
        ("ViewModel", "Vm"),
        ("IViewFor", "Ivf"),
        ("Internal", "Int"),
        ("Inheritance", "Inh"),
        ("Modifier", "Mod"),
        ("Access", "Acc"),
        ("Nested", "Nest"),
        ("Different", "Diff"),
        ("Complex", "Cplx"),
        ("Return", "Ret"),
        ("Type", "T"),
        ("With", "W"),
        ("From", "F"),
        ("And", string.Empty),
    ];

    /// <summary>The directory holding the snapshot folders.</summary>
    private static readonly string SnapshotRoot = ReadSnapshotRoot();

    /// <summary>Asserts that the generated files match the stored snapshots.</summary>
    /// <param name="driver">The driver after the generator has run.</param>
    /// <param name="folder">The snapshot folder, one per generator.</param>
    /// <param name="typeName">The snapshot name's type segment.</param>
    /// <param name="methodName">The snapshot name's method segment.</param>
    /// <returns>A task that completes once every file has been compared.</returns>
    internal static async Task VerifyAsync(GeneratorDriver driver, string folder, string typeName, string methodName)
    {
        ArgumentNullException.ThrowIfNull(driver);

        var directory = Path.Combine(SnapshotRoot, folder);
        _ = Directory.CreateDirectory(directory);

        var prefix = $"{AbbreviateTypeName(typeName)}.{Abbreviate(methodName)}#";
        var accept = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AcceptVariable));
        var produced = new HashSet<string>(StringComparer.Ordinal);
        var failures = new List<string>();

        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                var name = prefix + AbbreviateHintName(source.HintName);
                _ = produced.Add(name);

                var output = $"//HintName: {source.HintName}\n{Scrub(source.SourceText.ToString())}";
                if (!await StoreAsync(Path.Combine(directory, name), output, accept))
                {
                    failures.Add($"{folder}/{name}{VerifiedSuffix} does not match the generated output; see {name}{ReceivedSuffix}");
                }
            }
        }

        foreach (var snapshot in Directory.EnumerateFiles(directory, $"{prefix}*{VerifiedSuffix}"))
        {
            var fileName = Path.GetFileName(snapshot);
            if (produced.Contains(fileName[..^VerifiedSuffix.Length]))
            {
                continue;
            }

            if (accept)
            {
                File.Delete(snapshot);
            }
            else
            {
                failures.Add($"{folder}/{fileName} is no longer generated");
            }
        }

        await Assert.That(failures).IsEmpty();
    }

    /// <summary>Settles one generated file against its snapshot, writing the received or accepted output as needed.</summary>
    /// <param name="basePath">The snapshot path without its suffix.</param>
    /// <param name="output">The generated output, in snapshot form.</param>
    /// <param name="accept">Whether the output replaces the snapshot.</param>
    /// <returns><see langword="true"/> when the output equals the snapshot or was accepted as it.</returns>
    private static async Task<bool> StoreAsync(string basePath, string output, bool accept)
    {
        var verifiedPath = basePath + VerifiedSuffix;
        var receivedPath = basePath + ReceivedSuffix;

        // Compared before accepting, so a matching snapshot keeps its bytes rather than being rewritten. A final line
        // break is not significant: some stored snapshots end with one and some do not.
        if (File.Exists(verifiedPath)
            && string.Equals(Normalize(await File.ReadAllTextAsync(verifiedPath)).TrimEnd('\n'), output.TrimEnd('\n'), StringComparison.Ordinal))
        {
            File.Delete(receivedPath);
            return true;
        }

        if (accept)
        {
            await File.WriteAllTextAsync(verifiedPath, output, SnapshotEncoding);
            File.Delete(receivedPath);
            return true;
        }

        await File.WriteAllTextAsync(receivedPath, output, SnapshotEncoding);
        return false;
    }

    /// <summary>Reads the snapshot root the test project recorded when it was built.</summary>
    /// <returns>The absolute path of the snapshot root.</returns>
    /// <exception cref="InvalidOperationException">The test assembly records no snapshot root.</exception>
    private static string ReadSnapshotRoot()
    {
        // Recorded at build time: a CI build maps caller file paths to /_/, which does not exist on disk.
        foreach (var attribute in typeof(GeneratorSnapshot).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (attribute.Key == DirectoryMetadataKey && !string.IsNullOrEmpty(attribute.Value))
            {
                return attribute.Value;
            }
        }

        throw new InvalidOperationException($"The test assembly records no '{DirectoryMetadataKey}' assembly metadata.");
    }

    /// <summary>Shortens a test class name to its capitals, such as <c>RGE</c> for <c>RxGenExtTests</c>.</summary>
    /// <param name="typeName">The test class name.</param>
    /// <returns>The abbreviated name.</returns>
    /// <remarks>Each generator's snapshots live in their own folder, so the codes only need to differ within one folder.</remarks>
    private static string AbbreviateTypeName(string typeName)
    {
        var name = typeName.EndsWith(TestClassSuffix, StringComparison.Ordinal) ? typeName[..^TestClassSuffix.Length] : typeName;
        var builder = new StringBuilder(name.Length);
        foreach (var character in name)
        {
            if (char.IsUpper(character) || char.IsDigit(character))
            {
                _ = builder.Append(character);
            }
        }

        return builder.ToString();
    }

    /// <summary>Shortens a generated file's hint name: no <c>.g.cs</c> suffix, and the common words abbreviated.</summary>
    /// <param name="hintName">The hint name.</param>
    /// <returns>The abbreviated name.</returns>
    private static string AbbreviateHintName(string hintName) =>
        Abbreviate(hintName.EndsWith(HintSuffix, StringComparison.Ordinal) ? hintName[..^HintSuffix.Length] : Path.GetFileNameWithoutExtension(hintName));

    /// <summary>Replaces each common word with its abbreviation.</summary>
    /// <param name="text">The text to shorten.</param>
    /// <returns>The shortened text.</returns>
    private static string Abbreviate(string text)
    {
        foreach (var (word, abbreviation) in Abbreviations)
        {
            text = text.Replace(word, abbreviation, StringComparison.Ordinal);
        }

        return text;
    }

    /// <summary>Puts generated text in snapshot form: LF line endings, without the generated-code attribute lines.</summary>
    /// <param name="text">The generated text.</param>
    /// <returns>The text as a snapshot stores it.</returns>
    private static string Scrub(string text)
    {
        var normalized = Normalize(text);
        if (!normalized.Contains(GeneratedCodeAttributeLine, StringComparison.Ordinal))
        {
            return normalized;
        }

        var builder = new StringBuilder(normalized.Length);
        foreach (var line in normalized.Split('\n'))
        {
            if (!line.Contains(GeneratedCodeAttributeLine, StringComparison.Ordinal))
            {
                _ = builder.Append(line).Append('\n');
            }
        }

        // Split yields an empty last element after a trailing newline; appending it added one newline too many.
        return builder.ToString(0, builder.Length - 1);
    }

    /// <summary>Normalises line endings so a snapshot compares the same on every checkout.</summary>
    /// <param name="text">The text to normalise.</param>
    /// <returns>The text with LF line endings.</returns>
    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);
}
