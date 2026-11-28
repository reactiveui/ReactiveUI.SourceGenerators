# PlatformBindingsVerification

Check that `ToProperty` feeds a WPF, WinForms or MAUI control from a background thread. On Windows, run the desktop
checks from the repository root. For MAUI, install its workload and select `net11.0`. Both checks also run on .NET 10.

```sh
dotnet run --project src/examples/Platforms/PlatformBindingsVerification -f net11.0-windows10.0.19041.0 -- --verify
dotnet run --project src/examples/Platforms/PlatformBindingsVerification -f net11.0 -- --verify
```
