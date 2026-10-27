# ToPropertyVerification

A self-check of `ToProperty` and `[ObservableAsProperty]` on every runtime target, from .NET Framework 4.6.2 to
.NET 11. Each view model raises its notifications a different way.

```sh
dotnet run --project src/examples/Platforms/ToPropertyVerification -f net10.0
```
