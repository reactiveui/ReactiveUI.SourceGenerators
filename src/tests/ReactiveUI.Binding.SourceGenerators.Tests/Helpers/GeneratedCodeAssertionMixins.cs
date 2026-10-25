// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>TUnit assertion extension methods for <see cref="GeneratorTestResult"/>.</summary>
internal static class GeneratedCodeAssertionMixins
{
    /// <summary>Provides the generator-output assertions for <paramref name="result"/>.</summary>
    /// <param name="result">The generator test result being asserted about.</param>
    extension(GeneratorTestResult result)
    {
        /// <summary>Asserts that the output compilation has no errors.</summary>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task CompilationSucceeds()
        {
            if (result.CompilationErrors.IsEmpty)
            {
                return;
            }

            var errorMessages = string.Join(
                Environment.NewLine,
                result.CompilationErrors.Select(static d => $"  {d.Id}: {d.GetMessage()} at {d.Location}"));

            await Assert.That(result.CompilationErrors.Length).IsEqualTo(0)
                .Because(
                    $"Compilation should succeed but had {result.CompilationErrors.Length} error(s):{Environment.NewLine}{errorMessages}");
        }

        /// <summary>Asserts that the output compilation has no warnings.</summary>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task HasNoCompilationWarnings()
        {
            if (result.CompilationWarnings.IsEmpty)
            {
                return;
            }

            var warningMessages = string.Join(
                Environment.NewLine,
                result.CompilationWarnings.Select(static d => $"  {d.Id}: {d.GetMessage()} at {d.Location}"));

            await Assert.That(result.CompilationWarnings.Length).IsEqualTo(0)
                .Because($"Compilation should produce no warnings but had:{Environment.NewLine}{warningMessages}");
        }

        /// <summary>Asserts that a generated source file with the specified hint name exists.</summary>
        /// <param name="hintName">The hint name of the generated file.</param>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task HasGeneratedSource(string hintName) =>
            await Assert.That(result.GeneratedSources.ContainsKey(hintName)).IsTrue()
                .Because(
                    $"Expected generated source '{hintName}' but found: [{string.Join(", ", result.GeneratedSources.Keys)}]");

        /// <summary>Asserts that a generated source file contains the specified text.</summary>
        /// <param name="hintName">The hint name of the generated file.</param>
        /// <param name="text">The text expected in the generated source.</param>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task GeneratedSourceContains(string hintName, string text)
        {
            await result.HasGeneratedSource(hintName);
            var source = result.GeneratedSources[hintName];
            await Assert.That(source).Contains(text);
        }

        /// <summary>Asserts how many times a generated source file contains the specified text.</summary>
        /// <param name="hintName">The hint name of the generated file.</param>
        /// <param name="text">The text to count.</param>
        /// <param name="expected">The number of occurrences expected.</param>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task GeneratedSourceContainsCount(string hintName, string text, int expected)
        {
            await result.HasGeneratedSource(hintName);
            var source = result.GeneratedSources[hintName];

            var count = 0;
            var matchIndex = source.IndexOf(text, StringComparison.Ordinal);
            while (matchIndex >= 0)
            {
                count++;
                matchIndex = source.IndexOf(text, matchIndex + text.Length, StringComparison.Ordinal);
            }

            await Assert.That(count).IsEqualTo(expected)
                .Because($"'{text}' should appear {expected} time(s) in '{hintName}':{Environment.NewLine}{source}");
        }

        /// <summary>Asserts that a generated source file does not contain the specified text.</summary>
        /// <param name="hintName">The hint name of the generated file.</param>
        /// <param name="text">The text that must not appear in the generated source.</param>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task GeneratedSourceDoesNotContain(string hintName, string text)
        {
            await result.HasGeneratedSource(hintName);
            var source = result.GeneratedSources[hintName];
            await Assert.That(source).DoesNotContain(text);
        }

        /// <summary>Asserts that no generated source file with the specified hint name exists.</summary>
        /// <param name="hintName">The hint name that should NOT be present.</param>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task DoesNotHaveGeneratedSource(string hintName) =>
            await Assert.That(result.GeneratedSources.ContainsKey(hintName)).IsFalse()
                .Because(
                    $"Expected no generated source '{hintName}' but found it among: [{string.Join(", ", result.GeneratedSources.Keys)}]");

        /// <summary>Asserts that the generator produced no diagnostics.</summary>
        /// <returns>A task representing the asynchronous assertion.</returns>
        internal async Task HasNoGeneratorDiagnostics()
        {
            if (result.GeneratorDiagnostics.IsEmpty)
            {
                return;
            }

            var diagnosticMessages = string.Join(
                Environment.NewLine,
                result.GeneratorDiagnostics.Select(static d => $"  {d.Id}: {d.GetMessage()}"));

            await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0)
                .Because($"Generator should produce no diagnostics but had:{Environment.NewLine}{diagnosticMessages}");
        }
    }
}
