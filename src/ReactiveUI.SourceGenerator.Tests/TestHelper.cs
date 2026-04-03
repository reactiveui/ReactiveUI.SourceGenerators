// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
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
    /// Asynchronously initializes the source generator helper.
    /// </summary>
    /// <returns>A task representing the completed initialization operation.</returns>
    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Tests a generator expecting it to fail by throwing an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="source">The source code to test.</param>
    /// <returns>A task representing the asynchronous assertion operation.</returns>
    public Task TestFail(
        string source)
    {
#pragma warning disable IDE0053 // Use expression body for lambda expression
#pragma warning disable RCS1021 // Convert lambda expression body to expression body
        Assert.Throws<InvalidOperationException>(() => { RunGeneratorAndCheck(source); });
#pragma warning restore RCS1021 // Convert lambda expression body to expression body
#pragma warning restore IDE0053 // Use expression body for lambda expression

        return Task.CompletedTask;
    }

    /// <summary>
    /// Tests a generator expecting it to pass successfully.
    /// </summary>
    /// <param name="source">The source code to test.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    /// <exception cref="InvalidOperationException">Must have valid compiler instance.</exception>
    /// <exception cref="ArgumentNullException">callerType.</exception>
    public Task TestPass(
        string source,
        bool withPreDiagnosics = false)
        => RunGeneratorAndCheck(source, withPreDiagnosics);

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
    /// <exception cref="InvalidOperationException">Thrown if the compilation fails.</exception>
    public SettingsTask RunGeneratorAndCheck(
        string code,
        bool withPreDiagnosics = false,
        bool rerunCompilation = true)
    {
        // Collect required assembly references: runtime assemblies plus a support assembly
        // that provides attribute/enum definitions for generators OTHER than the active generator T.
        // Generator T injects its own definitions via RegisterPostInitializationOutput, so those
        // are excluded from the support assembly to avoid CS0433 duplicate-type errors.
        var assemblies = new HashSet<MetadataReference>(
            TestCompilationReferences.CreateDefault().Concat(CreateSupportReferences()));

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var syntaxTrees = new List<SyntaxTree>
        {
            // Mirror the test project's GlobalUsings.g.cs so test sources can use unqualified
            // attribute names (e.g. [BindableDerivedList]) without an explicit 'using' directive.
            CSharpSyntaxTree.ParseText(
                "global using ReactiveUI.SourceGenerators;",
                parseOptions,
                path: "GlobalUsings.g.cs"),
            CSharpSyntaxTree.ParseText(code, parseOptions),
        };

        // When the active generator is NOT ReactiveGenerator, the shared enum types
        // (AccessModifier, PropertyAccessModifier, InheritanceModifier, SplatRegistrationType)
        // are not injected by any generator but may be referenced by test source code or by the
        // generator's own output.  Add them directly as source trees so they are visible in both
        // the input and output compilations at the correct (non-internal) accessibility level.
        if (typeof(T) != typeof(ReactiveGenerator))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                GetAttributeDefinitionsMethodResult("GetAccessModifierEnum"),
                parseOptions,
                path: "AccessModifierEnum.g.cs"));
        }

        // When the active generator is IViewForGenerator, the [Reactive] and [ReactiveCommand]
        // attributes are not injected (those belong to ReactiveGenerator and ReactiveCommandGenerator).
        // They are also excluded from the support DLL (above), so add them directly as inline source
        // trees — this makes them visible in the test source compilation without CS0122.
        if (typeof(T) == typeof(IViewForGenerator))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                GetAttributeDefinitionsPropertyResult("ReactiveAttribute"),
                parseOptions,
                path: "ReactiveAttribute.g.cs"));
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                GetAttributeDefinitionsPropertyResult("ReactiveCommandAttribute"),
                parseOptions,
                path: "ReactiveCommandAttribute.g.cs"));
        }

        // When the active generator is ReactiveObjectGenerator, [Reactive] and [ObservableAsProperty]
        // attributes are not injected by this generator (they belong to ReactiveGenerator and
        // ObservableAsPropertyGenerator). They are excluded from the support DLL to avoid CS0433,
        // so add them directly as inline source trees for accessibility.
        if (typeof(T) == typeof(ReactiveObjectGenerator))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                GetAttributeDefinitionsPropertyResult("ReactiveAttribute"),
                parseOptions,
                path: "ReactiveAttribute.g.cs"));
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                GetAttributeDefinitionsPropertyResult("ObservableAsPropertyAttribute"),
                parseOptions,
                path: "ObservableAsPropertyAttribute.g.cs"));
        }

        // BindableDerivedListGenerator and ReactiveCollectionGenerator inject their own attribute
        // via RegisterPostInitializationOutput. Tests that also use [Reactive] (WithReactive tests)
        // need ReactiveAttribute as an inline source tree because it is excluded from the support DLL.
        if (typeof(T) == typeof(BindableDerivedListGenerator) || typeof(T) == typeof(ReactiveCollectionGenerator))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                GetAttributeDefinitionsPropertyResult("ReactiveAttribute"),
                parseOptions,
                path: "ReactiveAttribute.g.cs"));
        }

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

            var outputDiagnosticsToReport = outputCompilation.GetDiagnostics()
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .Where(d => !IsKnownExpectedOutputDiagnostic(d))
                .ToList();

            if (outputDiagnosticsToReport.Count > 0)
            {
                var diagnosticMessage = string.Join(Environment.NewLine, outputDiagnosticsToReport.Select(static d => $"{d.Id} - {d.GetMessage()}"));

                foreach (var diagnostic in outputDiagnosticsToReport)
                {
                    WriteTestOutput($"Output diagnostic: {diagnostic.Id} - {diagnostic.GetMessage()}");
                }

                throw new InvalidOperationException($"Output compilation failed due to the above diagnostics.{Environment.NewLine}{diagnosticMessage}");
            }

            // Validate generated code contains expected features
            ValidateGeneratedCode(code, rerunDriver);

            return VerifyGenerator(rerunDriver);
        }

        // If rerun is not needed, simply run the generator.
        return VerifyGenerator(driver.RunGenerators(compilation));
    }

    /// <summary>
    /// Returns all attribute/enum source strings that are NOT already injected by generator T
    /// via RegisterPostInitializationOutput. Including sources that the active generator also
    /// emits would create CS0433 (duplicate type) in the output compilation.
    /// </summary>
    private static IEnumerable<string> GetGeneratedSupportSources()
    {
        // Always include the shared enum block (AccessModifier, PropertyAccessModifier,
        // InheritanceModifier, SplatRegistrationType).  These are internal types so they
        // live inside the support-assembly DLL and never cause CS0433 conflicts, even when
        // ReactiveGenerator also injects them into the test compilation as source.
        // Omitting this block breaks ReactiveCommandAttribute (needs PropertyAccessModifier)
        // and IViewForAttribute (needs SplatRegistrationType).
        yield return GetAttributeDefinitionsMethodResult("GetAccessModifierEnum");

        // Yield each attribute definition only if generator T does NOT inject it.
        // Note: for IViewForGenerator, ReactiveAttribute and ReactiveCommandAttribute are
        // added as inline SyntaxTrees below (not in the support DLL) so they are accessible
        // in the test source compilation without CS0122 internal-visibility errors.
        if (typeof(T) != typeof(ReactiveCommandGenerator) && typeof(T) != typeof(IViewForGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveCommandAttribute");
        }

        if (typeof(T) != typeof(ReactiveGenerator) && typeof(T) != typeof(IViewForGenerator) && typeof(T) != typeof(ReactiveObjectGenerator)
            && typeof(T) != typeof(BindableDerivedListGenerator) && typeof(T) != typeof(ReactiveCollectionGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveAttribute");
        }

        if (typeof(T) != typeof(IViewForGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("IViewForAttribute");
        }

        if (typeof(T) != typeof(ObservableAsPropertyGenerator) && typeof(T) != typeof(ReactiveObjectGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ObservableAsPropertyAttribute");
        }

        if (typeof(T) != typeof(BindableDerivedListGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("BindableDerivedListAttribute");
        }

        if (typeof(T) != typeof(ReactiveCollectionGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveCollectionAttribute");
        }

        if (typeof(T) != typeof(ReactiveObjectGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ReactiveObjectAttribute");
        }

        if (typeof(T) != typeof(RoutedControlHostGenerator))
        {
            yield return GetAttributeDefinitionsMethodResult("GetRoutedControlHostAttribute");
        }

        if (typeof(T) != typeof(ViewModelControlHostGenerator))
        {
            yield return GetAttributeDefinitionsPropertyResult("ViewModelControlHostAttribute");
        }
    }

    private static ImmutableArray<MetadataReference> CreateSupportReferences()
    {
        var supportSources = GetGeneratedSupportSources().ToArray();
        if (supportSources.Length == 0)
        {
            return [];
        }

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var supportCompilation = CSharpCompilation.Create(
            $"{typeof(T).Name}.Support",
            supportSources.Select((source, index) => CSharpSyntaxTree.ParseText(source, parseOptions, path: $"Support{index}.g.cs")),
            TestCompilationReferences.CreateDefault(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        using var stream = new MemoryStream();
        var emitResult = supportCompilation.Emit(stream);

        if (!emitResult.Success)
        {
            var diagnostics = string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static d => d.ToString()));
            throw new InvalidOperationException($"Support assembly compilation failed for {typeof(T).Name}.{Environment.NewLine}{diagnostics}");
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

    private static bool IsKnownExpectedOutputDiagnostic(Diagnostic d) =>
        d.Id is "CS0579" or "CS8864" or "CS0115" or "CS8867" or "CS8866";

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

        if (typeof(T) == typeof(ReactiveCommandGenerator))
        {
            var hasReactiveCommandOutput = generatedTrees.Any(static s => s.HintName.EndsWith(".ReactiveCommands.g.cs", StringComparison.Ordinal));

            if (!hasReactiveCommandOutput)
            {
                WriteTestOutput("=== VALIDATION FAILURE ===");
                WriteTestOutput("ReactiveCommand generator produced no command source output.");
                WriteTestOutput("=== GENERATED HINTS ===");

                foreach (var generatedTree in generatedTrees)
                {
                    WriteTestOutput(generatedTree.HintName);
                }

                WriteTestOutput("=== END ===");

                throw new InvalidOperationException("ReactiveCommand generator produced no command source output.");
            }
        }

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

    private static void WriteTestOutput(string message) => TestContext.Current?.OutputWriter.WriteLine(message);

    private SettingsTask VerifyGenerator(GeneratorDriver driver)
        => Verify(driver)
            .UseDirectory(VerifiedFilePath())
            .ScrubLinesContaining("[global::System.CodeDom.Compiler.GeneratedCode(\"");
    }
