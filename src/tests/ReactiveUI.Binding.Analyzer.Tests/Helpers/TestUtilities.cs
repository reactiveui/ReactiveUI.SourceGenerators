// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>Utilities for testing analyzers and code fixes.</summary>
public static class TestUtilities
{
    /// <summary>Normalizes whitespace and line endings in source code for comparison.</summary>
    /// <param name="source">The source code to normalize.</param>
    /// <returns>Normalized source code suitable for comparison.</returns>
    public static string NormalizeWhitespace(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        return string.Join(
            "\n",
            source.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                .Select(static line => line.Trim()));
    }

    /// <summary>Compares two source code strings with whitespace normalization.</summary>
    /// <param name="expected">The expected source code.</param>
    /// <param name="actual">The actual source code.</param>
    /// <returns>True if the sources are equivalent after normalization.</returns>
    public static bool AreEquivalent(string expected, string actual)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expected);
        ArgumentException.ThrowIfNullOrWhiteSpace(actual);
        var result = NormalizeWhitespace(expected) == NormalizeWhitespace(actual);

        if (!result)
        {
            // Attach the two sources to the test's own output so a failure is diagnosable from the
            // test report; TestContext is null when the helper runs outside a test (e.g. a hook).
            TestContext.Current?.OutputWriter.WriteLine(
                $"=== EXPECTED ==={Environment.NewLine}{expected}{Environment.NewLine}{Environment.NewLine}=== ACTUAL ==={Environment.NewLine}{actual}");
        }

        return result;
    }
}
