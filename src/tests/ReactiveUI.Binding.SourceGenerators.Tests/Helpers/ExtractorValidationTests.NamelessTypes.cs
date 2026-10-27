// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Tests for the types generated code has no name for, and for calls whose only such type is the result.</summary>
public partial class ExtractorValidationTests
{
    /// <summary>The position of the call with an anonymous result among the statements of <c>Run</c>.</summary>
    private const int AnonymousResultCall = 0;

    /// <summary>The position of the call to a method with no selector.</summary>
    private const int NoSelectorCall = 1;

    /// <summary>The position of the call to a method of a generic class.</summary>
    private const int GenericClassCall = 2;

    /// <summary>The position of the call on a private nested receiver.</summary>
    private const int HiddenReceiverCall = 3;

    /// <summary>The position of the call whose result is the calling method's type parameter.</summary>
    private const int TypeParameterResultCall = 4;

    /// <summary>The position of <c>TRet</c> among the type parameters of <c>Api.Watch</c>.</summary>
    private const int WatchResultOrdinal = 2;

    /// <summary>Calls whose types range from all named to out of reach in different places, one per line of <c>Run</c>.</summary>
    private const string CallsSource = """
                                       using System;
                                       using System.Collections.Generic;
                                       using System.Linq;

                                       public static class Api
                                       {
                                           public static TRet Watch<TObj, T1, TRet>(this TObj o, Func<TObj, T1> p, Func<T1, TRet> selector) => default!;

                                           public static T1 NoSelector<TObj, T1>(this TObj o, Func<TObj, T1> p) => default!;
                                       }

                                       public static class Generic<TG>
                                       {
                                           public static TRet Watch<TObj, TRet>(TObj o, Func<TObj, TRet> selector) => default!;
                                       }

                                       public class Vm
                                       {
                                           public int A;
                                       }

                                       public static class Uses
                                       {
                                           public static void Run<TP>()
                                           {
                                               new Vm().Watch(x => x.A, a => new { a });
                                               new Vm().NoSelector(x => new { x.A });
                                               Generic<int>.Watch(new Vm(), v => new { v });
                                               new Hidden().Watch(x => 1, a => new { a });
                                               new Vm().Watch(x => x.A, a => default(TP));
                                               var enumerator = new[] { new { A = 1 } }.ToList().GetEnumerator();
                                               var plain = new List<string>();
                                           }

                                           private sealed class Hidden
                                           {
                                           }
                                       }

                                       file sealed class FileLocal
                                       {
                                           public sealed class Nested
                                           {
                                           }
                                       }
                                       """;

    /// <summary>An array of a type parameter is built from that type parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContainsTypeParameter_ArrayOfATypeParameter_ReturnsTrue()
    {
        var typeParameter = Call(TypeParameterResultCall).Method.TypeArguments[WatchResultOrdinal];
        var array = Compile().CreateArrayTypeSymbol(typeParameter);

        await Assert.That(ExtractorValidation.ContainsTypeParameter(array)).IsTrue();
    }

    /// <summary>A type with no type arguments and no element type, such as <c>dynamic</c>, is not nameless.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContainsNamelessType_DynamicType_ReturnsFalse() =>
        await Assert.That(ExtractorValidation.ContainsNamelessType(TestHelper.CreateCompilation(string.Empty).DynamicType)).IsFalse();

    /// <summary>A type nested in a generic closed over an anonymous type carries the anonymous type through its container.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContainsNamelessType_EnumeratorOfAListOfAnonymousType_ReturnsTrue() =>
        await Assert.That(ExtractorValidation.ContainsNamelessType(LocalType("enumerator"))).IsTrue();

    /// <summary>A generic closed over named types is not nameless.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContainsNamelessType_GenericOfNamedTypes_ReturnsFalse() =>
        await Assert.That(ExtractorValidation.ContainsNamelessType(LocalType("plain"))).IsFalse();

    /// <summary>A type nested in a file-local type has no name outside its file, like the file-local type itself.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContainsNamelessType_TypeNestedInAFileLocalType_ReturnsTrue()
    {
        var nested = Compile().GetSymbolsWithName("Nested", SymbolFilter.Type).OfType<INamedTypeSymbol>().Single();

        await Assert.That(ExtractorValidation.ContainsNamelessType(nested)).IsTrue();
    }

    /// <summary>An anonymous selector result is the one type argument a generic interceptor takes, at its position.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryFindUnnameableResult_AnonymousResult_FindsItsPosition()
    {
        var (method, compilation) = Call(AnonymousResultCall);

        await Assert.That(ExtractorValidation.TryFindUnnameableResult(method, compilation, out var ordinal)).IsTrue();
        await Assert.That(ordinal).IsEqualTo(WatchResultOrdinal);
    }

    /// <summary>A method with no selector has no result to take as a type parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryFindUnnameableResult_NoSelector_ReturnsFalse()
    {
        var (method, compilation) = Call(NoSelectorCall);

        await Assert.That(ExtractorValidation.TryFindUnnameableResult(method, compilation, out _)).IsFalse();
    }

    /// <summary>A method of a generic class would need the class's type parameters as well, so it is refused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryFindUnnameableResult_GenericContainingType_ReturnsFalse()
    {
        var (method, compilation) = Call(GenericClassCall);

        await Assert.That(ExtractorValidation.TryFindUnnameableResult(method, compilation, out _)).IsFalse();
    }

    /// <summary>A private type anywhere but the result has to be named, so the call is refused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryFindUnnameableResult_AnotherTypeArgumentOutOfReach_ReturnsFalse()
    {
        var (method, compilation) = Call(HiddenReceiverCall);

        await Assert.That(ExtractorValidation.TryFindUnnameableResult(method, compilation, out _)).IsFalse();
    }

    /// <summary>A result built from the calling code's type parameter is closed over by the interceptor, like any other result.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryFindUnnameableResult_ResultIsATypeParameter_ReturnsTrue()
    {
        var (method, compilation) = Call(TypeParameterResultCall);

        await Assert.That(ExtractorValidation.TryFindUnnameableResult(method, compilation, out _)).IsTrue();
    }

    /// <summary>Compiles the calls the tests read.</summary>
    /// <returns>The compilation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Compilation Compile() => TestHelper.CreateCompilation(CallsSource, LanguageVersion.CSharp11);

    /// <summary>Reads the type of a local declared in <c>Run</c>.</summary>
    /// <param name="name">The local's name.</param>
    /// <returns>The local's type.</returns>
    private static ITypeSymbol LocalType(string name)
    {
        var compilation = Compile();
        var tree = compilation.SyntaxTrees.First();
        var declarator = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().Single(d => d.Identifier.ValueText == name);
        return ((ILocalSymbol)compilation.GetSemanticModel(tree).GetDeclaredSymbol(declarator)!).Type;
    }

    /// <summary>Reads the method one of the calls in <c>Run</c> resolves to.</summary>
    /// <param name="index">The call's position among the statements of <c>Run</c>.</param>
    /// <returns>The resolved method and the compilation it belongs to.</returns>
    private static (IMethodSymbol Method, Compilation Compilation) Call(int index)
    {
        var compilation = Compile();
        var tree = compilation.SyntaxTrees.First();
        var statement = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static m => m.Identifier.ValueText == "Run")
            .Body!.Statements[index];
        var invocation = statement.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        return ((IMethodSymbol)compilation.GetSemanticModel(tree).GetSymbolInfo(invocation).Symbol!, compilation);
    }
}
