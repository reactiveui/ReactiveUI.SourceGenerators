// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>Main entry point.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ObservableAsPropertyGenerator : IIncrementalGenerator
{
    /// <summary>Gets the generator type name used in generated-code metadata.</summary>
    internal static readonly string GeneratorName = typeof(ObservableAsPropertyGenerator).FullName!;

    /// <summary>Gets the generator assembly version used in generated-code metadata.</summary>
    internal static readonly string GeneratorVersion = typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString();

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.ObservableAsPropertyAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ObservableAsPropertyAttribute, Encoding.UTF8)));

        RunObservableAsPropertyFromObservable(context);
        RunObservableAsPropertyFromField(context);
    }
}
