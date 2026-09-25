using System.ComponentModel;
using ReactiveUI.SourceGenerators;

namespace Mocks.ReactiveObject__N__;

[IReactiveObject]
public partial class PlainObject
{
    [Reactive]
    private string? _name;

    [Reactive]
    private int _count;

    [Reactive]
    public partial bool Enabled { get; set; }
}

[IReactiveObject]
public partial class Settings
{
    [Reactive]
    private double _volume;
}
