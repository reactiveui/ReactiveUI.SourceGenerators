// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows <see cref="IObservedChange{TSender, TValue}"/>, the value that <c>WhenAny</c> hands to its selector: the object
/// that changed, the property expression and the value, together with the helpers that read them.
/// </summary>
public static class ObservedChangeExamples
{
    /// <summary>The title of the review task.</summary>
    private const string ReviewPrTitle = "Review PR";

    /// <summary>The title the review task is renamed to.</summary>
    private const string RenamedTitle = "Review PR 204";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>Reads the sender and the value of each change to a to-do title.</summary>
    public static void ReadSenderAndValueOfEachChange()
    {
        TodoItem item = new TodoItem { Id = 1, Title = ReviewPrTitle };

        using IDisposable subscription = item.WhenAny(x => x.Title, static change => change)
            .Subscribe(static change => Console.WriteLine($"Item {change.Sender.Id} is now '{change.Value}'"));

        item.Title = RenamedTitle;

        // Output:
        // Item 1 is now 'Review PR'
        // Item 1 is now 'Review PR 204'
    }

    /// <summary>Reads the expression of a change; an observation the generator wrote has no expression tree to report.</summary>
    public static void ReadExpressionOfAGeneratedChange()
    {
        TodoItem item = new TodoItem { Id = 1, Title = ReviewPrTitle };

        using IDisposable subscription = item.WhenAny(x => x.Title, static change => change)
            .Subscribe(static change => Console.WriteLine(change.Expression is null));

        // Output:
        // True
    }

    /// <summary>Creates a change by hand with its expression, to describe a value that was read from an object.</summary>
    public static void CreateAChange()
    {
        TodoItem item = new TodoItem { Id = 1, Title = ReviewPrTitle };
        Expression<Func<TodoItem, string>> expression = x => x.Title;

        ObservedChange<TodoItem, string> change = new ObservedChange<TodoItem, string>(item, expression.Body, item.Title);

        Console.WriteLine(change.Sender.Title);
        Console.WriteLine(change.Expression);
        Console.WriteLine(change.Value);

        // Output:
        // Review PR
        // x.Title
        // Review PR
    }

    /// <summary>Creates a change without an expression, as an observation that needs no expression tree does.</summary>
    public static void CreateAChangeWithoutAnExpression()
    {
        TodoItem item = new TodoItem { Id = 1, Title = ReviewPrTitle };

        ObservedChange<TodoItem, string> change = new ObservedChange<TodoItem, string>(item, null, item.Title);

        Console.WriteLine(change.Sender.Title);
        Console.WriteLine(change.Value);
        Console.WriteLine(change.Expression is null);

        // Output:
        // Review PR
        // Review PR
        // True
    }

    /// <summary>Names the property a change describes, including every step of a path.</summary>
    public static void NameThePropertyOfAChange()
    {
        Issue issue = new Issue { Number = CheckoutNumber, Title = CheckoutTitle, Assignee = new User { Login = "priya-nair" } };
        Expression<Func<Issue, string>> title = x => x.Title;
        Expression<Func<Issue, string>> assigneeLogin = x => x.Assignee!.Login;

        ObservedChange<Issue, string> titleChange = new(issue, title.Body, issue.Title);
        ObservedChange<Issue, string> loginChange = new(issue, assigneeLogin.Body, issue.Assignee!.Login);

        Console.WriteLine(titleChange.GetPropertyName());
        Console.WriteLine(loginChange.GetPropertyName());

        // Output:
        // Title
        // Assignee.Login
    }

    /// <summary>Reads the value of a change; a change that carries the default value reads it from the sender again.</summary>
    public static void ReadTheValueOfAChange()
    {
        TodoItem item = new TodoItem { Id = 1, Title = ReviewPrTitle };
        Expression<Func<TodoItem, string>> expression = x => x.Title;

        ObservedChange<TodoItem, string> carried = new(item, expression.Body, RenamedTitle);
        ObservedChange<TodoItem, string> empty = new(item, expression.Body, default!);

        Console.WriteLine(carried.GetValue());
        Console.WriteLine(empty.GetValue());

        // Output:
        // Review PR 204
        // Review PR
    }

    /// <summary>Reads the value of a path that breaks halfway: <c>GetValue</c> reports it and <c>GetValueOrDefault</c> returns null.</summary>
    public static void ReadTheValueOfABrokenPath()
    {
        Issue issue = new Issue { Number = CheckoutNumber, Title = CheckoutTitle, Assignee = null };
        Expression<Func<Issue, string>> assigneeLogin = x => x.Assignee!.Login;

        ObservedChange<Issue, string> change = new(issue, assigneeLogin.Body, default!);

        Console.WriteLine(change.GetValueOrDefault() is null);

        try
        {
            Console.WriteLine(change.GetValue());
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Output:
        // True
        // One of the properties in the expression 'Assignee.Login' was null
    }

    /// <summary>Turns a stream of changes back into a stream of values.</summary>
    public static void ProjectChangesToValues()
    {
        TodoItem item = new TodoItem { Id = 1, Title = ReviewPrTitle };

        using IDisposable subscription = item.WhenAny(x => x.Title, static change => change)
            .Value()
            .Subscribe(Console.WriteLine);

        item.Title = RenamedTitle;

        // Output:
        // Review PR
        // Review PR 204
    }
}
