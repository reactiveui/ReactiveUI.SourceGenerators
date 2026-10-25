[![NuGet Stats](https://img.shields.io/nuget/v/ReactiveUI.Binding.svg)](https://www.nuget.org/packages/ReactiveUI.Binding) [![Build](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/actions/workflows/ci-build.yml) [![Code Coverage](https://codecov.io/gh/reactiveui/ReactiveUI.Binding.SourceGenerators/branch/main/graph/badge.svg)](https://codecov.io/gh/reactiveui/ReactiveUI.Binding.SourceGenerators) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
<br>
<a href="https://www.nuget.org/packages/ReactiveUI.Binding">
<img src="https://img.shields.io/nuget/dt/ReactiveUI.Binding.svg">
</a>
<a href="https://reactiveui.net/slack">
<img src="https://img.shields.io/badge/chat-slack-blue.svg">
</a>
<a href="https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/labels/good%20first%20issue">
<img src="https://img.shields.io/badge/first--timers--only-friendly-blue.svg">
</a>
<a href="https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/stargazers">
<img src="https://img.shields.io/github/stars/reactiveui/ReactiveUI.Binding.SourceGenerators.svg?style=social">
</a>

<img src="images/logo.png" width="200">

# ReactiveUI.Binding.SourceGenerators

You have a property. When it changes, something else needs to know. That might be a label, a validation rule
or another property. This library lets you say that in one line. It writes the code for you when you build.

> [!NOTE]
> This page covers this binding engine. It does not teach reactive programming. The
> [WhenAny handbook](https://www.reactiveui.net/documentation/handbook/when-any/) teaches property
> observation. The [data binding guide](https://www.reactiveui.net/documentation/handbook/data-binding/)
> covers the binding methods in depth.

## Table of Contents

- [The problem it solves](#the-problem-it-solves)
- [Your first binding](#your-first-binding)
- [How a property reports a change](#how-a-property-reports-a-change)
- [The generator checks each property in a path](#the-generator-checks-each-property-in-a-path)
- [Trimming and NativeAOT](#trimming-and-nativeaot)
- [Compiler requirements](#compiler-requirements)
- [When nothing claims the call](#when-nothing-claims-the-call)
- [Members written by another source generator](#members-written-by-another-source-generator)
- [Installing](#installing)
- [Supported frameworks](#supported-frameworks)
- [Packages](#packages)
- [Supported APIs](#supported-apis)
- [Examples](#examples)
- [The view locator](#the-view-locator)
- [Which mechanism wins](#which-mechanism-wins)
- [Which thread a binding writes on](#which-thread-a-binding-writes-on)
- [Rx library compatibility](#rx-library-compatibility)
- [Diagnostics](#diagnostics)
- [Where this differs from ReactiveUI](#where-this-differs-from-reactiveui)
- [Core team](#core-team)
- [Contribute](#contribute)

### Core Team

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

## The problem it solves

C# already tells you when a property changes. A class raises `PropertyChanged`. You attach a handler, check
which property changed, and read the new value.

That works for one property. It gets hard when you follow a path through two properties:

```csharp
// Tell me when the city changes.
viewModel.PropertyChanged += (sender, args) =>
{
    if (args.PropertyName != nameof(viewModel.Address))
    {
        return;
    }

    // Address was replaced. Detach from the old Address, attach to the new one,
    // and start watching its City. Then undo all of it when you are finished.
};
```

With this library, you write the path as a lambda instead:

```csharp
IObservable<string> city = viewModel.WhenChanged(x => x.Address.City);
```

This attaches to `Address` and to `City`. When `Address` is replaced, it moves to the new `Address`. When you
dispose the subscription, it detaches from everything.

The lambda only names the path. A source generator is a compiler add-on that writes C# code while your project
builds. This library's generator reads the properties in the lambda, `Address` and then `City`. It writes code
that reads each one and attaches to the event that reports its change.

## Your first binding

**1. Install the package.**

```
dotnet add package ReactiveUI.Binding
```

**2. Raise `PropertyChanged` from your view model.** Any class that does this can be observed. You do not need
a base class.

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    private string _name = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
```

**3. Observe a property.** You get an `IObservable<T>`. It gives you the current value, then every later value.

```csharp
var vm = new PersonViewModel { Name = "Ada" };

IDisposable subscription = vm.WhenChanged(x => x.Name)
    .Subscribe(name => Console.WriteLine(name));   // prints Ada

vm.Name = "Grace";                                // prints Grace
```

**4. Or bind it straight to a control.** This writes `Name` into the label now, and again on every change.

```csharp
IDisposable binding = vm.BindOneWay(view, x => x.Name, v => v.NameLabel.Text);
```

**5. Dispose when you are done.** Both calls return an `IDisposable`. Disposing it detaches every handler the
binding attached.

> [!TIP]
> Keep your subscriptions in a `CompositeDisposable`. Dispose it when the view goes away. A subscription you
> never dispose keeps the view model alive.

`WhenChanging`, `BindTwoWay`, `BindCommand` and the other methods work the same way.

## How a property reports a change

`PropertyChanged` is one way for a property to report a change. There are others. A WPF `TextBox.Text` does
not raise `PropertyChanged`. An iOS view does not raise it at all.

Each way of reporting a change is a mechanism. The generator first finds the mechanisms a type offers. Each
mechanism uses a different event, and attaches to it a different way.

| Mechanism | Supported properties | Before the change |
|-----------|----------------------|-------------------|
| `INotifyPropertyChanged` | Properties that raise `PropertyChanged`, including MAUI bindable properties. | no |
| `INotifyPropertyChanging` | Properties that raise `PropertyChanging`. | yes |
| `IReactiveObject` | ReactiveUI properties that raise change notifications. | yes |
| WPF dependency property | Dependency properties such as `TextBox.Text`. | see below |
| WinUI and Uno dependency property | Properties backed by a framework dependency property. | no |
| WinForms component | Properties with a public `{PropertyName}Changed` event. | no |
| Apple KVO | Native `NSObject` properties and exported properties. | yes |
| UIKit and AppKit notifications | Supported control text, value, date and selection properties. | no |
| Android view | Supported widget properties such as `TextView.Text`. | no |

> [!NOTE]
> A type can offer more than one mechanism. A `ReactiveObject` in a WPF window implements
> `INotifyPropertyChanged`. It may also have dependency properties. The generator uses the most specific
> mechanism that can reach the property you named. So a plain C# property on a `DependencyObject` still uses
> `PropertyChanged`. [Which mechanism wins](#which-mechanism-wins) lists the order.

Observing before a change needs the type to raise an event before the value is replaced. Some types do not. For
those, `WhenChanging` reads the value once and then stays silent. RXUIBIND004 warns you about this when you
build. A WPF dependency property is the exception. It keeps a live subscription and delivers each new value.

## The generator checks each property in a path

`x => x.Address.City` names two properties. They can use different mechanisms. `Address` might be a plain
property on a view model that raises `PropertyChanged`. `City` might be a dependency property on a WPF control.

The generator checks each property on its own when you build. It writes the code to attach to each one.

```csharp
// Address: INotifyPropertyChanged on the view model.
// City:    a dependency property on the control it holds.
vm.WhenChanged(x => x.Address.City);
```

When `Address` is replaced, the subscription detaches from the old `Address` and attaches to the new one. You
would otherwise write this part by hand, and it is easy to get wrong.

> [!WARNING]
> If a property in the path raises no change event, replacing it cannot be detected. Properties below it can
> still be observed on the initial object. The analyzer reports RXUIBIND010 when you build.

## Trimming and NativeAOT

For example, observing a WinForms text box uses its own change event:

```csharp
var changes = textBox.WhenChanged(x => x.Text);
```

The central framework calls are direct property access and event subscription:

```csharp
EventHandler handler = (_, _) => observer.OnNext(textBox.Text);
textBox.TextChanged += handler;
// When the subscription is disposed:
textBox.TextChanged -= handler;
```

The complete observation also delivers the initial value and handles disposal. Ordinary JIT projects benefit
from avoiding runtime expression analysis and property lookup, keeping values typed, and catching unsupported
bindings during the build. Trimming and NativeAOT support are additional benefits.

Bindings with property paths known at build time support `PublishTrimmed` and `PublishAot`. Use the normal
binding APIs with inline property lambdas. The `Unsafe` APIs use reflection and carry trimming warnings.
Custom providers remain responsible for their own trimming and NativeAOT requirements.

## Compiler requirements

Use supported build tools and C# 7.3 or later.

### Which compiler you have

You do not choose the compiler directly. It comes with your build tools.

- Building in Visual Studio, or with Visual Studio's `msbuild`, uses the compiler that ships with that Visual
  Studio version.
- Building with `dotnet build` uses the compiler that ships with that .NET SDK version.

| Your build tools | Support |
|------------------|---------------------------------------|
| Visual Studio 2022 17.13 or later, Visual Studio 2026, or .NET SDK 9.0.200 or later | Supported |
| Visual Studio 2022 17.8 to 17.12, or .NET SDK 8.0.100 to 9.0.1xx | Supported, with call-site restrictions reported by RXUIBIND009 |
| Anything older | Not supported. The build fails with RXUIBIND100. |

Microsoft's [Roslyn version table](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support)
lists the compiler in each Visual Studio version.

### .NET Framework projects

A .NET Framework project gets the same features as a .NET project. It needs the SDK-style project format and new
enough build tools. An SDK-style project file names the SDK on its first line and sets a target framework:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
</Project>
```

Build that project with Visual Studio 2022 17.13 or later, or with `dotnet build` on .NET SDK 9.0.200 or later.
The C# 7.3 language version used by .NET Framework projects is supported.

An old-style project file has no `Sdk` attribute and lists its source files one by one. It still works. It uses the
compiler from the Visual Studio that builds it.

### Build properties

Two build properties change this.

- Set `ReactiveUIBindingUseInterceptors` to `false` to use the overloads on a compiler that can intercept.
- Set `ReactiveUIBindingEmitGeneratedCodeMarkers` to `false` to drop the `// <auto-generated/>` header from
  generated files. Analyzer and compiler warnings inside those files then show up.

## When nothing claims the call

Some call sites cannot be read when you build. Examples are a lambda stored in a variable, an expression built
at run time, and a call on a generic type parameter `T`. None of them name a path the generator can see.

For those call sites, call the `Unsafe` overload. It uses reflection. Reflection looks up each property by name
while the app runs. A trimmer cannot see those lookups, so an `Unsafe` overload may break after trimming or
ahead-of-time publishing. The other overloads keep working.

When nothing was generated for a call site, the plain method throws. The error message names the `Unsafe`
overload to use:

```csharp
Expression<Func<MyViewModel, string>> selector = x => x.Name;

vm.WhenChanged(selector);        // throws, and names WhenChangedUnsafe
vm.WhenChangedUnsafe(selector);  // walks the path by reflection
```

Fourteen methods have an `Unsafe` twin.

| Resolved when you build | Resolved by reflection |
|-------------------------|------------------------|
| `WhenChanged` | `WhenChangedUnsafe` |
| `WhenChanging` | `WhenChangingUnsafe` |
| `WhenAnyValue` | `WhenAnyValueUnsafe` |
| `WhenAny` | `WhenAnyUnsafe` |
| `WhenAnyObservable` | `WhenAnyObservableUnsafe` |
| `BindOneWay` | `BindOneWayUnsafe` |
| `BindTwoWay` | `BindTwoWayUnsafe` |
| `OneWayBind` | `OneWayBindUnsafe` |
| `Bind` | `BindUnsafe` |
| `BindTo` | `BindToUnsafe` |
| `BindCommand` | `BindCommandUnsafe` |
| `BindInteraction` | `BindInteractionUnsafe` |
| `InvokeCommand` | `InvokeCommandUnsafe` |
| `ToProperty` | `ToPropertyUnsafe` |

Every `Unsafe` overload carries `[RequiresUnreferencedCode]`. The plain overloads carry none. So a
`PublishTrimmed` or `PublishAot` build warns about each call that uses reflection, and about nothing else.
RXUIBIND021 reports every plain call that nothing was generated for, whatever the reason. RXUIBIND001, RXUIBIND006
and RXUIBIND009 name some of the reasons.

The scheduler overloads split the same way. Both kinds live on `ReactiveSchedulerExtensions`.

Two members always use reflection, so their plain names carry the attribute.

- `WhenAnyDynamic`. See [Observing a path built at run time](#observing-a-path-built-at-run-time).
- The object-typed `ResolveView` on the view locator. See [The view locator](#the-view-locator).

## Members written by another source generator

The generator reads your code as you wrote it. It never sees what another source generator adds, because no
generator sees another's output. A call that binds such a member has nothing to read, so nothing is generated for it.

The members ReactiveUI.SourceGenerators writes are the exception. The generator reads that package's attributes and
follows its rules for what each one writes, so these members bind like any other.

| Attribute | What the generator reads |
|-----------|--------------------------|
| `[Reactive]` on a field | A public property named from the field: `_name`, `name` and `m_name` become `Name`. Its setter follows `SetModifier`. |
| `[Reactive]` on a partial property | The property you declared. |
| `[ReactiveCollection]` on a field | A public read-write property, named the same way. |
| `[BindableDerivedList]` on a field | A read-only property, named the same way, with the accessibility `AccessModifier` gives it. |
| `[ReactiveCommand]` on a method | A `ReactiveCommand<TInput, TOutput>` property named from the method, with `Async` dropped and `Command` added. |
| `[IReactiveObject]` on a class | The class implements `IReactiveObject`, so its properties raise change notifications. |

```csharp
public partial class ProfileViewModel : ReactiveObject
{
    [Reactive]
    private string _displayName = "Ada";

    [ReactiveCommand]
    private void Save() => Saves++;

    public int Saves { get; private set; }
}

profile.WhenAnyValue(x => x.DisplayName);                  // generated
view.BindCommand(profile, x => x.SaveCommand, v => v.Save); // generated
```

A member any other source generator adds is not seen. RXUIBIND021 reports each call that binds one, because the call
throws when it runs. Declare the member yourself as a partial property, or call the `Unsafe` overload.

### Observing from code a generator writes

The reverse holds too: code another source generator writes cannot call `WhenAnyValue`, because this generator never
sees that call. `ObservedProperty` gives such code the same observation without a generated binding. Hand-written code
calls `WhenAnyValue`.

| Generated code wants | It calls |
|----------------------|----------|
| `WhenAnyValue(x => x.Name)` | `ObservedProperty.Create(source, static x => x.Name, static x => x.Name)` |
| `WhenAnyValue(x => x.Name, x => x.Age)` | `ObservedProperty.Create(source, ...Name..., ...Age...)`, with an optional selector |
| `WhenAnyValue(x => x.Home.City)` | `ObservedProperty.Create(source, ...Home...).Then(static h => h.City, static h => h.City)` |
| `WhenAnyObservable(x => x.Messages)` | `ObservedProperty.Create(source, ...Messages...).Switch()` |

Each property is passed twice: as a lambda that names it, and as a delegate that reads it. The compiler builds the
lambda from metadata, so nothing uses reflection, and trimming and ahead-of-time publishing are safe. The types it
composes are the ones a generated binding uses, so the values match. `ObservedProperty` observes one or two properties;
`Then` and `Switch` extend either to longer paths.

### Controls named in XAML

MAUI and Avalonia declare a field for each control a page names in XAML, and they do it with a source generator. The
generator reads those pages itself, so `Bind`, `OneWayBind` and `BindCommand` reach the named controls:

```csharp
// LoginPage.xaml: <Entry x:Name="UserName" x:FieldModifier="internal" />
this.Bind(ViewModel, vm => vm.UserName, v => v.UserName.Text); // generated
```

- **MAUI:** each `x:Name` is a field, `private` unless `x:FieldModifier` says otherwise. Generated code cannot reach a
  private field, so give a bound control `x:FieldModifier="internal"`.
- **Avalonia:** each `x:Name` or `Name` is a field, with the accessibility `AvaloniaNameGeneratorDefaultFieldModifier`
  gives it, `internal` by default.
- **WPF and WinUI:** their build writes the fields before the compiler runs, so the generator sees them without reading
  the XAML.

The pages reach the generator through the platform's own build, so there is nothing to set up. A control inside a
template gets no field, and a generic control named with `x:TypeArguments` is not read.

## Installing

```
dotnet add package ReactiveUI.Binding
```

The generator and the analyzer ship inside that package. You do not need to reference anything else.

If your app uses System.Reactive, install `ReactiveUI.Binding.Reactive` instead. It is the same library, built
against `System.Reactive.Concurrency.IScheduler`. Its namespaces start with `ReactiveUI.Binding.Reactive`.

Reference one package or the other, never both. They share no type names. So both together put two copies of
every method in scope.

## Supported frameworks

| Target | Versions | Microsoft support ends |
|--------|----------|------------------------|
| .NET | 10.0, 11.0 | November 2028 for .NET 10 |
| .NET | 8.0, 9.0 | 10 November 2026 |
| .NET Framework | 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1 | tied to the Windows version |
| .NET Framework | 4.6.2 | 12 January 2027 |

> [!WARNING]
> .NET 8 and .NET 9 leave Microsoft support on 10 November 2026. This library drops them on that date. .NET
> Framework 4.6.2 leaves support on 12 January 2027, and this library drops it then. Before those dates, move to
> .NET 10 or later, or to .NET Framework 4.7 or later. See the
> [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) and the
> [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework).

The WPF and WinForms packages target the .NET Framework versions and the Windows targets of .NET 8 to 11.

The MAUI packages start at .NET 10. They add Android, iOS, macOS, Mac Catalyst and tvOS targets. The Apple
targets build only on Windows and macOS.

The Avalonia packages target .NET 8 to 11. They are trim and NativeAOT safe.

The Uno packages target the Uno Platform heads of .NET 10 and 11: desktop, WebAssembly, Android, iOS and Windows.

NativeAOT works on .NET 8 and later. Only the generated code runs there. The `Unsafe` overloads compile
expressions at run time, and NativeAOT cannot do that.

## Packages

Fourteen packages ship. Each runtime package carries the generator and the analyzer. The platform packages get them
through the runtime package.

| Package | What it is |
|---------|------------|
| `ReactiveUI.Binding` | The runtime library. It has lightweight observables and no System.Reactive dependency. |
| `ReactiveUI.Binding.Reactive` | The same library, built against System.Reactive's `IScheduler`. |
| `ReactiveUI.Binding.Wpf` | WPF dependency-property observation. |
| `ReactiveUI.Binding.Wpf.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.WinForms` | WinForms component observation. |
| `ReactiveUI.Binding.WinForms.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.Maui` | MAUI bindable-property observation. |
| `ReactiveUI.Binding.Maui.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.Avalonia` | Avalonia property observation and command binding. |
| `ReactiveUI.Binding.Avalonia.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.Uno` | Uno Platform view thread routing. |
| `ReactiveUI.Binding.Uno.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.SourceGenerators` | MSBuild props and targets only. A compatibility package. |
| `ReactiveUI.Binding.Analyzer` | The analyzer project. Its files ship inside the runtime packages. |

## Supported APIs

| API | What it does | Properties per call |
|-----|--------------|--------------------:|
| `WhenChanged` | Observes a property after it changes. | 16 |
| `WhenChanging` | Observes a property before it changes. | 16 |
| `WhenAnyValue` | Observes a property after it changes, like `WhenChanged`. | 16 |
| `WhenAny` | Observes properties and hands each change to a selector. | 12 |
| `WhenAnyObservable` | Observes properties that hold observables, and switches between them. | 12 |
| `WhenAnyDynamic` | Observes a path you built as an `Expression`. | 12 |
| `BindOneWay` | Writes a source property to a target property. | 1 each side |
| `BindTwoWay` | Carries a property both ways. | 1 each side |
| `OneWayBind` | A one-way binding, written view first. | 1 each side |
| `Bind` | A two-way binding, written view first. | 1 each side |
| `BindTo` | Writes an observable's values to a target property. | 1 |
| `BindCommand` | Binds a command to a control's event. | 1 |
| `BindInteraction` | Registers a handler for an interaction a property holds. | 1 |
| `InvokeCommand` | Runs a command with each value an observable produces. | 1 |
| `ToProperty` | Backs a read-only property with the latest value an observable produces. | 1 |

Each of them reads a single property, a path such as `x => x.Address.City`, or several properties at once. When
an object in the middle of a path is replaced, the subscription moves to the new object.

`BindOneWay`, `BindTwoWay`, `OneWayBind` and `Bind` also accept a scheduler. The observation methods do not.

## Examples

### Observing a property

```csharp
// One property.
IObservable<string> name = vm.WhenChanged(x => x.Name);

// A path. Replacing Address moves the subscription to the new Address.
IObservable<string> city = vm.WhenChanged(x => x.Address.City);

// Several properties, combined by a selector.
IObservable<string> fullName = vm.WhenChanged(
    x => x.FirstName,
    x => x.LastName,
    (first, last) => $"{first} {last}");

// Before the change. The type has to raise PropertyChanging.
IObservable<string> before = vm.WhenChanging(x => x.Name);
```

### Binding

```csharp
// One way.
IDisposable binding = vm.BindOneWay(view, x => x.Name, x => x.NameLabel);

// One way, converting on the way through.
IDisposable converted = vm.BindOneWay(view, x => x.Age, x => x.AgeLabel, age => $"Age: {age}");

// Both ways.
IDisposable twoWay = vm.BindTwoWay(view, x => x.Name, x => x.NameTextBox);

// Both ways, with a converter per direction.
IDisposable pair = vm.BindTwoWay(
    view,
    x => x.Age,
    x => x.AgeTextBox,
    age => age.ToString(),
    text => int.TryParse(text, out var n) ? n : 0);

// On a scheduler.
IDisposable scheduled = vm.BindOneWay(
    view, x => x.Name, x => x.NameLabel, scheduler: RxApp.MainThreadScheduler);
```

`OneWayBind` and `Bind` are the same bindings, written with the view first:

```csharp
IDisposable oneWay = view.OneWayBind(vm, x => x.Name, x => x.NameLabel);
IDisposable twoWay = view.Bind(vm, x => x.Name, x => x.NameTextBox);
```

### Running a command

```csharp
// The command lives on a view model property. Each value becomes the command parameter,
// and a value CanExecute refuses is dropped.
IDisposable invocation = searchText.InvokeCommand(vm, x => x.Search);

// The caller holds the command, so there is no property to observe.
IDisposable direct = searchText.InvokeCommand(vm.Search);
```

A `ReactiveCommand` works like any other `ICommand`. The parameter arrives as `object`, not as the command's
declared input type.

### Backing a property with an observable

`ToProperty` turns an observable into a read-only property. It returns an `ObservableAsPropertyHelper<T>`, a
helper that keeps the latest value. Each new value raises your type's own change notification for the property.

```csharp
public partial class PersonViewModel : INotifyPropertyChanged
{
    private readonly ObservableAsPropertyHelper<string> _fullName;

    public PersonViewModel(IObservable<string> fullNames) =>
        _fullName = fullNames.ToProperty(this, x => x.FullName);

    public event PropertyChangedEventHandler? PropertyChanged;

    public string FullName => _fullName.Value;
}
```

A type can only raise its own events, so the generator needs a way in. It uses the first of these that your type
offers:

| Your type | How the generator raises the notification |
|-----------|-------------------------------------------|
| A ReactiveUI `ReactiveObject`, or another `IReactiveObject` | ReactiveUI's own `RaisePropertyChanged`, so suppressed and delayed notifications still work. |
| A public or internal `RaisePropertyChanged`, `OnPropertyChanged`, `NotifyPropertyChanged` or `NotifyOfPropertyChange` method | Calls that method. |
| A `partial` type | Adds one small member to your type. The member invokes your `PropertyChanged` event, or calls a protected raise method your base class has. |

When the raise method takes `PropertyChangedEventArgs`, the generator passes one cached instance per property. So
raising a notification allocates nothing.

Name the property with a lambda of the form `x => x.Property`, or with a constant such as `nameof(Property)`.
The overloads that take an initial value, an initial value factory, `deferSubscription`, a scheduler, or an `out`
parameter for the helper all work the same way.

Below C# 13, name the initial value of a `string` property when you name the property with a lambda:
`ToProperty(this, x => x.Title, initialValue: "(untitled)")`. Without the name the compiler cannot choose an overload,
and RXUIBIND014 tells you which argument to name. From C# 13, and with a `nameof` name at any version, the plain
argument works.

RXUIBIND012 warns you when the generator has no way to raise your type's notifications. RXUIBIND013 warns you
when it cannot read the property's name. Either way the call throws when it runs. Call `ToPropertyUnsafe` for those
properties instead. It reads the name and finds the raise member by reflection, so it also reaches a protected raise
method or a plain `PropertyChanged` event on a type that is not `partial`.

#### The helper

`ObservableAsPropertyHelper<T>` holds the latest value and raises the notification for each new one.

| Member | What it does |
|--------|--------------|
| `Value` | The latest value. A deferred helper subscribes the first time you read it. |
| `IsSubscribed` | Whether the helper follows its observable yet. |
| `ThrownExceptions` | The errors the observable produced. With no observer, an error is rethrown on the thread that produced it. |
| `Dispose()` | Stops following the observable. `Value` keeps the last value. |
| `Default(...)` | A helper that never changes, for a property with nothing to follow yet. |

A helper that subscribes straight away announces its initial value once, then each value that differs from the one
before. A deferred helper announces nothing until you read `Value`. Values arrive in order, one at a time. Pass a
scheduler to raise the notifications on another thread.

#### Declaring the property with `[ObservableAsProperty]`

From C# 13 you can declare the property as `partial` and mark it `[ObservableAsProperty]`. The generator writes the
property's body and a field named after it, such as `_fullNameHelper`. You assign that field with `ToProperty`.

```csharp
public partial class PersonViewModel : INotifyPropertyChanged
{
    public PersonViewModel(IObservable<string> fullNames) =>
        _fullNameHelper = fullNames.ToProperty(this, x => x.FullName);

    public event PropertyChangedEventHandler? PropertyChanged;

    [ObservableAsProperty]
    public partial string FullName { get; }
}
```

The attribute ships in the runtime package, so the generator adds no types to your assembly. Two projects that see
each other's internals both keep working.

The attribute takes these options.

| Option | Effect |
|--------|--------|
| `InitialValue` | What the property returns until you assign its helper. For a `string` property it is the value, such as `"none"`. For any other type it is a C# expression, such as `"1.5d"`, evaluated once. A non-nullable `string` property with no initial value returns `string.Empty`. |
| `ReadOnly` | Makes the helper field `readonly`, so only a constructor can assign it. |
| `UseProtected` | Makes the helper field `protected` rather than `private`, so a derived type can assign it. |

```csharp
[ObservableAsProperty(InitialValue = "Loading", ReadOnly = true)]
public partial string Status { get; }
```

ReactiveUI's older source generator also accepted `[ObservableAsProperty]` on a field, on a method that returns an
observable, and on an observable property. Nothing is generated for those forms. RXUIBIND018 reports each one, with a
code fix that rewrites it as a partial property:

- A field becomes a property named after it, without its `_` or `m_` prefix. Its initializer becomes `InitialValue`, and
  its `Inheritance` becomes a `virtual`, `override` or `new` modifier.
- A method or an observable property keeps its declaration and gains a property named `PropertyName`, or its own name
  followed by `Property`. The fix writes an `InitializeOAPH` method that assigns each helper, which your constructor
  already calls.

### Observing a path built at run time

`WhenAnyDynamic` takes the path as an `Expression`, not a lambda. So you can build the path from a property name
or a setting. It observes 1 to 12 paths at once. Each count has an overload that reports only changed values,
and one that reports every value.

```csharp
Expression chain = ((Expression<Func<MyViewModel, string>>)(x => x.Address.City)).Body;

IObservable<string?> city = vm.WhenAnyDynamic(chain, static c => (string?)c.Value);
```

The generator cannot read an expression built at run time. So `WhenAnyDynamic` walks the path by reflection.
Every overload carries `[RequiresUnreferencedCode]`, so a `PublishAot` build reports each call. When you know the
path before you build, `WhenChanged` and `WhenAny` observe the same thing without reflection.

## The view locator

A view locator finds the view for a view model. Implement `IViewFor<T>`, and the generator registers the view for
you. This works with both the `ReactiveUI.Binding` and `ReactiveUI.Binding.Reactive` packages.

```csharp
public class LoginView : IViewFor<LoginViewModel>
{
    public LoginViewModel ViewModel { get; set; }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (LoginViewModel)value;
    }
}

IViewFor? view = ViewLocator.GetCurrent().ResolveView(myViewModel);
```

Three attributes change what is registered.

| Attribute | Effect |
|-----------|--------|
| `[ViewContract("name")]` | Registers the view under a contract, so one view model can have several views. Pass the contract to `ResolveView`. |
| `[SingleInstanceView]` | Keeps one instance instead of creating a view each time. Do not use it for a view that appears more than once in the tree. |
| `[ExcludeFromViewRegistration]` | Leaves the view out of the generated registration. |

A view that ReactiveUI.SourceGenerators completes with `[IViewFor<T>]` or `[IViewFor("TypeName")]` is registered too.
That generator adds `IViewFor<T>` to the class, so the generated lookup checks for the interface when it resolves the
view. A class it leaves alone resolves to null. A `RegistrationType` of `LazySingleton` or `Constant` keeps one
instance, created the first time the view resolves.

```csharp
[ViewContract("compact")]
public class CompactDashboardView : IViewFor<DashboardViewModel> { }

public class FullDashboardView : IViewFor<DashboardViewModel> { }

var compact = ViewLocator.GetCurrent().ResolveView(vm, "compact");
var full = ViewLocator.GetCurrent().ResolveView(vm);
```

`ResolveView` looks in these places, in order.

1. The generated lookup. It is a type switch with no reflection.
2. A mapping you added at run time with `Map<TViewModel, TView>()`.
3. The service locator, `Splat.AppLocator.Current`. Only the generic `ResolveView<T>` and `ResolveViewUnsafe` ask it.

`ResolveView` with a view model held as an `object` stops after the mappings. Asking the service locator would close
`IViewFor<>` over the view model's runtime type, which an ahead-of-time build cannot do. So a view registered only as
`IViewFor<T>` in the service locator is not found there. When it finds nothing, it logs a warning that says so.
`ResolveViewUnsafe` adds the service locator step, and carries `[RequiresDynamicCode]`. RXUIBIND020 points out a
registration that only the service locator knows about.

In the generated lookup, a view registered under the requested contract comes before the default view. The default
view answers only a request with no contract. A request under a contract that no view claims moves on to your
mappings. The view instance comes from the first of these that has one.

1. The service locator.
2. The cached instance, when the view is marked `[SingleInstanceView]`. The first resolution creates it.
3. A new instance from the view's parameterless constructor.

The cache is set with `Interlocked.CompareExchange`. So two threads resolving at once share one instance.

Both `ResolveView` overloads are safe for ahead-of-time publishing. `ResolveViewUnsafe` is not.

## Which mechanism wins

Each mechanism has a score. The highest-scoring mechanism that can reach the property you named wins.

| Mechanism | Type it keys on | Score |
|-----------|-----------------|------:|
| UIKit control notifications | Supported text, selection, date and switch properties | 30 |
| UIKit value changes | `UIKit.UIControl.Value` with `ValueChanged` | 20 |
| AppKit control notifications | Supported `AppKit.NSControl` value properties | 20 |
| Apple KVO | `Foundation.NSObject` | 15 |
| IReactiveObject | `ReactiveUI.IReactiveObject` | 10 |
| WinForms component | `System.ComponentModel.Component` | 8 |
| WinUI bindable property | `Microsoft.UI.Xaml.DependencyObject` | 6 |
| Uno dependency property | `Windows.UI.Xaml.DependencyObject` | 6 |
| INotifyPropertyChanged | `System.ComponentModel.INotifyPropertyChanged` | 5 |
| Android view | `Android.Views.View` | 5 |
| WPF dependency property | `System.Windows.DependencyObject` | 4 |
| Plain property | A readable property without notifications | 1 |

A mechanism has to reach the property. A plain C# property on a dependency object is not a dependency property. A
component property with no `{PropertyName}Changed` event has nothing to attach to. Both fall through to the next
mechanism down.

A plain property emits its current value when you subscribe. It cannot report later changes.

`INotifyPropertyChanged` and `Android.Views.View` share a score. `INotifyPropertyChanged` wins that tie.

An `ICreatesObservableForProperty` you register yourself uses the same scores. It takes the property when it
scores higher. The generated code wins a tie.

Call `ReactiveUI.Binding.Fallback.ObservationAffinityChecker.Refresh()` after changing observation-provider
registrations so subsequent subscriptions use the updated registrations.

### Platform adapters

Platform support uses the framework references in your application. No extra binding platform package or
platform registration is needed for these APIs.

`BindCommand` supports Android `Click`, UIKit target/action touch handling, refresh-control `ValueChanged`,
bar-button `Clicked`, and AppKit `Target`/`Action`. It follows command and control replacements, tracks streamed
or property-based parameters, and detaches handlers when disposed. Registered command binders take over when
their score exceeds the selected adapter's score.

`ICreatesCommandBinding` carries no trimming attributes, so a generated binding that hands a command to a
registered binder stays safe to trim and to publish ahead of time. A binder you write must not rely on reflection
that trimming can break.

| Command mechanism | Score |
|-------------------|------:|
| UIKit refresh control or bar button | 10 |
| UIKit touch target or Android click | 9 |
| `Command` and `CommandParameter` properties | 5 |
| AppKit target/action or an event with `Enabled` | 4 |
| An event without `Enabled` | 3 |

Supplying `toEvent` selects that event instead of the control's default command mechanism.

`BindTo` and `OneWayBind` can populate WinForms panel and table-layout control collections from collections of
derived controls. They update the existing collection, including a read-only `Controls` property. They suspend
layout during the write and resume it even when a collection operation throws. The generated setter scores 10;
a registered `ISetMethodBindingConverter` must score higher to replace it.

Generated conversions cover numeric, boolean, GUID, date and time strings; numeric nullable values; URIs;
framework visibility enums; and Apple `NSDate` values. Visibility conversions honor the framework's inversion
and hidden-value hints. Registered typed converters must beat the generated score. An explicit converter
override takes precedence over conversion voting.

## Which thread a binding writes on

A UI framework lets only one thread touch a view. That thread is the view's owning thread. A view model can raise
a change on any thread. So a binding moves each write to the owning thread.

The binding asks the object it writes to for its **sequencer**: the ReactiveUI.Primitives scheduler that runs work
on the object's owning thread. The sequencer says whether the calling thread owns it, and queues the write when it
does not.

| Target | Sequencer |
|--------|-----------|
| WPF `DispatcherObject` | `DispatcherSequencer` for the object's dispatcher, at normal priority |
| WinForms `Control` | `ControlSequencer` for the control |
| MAUI `BindableObject` | `MauiDispatcherSequencer` for the object's dispatcher |
| Avalonia `AvaloniaObject` | `AvaloniaScheduler` for the object's dispatcher, at background priority |
| Uno `DependencyObject` | `DispatcherQueueSequencer` for the object's dispatcher queue |

WPF and Avalonia can run several UI threads. Each window belongs to one of them. Asking the object sends each write
to the right one.

The binding asks on every write.

- A write on the owning thread runs straight away when no earlier write is waiting. Set a property on the UI
  thread, and the control has the new value on the next line.
- A write from another thread waits for the owning thread. A write on the owning thread also waits while an
  earlier write is waiting.
- Only the latest value waits. A newer change replaces the waiting one, so a burst of changes becomes one write.

Some objects have no owning thread. The binding writes to them straight away.

- A frozen WPF `Freezable`.
- A WinForms control with no window handle yet. Once the handle exists, writes go to the thread that created it.
- A MAUI object with no dispatcher, such as a view in a unit test.
- Any object that is not a WPF, WinForms, MAUI, Avalonia or Uno object, such as a plain view model.

Every binding API does this: `BindOneWay`, `BindTwoWay`, `OneWayBind`, `Bind`, `BindTo`, and `BindCommand` when
it binds a new command to the control. Each `Unsafe` twin does the same through the registered invokers.

### Invokers

An `IViewThreadInvoker` does the check and the queueing for one platform. Each platform package has a module
that registers one: `WpfBindingModule`, `WinFormsBindingModule`, `MauiBindingModule`, `AvaloniaBindingModule` or
`UnoBindingModule`.
You can register your own. An invoker you register is asked first.

To support another platform, derive from `SequencerViewThreadInvoker<TTarget, TSequencer>` and return the
sequencer that owns an object from `SequencerFor`. The sequencer has to implement `IThreadAffineSequencer`, as every
ReactiveUI.Primitives UI sequencer does. Return null for an object that has no owning thread.

A generated binding knows its target's type when it compiles. For a WPF, WinForms, MAUI, Avalonia or Uno target, it
carries that platform's invoker. So it routes writes even when the platform module is not registered.

An `Unsafe` binding only finds its target's type while the app runs. It uses the registered invokers alone. Register
the platform module when you use `Unsafe` bindings.

### Choosing the thread yourself

> [!TIP]
> Pass a scheduler to pick the thread for one binding: `vm.BindOneWay(view, x => x.Name, x => x.NameLabel,
> scheduler: someScheduler)`. That binding skips the check.

To change it for every binding, set `BindingSchedulers.MainThread`, or call
`BindingSchedulers.UseSynchronizationContext(context)`. Only writes from another thread go through it. A write on
the owning thread still runs straight away. So does a write to an object no invoker claims.

## Rx library compatibility

Rx means the Reactive Extensions: libraries of operators for `IObservable<T>`. This library's observables and
operators come from **ReactiveUI.Primitives**. `ReactiveUI.Binding` depends on no other Rx library.

`ReactiveUI.Binding.Reactive` uses System.Reactive through **ReactiveUI.Primitives.Reactive**. Neither package
references System.Reactive directly.

| Package | Depends on | Scheduler type |
|---------|------------|----------------|
| `ReactiveUI.Binding` | `ReactiveUI.Primitives` | `ISequencer` |
| `ReactiveUI.Binding.Reactive` | `ReactiveUI.Primitives.Reactive` | `IScheduler` |

Every binding returns a standard .NET `IObservable<T>`. So you are not tied to either library.

| Library | How it works |
|---------|--------------|
| ReactiveUI.Primitives | The default. Reference `ReactiveUI.Binding`. |
| System.Reactive | Reference `ReactiveUI.Binding.Reactive`. |
| R3 | R3 has its own `Observable<T>` class. Convert with `.ToObservable()`. |
| Anything else | Any library that accepts `IObservable<T>` works as it is. |

## Diagnostics

The analyzer ships inside the runtime packages. It reports these diagnostics.

| ID | Severity | What it means |
|----|----------|---------------|
| RXUIBIND001 | Info | The expression is not an inline lambda, so nothing is generated. Call the `Unsafe` overload, or the call will throw. |
| RXUIBIND002 | Warning | The type has no observable property and raises no change event. |
| RXUIBIND003 | Warning | The expression reads a private or protected member. A generated extension method cannot read it. |
| RXUIBIND004 | Warning | The type raises no before-change event. `WhenChanging` reads the value once and then stays silent. |
| RXUIBIND005 | Info | The source implements `INotifyDataErrorInfo`. No code is generated for validation state. |
| RXUIBIND006 | Warning | The path contains an indexer, a static field, a read-only field at its end, or a method call. Properties and instance fields are generated; a field is read once, because it raises no notification. |
| RXUIBIND007 | Warning | The control named by `BindCommand` has no default event to bind. Pass `toEvent`. |
| RXUIBIND008 | Warning | The property named by `BindInteraction` does not implement `IInteraction<TInput, TOutput>`. |
| RXUIBIND009 | Warning | The generated overload cannot be reached from this file, so the call throws. Not reported when an interceptor takes the call. |
| RXUIBIND010 | Warning | The path passes through a type that raises no change event. The value is read once, and the path is followed no further. |
| RXUIBIND011 | Warning | The call resolved to ReactiveUI's own extension method. Nothing is generated, and the call uses reflection. |
| RXUIBIND012 | Warning | Generated code has no way to raise the `ToProperty` source's notifications. Make the type `partial`, add a raise method, or call `ToPropertyUnsafe`. |
| RXUIBIND013 | Warning | `ToProperty` names its property in a form the generator cannot read. Use `x => x.Property` or a constant, or call `ToPropertyUnsafe`. |
| RXUIBIND014 | Error | Below C# 13, a `string` initial value passed by position makes a `ToProperty` call ambiguous. Write it as `initialValue: ...`. |
| RXUIBIND015 | Warning | The call names a private or protected nested type, which generated code cannot reach, so nothing is generated and the call throws. Make the type `internal` or `public`, or call the `Unsafe` overload. |
| RXUIBIND016 | Warning | The call is made through a type parameter of the calling code, so generated code cannot name its types and the call throws. Call the `Unsafe` overload. |
| RXUIBIND017 | Warning | The binding writes to a WPF, WinForms, MAUI or Avalonia object, but the matching `ReactiveUI.Binding.Wpf`, `.WinForms`, `.Maui` or `.Avalonia` package is not referenced, so writes from another thread are not marshalled onto the object's thread. Reference the platform package. |
| RXUIBIND018 | Warning | `[ObservableAsProperty]` marks something other than a partial get-only instance property, so nothing is generated. For a field, a method or an observable property, a code fix rewrites it as a partial property (C# 13). |
| RXUIBIND019 | Warning | A method marked `[ObservableAsProperty]` takes parameters, so it cannot supply a property's values. |
| RXUIBIND020 | Info | A view is registered as `IViewFor<T>` in the service locator, and this project has no generated view and no `Map` for `T`. `ResolveView` with a view model held as an `object` does not find it. Add it with `Map`, or call `ResolveViewUnsafe`. |
| RXUIBIND021 | Warning | Nothing was generated for the call, so it throws when it runs. A lambda the generator cannot read, or a member another source generator adds, are common causes. Name each member directly, declare a generated member as a partial property, or call the `Unsafe` overload. |

The package's build targets report one error of their own.

| ID | What it means |
|----|---------------|
| RXUIBIND100 | The compiler is older than Roslyn 4.8, so no generator would load. Upgrade to Visual Studio 2022 17.8 or the .NET 8.0.100 SDK. |

## Where this differs from ReactiveUI

The sections below list every way a binding here behaves differently from ReactiveUI.

### Bindings are generated, not reflected

ReactiveUI does the generator's job while the app runs. It compiles the lambda into a delegate, then finds each
property by name. That costs time on every binding. A trimmer cannot see which members those lookups need, so it
may remove them.

The generated code does that work once, when you build, so it runs faster and allocates less. ReactiveUI's engine
compiles expressions at run time, so it cannot run under NativeAOT.

### ReactiveUI's method names are kept

`WhenAnyValue`, `OneWayBind` and `Bind` use ReactiveUI's names. They make code written for ReactiveUI easier to
move across.

### Observation delivery is serialized

Property observations deliver on the raising thread when the delivery gate is available. A competing producer
waits up to 20 ms, then hands its notification to the delivering thread. Subscriber calls do not hold a lock,
so a subscriber can wait for another thread to update the same property. Observation APIs leave scheduling
to the caller.

After-change and custom-provider observations read inside the gate and collapse contended changes to the
latest value. A change raised by the subscriber is delivered after that subscriber returns. Before-change
observations capture values before the write and preserve their order; a nested notification on the delivering
thread reaches the subscriber before the nested setter writes. Disposal stops pending delivery.

### `TriggerUpdate` and `signalViewUpdate` use `BindUnsafe`

The view-first `BindUnsafe` overloads accept an update stream and `TriggerUpdate`, with either registered
converters or explicit conversion delegates. These overloads resolve property paths by reflection and carry
`RequiresUnreferencedCode`; calling them produces a trimming diagnostic. Generated bindings use the two
property lambdas and do not offer these parameters.

`ViewToViewModel` is the default. A supplied stream replaces the view's change notifications, so editing the
view writes back only when the stream signals. With `ViewModelToView`, the stream drives model-to-view updates
after the initial model notification, and the view's own notifications always participate. A null stream observes
both properties in either mode; pass a typed null such as `(IObservable<int>?)null` for type inference.

Both sides are wired before a single initial signal writes from view model to view. Each signal reads the
current values when delivered, converts in its selected direction, and skips a write when the converted value
equals the destination. Disposing the binding disconnects both directions and the supplied stream.

### Every binding writes on the thread that owns the view

ReactiveUI moves a write to the UI thread only on WPF. It does so for a two-way `Bind`, and when it swaps a
control's `Command`. Its other bindings write on the thread that raised the change. So do all its WinForms and
MAUI bindings.

Here every binding moves its writes, on all three platforms. See
[Which thread a binding writes on](#which-thread-a-binding-writes-on). A background update that throws under
ReactiveUI works here. An update from another thread that ReactiveUI applied at once arrives one turn of the
message loop later.

Where ReactiveUI does move a write, the order is the same. A write on the owning thread runs straight away. A
write from another thread goes through the main-thread scheduler. Set `BindingSchedulers.MainThread` to
ReactiveUI's main-thread scheduler to use the same scheduler.

A burst of changes from another thread is handled differently. ReactiveUI's one-way bindings and `BindTo` write
every value, on the thread that raised it. Its two-way `Bind` queues one signal per change and reads the current
value when each signal runs. Here every binding writes only the latest value, once. A binding's change stream
skips the values in between.

### A binding made through a type parameter is not generated

A generated overload has to name the bound types. A call through a type parameter names none. The type is only
known once code fills in the parameter. So the generator leaves those calls to the plain overload. A generic
binding helper then compiles, but throws when it runs. An overload that named a type parameter would break your
build instead. From the helper, call the `Unsafe` twin to bind by reflection.

### A property with no change event is reported when you build

ReactiveUI logs a warning the first time it observes a property on a type that raises no change event.
RXUIBIND010 reports the same thing when you build. A generator can only report it then. The observation behaves
the same either way. The value is read when you subscribe; replacing that property cannot be detected.

### `ToProperty` works without a ReactiveUI base class

ReactiveUI's `ToProperty` only accepts an `IReactiveObject`. Here it accepts any class the generator can raise
notifications for, including a plain `INotifyPropertyChanged` type. A ReactiveUI object still raises through
ReactiveUI, so its `Changed` stream and suppressed notifications behave as before.

ReactiveUI's `ToProperty` takes the property as an expression tree. The compiler builds that tree every time the
line runs, and ReactiveUI reads the property name from it by reflection. Here the property is a plain lambda. The
generator reads its name when you build, and the lambda is never called. So creating the property builds no tree,
uses no reflection, and passes one cached delegate per notification instead of new delegates and a closure.

A property backed this way costs one small helper. Raising each new value allocates nothing, unless your own raise
method takes the property name as a string and builds new event args.

The helper type has ReactiveUI's name, `ObservableAsPropertyHelper<T>`, in the `ReactiveUI.Binding` namespace. A
file that imports both `ReactiveUI` and `ReactiveUI.Binding` has to name one of them in full, or give it an alias.

### A helper delivers its values in order, on the thread that produced them

ReactiveUI's helper raises its notifications through the current-thread scheduler. Here a helper with no scheduler
raises them straight away, on the thread that produced the value. A value produced while an earlier one is still
being delivered waits for it, whether it came from a notification handler or from another thread. So the
notifications never overlap and always arrive in order. Pass a scheduler to raise them somewhere else.

### An error nobody observes is rethrown where it happened

ReactiveUI sends a helper's error to its default exception handler, which throws on the main thread. Here the error
goes to the helper's `ThrownExceptions` stream. When nothing observes that stream, the error is rethrown on the
thread that produced it.

### A write that throws behaves the same

A write that throws is logged against the bound expression. It is rethrown as a `TargetInvocationException` only
when it has an inner exception. The setter threw on the thread that raised the change, so no caller can catch it.
Swallowing the exception would hide the failure.

## Contribute

ReactiveUI.Binding.SourceGenerators uses an OSI-approved open source license. You can use and share it freely,
including for commercial work. We value everyone who takes part. We would love to have you, even if you have
never contributed to open source before.

Ways to help:

* [Answer questions on GitHub Discussions](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/discussions)
* [Share what you know and teach other developers](http://ericsink.com/entries/dont_use_rxui.html)
* Improve the docs where something is missing or unclear.
* Contribute code.

## Sponsors

[JetBrains](https://www.jetbrains.com/) gives ReactiveUI's maintainers licences for its tools through its
[open source support programme](https://www.jetbrains.com/community/opensource/).
[Anthropic](https://www.anthropic.com/) supports them with [Claude](https://claude.com/) through
[Claude for Open Source](https://claude.com/contact-sales/claude-for-oss).
[OpenAI](https://openai.com/) supports them with [Codex](https://openai.com/codex/) through
[Codex for Open Source](https://developers.openai.com/community/codex-for-oss).

[![JetBrains](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/jetbrains.svg)](https://www.jetbrains.com/)
[![Claude by Anthropic](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/claude.svg)](https://claude.com/)
[![OpenAI](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/openai.svg)](https://openai.com/codex/)

See [our sponsors](https://www.reactiveui.net/sponsors/) for more information.
JetBrains, Claude, Anthropic, OpenAI and Codex names and logos are trademarks of their respective owners.
