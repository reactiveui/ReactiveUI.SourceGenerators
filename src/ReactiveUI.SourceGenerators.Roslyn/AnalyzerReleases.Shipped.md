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
RXUISG0018 | ReactiveUI.SourceGenerators.ObservableAsPropertyFromObservableGenerator/ReactiveGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html
RXUISG0019 | ReactiveUI.SourceGenerators.BindableDerivedListGenerator | Error | See https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html

## Release 4.0

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0001 | ReactiveUI.SourceGenerators.UnsupportedCSharpLanguageVersionAnalyzer | Error | No longer reported
RXUISG0002 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | Reported by the code-fix project's ReactiveCommandAnalyzer
RXUISG0003 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | No longer reported
RXUISG0004 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | No longer reported
RXUISG0005 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | No longer reported
RXUISG0006 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | No longer reported
RXUISG0007 | ReactiveUI.SourceGenerators.ReactiveCommandGenerator | Error | No longer reported
RXUISG0008 | ReactiveUI.SourceGenerators.AsyncVoidReturningReactiveCommandMethodAnalyzer | Error | Reported by the code-fix project's ReactiveCommandAnalyzer
RXUISG0014 | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | `[ObservableAsProperty]` moved to ReactiveUI.Binding
RXUISG0017 | ReactiveUI.SourceGenerators.ObservableAsPropertyFromObservableGenerator | Error | `[ObservableAsProperty]` moved to ReactiveUI.Binding

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
RXUISG0012 | ReactiveUI.SourceGenerators | Error | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | Reports an unresolved attribute forwarded by any generator
RXUISG0013 | ReactiveUI.SourceGenerators | Error | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | Reports an invalid attribute expression forwarded by any generator
