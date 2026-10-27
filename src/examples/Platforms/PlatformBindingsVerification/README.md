# PlatformBindingsVerification

Checks that `ToProperty` feeds a real WPF, WinForms or MAUI control from a background thread. WPF and WinForms need
Windows; MAUI needs its workload.

```sh
dotnet run --project src/examples/Platforms/PlatformBindingsVerification -f net10.0-windows10.0.19041.0 -- --verify
```
