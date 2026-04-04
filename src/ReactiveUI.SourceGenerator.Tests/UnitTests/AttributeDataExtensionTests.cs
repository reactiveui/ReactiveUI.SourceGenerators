// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="AttributeDataExtensions"/> covering
/// <c>TryGetNamedArgument</c>, <c>GetNamedArgument</c>, <c>GetConstructorArguments</c>,
/// and <c>GetGenericType</c>.
/// </summary>
public sealed class AttributeDataExtensionTests
{
    /// <summary>
    /// TryGetNamedArgument returns true and the correct value when the argument exists.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentPresentThenTryGetReturnsTrue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public int Count { get; set; }
            }
            [MyAttr(Count = 42)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var found = attribute.TryGetNamedArgument("Count", out int? value);

        await Assert.That(found).IsTrue();
        await Assert.That(value).IsEqualTo(42);
    }

    /// <summary>
    /// TryGetNamedArgument returns false when the argument is not present.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentAbsentThenTryGetReturnsFalse()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public int Count { get; set; }
            }
            [MyAttr]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var found = attribute.TryGetNamedArgument("Count", out int? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    /// <summary>
    /// TryGetNamedArgument returns false and default when the argument name does not match.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenWrongArgumentNameThenTryGetReturnsFalse()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public int Count { get; set; }
            }
            [MyAttr(Count = 5)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var found = attribute.TryGetNamedArgument("Other", out int? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    /// <summary>
    /// TryGetNamedArgument returns false when called on a null AttributeData.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeDataIsNullThenTryGetReturnsFalse()
    {
        AttributeData? nullAttr = null;
        var found = nullAttr!.TryGetNamedArgument("X", out int? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    /// <summary>
    /// TryGetNamedArgument retrieves a string named argument.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenStringNamedArgumentPresentThenTryGetReturnsValue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public string? Name { get; set; }
            }
            [MyAttr(Name = "hello")]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var found = attribute.TryGetNamedArgument("Name", out string? value);

        await Assert.That(found).IsTrue();
        await Assert.That(value).IsEqualTo("hello");
    }

    /// <summary>
    /// TryGetNamedArgument retrieves a bool named argument.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenBoolNamedArgumentPresentThenTryGetReturnsValue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public bool IsEnabled { get; set; }
            }
            [MyAttr(IsEnabled = true)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var found = attribute.TryGetNamedArgument("IsEnabled", out bool? value);

        await Assert.That(found).IsTrue();
        await Assert.That(value).IsTrue();
    }

    /// <summary>
    /// GetNamedArgument returns the value when the argument is present.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentPresentThenGetNamedArgumentReturnsValue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public int Count { get; set; }
            }
            [MyAttr(Count = 7)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var value = attribute.GetNamedArgument<int>("Count");

        await Assert.That(value).IsEqualTo(7);
    }

    /// <summary>
    /// GetNamedArgument returns default when the argument is absent.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentAbsentThenGetNamedArgumentReturnsDefault()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public int Count { get; set; }
            }
            [MyAttr]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var value = attribute.GetNamedArgument<int>("Count");

        await Assert.That(value).IsEqualTo(0);
    }

    /// <summary>
    /// GetNamedArgument returns default when called on a null AttributeData.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeDataIsNullThenGetNamedArgumentReturnsDefault()
    {
        AttributeData? nullAttr = null;
        var value = nullAttr!.GetNamedArgument<int>("X");

        await Assert.That(value).IsEqualTo(0);
    }

    /// <summary>
    /// GetConstructorArguments yields all string constructor arguments.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenStringConstructorArgsPresentThenGetConstructorArgumentsYieldsAll()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute
            {
                public MyAttr(string a, string b) { }
            }
            [MyAttr("hello", "world")]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var args = attribute.GetConstructorArguments<string>().ToList();

        await Assert.That(args.Count).IsEqualTo(2);
        await Assert.That(args[0]).IsEqualTo("hello");
        await Assert.That(args[1]).IsEqualTo("world");
    }

    /// <summary>
    /// GetConstructorArguments yields nothing when there are no constructor arguments of the requested type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNoMatchingConstructorArgsThenGetConstructorArgumentsIsEmpty()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute { }
            [MyAttr]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var args = attribute.GetConstructorArguments<string>().ToList();

        await Assert.That(args.Count).IsEqualTo(0);
    }

    /// <summary>
    /// GetGenericType returns the type argument name for a generic attribute.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGenericAttributeThenGetGenericTypeReturnsTypeName()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr<T> : Attribute { }
            [MyAttr<int>]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var type = attribute.GetGenericType();

        await Assert.That(type).IsEqualTo("int");
    }

    /// <summary>
    /// GetGenericType returns null for a non-generic attribute.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNonGenericAttributeThenGetGenericTypeReturnsNull()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class MyAttr : Attribute { }
            [MyAttr]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "MyAttr");

        var type = attribute.GetGenericType();

        await Assert.That(type).IsNull();
    }

    /// <summary>
    /// GetGenericType returns the type keyword for a generic argument using a built-in type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGenericAttributeWithClassTypeThenGetGenericTypeReturnsClassName()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class WrapAttr<T> : Attribute { }
            [WrapAttr<string>]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "WrapAttr");

        var type = attribute.GetGenericType();

        await Assert.That(type).IsEqualTo("string");
    }

    /// <summary>
    /// GatherForwardedAttributesFromClass collects non-trigger attributes from the class declaration.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenClassHasAttributesThenForwardedAttributesCollected()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class TriggerAttr : Attribute { }
            [TriggerAttr]
            [Description("test")]
            public class C { }
            """;

        var compilation = CreateCompilation(source);
        var classDecl = compilation.SyntaxTrees
            .SelectMany(t => t.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "C");

        var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
        var typeSymbol = (INamedTypeSymbol)compilation.GetTypeByMetadataName("T.C")!;
        var triggerAttr = typeSymbol.GetAttributes()
            .First(a => a.AttributeClass?.Name == "TriggerAttr");

        triggerAttr.GatherForwardedAttributesFromClass(
            semanticModel,
            classDecl,
            default,
            out var forwarded);

        await Assert.That(forwarded.Length).IsGreaterThan(0);
        await Assert.That(forwarded.Any(a => a.TypeName.Contains("TriggerAttr"))).IsFalse();
    }

    private static AttributeData GetAttribute(string source, string typeName, string attributeSimpleName)
    {
        var compilation = CreateCompilation(source);
        var typeSymbol = compilation.GetTypeByMetadataName(typeName)
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in compilation.");

        return typeSymbol.GetAttributes()
            .First(a => a.AttributeClass?.Name == attributeSimpleName);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        return CSharpCompilation.Create(
            assemblyName: "AttrDataExtTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
