// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// Main entry point.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ObservableAsPropertyGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource($"{AttributeDefinitions.ObservableAsPropertyAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ObservableAsPropertyAttribute, Encoding.UTF8)));

        RunObservablePropertyAsFromObservable(context);
        RunGenerator(context);
    }
}
