; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0016 | ReactiveUI.SourceGenerators.PropertyToReactiveFieldCodeFixProvider | Info | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html


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

- RXUISG0020 - ReactiveUI.SourceGenerators.CodeFixers.ReactiveAttributeMisuseAnalyzer 
This rule warns when `[Reactive]` is used on non-partial property/type.
