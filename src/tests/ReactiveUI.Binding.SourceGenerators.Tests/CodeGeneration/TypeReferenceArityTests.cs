// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Checks generic arity matching for shared and flavour-specific runtime types.</summary>
public class TypeReferenceArityTests
{
    /// <summary>Nested syntax does not count as another outer type argument.</summary>
    /// <param name="suffix">The syntax after a type name.</param>
    /// <param name="expected">The outer generic arity.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("", 0)]
    [Arguments(".Create<int>()", 0)]
    [Arguments("<int>", 1)]
    [Arguments("<int, string>", 2)]
    [Arguments("<(int, string), int[,]>", 2)]
    [Arguments("<Dictionary<int, List<string>>, (int, int)>", 2)]
    public async Task Read_CountsOuterArguments(string suffix, int expected) =>
        await Assert.That(TypeReferenceArity.Read(suffix, 0)).IsEqualTo(expected);

    /// <summary>CLR metadata retains the distinction between non-generic and generic names.</summary>
    /// <param name="name">The metadata type name.</param>
    /// <param name="expected">The declared generic arity.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Signal", 0)]
    [Arguments("Signal`1", 1)]
    [Arguments("Tuple`16", 16)]
    public async Task FromMetadata_ReadsArity(string name, int expected) =>
        await Assert.That(TypeReferenceArity.FromMetadata(name, name.IndexOf('`'))).IsEqualTo(expected);
}
