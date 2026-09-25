; Unshipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RXUISG0014 | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | `[ObservableAsProperty]` moved to ReactiveUI.Binding
RXUISG0017 | ReactiveUI.SourceGenerators.ObservableAsPropertyFromObservableGenerator | Error | `[ObservableAsProperty]` moved to ReactiveUI.Binding

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
RXUISG0012 | ReactiveUI.SourceGenerators | Error | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | Reports an unresolved attribute forwarded by any generator
RXUISG0013 | ReactiveUI.SourceGenerators | Error | ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator | Error | Reports an invalid attribute expression forwarded by any generator
