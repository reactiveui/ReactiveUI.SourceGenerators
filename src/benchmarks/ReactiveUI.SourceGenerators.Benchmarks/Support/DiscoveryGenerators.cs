// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.SourceGenerators.Benchmarks.Support;

/// <summary>Three ways to find the fields marked <c>[Reactive]</c>, measured against each other.</summary>
/// <remarks>
/// Each generator only discovers: its transform reads the field's name and its output counts the fields, so the
/// measurement is the cost of finding the targets and nothing else.
/// </remarks>
internal static class DiscoveryGenerators
{
    /// <summary>The attribute every strategy looks for.</summary>
    private const string AttributeMetadataName = "ReactiveUI.SourceGenerators.ReactiveAttribute";

    /// <summary>Gets the strategies by name.</summary>
    internal static IReadOnlyList<(string Name, IIncrementalGenerator Generator)> Strategies { get; } =
    [
        ("ForAttributeWithMetadataName", new ForAttributeWithMetadataNameDiscovery()),
        ("SyntaxNameThenSymbol", new SyntaxNameThenSymbolDiscovery()),
        ("DeclaredSymbolScan", new DeclaredSymbolScanDiscovery()),
    ];

    /// <summary>Determines from syntax whether a declarator belongs to a field that carries attributes.</summary>
    /// <param name="node">The node.</param>
    /// <returns>Whether the node is a variable of an attributed field.</returns>
    private static bool IsAttributedField(SyntaxNode node) =>
        node is VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax { AttributeLists.Count: > 0 } };

    /// <summary>Determines whether a symbol carries the attribute, comparing its class's metadata name.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>Whether the attribute is present.</returns>
    private static bool HasReactiveAttribute(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == AttributeMetadataName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Writes the number of fields found, so every strategy's pipeline runs to its end.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="names">The names of the fields found.</param>
    private static void RegisterCount(in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<string> names) =>
        context.RegisterSourceOutput(names.Collect(), static (context, all) => context.AddSource("Count.g.cs", $"// {all.Length}"));

    /// <summary>Finds the fields with <c>ForAttributeWithMetadataName</c>.</summary>
    private sealed class ForAttributeWithMetadataNameDiscovery : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            RegisterCount(context, context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeMetadataName,
                static (node, _) => IsAttributedField(node),
                static (context, _) => context.TargetSymbol.Name));
    }

    /// <summary>Finds the fields by the attribute's simple name in syntax, then confirms the attribute on the symbol.</summary>
    private sealed class SyntaxNameThenSymbolDiscovery : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            RegisterCount(context, context.SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) => NamesReactiveAttribute(node),
                    static (context, token) => context.SemanticModel.GetDeclaredSymbol(context.Node, token) is { } symbol && HasReactiveAttribute(symbol) ? symbol.Name : null)
                .Where(static name => name is not null)
                .Select(static (name, _) => name!));

        /// <summary>Determines from syntax alone whether a field names an attribute <c>Reactive</c> or <c>ReactiveAttribute</c>.</summary>
        /// <param name="node">The node.</param>
        /// <returns>Whether any attribute's simple name matches.</returns>
        private static bool NamesReactiveAttribute(SyntaxNode node)
        {
            if (node is not VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax field })
            {
                return false;
            }

            foreach (var list in field.AttributeLists)
            {
                foreach (var attribute in list.Attributes)
                {
                    var name = attribute.Name switch
                    {
                        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                        AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText,
                        SimpleNameSyntax simple => simple.Identifier.ValueText,
                        _ => string.Empty,
                    };

                    if (name is "Reactive" or "ReactiveAttribute")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>Finds the fields by binding every attributed field and reading its attributes, as the old IViewFor helper did for classes.</summary>
    private sealed class DeclaredSymbolScanDiscovery : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            RegisterCount(context, context.SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) => IsAttributedField(node),
                    static (context, token) => context.SemanticModel.GetDeclaredSymbol(context.Node, token) is { } symbol && HasReactiveAttribute(symbol) ? symbol.Name : null)
                .Where(static name => name is not null)
                .Select(static (name, _) => name!));
    }
}
