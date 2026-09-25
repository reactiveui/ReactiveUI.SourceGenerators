using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Mocks.ReactiveCommand__N__;

public partial class CommandsViewModel : ReactiveObject
{
    private readonly IObservable<bool> _canSave = Observable.Return(true);

    [ReactiveCommand]
    private void Refresh()
    {
    }

    [ReactiveCommand]
    private int Add(int value) => value + 1;

    [ReactiveCommand(CanExecute = nameof(_canSave))]
    private Task SaveAsync() => Task.CompletedTask;

    [ReactiveCommand]
    private Task<string> LoadAsync(string key, CancellationToken token) => Task.FromResult(key);

    [ReactiveCommand]
    private IObservable<double> Stream(double seed) => Observable.Return(seed);

    [ReactiveCommand(OutputScheduler = "RxSchedulers.MainThreadScheduler")]
    private string Format(int value) => value.ToString();

    [ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]
    private void Reset()
    {
    }

    [ReactiveCommand]
    [property: System.ComponentModel.Description("delete")]
    private async Task DeleteAsync(int id, CancellationToken token) => await Task.Delay(id, token);
}
