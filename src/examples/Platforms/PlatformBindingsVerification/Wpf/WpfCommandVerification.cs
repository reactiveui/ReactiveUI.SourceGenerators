// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Wpf.Builder;

namespace PlatformBindingsVerification.Wpf;

/// <summary>
/// Binds a command to a real WPF <see cref="System.Windows.Controls.Button"/> held by a view that is not itself a
/// WPF element, then replaces the command from a background thread. The view has no dispatcher, so the generated
/// <c>BindCommand</c> routes each command through the button's own dispatcher; assigning it on the background
/// thread would throw.
/// </summary>
public static class WpfCommandVerification
{
    /// <summary>Binds the command, replaces it from a background thread, and confirms the button carries the replacement.</summary>
    /// <returns><see langword="true"/> when the button runs the replacement command and nothing threw.</returns>
    public static bool Verify()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().WithWpf().BuildApp();
        ViewThreadInvokers.Refresh();

        SaveCommand first = new();
        SaveCommand replacement = new();
        WpfCommandViewModel viewModel = new() { Save = first };
        WpfPlainCommandView view = new() { ViewModel = viewModel };

        using (view.BindCommand(viewModel, static x => x.Save, static v => v.SaveButton))
        {
            PumpDispatcher();
            if (!ReferenceEquals(view.SaveButton.Command, first))
            {
                return false;
            }

            Exception? thrown = null;
            var backgroundThread = new Thread(() =>
            {
                try
                {
                    viewModel.Save = replacement;
                }
                catch (Exception ex)
                {
                    thrown = ex;
                }
            });
            backgroundThread.Start();
            backgroundThread.Join();
            PumpDispatcher();

            return thrown is null && ReferenceEquals(view.SaveButton.Command, replacement);
        }
    }

    /// <summary>Drains the current dispatcher's queue so a posted write lands before the caller reads the control.</summary>
    private static void PumpDispatcher()
    {
        DispatcherFrame frame = new();
        _ = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    /// <summary>A command that can always run and does nothing; the check only compares which instance the button holds.</summary>
    private sealed class SaveCommand : ICommand
    {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => true;

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
        }
    }
}
