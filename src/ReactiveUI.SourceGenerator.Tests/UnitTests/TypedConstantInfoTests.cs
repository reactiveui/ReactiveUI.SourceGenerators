// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests serializable typed-constant models and their Roslyn factories.</summary>
public sealed class TypedConstantInfoTests
{
    /// <summary>The fully qualified Int32 type name.</summary>
    private const string Int32TypeName = "global::System.Int32";

    /// <summary>A representative character value.</summary>
    private const char CharacterValue = 'x';

    /// <summary>A representative double value.</summary>
    private const double DoubleValue = 1.25D;

    /// <summary>A representative single value.</summary>
    private const float SingleValue = 2.5F;

    /// <summary>A representative integer value.</summary>
    private const int IntegerValue = 3;

    /// <summary>A representative long value.</summary>
    private const long LongValue = 4L;

    /// <summary>A representative signed-byte value.</summary>
    private const sbyte SignedByteValue = -5;

    /// <summary>A representative short value.</summary>
    private const short ShortValue = -6;

    /// <summary>A representative unsigned-integer value.</summary>
    private const uint UnsignedIntegerValue = 7U;

    /// <summary>A representative unsigned-long value.</summary>
    private const ulong UnsignedLongValue = 8UL;

    /// <summary>A representative unsigned-short value.</summary>
    private const ushort UnsignedShortValue = 9;

    /// <summary>The first array value.</summary>
    private const int FirstArrayValue = 10;

    /// <summary>The second array value.</summary>
    private const int SecondArrayValue = 11;

    /// <summary>Source containing an attribute with every legal constructor constant kind.</summary>
    private const string AttributeConstantSource = """
        using System;

        namespace Test;

        public enum Choice
        {
            Negative = -1
        }

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class ValuesAttribute : Attribute
        {
            public ValuesAttribute(
                string? nullValue,
                string text,
                bool flag,
                byte byteValue,
                char characterValue,
                double doubleValue,
                float singleValue,
                int integerValue,
                long longValue,
                short shortValue,
                Type typeValue,
                Choice enumValue,
                int[] arrayValue)
            {
            }
        }

        [Values(
            null,
            "text",
            true,
            (byte)1,
            'x',
            1.25D,
            2.5F,
            3,
            4L,
            (short)-6,
            typeof(string),
            Choice.Negative,
            new[] { 10, 11 })]
        public sealed class Target
        {
        }
        """;

    /// <summary>All supported constant models produce the expected syntax shapes.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task GetSyntax_WithSupportedConstantModels_ProducesValidExpressions()
    {
        TypedConstantInfo[] values =
        [
            new TypedConstantInfo.Primitive.String("text"),
            new TypedConstantInfo.Primitive.Boolean(true),
            new TypedConstantInfo.Primitive.Boolean(false),
            new TypedConstantInfo.Primitive.Of<byte>(1),
            new TypedConstantInfo.Primitive.Of<char>(CharacterValue),
            new TypedConstantInfo.Primitive.Of<double>(DoubleValue),
            new TypedConstantInfo.Primitive.Of<float>(SingleValue),
            new TypedConstantInfo.Primitive.Of<int>(IntegerValue),
            new TypedConstantInfo.Primitive.Of<long>(LongValue),
            new TypedConstantInfo.Primitive.Of<sbyte>(SignedByteValue),
            new TypedConstantInfo.Primitive.Of<short>(ShortValue),
            new TypedConstantInfo.Primitive.Of<uint>(UnsignedIntegerValue),
            new TypedConstantInfo.Primitive.Of<ulong>(UnsignedLongValue),
            new TypedConstantInfo.Primitive.Of<ushort>(UnsignedShortValue),
            new TypedConstantInfo.Type("global::System.String"),
            new TypedConstantInfo.Enum("global::Test.Choice", 1),
            new TypedConstantInfo.Enum("global::Test.Choice", -1),
            new TypedConstantInfo.Null(),
            new TypedConstantInfo.Array(
                Int32TypeName,
                ImmutableArray.Create<TypedConstantInfo>(
                    new TypedConstantInfo.Primitive.Of<int>(FirstArrayValue),
                    new TypedConstantInfo.Primitive.Of<int>(SecondArrayValue))),
        ];
        List<string> expressions = [];
        foreach (var value in values)
        {
            expressions.Add(value.GetSyntax().ToString());
        }

        var allExpressionsHaveText = true;
        foreach (var expression in expressions)
        {
            allExpressionsHaveText &= !string.IsNullOrWhiteSpace(expression);
        }

        await Assert.That(expressions.Count).IsEqualTo(values.Length);
        await Assert.That(expressions[0]).IsEqualTo("\"text\"");
        await Assert.That(expressions[1]).IsEqualTo("true");
        await Assert.That(expressions[2]).IsEqualTo("false");
        await Assert.That(expressions[14]).IsEqualTo("typeof(global::System.String)");
        await Assert.That(expressions[16].Contains("(-1)", StringComparison.Ordinal)).IsTrue();
        await Assert.That(expressions[17]).IsEqualTo("null");
        await Assert.That(expressions[18].Contains(Int32TypeName, StringComparison.Ordinal)).IsTrue();
        await Assert.That(allExpressionsHaveText).IsTrue();
    }

    /// <summary>The attribute-data factory supports every legal attribute constant kind.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task Create_WithAttributeConstructorConstants_MapsEverySupportedKind()
    {
        var compilation = CreateCompilation(AttributeConstantSource);
        var target = compilation.GetTypeByMetadataName("Test.Target")
            ?? throw new InvalidOperationException("The attributed target type was not found.");
        var attribute = target.GetAttributes()[0];
        List<TypedConstantInfo> values = [];
        foreach (var argument in attribute.ConstructorArguments)
        {
            values.Add(TypedConstantInfo.Create(argument));
        }

        await Assert.That(values.Count).IsEqualTo(attribute.ConstructorArguments.Length);
        await Assert.That(values[0] is TypedConstantInfo.Null).IsTrue();
        await Assert.That(values[1] is TypedConstantInfo.Primitive.String).IsTrue();
        await Assert.That(values[2] is TypedConstantInfo.Primitive.Boolean).IsTrue();
        await Assert.That(values[10] is TypedConstantInfo.Type).IsTrue();
        await Assert.That(values[11] is TypedConstantInfo.Enum).IsTrue();
        await Assert.That(values[12] is TypedConstantInfo.Array).IsTrue();
        await Assert.That(values[12].GetSyntax().ToString().Contains("10", StringComparison.Ordinal)).IsTrue();
    }

    /// <summary>The operation factory handles constants, types, explicit and implicit arrays, and unsupported values.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task TryCreate_WithExpressionOperations_CoversSupportedAndUnsupportedShapes()
    {
        var integer = TryCreateExpression("42");
        var type = TryCreateExpression("typeof(string)");
        var implicitArray = TryCreateExpression("new[] { 1, 2 }");
        var explicitArray = TryCreateExpression("new int[] { 3, 4 }");
        var uninitializedArray = TryCreateExpression("new int[2]");
        var invocation = TryCreateExpression("GetValue()");
        var unsupportedArray = TryCreateExpression("new[] { GetValue() }");
        var allPrimitiveOperationsSucceeded = true;
        foreach (var expression in new[]
                 {
                     "null",
                     "\"text\"",
                     "true",
                     "(byte)1",
                     "'x'",
                     "1.25D",
                     "2.5F",
                     "3",
                     "4L",
                     "(sbyte)-5",
                     "(short)-6",
                     "7U",
                     "8UL",
                     "(ushort)9",
                 })
        {
            allPrimitiveOperationsSucceeded &= TryCreateExpression(expression).Success;
        }

        await Assert.That(integer.Success).IsTrue();
        await Assert.That(integer.Info?.GetSyntax().ToString()).IsEqualTo("42");
        await Assert.That(type.Success).IsTrue();
        await Assert.That(type.Info is TypedConstantInfo.Type).IsTrue();
        await Assert.That(implicitArray.Success).IsTrue();
        await Assert.That(explicitArray.Success).IsTrue();
        await Assert.That(uninitializedArray.Success).IsTrue();
        await Assert.That(uninitializedArray.Info is TypedConstantInfo.Array).IsTrue();
        await Assert.That((uninitializedArray.Info?.GetSyntax().ToString() ?? string.Empty).Contains("[]", StringComparison.Ordinal)).IsTrue();
        await Assert.That(allPrimitiveOperationsSucceeded).IsTrue();
        await Assert.That(invocation.Success).IsFalse();
        await Assert.That(invocation.Info).IsNull();
        await Assert.That(unsupportedArray.Success).IsFalse();
        await Assert.That(unsupportedArray.Info).IsNull();
    }

    /// <summary>Unsupported primitive values report the intended argument errors.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task UnsupportedPrimitiveValues_ThrowArgumentException()
    {
        await Assert.That(static () => new TypedConstantInfo.Primitive.Of<decimal>(1M).GetSyntax()).Throws<ArgumentException>();
        await Assert.That(static () => TryCreateExpression("1M")).Throws<ArgumentException>();
    }

    /// <summary>Creates an operation and asks the typed-constant factory to represent it.</summary>
    /// <param name="expressionText">The expression to compile.</param>
    /// <returns>The factory result.</returns>
    private static (bool Success, TypedConstantInfo? Info) TryCreateExpression(string expressionText)
    {
        var source = $$"""
            namespace Test;

            public static class Holder
            {
                private static int GetValue() => 1;

                private static readonly object? Value = {{expressionText}};
            }
            """;
        var compilation = CreateCompilation(source);
        var syntaxTree = compilation.SyntaxTrees[0];
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var expression = GetValueExpression(syntaxTree.GetRoot());
        var operation = semanticModel.GetOperation(expression)
            ?? throw new InvalidOperationException("The test expression operation was not found.");
        var success = TypedConstantInfo.TryCreate(operation, semanticModel, expression, default, out var info);
        return (success, info);
    }

    /// <summary>Finds the value initializer in a test syntax tree.</summary>
    /// <param name="root">The syntax root to search.</param>
    /// <returns>The value initializer expression.</returns>
    private static ExpressionSyntax GetValueExpression(SyntaxNode root)
    {
        foreach (var node in root.DescendantNodes())
        {
            if (node is VariableDeclaratorSyntax { Identifier.ValueText: "Value", Initializer.Value: { } value })
            {
                return value;
            }
        }

        throw new InvalidOperationException("The test expression was not found.");
    }

    /// <summary>Creates an in-memory compilation for typed-constant tests.</summary>
    /// <param name="source">The source text to compile.</param>
    /// <returns>The created compilation.</returns>
    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));
        return CSharpCompilation.Create(
            nameof(TypedConstantInfoTests),
            [syntaxTree],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
    }
}
