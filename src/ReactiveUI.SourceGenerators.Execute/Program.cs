// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;
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
        AppLocator.CurrentMutable.RegisterViewsForViewModelsSourceGenerated();
        Application.Run(new TestViewWinForms());
    }
}
