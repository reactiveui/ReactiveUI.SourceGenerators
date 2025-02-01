## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0001 | ReactiveUI.SourceGenerators.UnsupportedCSharpLanguageVersionAnalyzer | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0002 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0003 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0004 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0005 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0006 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0007 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0008 | ReactiveUI.SourceGenerators.AsyncVoidReturningReactiveCommandMethodAnalyzer | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0009 | ReactiveUI.SourceGenerators.ReactiveGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0010 | ReactiveUI.SourceGenerators.ReactiveGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0011 | ReactiveUI.SourceGenerators.ReactiveGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0012 | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0013 | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0014 | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0015 | ReactiveUI.SourceGenerators.ReactiveGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0017 | ReactiveUI.SourceGenerators.ObservableAsPropertyFromObservableGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0018 | ReactiveUI.SourceGenerators.ObservableAsPropertyFromObservableGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0018 | ReactiveUI.SourceGenerators.ReactiveGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html


## Rules
Shipped in ReactiveUI.SourceGenerators

- RXUISG0001 - Unsupported C# Language Version
This rule checks if the project is using an unsupported C# language version. The supported versions are C# 12.0 and above. If the project is using an unsupported version, the rule will raise an error.

- RXUISG0002 - ReactiveCommandGenerator
This rule checks if the `ReactiveCommand` has a Invalid ReactiveCommand method signature.

- RXUISG0003 - ReactiveCommandGenerator
This rule checks if the `ReactiveCommand` has a Invalid ReactiveCommand.CanExecute member name.

- RXUISG0004 - ReactiveCommandGenerator
This rule checks if the `ReactiveCommand` has Multiple ReactiveCommand.CanExecute member name matches.

- RXUISG0005 - ReactiveCommandGenerator
This rule checks if the `ReactiveCommand` has No valid ReactiveCommand.CanExecute member match.

- RXUISG0006 - ReactiveCommandGenerator
This rule checks if the `ReactiveCommand` has Invalid field or property targeted attribute type.

- RXUISG0007 - ReactiveCommandGenerator
This rule checks if the `ReactiveCommand` has Invalid field or property targeted attribute expression.

- RXUISG0008 - AsyncVoidReturningReactiveCommandMethodAnalyzer
This rule checks if the `ReactiveCommand` has Async void returning method annotated with ReactiveCommand.

- RXUISG0009 - ReactiveGenerator
This rule checks if the `Reactive` has Name collision for generated property.

- RXUISG0010 - ReactiveGenerator
This rule checks if the `Reactive` has Invalid property targeted attribute type.

- RXUISG0011 - ReactiveGenerator
This rule checks if the `Reactive` has Invalid property targeted attribute expression.

- RXUISG0012 - ObservableAsPropertyGenerator
This rule checks if the `ObservableAsProperty` has Invalid property targeted attribute type.

- RXUISG0013 - ObservableAsPropertyGenerator
This rule checks if the `ObservableAsProperty` has Invalid property targeted attribute expression.

- RXUISG0014 - ObservableAsPropertyGenerator
This rule checks if the `ObservableAsProperty` has Invalid generated property declaration.

- RXUISG0015 - ReactiveGenerator
This rule checks if the `Reactive` attribute is being used correctly. If the `Reactive` has Invalid generated property declaration.

- RXUISG0016 - PropertyToReactiveFieldCodeFixProvider
This rule checks if there are any Properties to change to Reactive Field, change to [Reactive] private type _fieldName;.

- RXUISG0017 - ObservableAsPropertyFromObservableGenerator
This rule checks if the `ObservableAsProperty` has Invalid generated property declaration.

- RXUISG0018 - ObservableAsPropertyFromObservableGenerator
This rule checks if the `ObservableAsProperty` has Invalid class inheritance, must inherit from `ReactiveObject`.

- RXUISG0018 - ReactiveGenerator
This rule checks if the `Reactive` has Invalid class inheritance, must inherit from `ReactiveObject`.
