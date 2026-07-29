// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="AttributeDataExtensions"/> covering <c>TryGetNamedArgument</c>, <c>GetNamedArgument</c>, <c>GetConstructorArguments</c>, and <c>GetGenericType</c>.</summary>
public sealed class AttributeDataExtensionTests
{
    /// <summary>TryGetNamedArgument returns true and the correct value when the argument exists.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentPresentThenTryGetReturnsTrue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeOne : Attribute
            {
                public int NamedValueOne { get; set; }
            }
            [AttributeOne(NamedValueOne = 1)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeOne");

        var found = attribute.TryGetNamedArgument("NamedValueOne", out int? value);

        await Assert.That(found).IsTrue();
        await Assert.That(value).IsEqualTo(1);
    }

    /// <summary>TryGetNamedArgument returns false when the argument is not present.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentAbsentThenTryGetReturnsFalse()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeTwo : Attribute
            {
                public int NamedValueTwo { get; set; }
            }
            [AttributeTwo]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeTwo");

        var found = attribute.TryGetNamedArgument("NamedValueTwo", out int? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    /// <summary>TryGetNamedArgument returns false and default when the argument name does not match.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenWrongArgumentNameThenTryGetReturnsFalse()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeThree : Attribute
            {
                public int NamedValueThree { get; set; }
            }
            [AttributeThree(NamedValueThree = 5)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeThree");

        var found = attribute.TryGetNamedArgument("Other", out int? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    /// <summary>TryGetNamedArgument returns false when called on a null AttributeData.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeDataIsNullThenTryGetReturnsFalse()
    {
        AttributeData? nullAttr = null;
        var found = nullAttr!.TryGetNamedArgument("X", out int? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    /// <summary>TryGetNamedArgument retrieves a string named argument.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenStringNamedArgumentPresentThenTryGetReturnsValue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeFour : Attribute
            {
                public string? Name { get; set; }
            }
            [AttributeFour(Name = "hello")]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeFour");

        var found = attribute.TryGetNamedArgument("Name", out string? value);

        await Assert.That(found).IsTrue();
        await Assert.That(value).IsEqualTo("hello");
    }

    /// <summary>TryGetNamedArgument retrieves a bool named argument.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenBoolNamedArgumentPresentThenTryGetReturnsValue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeFive : Attribute
            {
                public bool IsEnabled { get; set; }
            }
            [AttributeFive(IsEnabled = true)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeFive");

        var found = attribute.TryGetNamedArgument("IsEnabled", out bool? value);

        await Assert.That(found).IsTrue();
        await Assert.That(value).IsTrue();
    }

    /// <summary>GetNamedArgument returns the value when the argument is present.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentPresentThenGetNamedArgumentReturnsValue()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeSix : Attribute
            {
                public int NamedValueSix { get; set; }
            }
            [AttributeSix(NamedValueSix = 1)]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeSix");

        var value = attribute.GetNamedArgument<int>("NamedValueSix");

        await Assert.That(value).IsEqualTo(1);
    }

    /// <summary>GetNamedArgument returns default when the argument is absent.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNamedArgumentAbsentThenGetNamedArgumentReturnsDefault()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeSeven : Attribute
            {
                public int NamedValueSeven { get; set; }
            }
            [AttributeSeven]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeSeven");

        var value = attribute.GetNamedArgument<int>("Count");

        await Assert.That(value).IsEqualTo(0);
    }

    /// <summary>GetNamedArgument returns default when called on a null AttributeData.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeDataIsNullThenGetNamedArgumentReturnsDefault()
    {
        AttributeData? nullAttr = null;
        var value = nullAttr!.GetNamedArgument<int>("X");

        await Assert.That(value).IsEqualTo(0);
    }

    /// <summary>GetConstructorArguments yields all string constructor arguments.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenStringConstructorArgsPresentThenGetConstructorArgumentsYieldsAll()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeEight : Attribute
            {
                public AttributeEight(string a, string b) { }
            }
            [AttributeEight("hello", "world")]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeEight");

        List<string?> args = [.. attribute.GetConstructorArguments<string>()];

        await Assert.That(args[0]).IsEqualTo("hello");
        await Assert.That(args[1]).IsEqualTo("world");
    }

    /// <summary>GetConstructorArguments yields nothing when there are no constructor arguments of the requested type.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNoMatchingConstructorArgsThenGetConstructorArgumentsIsEmpty()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeNine : Attribute { }
            [AttributeNine]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeNine");

        List<string?> args = [.. attribute.GetConstructorArguments<string>()];

        await Assert.That(args.Count).IsEqualTo(0);
    }

    /// <summary>GetGenericType returns the type argument name for a generic attribute.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGenericAttributeThenGetGenericTypeReturnsTypeName()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeTen<T> : Attribute { }
            [AttributeTen<int>]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeTen");

        var type = attribute.GetGenericType();

        await Assert.That(type).IsEqualTo("int");
    }

    /// <summary>GetGenericType returns null for a non-generic attribute.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenNonGenericAttributeThenGetGenericTypeReturnsNull()
    {
        const string source = """
            using System;
            namespace T;
            [AttributeUsage(AttributeTargets.Class)]
            public class AttributeEleven : Attribute { }
            [AttributeEleven]
            public class C { }
            """;
        var attribute = GetAttribute(source, "T.C", "AttributeEleven");

        var type = attribute.GetGenericType();

        await Assert.That(type).IsNull();
    }

    /// <summary>GetGenericType returns the type keyword for a generic argument using a built-in type.</summary>
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

    /// <summary>GatherForwardedAttributesFromClass collects non-trigger attributes from the class declaration.</summary>
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
        var classDeclaration = await GetClassDeclaration(compilation);
        var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        var typeSymbol = (INamedTypeSymbol)compilation.GetTypeByMetadataName("T.C")!;
        AttributeData? triggerAttr = null;
        foreach (var candidate in typeSymbol.GetAttributes())
        {
            if (candidate.AttributeClass?.Name == "TriggerAttr")
            {
                triggerAttr = candidate;
                break;
            }
        }

        (triggerAttr ?? throw new InvalidOperationException("The trigger attribute was not found.")).GatherForwardedAttributesFromClass(
            semanticModel,
            classDeclaration,
            default,
            out var forwarded);

        await Assert.That(forwarded.Length).IsGreaterThan(0);
        var containsTriggerAttribute = false;
        foreach (var forwardedAttribute in forwarded)
        {
            if (forwardedAttribute.TypeName.Contains("TriggerAttr", StringComparison.Ordinal))
            {
                containsTriggerAttribute = true;
                break;
            }
        }

        await Assert.That(containsTriggerAttribute).IsFalse();
    }

    /// <summary>Finds the class declaration used by the forwarded-attribute test.</summary>
    /// <param name="compilation">The compilation containing the class declaration.</param>
    /// <returns>A task that resolves to the class declaration.</returns>
    private static async Task<ClassDeclarationSyntax> GetClassDeclaration(CSharpCompilation compilation)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            foreach (var node in (await syntaxTree.GetRootAsync()).DescendantNodes())
            {
                if (node is ClassDeclarationSyntax { Identifier.Text: "C" } declaration)
                {
                    return declaration;
                }
            }
        }

        throw new InvalidOperationException("The test class declaration was not found.");
    }

    /// <summary>Retrieves a named attribute from an in-memory test type.</summary>
    /// <param name="source">The source text containing the target type.</param>
    /// <param name="typeName">The metadata name of the target type.</param>
    /// <param name="attributeSimpleName">The simple name of the attribute to retrieve.</param>
    /// <returns>The requested attribute data.</returns>
    private static AttributeData GetAttribute(string source, string typeName, string attributeSimpleName)
    {
        var compilation = CreateCompilation(source);
        var typeSymbol = compilation.GetTypeByMetadataName(typeName)
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in compilation.");

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name == attributeSimpleName)
            {
                return attribute;
            }
        }

        throw new InvalidOperationException($"Attribute '{attributeSimpleName}' was not found.");
    }

    /// <summary>Creates an in-memory compilation for an attribute extension test source.</summary>
    /// <param name="source">The source text to compile.</param>
    /// <returns>The created compilation.</returns>
    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        return CSharpCompilation.Create(
            assemblyName: "AttrDataExtTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new(OutputKind.DynamicallyLinkedLibrary));
    }
}
