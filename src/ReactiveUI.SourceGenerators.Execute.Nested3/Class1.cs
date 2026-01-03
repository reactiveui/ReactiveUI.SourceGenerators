// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SGReactiveUI.SourceGenerators.Execute.Nested2;

namespace SGReactiveUI.SourceGenerators.Execute.Nested3;

/// <summary>
/// Class1.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Class1 : ReactiveObject
{
    [Reactive]
    private string? _property1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Class1"/> class.
    /// </summary>
    public Class1()
    {
        SetPropertyCommand.Execute(new Nested1.Class1 { Property1 = "Initial Value" }).Subscribe();
    }

    [ReactiveCommand]
    private SGReactiveUI.SourceGenerators.Execute.Nested2.Class1? SetProperty(Nested1.Class1? class1)
    {
        if (class1 == null)
        {
            return null;
        }

        return new() { Property1 = class1.Property1 };
    }
}
