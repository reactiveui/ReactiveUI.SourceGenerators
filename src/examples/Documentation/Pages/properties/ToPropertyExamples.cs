// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// Shows every family of <c>ToProperty</c> overload: a selector or a named property, an initial value or a
/// factory for one, deferred subscription, a scheduler, and the <see langword="out"/> parameter that returns the
/// helper. Each example backs a property on a view model with one of the three ways generated code can raise a
/// type's own change notifications.
/// </summary>
public static class ToPropertyExamples
{
    /// <summary>The due date printed by the scheduler example.</summary>
    private static readonly DateOnly RegistrationDue = new(2026, 3, 20);

    /// <summary>
    /// Backs <see cref="PartialOwnEventViewModel.Title"/> with the selector overload, and
    /// <see cref="PartialOwnEventViewModel.Notes"/> with the overload that returns its helper through an
    /// <see langword="out"/> parameter. The view model raises its own field-like <c>PropertyChanged</c> event, so it
    /// has to stay <see langword="partial"/>: the generator adds the member that invokes it.
    /// </summary>
    public static void BackPropertiesOnATypeWithItsOwnEvent()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration", Notes = "Bring the insurance certificate" };
        PartialOwnEventViewModel viewModel = new PartialOwnEventViewModel(item);

        Console.WriteLine(viewModel.Title);
        Console.WriteLine(viewModel.Notes);

        item.Title = "Renew car registration online";

        Console.WriteLine(viewModel.Title);
    }

    /// <summary>
    /// Backs <see cref="RaiseMethodViewModel.RemainingLabel"/> with the selector overload and an initial-value
    /// factory. Backs <see cref="RaiseMethodViewModel.Priority"/> by naming the property with a constant instead of
    /// a selector, and gives it a plain initial value instead of a factory. The view model raises through the
    /// public <see cref="RaiseMethodBase.RaisePropertyChanged"/> method it inherits, so the generator calls it
    /// directly and the type does not have to be partial.
    /// </summary>
    public static void BackPropertiesOnATypeWithAPublicRaiseMethod()
    {
        TodoItem item = new TodoItem { IsDone = false, Priority = TodoPriority.High };
        RaiseMethodViewModel viewModel = new RaiseMethodViewModel(item);

        Console.WriteLine(viewModel.RemainingLabel);
        Console.WriteLine(viewModel.Priority);

        item.IsDone = true;

        Console.WriteLine(viewModel.RemainingLabel);
    }

    /// <summary>
    /// Backs <see cref="PartialProtectedBaseViewModel.IsDone"/> with deferred subscription: nothing follows the item
    /// until <see cref="ObservableAsPropertyHelper{T}.Value"/> is first read, which here happens through the
    /// <c>IsDone</c> property. Backs <see cref="PartialProtectedBaseViewModel.DueDateLabel"/> with a scheduler, so
    /// its change notifications wait for the sequencer to run. The view model raises through the
    /// <see langword="protected"/> <c>RaisePropertyChanged</c> its base class declares, so the generator adds a
    /// member to a partial declaration that calls it.
    /// </summary>
    public static void BackPropertiesWithDeferredSubscriptionAndAScheduler()
    {
        TodoItem item = new TodoItem { IsDone = false, DueDate = RegistrationDue };
        VirtualClock scheduler = new();
        PartialProtectedBaseViewModel viewModel = new PartialProtectedBaseViewModel(item, scheduler);

        item.IsDone = true;

        // The first read subscribes. WhenChanged delivers the item's current value on subscribe, so it is not missed.
        Console.WriteLine(viewModel.IsDone);

        item.DueDate = null;

        // The scheduler has not run yet, so DueDateLabel still holds its default value.
        Console.WriteLine(viewModel.DueDateLabel);

        scheduler.Start();

        Console.WriteLine(viewModel.DueDateLabel);
    }
}
