﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;

using ReactiveMarbles.NuGet.Helpers;

using ReactiveMarbles.SourceGenerator.TestNuGetHelper.Compilation;

using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// A helper class to facilitate the testing of incremental source generators.
/// It provides utilities to initialize dependencies, run generators, and verify the output.
/// </summary>
/// <param name="testOutput">The test output helper for capturing test logs.</param>
public sealed class TestHelper(ITestOutputHelper testOutput) : IDisposable
{
    /// <summary>
    /// Represents the NuGet library dependency for the Splat library.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    private static readonly LibraryRange SplatLibrary =
        new("Splat", VersionRange.AllStableFloating, LibraryDependencyTarget.Package);

    /// <summary>
    /// Represents the NuGet library dependency for the ReactiveUI library.
    /// </summary>
    private static readonly LibraryRange ReactiveuiLibrary =
        new("ReactiveUI", VersionRange.AllStableFloating, LibraryDependencyTarget.Package);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Holds the compiler instance used for event-related code generation.
    /// </summary>
    private EventBuilderCompiler? _eventCompiler;

    /// <summary>
    /// Asynchronously initializes the source generator helper by downloading required packages.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        NuGetFramework[] targetFrameworks = [new NuGetFramework(".NETCoreApp", new Version(8, 0, 0, 0))];

        // Download necessary NuGet package files.
        var inputGroup = await NuGetPackageHelper.DownloadPackageFilesAndFolder(
            new[] { SplatLibrary, ReactiveuiLibrary },
            targetFrameworks,
            packageOutputDirectory: null).ConfigureAwait(false);

        // Initialize the event compiler with downloaded packages and target framework.
        var framework = targetFrameworks[0];
        _eventCompiler = new EventBuilderCompiler(inputGroup, inputGroup, framework);
    }

    /// <summary>
    /// Tests a generator expecting it to fail by throwing an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <typeparam name="T">The type of the incremental generator being tested.</typeparam>
    /// <param name="source">The source code to test.</param>
    /// <param name="contractParameter">The parameter to be used in the test.</param>
    /// <param name="callerType">The type that called this test method.</param>
    /// <param name="file">The path of the calling file (auto-populated).</param>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    public void TestFail<T>(
        string source,
        string contractParameter,
        Type callerType,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "")
        where T : IIncrementalGenerator, new()
    {
        if (callerType is null)
        {
            throw new ArgumentNullException(nameof(callerType));
        }

        if (_eventCompiler is null)
        {
            throw new InvalidOperationException("Must have valid compiler instance.");
        }

        var utility = new SourceGeneratorUtility(x => testOutput.WriteLine(x));

        Assert.Throws<InvalidOperationException>(() => RunGeneratorAndCheck<T>(source));
    }

    /// <summary>
    /// Tests a generator expecting it to pass successfully.
    /// </summary>
    /// <typeparam name="T">The type of the incremental generator being tested.</typeparam>
    /// <param name="source">The source code to test.</param>
    /// <param name="contractParameter">The parameter to be used in the test.</param>
    /// <param name="callerType">The type that called this test method.</param>
    /// <param name="file">The path of the calling file (auto-populated).</param>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <returns>The driver.</returns>
    public GeneratorDriver TestPass<T>(
        string source,
        string contractParameter,
        Type callerType,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "")
        where T : IIncrementalGenerator, new()
    {
        if (callerType is null)
        {
            throw new ArgumentNullException(nameof(callerType));
        }

        return RunGeneratorAndCheck<T>(source);
    }

    /// <inheritdoc/>
    public void Dispose() => _eventCompiler?.Dispose();

    /// <summary>
    /// Runs the specified source generator and validates the generated code.
    /// </summary>
    /// <typeparam name="T">The type of the source generator.</typeparam>
    /// <param name="code">The code to be parsed and processed by the generator.</param>
    /// <param name="ignoreConditional">Optional. A function to ignore specific diagnostics.</param>
    /// <param name="rerunCompilation">Indicates whether to rerun the compilation after running the generator.</param>
    /// <returns>The generator driver used to run the generator.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the compiler instance is not valid or if the compilation fails.
    /// </exception>
    public GeneratorDriver RunGeneratorAndCheck<T>(
        string code,
        Func<Diagnostic, bool>? ignoreConditional = null,
        bool rerunCompilation = true)
        where T : IIncrementalGenerator, new()
    {
        if (_eventCompiler is null)
        {
            throw new InvalidOperationException("Must have a valid compiler instance.");
        }

        // Collect required assembly references.
        var assemblies = new HashSet<MetadataReference>(
            Basic.Reference.Assemblies.Net80.References.All
            .Concat(_eventCompiler.Modules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)))
            .Concat(_eventCompiler.ReferencedModules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)))
            .Concat(_eventCompiler.NeededModules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName))));

        // Create a compilation with the provided source code.
        var compilation = CSharpCompilation.Create(
            "TestProject",
            [CSharpSyntaxTree.ParseText(code)],
            assemblies,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        // Optionally validate diagnostics before running the generator.
        if (ignoreConditional is not null)
        {
            var diagnostics = compilation.GetDiagnostics().ToList();
            diagnostics.Where(x => !ignoreConditional(x)).Should().BeEmpty();
        }

        var generator = new T();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator());

        if (rerunCompilation)
        {
            // Run the generator and capture diagnostics.
            var rerunDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

            // If any warnings or errors are found, log them to the test output before throwing an exception.
            var offendingDiagnostics = diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Warning)
                .ToList();

            if (offendingDiagnostics.Count > 0)
            {
                foreach (var diagnostic in offendingDiagnostics)
                {
                    testOutput.WriteLine($"Diagnostic: {diagnostic.Id} - {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException("Compilation failed due to the above diagnostics.");
            }

            return rerunDriver;
        }

        // If rerun is not needed, simply run the generator.
        return driver.RunGenerators(compilation);
    }
}
