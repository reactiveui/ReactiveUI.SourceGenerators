# CLAUDE.md — ReactiveUI.SourceGenerators

This document provides guidance for AI assistants and contributors working in this repository.

## Overview

ReactiveUI.SourceGenerators is a Roslyn incremental source-generator package that automates ReactiveUI boilerplate at compile-time. It generates reactive properties, observable-as-property helpers, reactive commands, IViewFor registrations, bindable derived lists, reactive collections, and full reactive-object scaffolding — all with zero runtime reflection, making generated code fully AOT-compatible.

**Minimum consumer requirements:** C# 12.0 · Visual Studio 17.8.0 · ReactiveUI 19.5.31+

## Architecture Overview

The repository ships **three versioned generator assemblies** built from a single shared source folder:

| Project | Roslyn version | Preprocessor constant | Extra features |
|---------|---------------|-----------------------|----------------|
| `ReactiveUI.SourceGenerators.Roslyn480` | 4.8.x (baseline) | _(none)_ | Field-based `[Reactive]`, `[ReactiveCommand]`, etc. |
| `ReactiveUI.SourceGenerators.Roslyn4140` | 4.14.0 | `ROSYLN_412` | + partial-property `[Reactive]` |
| `ReactiveUI.SourceGenerators.Roslyn5000` | 5.0.0 | `ROSYLN_500` | + same partial-property support on Roslyn 5 |

Each versioned project links all `.cs` files from `ReactiveUI.SourceGenerators.Roslyn/` via:

```xml
<Compile Include="..\ReactiveUI.SourceGenerators.Roslyn\**\*.cs" LinkBase="Shared" />
```

`#if ROSYLN_412 || ROSYLN_500` guards inside the shared source enable partial-property pipelines only on the newer Roslyn builds.

The `ReactiveUI.SourceGenerators` NuGet project packages all three DLLs under separate `analyzers/dotnet/roslyn4.8/cs`, `analyzers/dotnet/roslyn4.14/cs`, and `analyzers/dotnet/roslyn5.0/cs` paths, so NuGet/MSBuild automatically selects the right build based on the host compiler.

Diagnostics are **not** reported by generators. All `RXUISG*` diagnostics live in the separate `ReactiveUI.SourceGenerators.Analyzers.CodeFixes` project.

## Project Structure

```
src/
├── ReactiveUI.SourceGenerators.Roslyn/          # Shared source (linked into all versioned projects)
│   ├── AttributeDefinitions.cs                  # Injected attribute source texts
│   ├── Reactive/                                # [Reactive] generator + Execute + models
│   ├── ReactiveCommand/                         # [ReactiveCommand] generator + Execute + models
│   ├── IViewFor/                                # [IViewFor<T>] generator + Execute + models
│   ├── RoutedControlHost/                       # [RoutedControlHost] generator
│   ├── ViewModelControlHost/                    # [ViewModelControlHost] generator
│   ├── BindableDerivedList/                     # [BindableDerivedList] generator
│   ├── ReactiveCollection/                      # [ReactiveCollection] generator
│   ├── ReactiveObject/                          # [IReactiveObject] generator
│   ├── Diagnostics/                             # DiagnosticDescriptors, SuppressionDescriptors
│   ├── Polyfills/                               # netstandard2.0 polyfills, also linked into the code fixes
│   └── Core/
│       ├── CodeGeneration/                      # SourceWriter, PooledBuilder, SourceWriterExtensions
│       ├── Extensions/                          # ISymbol*, ITypeSymbol*, INamedTypeSymbol*, AttributeData extensions
│       ├── Helpers/                             # ImmutableArrayBuilder<T>, EquatableArray<T>, HashCode, etc.
│       └── Models/                              # Result<T>, DiagnosticInfo, TargetInfo, etc.
├── ReactiveUI.SourceGenerators.Roslyn480/       # Roslyn 4.8 build (no define)
├── ReactiveUI.SourceGenerators.Roslyn4140/      # Roslyn 4.14 build (ROSYLN_412)
├── ReactiveUI.SourceGenerators.Roslyn5000/      # Roslyn 5.0 build (ROSYLN_500)
├── ReactiveUI.SourceGenerators.Analyzers.CodeFixes/  # Analyzers + code fixers
├── ReactiveUI.SourceGenerators/                 # NuGet packaging project (bundles all three DLLs)
├── ReactiveUI.SourceGenerator.Tests/            # TUnit tests with generator snapshots (GeneratorSnapshot)
├── benchmarks/                                  # BenchmarkDotNet generation benchmarks with EventPipe tracing
├── ReactiveUI.SourceGenerators.Execute*/        # Compile-time execution verification projects
└── TestApps/                                    # Manual test applications (WPF, WinForms, MAUI, Avalonia)
```

## Code Generation Strategy

Generated source is written through `SourceWriter` (`Core/CodeGeneration`), an indentation-aware writer adopted from
the ReactiveUI.Binding generators. The writer indents each line from its level when the line's first character is
written, so an emitter says what it builds - a namespace, a containing type, a block - and the layout follows. It
rents its `StringBuilder` from a per-thread free list (`PooledBuilder`), so a generation pass reuses buffers.

```csharp
// CORRECT - append pieces straight into the writer; braces move the level
var writer = SourceWriter.Rent().AutoGenerated().DisableWarningsEnableNullable().BlankLine();
var depth = writer.OpenNamespace(target.TargetNamespace) + writer.OpenContainingTypes(target.ParentInfo);
_ = writer.OpenPartialType(target)
    .Append("public ").Append(property.Type).Append(' ').Line(property.Name)
    .OpenBlock()
    .Append("get => ").Append(property.FieldName).EndStatement()
    .CloseBlock();
return writer.CloseBlock().CloseBlocks(depth).RestoreNullableAndWarnings().ToStringAndReturn();

// Fixed multi-line text: Lines() with a raw literal, no interpolation, indented relative to the current level
_ = writer.Lines("""
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
    """);

// WRONG - $"" / $$"""...""" interpolation, string.Join, StringBuilder or SyntaxFactory in an emitter:
// each builds intermediate strings per member, and spelled-out indentation breaks in nested types.
```

`SourceWriterExtensions` names the constructs the generators share: `AutoGenerated`, `Using`,
`DisableWarningsEnableNullable`/`RestoreNullableAndWarnings`, `OpenNamespace` (block-scoped; the global namespace opens
nothing), `OpenContainingTypes`, `OpenPartialType`, `CloseBlocks`, `InheritDoc`, `ExcludeFromCodeCoverage`. Build a
generator's `GeneratedCode` attribute once, in a static field, with `SourceWriterExtensions.GeneratedCodeAttribute`.

The injected attribute source texts (in `AttributeDefinitions.cs`) are fixed `$$"""..."""` raw strings, each built once
per process.

A change to an emitter must keep the output token-for-token the same unless it deliberately changes generated code;
compare changed snapshots with whitespace removed to prove it.

## Roslyn Incremental Pipeline Pattern

Each generator follows this structure:

1. **`Initialize`** — registers post-initialization output (inject attribute source), then calls one or more `Run*` methods.
2. **`Run*`** — builds the `IncrementalValuesProvider` using `ForAttributeWithMetadataName` + a syntax predicate + a semantic extraction function.
3. **`Get*Info` (Execute file)** — stateless extraction function. Returns `Result<TModel?>` with embedded diagnostics. Must be pure; must not capture any `ISymbol` or `SyntaxNode` beyond this call.
4. **`GenerateSource` (Execute file)** — pure function that writes a model through `SourceWriter`. No Roslyn symbols allowed here.

```
Initialize()
  ├─ RegisterPostInitializationOutput → inject attribute definitions
  └─ SyntaxProvider.ForAttributeWithMetadataName
       ├─ syntax predicate (fast, node-type check only)
       ├─ semantic extraction → Get*Info() → Result<Model>
       └─ RegisterSourceOutput → GenerateSource() → AddSource()
```

**Fast paths:**
- Find attributed targets with `ForAttributeWithMetadataName`; benchmarks show it allocates less than a syntax
  provider, cold and incrementally. A generic attribute is registered by its arity name (``IViewForAttribute`1``).
- Reject nodes in the syntax predicate before any binding: node kind, parent shape, `partial` modifier.
- In the transform, read `context.Attributes[0]` rather than searching the symbol's attributes again, and ask for
  semantic information only when syntax says it can exist (documentation only when the node has a doc comment).
- Group models per type with `GroupByTarget` and register one source output per type, so an edit rewrites only the
  types whose models changed. Tag steps with `TrackingNames`; `IncrementalCachingTests` asserts what is reused.

**Incremental caching rules:**
- All pipeline output models must implement value equality (`record`, `IEquatable<T>`, or `EquatableArray<T>`).
- Never store `ISymbol`, `SyntaxNode`, `SemanticModel`, or `CancellationToken` in a model.
- Use `EquatableArray<T>` (from `Core/Helpers`) instead of `ImmutableArray<T>` in models.

## Generators

| Generator class | Attribute | Input target |
|-----------------|-----------|--------------|
| `ReactiveGenerator` | `[Reactive]` | Field (all Roslyn) or partial property (ROSYLN_412+) |
| `ReactiveCommandGenerator` | `[ReactiveCommand]` | Method |
| `IViewForGenerator` | `[IViewFor<T>]` | Class (the `IViewFor<T>` implementation; view registration belongs to ReactiveUI.Binding) |
| `RoutedControlHostGenerator` | `[RoutedControlHost]` | Class |
| `ViewModelControlHostGenerator` | `[ViewModelControlHost]` | Class |
| `BindableDerivedListGenerator` | `[BindableDerivedList]` | Field (`ReadOnlyObservableCollection<T>`) |
| `ReactiveCollectionGenerator` | `[ReactiveCollection]` | Field (`ObservableCollection<T>`) |
| `ReactiveObjectGenerator` | `[IReactiveObject]` | Class |

`[ObservableAsProperty]` and IViewFor view registration moved to ReactiveUI.Binding (ReactiveUI's binding engine) and
were removed here. Do not reintroduce features ReactiveUI.Binding provides.

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
- **`GeneratorSnapshot`** (in the test project) — compares each generated file with a stored snapshot. No Verify packages.
- **Microsoft.Testing.Platform** — native test execution (configured via `testconfig.json`).

### Test project targets

The test project multi-targets `net8.0;net9.0;net10.0` (controlled by `$(TestTfms)` in `Directory.Build.props`). Tests run against all three frameworks in CI.

### Snapshot tests

Generator tests extend `TestBase<TGenerator>` and call `TestHelper.TestPass(sourceCode)`. Each generated file is stored
as `{FOLDER}/{class}.{method}#{hint}.verified.cs` in the generator's folder (`REACTIVE/`, `REACTIVECMD/`, `IVIEWFOR/`,
`DERIVEDLIST/`, `REACTIVECOLL/`, `REACTIVEOBJ/`). The class segment is the test class's capitals, and common words in the
method and hint segments are abbreviated, so every path stays well inside the Windows path limit. A mismatch writes
`*.received.cs` beside the snapshot and fails; a snapshot the run no longer produces also fails. Generated-code
attribute lines, which carry the assembly version, are left out of the snapshots.

#### Accepting snapshot changes

1. Run `ACCEPT_SNAPSHOTS=1 dotnet test --project ReactiveUI.SourceGenerator.Tests/ReactiveUI.SourceGenerators.Tests.csproj -c Release -f net10.0` from `src`.
2. Review the snapshot diff; for a refactor, confirm the changes are whitespace only.
3. Re-run the tests without `ACCEPT_SNAPSHOTS` to confirm all pass.

### Test source language version

Test source strings are parsed with **CSharp13** (`LanguageVersion.CSharp13`). This is the version used by `TestHelper.RunGeneratorAndCheck`.

### Non-snapshot (unit) tests

Analyzer and helper tests use direct `CSharpCompilation` / `CompilationWithAnalyzers` to verify diagnostics without snapshots. See `PropertyToReactiveFieldAnalyzerTests.cs` for the pattern.

## Common Tasks

### Adding a New Generator

1. Create a value-equatable model record in `Core/Models/` or the generator's own `Models/` folder.
2. Add attribute source text to `AttributeDefinitions.cs` as a `$$"""..."""` raw string property initialised once.
3. Create `<Name>Generator.cs` with `Initialize` wiring up `ForAttributeWithMetadataName`.
4. Create `<Name>Generator.Execute.cs` with `Get*Info` (extraction) and `GenerateSource` (writes through `SourceWriter`).
5. Add snapshot tests in `ReactiveUI.SourceGenerator.Tests/UnitTests/`.
6. Accept snapshots with `ACCEPT_SNAPSHOTS=1` as above.
7. Add a mock to `src/benchmarks/ReactiveUI.SourceGenerators.Benchmarks/Mocks` and the generator to `GeneratorCatalog`.

### Adding a New Analyzer Diagnostic

1. Add a `DiagnosticDescriptor` to `DiagnosticDescriptors.cs`.
2. Update `AnalyzerReleases.Unshipped.md`.
3. Implement the analyzer in `ReactiveUI.SourceGenerators.Analyzers.CodeFixes/`.
4. Add unit tests in `ReactiveUI.SourceGenerator.Tests/UnitTests/`.

### Running Tests

```pwsh
dotnet test src/ReactiveUI.SourceGenerator.Tests --configuration Release
```

### Measuring allocations

```pwsh
cd src/benchmarks/ReactiveUI.SourceGenerators.Benchmarks
dotnet run -c Release -- --smoke                      # every corpus generates and compiles
dotnet run -c Release -- --eventpipe <dir>            # bytes per run from GCAllocationTick, one trace per scenario
dotnet run -c Release -- --eventpipe-discovery <dir>  # attribute discovery strategies against each other
cd ../ReactiveUI.SourceGenerators.Runtime.Benchmarks
dotnet run -c Release -- --eventpipe <dir>            # the generated code, as an app runs it
dotnet run ~/source/rxui/tools/nettrace-analyzer.cs -- --top 40 <dir>/<scenario>.nettrace
```

Report allocation figures from the EventPipe traces, not the memory diagnoser, and compare a change against the
previous commit with the same harness. `src/benchmarks/README.md` has the method and the current figures.

### Building

```pwsh
dotnet build src/ReactiveUI.SourceGenerators.sln
```

## What to Avoid

- **`ISymbol` / `SyntaxNode` in pipeline output models** — breaks incremental caching; use value-equatable data records instead.
- **`SyntaxFactory` for code generation** — write through `SourceWriter`.
- **`StringBuilder`, `string.Join` or interpolation in emitters** — append into `SourceWriter`.
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
- **No shallow clones:** The repository uses MinVer; a full `git clone` with tags is required for correct versioning.
- **NuGet packaging:** The `ReactiveUI.SourceGenerators` project bundles all three versioned generator DLLs at different `analyzers/dotnet/roslyn*/cs` paths.
- **Cross-platform tests:** On non-Windows platforms, WPF/WinForms types are injected as source stubs so generator tests compile cross-platform.
- **`SyntaxFactory` helper:** https://roslynquoter.azurewebsites.net/ — useful for inspecting how Roslyn models a given syntax construct (reference only; do not use SyntaxFactory in code-gen paths).

**Philosophy:** Generate zero-reflection, AOT-compatible ReactiveUI boilerplate at compile-time. Separate diagnostic reporting from code generation. Keep the incremental pipeline pure and value-equatable so Roslyn can cache and skip unchanged work.
