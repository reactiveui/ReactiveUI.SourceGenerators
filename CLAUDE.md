# CLAUDE.md — ReactiveUI.SourceGenerators

This document provides guidance for AI assistants and contributors working in this repository.

## Overview

ReactiveUI.SourceGenerators is a Roslyn incremental source-generator package that automates ReactiveUI boilerplate at compile-time. It generates reactive properties, reactive commands, WinForms view hosts, bindable derived lists, reactive collections, and full reactive-object scaffolding — all with zero runtime reflection, making generated code fully AOT-compatible.

**Minimum consumer requirements:** C# 12.0 · Visual Studio 17.8.0 · ReactiveUI 23.2.28+

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

Each versioned project builds `ReactiveUI.SourceGenerators.Roslyn.dll`. The `ReactiveUI.SourceGenerators` project is
the package consumers install: it builds the attributes into `ReactiveUI.SourceGenerators.dll` under `lib/`, and bundles
the three generator DLLs, each with the code fixes, under `analyzers/dotnet/roslyn4.8/cs`, `analyzers/dotnet/roslyn4.14/cs`,
and `analyzers/dotnet/roslyn5.0/cs`, so NuGet/MSBuild automatically selects the right build based on the host compiler.

### Generated code never declares a shared type

The attributes and the enums they take (`AccessModifier`, `PropertyAccessModifier`, `InheritanceModifier`) are public
types in `ReactiveUI.SourceGenerators`, which targets `$(LibraryTfms)` (net8.0-net11.0, net462-net481). A type
declared into each consumer would repeat in every assembly, and an assembly granted `InternalsVisibleTo` would see two
copies (CS0436).

- **Do not use `RegisterPostInitializationOutput`**, or emit any type whose fully qualified name another assembly
  could also declare.
- A new attribute goes in `ReactiveUI.SourceGenerators` and is `[Conditional(KeepAttributes.Symbol)]`: the generators
  read it from source, so a consumer keeps no reference to the attributes assembly.
- Generators, analyzers and code fixes stay `netstandard2.0`, the Roslyn host's framework. Only the attributes library
  targets `$(LibraryTfms)`.
- Never emit a call ReactiveUI.Binding has to intercept (`WhenAny*`, `Bind`, `OneWayBind`, `BindCommand`): its
  generator cannot see another generator's output, so the call has no dispatch and throws at run time. Generated code
  that follows properties uses Binding's non-intercepted `ObservedProperty` when `ViewApiRules.HasObservedProperty`
  finds it (ReactiveUI.Binding 8.4.0 or later), and its own `PropertyChanged` following otherwise.

Generators report only the `RXUISG*` diagnostics about input they cannot generate from (see
[Analyzer Separation](#analyzer-separation-roslyn-best-practice)). Diagnostics about how code should be written, and
their code fixes, live in the separate `ReactiveUI.SourceGenerators.Analyzers.CodeFixes` project.

## Project Structure

```
src/
├── ReactiveUI.SourceGenerators.Roslyn/          # Shared source (linked into all versioned projects)
│   ├── AttributeDefinitions.cs                  # Metadata names of the attributes the generators read
│   ├── Reactive/                                # [Reactive] generator + Execute + models
│   ├── ReactiveCommand/                         # [ReactiveCommand] generator + Execute + models
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
├── ReactiveUI.SourceGenerators.Roslyn480/       # Roslyn 4.8 build (no define) of ReactiveUI.SourceGenerators.Roslyn.dll
├── ReactiveUI.SourceGenerators.Roslyn4140/      # Roslyn 4.14 build (ROSYLN_412)
├── ReactiveUI.SourceGenerators.Roslyn5000/      # Roslyn 5.0 build (ROSYLN_500)
├── ReactiveUI.SourceGenerators.Analyzers.CodeFixes/  # Analyzers + code fixers
├── ReactiveUI.SourceGenerators/                 # The package: public attributes, with the Roslyn builds bundled
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

A change to an emitter must keep the output token-for-token the same unless it deliberately changes generated code;
compare changed snapshots with whitespace removed to prove it.

## Roslyn Incremental Pipeline Pattern

Each generator follows this structure:

1. **`Initialize`** — calls one or more `Run*` methods. It registers no post-initialization output.
2. **`Run*`** — builds the `IncrementalValuesProvider` using `ForAttributeWithMetadataName` + a syntax predicate + a semantic extraction function.
3. **`Get*Info` (Execute file)** — stateless extraction function. Returns `Result<TModel?>` with embedded diagnostics. Must be pure; must not capture any `ISymbol` or `SyntaxNode` beyond this call.
4. **`GenerateSource` (Execute file)** — pure function that writes a model through `SourceWriter`. No Roslyn symbols allowed here.

```
Initialize()
  └─ SyntaxProvider.ForAttributeWithMetadataName
       ├─ syntax predicate (fast, node-type check only)
       ├─ semantic extraction → Get*Info() → Result<Model>
       └─ RegisterSourceOutput → GenerateSource() → AddSource()
```

**Fast paths:**
- Find attributed targets with `ForAttributeWithMetadataName`; benchmarks show it allocates less than a syntax
  provider, cold and incrementally. A generic attribute is registered by its arity name (``NameAttribute`1``).
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
| `RoutedControlHostGenerator` | `[RoutedControlHost]` | Class |
| `ViewModelControlHostGenerator` | `[ViewModelControlHost]` | Class |
| `BindableDerivedListGenerator` | `[BindableDerivedList]` | Field (`ReadOnlyObservableCollection<T>`) |
| `ReactiveCollectionGenerator` | `[ReactiveCollection]` | Field (`ObservableCollection<T>`) |
| `ReactiveObjectGenerator` | `[IReactiveObject]` | Class |

`[ObservableAsProperty]`, IViewFor view registration and `[IViewFor]` were removed here in favour of ReactiveUI.Binding
(ReactiveUI's binding engine). Do not reintroduce features ReactiveUI.Binding provides.

## Analyzers & Suppressors

All diagnostics use the `RXUISG` prefix. All suppressions use the `RXUISPR` prefix.

| Class | ID range | Purpose |
|-------|----------|---------|
| `PropertyToReactiveFieldAnalyzer` | RXUISG0016 | Suggests converting auto-properties to `[Reactive]` properties |
| `ReactiveAttributeMisuseAnalyzer` | RXUISG0020 | Detects `[Reactive]` on non-partial or non-partial-type members |
| `ReactiveCommandAnalyzer` | RXUISG0002, RXUISG0008, RXUISG0021 | Reports `[ReactiveCommand]` methods and schedulers the generator skips |
| `ControlHostAnalyzer` | RXUISG0022 | Reports WinForms hosts generated without ReactiveUI.Binding 8.4.0's `ObservedProperty` (Info) |
| `PropertyToReactiveFieldCodeFixProvider` | — | Converts auto-property → `[Reactive]` partial property (C# 13+; C# 14+ with an initializer), else a `[Reactive]` field |
| `ReactiveAttributeMisuseCodeFixProvider` | — | Fixes misuse of `[Reactive]` attribute |

Suppressors silence noisy Roslyn/Roslynator diagnostics that are expected for generator-backed patterns (e.g. fields never read, methods that don't need to be static).

### Analyzer Separation (Roslyn Best Practice)

- Generators report a diagnostic only when their input cannot be generated from: a name collision (RXUISG0009), an
  invalid forwarded attribute (RXUISG0012, RXUISG0013), a containing type that is not a `ReactiveObject`
  (RXUISG0018), or a `[BindableDerivedList]` field of the wrong type (RXUISG0019). The extraction step returns them as
  `DiagnosticInfo` values inside `Result<T>`, and `RegisterDiagnostics` (`Core/Extensions/IncrementalPipelineExtensions.cs`)
  reports them from a source output, so they never break caching. Do not add new diagnostics to generators.
- The `ReactiveUI.SourceGenerators.Analyzers.CodeFixes` project owns every other `RXUISG*` diagnostic (such as
  RXUISG0016 and RXUISG0020) and all code fixers.
- `DiagnosticDescriptors.cs` and related files are compiled from the shared Roslyn source via the linked `<Compile>` items.
- When an analyzer reports what a generator skips, both compile the same rules file (for commands,
  `ReactiveCommand/ReactiveCommandRules.cs`, linked into the code-fix project), so the two cannot disagree.

## Testing

### Framework

- **TUnit** — test runner and assertion library (replaces xUnit/NUnit).
- **`GeneratorSnapshot`** (in the test project) — compares each generated file with a stored snapshot. No Verify packages.
- **Microsoft.Testing.Platform** — native test execution (configured via `testconfig.json`).

### Test project targets

The test project multi-targets `net8.0;net9.0;net10.0` (controlled by `$(TestTfms)` in `Directory.Build.props`). Tests run against all three frameworks in CI.

### Snapshot tests

Generator tests extend `TestBase<TGenerator>` and call `TestHelper.TestPass(sourceCode)`. Each generated file is stored
as `{FOLDER}/{class}.{method}#{hint}.verified.cs` in the generator's folder (`REACTIVE/`, `REACTIVECMD/`,
`DERIVEDLIST/`, `REACTIVECOLL/`, `REACTIVEOBJ/`). The class segment is the test class's capitals, and common words in the
method and hint segments are abbreviated, so every path stays well inside the Windows path limit. A mismatch writes
`*.received.cs` beside the snapshot and fails; a snapshot the run no longer produces also fails. Generated-code
attribute lines, which carry the assembly version, are left out of the snapshots.

#### Accepting snapshot changes

1. From `src`, set `ACCEPT_SNAPSHOTS` to `1` (or `true`) and run the tests:
   ```pwsh
   $env:ACCEPT_SNAPSHOTS = '1'
   dotnet test --project ReactiveUI.SourceGenerator.Tests/ReactiveUI.SourceGenerators.Tests.csproj -c Release -f net10.0
   Remove-Item Env:ACCEPT_SNAPSHOTS
   ```
2. Review the snapshot diff; for a refactor, confirm the changes are whitespace only.
3. Re-run the tests without `ACCEPT_SNAPSHOTS` to confirm all pass.

### Test source language version

Test source strings are parsed with **CSharp13** (`LanguageVersion.CSharp13`). This is the version used by `TestHelper.RunGeneratorAndCheck`.

### Non-snapshot (unit) tests

Analyzer and helper tests use direct `CSharpCompilation` / `CompilationWithAnalyzers` to verify diagnostics without snapshots. See `PropertyToReactiveFieldAnalyzerTests.cs` for the pattern.

## Common Tasks

### Adding a New Generator

1. Create a value-equatable model record in `Core/Models/` or the generator's own `Models/` folder.
2. Add the attribute as a public `[Conditional(KeepAttributes.Symbol)]` type in `ReactiveUI.SourceGenerators`, and its
   metadata name to `AttributeDefinitions.cs`.
3. Create `<Name>Generator.cs` with `Initialize` wiring up `ForAttributeWithMetadataName`.
4. Create `<Name>Generator.Execute.cs` with `Get*Info` (extraction) and `GenerateSource` (writes through `SourceWriter`).
5. Add snapshot tests in `ReactiveUI.SourceGenerator.Tests/UnitTests/`.
6. Accept snapshots with `ACCEPT_SNAPSHOTS=1` as above.
7. Add a mock to `src/benchmarks/ReactiveUI.SourceGenerators.Benchmarks/Mocks` and the generator to `GeneratorCatalog`.

### Adding a New Analyzer Diagnostic

1. Add a `DiagnosticDescriptor` to `DiagnosticDescriptors.cs`.
2. Record the rule in `AnalyzerReleases.Shipped.md`, under the release section for the current major version. Never use
   `AnalyzerReleases.Unshipped.md`: it stays empty apart from its header, and every new, changed or removed rule is
   recorded as released.
3. Implement the analyzer in `ReactiveUI.SourceGenerators.Analyzers.CodeFixes/`.
4. Add unit tests in `ReactiveUI.SourceGenerator.Tests/UnitTests/`.

### Running Tests

Run from `src`, where `global.json` selects the SDK and Microsoft.Testing.Platform:

```pwsh
cd src
dotnet test --project ReactiveUI.SourceGenerator.Tests/ReactiveUI.SourceGenerators.Tests.csproj -c Release
```

### Measuring allocations

```pwsh
cd src/benchmarks/ReactiveUI.SourceGenerators.Benchmarks
dotnet run -c Release -- --smoke                      # every corpus generates and compiles
dotnet run -c Release -- --eventpipe <dir>            # bytes per run from GCAllocationTick, one trace per scenario
dotnet run -c Release -- --eventpipe-discovery <dir>  # attribute discovery strategies against each other
cd ../ReactiveUI.SourceGenerators.Runtime.Benchmarks
dotnet run -c Release -- --eventpipe <dir>            # the generated code, as an app runs it
```

Each scenario writes `<dir>/<scenario>.nettrace`; open it in PerfView's "GC Heap Alloc Ignore Free (Coarse Sampling)
Stacks" view to see which frames allocate.

Report allocation figures from the EventPipe traces, not the memory diagnoser, and compare a change against the
previous commit with the same harness. `src/benchmarks/README.md` has the method and the current figures.

### Building

```pwsh
dotnet build src/ReactiveUI.SourceGenerators.slnx
```

## What to Avoid

- **`ISymbol` / `SyntaxNode` in pipeline output models** — breaks incremental caching; use value-equatable data records instead.
- **`SyntaxFactory` for code generation** — write through `SourceWriter`.
- **`StringBuilder`, `string.Join` or interpolation in emitters** — append into `SourceWriter`.
- **New diagnostics reported inside generators** — put new `RXUISG*` diagnostics in the separate analyzer project.
- **LINQ in hot Roslyn pipeline paths** — use `foreach` loops (Roslyn convention for incremental generators).
- **Non-value-equatable models** in the incremental pipeline — will defeat caching and cause unnecessary regeneration.
- **APIs unavailable in `netstandard2.0`** inside `ReactiveUI.SourceGenerators.Roslyn*` projects — the generator must run inside the compiler host which targets netstandard2.0.
- **Runtime reflection** in generated code — breaks Native AOT compatibility.
- **Language features newer than C# 12 in generated output** — generated code must compile at the minimum consumer
  C# version (12.0). `#nullable enable` and nullable annotations are fine; generated files open with
  `DisableWarningsEnableNullable()`.
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
