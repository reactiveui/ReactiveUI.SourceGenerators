// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A view model naming one property of each kind the binding dispatch stubs take.</summary>
/// <remarks>
/// The command and interaction are never invoked: a stub throws before it reads either, so the
/// properties exist to satisfy the overloads' type constraints rather than to behave.
/// </remarks>
public class DispatchStubViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the bound text.</summary>
    public string Caption
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Caption)));
        }
    } = "a";

    /// <summary>Gets or sets the value a command binding passes as its parameter.</summary>
    public string Parameter
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Parameter)));
        }
    } = "a";

    /// <summary>Gets or sets the command a command binding names.</summary>
    public ICommand? Run { get; set; }

    /// <summary>Gets or sets the interaction an interaction binding names.</summary>
    public IInteraction<string, bool> Confirm { get; set; } = null!;

    /// <summary>Gets or sets the stream a WhenAnyObservable call names.</summary>
    public IObservable<string>? Signal { get; set; }
}
