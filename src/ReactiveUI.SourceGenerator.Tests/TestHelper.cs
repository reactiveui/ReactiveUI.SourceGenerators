// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveMarbles.NuGet.Helpers;

using ReactiveMarbles.SourceGenerator.TestNuGetHelper.Compilation;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// A helper class to facilitate the testing of incremental source generators.
/// It provides utilities to initialize dependencies, run generators, and verify the output.
/// </summary>
/// <typeparam name="T">Type of Incremental Generator.</typeparam>
/// <seealso cref="IDisposable" />
public sealed partial class TestHelper<T> : IDisposable
        where T : IIncrementalGenerator, new()
{
    /// <summary>
    /// Represents the NuGet library dependency for the Splat library.
    /// </summary>
    private static readonly LibraryRange SplatLibrary =
        new("Splat", VersionRange.AllStable, LibraryDependencyTarget.Package);

    /// <summary>
    /// Represents the NuGet library dependency for the ReactiveUI library.
    /// </summary>
    private static readonly LibraryRange ReactiveuiLibrary =
        new("ReactiveUI", VersionRange.AllStable, LibraryDependencyTarget.Package);

    private static readonly string mscorlibPath = Path.Combine(
            System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
            "mscorlib.dll");

    private static readonly Assembly[] References =
    [
        typeof(object).Assembly,
        typeof(Enumerable).Assembly,
        typeof(T).Assembly,
        typeof(TestHelper<T>).Assembly,
        typeof(IViewFor).Assembly,
    ];

    /// <summary>
    /// Holds the compiler instance used for event-related code generation.
    /// </summary>
    private static readonly Lazy<Task<EventBuilderCompiler>> SharedEventCompiler = new(CreateSharedEventCompilerAsync);

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
            nameof(ReactiveGenerator) => "REACTIVE",
            nameof(ReactiveCommandGenerator) => "REACTIVECMD",
            nameof(ObservableAsPropertyGenerator) => "OAPH",
            nameof(IViewForGenerator) => "IVIEWFOR",
            nameof(RoutedControlHostGenerator) => "ROUTEDHOST",
            nameof(ViewModelControlHostGenerator) => "CONTROLHOST",
            nameof(BindableDerivedListGenerator) => "DERIVEDLIST",
            nameof(ReactiveCollectionGenerator) => "REACTIVECOLL",
            nameof(ReactiveObjectGenerator) => "REACTIVEOBJ",
            _ => name,
        };
    }

    /// <summary>
    /// Asynchronously initializes the source generator helper by downloading required packages.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeAsync() => _eventCompiler = await SharedEventCompiler.Value.ConfigureAwait(false);

    /// <summary>
    /// Tests a generator expecting it to fail by throwing an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="source">The source code to test.</param>
    /// <returns>A task representing the asynchronous assertion operation.</returns>
    public async Task TestFail(
        string source)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        var utility = new SourceGeneratorUtility(WriteTestOutput);

#pragma warning disable IDE0053 // Use expression body for lambda expression
#pragma warning disable RCS1021 // Convert lambda expression body to expression body
        Assert.Throws<InvalidOperationException>(() => { RunGeneratorAndCheck(source); });
#pragma warning restore RCS1021 // Convert lambda expression body to expression body
#pragma warning restore IDE0053 // Use expression body for lambda expression
    }

    /// <summary>
    /// Tests a generator expecting it to pass successfully.
    /// </summary>
    /// <param name="source">The source code to test.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    /// <exception cref="InvalidOperationException">Must have valid compiler instance.</exception>
    /// <exception cref="ArgumentNullException">callerType.</exception>
    public async Task TestPass(
        string source,
        bool withPreDiagnosics = false)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        await RunGeneratorAndCheck(source, withPreDiagnosics);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

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
    public SettingsTask RunGeneratorAndCheck(
        string code,
        bool withPreDiagnosics = false,
        bool rerunCompilation = true)
    {
        if (_eventCompiler is null)
        {
            throw new InvalidOperationException("Must have a valid compiler instance.");
        }

        IEnumerable<MetadataReference> basicReferences;
#if NET10_0_OR_GREATER
        basicReferences = Basic.Reference.Assemblies.Net100.References.All;
#elif NET9_0_OR_GREATER
        basicReferences = Basic.Reference.Assemblies.Net90.References.All;
#else
        basicReferences = Basic.Reference.Assemblies.Net80.References.All;
#endif

        // Collect required assembly references.
        var assemblies = new HashSet<MetadataReference>(
        basicReferences
            .Concat([MetadataReference.CreateFromFile(mscorlibPath)])
            .Concat(basicReferences)
            .Concat(GetTransitiveReferences(References))
            .Concat(_eventCompiler.Modules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)))
            .Concat(_eventCompiler.ReferencedModules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)))
            .Concat(_eventCompiler.NeededModules.Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName))));

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(code, parseOptions),
        };

        // Create a compilation with the provided source code.
        var compilation = CSharpCompilation.Create(
            "TestProject",
            syntaxTrees,
            assemblies,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        if (withPreDiagnosics)
        {
            // Validate diagnostics before running the generator.
            var prediagnostics = compilation.GetDiagnostics()
                .Where(d => d.Severity > DiagnosticSeverity.Warning)
                .ToList();

            if (prediagnostics.Count > 0)
            {
                foreach (var diagnostic in prediagnostics)
                {
                    WriteTestOutput($"Diagnostic: {diagnostic.Id} - {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException("Pre-generator compilation failed due to the above diagnostics.");
            }
        }

        var generator = new T();
        var driver = CSharpGeneratorDriver.Create(generator).WithUpdatedParseOptions(parseOptions);

        if (rerunCompilation)
        {
            // Run the generator and capture diagnostics.
            var rerunDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            // If any warnings or errors are found, log them to the test output before throwing an exception.
            var offendingDiagnostics = diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Warning)
                .ToList();

            if (offendingDiagnostics.Count > 0)
            {
                foreach (var diagnostic in offendingDiagnostics)
                {
                    WriteTestOutput($"Diagnostic: {diagnostic.Id} - {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException("Compilation failed due to the above diagnostics.");
            }

            // Validate generated code contains expected features
            ValidateGeneratedCode(code, rerunDriver);

            return VerifyGenerator(rerunDriver);
        }

        // If rerun is not needed, simply run the generator.
        return VerifyGenerator(driver.RunGenerators(compilation));
    }

    private static IEnumerable<string> GetGeneratedSupportSources()
    {
        yield return "using System.Runtime.CompilerServices;\n[assembly: InternalsVisibleTo(\"TestProject\")]";

        yield return GetAttributeDefinitionsMethodResult("GetAccessModifierEnum");

        if (typeof(T) == typeof(ReactiveCommandGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveCommandAttribute");
            yield return GetAttributeDefinitionsPropertyResult("ReactiveAttribute");
        }
        else if (typeof(T) == typeof(IViewForGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("IViewForAttribute");
        }
        else if (typeof(T) == typeof(ReactiveGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveAttribute");
        }
        else if (typeof(T) == typeof(ObservableAsPropertyGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ObservableAsPropertyAttribute");
        }
        else if (typeof(T) == typeof(BindableDerivedListGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("BindableDerivedListAttribute");
        }
        else if (typeof(T) == typeof(ReactiveCollectionGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveCollectionAttribute");
        }
        else if (typeof(T) == typeof(ReactiveObjectGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveObjectAttribute");
        }
    }

    private static ImmutableArray<MetadataReference> CreateSupportReferences()
    {
        var supportSources = GetGeneratedSupportSources().ToArray();

        if (supportSources.Length == 0)
        {
            return [];
        }

        IEnumerable<MetadataReference> basicReferences;
#if NET10_0_OR_GREATER
        basicReferences = Basic.Reference.Assemblies.Net100.References.All;
#elif NET9_0_OR_GREATER
        basicReferences = Basic.Reference.Assemblies.Net90.References.All;
#else
        basicReferences = Basic.Reference.Assemblies.Net80.References.All;
#endif

        var supportCompilation = CSharpCompilation.Create(
            $"{typeof(T).Name}.Support",
            supportSources.Select((source, index) => CSharpSyntaxTree.ParseText(source, path: $"Support{index}.g.cs")),
            basicReferences.Concat([MetadataReference.CreateFromFile(mscorlibPath)]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        using var stream = new MemoryStream();
        var emitResult = supportCompilation.Emit(stream);

        if (!emitResult.Success)
        {
            var diagnostics = string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static d => d.ToString()));
            throw new InvalidOperationException($"Failed to compile support sources for {typeof(T).Name}.{Environment.NewLine}{diagnostics}");
        }

        return [MetadataReference.CreateFromImage(stream.ToArray())];
    }

    private static string GetAttributeDefinitionsMethodResult(string methodName)
    {
        var attributeDefinitionsType = typeof(ReactiveGenerator).Assembly.GetType("ReactiveUI.SourceGenerators.Helpers.AttributeDefinitions", throwOnError: false, ignoreCase: false)
            ?? throw new InvalidOperationException("Could not locate AttributeDefinitions type.");

        var method = attributeDefinitionsType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Could not locate AttributeDefinitions.{methodName}.");

        return (string?)method.Invoke(null, null)
            ?? throw new InvalidOperationException($"AttributeDefinitions.{methodName} returned null.");
    }

    private static string GetAttributeDefinitionsPropertyResult(string propertyName)
    {
        var attributeDefinitionsType = typeof(ReactiveGenerator).Assembly.GetType("ReactiveUI.SourceGenerators.Helpers.AttributeDefinitions", throwOnError: false, ignoreCase: false)
            ?? throw new InvalidOperationException("Could not locate AttributeDefinitions type.");

        var property = attributeDefinitionsType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Could not locate AttributeDefinitions.{propertyName}.");

        return (string?)property.GetValue(null)
            ?? throw new InvalidOperationException($"AttributeDefinitions.{propertyName} returned null.");
    }

    [GeneratedRegex(@"\[Reactive\((?:.*?nameof\((\w+)\))+", RegexOptions.Singleline)]
    private static partial Regex ReactiveRegex();

    [GeneratedRegex(@"nameof\((\w+)\)")]
    private static partial Regex NameOfRegex();

    /// <summary>
    /// Validates that generated code contains expected features based on the source code attributes.
    /// </summary>
    /// <param name="sourceCode">The original source code.</param>
    /// <param name="driver">The generator driver with generated output.</param>
    private static void ValidateGeneratedCode(string sourceCode, GeneratorDriver driver)
    {
        var runResult = driver.GetRunResult();
        var generatedTrees = runResult.Results.SelectMany(r => r.GeneratedSources).ToList();
        var allGeneratedCode = string.Join("\n", generatedTrees.Select(t => t.SourceText.ToString()));

        // Check for AlsoNotify feature in Reactive attributes
        // Pattern matches: [Reactive(nameof(PropertyName))] or [Reactive(nameof(Prop1), nameof(Prop2))]
        var alsoNotifyPattern = ReactiveRegex();
        var nameofPattern = NameOfRegex();
        var matches = alsoNotifyPattern.Matches(sourceCode);

        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                // Extract all nameof() references within this attribute
                var nameofMatches = nameofPattern.Matches(match.Value);

                foreach (Match nameofMatch in nameofMatches)
                {
                    var propertyToNotify = nameofMatch.Groups[1].Value;

                    // Verify that the generated code contains calls to raise property changed for the additional property
                    // Check for various forms of property change notification
                    var hasNotification =
                        allGeneratedCode.Contains($"this.RaisePropertyChanged(nameof({propertyToNotify}))") ||
                        allGeneratedCode.Contains($"this.RaisePropertyChanged(\"{propertyToNotify}\")") ||
                        allGeneratedCode.Contains($"RaisePropertyChanged(nameof({propertyToNotify}))") ||
                        allGeneratedCode.Contains($"RaisePropertyChanged(\"{propertyToNotify}\")");

                    if (!hasNotification)
                    {
                        var errorMessage = $"Generated code does not include AlsoNotify for property '{propertyToNotify}'. " +
                                         $"Expected to find property change notification for '{propertyToNotify}' in the generated code.\n" +
                                         $"Source attribute: {match.Value}";

                        WriteTestOutput("=== VALIDATION FAILURE ===");
                        WriteTestOutput(errorMessage);
                        WriteTestOutput("=== SOURCE CODE SNIPPET ===");
                        WriteTestOutput(match.Value);
                        WriteTestOutput("=== GENERATED CODE ===");
                        WriteTestOutput(allGeneratedCode);
                        WriteTestOutput("=== END ===");

                        throw new InvalidOperationException(errorMessage);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively walks assembly references from the seed assemblies to collect
    /// all transitive dependencies as metadata references.
    /// </summary>
    /// <param name="seedAssemblies">The root assemblies to start from.</param>
    /// <returns>Metadata references for all reachable assemblies.</returns>
    private static IEnumerable<MetadataReference> GetTransitiveReferences(params Assembly[] seedAssemblies)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<Assembly>(seedAssemblies);

        while (queue.Count > 0)
        {
            var assembly = queue.Dequeue();
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                continue;
            }

            if (!seen.Add(assembly.Location))
            {
                continue;
            }

            yield return MetadataReference.CreateFromFile(assembly.Location);

            foreach (var referencedName in assembly.GetReferencedAssemblies())
            {
                try
                {
                    queue.Enqueue(System.Reflection.Assembly.Load(referencedName));
                }
                catch
                {
                    // System assemblies already covered by Basic.Reference.Assemblies
                }
            }
        }
    }

    private static void WriteTestOutput(string message) => TestContext.Current?.OutputWriter.WriteLine(message);

    private static async Task<EventBuilderCompiler> CreateSharedEventCompilerAsync()
    {
#if NET10_0_OR_GREATER
        NuGetFramework[] targetFrameworks = [new NuGetFramework(".NETCoreApp", new Version(10, 0, 0, 0))];
#elif NET9_0_OR_GREATER
        NuGetFramework[] targetFrameworks = [new NuGetFramework(".NETCoreApp", new Version(9, 0, 0, 0))];
#else
        NuGetFramework[] targetFrameworks = [new NuGetFramework(".NETCoreApp", new Version(8, 0, 0, 0))];
#endif

        var inputGroup = await NuGetPackageHelper.DownloadPackageFilesAndFolder(
            [SplatLibrary, ReactiveuiLibrary],
            targetFrameworks,
            packageOutputDirectory: null).ConfigureAwait(false);

        var framework = targetFrameworks[0];
        return new EventBuilderCompiler(inputGroup, inputGroup, framework);
    }

    private SettingsTask VerifyGenerator(GeneratorDriver driver) => Verify(driver)
        .UseDirectory(VerifiedFilePath())
        .ScrubLinesContaining("[global::System.CodeDom.Compiler.GeneratedCode(\"");

    private async Task EnsureInitializedAsync()
    {
        if (_eventCompiler is null)
        {
            await InitializeAsync().ConfigureAwait(false);
        }
    }
}
