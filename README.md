# ReactiveUI.SourceGenerators
Use source generators to generate ReactiveUI objects. 

Not taking public contributions at this time  

## Usage Reactive property `[Reactive]`
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [Reactive] private string _myProperty;
}
```

## Usage ObservableAsPropertyHelper `[ObservableAsProperty]`
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
    [ObservableAsProperty] private string _myProperty;
}
```

## Usage ReactiveCommand `[ReactiveCommand]`

### Usage ReactiveCommand without parameter
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveCommand()
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
    public MyReactiveCommand()
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
    public MyReactiveCommand()
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
    public MyReactiveCommand()
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
    public MyReactiveCommand()
    {
        InitializeCommands();
    }

    [ReactiveCommand]
    private IObservable<string> Execute(string parameter) => Observable.Return(parameter);
}
```
