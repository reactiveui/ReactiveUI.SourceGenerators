// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Compares a generator run's output with the snapshots stored in the test project.</summary>
internal static class GeneratorSnapshot
{
    /// <summary>The environment variable that makes a run write its output over the snapshots.</summary>
    private const string AcceptVariable = "ACCEPT_SNAPSHOTS";

    /// <summary>The assembly metadata key the test project records its snapshot directory under.</summary>
    private const string DirectoryMetadataKey = "GeneratorSnapshotDirectory";

    /// <summary>The suffix of a stored snapshot.</summary>
    private const string VerifiedSuffix = ".verified.cs";

    /// <summary>The suffix of the output written beside a snapshot it does not match.</summary>
    private const string ReceivedSuffix = ".received.cs";

    /// <summary>The directory holding the snapshots.</summary>
    private static readonly string SnapshotDirectory = ReadSnapshotDirectory();

    /// <summary>Gets every stored snapshot, for checks that apply to all generated output.</summary>
    internal static IEnumerable<string> VerifiedSnapshots => Directory.EnumerateFiles(SnapshotDirectory, $"*{VerifiedSuffix}");

    /// <summary>Asserts that the generated files match the stored snapshots.</summary>
    /// <param name="driver">The driver after the generator has run.</param>
    /// <param name="typeName">The snapshot name's type segment.</param>
    /// <param name="methodName">The snapshot name's method segment.</param>
    /// <returns>A task that completes once every file has been compared.</returns>
    internal static async Task VerifyAsync(GeneratorDriver driver, string typeName, string methodName)
    {
        ArgumentNullException.ThrowIfNull(driver);

        var prefix = $"{typeName}.{methodName}#";
        var accept = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AcceptVariable));
        var produced = new HashSet<string>(StringComparer.Ordinal);
        var failures = new List<string>();

        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                var name = prefix + Path.GetFileNameWithoutExtension(source.HintName);
                _ = produced.Add(name);

                var output = $"//HintName: {source.HintName}\n{Normalize(source.SourceText.ToString())}";
                if (!await StoreAsync(Path.Combine(SnapshotDirectory, name), output, accept))
                {
                    failures.Add($"{name}{VerifiedSuffix} does not match the generated output; see {name}{ReceivedSuffix}");
                }
            }
        }

        foreach (var snapshot in Directory.EnumerateFiles(SnapshotDirectory, $"{prefix}*{VerifiedSuffix}"))
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
                failures.Add($"{fileName} is no longer generated");
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

        // Compared before accepting, so a matching snapshot keeps its bytes rather than being rewritten.
        if (File.Exists(verifiedPath) && Normalize(await File.ReadAllTextAsync(verifiedPath)) == output)
        {
            File.Delete(receivedPath);
            return true;
        }

        if (accept)
        {
            await File.WriteAllTextAsync(verifiedPath, output);
            File.Delete(receivedPath);
            return true;
        }

        await File.WriteAllTextAsync(receivedPath, output);
        return false;
    }

    /// <summary>Reads the snapshot directory the test project recorded when it was built.</summary>
    /// <returns>The absolute path of the snapshot directory.</returns>
    /// <exception cref="InvalidOperationException">The test assembly records no snapshot directory.</exception>
    private static string ReadSnapshotDirectory()
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

    /// <summary>Normalises line endings so a snapshot compares the same on every checkout.</summary>
    /// <param name="text">The text to normalise.</param>
    /// <returns>The text with LF line endings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);
}
