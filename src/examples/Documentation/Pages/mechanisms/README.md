# mechanisms

How a property reports a change, and which mechanism wins.

- `INotifyPropertyChanged`, `INotifyPropertyChanging`, `{Name}Changed` events and bindable objects.
- Affinity: how competing mechanisms are ranked.
- A custom `ICreatesObservableForProperty` and custom command binders.

```sh
dotnet run --project src/examples/Documentation/Pages/mechanisms -f net10.0
```
