// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using Splat;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the application entry point.</summary>
[ExcludeFromCodeCoverage]
public static class Program
{
    /// <summary>Defines the entry point of the application.</summary>
    [System.STAThread]
    public static void Main()
    {
        // ReactiveUI.Binding's view locator registers every IViewFor<T> view at compile time. Without it, register the
        // views with Splat yourself.
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestViewWinForms());
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel2<int>>>(static () => new TestViewWpf2());
        Application.Run(new TestViewWinForms());
    }
}
