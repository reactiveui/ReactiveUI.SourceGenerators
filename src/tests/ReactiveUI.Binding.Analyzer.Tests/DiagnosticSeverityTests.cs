// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Every diagnostic that reports a call no generated code claims, so that the call throws when it runs, fails the build.
/// The rest describe a call that still runs, and stay warnings or information.
/// </summary>
public class DiagnosticSeverityTests
{
    /// <summary>Verifies that a diagnostic about a call that would throw is an error.</summary>
    /// <param name="id">The diagnostic id.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("RXUIBIND001")]
    [Arguments("RXUIBIND003")]
    [Arguments("RXUIBIND006")]
    [Arguments("RXUIBIND009")]
    [Arguments("RXUIBIND012")]
    [Arguments("RXUIBIND013")]
    [Arguments("RXUIBIND014")]
    [Arguments("RXUIBIND015")]
    [Arguments("RXUIBIND016")]
    [Arguments("RXUIBIND021")]
    public async Task CallThatWouldThrow_IsAnError(string id) =>
        await Assert.That(Descriptor(id).DefaultSeverity).IsEqualTo(DiagnosticSeverity.Error);

    /// <summary>Verifies that a diagnostic about a call that still runs is not an error.</summary>
    /// <param name="id">The diagnostic id.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("RXUIBIND002")]
    [Arguments("RXUIBIND004")]
    [Arguments("RXUIBIND005")]
    [Arguments("RXUIBIND007")]
    [Arguments("RXUIBIND008")]
    [Arguments("RXUIBIND010")]
    [Arguments("RXUIBIND011")]
    [Arguments("RXUIBIND017")]
    [Arguments("RXUIBIND018")]
    [Arguments("RXUIBIND019")]
    [Arguments("RXUIBIND020")]
    public async Task CallThatRuns_IsNotAnError(string id) =>
        await Assert.That(Descriptor(id).DefaultSeverity).IsNotEqualTo(DiagnosticSeverity.Error);

    /// <summary>Finds a descriptor among every analyzer the assembly ships.</summary>
    /// <param name="id">The diagnostic id.</param>
    /// <returns>The descriptor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DiagnosticDescriptor Descriptor(string id) =>
        typeof(BindingInvocationAnalyzer).Assembly.GetTypes()
            .Where(static t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
            .SelectMany(static t => ((DiagnosticAnalyzer)Activator.CreateInstance(t)!).SupportedDiagnostics)
            .First(d => d.Id == id);
}
