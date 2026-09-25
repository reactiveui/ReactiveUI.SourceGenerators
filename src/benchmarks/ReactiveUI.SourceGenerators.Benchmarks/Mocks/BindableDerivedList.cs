using System.Collections.ObjectModel;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Mocks.BindableDerivedList__N__;

public partial class ListViewModel : ReactiveObject
{
    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<string>? _names;

    [BindableDerivedList(AccessModifier = PropertyAccessModifier.Internal)]
    private readonly ReadOnlyObservableCollection<int>? _numbers;

    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<ListViewModel>? _children;
}
