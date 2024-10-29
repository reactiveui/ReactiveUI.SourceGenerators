﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;

using ReactiveMarbles.NuGet.Helpers;

using ReactiveMarbles.SourceGenerator.TestNuGetHelper.Compilation;

using ReactiveUI.SourceGenerators;
using ReactiveUI.SourceGenerators.WinForms;

using Xunit.Abstractions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// A helper class to facilitate the testing of incremental source generators.
/// It provides utilities to initialize dependencies, run generators, and verify the output.
/// </summary>
/// <typeparam name="T">Type of Incremental Generator.</typeparam>
/// <seealso cref="System.IDisposable" />
/// <param name="testOutput">The test output helper for capturing test logs.</param>
public sealed class TestHelper<T>(ITestOutputHelper testOutput) : IDisposable
        where T : IIncrementalGenerator, new()
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

    private static readonly string mscorlibPath = Path.Combine(
            System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
            "mscorlib.dll");

    private static readonly MetadataReference[] References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(TestHelper<T>).Assembly.Location),

        // Create mscorlib Reference
        MetadataReference.CreateFromFile(mscorlibPath)

        // Wpf references
        ////MetadataReference.CreateFromFile(Assembly.Load("PresentationCore").Location),
        ////MetadataReference.CreateFromFile(Assembly.Load("PresentationFramework").Location),
        ////MetadataReference.CreateFromFile(Assembly.Load("WindowsBase").Location),
        ////MetadataReference.CreateFromFile(Assembly.Load("System.Xaml").Location),
    ];

    /// <summary>
    /// Holds the compiler instance used for event-related code generation.
    /// </summary>
    private EventBuilderCompiler? _eventCompiler;

    /// <summary>
    /// Verifieds the file path.
    /// </summary>
    /// <returns>
    /// A string.
    /// </returns>
    public string VerifiedFilePath()
    {
        var name = typeof(T).Name;
        return name switch
        {
            nameof(ReactiveGenerator) => "..\\REACTIVE",
            nameof(ReactiveCommandGenerator) => "..\\REACTIVECMD",
            nameof(ObservableAsPropertyGenerator) => "..\\OAPH",
            nameof(IViewForGenerator) => "..\\IVIEWFOR",
            nameof(RoutedControlHostGenerator) => "..\\ROUTEDHOST",
            nameof(ViewModelControlHostGenerator) => "..\\CONTROLHOST",
            _ => name,
        };
    }

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
    /// <param name="source">The source code to test.</param>
    public void TestFail(
        string source)
    {
        if (_eventCompiler is null)
        {
            throw new InvalidOperationException("Must have valid compiler instance.");
        }

        var utility = new SourceGeneratorUtility(x => testOutput.WriteLine(x));

        Assert.Throws<InvalidOperationException>(() => RunGeneratorAndCheck(source));
    }

    /// <summary>
    /// Tests a generator expecting it to pass successfully.
    /// </summary>
    /// <param name="source">The source code to test.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <returns>
    /// The driver.
    /// </returns>
    /// <exception cref="InvalidOperationException">Must have valid compiler instance.</exception>
    /// <exception cref="ArgumentNullException">callerType.</exception>
    public GeneratorDriver TestPass(
        string source,
        bool withPreDiagnosics = false)
    {
        if (_eventCompiler is null)
        {
            throw new InvalidOperationException("Must have valid compiler instance.");
        }

        return RunGeneratorAndCheck(source, withPreDiagnosics);
    }

    /// <inheritdoc/>
    public void Dispose() => _eventCompiler?.Dispose();

    /// <summary>
    /// Runs the specified source generator and validates the generated code.
    /// </summary>
    /// <param name="code">The code to be parsed and processed by the generator.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <param name="rerunCompilation">Indicates whether to rerun the compilation after running the generator.</param>
    /// <returns>
    /// The generator driver used to run the generator.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the compiler instance is not valid or if the compilation fails.</exception>
    public GeneratorDriver RunGeneratorAndCheck(
        string code,
        bool withPreDiagnosics = false,
        bool rerunCompilation = true)
    {
        if (_eventCompiler is null)
        {
            throw new InvalidOperationException("Must have a valid compiler instance.");
        }

        // Collect required assembly references.
        var assemblies = new HashSet<MetadataReference>(
            Basic.Reference.Assemblies.Net80.References.All
            .Concat(References)
            .Concat(_eventCompiler.Modules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)))
            .Concat(_eventCompiler.ReferencedModules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)))
            .Concat(_eventCompiler.NeededModules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName))));

        var syntaxTree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12));

        // Create a compilation with the provided source code.
        var compilation = CSharpCompilation.Create(
            "TestProject",
            [syntaxTree],
            assemblies,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        if (withPreDiagnosics)
        {
            // Validate diagnostics before running the generator.
            var prediagnostics = compilation.GetDiagnostics()
                .Where(d => d.Severity > DiagnosticSeverity.Warning)
                .ToList();
            prediagnostics.Should().BeEmpty();
        }

        var generator = new T();
        var driver = CSharpGeneratorDriver.Create(generator).WithUpdatedParseOptions((CSharpParseOptions)syntaxTree.Options);

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
