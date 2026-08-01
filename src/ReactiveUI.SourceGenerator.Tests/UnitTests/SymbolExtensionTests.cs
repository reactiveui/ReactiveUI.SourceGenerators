// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for symbol and compilation extensions using real in-memory Roslyn symbols.</summary>
public sealed class SymbolExtensionTests
{
    /// <summary>The fully qualified metadata name of <see cref="ObsoleteAttribute"/>.</summary>
    private const string ObsoleteAttributeMetadataName = "System.ObsoleteAttribute";

    /// <summary>The fully qualified metadata name of the test-derived type.</summary>
    private const string DerivedTypeMetadataName = "T.Derived";

    /// <summary>The metadata-name prefix used by hierarchy classifier tests.</summary>
    private const string TypeMetadataNamePrefix = "T.Prefix";

    /// <summary>Source for a public class in the shared test namespace.</summary>
    private const string PublicClassSource = """
        namespace T;
        public class C { }
        """;

    /// <summary>GetFullyQualifiedName returns the global:: prefixed name.</summary>
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

    /// <summary>GetFullyQualifiedNameWithNullabilityAnnotations includes the nullable annotation marker.</summary>
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

    /// <summary>HasAttributeWithFullyQualifiedMetadataName returns true when the attribute is present.</summary>
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

        var result = symbol.HasAttributeWithFullyQualifiedMetadataName(ObsoleteAttributeMetadataName);

        await Assert.That(result).IsTrue();
    }

    /// <summary>HasAttributeWithFullyQualifiedMetadataName returns false when the attribute is absent.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeAbsentThenHasAttributeWithNameReturnsFalse()
    {
        var symbol = GetTypeSymbol(
            """
            namespace AttributeFreeTest;
            public class C { }
            """,
            "C");

        var result = symbol.HasAttributeWithFullyQualifiedMetadataName(ObsoleteAttributeMetadataName);

        await Assert.That(result).IsFalse();
    }

    /// <summary>TryGetAttributeWithFullyQualifiedMetadataName returns true and outputs AttributeData when present.</summary>
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
            ObsoleteAttributeMetadataName,
            out var attributeData);

        await Assert.That(found).IsTrue();
        await Assert.That(attributeData).IsNotNull();
    }

    /// <summary>TryGetAttributeWithFullyQualifiedMetadataName returns false when attribute is absent.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenAttributeAbsentThenTryGetAttributeFails()
    {
        var symbol = GetTypeSymbol(
            """
            namespace AttributeLookupTest;
            public class C { }
            """,
            "C");

        var found = symbol.TryGetAttributeWithFullyQualifiedMetadataName(
            ObsoleteAttributeMetadataName,
            out var attributeData);

        await Assert.That(found).IsFalse();
        await Assert.That(attributeData).IsNull();
    }

    /// <summary>Type-based attribute lookup covers null, matching, and nonmatching symbols.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task AttributeTypeLookupCoversAllOutcomes()
    {
        var compilation = CreateCompilation("""
            using System;
            namespace T;
            [Obsolete]
            public class C { }
            """);
        var type = compilation.GetTypeByMetadataName("T.C")!;
        var obsoleteType = compilation.GetTypeByMetadataName(ObsoleteAttributeMetadataName)!;
        var attributeUsageType = compilation.GetTypeByMetadataName("System.AttributeUsageAttribute")!;

        await Assert.That(type.HasAttributeWithType(null)).IsFalse();
        await Assert.That(type.HasAttributeWithType(obsoleteType)).IsTrue();
        await Assert.That(type.HasAttributeWithType(attributeUsageType)).IsFalse();
        await Assert.That(type.TryGetAttributeWithType(obsoleteType, out var found)).IsTrue();
        await Assert.That(found).IsNotNull();
        await Assert.That(type.TryGetAttributeWithType(attributeUsageType, out var missing)).IsFalse();
        await Assert.That(missing).IsNull();
    }

    /// <summary>GetEffectiveAccessibility returns Public for a public class.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPublicClassThenEffectiveAccessibilityIsPublic()
    {
        var symbol = GetTypeSymbol(PublicClassSource, "C");

        var accessibility = symbol.GetEffectiveAccessibility();

        await Assert.That(accessibility).IsEqualTo(Accessibility.Public);
    }

    /// <summary>GetEffectiveAccessibility returns Internal for an internal class.</summary>
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

    /// <summary>Effective-accessibility and assembly-access checks cover nested and special symbols.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task EffectiveAccessibilityAndAssemblyAccessCoverSpecialSymbols()
    {
        var compilation = CreateCompilation("""
            namespace T;
            internal class Container<T>
            {
                public int PublicField;
                private int PrivateField;
                public void Method(int parameter) { }
            }
            public class PublicContainer
            {
                public int PublicField;
            }
            """);
        var internalType = compilation.GetTypeByMetadataName("T.Container`1")!;
        var publicType = compilation.GetTypeByMetadataName("T.PublicContainer")!;
        var internalPublicField = (IFieldSymbol)internalType.GetMembers("PublicField")[0];
        var privateField = (IFieldSymbol)internalType.GetMembers("PrivateField")[0];
        var publicField = (IFieldSymbol)publicType.GetMembers("PublicField")[0];
        var parameter = ((IMethodSymbol)internalType.GetMembers("Method")[0]).Parameters[0];
        var foreignAssembly = CSharpCompilation.Create("ForeignAssembly", references: TestCompilationReferences.CreateDefault()).Assembly;

        await Assert.That(internalType.TypeParameters[0].GetEffectiveAccessibility()).IsEqualTo(Accessibility.Private);
        await Assert.That(parameter.GetEffectiveAccessibility()).IsEqualTo(Accessibility.Internal);
        await Assert.That(privateField.GetEffectiveAccessibility()).IsEqualTo(Accessibility.Private);
        await Assert.That(internalPublicField.GetEffectiveAccessibility()).IsEqualTo(Accessibility.Internal);
        await Assert.That(publicField.CanBeAccessedFrom(foreignAssembly)).IsTrue();
        await Assert.That(internalPublicField.CanBeAccessedFrom(compilation.Assembly)).IsTrue();
        await Assert.That(internalPublicField.CanBeAccessedFrom(foreignAssembly)).IsFalse();
    }

    /// <summary>GetAccessibilityString returns "public" for a public symbol.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPublicThenGetAccessibilityStringReturnsPublic()
    {
        var symbol = GetTypeSymbol(PublicClassSource, "C");

        await Assert.That(symbol.GetAccessibilityString()).IsEqualTo("public");
    }

    /// <summary>GetAccessibilityString returns "internal" for an internal symbol.</summary>
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

    /// <summary>GetAccessibilityString preserves protected and compound C# accessibility semantics.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task CompoundAccessibilityStringsMatchCSharpKeywords()
    {
        const string source = """
            namespace T;
            public class C
            {
                private int PrivateField;
                protected int ProtectedField;
                protected internal int ProtectedInternalField;
                private protected int PrivateProtectedField;
            }
            """;

        await Assert.That(GetFieldSymbol(source, "PrivateField").GetAccessibilityString()).IsEqualTo("private");
        await Assert.That(GetFieldSymbol(source, "ProtectedField").GetAccessibilityString()).IsEqualTo("protected");
        await Assert.That(GetFieldSymbol(source, "ProtectedInternalField").GetAccessibilityString()).IsEqualTo("protected internal");
        await Assert.That(GetFieldSymbol(source, "PrivateProtectedField").GetAccessibilityString()).IsEqualTo("private protected");
    }

    /// <summary>HasOrInheritsFromFullyQualifiedMetadataName returns true for the type itself.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeIsSelfThenHasOrInheritsReturnsTrue()
    {
        var symbol = GetTypeSymbol(PublicClassSource, "C");

        var result = symbol.HasOrInheritsFromFullyQualifiedMetadataName("T.C");

        await Assert.That(result).IsTrue();
    }

    /// <summary>HasOrInheritsFromFullyQualifiedMetadataName returns true for a direct base class.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeDerivedFromBaseThenHasOrInheritsReturnsTrue()
    {
        var compilation = CreateCompilation("""
            namespace T;
            public class Base { }
            public class Derived : Base { }
            """);

        var derived = compilation.GetTypeByMetadataName(DerivedTypeMetadataName)!;
        var result = derived.HasOrInheritsFromFullyQualifiedMetadataName("T.Base");

        await Assert.That(result).IsTrue();
    }

    /// <summary>HasOrInheritsFromFullyQualifiedMetadataName returns false for an unrelated type.</summary>
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

    /// <summary>InheritsFromFullyQualifiedMetadataName returns false for the type itself (not inherited).</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenTypeSelfThenInheritsReturnsFalse()
    {
        var symbol = GetTypeSymbol(PublicClassSource, "C");

        var result = symbol.InheritsFromFullyQualifiedMetadataName("T.C");

        await Assert.That(result).IsFalse();
    }

    /// <summary>ImplementsFullyQualifiedMetadataName returns true when the interface is implemented.</summary>
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

    /// <summary>Hierarchy helpers cover negative interfaces, prefix matching, and inherited attributes.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task HierarchyClassifiersCoverPositiveAndNegativePaths()
    {
        var compilation = CreateCompilation("""
            using System;

            namespace T;

            [Obsolete]
            public class PrefixBase { }

            public class Derived : PrefixBase, IDisposable
            {
                public void Dispose() { }
            }

            public class Unrelated { }
            """);
        var derived = compilation.GetTypeByMetadataName(DerivedTypeMetadataName)!;
        var unrelated = compilation.GetTypeByMetadataName("T.Unrelated")!;

        await Assert.That(derived.ImplementsFullyQualifiedMetadataName("System.IDisposable")).IsTrue();
        await Assert.That(derived.ImplementsFullyQualifiedMetadataName("System.ICloneable")).IsFalse();
        await Assert.That(derived.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith(TypeMetadataNamePrefix)).IsTrue();
        await Assert.That(derived.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("Missing.Prefix")).IsFalse();
        await Assert.That(derived.InheritsFromFullyQualifiedMetadataNameStartingWith(TypeMetadataNamePrefix)).IsTrue();
        await Assert.That(unrelated.InheritsFromFullyQualifiedMetadataNameStartingWith(TypeMetadataNamePrefix)).IsFalse();
        await Assert.That(derived.HasOrInheritsAttributeWithFullyQualifiedMetadataName(ObsoleteAttributeMetadataName)).IsTrue();
        await Assert.That(unrelated.HasOrInheritsAttributeWithFullyQualifiedMetadataName(ObsoleteAttributeMetadataName)).IsFalse();
    }

    /// <summary>Task, observable, scheduler, nullability, and task-result classifiers cover each API shape.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task TypeShapeClassifiersCoverSupportedAndUnsupportedTypes()
    {
        var compilation = CreateCompilation("""
            #nullable enable
            namespace T;

            public sealed class Shapes
            {
                public System.Threading.Tasks.Task Task = null!;
                public System.Threading.Tasks.Task<int> TaskOfInt = null!;
                public object Plain = null!;
                public System.IObservable<bool> ObservableBool = null!;
                public System.IObservable<int> ObservableInt = null!;
                public ReactiveUI.Primitives.Concurrency.ISequencer Sequencer = null!;
                public System.Reactive.Concurrency.IScheduler Scheduler = null!;
                public string? NullableText;
            }
            """);
        var shape = compilation.GetTypeByMetadataName("T.Shapes")!;
        var task = GetFieldType(shape, nameof(Task));
        var taskOfInt = GetFieldType(shape, "TaskOfInt");
        var plain = GetFieldType(shape, "Plain");
        var observableBool = GetFieldType(shape, "ObservableBool");
        var observableInt = GetFieldType(shape, "ObservableInt");
        var sequencer = GetFieldType(shape, "Sequencer");
        var scheduler = GetFieldType(shape, "Scheduler");
        var nullableText = GetFieldType(shape, "NullableText");
        ITypeSymbol? missing = null;

        await Assert.That(task.IsTaskReturnType()).IsTrue();
        await Assert.That(plain.IsTaskReturnType()).IsFalse();
        await Assert.That(missing.IsTaskReturnType()).IsFalse();
        await Assert.That(observableBool.IsObservableReturnType()).IsTrue();
        await Assert.That(plain.IsObservableReturnType()).IsFalse();
        await Assert.That(missing.IsObservableReturnType()).IsFalse();
        await Assert.That(sequencer.IsSchedulerType(ReactiveUiApi.Primitives)).IsTrue();
        await Assert.That(sequencer.IsSchedulerType(ReactiveUiApi.SystemReactive)).IsFalse();
        await Assert.That(scheduler.IsSchedulerType(ReactiveUiApi.SystemReactive)).IsTrue();
        await Assert.That(scheduler.IsSchedulerType(ReactiveUiApi.Primitives)).IsFalse();
        await Assert.That(missing.IsSchedulerType(ReactiveUiApi.Primitives)).IsFalse();
        await Assert.That(observableBool.IsObservableBoolType()).IsTrue();
        await Assert.That(observableInt.IsObservableBoolType()).IsFalse();
        await Assert.That(missing.IsObservableBoolType()).IsFalse();
        await Assert.That(nullableText.IsNullableType()).IsTrue();
        await Assert.That(plain.IsNullableType()).IsFalse();
        await Assert.That(missing.IsNullableType()).IsFalse();
        await Assert.That(taskOfInt.GetTaskReturnType(compilation).SpecialType).IsEqualTo(SpecialType.System_Int32);
        await Assert.That(task.GetTaskReturnType(compilation).SpecialType).IsEqualTo(SpecialType.System_Void);
    }

    /// <summary>GetFullyQualifiedMetadataName returns dotted name without global:: prefix.</summary>
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

    /// <summary>GetAllMembers returns members from both the type and its base types.</summary>
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

        var derived = (INamedTypeSymbol)compilation.GetTypeByMetadataName(DerivedTypeMetadataName)!;
        var members = new List<string>();
        foreach (var member in derived.GetAllMembers())
        {
            members.Add(member.Name);
        }

        await Assert.That(members.Contains("DerivedField")).IsTrue();
        await Assert.That(members.Contains("BaseField")).IsTrue();
    }

    /// <summary>GetAllMembers(name) returns members with the matching name from base types.</summary>
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

        var derived = (INamedTypeSymbol)compilation.GetTypeByMetadataName(DerivedTypeMetadataName)!;
        List<ISymbol> members = [.. derived.GetAllMembers("Shared")];

        await Assert.That(members.Count).IsEqualTo(1);
        await Assert.That(members[0].Name).IsEqualTo("Shared");
    }

    /// <summary>GetTypeString returns "class" for a regular class.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenRegularClassThenGetTypeStringReturnsClass()
    {
        var symbol = (INamedTypeSymbol)GetTypeSymbol(PublicClassSource, "C");

        await Assert.That(symbol.GetTypeString()).IsEqualTo("class");
    }

    /// <summary>GetTypeString returns "record" for a record class.</summary>
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

    /// <summary>GetTypeString returns "struct" for a regular struct.</summary>
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

    /// <summary>GetTypeString returns "record struct" for a record struct.</summary>
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

    /// <summary>GetTypeString returns "interface" for an interface.</summary>
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

    /// <summary>HasAccessibleTypeWithMetadataName returns true for System.String (always accessible).</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenWellKnownTypeThenHasAccessibleTypeReturnsTrue()
    {
        var compilation = CreateCompilation("namespace T; public class C {}");

        var result = compilation.HasAccessibleTypeWithMetadataName("System.String");

        await Assert.That(result).IsTrue();
    }

    /// <summary>HasAccessibleTypeWithMetadataName returns false for a type that doesn't exist.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenUnknownTypeThenHasAccessibleTypeReturnsFalse()
    {
        var compilation = CreateCompilation("namespace T; public class C {}");

        var result = compilation.HasAccessibleTypeWithMetadataName("DoesNot.Exist.Type");

        await Assert.That(result).IsFalse();
    }

    /// <summary>SymbolInfo falls back to a single candidate constructor for an incomplete attribute invocation.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task SingleAttributeCandidateResolvesItsContainingType()
    {
        var compilation = CreateCompilation(
            """
            using System;
            namespace T;
            [CLSCompliant]
            public class C { }
            """);
        SyntaxTree? syntaxTree = null;
        foreach (var tree in compilation.SyntaxTrees)
        {
            syntaxTree ??= tree;
        }

        if (syntaxTree is null)
        {
            throw new InvalidOperationException("The test syntax tree was not found.");
        }

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        AttributeSyntax? attribute = null;
        foreach (var node in (await syntaxTree.GetRootAsync()).DescendantNodes())
        {
            if (attribute is null && node is AttributeSyntax attributeSyntax)
            {
                attribute = attributeSyntax;
            }
        }

        var resolved = semanticModel.GetSymbolInfo(
            attribute ?? throw new InvalidOperationException("The test attribute was not found.")).TryGetAttributeTypeSymbol(out var typeSymbol);

        await Assert.That(resolved).IsTrue();
        await Assert.That(typeSymbol?.Name).IsEqualTo(nameof(CLSCompliantAttribute));
    }

    /// <summary>HasAccessibleTypeWithMetadataName resolves ambiguous referenced types according to effective accessibility.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task AmbiguousReferencedTypesRespectPublicAndFriendAccessibility()
    {
        const string consumerSource = "namespace Consumer; public class C { }";
        var publicCompilation = CreateCompilation(
            consumerSource,
            CreateMetadataReference("PublicOne", "namespace Shared; public class Candidate { }"),
            CreateMetadataReference("PublicTwo", "namespace Shared; public class Candidate { }"));
        var friendCompilation = CreateCompilation(
            consumerSource,
            CreateMetadataReference(
                "FriendOne",
                """
                using System.Runtime.CompilerServices;
                [assembly: InternalsVisibleTo("SymbolExtTests")]
                namespace Shared;
                internal class FriendCandidate { }
                """),
            CreateMetadataReference(
                "FriendTwo",
                """
                using System.Runtime.CompilerServices;
                [assembly: InternalsVisibleTo("SymbolExtTests")]
                namespace Shared;
                internal class FriendCandidate { }
                """));
        var inaccessibleCompilation = CreateCompilation(
            consumerSource,
            CreateMetadataReference("HiddenOne", "namespace Shared; internal class HiddenCandidate { }"),
            CreateMetadataReference("HiddenTwo", "namespace Shared; internal class HiddenCandidate { }"));

        await Assert.That(publicCompilation.HasAccessibleTypeWithMetadataName("Shared.Candidate")).IsTrue();
        await Assert.That(friendCompilation.HasAccessibleTypeWithMetadataName("Shared.FriendCandidate")).IsTrue();
        await Assert.That(inaccessibleCompilation.HasAccessibleTypeWithMetadataName("Shared.HiddenCandidate")).IsFalse();
    }

    /// <summary>Finds the single type symbol with the requested name.</summary>
    /// <param name="source">The source text containing the type.</param>
    /// <param name="typeName">The type name to find.</param>
    /// <returns>The matching type symbol.</returns>
    private static ITypeSymbol GetTypeSymbol(string source, string typeName)
    {
        var compilation = CreateCompilation(source);
        foreach (var symbol in compilation.GetSymbolsWithName(typeName, SymbolFilter.Type))
        {
            if (symbol is ITypeSymbol typeSymbol)
            {
                return typeSymbol;
            }
        }

        throw new InvalidOperationException($"Type '{typeName}' was not found.");
    }

    /// <summary>Finds the single field symbol with the requested name.</summary>
    /// <param name="source">The source text containing the field.</param>
    /// <param name="fieldName">The field name to find.</param>
    /// <returns>The matching field symbol.</returns>
    private static IFieldSymbol GetFieldSymbol(string source, string fieldName)
    {
        var compilation = CreateCompilation(source);
        foreach (var symbol in compilation.GetSymbolsWithName(fieldName, SymbolFilter.Member))
        {
            if (symbol is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol;
            }
        }

        throw new InvalidOperationException($"Field '{fieldName}' was not found.");
    }

    /// <summary>Gets a named field's type from a containing type.</summary>
    /// <param name="containingType">The containing type.</param>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The field type.</returns>
    private static ITypeSymbol GetFieldType(INamedTypeSymbol containingType, string fieldName)
    {
        foreach (var member in containingType.GetMembers(fieldName))
        {
            if (member is IFieldSymbol field)
            {
                return field.Type;
            }
        }

        throw new InvalidOperationException($"Field '{fieldName}' was not found.");
    }

    /// <summary>Creates an in-memory compilation for a symbol extension test source.</summary>
    /// <param name="source">The source text to compile.</param>
    /// <param name="additionalReferences">Additional references visible to the compilation.</param>
    /// <returns>The created compilation.</returns>
    private static CSharpCompilation CreateCompilation(string source, params MetadataReference[] additionalReferences)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        return CSharpCompilation.Create(
            assemblyName: "SymbolExtTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault().AddRange(additionalReferences),
            options: new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>Compiles source into a portable metadata reference.</summary>
    /// <param name="assemblyName">The reference assembly name.</param>
    /// <param name="source">The reference source.</param>
    /// <returns>The compiled metadata reference.</returns>
    private static PortableExecutableReference CreateMetadataReference(string assemblyName, string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
