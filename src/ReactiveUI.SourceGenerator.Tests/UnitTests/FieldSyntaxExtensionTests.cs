// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Extensions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="FieldSyntaxExtensions"/> covering
/// <c>GetGeneratedPropertyName</c> and <c>GetGeneratedFieldName</c>.
/// These are verified by running the generator over source strings and inspecting
/// the field/property symbols extracted from the resulting compilation.
/// </summary>
public sealed class FieldSyntaxExtensionTests
{
    /// <summary>A field named with leading underscore prefix has the underscore stripped and the first letter upper-cased.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenFieldHasLeadingUnderscoreThenPropertyNameCapitalises()
    {
        const string source = """
            namespace T;
            public class C
            {
                private int _myField;
            }
            """;
        var fieldSymbol = GetFieldSymbol(source, "_myField");

        var name = fieldSymbol.GetGeneratedPropertyName();

        await Assert.That(name).IsEqualTo("MyField");
    }

    /// <summary>A field named with "m_" prefix has the prefix stripped and the first remaining letter upper-cased.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenFieldHasMUnderscorePrefixThenPropertyNameCapitalises()
    {
        const string source = """
            namespace T;
            public class C
            {
                private int m_value;
            }
            """;
        var fieldSymbol = GetFieldSymbol(source, "m_value");

        var name = fieldSymbol.GetGeneratedPropertyName();

        await Assert.That(name).IsEqualTo("Value");
    }

    /// <summary>A field named with lowerCamel (no prefix) has its first letter upper-cased.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenFieldHasLowerCamelThenPropertyNameCapitalises()
    {
        const string source = """
            namespace T;
            public class C
            {
                private int count;
            }
            """;
        var fieldSymbol = GetFieldSymbol(source, "count");

        var name = fieldSymbol.GetGeneratedPropertyName();

        await Assert.That(name).IsEqualTo("Count");
    }

    /// <summary>Multiple leading underscores are all stripped before capitalisation.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenFieldHasMultipleLeadingUnderscoresThenAllStripped()
    {
        const string source = """
            namespace T;
            public class C
            {
                private int __item;
            }
            """;
        var fieldSymbol = GetFieldSymbol(source, "__item");

        var name = fieldSymbol.GetGeneratedPropertyName();

        await Assert.That(name).IsEqualTo("Item");
    }

    /// <summary>A single-character field name (after stripping) still capitalises correctly.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenFieldIsOneCharacterThenCapitalisedCorrectly()
    {
        const string source = """
            namespace T;
            public class C
            {
                private int _x;
            }
            """;
        var fieldSymbol = GetFieldSymbol(source, "_x");

        var name = fieldSymbol.GetGeneratedPropertyName();

        await Assert.That(name).IsEqualTo("X");
    }

    /// <summary>A property named "MyProperty" produces a backing field named "_myProperty".</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPropertyNamedMyPropertyThenFieldIsUnderscoreLower()
    {
        const string source = """
            namespace T;
            public class C
            {
                public int MyProperty { get; set; }
            }
            """;
        var propertySymbol = GetPropertySymbol(source, "MyProperty");

        var name = propertySymbol.GetGeneratedFieldName();

        await Assert.That(name).IsEqualTo("_myProperty");
    }

    /// <summary>A single-character property name produces a correct field name.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPropertyIsSingleCharacterThenFieldNameIsCorrect()
    {
        const string source = """
            namespace T;
            public class C
            {
                public int X { get; set; }
            }
            """;
        var propertySymbol = GetPropertySymbol(source, "X");

        var name = propertySymbol.GetGeneratedFieldName();

        await Assert.That(name).IsEqualTo("_x");
    }

    /// <summary>A property already starting with a lowercase letter still prefixes underscore.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WhenPropertyStartsWithLowercaseThenFieldNameHasUnderscore()
    {
        const string source = """
            namespace T;
            public class C
            {
                public int value { get; set; }
            }
            """;
        var propertySymbol = GetPropertySymbol(source, "value");

        var name = propertySymbol.GetGeneratedFieldName();

        await Assert.That(name).IsEqualTo("_value");
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

    /// <summary>Finds the single property symbol with the requested name.</summary>
    /// <param name="source">The source text containing the property.</param>
    /// <param name="propertyName">The property name to find.</param>
    /// <returns>The matching property symbol.</returns>
    private static IPropertySymbol GetPropertySymbol(string source, string propertyName)
    {
        var compilation = CreateCompilation(source);
        foreach (var symbol in compilation.GetSymbolsWithName(propertyName, SymbolFilter.Member))
        {
            if (symbol is IPropertySymbol propertySymbol)
            {
                return propertySymbol;
            }
        }

        throw new InvalidOperationException($"Property '{propertyName}' was not found.");
    }

    /// <summary>Creates an in-memory compilation for a field or property test source.</summary>
    /// <param name="source">The source text to compile.</param>
    /// <returns>The created compilation.</returns>
    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        return CSharpCompilation.Create(
            assemblyName: "FieldSyntaxTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new(OutputKind.DynamicallyLinkedLibrary));
    }
}
