using System.Collections.ObjectModel;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Mocks.ReactiveCollection__N__;

public partial class CollectionViewModel : ReactiveObject
{
    [ReactiveCollection]
    private ObservableCollection<string> _names = [];

    [ReactiveCollection]
    private ObservableCollection<int> _numbers = [];

    [ReactiveCollection]
    private ObservableCollection<CollectionViewModel>? _children;
}
