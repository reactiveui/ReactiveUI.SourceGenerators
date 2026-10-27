// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Generator.Benchmarks.Mocks;

/// <summary>One call site for each observation and binding API the generator writes code for.</summary>
public static class PersonBindings
{
    /// <summary>Observes the name.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>The name, now and after each change.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> ObserveName(PersonViewModel viewModel) =>
        viewModel.WhenChanged(x => x.Name);

    /// <summary>Observes the city through the address.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>The city, now and after each change to either link.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> ObserveCity(PersonViewModel viewModel) =>
        viewModel.WhenChanged(x => x.Address.City);

    /// <summary>Observes the name and the age together.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>Both values, now and after each change to either.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, int>> ObserveNameAndAge(PersonViewModel viewModel) =>
        viewModel.WhenChanged(x => x.Name, x => x.Age);

    /// <summary>Observes the age before each change.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>The age, now and before each change.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<int> ObserveAgeChanging(PersonViewModel viewModel) =>
        viewModel.WhenChanging(x => x.Age);

    /// <summary>Observes the name under the ReactiveUI-compatible name.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>The name, now and after each change.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> ObserveNameValue(PersonViewModel viewModel) =>
        viewModel.WhenAnyValue(x => x.Name);

    /// <summary>Observes the name through <c>WhenAny</c>, which delivers the whole change.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>The name, now and after each change.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> ObserveNameChange(PersonViewModel viewModel) =>
        viewModel.WhenAny(x => x.Name, static change => change.Value);

    /// <summary>Observes the ticks the person reports.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <returns>Each tick of the current observable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<int> ObserveTicks(PersonViewModel viewModel) =>
        viewModel.WhenAnyObservable(x => x.Ticks);

    /// <summary>Runs the save command for each value a source produces.</summary>
    /// <param name="source">The values that trigger the command.</param>
    /// <param name="viewModel">The view model holding the command.</param>
    /// <returns>The subscription.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable RunSaveFor(IObservable<int> source, PersonViewModel viewModel) =>
        source.InvokeCommand(viewModel, x => x.Save);

    /// <summary>Answers the confirmation the view model asks for.</summary>
    /// <param name="viewModel">The view model holding the interaction.</param>
    /// <param name="view">The view that answers it.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindConfirm(PersonViewModel viewModel, PersonView view) =>
        view.BindInteraction(viewModel, x => x.Confirm, static context =>
        {
            context.SetOutput(true);
            return System.Threading.Tasks.Task.CompletedTask;
        });

    /// <summary>Writes the name to the view.</summary>
    /// <param name="viewModel">The view model to read.</param>
    /// <param name="view">The view to write.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindName(PersonViewModel viewModel, PersonView view) =>
        viewModel.BindOneWay(view, x => x.Name, x => x.NameText);

    /// <summary>Keeps the active flag and the active box in step.</summary>
    /// <param name="viewModel">The view model to bind.</param>
    /// <param name="view">The view to bind.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindIsActive(PersonViewModel viewModel, PersonView view) =>
        viewModel.BindTwoWay(view, x => x.IsActive, x => x.IsActiveValue);

    /// <summary>Writes the name to the view, starting from the view.</summary>
    /// <param name="viewModel">The view model to read.</param>
    /// <param name="view">The view to write.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable OneWayBindName(PersonViewModel viewModel, PersonView view) =>
        view.OneWayBind(viewModel, x => x.Name, x => x.NameText);

    /// <summary>Keeps the name and the name text in step, starting from the view.</summary>
    /// <param name="viewModel">The view model to bind.</param>
    /// <param name="view">The view to bind.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindNameBothWays(PersonViewModel viewModel, PersonView view) =>
        view.Bind(viewModel, x => x.Name, x => x.NameText);

    /// <summary>Runs the save command when the save button is clicked.</summary>
    /// <param name="viewModel">The view model holding the command.</param>
    /// <param name="view">The view holding the button.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindSave(PersonViewModel viewModel, PersonView view) =>
        view.BindCommand(viewModel, x => x.Save, x => x.SaveButton);

    /// <summary>Writes each name the view model reports into the view.</summary>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="view">The view to write.</param>
    /// <returns>The binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindNameTo(PersonViewModel viewModel, PersonView view) =>
        viewModel.WhenChanged(x => x.Name).BindTo(view, x => x.NameText);
}
