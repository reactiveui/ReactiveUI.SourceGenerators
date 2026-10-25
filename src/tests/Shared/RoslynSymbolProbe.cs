// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveUI.Binding.Tests.Shared;

/// <summary>Resolves real symbols from a throwaway compilation, for helpers that take one directly.</summary>
/// <remarks>
/// The compiler's symbol interfaces cannot be implemented outside Roslyn itself, so a helper taking an
/// <see cref="ISymbol"/> is exercised by compiling source that produces the symbol wanted and handing the
/// real one over. That also keeps the tests honest: a shape source cannot express is a shape the generator
/// will never meet. The exotic ones still have a source form - a function pointer's signature belongs to no
/// type, and an array type to no namespace - which is how the guards against those are reached.
/// </remarks>
internal static class RoslynSymbolProbe
{
    /// <summary>The name given to the class every probe declares its members on.</summary>
    internal const string HolderTypeName = "Probe.Holder";

    /// <summary>Compiles probe source against the framework reference set.</summary>
    /// <param name="source">The source to compile.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation Compile(string source)
    {
        var references = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string paths
            ? paths.Split(System.IO.Path.PathSeparator)
                .Where(static p => p.EndsWith(".dll", StringComparison.Ordinal))
                .Select(static p => MetadataReference.CreateFromFile(p))
                .Cast<MetadataReference>()
                .ToImmutableArray()
            : ImmutableArray<MetadataReference>.Empty;

        return CSharpCompilation.Create(
            "SymbolProbe",
            [CSharpSyntaxTree.ParseText(source, new(LanguageVersion.CSharp14))],
            references,
            new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
    }

    /// <summary>Resolves the type of a field declared on the probe's holder class.</summary>
    /// <param name="source">The source declaring the holder class.</param>
    /// <param name="fieldName">The field whose type is wanted.</param>
    /// <returns>The field's type.</returns>
    /// <exception cref="InvalidOperationException">The holder class or the field is not in the source.</exception>
    internal static ITypeSymbol FieldType(string source, string fieldName) =>
        Holder(source).GetMembers(fieldName).OfType<IFieldSymbol>().FirstOrDefault()?.Type
        ?? throw new InvalidOperationException($"The probe declares no field named '{fieldName}'.");

    /// <summary>Resolves the parameters of a method declared on the probe's holder class.</summary>
    /// <param name="source">The source declaring the holder class.</param>
    /// <param name="methodName">The method whose parameters are wanted.</param>
    /// <returns>The method's parameters.</returns>
    /// <exception cref="InvalidOperationException">The holder class or the method is not in the source.</exception>
    internal static ImmutableArray<IParameterSymbol> MethodParameters(string source, string methodName) =>
        Holder(source).GetMembers(methodName).OfType<IMethodSymbol>().FirstOrDefault()?.Parameters
        ?? throw new InvalidOperationException($"The probe declares no method named '{methodName}'.");

    /// <summary>A method belonging to no type, which a function pointer's signature is and no declaration is.</summary>
    /// <returns>The method.</returns>
    /// <exception cref="InvalidOperationException">The probe did not produce a function pointer type.</exception>
    internal static IMethodSymbol MethodWithNoContainingType()
    {
        const string Source = """
                              namespace Probe
                              {
                                  public unsafe class Holder
                                  {
                                      public delegate*<int, string> Callback;
                                  }
                              }
                              """;

        return FieldType(Source, "Callback") is IFunctionPointerTypeSymbol pointer
            ? pointer.Signature
            : throw new InvalidOperationException("The probe did not produce a function pointer type.");
    }

    /// <summary>Resolves the probe's holder class.</summary>
    /// <param name="source">The source declaring it.</param>
    /// <returns>The holder class.</returns>
    /// <exception cref="InvalidOperationException">The holder class is not in the source.</exception>
    private static INamedTypeSymbol Holder(string source) =>
        Compile(source).GetTypeByMetadataName(HolderTypeName)
        ?? throw new InvalidOperationException($"The probe source declares no {HolderTypeName}.");
}
