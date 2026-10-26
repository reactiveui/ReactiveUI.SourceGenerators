<div align="center">

<img src="images/logo.png" width="160" alt="ReactiveUI">

# ReactiveUI.SourceGenerators

**Compile-time code for ReactiveUI. No reflection. Trim and NativeAOT safe.**

[![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.svg?label=ReactiveUI.Binding)](https://www.nuget.org/packages/ReactiveUI.Binding)
[![NuGet](https://img.shields.io/nuget/v/ReactiveUI.SourceGenerators.svg?label=ReactiveUI.SourceGenerators)](https://www.nuget.org/packages/ReactiveUI.SourceGenerators)
[![Build](https://github.com/reactiveui/ReactiveUI.SourceGenerators/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/ReactiveUI.SourceGenerators/actions/workflows/ci-build.yml)
[![Coverage](https://codecov.io/gh/reactiveui/ReactiveUI.SourceGenerators/branch/main/graph/badge.svg)](https://codecov.io/gh/reactiveui/ReactiveUI.SourceGenerators)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Slack](https://img.shields.io/badge/chat-slack-blue.svg)](https://reactiveui.net/slack)

</div>

Two families of Roslyn source generators that work together:

| | Attribute generators | Binding generators |
|---|---|---|
| **Package** | `ReactiveUI.SourceGenerators` | `ReactiveUI.Binding` (and platform packages) |
| **You write** | `[Reactive]`, `[ReactiveCommand]`, `[IViewFor]` | `WhenChanged`, `Bind`, `ToProperty`, `[ObservableAsProperty]` |
| **You get** | Properties, commands, views, collections | Observations and bindings, with no expression trees |
| **Guide** | [Attribute generators](src/ReactiveUI.SourceGenerators/README.md) | [Binding reference](docs/ReactiveUI.Binding.md) |

## Quick start

**Attributes.** Mark a field. The property, its change notification and the command are written for you.

```csharp
public partial class LoginViewModel : ReactiveObject
{
    [Reactive] private string _userName = "";

    [ReactiveCommand]
    private void SignIn() { }
}
```

**Binding.** Name the path as a lambda. The generator writes the code that attaches to each link of it.

```csharp
IObservable<string> city = viewModel.WhenChanged(x => x.Address.City);

view.OneWayBind(viewModel, x => x.UserName, v => v.NameLabel.Text);
```

## Install

```bash
dotnet add package ReactiveUI.SourceGenerators   # attribute generators
dotnet add package ReactiveUI.Binding            # binding generators, no System.Reactive
dotnet add package ReactiveUI.Binding.Reactive   # binding generators on System.Reactive
```

Reference one binding runtime package, not both. Add `PrivateAssets="all"` to `ReactiveUI.SourceGenerators`.

## Packages

| Package | Purpose |
|---------|---------|
| `ReactiveUI.SourceGenerators` | Attribute generators |
| `ReactiveUI.Binding`, `ReactiveUI.Binding.Reactive` | Binding runtime, generator and analyzer |
| `ReactiveUI.Binding.Wpf`, `.WinForms`, `.Maui`, `.Avalonia`, `.Uno` | Platform property observation and view thread routing |
| `*.Reactive` platform packages | The same, for System.Reactive apps |

## Requirements

| | Attribute generators | Binding generators |
|---|---|---|
| Language | C# 12 | C# 7.3 for generated code; interceptors need C# 11 |
| Compiler | Roslyn 4.8 or later | Roslyn 4.8 or later; interceptors need 4.13 |
| Runtime | .NET 8 to 11, .NET Framework 4.6.2 to 4.8.1 | The same |

## Examples

Every example runs and prints what it does.

| Example | Shows |
|---------|-------|
| [index](src/examples/Documentation/Pages/index) | Your first binding |
| [observing](src/examples/Documentation/Pages/observing) | `WhenChanged`, `WhenAny`, `WhenAnyValue`, `WhenAnyObservable` |
| [bindings](src/examples/Documentation/Pages/bindings) | One-way, two-way, command and interaction bindings |
| [properties](src/examples/Documentation/Pages/properties) | `ToProperty` and `[ObservableAsProperty]` |
| [converters](src/examples/Documentation/Pages/converters) | Built-in type converters |
| [custom-converters](src/examples/Documentation/Pages/custom-converters) | Writing your own converters |
| [mechanisms](src/examples/Documentation/Pages/mechanisms) | How a property reports a change |
| [views](src/examples/Documentation/Pages/views) | The view locator and `IViewFor<T>` |
| [threading](src/examples/Documentation/Pages/threading) | Which thread a binding writes on |
| [xaml](src/examples/Documentation/Pages/xaml) | Controls named in MAUI and Avalonia XAML |
| [sourcegenerators](src/examples/Documentation/Pages/sourcegenerators) | Binding to members the attribute generators write |
| [unsafe](src/examples/Documentation/Pages/unsafe) | Run-time expressions |
| [setup](src/examples/Documentation/Pages/setup) | Configuration, analyzer and NativeAOT |
| [Execute](src/tests/ReactiveUI.SourceGenerators.Execute) | The attribute generators in use |
| [ToPropertyVerification](src/examples/Platforms/ToPropertyVerification) | `ToProperty` on every runtime target |
| [PlatformBindingsVerification](src/examples/Platforms/PlatformBindingsVerification) | `ToProperty` feeding real WPF, WinForms and MAUI controls |

## Documentation

| Topic | Where |
|-------|-------|
| Attribute generators | [src/ReactiveUI.SourceGenerators](src/ReactiveUI.SourceGenerators/README.md) |
| Binding reference: APIs, diagnostics, threading, differences from ReactiveUI | [docs/ReactiveUI.Binding.md](docs/ReactiveUI.Binding.md) |
| Benchmarks and current figures | [src/benchmarks](src/benchmarks/README.md) |
| Contributing and repository layout | [CLAUDE.md](CLAUDE.md) |
| ReactiveUI handbook | [reactiveui.net](https://www.reactiveui.net/documentation/handbook/) |

## Core team

<table>
  <tbody>
    <tr>
      <td align="center" valign="top">
        <img width="100" height="100" src="https://github.com/ChrisPulman.png?s=150">
        <br>
        <a href="https://github.com/ChrisPulman">Chris Pulman</a>
        <p>London, UK</p>
      </td>
      <td align="center" valign="top">
        <img width="100" height="100" src="https://github.com/glennawatson.png?s=150">
        <br>
        <a href="https://github.com/glennawatson">Glenn Watson</a>
        <p>Melbourne, Australia</p>
      </td>
    </tr>
  </tbody>
</table>

## Contribute

The project uses the MIT license. Everyone is welcome, including first-time contributors.

- [Answer questions on GitHub Discussions](https://github.com/reactiveui/ReactiveUI.SourceGenerators/discussions)
- Improve the docs where something is missing or unclear.
- Contribute code. Read [CLAUDE.md](CLAUDE.md) first.

## Credits

Portions of this code are derived from [PolySharp](https://github.com/Sergio0694/PolySharp) and the
[Microsoft MVVM Community Toolkit](https://github.com/CommunityToolkit/dotnet).

## Sponsors

[JetBrains](https://www.jetbrains.com/) supports ReactiveUI's maintainers through its
[open source programme](https://www.jetbrains.com/community/opensource/).
[Anthropic](https://www.anthropic.com/) supports them with [Claude](https://claude.com/), and
[OpenAI](https://openai.com/) with [Codex](https://openai.com/codex/).

[![JetBrains](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/jetbrains.svg)](https://www.jetbrains.com/)
[![Claude by Anthropic](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/claude.svg)](https://claude.com/)
[![OpenAI](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/openai.svg)](https://openai.com/codex/)

See [our sponsors](https://www.reactiveui.net/sponsors/). All names and logos belong to their owners.
