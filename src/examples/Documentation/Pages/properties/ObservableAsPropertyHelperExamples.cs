// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// Shows <see cref="ObservableAsPropertyHelper{T}"/> constructed directly, without <c>ToProperty</c>. Construct it
/// yourself when you already have a callback that raises a change notification, or when a property never changes
/// and does not need an observable at all.
/// </summary>
public static class ObservableAsPropertyHelperExamples
{
    /// <summary>
    /// Constructs a helper with an <c>onChanged</c> callback that prints each value, and an <c>onChanging</c>
    /// callback that prints the value it is about to replace. Both callbacks run on the thread that produced the
    /// value, because no scheduler was given. Starting the helper delivers its default value as a first
    /// notification, then follows the source, whose own current value arrives as a second notification.
    /// </summary>
    public static void ConstructAHelperWithChangeCallbacks()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };
        IObservable<string> titles = item.WhenChanged(x => x.Title);

        using ObservableAsPropertyHelper<string> helper = new ObservableAsPropertyHelper<string>(
            titles,
            onChanged: static value => Console.WriteLine($"Now: {value}"),
            onChanging: static value => Console.WriteLine($"Was: {value}"));

        item.Title = "Renew car registration online";
    }

    /// <summary>
    /// Reads <see cref="ObservableAsPropertyHelper{T}.ThrownExceptions"/> after the source observable fails.
    /// Nothing observes the stream here, so the error is instead rethrown on the thread that produced it; the
    /// example subscribes first so it can print the error instead.
    /// </summary>
    public static void ReadThrownExceptions()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };
        IObservable<string> titles = item.WhenChanged(x => x.Title);
        InvalidOperationException failure = new InvalidOperationException("The title source failed.");
        IObservable<string> withFailure = Signal.Concat(titles, Signal.Fail<string>(failure));

        ObservableAsPropertyHelper<string> helper = new ObservableAsPropertyHelper<string>(withFailure, Console.WriteLine);

        using IDisposable subscription = helper.ThrownExceptions.Subscribe(static ex => Console.WriteLine(ex.Message));

        item.Title = "Renew car registration online";

        helper.Dispose();
    }

    /// <summary>
    /// Creates a helper with <see cref="ObservableAsPropertyHelper{T}.Default()"/>, which holds a fixed value and
    /// never subscribes to anything. Use it for a property that has no source yet, such as a design-time or
    /// disconnected view model.
    /// </summary>
    public static void CreateAHelperThatNeverChanges()
    {
        using ObservableAsPropertyHelper<string> helper = ObservableAsPropertyHelper<string>.Default("Not connected");

        Console.WriteLine(helper.Value);
        Console.WriteLine(helper.IsSubscribed);
    }
}
