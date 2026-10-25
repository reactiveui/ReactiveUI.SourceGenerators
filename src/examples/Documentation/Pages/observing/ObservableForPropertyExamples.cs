// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows the run-time observation methods of <see cref="ReactiveNotifyPropertyChangedMixins"/>. They observe one
/// property by expression or by name, and they observe a chain that a program built as an expression. Each method
/// delivers <c>IObservedChange</c> values through the observation provider that suits the object. Register the
/// providers first with <see cref="WhenAnyDynamicExamples.RegisterObservationServices"/>.
/// </summary>
public static class ObservableForPropertyExamples
{
    /// <summary>The amount the customer starts with.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>The amount the customer changes to.</summary>
    private const decimal RevisedAmount = 300M;

    /// <summary>The reference the customer starts with.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The reference the customer changes to.</summary>
    private const string RevisedReference = "Rent March, paid early";

    /// <summary>Observes a property by expression; it reports each change and skips the current value.</summary>
    public static void ObserveAmountByExpression()
    {
        TransferForm form = new() { Amount = RentAmount };

        using (form.ObservableForProperty(x => x.Amount).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 300
    }

    /// <summary>Observes a property by expression and asks for the value before each change, skipping the current value.</summary>
    public static void ObserveAmountBeforeItChanges()
    {
        TransferForm form = new() { Amount = RentAmount };

        using (form.ObservableForProperty(x => x.Amount, true, true, true).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250
    }

    /// <summary>Observes a property by name; it reports each change and skips the current value.</summary>
    public static void ObserveReferenceByName()
    {
        TransferForm form = new() { Reference = RentReference };

        using (form.ObservableForProperty<TransferForm, string>(nameof(TransferForm.Reference)).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Reference = RevisedReference;
        }

        // Output:
        // Rent March, paid early
    }

    /// <summary>Observes a property by name and asks for the current value first.</summary>
    public static void ObserveReferenceByNameWithTheCurrentValue()
    {
        TransferForm form = new() { Reference = RentReference };

        using (form.ObservableForProperty<TransferForm, string>(nameof(TransferForm.Reference), false).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Reference = RevisedReference;
        }

        // Output:
        // Rent March
        // Rent March, paid early
    }

    /// <summary>Observes a property by name and asks for the value before each change, skipping the current value.</summary>
    public static void ObserveReferenceByNameBeforeItChanges()
    {
        TransferForm form = new() { Reference = RentReference };

        using (form.ObservableForProperty<TransferForm, string>(nameof(TransferForm.Reference), true, true, true).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Reference = RevisedReference;
        }

        // Output:
        // Rent March
    }

    /// <summary>Observes a property chain that a program built as an expression; it reports each change and skips the current value.</summary>
    public static void ObserveExpressionChain()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;

        using (form.SubscribeToExpressionChain<TransferForm, decimal>(amount.Body).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 300
    }

    /// <summary>Observes an expression chain and asks for the current value first.</summary>
    public static void ObserveExpressionChainWithTheCurrentValue()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;

        using (form.SubscribeToExpressionChain<TransferForm, decimal>(amount.Body, false).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250
        // 300
    }

    /// <summary>Observes an expression chain before each change, skipping the current value.</summary>
    public static void ObserveExpressionChainBeforeItChanges()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;

        using (form.SubscribeToExpressionChain<TransferForm, decimal>(amount.Body, true, true, true).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 250
    }

    /// <summary>Observes an expression chain and turns off the warnings the engine writes when it cannot observe a link.</summary>
    public static void ObserveExpressionChainWithoutWarnings()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;

        using (form.SubscribeToExpressionChain<TransferForm, decimal>(amount.Body, false, true, true, true).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            form.Amount = RevisedAmount;
        }

        // Output:
        // 300
    }
}
