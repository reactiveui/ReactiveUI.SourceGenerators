# Generator benchmarks

Two projects measure what the generators allocate: the generators while they run, and the code they generate while a
consumer's app runs it. Allocation is the figure to watch: it varies far less between runs than time does.

| Project | Measures |
| --- | --- |
| `ReactiveUI.SourceGenerators.Benchmarks` | Generation: a cold run per generator's mock corpus, the whole corpus, and warm reruns after an edit. Also the attribute-discovery strategies against each other. |
| `ReactiveUI.SourceGenerators.Runtime.Benchmarks` | Generated code: a view model the generators write for, exercised as an app would. |

## How allocations are measured

Figures come from EventPipe, not BenchmarkDotNet's memory diagnoser. `--eventpipe <dir>` runs each scenario a fixed
number of times inside an EventPipe session on its own process, with the runtime provider's GC keyword at verbose
level. The runtime raises a `GCAllocationTick` event about every 100 KB allocated, carrying the amount, the type and the
call stack. The sum of the amounts divided by the number of operations is the allocation per operation. Its sampling
error is about one tick over the whole session, so sessions run enough operations for that to be small. Each scenario's
trace is kept for breaking down by type and frame:

```bash
cd src/benchmarks/ReactiveUI.SourceGenerators.Benchmarks
dotnet run -c Release -- --smoke                          # every corpus generates and compiles; discovery strategies agree
dotnet run -c Release -- --eventpipe <dir>                # generation scenarios
dotnet run -c Release -- --eventpipe-discovery <dir>      # attribute discovery strategies
cd ../ReactiveUI.SourceGenerators.Runtime.Benchmarks
dotnet run -c Release -- --eventpipe <dir>                # generated code
```

Each scenario writes `<dir>/<scenario>.nettrace`. To see which frames allocate, open it in PerfView and use the
"GC Heap Alloc Ignore Free (Coarse Sampling) Stacks" view.

The BenchmarkDotNet entry points remain for timings; they attach the same EventPipe profiler to each benchmark.

The mock corpus repeats each mock file 20 times, each copy in its own namespace, and every corpus includes `Noise.cs`:
attributed code no generator acts on, so each generator's cost of looking past code that is not its own is counted.
A scenario builds its compilation once and reuses it, so after the warmup the compiler's own symbol caches are warm and
the figures isolate the generators and the driver. The incremental scenarios keep one warm driver and rerun it against
the same edited compilation, which is what one keystroke costs in the IDE.

All figures are on .NET 10 (x64, Linux).

## Results

"Before" is the generators with `[ObservableAsProperty]` and view registration already removed, but still building
their output with interpolated strings, binding every attribute for `[IViewFor]` and `[IReactiveObject]`, and writing
every file from one collected output. It was measured with this same harness copied onto it. "After" is the generators
as they are now.

### Generation

| Scenario | Before, bytes/op | After, bytes/op | Change |
| --- | ---: | ---: | ---: |
| Cold, whole corpus | 17,330,975 | 11,636,366 | −32.9% |
| Cold, `[Reactive]` | 6,117,449 | 3,701,964 | −39.5% |
| Cold, `[ReactiveCommand]` | 2,849,862 | 1,764,827 | −38.1% |
| Cold, `[IReactiveObject]` | 2,851,598 | 1,542,311 | −45.9% |
| Cold, `[ReactiveCollection]` | 1,598,212 | 1,025,509 | −35.8% |
| Cold, `[IViewFor]` | 1,659,840 | 1,155,046 | −30.4% |
| Cold, `[BindableDerivedList]` | 1,142,445 | 952,705 | −16.6% |
| Cold, `[ViewModelControlHost]` | 1,168,922 | 1,166,055 | −0.2% |
| Cold, `[RoutedControlHost]` | 1,090,552 | 1,085,232 | −0.5% |
| Rerun after an unrelated edit | 8,984,317 | 7,838,332 | −12.8% |
| Rerun after editing a `[Reactive]` class's other members | 8,986,035 | 7,841,624 | −12.7% |
| Rerun after renaming a `[Reactive]` field | 10,990,885 | 7,986,248 | −27.3% |

In the whole-corpus trace, `System.String` fell from 5.53 MB to 2.83 MB per run, and `StringBuilder` (0.80 MB) and
`char[]` (0.70 MB) dropped out of the top allocated types. What remains is mostly the compiler's own binding and the
driver's state tables. For the WinForms hosts, half of what they allocate is the generated file's own text, which is
the output itself, so their figures did not move.

Where it came from, largest first:

- Writing through the indentation-aware `SourceWriter` instead of nested raw strings, interpolation, `string.Join` and
  `StringBuilder` fragments.
- `[IViewFor]` and `[IReactiveObject]` found their targets by binding every attribute of every attributed class; they
  now use `ForAttributeWithMetadataName`. That path was 66% of the cold `[IViewFor]` corpus's allocations.
- `TargetInfo` built once per type symbol instead of once per member; metadata-name checks compared without building
  names; `Task`/observable/scheduler checks compared structurally instead of building display strings; documentation
  only requested when the declaration has a documentation comment; `[Reactive]`'s AlsoNotify fallback no longer
  evaluates named arguments.
- Each type's file written from its own output: an edit that changes one type's models rewrites only that type's file.

### Attribute discovery

Three ways to find the 280 `[Reactive]` fields in the corpus, each in a generator that only discovers:

| Strategy | Cold, bytes/op | Rerun after an unrelated edit, bytes/op |
| --- | ---: | ---: |
| `ForAttributeWithMetadataName` | 484,701 | 314,141 |
| Attribute simple name from syntax, then the symbol | 770,266 | 363,839 |
| Bind every attributed field and read its attributes | 875,872 | 437,790 |

`ForAttributeWithMetadataName` allocates the least both cold and incrementally, so every generator uses it, with a
syntax predicate that rejects a node before any binding where the shape allows (for example, only `partial` classes for
`[IViewFor]` and `[IReactiveObject]`).

### Generated code

| Operation | Before, bytes/op | After, bytes/op |
| --- | ---: | ---: |
| Replace a `[ReactiveCollection]` property's collection | 355 | 70 |
| Add to and clear that collection, after 20,000 replacements | 640,392 | 161 |
| Set a `[Reactive]` property that also notifies another | 64 | 64 |
| Set a property of an `[IReactiveObject]` type | 32 | 32 |
| Read a `[ReactiveCommand]` command | 0 | 0 |

The generated `[ReactiveCollection]` setter created a new handler for each subscribe and unsubscribe, so it never
unsubscribed anything: every replacement left another handler behind, each collection change raised a notification per
leaked handler, and the replacement itself grew slower as the handler list grew. It now keeps one handler per property.
The 64 and 32 bytes are ReactiveUI's own notification arguments, not generated code. The collection-replacement figures
rest on few allocation ticks (13 and 66), so they carry a sampling error of roughly 100 KB over 20,000 operations.
