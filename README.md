# ReactiveUI Source Generators Documentation

This documentation covers using ReactiveUI Source Generators to simplify and enhance the use of ReactiveUI objects.

- **Minimum Requirements**:
  - **C# Version**: 12.0
  - **Visual Studio Version**: 17.14.0 (Roslyn 4.14) — or Visual Studio 2026 (Roslyn 5.0)
  - **ReactiveUI Version**: 23.2.28+

## Table of Contents
- [Installation](#installation)
- [Overview](#overview)
- [Supported Attributes & Features](#supported-attributes)
- [Detailed Usage](#welcome-to-a-new-way---source-generators)
  - [Reactive](#reactive)
  - [ReactiveCommand](#reactivecommand)
  - [IViewFor](#iviewfor)
  - [BindableDerivedList](#usage-readonlyobservablecollection)
  - [Platform-Specific Attributes](#platform-specific-attributes)
- [Moved to ReactiveUI.Binding](#moved-to-reactiveuibinding)
- [Compatibility Notes](#compatibility-notes)
- [Credits](#credits)

## Installation

dotnet add package ReactiveUI.SourceGenerators

Ensure the package is loaded with `PrivateAssets="all"` to avoid issues with generated code in consuming projects.

ReactiveUI V24.x.x consumers can reference either `ReactiveUI` for the primitives-based API without
System.Reactive, or `ReactiveUI.Reactive` for the System.Reactive-based API. The generators detect
the referenced API surface automatically. ReactiveUI releases from 23.2.28 remain supported.

## Overview

ReactiveUI Source Generators automatically generate ReactiveUI objects to streamline your code. These Source Generators are designed to work with ReactiveUI V23.2.28+ and support the following features:

- `[Reactive]` With field and access modifiers, partial property support (C# 13 Visual Studio Version 17.12.0), partial properties with initializer support (C# 14+/preview only)
- `[Reactive(SetModifier = AccessModifier.Protected)]` With field and access modifiers, (Not Required for partial properties, configure set accessor with the property declaration).
- `[Reactive(Inheritance = InheritanceModifier.Virtual)]` With field and access modifiers. This will generate a virtual property.
- `[Reactive(UseRequired = true)]` With field and access modifiers. This will generate a required property, (Not Required for partial properties, use required keyword for property declaration).
- `[Reactive(nameof(RaiseProperty1), nameof(RaiseProperty2))]` With field and property changed notification for additional properties.
- `[ReactiveCommand]`
- `[ReactiveCommand(RunInBackground = true)]` runs a synchronous command on ReactiveUI's background scheduler
- `[ReactiveCommand(CanExecute = nameof(IObservableBoolName))]` with CanExecute
- `[ReactiveCommand(OutputScheduler = "RxSchedulers.MainThreadScheduler")]` using a ReactiveUI Scheduler
- `[ReactiveCommand(OutputScheduler = nameof(_isheduler))]` using a Scheduler defined in the class
- `[ReactiveCommand][property: AttributeToAddToCommand]` with Attribute passthrough
- `[ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]` sets the access modifier of the generated command property
- `[IViewFor(nameof(ViewModelName))]`
- `[IViewFor<YourViewModelType>]`
- `[IViewFor("YourNameSpace.YourGenericViewModel<int>")]` Generic
- `[RoutedControlHost("YourNameSpace.CustomControl")]`
- `[ViewModelControlHost("YourNameSpace.CustomControl")]`
- `[BindableDerivedList]` Generates a derived list from a ReadOnlyObservableCollection backing field
- `[ReactiveCollection]` Generates property changed notifications on add, remove, new actions on a ObservableCollection backing field
- `[IReactiveObject]` Generates IReactiveObject implementation for classes not able to inherit from ReactiveObject

`[ObservableAsProperty]` and IViewFor view registration moved to ReactiveUI.Binding; see [Moved to ReactiveUI.Binding](#moved-to-reactiveuibinding).

### Compatibility Notes
- For **.NET Framework 4.8 and older**, add [Polyfill by Simon Cropp](https://github.com/SimonCropp/Polyfill) or [PolySharp by Sergio Pedri](https://github.com/Sergio0694/PolySharp) to your project and set the `LangVersion` to 12.0 or later in your project file.

### Roslyn / Visual Studio support

The generators ship three Roslyn analyzer bands and the matching one is selected automatically by your compiler (it picks the highest band that is less than or equal to its own Roslyn version):

| Band (`analyzers/dotnet/...`) | Roslyn | Minimum tooling | Covers |
| --- | --- | --- | --- |
| `roslyn4.8` | 4.8 | Visual Studio 2022 17.8, .NET 8 SDK (8.0.1xx) | The whole .NET 8 SDK line (Roslyn 4.8–4.11) plus VS 17.12/17.13 (Roslyn 4.12/4.13) |
| `roslyn4.14` | 4.14 | Visual Studio 2022 17.14, .NET SDK 9.0.3xx | VS 17.14, current .NET 9 SDK |
| `roslyn5.0` | 5.0 | Visual Studio 2026, .NET 10 SDK | VS 2026, .NET 10 SDK |

The **`roslyn4.8` baseline keeps the entire .NET 8 SDK line working** — .NET 8 is supported through November 2026 and its SDK feature bands carry Roslyn 4.8 (`8.0.1xx`) through 4.11 (`8.0.4xx`); all of them select the `roslyn4.8` band. The 4.8 band compiles against the Roslyn 4.8 API only (the 4.12+ surface is compiled out), so it loads on those compilers without the `CS9057` "analyzer references a newer compiler" error. See the official [Roslyn package version mappings](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support).

| Visual Studio 2022 LTSC | Roslyn | .NET SDK band |
| --- | --- | --- |
| 17.8 | 4.8 | 8.0.1xx |
| 17.9 | 4.9 | 8.0.2xx |
| 17.10 | 4.10 | 8.0.3xx |
| 17.11 | 4.11 | 8.0.4xx |
| 17.14 | 4.14 | 9.0.3xx |

For more information on analyzer codes, see the [analyzer codes documentation](https://github.com/reactiveui/ReactiveUI.SourceGenerators/blob/main/src/ReactiveUI.SourceGenerators/AnalyzerReleases.Shipped.md).

---

## Supported Attributes

### `[Reactive]`
Marks properties as reactive, generating getter and setter code.

### `[ReactiveCommand]`
Generates commands, with options to add attributes or enable `CanExecute` functionality.

### `[IViewFor]`
Links a view to a view model for data binding. Supports generic types.

### `[RoutedControlHost]` and `[ViewModelControlHost]`
Platform-specific attributes for control hosting in WinForms applications.

### `[BindableDerivedList]`
Generates a derived list from a `ReadOnlyObservableCollection` backing field.

### `[ReactiveCollection]`
Generates property changed notifications on add, remove, and new actions on an `ObservableCollection` backing field.

### `[IReactiveObject]`
Generates `IReactiveObject` implementation for classes not able to inherit from `ReactiveObject`.

## Historical Approach

### Read-Write Properties
Previously, properties were declared like this:

```csharp
private string _name;
public string Name 
{
    get => _name;
    set => this.RaiseAndSetIfChanged(ref _name, value);
}
```

Before these Source Generators were available we used ReactiveUI.Fody.
With ReactiveUI.Fody the `[Reactive]` Attribute was placed on a Public Property with Auto get / set properties, the generated code from the Source Generator and the Injected code using Fody are very similar with the exception of the Attributes.

```csharp
[Reactive]
public string Name { get; set; }
```

## Migration from ReactiveUI.Fody
To migrate from ReactiveUI.Fody to ReactiveUI.SourceGenerators, follow these steps:
1. **Remove ReactiveUI.Fody**: Uninstall the ReactiveUI.Fody NuGet package from your project.
Remove the Fody support files from the project directory

- FodyWeaver.xml
- FodyWeavers.xsd
2. **Install ReactiveUI.SourceGenerators**: Add the ReactiveUI.SourceGenerators NuGet package
3. **Update Property Declarations**: Place `[Reactive]` attributes on your properties as shown in the examples below and ensure that your classes and properties are declared as `partial`. You can also use field backing for `[Reactive]` properties as shown in the examples. This is a change from ReactiveUI.Fody which only supported property backing.
Remove using directives for `ReactiveUI.Fody.Helpers` and add using directives for `ReactiveUI.SourceGenerators`.
Fody's `[ObservableAsProperty]` has no equivalent in this package; see [A read-only property backed by an observable](#a-read-only-property-backed-by-an-observable).
4. **Rebuild Your Project**: Ensure that your project builds successfully and that the generated code behaves as expected.

# Welcome to a new way - Source Generators

## Usage Reactive property `[Reactive]`
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [Reactive]
    private string _myProperty;
}
```

### Usage Reactive property with set Access Modifier
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [Reactive(SetModifier = AccessModifier.Protected)]
    private string _myProperty;
}
```

### Usage Reactive property with property Attribute pass through
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [Reactive]
    [property: JsonIgnore]
    private string _myProperty;
}
```

### Usage Reactive property from partial property

Partial properties are supported in C# 13 and Visual Studio 17.12.0 and later.
Both the getter and setter must be empty, and the `[Reactive]` attribute must be placed on the property.
Override and Virtual properties are supported.
Set Access Modifier is also supported on partial properties.

```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [Reactive]
    public partial string MyProperty { get; set; }
}
```

### Usage Reactive property from partial property with default value
Partial properties with initial value are supported in C# preview and Visual Studio 17.12.0 and later.
Both the getter and setter must be empty, and the `[Reactive]` attribute must be placed on the property.
Override and Virtual properties are supported.
Set Access Modifier is also supported on partial properties.
```csharp
using ReactiveUI.SourceGenerators;
public partial class MyReactiveClass : ReactiveObject
{
    [Reactive]
    public partial string MyProperty { get; set; } = "Default Value"
}
```

### Usage Reactive property with other source generators

Roslyn source generators don’t have a defined run order and each generator sees the same initial compilation.
Code/attributes that one generator emits aren’t visible to other generators in the same compilation round,
so adding `[JsonPropertyName]`/`[JsonInclude]` from `ReactiveUI.SourceGenerators` won’t cause the `System.Text.Json`
source generator to pick them up in that project. That’s by design of the generator pipeline (no inter-generator dependencies / ordering).

So `System.Text.Json` needs special care. In the case that you want to Json-serialize `[Reactive]`
properties, and you want to use the `System.Text.Json` source generator they must run in different
assemblies. The same applies to other source generators depending on the output of `ReactiveUI.SourceGenerators`.

Define types with `[Reactive]` properties in assembly `A`, and then define the
`System.Text.Json.JsonSerializerContext` source generation context in assembly `B`, and let
`B` reference `A`.

## Moved to ReactiveUI.Binding

ReactiveUI's binding engine, [ReactiveUI.Binding](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators),
now provides two features this package used to generate. They have been removed from ReactiveUI.SourceGenerators.

| Removed | Use instead |
| --- | --- |
| `[ObservableAsProperty]` on a field, method, observable property or partial property, and the generated `InitializeOAPH()` | ReactiveUI.Binding's `[ObservableAsProperty]` on a `partial` property, assigned with `ToProperty` |
| `RegisterViewsForViewModelsSourceGenerated()`, the `RegistrationType` and `ViewModelRegistrationType` options of `[IViewFor]`, and `SplatRegistrationType` | ReactiveUI.Binding's view locator, which registers views at compile time |
| RXUISG0014, RXUISG0017 and the RXUISPR0002 suppression | Nothing: they only applied to `[ObservableAsProperty]` |

`[IViewFor]` still generates the `ViewModel` property and the `IViewFor<T>` implementation for each UI platform.

The ReactiveUI.Binding replacements need a ReactiveUI release built on ReactiveUI.Binding. ReactiveUI 24.3 and earlier
are not: there, ReactiveUI's own `ObservableAsPropertyHelper<T>` and `IViewFor<T>` are the ones in use, so
ReactiveUI.Binding's `ToProperty` does not produce the helper its generated property expects, and its view locator does
not see ReactiveUI's `IViewFor<T>`. On those releases, use the hand-written forms below.

### A read-only property backed by an observable

With ReactiveUI.Binding, mark a `partial` property `[ObservableAsProperty]` (C# 13 or later). The generator writes the
property's body and a helper field named after it, which you assign with `ToProperty`:

```csharp
using ReactiveUI;
using ReactiveUI.Binding;

public partial class MyReactiveClass : ReactiveObject
{
    public MyReactiveClass(IObservable<string> myPropertySource) =>
        _myPropertyHelper = myPropertySource.ToProperty(this, x => x.MyProperty, initialValue: "Default Value");

    [ObservableAsProperty]
    public partial string MyProperty { get; }
}
```

Without ReactiveUI.Binding, or on ReactiveUI 24.3 and earlier, write the helper with ReactiveUI's `ToProperty`:

```csharp
using ReactiveUI;

public class MyReactiveClass : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<string> _myPropertyHelper;

    public MyReactiveClass(IObservable<string> myPropertySource) =>
        _myPropertyHelper = myPropertySource.ToProperty(this, x => x.MyProperty, initialValue: "Default Value");

    public string MyProperty => _myPropertyHelper.Value;
}
```

### Registering views

ReactiveUI.Binding's view locator registers every class whose declaration implements `IViewFor<T>`. Source generators
cannot see each other's output, so the view locator does not see the interface `[IViewFor]` adds. List the interface
on the class as well; the generated members still implement it:

```csharp
[IViewFor<LoginViewModel>]
public partial class LoginView : UserControl, IViewFor<LoginViewModel>
{
}
```

Without ReactiveUI.Binding, or on ReactiveUI 24.3 and earlier, register each view with Splat:

```csharp
AppLocator.CurrentMutable.Register<IViewFor<LoginViewModel>>(static () => new LoginView());
```

## Usage ReactiveCommand `[ReactiveCommand]`

### Usage ReactiveCommand without parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private void Execute() { }
}
```

### Usage ReactiveCommand with parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private void Execute(string parameter) { }
}
```

### Usage ReactiveCommand on the background scheduler

Use `RunInBackground` for synchronous command methods that should be created with `ReactiveCommand.CreateRunInBackground`. Task- and observable-returning methods continue to use their asynchronous ReactiveCommand factories.

```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand(RunInBackground = true)]
    private void ExecuteExpensiveWork() { }
}
```

### Usage ReactiveCommand with parameter and return value
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private string Execute(string parameter) => parameter;
}
```

### Usage ReactiveCommand with parameter and async return value.

Note: the Async suffix is removed from the generated command

```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private async Task<string> ExecuteAsync(string parameter) => await Task.FromResult(parameter);

    // Generates the following code ExecuteCommand, Note the Async suffix is removed
}
```

### Usage ReactiveCommand with IObservable return value
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private IObservable<string> Execute(string parameter) => Observable.Return(parameter);
}
```

### Usage ReactiveCommand with CancellationToken
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private async Task Execute(CancellationToken token) => await Task.Delay(1000, token);
}
```

### Usage ReactiveCommand with CancellationToken and parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private async Task<string> Execute(string parameter, CancellationToken token)
    {
        await Task.Delay(1000, token);
        return parameter;
    }
}
```

### Usage ReactiveCommand with CanExecute
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    private IObservable<bool> _canExecute;

    [Reactive]
    private string _myProperty1;

    [Reactive]
    private string _myProperty2;

    public MyReactiveClass()
    {
        _canExecute = this.WhenAnyValue(x => x.MyProperty1, x => x.MyProperty2, (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));
    }

    [ReactiveCommand(CanExecute = nameof(_canExecute))]
    private void Search() { }
}
```

### Usage ReactiveCommand with property Attribute pass through
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    private IObservable<bool> _canExecute;

    [Reactive]
    private string _myProperty1;

    [Reactive]
    private string _myProperty2;

    public MyReactiveClass()
    {
        _canExecute = this.WhenAnyValue(x => x.MyProperty1, x => x.MyProperty2, (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));
    }

    [ReactiveCommand(CanExecute = nameof(_canExecute))]
    [property: JsonIgnore]
    private void Search() { }
}
```

### Usage ReactiveCommand with ReactiveUI OutputScheduler
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand(OutputScheduler = "RxSchedulers.MainThreadScheduler")]
    private void Execute() { }
}
```

### Usage ReactiveCommand with custom OutputScheduler
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    private IScheduler _customScheduler = new TestScheduler();

    [ReactiveCommand(OutputScheduler = nameof(_customScheduler))]
    private void Execute() { }
}
```

### Usage ReactiveCommand with AccessModifier
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]
    private void Execute() { }
}
```

## Usage IViewFor `[IViewFor(nameof(ViewModelName))]`

### IViewFor usage

IViewFor is used to link a View to a ViewModel, this is used to link the ViewModel to the View in a way that ReactiveUI can use it to bind the ViewModel to the View.
The ViewModel is passed as a type to the IViewFor Attribute using generics.
The class must inherit from a UI Control from any of the following platforms and namespaces:
- Maui (Microsoft.Maui)
- WinUI (Microsoft.UI.Xaml)
- WPF (System.Windows or System.Windows.Controls)
- WinForms (System.Windows.Forms)
- Avalonia (Avalonia)
- Uno (Windows.UI.Xaml).

### Usage IViewFor with ViewModel Name - Generic Types should be used with the fully qualified name, otherwise use nameof(ViewModelTypeName)
```csharp
using ReactiveUI.SourceGenerators;

[IViewFor("MyReactiveGenericClass<int>")]
public partial class MyReactiveControl : UserControl
{
    public MyReactiveControl()
    {
        InitializeComponent();
        ViewModel = new MyReactiveClass();
    }
}
```

### Usage IViewFor with ViewModel Type

```csharp
using ReactiveUI.SourceGenerators;

[IViewFor<MyReactiveClass>]
public partial class MyReactiveControl : UserControl
{
    public MyReactiveControl()
    {
        InitializeComponent();
        ViewModel = new MyReactiveClass();
    }
}
```

### Usage ReadOnlyObservableCollection

```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<string> _myList;
}
```

## Platform specific Attributes

### WinForms

#### RoutedControlHost
```csharp
using ReactiveUI.SourceGenerators.WinForms;

[RoutedControlHost("YourNameSpace.CustomControl")]
public partial class MyCustomRoutedControlHost;
```

#### ViewModelControlHost
```csharp
using ReactiveUI.SourceGenerators.WinForms;

[ViewModelControlHost("YourNameSpace.CustomControl")]
public partial class MyCustomViewModelControlHost;
```

### ReactiveCollection
```csharp
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [ReactiveCollection]
    private ObservableCollection<string> _myCollection;

    public MyReactiveClass()
    {
        MyCollection = new ObservableCollection<string>();
        _myCollection.Add("Item 1");
    }
}
```

### IReactiveObject implementation for classes not able to inherit from ReactiveObject
```csharp
using ReactiveUI;
using ReactiveUI.SourceGenerators;

[IReactiveObject]
public partial class MyReactiveClass
{
    [Reactive]
    private string _myProperty;
}
```

# Credits

Portions of this code base are based on and derived from
* [PolySharp](https://github.com/Sergio0694/PolySharp) library. Thanks go to @Sergio0694
* [Microsoft MVVM Community Toolkit](https://github.com/CommunityToolkit/dotnet)

## Sponsors

[JetBrains](https://www.jetbrains.com/) gives ReactiveUI's maintainers licences for its tools through its
[open source support programme](https://www.jetbrains.com/community/opensource/).
[Anthropic](https://www.anthropic.com/) supports them with [Claude](https://claude.com/) through
[Claude for Open Source](https://claude.com/contact-sales/claude-for-oss).
[OpenAI](https://openai.com/) supports them with [Codex](https://openai.com/codex/) through
[Codex for Open Source](https://developers.openai.com/community/codex-for-oss).

[![JetBrains](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/jetbrains.svg)](https://www.jetbrains.com/)
[![Claude by Anthropic](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/claude.svg)](https://claude.com/)
[![OpenAI](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/openai.svg)](https://openai.com/codex/)

See [our sponsors](https://www.reactiveui.net/sponsors/) for more information.
JetBrains, Claude, Anthropic, OpenAI and Codex names and logos are trademarks of their respective owners.
