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

### Usage ReactiveCommand with CancellationToken
```csharp
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
    public MyReactiveCommand()
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
    public MyReactiveCommand()
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
