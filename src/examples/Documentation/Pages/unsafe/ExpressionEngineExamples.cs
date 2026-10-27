// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reflection;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Expressions;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates the expression helpers the runtime fallback reads a property path with: <see cref="ExpressionMixins"/>
/// splits an expression into its links, and <see cref="Reflection"/> reads and writes the value each link holds.
/// </summary>
public static class ExpressionEngineExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The name of the <see cref="TodoItem.Title"/> property.</summary>
    private const string TitleName = nameof(TodoItem.Title);

    /// <summary>The amount an example writes.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The first tag of the item an example reads.</summary>
    private const string FirstTag = "car";

    /// <summary>Names the properties of a path, joined by dots.</summary>
    public static void NameThePathOfAnExpression()
    {
        Expression<Func<TransferViewModel, decimal>> path = x => x.Draft.Amount;

        Console.WriteLine(Reflection.ExpressionToPropertyNames(path.Body));

        // Output:
        // Draft.Amount
    }

    /// <summary>Splits an expression into one link for each member, from the root outwards.</summary>
    public static void SplitAnExpressionIntoLinks()
    {
        Expression<Func<TransferViewModel, decimal>> path = x => x.Draft.Amount;

        List<Expression> chain = new List<Expression>(path.Body.GetExpressionChain());

        Console.WriteLine(chain.Count);
        Console.WriteLine(((MemberExpression)chain[0]).Member.Name);
        Console.WriteLine(((MemberExpression)chain[1]).Member.Name);

        // Output:
        // 2
        // Draft
        // Amount
    }

    /// <summary>Finds the member an expression points at, looking through the conversion a compiler adds when it boxes a value.</summary>
    public static void FindTheMemberBehindAConversion()
    {
        Expression<Func<TodoItem, object>> path = x => x.Id;

        Console.WriteLine(path.Body.NodeType);
        Console.WriteLine(path.Body.GetMemberInfo()!.Name);

        // Output:
        // Convert
        // Id
    }

    /// <summary>Finds the expression a member is read from.</summary>
    public static void FindTheParentOfAMember()
    {
        Expression<Func<TransferViewModel, decimal>> path = x => x.Draft.Amount;

        MemberExpression parent = (MemberExpression)path.Body.GetParent()!;

        Console.WriteLine(parent.Member.Name);

        // Output:
        // Draft
    }

    /// <summary>Reads the arguments of an indexer link, and reports none for a plain member.</summary>
    public static void ReadTheArgumentsOfAnIndexer()
    {
        Expression<Func<TodoItem, string>> path = x => x.Tags[0];

        Expression indexer = Reflection.Rewrite(path.Body);
        object?[]? arguments = indexer.GetArgumentsArray();

        Console.WriteLine(indexer.NodeType);
        Console.WriteLine(arguments!.Length);
        Console.WriteLine((int)arguments[0]!);

        Expression<Func<TodoItem, string>> member = x => x.Title;

        Console.WriteLine(member.Body.GetArgumentsArray() is null);

        // Output:
        // Index
        // 1
        // 0
        // True
    }

    /// <summary>Rewrites an expression into the plain shape the chain helpers read: conversions go and an indexer call becomes an indexer link.</summary>
    public static void RewriteAnExpressionIntoChainShape()
    {
        Expression<Func<TodoItem, object>> boxed = x => x.Id;

        Console.WriteLine(Reflection.Rewrite(boxed.Body).NodeType);

        Expression<Func<TodoItem, string>> indexed = x => x.Tags[0];

        Console.WriteLine(indexed.Body.NodeType);
        Console.WriteLine(Reflection.Rewrite(indexed.Body).NodeType);

        // Output:
        // MemberAccess
        // Call
        // Index
    }

    /// <summary>Builds a function that reads a property, and reports none for a member that is not a property or a field.</summary>
    public static void ReadAPropertyWithAFetcher()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        PropertyInfo? title = typeof(TodoItem).GetProperty(TitleName);

        Func<object?, object?[]?, object?>? fetcher = Reflection.GetValueFetcherForProperty(title);

        Console.WriteLine((string)fetcher!(item, null)!);
        Console.WriteLine(Reflection.GetValueFetcherForProperty(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone))) is null);

        // Output:
        // Renew car registration
        // True
    }

    /// <summary>Builds a function that reads a property, and throws for a member that is not a property or a field.</summary>
    public static void ReadAPropertyWithAFetcherOrThrow()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Func<object?, object?[]?, object?> fetcher = Reflection.GetValueFetcherOrThrow(typeof(TodoItem).GetProperty(TitleName));

        Console.WriteLine((string)fetcher(item, null)!);
        Console.WriteLine(Throws<ArgumentException>(static () => Reflection.GetValueFetcherOrThrow(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone)))));

        // Output:
        // Renew car registration
        // True
    }

    /// <summary>Builds a function that writes a property, and reports none for a member that is not a property or a field.</summary>
    public static void WriteAPropertyWithASetter()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Action<object?, object?, object?[]?>? setter = Reflection.GetValueSetterForProperty(typeof(TodoItem).GetProperty(TitleName));

        setter!(item, RenamedTitle, null);

        Console.WriteLine(item.Title);
        Console.WriteLine(Reflection.GetValueSetterForProperty(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone))) is null);

        // Output:
        // Renew car registration online
        // True
    }

    /// <summary>Builds a function that writes a property, and throws for a member that is not a property or a field.</summary>
    public static void WriteAPropertyWithASetterOrThrow()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Action<object?, object?, object?[]?> setter = Reflection.GetValueSetterOrThrow(typeof(TodoItem).GetProperty(TitleName));

        setter(item, RenamedTitle, null);

        Console.WriteLine(item.Title);
        Console.WriteLine(Throws<ArgumentException>(static () => Reflection.GetValueSetterOrThrow(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone)))));

        // Output:
        // Renew car registration online
        // True
    }

    /// <summary>Reads the value at the end of a path.</summary>
    public static void ReadTheValueAtTheEndOfAPath()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        viewModel.Draft.Amount = AmountValue;
        List<Expression> chain = CreateChain<TransferViewModel, decimal>(static x => x.Draft.Amount);

        bool found = Reflection.TryGetValueForPropertyChain<decimal>(out var amount, viewModel, chain);

        Console.WriteLine(found);
        Console.WriteLine(amount);

        // Output:
        // True
        // 125.50
    }

    /// <summary>Reports that a path has no value when a link along it is null.</summary>
    public static void ReadThroughANullLink()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        List<Expression> chain = CreateChain<TodoListViewModel, string>(static x => x.SelectedItem!.Title);

        bool found = Reflection.TryGetValueForPropertyChain<string>(out var title, viewModel, chain);

        Console.WriteLine(found);
        Console.WriteLine(title is null);

        // Output:
        // False
        // True
    }

    /// <summary>Reads the value of every link along a path, each with the object it was read from.</summary>
    public static void ReadEveryValueAlongAPath()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        viewModel.Draft.Amount = AmountValue;
        List<Expression> chain = CreateChain<TransferViewModel, decimal>(static x => x.Draft.Amount);

        bool found = Reflection.TryGetAllValuesForPropertyChain(out var changes, viewModel, chain);

        Console.WriteLine(found);
        Console.WriteLine(changes.Length);
        Console.WriteLine(ReferenceEquals(viewModel, changes[0].Sender));
        Console.WriteLine(ReferenceEquals(viewModel.Draft, changes[0].Value));
        Console.WriteLine(ReferenceEquals(viewModel.Draft, changes[1].Sender));
        Console.WriteLine((decimal)changes[1].Value!);

        // Output:
        // True
        // 2
        // True
        // True
        // True
        // 125.50
    }

    /// <summary>Writes the value at the end of a path.</summary>
    public static void WriteTheValueAtTheEndOfAPath()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        List<Expression> chain = CreateChain<TransferViewModel, decimal>(static x => x.Draft.Amount);

        bool written = Reflection.TrySetValueToPropertyChain(viewModel, chain, AmountValue);

        Console.WriteLine(written);
        Console.WriteLine(viewModel.Draft.Amount);

        // Output:
        // True
        // 125.50
    }

    /// <summary>Writes the value at the end of a path and chooses whether a missing member throws.</summary>
    public static void WriteTheValueAtTheEndOfAPathWithoutThrowing()
    {
        TodoItem item = new TodoItem { Tags = [FirstTag] };
        List<Expression> chain = CreateChain<TodoItem, string>(static x => x.Title);

        bool written = Reflection.TrySetValueToPropertyChain(item, chain, RenamedTitle, false);

        Console.WriteLine(written);
        Console.WriteLine(item.Title);

        // Output:
        // True
        // Renew car registration online
    }

    /// <summary>Builds the links of a path from the expression the way the runtime fallback does.</summary>
    /// <typeparam name="TRoot">The type the path starts from.</typeparam>
    /// <typeparam name="TValue">The type of the value the path ends at.</typeparam>
    /// <param name="path">The path to split.</param>
    /// <returns>The links of the path.</returns>
    private static List<Expression> CreateChain<TRoot, TValue>(Expression<Func<TRoot, TValue>> path) =>
        [.. Reflection.Rewrite(path.Body).GetExpressionChain()];

    /// <summary>Runs an action and reports whether it throws the given exception.</summary>
    /// <typeparam name="TException">The exception to look for.</typeparam>
    /// <param name="action">The action to run.</param>
    /// <returns><see langword="true"/> when the action throws <typeparamref name="TException"/>.</returns>
    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return true;
        }

        return false;
    }
}
