# ReactiveUI Source Generators Documentation

This documentation covers using ReactiveUI Source Generators to simplify and enhance the use of ReactiveUI objects.

- **Minimum Requirements**:
  - **C# Version**: 12.0
  - **Visual Studio Version**: 17.8.0
  - **ReactiveUI Version**: 19.5.31+

## Overview

ReactiveUI Source Generators automatically generate ReactiveUI objects to streamline your code. These Source Generators are designed to work with ReactiveUI V19.5.31+ and support the following features:

- `[Reactive]`
- `[ObservableAsProperty]`
- `[ObservableAsProperty(PropertyName = "ReadOnlyPropertyName")]`
- `[ReactiveCommand]`
- `[ReactiveCommand(CanExecute = nameof(IObservableBoolName))]` with CanExecute
- `[ReactiveCommand][property: AttributeToAddToCommand]` with Attribute passthrough
- `[IViewFor(nameof(ViewModelName))]`
- `[RoutedControlHost("YourNameSpace.CustomControl")]`
- `[ViewModelControlHost("YourNameSpace.CustomControl")]`

### Compatibility Notes
- For ReactiveUI versions **older than V19.5.31**, all `[ReactiveCommand]` options are supported except for async methods with a `CancellationToken`.
- For **.NET Framework 4.8 and older**, add [Polyfill by Simon Cropp](https://github.com/Fody/Polyfill) or [PolySharp by Manuel Römer](https://github.com/manuelroemer/PolySharp) to your project and set the `LangVersion` to 12.0 or later in your project file.

For more information on analyzer codes, see the [analyzer codes documentation](https://github.com/reactiveui/ReactiveUI.SourceGenerators/blob/main/src/ReactiveUI.SourceGenerators/AnalyzerReleases.Shipped.md).

---

## Supported Attributes

### `[Reactive]`
Marks properties as reactive, generating getter and setter code.

### `[ObservableAsProperty]`
Generates read-only properties backed by an `ObservableAsPropertyHelper` based on an `IObservable`.

### `[ReactiveCommand]`
Generates commands, with options to add attributes or enable `CanExecute` functionality.

### `[IViewFor]`
Links a view to a view model for data binding.

### `[RoutedControlHost]` and `[ViewModelControlHost]`
Platform-specific attributes for control hosting in WinForms applications.

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

## ObservableAsPropertyHelper properties
Similarly, to declare output properties, the code looks like this:
```csharp
public partial class MyReactiveClass : ReactiveObject
{
    ObservableAsPropertyHelper<string> _firstName;

    public MyReactiveClass()
    {
        _firstName = firstNameObservable
            .ToProperty(this, x => x.FirstName);
    }

    public string FirstName => _firstName.Value;

    private IObservable<string> firstNameObservable() => Observable.Return("Test");
}
```

With ReactiveUI.Fody, you can simply declare a read-only property using the [ObservableAsProperty] attribute, using either option of the two options shown below.
```csharp
[ObservableAsProperty]
public string FirstName { get; }
```

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

## Usage ObservableAsPropertyHelper `[ObservableAsProperty]`

ObservableAsPropertyHelper is used to create a read-only property from an IObservable. The generated code will create a backing field and a property that returns the value of the backing field. The backing field is initialized with the value of the IObservable when the class is instantiated.

A private field is created with the name of the property prefixed with an underscore. The field is initialized with the value of the IObservable when the class is instantiated. The property is created with the same name as the field without the underscore. The property returns the value of the field until initialized, then it returns the value of the IObservable.

You can define the name of the property by using the PropertyName parameter. If you do not define the PropertyName, the property name will be the same as the field name without the underscore.

### Usage ObservableAsPropertyHelper with Field
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [ObservableAsProperty]
    private string _myProperty = "Default Value";

    public MyReactiveClass()
    {
        _myPrpertyHelper = MyPropertyObservable()
            .ToProperty(this, x => x.MyProperty);
    }

    IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
```

### Usage ObservableAsPropertyHelper with Field and non readonly nullable OAPH field
```csharp

### Usage ObservableAsPropertyHelper with Observable Property
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject, IActivatableViewModel
{
    [ObservableAsProperty(ReadOnly = false)]
    private string _myProperty = "Default Value";

    public MyReactiveClass()
    {
        this.WhenActivated(disposables =>
        {
            _myPrpertyHelper = MyPropertyObservable()
                .ToProperty(this, x => x.MyProperty)
                .DisposeWith(disposables);
        });
    }

    IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
```

### Usage ObservableAsPropertyHelper with Observable Property and specific PropertyName
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
    public MyReactiveClass()
    {
        // default value for TestValueProperty prior to initialization.
        _testValueProperty = "Test Value Pre Init";

        // Initialize generated _testValuePropertyHelper
        // for the generated TestValueProperty
        InitializeOAPH();
    }

    [ObservableAsProperty(PropertyName = TestValueProperty)]
    IObservable<string> MyObservable => Observable.Return("Test Value");
}
```

### Usage ObservableAsPropertyHelper with Observable Method

NOTE: This does not currently support methods with parameters
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
    public MyReactiveClass()
    { 
        // Initialize generated _myObservablePropertyHelper
        // for the generated MyObservableProperty
        InitializeOAPH();
    }

    [ObservableAsProperty]
    IObservable<string> MyObservable() => Observable.Return("Test Value");
}
```

### Usage ObservableAsPropertyHelper with Observable Method and specific PropertyName
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
    public MyReactiveClass()
    { 
        // Initialize generated _testValuePropertyHelper
        // for the generated TestValueProperty
        InitializeOAPH();
    }

    [ObservableAsProperty(PropertyName = TestValueProperty)]
    IObservable<string> MyObservable() => Observable.Return("Test Value");
}
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

### Usage ReactiveCommand with parameter and return value
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    [ReactiveCommand]
    private string Execute(string parameter) => parameter;
}
```

### Usage ReactiveCommand with parameter and async return value
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
        MyReactiveClass = new MyReactiveClass();
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
        MyReactiveClass = new MyReactiveClass();
    }
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


### TODO:
- Add ObservableAsProperty to generate from a IObservable method with parameters.
