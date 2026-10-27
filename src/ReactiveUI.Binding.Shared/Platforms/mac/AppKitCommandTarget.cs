// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;
using AppKit;
using Foundation;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.CommandBinding;
#else
namespace ReactiveUI.Binding.CommandBinding;
#endif

/// <summary>The Cocoa target/action receiver that runs a command when an AppKit control or menu item fires its action.</summary>
/// <remarks>
/// Generated command bindings set a control's <c>Target</c> to an instance and its <c>Action</c> to the
/// <c>theAction:</c> selector, and keep <see cref="IsEnabled"/> in step with the command, which is what
/// <c>validateMenuItem:</c> answers for a menu item.
/// </remarks>
[DebuggerDisplay("AppKitCommandTarget: IsEnabled = {IsEnabled}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class AppKitCommandTarget : NSObject
{
    /// <summary>The selector the target exports for the control's action.</summary>
    private const string ActionSelectorName = "theAction:";

    /// <summary>The command the action runs.</summary>
    private readonly ICommand _command;

    /// <summary>Reads the parameter the command runs with.</summary>
    private readonly Func<object?> _parameter;

    /// <summary>Initializes a new instance of the <see cref="AppKitCommandTarget"/> class.</summary>
    /// <param name="command">The command the action runs.</param>
    /// <param name="parameter">Reads the parameter the command runs with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="command"/> or <paramref name="parameter"/> is <see langword="null"/>.</exception>
    public AppKitCommandTarget(ICommand command, Func<object?> parameter)
    {
        ArgumentExceptionHelper.ThrowIfNull(command);
        ArgumentExceptionHelper.ThrowIfNull(parameter);

        _command = command;
        _parameter = parameter;
        IsEnabled = command.CanExecute(null);
    }

    /// <summary>Gets or sets a value indicating whether the command can run, which a menu item's validation reports.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Runs the command with the current parameter when it can execute.</summary>
    /// <param name="sender">The control or menu item that fired the action.</param>
    [Export(ActionSelectorName)]
    public void Execute(NSObject sender)
    {
        var parameter = _parameter();
        if (_command.CanExecute(parameter))
        {
            _command.Execute(parameter);
        }
    }

    /// <summary>Reports whether a menu item bound to this target is enabled.</summary>
    /// <param name="item">The menu item being validated.</param>
    /// <returns><see langword="true"/> when the command can run.</returns>
    [Export("validateMenuItem:")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ValidateMenuItem(NSMenuItem item) => IsEnabled;
}
