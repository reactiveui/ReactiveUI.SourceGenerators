# CLAUDE.md — ReactiveUI.SourceGenerators

This document provides guidance for AI assistants and contributors working in this repository.

## Overview

ReactiveUI.SourceGenerators is a Roslyn incremental source-generator package that automates ReactiveUI boilerplate at compile-time. It generates reactive properties, observable-as-property helpers, reactive commands, IViewFor registrations, bindable derived lists, reactive collections, and full reactive-object scaffolding — all with zero runtime reflection, making generated code fully AOT-compatible.

**Minimum consumer requirements:** C# 12.0 · Visual Studio 17.8.0 · ReactiveUI 19.5.31+

## Architecture Overview

The repository ships **three versioned generator assemblies** built from a single shared source folder:

| Project | Roslyn version | Preprocessor constant | Extra features |
|---------|---------------|-----------------------|----------------|
| `ReactiveUI.SourceGenerators.Roslyn480` | 4.8.x (baseline) | _(none)_ | Field-based `[Reactive]`, `[ObservableAsProperty]`, `[ReactiveCommand]`, etc. |
| `ReactiveUI.SourceGenerators.Roslyn4120` | 4.12.0 | `ROSYLN_412` | + partial-property `[Reactive]` and `[ObservableAsProperty]` |
| `ReactiveUI.SourceGenerators.Roslyn5000` | 5.0.0 | `ROSYLN_500` | + same partial-property support on Roslyn 5 |

Each versioned project links all `.cs` files from `ReactiveUI.SourceGenerators.Roslyn/` via:

```xml
<Compile Include="..\ReactiveUI.SourceGenerators.Roslyn\**\*.cs" LinkBase="Shared" />
```

`#if ROSYLN_412 || ROSYLN_500` guards inside the shared source enable partial-property pipelines only on the newer Roslyn builds.

The `ReactiveUI.SourceGenerators` NuGet project packages all three DLLs under separate `analyzers/dotnet/roslyn4.8/cs`, `analyzers/dotnet/roslyn4.12/cs`, and `analyzers/dotnet/roslyn5.0/cs` paths, so NuGet/MSBuild automatically selects the right build based on the host compiler.

Diagnostics are **not** reported by generators. All `RXUISG*` diagnostics live in the separate `ReactiveUI.SourceGenerators.Analyzers.CodeFixes` project.

## Project Structure

```
src/
├── ReactiveUI.SourceGenerators.Roslyn/          # Shared source (linked into all versioned projects)
│   ├── AttributeDefinitions.cs                  # Injected attribute source texts
│   ├── Reactive/                                # [Reactive] generator + Execute + models
│   ├── ReactiveCommand/                         # [ReactiveCommand] generator + Execute + models
│   ├── ObservableAsProperty/                    # [ObservableAsProperty] generator + Execute + models
│   ├── IViewFor/                                # [IViewFor<T>] generator + Execute + models
│   ├── RoutedControlHost/                       # [RoutedControlHost] generator
│   ├── ViewModelControlHost/                    # [ViewModelControlHost] generator
│   ├── BindableDerivedList/                     # [BindableDerivedList] generator
│   ├── ReactiveCollection/                      # [ReactiveCollection] generator
│   ├── ReactiveObject/                          # [IReactiveObject] generator
│   ├── Diagnostics/                             # DiagnosticDescriptors, SuppressionDescriptors
│   └── Core/
│       ├── Extensions/                          # ISymbol*, ITypeSymbol*, INamedTypeSymbol*, AttributeData extensions
│       ├── Helpers/                             # ImmutableArrayBuilder<T>, EquatableArray<T>, HashCode, etc.
│       └── Models/                              # Result<T>, DiagnosticInfo, TargetInfo, etc.
├── ReactiveUI.SourceGenerators.Roslyn480/       # Roslyn 4.8 build (no define)
├── ReactiveUI.SourceGenerators.Roslyn4120/      # Roslyn 4.12 build (ROSYLN_412)
├── ReactiveUI.SourceGenerators.Roslyn5000/      # Roslyn 5.0 build (ROSYLN_500)
├── ReactiveUI.SourceGenerators.Analyzers.CodeFixes/  # Analyzers + code fixers
├── ReactiveUI.SourceGenerators/                 # NuGet packaging project (bundles all three DLLs)
├── ReactiveUI.SourceGenerator.Tests/            # TUnit + Verify snapshot tests
├── ReactiveUI.SourceGenerators.Execute*/        # Compile-time execution verification projects
└── TestApps/                                    # Manual test applications (WPF, WinForms, MAUI, Avalonia)
```

## Code Generation Strategy

All generated C# source is produced using **raw string literals** (`$$"""..."""`). Do **not** use `StringBuilder` or `SyntaxFactory` for code generation.

```csharp
// CORRECT — raw string literal with $$ interpolation
internal static string GenerateProperty(string name, string type) => $$"""
    public {{type}} {{name}}
    {
        get => _{{char.ToLower(name[0])}{{name.Substring(1)}}};
        set => this.RaiseAndSetIfChanged(ref _{{char.ToLower(name[0])}{{name.Substring(1)}}}, value);
    }
    """;

// WRONG — do not use StringBuilder
var sb = new StringBuilder();
sb.AppendLine($"public {type} {name}");
// ...

// WRONG — do not use SyntaxFactory
SyntaxFactory.PropertyDeclaration(...)
```

Raw string literals preserve formatting intent, are trivially diffable in code review, and do not require the overhead of SyntaxFactory node construction.

The injected attribute source texts (in `AttributeDefinitions.cs`) also use `$$"""..."""` raw string literals.

## Roslyn Incremental Pipeline Pattern

Each generator follows this structure:

1. **`Initialize`** — registers post-initialization output (inject attribute source), then calls one or more `Run*` methods.
2. **`Run*`** — builds the `IncrementalValuesProvider` using `ForAttributeWithMetadataName` + a syntax predicate + a semantic extraction function.
3. **`Get*Info` (Execute file)** — stateless extraction function. Returns `Result<TModel?>` with embedded diagnostics. Must be pure; must not capture any `ISymbol` or `SyntaxNode` beyond this call.
4. **`GenerateSource` (Execute file)** — pure function that converts model → raw string source text. No Roslyn symbols allowed here.

```
Initialize()
  ├─ RegisterPostInitializationOutput → inject attribute definitions
  └─ SyntaxProvider.ForAttributeWithMetadataName
       ├─ syntax predicate (fast, node-type check only)
       ├─ semantic extraction → Get*Info() → Result<Model>
       └─ RegisterSourceOutput → GenerateSource() → AddSource()
```

**Incremental caching rules:**
- All pipeline output models must implement value equality (`record`, `IEquatable<T>`, or `EquatableArray<T>`).
- Never store `ISymbol`, `SyntaxNode`, `SemanticModel`, or `CancellationToken` in a model.
- Use `EquatableArray<T>` (from `Core/Helpers`) instead of `ImmutableArray<T>` in models.

## Generators

| Generator class | Attribute | Input target |
|-----------------|-----------|--------------|
| `ReactiveGenerator` | `[Reactive]` | Field (all Roslyn) or partial property (ROSYLN_412+) |
| `ReactiveCommandGenerator` | `[ReactiveCommand]` | Method |
| `ObservableAsPropertyGenerator` | `[ObservableAsProperty]` | Field or observable method |
| `IViewForGenerator` | `[IViewFor<T>]` | Class |
| `RoutedControlHostGenerator` | `[RoutedControlHost]` | Class |
| `ViewModelControlHostGenerator` | `[ViewModelControlHost]` | Class |
| `BindableDerivedListGenerator` | `[BindableDerivedList]` | Field (`ReadOnlyObservableCollection<T>`) |
| `ReactiveCollectionGenerator` | `[ReactiveCollection]` | Field (`ObservableCollection<T>`) |
| `ReactiveObjectGenerator` | `[IReactiveObject]` | Class |

## Analyzers & Suppressors

All diagnostics use the `RXUISG` prefix. All suppressions use the `RXUISPR` prefix.

| Class | ID range | Purpose |
|-------|----------|---------|
| `PropertyToReactiveFieldAnalyzer` | RXUISG0016 | Suggests converting auto-properties to `[Reactive]` fields |
| `ReactiveAttributeMisuseAnalyzer` | RXUISG0020 | Detects `[Reactive]` on non-partial or non-partial-type members |
| `PropertyToReactiveFieldCodeFixProvider` | — | Converts auto-property → `[Reactive]` field |
| `ReactiveAttributeMisuseCodeFixProvider` | — | Fixes misuse of `[Reactive]` attribute |

Suppressors silence noisy Roslyn/Roslynator diagnostics that are expected for generator-backed patterns (e.g. fields never read, methods that don't need to be static).

### Analyzer Separation (Roslyn Best Practice)

- Generators do **not** report diagnostics — they only call `context.ReportDiagnostic` for internal invariant violations via `DiagnosticInfo` models.
- The `ReactiveUI.SourceGenerators.Analyzers.CodeFixes` project owns all `RXUISG*` diagnostic descriptors and code fixers.
- `DiagnosticDescriptors.cs` and related files are compiled from the shared Roslyn source via the linked `<Compile>` items.

## Testing

### Framework

- **TUnit** — test runner and assertion library (replaces xUnit/NUnit).
- **Verify.SourceGenerators** — snapshot-based verification of generated source output.
- **Microsoft.Testing.Platform** — native test execution (configured via `testconfig.json`).

### Test project targets

The test project multi-targets `net8.0;net9.0;net10.0` (controlled by `$(TestTfms)` in `Directory.Build.props`). Tests run against all three frameworks in CI.

### Snapshot tests

Generator tests extend `TestBase<TGenerator>` and call `TestHelper.TestPass(sourceCode)`. Verify saves `.verified.txt` snapshots in the appropriate subdirectory (`REACTIVE/`, `REACTIVECMD/`, `OAPH/`, `IVIEWFOR/`, `DERIVEDLIST/`, `REACTIVECOLL/`, `REACTIVEOBJ/`).

#### Accepting snapshot changes

1. Enable `VerifierSettings.AutoVerify()` in `ModuleInitializer.cs`.
2. Run `dotnet test --project src/ReactiveUI.SourceGenerator.Tests -c Release`.
3. Disable `VerifierSettings.AutoVerify()`.
4. Re-run tests to confirm all pass without AutoVerify.

### Test source language version

Test source strings are parsed with **CSharp13** (`LanguageVersion.CSharp13`). This is the version used by `TestHelper.RunGeneratorAndCheck`.

### Non-snapshot (unit) tests

Analyzer and helper tests use direct `CSharpCompilation` / `CompilationWithAnalyzers` to verify diagnostics without snapshots. See `PropertyToReactiveFieldAnalyzerTests.cs` for the pattern.

## Common Tasks

### Adding a New Generator

1. Create a value-equatable model record in `Core/Models/` or the generator's own `Models/` folder.
2. Add attribute source text to `AttributeDefinitions.cs` using a `$$"""..."""` raw string literal.
3. Create `<Name>Generator.cs` with `Initialize` wiring up `ForAttributeWithMetadataName`.
4. Create `<Name>Generator.Execute.cs` with `Get*Info` (extraction) and `GenerateSource` (raw string template).
5. Add snapshot tests in `ReactiveUI.SourceGenerator.Tests/UnitTests/`.
6. Accept snapshots using the AutoVerify trick above.

### Adding a New Analyzer Diagnostic

1. Add a `DiagnosticDescriptor` to `DiagnosticDescriptors.cs`.
2. Update `AnalyzerReleases.Unshipped.md`.
3. Implement the analyzer in `ReactiveUI.SourceGenerators.Analyzers.CodeFixes/`.
4. Add unit tests in `ReactiveUI.SourceGenerator.Tests/UnitTests/`.

### Running Tests

```pwsh
dotnet test src/ReactiveUI.SourceGenerator.Tests --configuration Release
```

### Building

```pwsh
dotnet build src/ReactiveUI.SourceGenerators.sln
```

## What to Avoid

- **`ISymbol` / `SyntaxNode` in pipeline output models** — breaks incremental caching; use value-equatable data records instead.
- **`SyntaxFactory` for code generation** — use `$$"""..."""` raw string literals.
- **`StringBuilder` for code generation** — use `$$"""..."""` raw string literals.
- **Diagnostics reported inside generators** — use the separate analyzer project for all `RXUISG*` diagnostics.
- **LINQ in hot Roslyn pipeline paths** — use `foreach` loops (Roslyn convention for incremental generators).
- **Non-value-equatable models** in the incremental pipeline — will defeat caching and cause unnecessary regeneration.
- **APIs unavailable in `netstandard2.0`** inside `ReactiveUI.SourceGenerators.Roslyn*` projects — the generator must run inside the compiler host which targets netstandard2.0.
- **Runtime reflection** in generated code — breaks Native AOT compatibility.
- **`#nullable enable` / nullable annotations in generated output** — these require C# 8+ features; generated code must be compatible with the minimum consumer C# version (12.0).
- **File-scoped namespaces in generated output** — requires C# 10; use block-scoped namespaces.

## Important Notes

- **Required .NET SDKs:** .NET 8.0, 9.0, and 10.0 (all required for multi-targeting the test project).
- **Generator + Analyzer targets:** `netstandard2.0` (Roslyn host requirement).
- **Test project targets:** `net8.0;net9.0;net10.0`.
- **No shallow clones:** The repository uses Nerdbank.GitVersioning; a full `git clone` is required for correct versioning.
- **NuGet packaging:** The `ReactiveUI.SourceGenerators` project bundles all three versioned generator DLLs at different `analyzers/dotnet/roslyn*/cs` paths.
- **Cross-platform tests:** On non-Windows platforms, WPF/WinForms types are injected as source stubs so generator tests compile cross-platform.
- **`SyntaxFactory` helper:** https://roslynquoter.azurewebsites.net/ — useful for inspecting how Roslyn models a given syntax construct (reference only; do not use SyntaxFactory in code-gen paths).

**Philosophy:** Generate zero-reflection, AOT-compatible ReactiveUI boilerplate at compile-time. Separate diagnostic reporting from code generation. Keep the incremental pipeline pure and value-equatable so Roslyn can cache and skip unchanged work.
