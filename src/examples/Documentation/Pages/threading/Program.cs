// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Threading;
#if WINDOWS
using System.Windows.Threading;
using Application = System.Windows.Forms.Application;
#else
using Dispatcher = Avalonia.Threading.Dispatcher;
#endif

// The converters format numbers with the current culture, so the run does not depend on the machine's settings.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

#if WINDOWS
if (args is ["winforms"])
{
    // A Windows Forms application runs its message loop on a single-threaded apartment thread; awaits return to it.
    Thread? winForms = new Thread(static () =>
    {
        SynchronizationContext.SetSynchronizationContext(new System.Windows.Forms.WindowsFormsSynchronizationContext());
        _ = RunWinFormsExamplesAsync();
        Application.Run();
    });
    winForms.SetApartmentState(ApartmentState.STA);
    winForms.Start();
    winForms.Join();
}
else
{
    // A WPF application runs its dispatcher on a single-threaded apartment thread; awaits return to it.
    Thread? wpf = new Thread(static () =>
    {
        _ = Dispatcher.CurrentDispatcher.InvokeAsync(RunWpfExamplesAsync);
        Dispatcher.Run();
    });
    wpf.SetApartmentState(ApartmentState.STA);
    wpf.Start();
    wpf.Join();
}

static async Task RunWpfExamplesAsync()
{
    try
    {
        WpfExamples.ConvertBooleanToVisibility();

        WpfExamples.ConvertVisibilityToBoolean();

        WpfExamples.DescribeDependencyPropertyAffinity();

        WpfExamples.ObserveTextBoxThroughProvider();

        WpfExamples.RejectPropertyWithoutDependencyProperty();

        WpfExamples.ObserveFilterBoxWithWhenChanged();

        WpfExamples.KeepDataContextInStepWithViewModel();

        WpfExamples.RefuseWriteFromWorkerThread();

        WpfExamples.CallInvokerMembers();

        WpfExamples.PostToFrozenBrushRunsInline();

        WpfExamples.RejectMissingCallback();

        await WpfExamples.WriteProgressFromWorkerThreadWithoutModuleAsync();

        WpfExamples.ChainWithWpfThroughAppBuilder();

        WpfExamples.ChainWithWpfThroughBindingBuilder();

        WpfExamples.ConfigureWpfModuleOnResolver();

        WpfExamples.BuildWpfApplication();

        await WpfExamples.WriteProgressFromWorkerThreadThroughRegisteredInvokerAsync();

        await WpfExamples.BindTransferFormAsync();

        await WpfExamples.BindTodoFilterAndCheckBoxAsync();

        WpfExamples.BindDoneMarkWithBothConverters();
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
        Environment.ExitCode = 1;
    }
    finally
    {
        Dispatcher.CurrentDispatcher.InvokeShutdown();
    }
}

static async Task RunWinFormsExamplesAsync()
{
    try
    {
        WinFormsExamples.DescribeChangedEventAffinity();

        WinFormsExamples.ObserveTextBoxThroughObserver();

        WinFormsExamples.RejectPropertyWithoutChangedEvent();

        WinFormsExamples.ObserveFilterBoxWithWhenChanged();

        WinFormsExamples.CallInvokerMembers();

        WinFormsExamples.PostToControlWithoutHandleRunsInline();

        WinFormsExamples.RejectMissingCallback();

        await WinFormsExamples.WriteProgressFromWorkerThreadWithoutModuleAsync();

        WinFormsExamples.ChainWithWinFormsThroughAppBuilder();

        WinFormsExamples.ChainWithWinFormsThroughBindingBuilder();

        WinFormsExamples.ConfigureWinFormsModuleOnResolver();

        WinFormsExamples.BuildWinFormsApplication();

        await WinFormsExamples.WriteProgressFromWorkerThreadThroughRegisteredInvokerAsync();

        await WinFormsExamples.BindTransferFormAsync();

        await WinFormsExamples.BindTodoFilterAndCheckBoxAsync();

        await WinFormsExamples.BindItemsListAndPriorityPickerAsync();

        await WinFormsExamples.BindPriorityPickerToSelectedIndexAsync();

        WinFormsExamples.RefuseWriteFromWorkerThread();
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
        Environment.ExitCode = 1;
    }
    finally
    {
        Application.ExitThread();
    }
}
#else
// The Unsafe examples read their property paths through the core observation services, so register them first.
// This registers no view thread invoker, so the examples still start with none.
IReactiveUIBindingBuilder coreServices = RxBindingBuilder.CreateReactiveUIBindingBuilder();

_ = coreServices.WithCoreServices().BuildApp();

// The Avalonia examples run inside Avalonia's dispatcher on this thread, so an await returns to the UI thread.
using CancellationTokenSource stop = new();

Task avaloniaExamples = Dispatcher.UIThread.InvokeAsync(async () =>
{
    try
    {
        AvaloniaExamples.RaisePropertyChangedFromControl();

        AvaloniaExamples.ObserveFilterBoxWithWhenChanged();

        AvaloniaExamples.WriteFromWorkerThreadWithoutInvoker();

        BindingSchedulersExamples.ObserveOnViewThreadWithFallback();

        ViewThreadInvokerExamples.RegisterInvoker();

        ViewThreadInvokerExamples.CallInvokerMembers();

        ViewThreadInvokerExamples.RefuseWriteFromWorkerThread();

        ViewThreadInvokerExamples.WriteOnOwningThreadRunsInline();

        ViewThreadInvokerExamples.WriteFromWorkerThreadWaitsForOwningThread();

        ViewThreadInvokerExamples.LatestValueWins();

        BindingSchedulersExamples.SetMainThread();

        BindingSchedulersExamples.WriteOnOwningThreadSkipsMainThread();

        BindingSchedulersExamples.WriteToUnclaimedControlSkipsMainThread();

        BindingSchedulersExamples.UseSynchronizationContext();

        BindingSchedulersExamples.ObserveOnViewThread();

        BindingSchedulersExamples.ObserveOnSequencer();

        await AvaloniaExamples.BindTodoFilterAndCheckBoxAsync();

        await AvaloniaExamples.BindItemsListAndSelectionAsync();

        await AvaloniaExamples.BindTransferFormAsync();

        await AvaloniaExamples.WriteProgressFromWorkerThreadAsync();
    }
    finally
    {
        await stop.CancelAsync().ConfigureAwait(false);
    }
});

Dispatcher.UIThread.MainLoop(stop.Token);

await avaloniaExamples;

MauiExamples.ConvertBooleanToVisibility();

MauiExamples.ConvertVisibilityToBoolean();

MauiExamples.ObserveFilterEntryWithWhenChanged();

MauiExamples.CallInvokerMembers();

MauiExamples.CallInvokerThroughInterface(new ReactiveUI.Binding.Maui.DispatcherViewThreadInvoker());

MauiExamples.PostWithoutDispatcherRunsInline();

MauiExamples.RejectMissingCallback();

await MauiExamples.WriteProgressFromWorkerThreadWithoutModuleAsync();

MauiExamples.ChainWithMauiThroughAppBuilder();

MauiExamples.ConfigureMauiModuleOnResolver();

MauiExamples.BuildMauiApplication();

await MauiExamples.WriteProgressFromWorkerThreadThroughRegisteredInvokerAsync();

await MauiExamples.BindTransferFormAsync();

await MauiExamples.BindTodoFilterAndCheckBoxAsync();

MauiExamples.BindDoneMarkWithBothConverters();

await MauiExamples.BindListViewItemsAndSelectionAsync();

SequencerExamples.BindOneWayOnSequencer();

await SequencerExamples.BindOneWayWithConversionOnSequencerAsync();

SequencerExamples.BindTwoWayOnSequencer();

SequencerExamples.BindTwoWayBurstOnSequencer();

SequencerExamples.BindTwoWayWithConversionOnSequencer();

SequencerExamples.BindViewOnSequencer();

await SequencerExamples.OneWayBindViewOnSequencerAsync();

SequencerExamples.BindOneWayWithConverterOnSequencer();

SequencerExamples.BindTwoWayWithConvertersOnSequencer();

SequencerExamples.BindViewWithConvertersOnSequencer();

SequencerExamples.OneWayBindViewWithConverterOnSequencer();

SequencerExamples.BindOneWayUnsafeOnSequencer();

await SequencerExamples.BindOneWayUnsafeWithConversionOnSequencerAsync();

SequencerExamples.BindOneWayUnsafeWithConverterOnSequencer();

SequencerExamples.BindTwoWayUnsafeOnSequencer();

SequencerExamples.BindTwoWayUnsafeWithConversionOnSequencer();

SequencerExamples.BindTwoWayUnsafeWithConvertersOnSequencer();

SequencerExamples.BindViewUnsafeOnSequencer();

SequencerExamples.BindViewUnsafeWithConvertersOnSequencer();

await SequencerExamples.OneWayBindViewUnsafeOnSequencerAsync();

SequencerExamples.OneWayBindViewUnsafeWithConverterOnSequencer();
#endif
