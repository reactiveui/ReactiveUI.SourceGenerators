// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace MauiApp1;

/// <summary>The view model for <see cref="MainPage"/> that counts button clicks.</summary>
/// <seealso cref="ReactiveObject" />
public partial class MainViewModel : ReactiveObject
{
    /// <summary>Stores the number of times the counter button has been clicked.</summary>
    [Reactive]
    private int _count;

    /// <summary>Stores the text displayed on the counter button.</summary>
    [Reactive]
    private string _counterText = "Click me";

    /// <summary>Increments the click count and updates the counter button text.</summary>
    [ReactiveCommand]
    private void IncrementCount()
    {
        Count++;
        CounterText = Count == 1 ? $"Clicked {Count} time" : $"Clicked {Count} times";
        SemanticScreenReader.Announce(CounterText);
    }
}
