// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>Helper for testing Roslyn analyzers.</summary>
public static class AnalyzerTestHelper
{
    /// <summary>Runs an analyzer on the provided source code and returns diagnostics.</summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to run.</typeparam>
    /// <param name="source">The source code to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the diagnostics.</returns>
    [SuppressMessage("Design", "SST2307:Type parameter is not inferable", Justification = "the analyzer under test is specified explicitly by the caller")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        GetDiagnosticsAsync<TAnalyzer>(source, (LanguageVersion?)null, null);

    /// <summary>
    /// Runs an analyzer against source compiled at a given language version, with the root namespace reported
    /// the way a real build reports it.
    /// </summary>
    /// <typeparam name="TAnalyzer">The analyzer to run.</typeparam>
    /// <param name="source">The source to analyze.</param>
    /// <param name="languageVersion">The language version to compile at, or null for the default.</param>
    /// <param name="rootNamespace">The root namespace the build exposes, or null for none.</param>
    /// <returns>The analyzer diagnostics.</returns>
    [SuppressMessage("Design", "SST2307:Type parameter is not inferable", Justification = "the analyzer under test is specified explicitly by the caller")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(
        string source,
        LanguageVersion? languageVersion,
        string? rootNamespace)
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        GetDiagnosticsAsync<TAnalyzer>(source, ParseOptionsFor(languageVersion), rootNamespace);

    /// <summary>
    /// Runs an analyzer against source parsed exactly as the caller asks, which is how a scenario reaches the
    /// options a language version alone cannot express.
    /// </summary>
    /// <typeparam name="TAnalyzer">The analyzer to run.</typeparam>
    /// <param name="source">The source to analyze.</param>
    /// <param name="parseOptions">The options the source is parsed with, or null for the default.</param>
    /// <param name="rootNamespace">The root namespace the build exposes, or null for none.</param>
    /// <returns>The analyzer diagnostics.</returns>
    [SuppressMessage("Design", "SST2307:Type parameter is not inferable", Justification = "the analyzer under test is specified explicitly by the caller")]
    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(
        string source,
        CSharpParseOptions? parseOptions,
        string? rootNamespace)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CreateCompilation(source, parseOptions);
        var analyzer = new TAnalyzer();

        AnalyzerOptions? analyzerOptions = rootNamespace is null
            ? null
            : new([], new RootNamespaceOptionsProvider(rootNamespace));

        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer], analyzerOptions);

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        // Filter to only analyzer diagnostics (exclude compiler errors)
        return
        [
            ..diagnostics
                .Where(d => analyzer.SupportedDiagnostics.Any(sd => sd.Id == d.Id))
        ];
    }

    /// <summary>The parse options a build produces from a language version, which is what the tests usually name.</summary>
    /// <param name="languageVersion">The language version, or null for the default.</param>
    /// <returns>The parse options, or null for the default.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CSharpParseOptions? ParseOptionsFor(LanguageVersion? languageVersion) =>
        languageVersion.HasValue ? new CSharpParseOptions(languageVersion.Value) : null;

    /// <summary>
    /// The parse options of a build that has the package's targets on it, which list the generated namespace so
    /// the compiler honours an interceptor emitted into it.
    /// </summary>
    /// <param name="languageVersion">The language version, or null for the default.</param>
    /// <returns>The parse options, carrying the opt-in.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CSharpParseOptions InterceptingParseOptionsFor(LanguageVersion? languageVersion) =>
        (ParseOptionsFor(languageVersion) ?? new CSharpParseOptions())
            .WithFeatures([new KeyValuePair<string, string>(
                SourceGenerators.Constants.InterceptorsNamespacesFeature,
                SourceGenerators.Constants.InterceptorNamespace)]);

    /// <summary>Creates a CSharpCompilation from the specified source code with required assembly references.</summary>
    /// <param name="source">The source code to compile into a CSharpCompilation.</param>
    /// <returns>A CSharpCompilation object representing the compiled source code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CSharpCompilation CreateCompilation(string source) => CreateCompilation(source, (LanguageVersion?)null);

    /// <summary>Creates a compilation from source at a given language version.</summary>
    /// <param name="source">The source to compile.</param>
    /// <param name="languageVersion">The language version, or null for the default.</param>
    /// <returns>The compilation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CSharpCompilation CreateCompilation(string source, LanguageVersion? languageVersion) =>
        CreateCompilation(source, ParseOptionsFor(languageVersion));

    /// <summary>Creates a compilation from source parsed exactly as the caller asks.</summary>
    /// <param name="source">The source to compile.</param>
    /// <param name="parseOptions">The options the source is parsed with, or null for the default.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation CreateCompilation(string source, CSharpParseOptions? parseOptions)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        var addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = new List<MetadataReference>();

        void AddReference(Assembly assembly)
        {
            if (assembly.IsDynamic || !addedPaths.Add(assembly.Location))
            {
                return;
            }

            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        // Add core framework references via typeof to ensure assemblies are loaded
        AddReference(typeof(object).Assembly);
        AddReference(typeof(Enumerable).Assembly);
        AddReference(typeof(Attribute).Assembly);
        AddReference(typeof(Expression<>).Assembly);
        AddReference(typeof(System.ComponentModel.INotifyPropertyChanged).Assembly);
        AddReference(typeof(System.ComponentModel.INotifyPropertyChanging).Assembly);
        AddReference(typeof(System.ComponentModel.INotifyDataErrorInfo).Assembly);

        // Add ReactiveUI reference
        AddReference(typeof(IReactiveObject).Assembly);

        // Add runtime assemblies by name for any that typeof didn't cover
        var assemblyNames = new[]
        {
            "System.Runtime", "System.ComponentModel.Primitives", "System.ObjectModel", "System.Collections",
        };

        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var name in assemblyNames)
        {
            var asm = System.Array.Find(loadedAssemblies, a => a.GetName().Name == name);
            if (asm is not null)
            {
                AddReference(asm);
            }
        }

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
    }

    /// <summary>Reports a root namespace to an analyzer the way a real build does.</summary>
    /// <param name="rootNamespace">The root namespace to report.</param>
    private sealed class RootNamespaceOptionsProvider(string rootNamespace) : AnalyzerConfigOptionsProvider
    {
        /// <summary>The analyzer config key a build exposes the root namespace under.</summary>
        private const string RootNamespaceKey = "build_property.RootNamespace";

        /// <summary>The shared empty option map.</summary>
        private static readonly Dictionary<string, string> EmptyOptions = [with(StringComparer.Ordinal)];

        /// <summary>The per-file options, which nothing under test reads.</summary>
        private static readonly FixedOptions NoOptions = new(EmptyOptions);

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GlobalOptions { get; } = new FixedOptions(
            new Dictionary<string, string>(StringComparer.Ordinal) { [RootNamespaceKey] = rootNamespace });

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => NoOptions;

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => NoOptions;
    }

    /// <summary>A fixed set of analyzer config options.</summary>
    /// <param name="options">The options to expose.</param>
    private sealed class FixedOptions(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptions
    {
        /// <inheritdoc/>
        public override IEnumerable<string> Keys => options.Keys;

        /// <inheritdoc/>
        public override bool TryGetValue(string key, out string value)
        {
            if (options.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
