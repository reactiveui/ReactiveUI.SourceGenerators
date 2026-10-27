// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows <see cref="ExpressionChainParameters{TSender}"/>, which bundles what the run-time engine needs to observe one
/// property chain: the object, the chain as an expression, its links and the observation options. It is a read-only
/// record struct, so two bundles are equal when every member is equal.
/// </summary>
public static class ExpressionChainParametersExamples
{
    /// <summary>The amount the customer sends.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>Splits a bundle back into its members.</summary>
    public static void DeconstructChainParameters()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;
        Expression[] links = [amount.Body];
        ExpressionChainParameters<TransferForm> parameters = new(form, amount.Body, links, false, true, true, false);

        var (source, expression, chain, beforeChange, skipInitial, isDistinct, suppressWarnings) = parameters;

        Console.WriteLine(source?.Amount);
        Console.WriteLine(expression);
        Console.WriteLine(chain.Length);
        Console.WriteLine(beforeChange);
        Console.WriteLine(skipInitial);
        Console.WriteLine(isDistinct);
        Console.WriteLine(suppressWarnings);

        // Output:
        // 250
        // x.Amount
        // 1
        // False
        // True
        // True
        // False
    }

    /// <summary>Compares bundles; equal members make equal bundles.</summary>
    public static void CompareChainParameters()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;
        Expression[] links = [amount.Body];
        ExpressionChainParameters<TransferForm> after = new(form, amount.Body, links, false, true, true, false);
        ExpressionChainParameters<TransferForm> sameAfter = new(form, amount.Body, links, false, true, true, false);
        ExpressionChainParameters<TransferForm> before = new(form, amount.Body, links, true, true, true, false);

        Console.WriteLine(after == sameAfter);
        Console.WriteLine(after != before);
        Console.WriteLine(after.Equals(sameAfter));
        Console.WriteLine(after.Equals((object)before));
        Console.WriteLine(after.GetHashCode() == sameAfter.GetHashCode());

        // Output:
        // True
        // True
        // True
        // False
        // True
    }

    /// <summary>Prints a bundle; the text names each member.</summary>
    public static void PrintChainParameters()
    {
        TransferForm form = new() { Amount = RentAmount };
        Expression<Func<TransferForm, decimal>> amount = x => x.Amount;
        Expression[] links = [amount.Body];
        ExpressionChainParameters<TransferForm> parameters = new(form, amount.Body, links, false, true, true, false);

        string text = parameters.ToString();

        foreach (string member in text.TrimEnd(' ', '}').Split(", "))
        {
            Console.WriteLine(member);
        }

        // Output:
        // ExpressionChainParameters { Source = ReactiveUI.Binding.Documentation.Observing.TransferForm
        // Expression = x.Amount
        // Links = System.Linq.Expressions.Expression[]
        // BeforeChange = False
        // SkipInitial = True
        // IsDistinct = True
        // SuppressWarnings = False
    }
}
