# Examples

Every project here runs. Each one prints what it does, so you can compare the output with the docs.

| Folder | What it holds |
|--------|---------------|
| `Documentation/Pages/<page>` | One project per page of the ReactiveUI.Binding documentation. Each snippet on a page comes from its project. |
| `Platforms/ToPropertyVerification` | A self-check of `ToProperty` and `[ObservableAsProperty]` on every runtime target, from .NET Framework 4.6.2 to .NET 11. |
| `Platforms/PlatformBindingsVerification` | A self-check of `ToProperty` feeding a real WPF, WinForms or MAUI control from a background thread. Run it with `--verify`. |

## The System.Reactive package

All examples use the lean `ReactiveUI.Binding` package. It schedules work with a *sequencer*, the `ISequencer` type from
`ReactiveUI.Primitives`.

The `ReactiveUI.Binding.Reactive` package is the same library compiled against System.Reactive. It takes System.Reactive's
`IScheduler` wherever the lean package takes an `ISequencer`. The calls are the same. Only the names change:

| Lean package | System.Reactive package |
|--------------|-------------------------|
| package `ReactiveUI.Binding` | package `ReactiveUI.Binding.Reactive` |
| package `ReactiveUI.Binding.Wpf`, `.WinForms`, `.Maui` | package `ReactiveUI.Binding.Wpf.Reactive`, `.WinForms.Reactive`, `.Maui.Reactive` |
| namespace `ReactiveUI.Binding` | namespace `ReactiveUI.Binding.Reactive` |
| namespace `ReactiveUI.Binding.Builder` | namespace `ReactiveUI.Binding.Reactive.Builder` |
| namespace `ReactiveUI.Binding.Maui` | namespace `ReactiveUI.Binding.Reactive.Maui` |
| a parameter of type `ISequencer` | a parameter of type `IScheduler` |

To run an example against System.Reactive:

1. Change each `ReactiveUI.Binding*` project reference to its `.Reactive` twin.
2. Change each `<Using Include="ReactiveUI.Binding..." />` item and `using` directive to the shifted namespace.
3. Pass an `IScheduler` wherever the example passes a sequencer.

Reference one runtime package, not both. Each package carries the generator, and two copies write the same files twice.
