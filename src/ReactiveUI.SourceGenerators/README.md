# ReactiveUI.SourceGenerators

ReactiveUI.SourceGenerators writes ReactiveUI boilerplate for you when you build: reactive properties, commands,
`IReactiveObject` implementations, `IViewFor<T>` views, derived lists, reactive collections and WinForms control hosts.
The generated code uses no reflection, so it works with trimming and Native AOT.

The same repository also builds [ReactiveUI.Binding](https://github.com/reactiveui/ReactiveUI.SourceGenerators#readme),
which generates the code behind `WhenAnyValue`, `Bind`, `BindCommand`, `ToProperty` and `[ObservableAsProperty]`.
The two packages work together and share their infrastructure.

- **Minimum requirements**
  - C# 12.0
  - Visual Studio 17.8 (Roslyn 4.8) or the .NET 8 SDK; partial properties need Visual Studio 17.14 or later
  - ReactiveUI 23.2.28 or later

## Table of contents

- [Installation](#installation)
- [Overview](#overview)
- [Roslyn and Visual Studio support](#roslyn-and-visual-studio-support)
- [Migration from ReactiveUI.Fody](#migration-from-reactiveuifody)
- [Reactive properties](#reactive-properties)
- [Reactive commands](#reactive-commands)
- [Views](#views)
- [Derived lists and collections](#derived-lists-and-collections)
- [WinForms control hosts](#winforms-control-hosts)
- [Working with ReactiveUI.Binding](#working-with-reactiveuibinding)
- [Credits](#credits)

## Installation

```bash
dotnet add package ReactiveUI.SourceGenerators
```

Load the package with `PrivateAssets="all"` so the generator does not flow to projects that reference yours:

```xml
<PackageReference Include="ReactiveUI.SourceGenerators" Version="..." PrivateAssets="all" />
```

The attributes (`[Reactive]`, `[ReactiveCommand]` and the rest) are public types in the package's
`ReactiveUI.SourceGenerators` assembly. The generators do not declare them in your project, so assemblies that share
internals through `InternalsVisibleTo` never see two copies. Your project compiles against that assembly but keeps no
reference to it. If an older install added an `<IncludeAssets>` line without `compile`, remove it.

ReactiveUI 24 consumers can reference either `ReactiveUI`, for the API without System.Reactive, or `ReactiveUI.Reactive`,
for the System.Reactive API. The generators detect the API you reference.

For .NET Framework 4.8 and older, add [Polyfill](https://github.com/SimonCropp/Polyfill) or
[PolySharp](https://github.com/Sergio0694/PolySharp) and set `LangVersion` to 12.0 or later.

## Overview

| Attribute | What it generates |
|-----------|-------------------|
| `[Reactive]` | A property with change notification, from a field or from a partial property |
| `[ReactiveCommand]` | A `ReactiveCommand` property for a method, with `CanExecute`, schedulers and background options |
| `[IReactiveObject]` | The `IReactiveObject` implementation, for classes that cannot derive from `ReactiveObject` |
| `[IViewFor]`, `[IViewFor<T>]` | The `ViewModel` property and the `IViewFor<T>` implementation of a view |
| `[BindableDerivedList]` | A derived list for a `ReadOnlyObservableCollection<T>` field |
| `[ReactiveCollection]` | Change notifications for an `ObservableCollection<T>` field |
| `[RoutedControlHost]`, `[ViewModelControlHost]` | WinForms hosts that show the view for a view model |

## Roslyn and Visual Studio support

The package ships three analyzer bands. Your compiler picks the highest band at or below its own Roslyn version.

| Band (`analyzers/dotnet/...`) | Roslyn | Minimum tooling | Covers |
| --- | --- | --- | --- |
| `roslyn4.8` | 4.8 | Visual Studio 2022 17.8, .NET 8 SDK | The whole .NET 8 SDK line (Roslyn 4.8 to 4.11) and Visual Studio 17.12 and 17.13 |
| `roslyn4.14` | 4.14 | Visual Studio 2022 17.14, .NET 9 SDK | Partial-property `[Reactive]` |
| `roslyn5.0` | 5.0 | Visual Studio 2026, .NET 10 SDK | Partial-property `[Reactive]` |

The `roslyn4.8` band compiles against the Roslyn 4.8 API only, so it loads on every .NET 8 SDK band without the
`CS9057` error. See the
[Roslyn package version mappings](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support).

## Migration from ReactiveUI.Fody

1. Remove the ReactiveUI.Fody package, `FodyWeavers.xml` and `FodyWeavers.xsd`.
2. Add the ReactiveUI.SourceGenerators package.
3. Declare your classes and properties `partial`, put `[Reactive]` on them, and replace the
   `ReactiveUI.Fody.Helpers` using with `ReactiveUI.SourceGenerators`. `[Reactive]` also works on a backing field.
4. Replace Fody's `[ObservableAsProperty]` with [ReactiveUI.Binding's](#working-with-reactiveuibinding).
5. Rebuild and check the generated code.

## Reactive properties

```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [Reactive]
    private string _myProperty;
}
```

Options:

```csharp
// Access modifier of the set accessor (not needed for a partial property: declare it on the property).
[Reactive(SetModifier = AccessModifier.Protected)]
private string _first;

// A virtual property.
[Reactive(Inheritance = InheritanceModifier.Virtual)]
private string _second;

// Also raise notifications for other properties.
[Reactive(nameof(Other1), nameof(Other2))]
private string _third;

// Pass an attribute to the generated property.
[Reactive]
[property: JsonIgnore]
private string _fourth;
```

### From a partial property

Partial properties need C# 13 and Visual Studio 17.14 or later. Leave both accessors empty. `virtual`, `override`,
`required` and a `private` or `protected` set accessor are written on the property itself. C# 14 also allows an
initializer.

```csharp
public partial class MyReactiveClass : ReactiveObject
{
    [Reactive]
    public partial string MyProperty { get; set; }
}
```

### With other source generators

Roslyn does not order generators, and each one sees the original compilation. Code that one generator writes is
invisible to the others in the same project. For example, the `System.Text.Json` generator does not see a
`[JsonPropertyName]` that `[Reactive]` adds. To serialize `[Reactive]` properties with that generator, define the types
in assembly `A` and the `JsonSerializerContext` in assembly `B`, which references `A`.

## Reactive commands

```csharp
public partial class MyReactiveClass
{
    [ReactiveCommand]
    private void Execute() { }                                          // ExecuteCommand

    [ReactiveCommand]
    private string Echo(string parameter) => parameter;                  // parameter and result

    [ReactiveCommand]
    private async Task<string> LoadAsync(string id) => await Task.FromResult(id); // LoadCommand: Async is dropped

    [ReactiveCommand]
    private IObservable<string> Watch(string id) => Observable.Return(id);

    [ReactiveCommand]
    private async Task Delay(CancellationToken token) => await Task.Delay(1000, token);
}
```

### CanExecute

```csharp
private IObservable<bool> _canSearch;

[ReactiveCommand(CanExecute = nameof(_canSearch))]
private void Search() { }
```

### Schedulers and background work

`RunInBackground` runs a synchronous method on ReactiveUI's background scheduler. A task-returning method starts with
`Task.Run`, so the code before its first `await` leaves the calling thread. The command parameter and
`CancellationToken` are passed through. `BackgroundScheduler` chooses the scheduler a synchronous method runs on and
implies `RunInBackground`.

`OutputScheduler` and `BackgroundScheduler` name a scheduler as you would write it in the class: a field, property or
parameterless method of the class, or a static one elsewhere, such as `RxSchedulers.MainThreadScheduler`. A name that
does not resolve to a scheduler is reported as RXUISG0021.

```csharp
[ReactiveCommand(RunInBackground = true)]
private void Crunch() { }

[ReactiveCommand(BackgroundScheduler = nameof(_workScheduler))]
private void CrunchOnMine() { }

[ReactiveCommand(OutputScheduler = "RxSchedulers.MainThreadScheduler")]
private void ShowResult() { }

[ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]
private void Hidden() { }

[ReactiveCommand]
[property: JsonIgnore]
private void Save() { }
```

A `[ReactiveCommand]` method with more than one parameter besides a `CancellationToken` (RXUISG0002), an `async void`
method (RXUISG0008) and a scheduler name that does not resolve (RXUISG0021) are reported, because the generator skips
them.

## Views

`[IViewFor]` makes a partial control implement `IViewFor<TViewModel>`. It writes the `ViewModel` property and the
interface members, and registers the view with ReactiveUI.Binding's view locator at compile time.

```csharp
using ReactiveUI.SourceGenerators;

[IViewFor<LoginViewModel>]
public partial class LoginView : UserControl
{
}

// The view model by name, when its type is not in scope or is generic.
[IViewFor("Contracts.LoginViewModel")]
public partial class OtherView : UserControl
{
}
```

## Derived lists and collections

```csharp
public partial class MyReactiveClass : ReactiveObject
{
    // A ReadOnlyObservableCollection<string> property bound to a source list.
    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<string> _myList;

    // Change notifications on add, remove and replace.
    [ReactiveCollection]
    private ObservableCollection<string> _myCollection;
}
```

### `IReactiveObject` for classes that cannot derive from `ReactiveObject`

```csharp
[IReactiveObject]
public partial class MyReactiveClass
{
    [Reactive]
    private string _myProperty;
}
```

## WinForms control hosts

```csharp
using ReactiveUI.SourceGenerators.WinForms;

[RoutedControlHost("YourNameSpace.CustomControl")]
public partial class MyCustomRoutedControlHost;

[ViewModelControlHost("YourNameSpace.CustomControl")]
public partial class MyCustomViewModelControlHost;
```

Both resolve views through ReactiveUI's view locator, or through ReactiveUI.Binding's when your ReactiveUI is built on
it. With ReactiveUI.Binding 8.4.0 or later they follow their own properties through Binding's `ObservedProperty`.

## Working with ReactiveUI.Binding

ReactiveUI.Binding generates the code behind `[ObservableAsProperty]` and view registration. Use it for both.

### A read-only property backed by an observable

Mark a `partial` property `[ObservableAsProperty]` (C# 13 or later). The generator writes the property body and a helper
field named after it, which you assign with `ToProperty`:

```csharp
using ReactiveUI;
using ReactiveUI.Binding;

public partial class MyReactiveClass : ReactiveObject
{
    public MyReactiveClass(IObservable<string> source) =>
        _myPropertyHelper = source.ToProperty(this, x => x.MyProperty, initialValue: "Default Value");

    [ObservableAsProperty]
    public partial string MyProperty { get; }
}
```

ReactiveUI.Binding needs a ReactiveUI release built on it. On ReactiveUI 24.3 and earlier, write the helper by hand:

```csharp
private readonly ObservableAsPropertyHelper<string> _myPropertyHelper;

public string MyProperty => _myPropertyHelper.Value;
```

### Generated code and intercepted calls

One generator cannot see another generator's output. The attribute generators therefore never write a call that
ReactiveUI.Binding has to intercept (`WhenAny*`, `Bind`, `OneWayBind`, `BindCommand`). Generated code that follows a
property uses Binding's `ObservedProperty` when it is available, and `PropertyChanged` otherwise.

# Credits

Portions of this code base are based on and derived from
* [PolySharp](https://github.com/Sergio0694/PolySharp) library. Thanks go to @Sergio0694
* [Microsoft MVVM Community Toolkit](https://github.com/CommunityToolkit/dotnet)

See [our sponsors](https://www.reactiveui.net/sponsors/) for the people and companies that support the project.
