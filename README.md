# ReactiveUI.SourceGenerators
Use source generators to generate ReactiveUI objects. 

Not taking public contributions at this time

These Source Generators were designed to work in full with ReactiveUI V19.5.31 and newer supporting all features, currently:
- [Reactive]
- [ObservableAsProperty]
- [ReactiveCommand]

Versions older than V19.5.31 to this:
- [Reactive] fully supported, 
- [ObservableAsProperty] fully supported, 
- [ReactiveCommand] all supported except Cancellation Token asnyc methods.

# Historical ways
## Read-write properties
Typically properties are declared like this:

```csharp
private string _name;
public string Name 
{
    get => _name;
    set => this.RaiseAndSetIfChanged(ref _name, value);
}
```

Before these Source Generators were avaliable we used ReactiveUI.Fody.
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

## Usage ObservableAsPropertyHelper `[ObservableAsProperty]`
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [ObservableAsProperty]
    private string _myProperty;
}
```

## Usage ReactiveCommand `[ReactiveCommand]`

### Usage ReactiveCommand without parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private void Execute() { }
}
```

### Usage ReactiveCommand with parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private void Execute(string parameter) { }
}
```

### Usage ReactiveCommand with parameter and return value
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private string Execute(string parameter) => parameter;
}
```

### Usage ReactiveCommand with parameter and async return value
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private async Task<string> Execute(string parameter) => await Task.FromResult(parameter);
}
```

### Usage ReactiveCommand with IObservable return value
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private IObservable<string> Execute(string parameter) => Observable.Return(parameter);
}
```

### Usage ReactiveCommand with CancellationToken
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private async Task Execute(CancellationToken token) => await Task.Delay(1000, token);
}
```

### Usage ReactiveCommand with CancellationToken and parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveClass()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private async Task<string> Execute(string parameter, CancellationToken token)
    {
        await Task.Delay(1000, token);
        return parameter;
    }
}
```
