; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0016 | ReactiveUI.SourceGenerators.PropertyToReactiveFieldCodeFixProvider | Info | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html

## Release 4.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0002 | ReactiveUI.SourceGenerators.CodeFixers.ReactiveCommandAnalyzer | Error | A `[ReactiveCommand]` method takes more than one parameter besides a CancellationToken
RXUISG0008 | ReactiveUI.SourceGenerators.CodeFixers.ReactiveCommandAnalyzer | Error | A `[ReactiveCommand]` method is async void
RXUISG0020 | ReactiveUI.SourceGenerators.CodeFixers.ReactiveAttributeMisuseAnalyzer | Warning | `[Reactive]` is on a member or type that is not partial
RXUISG0021 | ReactiveUI.SourceGenerators.CodeFixers.ReactiveCommandAnalyzer | Warning | A `[ReactiveCommand]` OutputScheduler or BackgroundScheduler does not resolve to a scheduler
RXUISG0022 | ReactiveUI.SourceGenerators.CodeFixers.ControlHostAnalyzer | Info | A WinForms host follows its properties without ReactiveUI.Binding 8.4.0's ObservedProperty

## Release 10.0

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0016 | ReactiveUI.SourceGenerators.PropertyToReactiveFieldCodeFixProvider | Info | Advice only; the analyzer and its code fix are removed
RXUISG0022 | ReactiveUI.SourceGenerators.CodeFixers.ControlHostAnalyzer | Info | Advice only; the analyzer is removed
