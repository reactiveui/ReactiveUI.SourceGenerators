// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Text;
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
    /// <summary>The Reactive attribute definition property name.</summary>
    private const string ReactiveAttributeName = "ReactiveAttribute";

    /// <summary>The generated hint name for the Reactive attribute.</summary>
    private const string ReactiveAttributeHintName = "ReactiveAttribute.g.cs";

    /// <summary>The fully qualified type name containing attribute definitions.</summary>
    private const string AttributeDefinitionsTypeName = "ReactiveUI.SourceGenerators.Helpers.AttributeDefinitions";

    /// <summary>
    /// Cache support references per generator type T.  The support assembly compiles attribute
    /// definitions that are NOT injected by T via RegisterPostInitializationOutput — an expensive
    /// Roslyn compilation + Emit step that produces an identical result for every test in the same
    /// generator class.  Compute it once and reuse it for all subsequent tests.
    /// </summary>
    private static readonly Lazy<ImmutableArray<MetadataReference>> supportReferences =
        new(CreateSupportReferences, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>The concrete generator type name used for snapshots and support assemblies.</summary>
    private static readonly string generatorTypeName = new T().GetType().Name;

    /// <summary>Gets the verified file path for generator type <typeparamref name="T"/>.</summary>
    /// <returns>
    /// A string.
    /// </returns>
    public string VerifiedFilePath() => GetVerifiedFilePath();

    /// <summary>Asynchronously initializes the source generator helper.</summary>
    /// <returns>A task representing the completed initialization operation.</returns>
    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>Tests a generator expecting it to fail by throwing an <see cref="InvalidOperationException"/>.</summary>
    /// <param name="source">The source code to test.</param>
    /// <returns>A task representing the asynchronous assertion operation.</returns>
    public async Task TestFail(string source) =>
        await Assert.That(() => RunGeneratorAndCheck(source)).Throws<InvalidOperationException>();

    /// <summary>Tests a generator expecting it to pass successfully.</summary>
    /// <param name="source">The source code to test.</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    public Task TestPass(string source) =>
        TestPass(source, withPreDiagnosics: false);

    /// <summary>Tests a generator expecting it to pass successfully.</summary>
    /// <param name="source">The source code to test.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    public Task TestPass(string source, bool withPreDiagnosics) =>
        RunGeneratorAndCheck(source, withPreDiagnosics);

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <summary>Runs the specified source generator and validates the generated code.</summary>
    /// <param name="code">The code to be parsed and processed by the generator.</param>
    /// <returns>The generator driver used to run the generator.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the compilation fails.</exception>
    public SettingsTask RunGeneratorAndCheck(string code) =>
        RunGeneratorAndCheck(code, withPreDiagnosics: false, rerunCompilation: true);

    /// <summary>Runs the specified source generator and validates the generated code.</summary>
    /// <param name="code">The code to be parsed and processed by the generator.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <returns>The generator driver used to run the generator.</returns>
    public SettingsTask RunGeneratorAndCheck(string code, bool withPreDiagnosics) =>
        RunGeneratorAndCheck(code, withPreDiagnosics, rerunCompilation: true);

    /// <summary>Runs the specified source generator and validates the generated code.</summary>
    /// <param name="code">The code to be parsed and processed by the generator.</param>
    /// <param name="withPreDiagnosics">if set to <c>true</c> [with pre diagnosics].</param>
    /// <param name="rerunCompilation">Indicates whether to rerun the compilation after running the generator.</param>
    /// <returns>The generator driver used to run the generator.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the compilation fails.</exception>
    public SettingsTask RunGeneratorAndCheck(
        string code,
        bool withPreDiagnosics,
        bool rerunCompilation)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CreateTestCompilation(code, parseOptions);

        if (withPreDiagnosics)
        {
            // Validate diagnostics before running the generator.
            var prediagnostics = GetDiagnosticsAboveSeverity(compilation.GetDiagnostics(), DiagnosticSeverity.Warning);

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

        return rerunCompilation
            ? RunGeneratorAndVerify(code, driver, compilation)
            : VerifyGenerator(driver.RunGenerators(compilation));
    }

    /// <summary>Gets the verified file path for generator type <typeparamref name="T"/>.</summary>
    /// <returns>The snapshot directory name.</returns>
    private static string GetVerifiedFilePath()
    {
        var name = generatorTypeName;
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

    /// <summary>Creates an in-memory compilation containing the supplied test code.</summary>
    /// <param name="code">The test code to compile.</param>
    /// <param name="parseOptions">The language version settings.</param>
    /// <returns>The prepared Roslyn compilation.</returns>
    private static CSharpCompilation CreateTestCompilation(string code, CSharpParseOptions parseOptions)
    {
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText("global using ReactiveUI.SourceGenerators;", parseOptions, path: "GlobalUsings.g.cs"),
            CSharpSyntaxTree.ParseText(code, parseOptions),
        };

        AddGeneratorSpecificSyntaxTrees(syntaxTrees, parseOptions);
        AddWindowsDesktopStubsWhenNeeded(syntaxTrees, parseOptions);
        return CSharpCompilation.Create("TestProject", syntaxTrees, CreateAssemblyReferences(), new(OutputKind.DynamicallyLinkedLibrary, deterministic: true));
    }

    /// <summary>Adds source trees required by the active generator.</summary>
    /// <param name="syntaxTrees">The source-tree collection to extend.</param>
    /// <param name="parseOptions">The language version settings.</param>
    private static void AddGeneratorSpecificSyntaxTrees(List<SyntaxTree> syntaxTrees, CSharpParseOptions parseOptions)
    {
        if (typeof(T) != typeof(ReactiveGenerator))
        {
            AddSyntaxTree(syntaxTrees, GetAttributeDefinitionsMethodResult("GetAccessModifierEnum"), parseOptions, "AccessModifierEnum.g.cs");
        }

        if (typeof(T) == typeof(IViewForGenerator))
        {
            AddSyntaxTree(syntaxTrees, GetAttributeDefinitionsPropertyResult(ReactiveAttributeName), parseOptions, ReactiveAttributeHintName);
            AddSyntaxTree(syntaxTrees, GetAttributeDefinitionsPropertyResult("ReactiveCommandAttribute"), parseOptions, "ReactiveCommandAttribute.g.cs");
        }

        if (typeof(T) == typeof(ReactiveObjectGenerator))
        {
            AddSyntaxTree(syntaxTrees, GetAttributeDefinitionsPropertyResult(ReactiveAttributeName), parseOptions, ReactiveAttributeHintName);
            AddSyntaxTree(syntaxTrees, GetAttributeDefinitionsPropertyResult("ObservableAsPropertyAttribute"), parseOptions, "ObservableAsPropertyAttribute.g.cs");
        }

        if (typeof(T) != typeof(BindableDerivedListGenerator) && typeof(T) != typeof(ReactiveCollectionGenerator))
        {
            return;
        }

        AddSyntaxTree(syntaxTrees, GetAttributeDefinitionsPropertyResult(ReactiveAttributeName), parseOptions, ReactiveAttributeHintName);
    }

    /// <summary>Adds Windows desktop stubs when the operating system does not provide them.</summary>
    /// <param name="syntaxTrees">The source-tree collection to extend.</param>
    /// <param name="parseOptions">The language version settings.</param>
    private static void AddWindowsDesktopStubsWhenNeeded(List<SyntaxTree> syntaxTrees, CSharpParseOptions parseOptions)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        AddSyntaxTree(syntaxTrees, TestCompilationReferences.WindowsDesktopStubs, parseOptions, "WindowsDesktopStubs.g.cs");
    }

    /// <summary>Adds a parsed source tree to a syntax-tree collection.</summary>
    /// <param name="syntaxTrees">The source-tree collection to extend.</param>
    /// <param name="source">The source code to parse.</param>
    /// <param name="parseOptions">The language version settings.</param>
    /// <param name="path">The generated source path.</param>
    private static void AddSyntaxTree(List<SyntaxTree> syntaxTrees, string source, CSharpParseOptions parseOptions, string path) =>
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(source, parseOptions, path: path));

    /// <summary>Runs a generator and validates all resulting diagnostics and source.</summary>
    /// <param name="code">The original source code.</param>
    /// <param name="driver">The configured generator driver.</param>
    /// <param name="compilation">The input compilation.</param>
    /// <returns>The snapshot verification settings.</returns>
    private static SettingsTask RunGeneratorAndVerify(string code, GeneratorDriver driver, Compilation compilation)
    {
        var rerunDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        ThrowIfDiagnosticsExist(GetDiagnosticsAtLeastSeverity(diagnostics, DiagnosticSeverity.Warning), "Diagnostic", "Compilation failed due to the above diagnostics.");
        ThrowIfDiagnosticsExist(GetUnexpectedOutputDiagnostics(outputCompilation.GetDiagnostics()), "Output diagnostic", "Output compilation failed due to the above diagnostics.");
        ValidateGeneratedCode(code, rerunDriver);
        return VerifyGenerator(rerunDriver);
    }

    /// <summary>Writes diagnostics and throws when a collection is non-empty.</summary>
    /// <param name="diagnostics">The diagnostics to evaluate.</param>
    /// <param name="prefix">The output prefix.</param>
    /// <param name="failureMessage">The exception message prefix.</param>
    private static void ThrowIfDiagnosticsExist(List<Diagnostic> diagnostics, string prefix, string failureMessage)
    {
        if (diagnostics.Count == 0)
        {
            return;
        }

        foreach (var diagnostic in diagnostics)
        {
            WriteTestOutput($"{prefix}: {diagnostic.Id} - {diagnostic.GetMessage()}");
        }

        throw new InvalidOperationException($"{failureMessage}{Environment.NewLine}{CreateDiagnosticMessage(diagnostics)}");
    }

    /// <summary>Returns attribute and enum source strings not injected by generator <typeparamref name="T"/>.</summary>
    /// <returns>The source strings required by the support assembly.</returns>
    private static List<string> GetGeneratedSupportSources()
    {
        var supportSources = new List<string> { GetAttributeDefinitionsMethodResult("GetAccessModifierEnum") };

        AddRequiredAttributeDefinitions(supportSources);
        return supportSources;
    }

    /// <summary>Adds attribute definitions required before the common definitions.</summary>
    /// <param name="supportSources">The support-source collection to extend.</param>
    private static void AddRequiredAttributeDefinitions(List<string> supportSources)
    {
        // Yield each attribute definition only if generator T does NOT inject it.
        // Note: for IViewForGenerator, ReactiveAttribute and ReactiveCommandAttribute are
        // added as inline SyntaxTrees below (not in the support DLL) so they are accessible
        // in the test source compilation without CS0122 internal-visibility errors.
        if (typeof(T) != typeof(ReactiveCommandGenerator) && typeof(T) != typeof(IViewForGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsPropertyResult("ReactiveCommandAttribute"));
        }

        AddReactiveAttributeDefinitionIfNeeded(supportSources);

        if (typeof(T) != typeof(IViewForGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsPropertyResult("IViewForAttribute"));
        }

        if (typeof(T) != typeof(ObservableAsPropertyGenerator) && typeof(T) != typeof(ReactiveObjectGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsPropertyResult("ObservableAsPropertyAttribute"));
        }

        AddRemainingAttributeDefinitions(supportSources);
    }

    /// <summary>Adds the Reactive attribute definition when the active generator does not provide it.</summary>
    /// <param name="supportSources">The support-source collection to extend.</param>
    private static void AddReactiveAttributeDefinitionIfNeeded(List<string> supportSources)
    {
        if (typeof(T) == typeof(ReactiveGenerator) || typeof(T) == typeof(IViewForGenerator) || typeof(T) == typeof(ReactiveObjectGenerator)
            || typeof(T) == typeof(BindableDerivedListGenerator) || typeof(T) == typeof(ReactiveCollectionGenerator))
        {
            return;
        }

        supportSources.Add(GetAttributeDefinitionsPropertyResult(ReactiveAttributeName));
    }

    /// <summary>Adds the remaining conditional attribute definitions.</summary>
    /// <param name="supportSources">The support-source collection to extend.</param>
    private static void AddRemainingAttributeDefinitions(List<string> supportSources)
    {
        if (typeof(T) != typeof(BindableDerivedListGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsPropertyResult("BindableDerivedListAttribute"));
        }

        if (typeof(T) != typeof(ReactiveCollectionGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsPropertyResult("ReactiveCollectionAttribute"));
        }

        if (typeof(T) != typeof(ReactiveObjectGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsPropertyResult("ReactiveObjectAttribute"));
        }

        if (typeof(T) != typeof(RoutedControlHostGenerator))
        {
            supportSources.Add(GetAttributeDefinitionsMethodResult("GetRoutedControlHostAttribute"));
        }

        if (typeof(T) == typeof(ViewModelControlHostGenerator))
        {
            return;
        }

        supportSources.Add(GetAttributeDefinitionsPropertyResult("ViewModelControlHostAttribute"));
    }

    /// <summary>Creates metadata references for the support source assembly.</summary>
    /// <returns>The references that provide support attributes and enums.</returns>
    private static ImmutableArray<MetadataReference> CreateSupportReferences()
    {
        var supportSources = GetGeneratedSupportSources();

        if (supportSources.Count == 0)
        {
            return [];
        }

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var supportCompilation = CSharpCompilation.Create(
            $"{generatorTypeName}.Support",
            CreateSupportSyntaxTrees(supportSources, parseOptions),
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        using var stream = new MemoryStream();
        var emitResult = supportCompilation.Emit(stream);

        if (!emitResult.Success)
        {
            var diagnostics = CreateDiagnosticMessage(emitResult.Diagnostics);
            throw new InvalidOperationException($"Support assembly compilation failed for {generatorTypeName}.{Environment.NewLine}{diagnostics}");
        }

        return [MetadataReference.CreateFromImage(stream.ToArray())];
    }

    /// <summary>Creates the metadata references used by an in-memory test compilation.</summary>
    /// <returns>The default and generator support metadata references.</returns>
    private static HashSet<MetadataReference> CreateAssemblyReferences()
    {
        var references = new HashSet<MetadataReference>(TestCompilationReferences.CreateDefault());
        references.UnionWith(supportReferences.Value);
        return references;
    }

    /// <summary>Creates syntax trees for the supplied support source strings.</summary>
    /// <param name="supportSources">The support source strings.</param>
    /// <param name="parseOptions">The language version settings.</param>
    /// <returns>The parsed support syntax trees.</returns>
    private static List<SyntaxTree> CreateSupportSyntaxTrees(List<string> supportSources, CSharpParseOptions parseOptions)
    {
        var syntaxTrees = new List<SyntaxTree>(supportSources.Count);
        for (var index = 0; index < supportSources.Count; index++)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(supportSources[index], parseOptions, path: $"Support{index}.g.cs"));
        }

        return syntaxTrees;
    }

    /// <summary>Gets a public attribute-definition method result.</summary>
    /// <param name="methodName">The public static method name.</param>
    /// <returns>The generated source returned by the method.</returns>
    private static string GetAttributeDefinitionsMethodResult(string methodName)
    {
        var attributeDefinitionsType = typeof(ReactiveGenerator).Assembly.GetType(AttributeDefinitionsTypeName, throwOnError: false, ignoreCase: false)
            ?? throw new InvalidOperationException("Could not locate AttributeDefinitions type.");

        var method = attributeDefinitionsType.GetMethod(methodName)
            ?? throw new InvalidOperationException($"Could not locate AttributeDefinitions.{methodName}.");

        var result = method.Invoke(null, null);
        return (string?)result
            ?? throw new InvalidOperationException($"AttributeDefinitions.{methodName} returned null.");
    }

    /// <summary>Gets a public attribute-definition property result.</summary>
    /// <param name="propertyName">The public static property name.</param>
    /// <returns>The generated source returned by the property.</returns>
    private static string GetAttributeDefinitionsPropertyResult(string propertyName)
    {
        var attributeDefinitionsType = typeof(ReactiveGenerator).Assembly.GetType(AttributeDefinitionsTypeName, throwOnError: false, ignoreCase: false)
            ?? throw new InvalidOperationException("Could not locate AttributeDefinitions type.");

        var property = attributeDefinitionsType.GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Could not locate AttributeDefinitions.{propertyName}.");

        return (string?)property.GetValue(null)
            ?? throw new InvalidOperationException($"AttributeDefinitions.{propertyName} returned null.");
    }

    /// <summary>Determines whether a diagnostic is an accepted generated-output diagnostic.</summary>
    /// <param name="d">The diagnostic to inspect.</param>
    /// <returns><see langword="true"/> when the diagnostic is expected.</returns>
    private static bool IsKnownExpectedOutputDiagnostic(Diagnostic d) =>
        d.Id is "CS0579" or "CS8864" or "CS0115" or "CS8867" or "CS8866";

    /// <summary>Filters diagnostics more severe than a specified level.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="severity">The exclusive severity threshold.</param>
    /// <returns>Diagnostics exceeding the threshold.</returns>
    private static List<Diagnostic> GetDiagnosticsAboveSeverity(IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity)
    {
        var result = new List<Diagnostic>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity > severity)
            {
                result.Add(diagnostic);
            }
        }

        return result;
    }

    /// <summary>Filters diagnostics at or above a specified severity level.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="severity">The inclusive severity threshold.</param>
    /// <returns>Diagnostics meeting the threshold.</returns>
    private static List<Diagnostic> GetDiagnosticsAtLeastSeverity(IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity)
    {
        var result = new List<Diagnostic>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity >= severity)
            {
                result.Add(diagnostic);
            }
        }

        return result;
    }

    /// <summary>Filters error diagnostics that are not expected generator output diagnostics.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <returns>Unexpected error diagnostics.</returns>
    private static List<Diagnostic> GetUnexpectedOutputDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        var result = new List<Diagnostic>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity >= DiagnosticSeverity.Error && !IsKnownExpectedOutputDiagnostic(diagnostic))
            {
                result.Add(diagnostic);
            }
        }

        return result;
    }

    /// <summary>Formats diagnostics for test-output reporting.</summary>
    /// <param name="diagnostics">The diagnostics to format.</param>
    /// <returns>A newline-delimited diagnostic message.</returns>
    private static string CreateDiagnosticMessage(IEnumerable<Diagnostic> diagnostics)
    {
        var messages = new List<string>();
        foreach (var diagnostic in diagnostics)
        {
            messages.Add($"{diagnostic.Id} - {diagnostic.GetMessage()}");
        }

        return string.Join(Environment.NewLine, messages);
    }

    [GeneratedRegex(@"\[Reactive\((?:.*?nameof\((\w+)\))+", RegexOptions.Singleline)]
    private static partial Regex ReactiveRegex();

    [GeneratedRegex(@"nameof\((\w+)\)")]
    private static partial Regex NameOfRegex();

    /// <summary>Validates that generated code contains expected features based on the source code attributes.</summary>
    /// <param name="sourceCode">The original source code.</param>
    /// <param name="driver">The generator driver with generated output.</param>
    private static void ValidateGeneratedCode(string sourceCode, GeneratorDriver driver)
    {
        var runResult = driver.GetRunResult();
        var generatedTrees = GetGeneratedSources(runResult);
        var allGeneratedCode = GetGeneratedCode(generatedTrees);

        ValidateReactiveCommandOutput(generatedTrees);
        ValidateAlsoNotifyOutput(sourceCode, allGeneratedCode);
    }

    /// <summary>Gets every generated source from a generator run result.</summary>
    /// <param name="runResult">The result to inspect.</param>
    /// <returns>The generated sources from all generators.</returns>
    private static List<GeneratedSourceResult> GetGeneratedSources(GeneratorDriverRunResult runResult)
    {
        var generatedSources = new List<GeneratedSourceResult>();
        foreach (var result in runResult.Results)
        {
            generatedSources.AddRange(result.GeneratedSources);
        }

        return generatedSources;
    }

    /// <summary>Combines generated source text for validation.</summary>
    /// <param name="generatedSources">The sources to combine.</param>
    /// <returns>The combined generated source code.</returns>
    private static string GetGeneratedCode(List<GeneratedSourceResult> generatedSources)
    {
        var generatedCode = new StringBuilder();
        foreach (var generatedSource in generatedSources)
        {
            _ = generatedCode.AppendLine(generatedSource.SourceText.ToString());
        }

        return generatedCode.ToString();
    }

    /// <summary>Ensures the command generator emits its command source.</summary>
    /// <param name="generatedSources">The generated sources to inspect.</param>
    private static void ValidateReactiveCommandOutput(List<GeneratedSourceResult> generatedSources)
    {
        if (typeof(T) != typeof(ReactiveCommandGenerator))
        {
            return;
        }

        foreach (var generatedSource in generatedSources)
        {
            if (generatedSource.HintName.EndsWith(".ReactiveCommands.g.cs", StringComparison.Ordinal))
            {
                return;
            }
        }

        WriteTestOutput("=== VALIDATION FAILURE ===");
        WriteTestOutput("ReactiveCommand generator produced no command source output.");
        WriteTestOutput("=== GENERATED HINTS ===");
        foreach (var generatedSource in generatedSources)
        {
            WriteTestOutput(generatedSource.HintName);
        }

        WriteTestOutput("=== END ===");
        throw new InvalidOperationException("ReactiveCommand generator produced no command source output.");
    }

    /// <summary>Ensures reactive attributes produce their requested additional notifications.</summary>
    /// <param name="sourceCode">The original source code.</param>
    /// <param name="allGeneratedCode">The combined generated code.</param>
    private static void ValidateAlsoNotifyOutput(string sourceCode, string allGeneratedCode)
    {
        foreach (object? matchValue in ReactiveRegex().Matches(sourceCode))
        {
            if (matchValue is not Match match)
            {
                continue;
            }

            foreach (object? nameofMatchValue in NameOfRegex().Matches(match.Value))
            {
                if (nameofMatchValue is not Match nameofMatch)
                {
                    continue;
                }

                ValidatePropertyNotification(nameofMatch.Groups[1].Value, match.Value, allGeneratedCode);
            }
        }
    }

    /// <summary>Ensures generated code raises a requested property notification.</summary>
    /// <param name="propertyToNotify">The requested property name.</param>
    /// <param name="sourceAttribute">The source attribute that requested it.</param>
    /// <param name="allGeneratedCode">The combined generated code.</param>
    private static void ValidatePropertyNotification(string propertyToNotify, string sourceAttribute, string allGeneratedCode)
    {
        if (ContainsPropertyNotification(allGeneratedCode, propertyToNotify))
        {
            return;
        }

        var errorMessage = $"Generated code does not include AlsoNotify for property '{propertyToNotify}'. "
            + $"Expected to find property change notification for '{propertyToNotify}' in the generated code.{Environment.NewLine}"
            + $"Source attribute: {sourceAttribute}";
        WriteTestOutput("=== VALIDATION FAILURE ===");
        WriteTestOutput(errorMessage);
        WriteTestOutput("=== SOURCE CODE SNIPPET ===");
        WriteTestOutput(sourceAttribute);
        WriteTestOutput("=== GENERATED CODE ===");
        WriteTestOutput(allGeneratedCode);
        WriteTestOutput("=== END ===");
        throw new InvalidOperationException(errorMessage);
    }

    /// <summary>Determines whether generated code contains a requested notification.</summary>
    /// <param name="generatedCode">The generated code.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns><see langword="true"/> when the notification appears.</returns>
    private static bool ContainsPropertyNotification(string generatedCode, string propertyName) =>
        generatedCode.Contains($"this.RaisePropertyChanged(nameof({propertyName}))", StringComparison.Ordinal)
        || generatedCode.Contains($"this.RaisePropertyChanged(\"{propertyName}\")", StringComparison.Ordinal)
        || generatedCode.Contains($"RaisePropertyChanged(nameof({propertyName}))", StringComparison.Ordinal)
        || generatedCode.Contains($"RaisePropertyChanged(\"{propertyName}\")", StringComparison.Ordinal);

    /// <summary>Writes a validation message to the current test output.</summary>
    /// <param name="message">The message to write.</param>
    private static void WriteTestOutput(string message) => TestContext.Current?.OutputWriter.WriteLine(message);

    /// <summary>Creates snapshot verification settings for a generator driver.</summary>
    /// <param name="driver">The generator driver to verify.</param>
    /// <returns>The snapshot verification settings.</returns>
    private static SettingsTask VerifyGenerator(GeneratorDriver driver) =>
        Verify(driver)
            .UseDirectory(GetVerifiedFilePath())
            .ScrubLinesContaining("[global::System.CodeDom.Compiler.GeneratedCode(\"");
    }
