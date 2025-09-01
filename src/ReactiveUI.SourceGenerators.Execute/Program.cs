// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using Splat;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// EntryPoint.
/// </summary>
public static class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    public static void Main()
    {
        AppLocator.CurrentMutable.RegisterViewsForViewModelsSourceGenerated();
        Application.Run(new TestViewWinForms());
    }
}
