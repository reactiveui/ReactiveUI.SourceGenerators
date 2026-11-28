# Examples

Run the console projects to compare their output with the docs. The MAUI app shows the bindings on a button.

| Folder | What it holds |
|--------|---------------|
| `Documentation/Pages/<page>` | One project per page of the ReactiveUI.Binding documentation. Each snippet on a page comes from its project. |
| `Platforms/ToPropertyVerification` | A self-check of `ToProperty` and `[ObservableAsProperty]` on every runtime target, from .NET Framework 4.7 to .NET 11. |
| `Platforms/PlatformBindingsVerification` | A self-check of `ToProperty` feeding a real WPF, WinForms or MAUI control from a background thread. Run it with `--verify`. |
| [MAUI counter](TestApps/TestMauiApplication/MainPage.xaml.cs) | Tap the button to run a generated command and update its caption. The page uses `[IViewFor<MainViewModel>]` and attaches both bindings while it is visible. |
| [Review queue](Documentation/Pages/sourcegenerators/ReviewQueue.cs) | Replace a batch with `[ReactiveCollection]` and expose completed reviews through `[BindableDerivedList]`. |
| [Windows Forms hosts](TestApps/TestWinFormsApplication) | Display the view for a view model or router with `[ViewModelControlHost]` and `[RoutedControlHost]`. |

With the MAUI Android workload and an Android device or emulator, launch the counter from `src` with
`dotnet build examples/TestApps/TestMauiApplication -t:Run -f net11.0-android37`. For .NET 10, select `net10.0-android36.0`.

The ReactiveUI handbook has runnable clients for the attribute generators that work with its application APIs.

| Task | Example |
| --- | --- |
| Register default and named views at startup with `[RegisterView]`. | [Playlist view registration](https://github.com/reactiveui/ReactiveUI/tree/main/src/examples/Documentation/Pages/registration) |
| Save changes to data members using `[AutoPersist]`. | [Save a note](https://github.com/reactiveui/ReactiveUI/tree/main/src/examples/Documentation/Pages/data-persistence) |
| Apply DataAnnotations to reactive properties using `[ReactiveValidation]`. | [Student signup](https://github.com/reactiveui/ReactiveUI/tree/main/src/examples/Documentation/Pages/reactive-property) |
| Bind Android controls by resource ID, override a name or skip a property. | [Absence activity](https://github.com/reactiveui/ReactiveUI/blob/main/src/examples/Documentation/Pages/platform-android/GeneratedAbsenceActivity.cs) |

## The System.Reactive package

The main documentation projects use the lean `ReactiveUI.Binding` package. It schedules work with a *sequencer*, the `ISequencer` type from
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

The [System.Reactive task row](Documentation/Pages/setup/reactive/Program.cs) uses the same model as the Native AOT example. It observes title changes on an `IScheduler`, edits the title through a two-way binding and checks what happens after disposal. Run it from `src` with
`dotnet run --project examples/Documentation/Pages/setup/reactive -f net11.0` or select `net10.0`.

To adapt another example to System.Reactive:

1. Change each `ReactiveUI.Binding*` project reference to its `.Reactive` twin.
2. Change each `<Using Include="ReactiveUI.Binding..." />` item and `using` directive to the shifted namespace.
3. Pass an `IScheduler` wherever the example passes a sequencer.

Reference one runtime package, not both. Each package carries the generator, and two copies write the same files twice.
