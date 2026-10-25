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
