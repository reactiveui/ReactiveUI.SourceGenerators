// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows what a binding does when a write to the target faults.</summary>
public static class BindingErrorExamples
{
    /// <summary>A reference that fits on a statement.</summary>
    private const string ShortReference = "Rent March";

    /// <summary>A reference the customer types later.</summary>
    private const string AprilReference = "Rent April";

    /// <summary>A reference that is longer than the bank prints.</summary>
    private const string LongReference = "Rent for the flat on Harbour Street";

    /// <summary>A reference the conversion function cannot show.</summary>
    private const string ForbiddenReference = "forbidden";

    /// <summary>The expression a binding names when it reports a fault.</summary>
    private const string BoundExpression = "x => x.Reference";

    /// <summary>The message of a fault a conversion function raises.</summary>
    private const string ConversionMessage = "The reference is not allowed.";

    /// <summary>The message of the exception a conversion function wraps.</summary>
    private const string CauseMessage = "The reference list is unavailable.";

    /// <summary>Registers a logger that writes each entry of the application to the console.</summary>
    public static void RegisterConsoleLogger() =>
        AppLocator.CurrentMutable.RegisterConstant<ILogger>(new ConsoleLogger { ExceptionMessageFormat = "{0}" });

    /// <summary>
    /// Raises a fault with no inner exception, as the sequence feeding a write ending in error does. The binding logs the
    /// fault against its expression and rethrows nothing, because the caller can observe the sequence itself.
    /// </summary>
    public static void FaultWithoutInnerExceptionIsLogged()
    {
        TransferDraft draft = new() { Reference = ShortReference };
        Label label = new();

        using (draft.BindOneWay(label, x => x.Reference, v => v.Text, static reference => reference == ForbiddenReference ? throw new InvalidOperationException(ConversionMessage) : reference))
        {
            Console.WriteLine(label.Text);

            draft.Reference = ForbiddenReference;

            // The faulted sequence has ended, so the binding writes nothing more.
            draft.Reference = ShortReference;
            Console.WriteLine(label.Text);
            draft.Reference = AprilReference;
            Console.WriteLine(label.Text);
        }

        // Output:
        // Rent March
        // LogHost: v => v.Text Binding received an Exception! (OnError)
        // Rent March
        // Rent March
    }

    /// <summary>
    /// Raises a fault that wraps a cause, as a setter or converter that threw does. The binding logs it and rethrows it
    /// as a <see cref="TargetInvocationException"/> that names the bound expression and carries the cause.
    /// </summary>
    public static void FaultWithInnerExceptionIsRethrown()
    {
        TransferDraft draft = new() { Reference = ShortReference };
        Label label = new();
        InvalidOperationException cause = new(CauseMessage);

        using (draft.BindOneWay(label, x => x.Reference, v => v.Text, reference => reference == ForbiddenReference ? throw new InvalidOperationException(ConversionMessage, cause) : reference))
        {
            try
            {
                draft.Reference = ForbiddenReference;
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine(ReferenceEquals(cause, ex.InnerException));
                Console.WriteLine(ex.Message);
            }
        }

        // Output:
        // LogHost: v => v.Text Binding received an Exception! (OnError)
        // True
        // v => v.Text Binding received an Exception!
    }

    /// <summary>
    /// Writes to a target whose setter throws. The setter runs on the thread that raised the change, so the exception
    /// reaches whoever changed the view model property, and a binding created over a bad value throws while it is created.
    /// </summary>
    public static void SetterThatThrowsReachesTheCaller()
    {
        TransferDraft draft = new() { Reference = ShortReference };
        StatementReferenceView field = new();

        using (draft.BindOneWay(field, x => x.Reference, v => v.Reference))
        {
            Console.WriteLine(field.Reference);

            try
            {
                draft.Reference = LongReference;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine(field.Reference);

        try
        {
            using IDisposable second = draft.BindOneWay(field, x => x.Reference, v => v.Reference);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Output:
        // Rent March
        // A statement reference has at most 18 characters. (Parameter 'value')
        // Rent March
        // A statement reference has at most 18 characters. (Parameter 'value')
    }

    /// <summary>Applies the fault contract directly with <see cref="BindingErrors.Subscribe{T}"/>: values reach the write, and each kind of fault is handled as a binding handles it.</summary>
    public static void SubscribeAppliesTheFaultContract()
    {
        List<string> written = [];
        Signal<string> values = new();
        Signal<string> withoutCause = new();
        Signal<string> withCause = new();
        InvalidOperationException cause = new(CauseMessage);

        using (BindingErrors.Subscribe(values, written.Add, BoundExpression))
        using (BindingErrors.Subscribe(withoutCause, written.Add, BoundExpression))
        using (BindingErrors.Subscribe(withCause, written.Add, BoundExpression))
        {
            values.OnNext(ShortReference);
            values.OnCompleted();

            Console.WriteLine(string.Join(", ", written));

            // A fault with no cause is logged.
            withoutCause.OnError(new InvalidOperationException(ConversionMessage));

            // A fault with a cause is logged and rethrown, wrapped with the bound expression.
            try
            {
                withCause.OnError(new InvalidOperationException(ConversionMessage, cause));
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine(ReferenceEquals(cause, ex.InnerException));
                Console.WriteLine(ex.Message.Contains(BoundExpression, StringComparison.Ordinal));
            }
        }

        // Output:
        // Rent March
        // LogHost: x => x.Reference Binding received an Exception! (OnError)
        // LogHost: x => x.Reference Binding received an Exception! (OnError)
        // True
        // True
    }
}
