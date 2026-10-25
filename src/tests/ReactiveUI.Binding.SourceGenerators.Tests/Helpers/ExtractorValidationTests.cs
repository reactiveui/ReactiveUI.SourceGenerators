// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.Tests.Shared;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Unit tests for <see cref="ExtractorValidation"/> helper methods. Tests the guard-clause branches extracted from extractor classes.</summary>
public partial class ExtractorValidationTests
{
    /// <summary>The <c>selector</c> name these tests generate against.</summary>
    private const string SelectorName = "selector";

    /// <summary>The <c>string</c> name these tests generate against.</summary>
    private const string StringName = "string";

    /// <summary>The <c>int</c> name these tests generate against.</summary>
    private const string IntName = "int";

    /// <summary>A class name no recognized extension class uses.</summary>
    private const string UnknownClassName = "CustomExtensions";

    /// <summary>A class declaring one parameter under each name and shape these tests look for.</summary>
    private const string SelectorProbeSource = """
                                               namespace Probe
                                               {
                                                   public class Holder
                                                   {
                                                       public void TakesSelector(System.Func<string> selector)
                                                       {
                                                       }

                                                       public void TakesAnotherName(System.Func<string> otherParam)
                                                       {
                                                       }

                                                       public void TakesConversion(System.Func<int> conversionFunc)
                                                       {
                                                       }

                                                       public void TakesPlainType(string selector)
                                                       {
                                                       }
                                                   }
                                               }
                                               """;

    /// <summary>Verifies that the stub extension class name is recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_StubClassName_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(nameof(ReactiveUIBindingExtensions));
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that the scheduler extension class name is recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_SchedulerClassName_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(nameof(ReactiveSchedulerExtensions));
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that the generated extension class name is recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_GeneratedClassName_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass("__ReactiveUIGeneratedBindings");
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that an unrecognized class name is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_UnknownClassName_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(UnknownClassName);
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that null is rejected as unrecognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_NullName_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass((string?)null);
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that a null containing type is rejected as unrecognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_NullContainingType_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass((INamedTypeSymbol?)null);
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// A member declared in an extension block belongs to a synthesized type nested inside the static class
    /// rather than to the class itself, so recognition has to reach the enclosing name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_SynthesizedTypeNestedInRecognizedClass_ReturnsTrue()
    {
        var nested = SynthesizedNestedTypeIn(nameof(ReactiveSchedulerExtensions));

        var result = ExtractorValidation.IsRecognizedExtensionClass(nested);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Reaching the enclosing name does not make an unrecognized class recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_SynthesizedTypeNestedInUnknownClass_ReturnsFalse()
    {
        var nested = SynthesizedNestedTypeIn(UnknownClassName);

        var result = ExtractorValidation.IsRecognizedExtensionClass(nested);

        await Assert.That(result).IsFalse();
    }

    /// <summary>A plainly named type is judged on its own name, not on the class that encloses it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_NamedTypeNestedInRecognizedClass_ReturnsFalse()
    {
        var outer = CompiledType($$"""
                                  public static class {{nameof(ReactiveSchedulerExtensions)}}
                                  {
                                      public class Inner
                                      {
                                      }
                                  }
                                  """);

        var result = ExtractorValidation.IsRecognizedExtensionClass(outer.GetTypeMembers("Inner")[0]);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that an empty string is rejected as unrecognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_Empty_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(string.Empty);
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that argument count at the minimum is accepted.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_ExactMinimum_ReturnsTrue()
    {
        const int ArgumentCount = 3;
        var result = ExtractorValidation.HasMinimumArguments(ArgumentCount, ArgumentCount);
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that argument count above the minimum is accepted.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_AboveMinimum_ReturnsTrue()
    {
        const int ArgumentCount = 5;
        const int MinimumRequired = 3;
        var result = ExtractorValidation.HasMinimumArguments(ArgumentCount, MinimumRequired);
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that argument count below the minimum is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_BelowMinimum_ReturnsFalse()
    {
        const int ArgumentCount = 2;
        const int MinimumRequired = 3;
        var result = ExtractorValidation.HasMinimumArguments(ArgumentCount, MinimumRequired);
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that zero arguments is rejected when minimum is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasMinimumArguments_Zero_ReturnsFalse()
    {
        const int MinimumRequired = 3;
        var result = ExtractorValidation.HasMinimumArguments(0, MinimumRequired);
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that a populated immutable array is accepted.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_PopulatedArray_ReturnsTrue()
    {
        var items = ImmutableArray.Create("a", "b");
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that a single-item array is accepted.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_SingleItem_ReturnsTrue()
    {
        var items = ImmutableArray.Create("a");
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that an empty immutable array is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_EmptyArray_ReturnsFalse()
    {
        var items = ImmutableArray<string>.Empty;
        var result = ExtractorValidation.HasItems(items);
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that a default (uninitialized) immutable array is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasItems_DefaultArray_ReturnsFalse()
    {
        var result = ExtractorValidation.HasItems(default(ImmutableArray<string>));
        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that ExtractMethodSymbol returns null for a default SymbolInfo.
    /// Exercises the "symbol is not IMethodSymbol" guard extracted from all extractors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractMethodSymbol_DefaultSymbolInfo_ReturnsNull()
    {
        var result = ExtractorValidation.ExtractMethodSymbol(default);
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that GetTypeDisplayName returns null for a null type symbol.
    /// Exercises the type info null guard extracted from all extractors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetTypeDisplayName_NullType_ReturnsNull()
    {
        var result = ExtractorValidation.GetTypeDisplayName(null);
        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// Verifies that ResolveEventArgsType returns the fallback for a null delegate type.
    /// Exercises the event delegate null guard extracted from EventHelpers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveEventArgsType_NullDelegateType_ReturnsFallback()
    {
        var result = ExtractorValidation.ResolveEventArgsType(null);
        await Assert.That(result).IsEqualTo("global::System.EventArgs");
    }

    /// <summary>Verifies that FindSelectorReturnType returns null when the parameters array is empty.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_EmptyParameters_ReturnsNull()
    {
        var result = ExtractorValidation.FindSelectorReturnType(
            [],
            SelectorName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that FindSelectorReturnType returns null when no parameter matches the name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_NoMatchingParameter_ReturnsNull()
    {
        var parameters = RoslynSymbolProbe.MethodParameters(SelectorProbeSource, "TakesAnotherName");

        var result = ExtractorValidation.FindSelectorReturnType(parameters, SelectorName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that FindSelectorReturnType returns the return type when a matching parameter is found.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_MatchingParameter_ReturnsType()
    {
        var parameters = RoslynSymbolProbe.MethodParameters(SelectorProbeSource, "TakesSelector");

        var result = ExtractorValidation.FindSelectorReturnType(parameters, SelectorName);

        await Assert.That(result).IsEqualTo(StringName);
    }

    /// <summary>Verifies that FindSelectorReturnType matches any of multiple parameter names.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_MultipleNames_MatchesSecondName()
    {
        var parameters = RoslynSymbolProbe.MethodParameters(SelectorProbeSource, "TakesConversion");

        var result = ExtractorValidation.FindSelectorReturnType(parameters, SelectorName, "conversionFunc");

        await Assert.That(result).IsEqualTo(IntName);
    }

    /// <summary>Verifies that FindSelectorReturnType skips parameters with non-generic types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindSelectorReturnType_NonGenericType_ReturnsNull()
    {
        var parameters = RoslynSymbolProbe.MethodParameters(SelectorProbeSource, "TakesPlainType");

        var result = ExtractorValidation.FindSelectorReturnType(parameters, SelectorName);

        await Assert.That(result).IsNull();
    }

    /// <summary>
    /// An extension block read from source declares its members in a grouping type with no name at all,
    /// which is the other spelling of the shape metadata renders as <c>&lt;&gt;E__N</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_UnnamedGroupingInRecognizedClass_ReturnsTrue()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(
            UnnamedGroupingIn(nameof(ReactiveSchedulerExtensions)));

        await Assert.That(result).IsTrue();
    }

    /// <summary>Reaching the enclosing name of an unnamed grouping type does not make it recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_UnnamedGroupingInUnknownClass_ReturnsFalse()
    {
        var result = ExtractorValidation.IsRecognizedExtensionClass(UnnamedGroupingIn(UnknownClassName));

        await Assert.That(result).IsFalse();
    }

    /// <summary>A compiler-synthesized top-level type has no enclosing class to be recognized by.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsRecognizedExtensionClass_SynthesizedTopLevelType_ReturnsFalse()
    {
        var synthesized = CompiledType(
            """
            public static class Holder
            {
                public static readonly byte[] Data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            }
            """,
            "<PrivateImplementationDetails>");

        var result = ExtractorValidation.IsRecognizedExtensionClass(synthesized);

        await Assert.That(result).IsFalse();
    }

    /// <summary>A type with no type arguments and no element type, such as <c>dynamic</c>, contains no type parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContainsTypeParameter_DynamicType_ReturnsFalse()
    {
        var dynamicType = TestHelper.CreateCompilation(string.Empty).DynamicType;

        await Assert.That(ExtractorValidation.ContainsTypeParameter(dynamicType)).IsFalse();
    }

    /// <summary>
    /// Compiles a static class holding a closure and returns the display class the compiler synthesized inside
    /// it, which carries the same shape as the grouping type an extension block declares its members in: a name
    /// no C# identifier can spell, nested one level inside the class that names the API.
    /// </summary>
    /// <param name="className">The name to give the enclosing static class.</param>
    /// <returns>The synthesized nested type.</returns>
    /// <exception cref="InvalidOperationException">The compiler synthesized no nested type.</exception>
    private static INamedTypeSymbol SynthesizedNestedTypeIn(string className)
    {
        var outer = CompiledType($$"""
                                  public static class {{className}}
                                  {
                                      public static System.Func<int> Capture(int seed)
                                      {
                                          return () => seed;
                                      }
                                  }
                                  """);

        var nested = outer.GetTypeMembers();
        for (var i = 0; i < nested.Length; i++)
        {
            if (nested[i].Name.Length > 0 && nested[i].Name[0] == '<')
            {
                return nested[i];
            }
        }

        throw new InvalidOperationException(
            $"The compiler synthesized no nested type inside '{className}'.");
    }

    /// <summary>
    /// Compiles source to an image and reads the named type back out of it. Emitting matters: the types the
    /// compiler synthesizes exist only in the emitted assembly, not in the declaring compilation's symbols.
    /// </summary>
    /// <param name="source">The source declaring a single top-level type.</param>
    /// <param name="metadataName">The type to read back, or null for the type the source declares.</param>
    /// <returns>The named type symbol as a consumer sees it.</returns>
    /// <exception cref="InvalidOperationException">The source did not compile, or the type was not emitted.</exception>
    private static INamedTypeSymbol CompiledType(string source, string? metadataName = null)
    {
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);

        using var image = new MemoryStream();
        var emitResult = compilation.Emit(image);
        if (!emitResult.Success)
        {
            throw new InvalidOperationException(
                "The probe source failed to compile:" + Environment.NewLine
                + string.Join(
                    Environment.NewLine,
                    emitResult.Diagnostics
                        .Where(static d => d.Severity == DiagnosticSeverity.Error)
                        .Select(static d => $"  {d.Id}: {d.GetMessage()}")));
        }

        var reference = MetadataReference.CreateFromImage(image.ToArray());
        var consumer = TestHelper.CreateCompilation(string.Empty, LanguageVersion.CSharp10, false, "Consumer", [reference]);

        var typeName = metadataName ?? compilation.GetSymbolsWithName(
            static _ => true,
            SymbolFilter.Type).OfType<INamedTypeSymbol>().First().Name;

        return consumer.GetTypeByMetadataName(typeName)
            ?? throw new InvalidOperationException($"'{typeName}' was not found in the emitted image.");
    }

    /// <summary>
    /// Compiles an extension block and returns the grouping type the compiler declares its members in: a type
    /// with no name of its own, nested one level inside the class that names the API. Read from source rather
    /// than from an emitted image, which is the spelling that has no name at all.
    /// </summary>
    /// <param name="className">The name to give the enclosing static class.</param>
    /// <returns>The unnamed grouping type.</returns>
    /// <exception cref="InvalidOperationException">The compiler declared no unnamed nested type.</exception>
    private static INamedTypeSymbol UnnamedGroupingIn(string className)
    {
        var compilation = TestHelper.CreateCompilation(
            $$"""
              public static class {{className}}
              {
                  extension(int value)
                  {
                      public int Doubled => value * 2;
                  }
              }
              """,
            LanguageVersion.CSharp14);

        var outer = compilation.GetTypeByMetadataName(className)
            ?? throw new InvalidOperationException($"'{className}' did not compile.");

        var nested = outer.GetTypeMembers();
        for (var i = 0; i < nested.Length; i++)
        {
            if (nested[i].Name.Length == 0)
            {
                return nested[i];
            }
        }

        throw new InvalidOperationException(
            $"The compiler declared no unnamed grouping type inside '{className}'.");
    }
}
