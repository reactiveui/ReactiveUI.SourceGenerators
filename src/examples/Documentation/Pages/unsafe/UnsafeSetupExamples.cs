// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Shows when a call needs an <c>Unsafe</c> overload and what the runtime fallback needs before it can run. The
/// generator writes code only for a property path it can read in the source. A path held in a variable is not
/// readable, so the plain method has nothing to run and the <c>Unsafe</c> method finds the properties at run time.
/// </summary>
public static class UnsafeSetupExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>
    /// Calls a plain method with the property path written in the call, which the generator reads. Given a path held in a
    /// variable, as in <c>item.WhenChanged(titleColumn)</c>, the plain method has no generated binding: the build reports
    /// RXUIBIND021, and the call throws when it runs. Such a path needs an <c>Unsafe</c> method.
    /// </summary>
    public static void PlainCallNeedsAPathWrittenInTheCall()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        using IDisposable subscription = item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine);

        // Output:
        // Renew car registration
    }

    /// <summary>Calls an <c>Unsafe</c> method before the application registers its services; the fallback finds no way to observe the property.</summary>
    public static void UnsafeCallBeforeBuildFails()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;

        try
        {
            using IDisposable subscription = item.WhenChangedUnsafe(titleColumn).Subscribe(Console.WriteLine);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message.Split(". ")[0]);
        }

        // Output:
        // Could not find a ICreatesObservableForProperty for ReactiveUI.Binding.Documentation.Todo.TodoItem property Title
    }

    /// <summary>Registers the default property observation, a converter and a command binder that the <c>Unsafe</c> methods look up while the app runs.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .WithConverter(new TodoItemTitleConverter())
            .WithCommandBinder(new ButtonClickCommandBinder())
            .BuildApp();
    }

    /// <summary>Calls an <c>Unsafe</c> method after the services are registered; the fallback reads the path and observes the property.</summary>
    public static void UnsafeCallAfterBuildObserves()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;

        using IDisposable subscription = item.WhenChangedUnsafe(titleColumn).Subscribe(Console.WriteLine);

        // Output:
        // Renew car registration
    }
}
