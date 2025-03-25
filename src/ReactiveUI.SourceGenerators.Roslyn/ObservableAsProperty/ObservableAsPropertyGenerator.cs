// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    internal static readonly string GeneratorName = typeof(ObservableAsPropertyGenerator).FullName!;
    internal static readonly string GeneratorVersion = typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString();

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource($"{AttributeDefinitions.ObservableAsPropertyAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ObservableAsPropertyAttribute, Encoding.UTF8)));

        RunObservableAsPropertyFromObservable(context);
        RunObservableAsPropertyFromField(context);
    }
}
