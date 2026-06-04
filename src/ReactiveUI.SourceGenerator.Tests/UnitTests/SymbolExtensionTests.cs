// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Extensions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="ReactiveUI.SourceGenerators.Extensions.ISymbolExtensions"/>,
/// <see cref="ReactiveUI.SourceGenerators.Extensions.ITypeSymbolExtensions"/>,
/// <see cref="ReactiveUI.SourceGenerators.Extensions.INamedTypeSymbolExtensions"/>, and
/// <see cref="ReactiveUI.SourceGenerators.Extensions.CompilationExtensions"/>.
/// All tests compile small in-memory programs to obtain real Roslyn symbol instances.
/// </summary>
public sealed class SymbolExtensionTests
{
    /// <summary>
    /// GetFullyQualifiedName returns the global:: prefixed name.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGetFullyQualifiedNameCalledThenReturnsGlobalPrefixedName()
    {
        var symbol = GetTypeSymbol(
            """
            namespace Foo.Bar;
            public class MyClass { }
            """,
            "MyClass");

        var name = symbol.GetFullyQualifiedName();

        await Assert.That(name).IsEqualTo("global::Foo.Bar.MyClass");
    }

    /// <summary>
    /// GetFullyQualifiedNameWithNullabilityAnnotations includes the nullable annotation marker.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGetFullyQualifiedNameWithNullabilityCalledThenIncludesAnnotation()
    {
        var symbol = GetFieldSymbol(
            """
            #nullable enable
            namespace T;
            public class C
            {
                public string? _name;
            }
            """,
            "_name");

        var name = symbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();

        await Assert.That(name).IsEqualTo("string?");
    }

    /// <summary>
    /// HasAttributeWithFullyQualifiedMetadataName returns true when the attribute is present.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributePresentThenHasAttributeWithNameReturnsTrue()
    {
        var symbol = GetTypeSymbol(
            """
            using System;
            namespace T;
            [Obsolete]
            public class C { }
            """,
            "C");

        var result = symbol.HasAttributeWithFullyQualifiedMetadataName("System.ObsoleteAttribute");

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// HasAttributeWithFullyQualifiedMetadataName returns false when the attribute is absent.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeAbsentThenHasAttributeWithNameReturnsFalse()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        var result = symbol.HasAttributeWithFullyQualifiedMetadataName("System.ObsoleteAttribute");

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// TryGetAttributeWithFullyQualifiedMetadataName returns true and outputs AttributeData when present.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributePresentThenTryGetAttributeSucceeds()
    {
        var symbol = GetTypeSymbol(
            """
            using System;
            namespace T;
            [Obsolete("old")]
            public class C { }
            """,
            "C");

        var found = symbol.TryGetAttributeWithFullyQualifiedMetadataName(
            "System.ObsoleteAttribute",
            out var attributeData);

        await Assert.That(found).IsTrue();
        await Assert.That(attributeData).IsNotNull();
    }

    /// <summary>
    /// TryGetAttributeWithFullyQualifiedMetadataName returns false when attribute is absent.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeAbsentThenTryGetAttributeFails()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        var found = symbol.TryGetAttributeWithFullyQualifiedMetadataName(
            "System.ObsoleteAttribute",
            out var attributeData);

        await Assert.That(found).IsFalse();
        await Assert.That(attributeData).IsNull();
    }

    /// <summary>
    /// GetEffectiveAccessibility returns Public for a public class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPublicClassThenEffectiveAccessibilityIsPublic()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        var accessibility = symbol.GetEffectiveAccessibility();

        await Assert.That(accessibility).IsEqualTo(Accessibility.Public);
    }

    /// <summary>
    /// GetEffectiveAccessibility returns Internal for an internal class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenInternalClassThenEffectiveAccessibilityIsInternal()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            internal class C { }
            """,
            "C");

        var accessibility = symbol.GetEffectiveAccessibility();

        await Assert.That(accessibility).IsEqualTo(Accessibility.Internal);
    }

    /// <summary>
    /// GetAccessibilityString returns "public" for a public symbol.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPublicThenGetAccessibilityStringReturnsPublic()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        await Assert.That(symbol.GetAccessibilityString()).IsEqualTo("public");
    }

    /// <summary>
    /// GetAccessibilityString returns "internal" for an internal symbol.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenInternalThenGetAccessibilityStringReturnsInternal()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            internal class C { }
            """,
            "C");

        await Assert.That(symbol.GetAccessibilityString()).IsEqualTo("internal");
    }

    /// <summary>
    /// HasOrInheritsFromFullyQualifiedMetadataName returns true for the type itself.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeIsSelfThenHasOrInheritsReturnsTrue()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        var result = symbol.HasOrInheritsFromFullyQualifiedMetadataName("T.C");

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// HasOrInheritsFromFullyQualifiedMetadataName returns true for a direct base class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeDerivedFromBaseThenHasOrInheritsReturnsTrue()
    {
        var compilation = CreateCompilation("""
            namespace T;
            public class Base { }
            public class Derived : Base { }
            """);

        var derived = compilation.GetTypeByMetadataName("T.Derived")!;
        var result = derived.HasOrInheritsFromFullyQualifiedMetadataName("T.Base");

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// HasOrInheritsFromFullyQualifiedMetadataName returns false for an unrelated type.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeUnrelatedThenHasOrInheritsReturnsFalse()
    {
        var compilation = CreateCompilation("""
            namespace T;
            public class A { }
            public class B { }
            """);

        var a = compilation.GetTypeByMetadataName("T.A")!;
        var result = a.HasOrInheritsFromFullyQualifiedMetadataName("T.B");

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// InheritsFromFullyQualifiedMetadataName returns false for the type itself (not inherited).
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeSelfThenInheritsReturnsFalse()
    {
        var symbol = GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        var result = symbol.InheritsFromFullyQualifiedMetadataName("T.C");

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// ImplementsFullyQualifiedMetadataName returns true when the interface is implemented.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenInterfaceImplementedThenImplementsReturnsTrue()
    {
        var compilation = CreateCompilation("""
            namespace T;
            public interface IFoo { }
            public class C : IFoo { }
            """);

        var c = compilation.GetTypeByMetadataName("T.C")!;
        var result = c.ImplementsFullyQualifiedMetadataName("T.IFoo");

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// GetFullyQualifiedMetadataName returns dotted name without global:: prefix.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGetFullyQualifiedMetadataNameCalledThenReturnsDottedName()
    {
        var symbol = GetTypeSymbol(
            """
            namespace Foo.Bar;
            public class Baz { }
            """,
            "Baz");

        var name = symbol.GetFullyQualifiedMetadataName();

        await Assert.That(name).IsEqualTo("Foo.Bar.Baz");
    }

    /// <summary>
    /// GetAllMembers returns members from both the type and its base types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGetAllMembersCalledThenIncludesInheritedMembers()
    {
        var compilation = CreateCompilation("""
            namespace T;
            public class Base
            {
                public int BaseField;
            }
            public class Derived : Base
            {
                public int DerivedField;
            }
            """);

        var derived = (INamedTypeSymbol)compilation.GetTypeByMetadataName("T.Derived")!;
        var members = derived.GetAllMembers().Select(m => m.Name).ToList();

        await Assert.That(members.Contains("DerivedField")).IsTrue();
        await Assert.That(members.Contains("BaseField")).IsTrue();
    }

    /// <summary>
    /// GetAllMembers(name) returns members with the matching name from base types.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenGetAllMembersWithNameCalledThenFiltersCorrectly()
    {
        var compilation = CreateCompilation("""
            namespace T;
            public class Base
            {
                public int Shared;
            }
            public class Derived : Base
            {
                public int Unique;
            }
            """);

        var derived = (INamedTypeSymbol)compilation.GetTypeByMetadataName("T.Derived")!;
        var members = derived.GetAllMembers("Shared").ToList();

        await Assert.That(members.Count).IsEqualTo(1);
        await Assert.That(members[0].Name).IsEqualTo("Shared");
    }

    /// <summary>
    /// GetTypeString returns "class" for a regular class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenRegularClassThenGetTypeStringReturnsClass()
    {
        var symbol = (INamedTypeSymbol)GetTypeSymbol(
            """
            namespace T;
            public class C { }
            """,
            "C");

        await Assert.That(symbol.GetTypeString()).IsEqualTo("class");
    }

    /// <summary>
    /// GetTypeString returns "record" for a record class.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenRecordClassThenGetTypeStringReturnsRecord()
    {
        var symbol = (INamedTypeSymbol)GetTypeSymbol(
            """
            namespace T;
            public record C { }
            """,
            "C");

        await Assert.That(symbol.GetTypeString()).IsEqualTo("record");
    }

    /// <summary>
    /// GetTypeString returns "struct" for a regular struct.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenStructThenGetTypeStringReturnsStruct()
    {
        var symbol = (INamedTypeSymbol)GetTypeSymbol(
            """
            namespace T;
            public struct S { }
            """,
            "S");

        await Assert.That(symbol.GetTypeString()).IsEqualTo("struct");
    }

    /// <summary>
    /// GetTypeString returns "record struct" for a record struct.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenRecordStructThenGetTypeStringReturnsRecordStruct()
    {
        var symbol = (INamedTypeSymbol)GetTypeSymbol(
            """
            namespace T;
            public record struct RS { }
            """,
            "RS");

        await Assert.That(symbol.GetTypeString()).IsEqualTo("record struct");
    }

    /// <summary>
    /// GetTypeString returns "interface" for an interface.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenInterfaceThenGetTypeStringReturnsInterface()
    {
        var symbol = (INamedTypeSymbol)GetTypeSymbol(
            """
            namespace T;
            public interface IFoo { }
            """,
            "IFoo");

        await Assert.That(symbol.GetTypeString()).IsEqualTo("interface");
    }

    /// <summary>
    /// HasAccessibleTypeWithMetadataName returns true for System.String (always accessible).
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenWellKnownTypeThenHasAccessibleTypeReturnsTrue()
    {
        var compilation = CreateCompilation("namespace T; public class C {}");

        var result = compilation.HasAccessibleTypeWithMetadataName("System.String");

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// HasAccessibleTypeWithMetadataName returns false for a type that doesn't exist.
    /// </summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenUnknownTypeThenHasAccessibleTypeReturnsFalse()
    {
        var compilation = CreateCompilation("namespace T; public class C {}");

        var result = compilation.HasAccessibleTypeWithMetadataName("DoesNot.Exist.Type");

        await Assert.That(result).IsFalse();
    }

    private static ITypeSymbol GetTypeSymbol(string source, string typeName)
    {
        var compilation = CreateCompilation(source);
        return compilation.GetSymbolsWithName(typeName, SymbolFilter.Type)
            .OfType<ITypeSymbol>()
            .Single();
    }

    private static IFieldSymbol GetFieldSymbol(string source, string fieldName)
    {
        var compilation = CreateCompilation(source);
        return compilation.GetSymbolsWithName(fieldName, SymbolFilter.Member)
            .OfType<IFieldSymbol>()
            .Single();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        return CSharpCompilation.Create(
            assemblyName: "SymbolExtTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
