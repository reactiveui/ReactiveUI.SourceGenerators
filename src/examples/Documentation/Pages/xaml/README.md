# xaml

Binding controls named in MAUI and Avalonia XAML.

- `x:Name` controls become fields the generator can bind, including `x:FieldModifier`.
- Both the 2006 and 2009 XAML namespaces are read.

The MAUI and Avalonia folders need their workloads.

```sh
dotnet run --project src/examples/Documentation/Pages/xaml -f net10.0
```
