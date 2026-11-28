# sourcegenerators

Using ReactiveUI.Binding with members that ReactiveUI.SourceGenerators writes.

- `[Reactive]`, `[ReactiveCommand]` and `[IViewFor]` members used in bindings.
- Why a binding can follow a member that another generator wrote.
- How `[ReactiveCollection]` switches to a replacement collection and `[BindableDerivedList]` exposes a live read-only list. [Review queue](ReviewQueue.cs)

```sh
dotnet run --project src/examples/Documentation/Pages/sourcegenerators -f net10.0
```
