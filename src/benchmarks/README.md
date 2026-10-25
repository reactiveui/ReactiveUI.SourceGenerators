# Benchmarks

This folder measures two different things. One is what a consumer's application pays at run time when a
binding fires. The other is what a consumer's build pays while the generator runs.

## What it costs

Measured by the `Benchmarks` workflow on a GitHub-hosted Windows runner: two physical cores under Hyper-V,
.NET 10.0.12. Each case drives a thousand property changes through one subscription or binding.

Allocation is deterministic and compares directly. The timings carry real spread on a runner that size - the
observation cases move by twenty to thirty per cent between iterations - so read them as the shape of the
difference rather than as a score, and run the suite on your own hardware for a number you can hold someone
to.

| Observation | ReactiveUI's engine | `WhenChanged` | `WhenAnyValue` |
|-------------|--------------------:|--------------:|---------------:|
| One property | 145.6 us / 105.5 KB | 81.7 us / 65.5 KB | 79.5 us / 65.5 KB |
| A deep chain | 120.9 us / 106.2 KB | 108.5 us / 66.2 KB | 107.6 us / 66.2 KB |
| Two properties | 340.5 us / 195.6 KB | 182.4 us / 90.5 KB | 142.3 us / 90.5 KB |
| Creating the subscription | 9.8 us / 1.5 KB | 9.9 us / 1.4 KB | 9.0 us / 1.4 KB |

| Binding | ReactiveUI's engine | Generated |
|---------|--------------------:|----------:|
| `OneWayBind` | 287.8 us / 130.4 KB | 114.5 us / 88.5 KB |
| `Bind` | 686.5 us / 932.3 KB | 53.6 us / 42.2 KB |
| Creating a one-way binding | 22.5 us / 5.4 KB | 11.3 us / 2.6 KB |

`BindOneWay` and `BindTwoWay` have no counterpart in the other engine to read against. They cost
93.1 us / 65.4 KB and 105.9 us / 87.8 KB, and naming a scheduler lowers both rather than raising them
(71.1 us and 90.9 us), because the write is then queued instead of applied inline.

Published ahead of time, the generated path measures within a few per cent of the same code on the JIT:
`WhenAnyValue` 83.6 us against 79.5 us, `BindOneWay` 93.3 us against 93.1 us, `Bind` 53.8 us against
53.6 us. The expression-tree engine has no figure here because it cannot run under NativeAOT at all.

The `Unsafe` overloads are the price of an expression the generator could not read: `WhenChanged` 146.5 us /
103.1 KB against 81.7 us / 65.5 KB generated, `BindOneWay` 223.2 us / 221.4 KB against 93.1 us / 65.4 KB,
`Bind` 405.5 us / 402.7 KB against 53.6 us / 42.2 KB.

A generation pass costs 3.6 ms and 1.9 MB for one view-model and view pair, 52.3 ms and 29.5 MB for sixteen,
and 213.6 ms and 118.1 MB for sixty-four. Whether the build claims call sites outright or offers a competing
overload makes no measurable difference.

## The projects

| Project | Measures |
|---------|----------|
| `ReactiveUI.Binding.Benchmarks` | The generated observation and binding path. |
| `ReactiveUI.Binding.Benchmarks.ReactiveUI` | The same scenarios on ReactiveUI's expression-tree engine, as the baseline. |
| `ReactiveUI.Binding.Generator.Benchmarks` | A whole generation pass over a corpus of consumer code. |
| `ReactiveUI.Binding.Generator.Benchmarks.Roslyn413` | The same generation pass against Roslyn 4.13. |
| `ReactiveUI.Binding.Analyzer.Benchmarks` | What the analyzers cost a compilation, on startup, clean and violating source. |

The baseline lives in its own project because it needs ReactiveUI's own binding surface. Keeping the two
apart stops one engine's imports reaching the other's call sites.

## What the runtime benchmarks cover

Each case creates one observation or binding and then drives a thousand property changes through it. The
cost of creating the subscription is measured separately, by the `First...` cases.

| Benchmark | Operator | Cases |
|-----------|----------|-------|
| `WhenChangedBenchmark` | `WhenChanged` | `SingleProperty`, `DeepChain`, `TwoProperties`, `FirstObservation` |
| `WhenChangingBenchmark` | `WhenChanging` | `SingleProperty`, `TwoProperties`, `FirstObservation` |
| `WhenAnyValueBenchmark` | `WhenAnyValue` | `SingleProperty`, `DeepChain`, `TwoProperties`, `FirstObservation` |
| `WhenAnyBenchmark` | `WhenAny` | `SingleProperty`, `TwoProperties`, `FirstObservation` |
| `WhenAnyObservableBenchmark` | `WhenAnyObservable` | `SingleStream`, `TwoStreams`, `FirstObservation` |
| `WhenAnyDynamicBenchmark` | `WhenAnyDynamic` | `SingleChain`, `TwoChains`, `DeepChain`, `FirstObservation`, `SingleChainGenerated` |
| `BindOneWayBenchmark` | `BindOneWay` | `Standard`, `WithScheduler`, `FirstBinding`, `SetupTeardown` |
| `BindTwoWayBenchmark` | `BindTwoWay` | `Standard`, `WithScheduler`, `Bidirectional` |
| `BindTwoWayQueuedBenchmark` | `BindTwoWay` through a sequencer that queues | `InitialDelivery`, `ModelToView`, `ViewToModel` |
| `BindBenchmark` | `Bind` | `Standard`, `Bidirectional`, `WithObservedChanges` |
| `OneWayBindBenchmark` | `OneWayBind` | `Standard`, `FirstBinding` |
| `BindToBenchmark` | `BindTo` | `Standard`, `FirstBinding` |
| `TypedAdapterBenchmark` | `BindTo` conversions | `NullableValues`, `FormattedValues` |
| `InvokeCommandBenchmark` | `InvokeCommand` | `Standard`, `FirstInvocation` |
| `BindCommandBenchmark` | `BindCommand` | Subscription and command replacement through an established binding |
| `BindInteractionBenchmark` | `BindInteraction` | Subscription and handler migration between existing interactions |
| `ObservationDeliveryBenchmark` | After-change, before-change and plugin observations | Subscription and steady delivery of integer values |
| `ObservationContentionBenchmark` | The three observation delivery contracts | Competing producer latency and slow-subscriber delivery-thread occupancy |
| `CombineLatestArityBenchmark` | 2–16-source `CombineLatest` | Public concrete constructor versus extension, including subscription and disposal |
| `UnsafeFallbackBenchmark` | the `Unsafe` overloads | `WhenChangedUnsafe`, `WhenChangedUnsafe deep chain`, `WhenAnyValueUnsafe`, `BindOneWayUnsafe`, `BindUnsafe` |
| `ViewLocatorBenchmark` | `DefaultViewLocator.ResolveView` | `SingleLookupHit`, `HitInFirstOfThreeLookups`, `HitInLastOfThreeLookups`, `MissInEveryLookup`, `AssemblyGeneratedLookup` |
| `RxUiDynamicChainBaseline` | ReactiveUI's dynamic chain | `SingleChain`, `TwoChains`, `DeepChain`, `FirstObservation` |

`WhenAnyValueBenchmark` and `OneWayBindBenchmark` exist so the two operators the ReactiveUI baseline measures
have a counterpart here. Read each against `ReactiveUIObservationBenchmark` and `ReactiveUIBindingBenchmark`.

`WhenAnyDynamicBenchmark` carries `SingleChainGenerated` so the two halves of the same scenario sit in one
table. A chain named at run time is walked by reflection; the same chain written as a lambda is resolved at
compile time.

Each class declares a job per runtime: .NET 10 and 11, and NativeAOT 10 and 11 where the code can run
ahead of time. A class that walks a chain by reflection declares no NativeAOT job, because it cannot run
there.

## What the generation benchmark covers

`GenerationBenchmarks.Generate` runs a cold pass: syntax scan, extraction and emission. Two parameters
vary.

| Parameter | Values | Meaning |
|-----------|--------|---------|
| `Pairs` | 1, 16, 64 | How many view-model and view pairs the corpus holds. |
| `Intercept` | false, true | Whether the build claims each call site outright or offers an overload that competes for them all. |

A fresh generator driver is built for every iteration. A reused driver would serve the next iteration from
its incremental caches, which measures the cache rather than the pass a consumer's build actually pays for.
The corpus is built once per parameter set, because loading a framework's worth of metadata references
costs far more than the pass under measurement.

The `GcVerbose` EventPipe profiler records allocations. Analysis uses the measured workload windows and their
operation counts to report sampled bytes per generation and the allocation sites. NativeAOT timing runs remain
separate where EventPipe capture is unavailable.

## Views that name their controls in XAML

`XamlViewGenerationBenchmarks` measures four views with controls named in XAML.
The generation rows use 100 bindings per view. The rebuild rows use v2rayN.

| Measurement | Before | After |
| --- | ---: | ---: |
| Generation (.NET 10) | 11.95 s | 228 ms |
| Generation (.NET 11) | 12.79 s | 204 ms |
| Allocation (.NET 10) | 3,703 MB | 61 MB |
| Allocation (.NET 11) | 3,702 MB | 61 MB |
| v2rayN generator | 10.97 s | 441 ms |
| v2rayN rebuild | 17.45 s | 5.81 s |

## What the analyzer benchmark covers

`AnalyzerBenchmarks` runs each analyzer over a corpus through `CompilationWithAnalyzers`. It has two cases,
`AnalyzeInvocations` for `BindingInvocationAnalyzer` and `AnalyzeTypes` for `TypeAnalyzer`. One parameter
varies.

| `Corpus` | Meaning |
|----------|---------|
| `Startup` | One empty class. This is the fixed cost every compilation pays before an analyzer has anything to decide. |
| `Clean` | Calls to `WhenChanged`, `BindOneWay`, `BindTwoWay`, `BindTo` and `InvokeCommand` with nothing to report. |
| `CleanNullForgiving` | The `Clean` calls with a `!` after each link of each path. |
| `Violating` | Calls that report a private member, an unsupported segment, a silent link and a type that never notifies. |
| `ViolatingNullForgiving` | The `Violating` calls with a `!` after each link. |
| `UnsafeTargets` | Calls to `BindToUnsafe`, `InvokeCommandUnsafe` and `OneWayBindUnsafe`, whose first type argument is not the object they observe. |

`Clean` minus `Startup` is the cost of deciding there is nothing to report. Report it apart from the
`Violating` figure, which adds the cost of the diagnostics themselves. Setup fails when a corpus does not bind,
when `Startup`, `Clean` or `CleanNullForgiving` reports anything, or when `Violating` reports nothing. Setup
logs the diagnostic count of every corpus.

To compare two checkouts, build this project in each and run the same filter in the same session:

```bash
dotnet run -c Release --project benchmarks/ReactiveUI.Binding.Analyzer.Benchmarks -f net10.0 -- --filter '*AnalyzerBenchmarks*'
```

## Focused adapter measurements

`AdapterGenerationBenchmarks` covers native observation, Android/UIKit/AppKit commands, WinForms collection
bindings and typed conversions. Each corpus checks the exact number of binding methods and the intended
native operations before collecting timings.

`TypedAdapterBenchmark` measures nullable and formatting delivery after creating the bindings.
`NativeAdapterBenchmark` measures observation, command delivery and collection writes against native-shaped
contracts. These fixtures exclude platform rendering costs. Both benchmarks count actual deliveries and reject
runs that drop updates or leave layout suspended. Timings include this counting overhead.

Run only these cases with `--filter '*AdapterGenerationBenchmarks*'` in the generator benchmark project, or
`--filter '*TypedAdapterBenchmark*' '*NativeAdapterBenchmark*'` in the runtime benchmark project. Use `--artifacts` to put reports and
traces under `~/.cache`.

## Delivery and operator adoption

`ObservationDeliveryBenchmark.Subscribe` includes observable construction, initial delivery and disposal.
`Deliver` uses an established subscription and reports cost per property change. Its integer source caches
event arguments. The plugin fixture forwards a cached notification token and leaves value reads to the binding.

`ObservationContentionBenchmark.ProducerLatency` times one setter while another thread holds the subscriber.
Setup waits until the subscriber has entered; cleanup releases it after the setter returns.
`DeliveryThread` times the raising thread through one 40 ms subscriber call while another producer makes
16 changes. It includes the subscriber's fixed workload and any pending delivery that the raising thread
drains. Read it as delivery-thread occupancy, including work a UI thread would perform when it raises the
change. These cases do not measure a platform dispatcher. Thread creation, subscription and joining are
outside both measurements. Before-change delivery preserves captured values; after-change delivery can
collapse pending changes, so compare producer latency and delivery occupancy together.

`CombineLatestArityBenchmark` has two comparison groups. `ConstructAndSubscribe` includes construction;
`Subscribe` subscribes to existing pipelines to isolate subscription allocations. Both groups create fresh
subscription state and dispose it inside each measurement. The public constructor captures typed slots for
each arity. Setup verifies that both routes combine every input. Compare the allocation columns before
replacing a wide-arity extension with a constructor.

`BindCommandBenchmark` uses a control with `Command` and `CommandParameter` properties. `BindInteractionBenchmark`
uses a synchronous handler. Each measures setup separately from replacing the source property through a live
binding. The replacement cases report cost per replacement and reuse commands or interactions.

Run these families separately from `src/` after builds and tests finish:

```sh
dotnet run -c Release -f net10.0 --project benchmarks/ReactiveUI.Binding.Benchmarks -- --filter '*CombineLatestArityBenchmark*' --warmupCount 5 --iterationCount 15
dotnet run -c Release -f net10.0 --project benchmarks/ReactiveUI.Binding.Benchmarks -- --filter '*ObservationDeliveryBenchmark*' '*ObservationContentionBenchmark*' --warmupCount 5 --iterationCount 15
dotnet run -c Release -f net10.0 --project benchmarks/ReactiveUI.Binding.Benchmarks -- --filter '*BindCommandBenchmark*' '*BindInteractionBenchmark*' --warmupCount 5 --iterationCount 15
```

Each command retains the shared managed tracing and NativeAOT jobs. On Linux, pin the process to separate
physical cores and raise priority where permitted. Compare error ranges as well as means, and inspect the
allocation trace when a route allocates more. Keep benchmark runs separate from builds and tests.

## ToProperty

`ToPropertyBenchmark` creates a helper-backed property through each way the generator can raise notifications,
and pushes values through a live one. `ReactiveUIToPropertyBenchmark`, in the ReactiveUI project, runs the same
cases on ReactiveUI's own `ToProperty`. `ToPropertyContentionBenchmark` pushes distinct values into one helper
from one, two and four threads at once. `ToPropertyUnsafeBenchmark` creates and drives properties backed by
`ToPropertyUnsafe`, on a view model with a protected raise method and on one with only a field-like event.

`ToPropertyExtractionBenchmarks`, in the generator project, measures the two checks that can turn a call site away:
the selector's shape read from syntax, and the call resolved through the semantic model.
`ObservableAsPropertyDiscoveryBenchmarks` compares finding `[ObservableAsProperty]` properties with
`ForAttributeWithMetadataName` against a syntax filter followed by a semantic check.

```sh
dotnet run -c Release -f net10.0 --project benchmarks/ReactiveUI.Binding.Benchmarks -- --filter '*ToPropertyBenchmark*' '*ToPropertyContentionBenchmark*' '*ToPropertyUnsafeBenchmark*'
dotnet run -c Release -f net10.0 --project benchmarks/ReactiveUI.Binding.Benchmarks.ReactiveUI -- --filter '*ReactiveUIToPropertyBenchmark*'
dotnet run -c Release -f net10.0 --project benchmarks/ReactiveUI.Binding.Generator.Benchmarks -- --filter '*ToPropertyExtractionBenchmarks*' '*ObservableAsPropertyDiscoveryBenchmarks*'
```

## Unsafe paths

`UnsafeFallbackBenchmark` declares no NativeAOT job. Those overloads carry `RequiresUnreferencedCode` because
they walk the path at run time, so an ahead-of-time publish cannot be relied on to keep the members they
reach. That class is the measurement of what the fallback costs against the generated operator of the same
name.

## Running them

Each project hands its command line to the BenchmarkDotNet switcher, so `--filter` selects cases.

```sh
cd src/benchmarks/ReactiveUI.Binding.Benchmarks
dotnet run -c Release -f net10.0 -- --filter '*'

cd ../ReactiveUI.Binding.Benchmarks.ReactiveUI
dotnet run -c Release -f net10.0 -- --filter '*'

cd ../ReactiveUI.Binding.Generator.Benchmarks
dotnet run -c Release -f net10.0 -- --filter '*'
```

Release configuration is required. A Debug build measures the absence of the optimiser.

On Windows, add `--artifacts-path C:\a`. BenchmarkDotNet generates a project per job beside the built
assembly, and the NativeAOT build writes below that again; from a deep checkout the linker's module
definition file lands past 260 characters, and every NativeAOT job then reports `NA` instead of a
measurement.

Results land in `BenchmarkDotNet.Artifacts/results`, which is ignored by git.

## Reading the results

Three things are worth knowing before comparing columns.

The generated path removes reflection and expression compilation, so it wins on both time and allocation
against the baseline. Compare `WhenChangedBenchmark` against `ReactiveUIObservationBenchmark`, and
`BindOneWayBenchmark` and `BindBenchmark` against `ReactiveUIBindingBenchmark`.

A chain named at run time is the one place this library does not win. Both engines walk it by reflection and
allocate the same objects doing it - 105.1 KB against 105.0 KB - and ReactiveUI is marginally the quicker of
the two, at 118.0 us against 128.7 us. The gain there comes from writing the chain as a lambda instead, which
`SingleChainGenerated` measures at 81.1 us / 65.5 KB.

The expression-tree engine cannot run under NativeAOT at all, because it compiles expressions and reflects
at run time. Only the generated path has a NativeAOT column to report.
