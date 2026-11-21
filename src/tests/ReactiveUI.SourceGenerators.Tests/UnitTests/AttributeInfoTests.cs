// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests the attribute model that forwarded attributes are written from.</summary>
public sealed class AttributeInfoTests
{
    /// <summary>The metadata name of the attribute the test source applies.</summary>
    private const string AttributeMetadataName = "Test.ValuesAttribute";

    /// <summary>Constructor and named arguments are written in order, separated by commas.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task TryCreate_WithConstructorAndNamedArguments_WritesBothInOrder()
    {
        var info = TryCreate("""[Values(1, "two", First = 3, Second = typeof(int))]""");

        await Assert.That(info?.ToString())
            .IsEqualTo("""[global::Test.ValuesAttribute(1,"two",First=3,Second=typeof(int))]""");
    }

    /// <summary>An attribute with only named arguments starts with the first of them.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task TryCreate_WithOnlyNamedArguments_WritesNoLeadingSeparator()
    {
        var info = TryCreate("[Values(First = 3, Second = null)]");

        await Assert.That(info?.ToString()).IsEqualTo("[global::Test.ValuesAttribute(First=3,Second=null)]");
    }

    /// <summary>An argument that is not a constant produces no model.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task TryCreate_WithArgumentThatIsNotConstant_ReturnsNoModel()
    {
        var info = TryCreate("[Values(First = Holder.Value)]");

        await Assert.That(info).IsNull();
    }

    /// <summary>Creates the model for the attribute a test class carries.</summary>
    /// <param name="attributeText">The attribute list to apply.</param>
    /// <returns>The model, or <see langword="null"/> when none could be created.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the test source has no attribute or no attribute type.</exception>
    private static AttributeInfo? TryCreate(string attributeText)
    {
        var source = $$"""
            using System;

            namespace Test;

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class ValuesAttribute : Attribute
            {
                public ValuesAttribute()
                {
                }

                public ValuesAttribute(int number, string text)
                {
                }

                public int First { get; set; }

                public object? Second { get; set; }
            }

            public static class Holder
            {
                public static int Value => 1;
            }

            {{attributeText}}
            public sealed class Target
            {
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));
        var compilation = CSharpCompilation.Create(
            nameof(AttributeInfoTests),
            [syntaxTree],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        var attributeType = compilation.GetTypeByMetadataName(AttributeMetadataName)
            ?? throw new InvalidOperationException("The attribute type was not found.");
        var attribute = FindTargetAttribute(syntaxTree.GetRoot());
        _ = AttributeInfo.TryCreate(
            attributeType,
            compilation.GetSemanticModel(syntaxTree),
            attribute.ArgumentList!.Arguments,
            default,
            out var info);
        return info;
    }

    /// <summary>Finds the attribute applied to the target class.</summary>
    /// <param name="root">The syntax root to search.</param>
    /// <returns>The attribute.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the target class carries no attribute.</exception>
    private static AttributeSyntax FindTargetAttribute(SyntaxNode root)
    {
        foreach (var node in root.DescendantNodes())
        {
            if (node is ClassDeclarationSyntax { Identifier.ValueText: "Target" } target)
            {
                return target.AttributeLists[0].Attributes[0];
            }
        }

        throw new InvalidOperationException("The target class was not found.");
    }
}
