// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests for <see cref="SymbolHelpers"/> methods.</summary>
public class SymbolHelpersTests
{
    /// <summary>The interaction property selected in symbol tests.</summary>
    private const string InteractionPropertyName = "Confirm";

    /// <summary>Members of <c>MyVm</c> that hold an observable, or something that is not one.</summary>
    private const string ObservableMembersSource = """
                                                           public IObservable<int> ObservableField = null!;
                                                           public IObservable<int>[] ObservableArray { get; set; } = null!;
                                                           public string Name { get; set; } = "";
                                                           public IObservable<int> Method() => null!;
                                                   """;

    /// <summary>Types that resemble <c>IInteraction&lt;TInput, TOutput&gt;</c> without being it.</summary>
    private const string InteractionLookalikeSource = """
                                                      using System;
                                                      namespace Elsewhere
                                                      {
                                                          public interface IInteraction<TInput, TOutput>
                                                          {
                                                          }
                                                      }

                                                      namespace TestApp
                                                      {
                                                          public class MyVm
                                                          {
                                                              public Elsewhere.IInteraction<string, bool> Lookalike { get; set; }
                                                              public Tuple<string, bool> OtherPair { get; set; }
                                                              public Tuple<string> Single { get; set; }
                                                          }
                                                      }
                                                      """;

    /// <summary>A leaf segment whose type is returned when the observable cannot be unwrapped.</summary>
    private static readonly PropertyPathSegment FallbackSegment = new(
        "Leaf",
        "global::System.IObservable<int>",
        "global::TestApp.MyVm",
        true,
        null);

    /// <summary>Verifies GetWellKnownSymbols resolves INPC symbol from a compilation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetWellKnownSymbols_ResolvesINPC()
    {
        const string source = """
                              using System.ComponentModel;
                              namespace TestApp { public class Dummy {} }
                              """;

        var compilation = TestHelper.CreateCompilation(source);

        var symbols = SymbolHelpers.GetWellKnownSymbols(compilation);

        await Assert.That(symbols.INPC).IsNotNull();
        await Assert.That(symbols.INPC!.Name).IsEqualTo("INotifyPropertyChanged");
    }

    /// <summary>Verifies GetWellKnownSymbols returns cached instance for same compilation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetWellKnownSymbols_SameCompilation_ReturnsCachedInstance()
    {
        const string source = "namespace TestApp { public class Dummy {} }";

        var compilation = TestHelper.CreateCompilation(source);

        var symbols1 = SymbolHelpers.GetWellKnownSymbols(compilation);
        var symbols2 = SymbolHelpers.GetWellKnownSymbols(compilation);

        await Assert.That(ReferenceEquals(symbols1, symbols2)).IsTrue();
    }

    /// <summary>Verifies IsIObservable returns true for a direct IObservable&lt;T&gt; type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsIObservable_DirectIObservableType_ReturnsTrue()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class MyVm { public IObservable<string> Obs { get; set; } }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Obs").OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.IsIObservable((INamedTypeSymbol)prop.Type);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies IsIObservable returns false for a non-observable type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsIObservable_NonObservableType_ReturnsFalse()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class MyVm { public string Name { get; set; } = ""; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Name").OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.IsIObservable((INamedTypeSymbol)prop.Type);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies IsInteractionType returns true for a direct IInteraction&lt;TInput,TOutput&gt; type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInteractionType_DirectIInteractionType_ReturnsTrue()
    {
        const string source = """
                              using ReactiveUI.Binding;
                              namespace TestApp
                              {
                                  public class MyVm { public IInteraction<string, bool> Confirm { get; set; } }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers(InteractionPropertyName).OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.IsInteractionType((INamedTypeSymbol)prop.Type);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies IsInteractionType returns false for a non-interaction type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInteractionType_NonInteractionType_ReturnsFalse()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class MyVm { public string Name { get; set; } = ""; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Name").OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.IsInteractionType((INamedTypeSymbol)prop.Type);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies ExtractInteractionTypeArguments extracts the types of a property typed as <c>IInteraction&lt;TInput, TOutput&gt;</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInteractionTypeArguments_DirectIInteractionProperty_ReturnsTrueWithTypes()
    {
        const string source = """
                              using ReactiveUI.Binding;
                              namespace TestApp
                              {
                                  public class MyVm { public IInteraction<string, bool> Confirm { get; set; } }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers(InteractionPropertyName).OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.ExtractInteractionTypeArguments(
            prop.Type,
            out var inputType,
            out var outputType);

        await Assert.That(result).IsTrue();
        await Assert.That(inputType).IsEqualTo("string");
        await Assert.That(outputType).IsEqualTo("bool");
    }

    /// <summary>Extracts interaction arguments from the System.Reactive runtime flavor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInteractionTypeArguments_ReactiveFlavor_ReturnsTrueWithTypes()
    {
        const string source = """
            using ReactiveUI.Binding.Reactive;
            namespace TestApp
            {
                public class MyVm { public IInteraction<string, bool> Confirm { get; set; } }
            }
            """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10, true);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers(InteractionPropertyName).OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.ExtractInteractionTypeArguments(
            prop.Type,
            out var inputType,
            out var outputType);

        await Assert.That(result).IsTrue();
        await Assert.That(inputType).IsEqualTo("string");
        await Assert.That(outputType).IsEqualTo("bool");
    }

    /// <summary>Verifies ExtractInteractionTypeArguments returns false when the type is not an interaction type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInteractionTypeArguments_NonInteractionType_ReturnsFalse()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class MyVm { public string Name { get; set; } = ""; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");
        var prop = typeSymbol.GetMembers("Name").OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.ExtractInteractionTypeArguments(
            prop.Type,
            out var inputType,
            out var outputType);

        await Assert.That(result).IsFalse();
        await Assert.That(inputType).IsEqualTo(string.Empty);
        await Assert.That(outputType).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies ExtractInnerObservableType extracts the inner type from a property implementing IObservable&lt;T&gt; through an interface.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInnerObservableType_ViaInterfaceImplementation_ReturnsInnerType()
    {
        const string source = """
                              using System;
                              using System.Reactive.Subjects;
                              using System.Linq.Expressions;
                              namespace TestApp
                              {
                                  public class MyVm { public Subject<int> Count { get; set; } }
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyVm, Subject<int>>> expr = x => x.Count;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        var leafSegment = path![0];

        var innerType = SymbolHelpers.ExtractInnerObservableType(leafSegment, semanticModel, lambda, default);

        await Assert.That(innerType).IsEqualTo("int");
    }

    /// <summary>Verifies ExtractInnerObservableType falls back to the leaf segment type for a non-member-access lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInnerObservableType_NonLambdaArg_ReturnsFallbackType()
    {
        const string source = """
                              using System;
                              using System.Linq.Expressions;
                              namespace TestApp
                              {
                                  public class MyVm { public IObservable<string> Obs { get; set; } }
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyVm, IObservable<string>>> expr = x => x.Obs;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        var leafSegment = path![0];

        // Pass a non-lambda expression as argExpression to hit the fallback return
        var memberAccess = (ExpressionSyntax)(await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .First();

        var innerType = SymbolHelpers.ExtractInnerObservableType(leafSegment, semanticModel, memberAccess, default);

        // Fallback: should return the leaf segment's PropertyTypeFullName unchanged
        await Assert.That(innerType).IsEqualTo(leafSegment.PropertyTypeFullName);
    }

    /// <summary>Verifies DetectHasConverterOverride returns true when a parameter is typed as IBindingTypeConverter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectHasConverterOverride_WithIBindingTypeConverterParam_ReturnsTrue()
    {
        const string source = """
                              using ReactiveUI.Binding;
                              namespace TestApp
                              {
                                  public static class Ext
                                  {
                                      public static void Bind(IBindingTypeConverter converter) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = (await tree.GetRootAsync()).DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(static c => c.Identifier.Text == "Ext");
        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl)!;
        var methodSymbol = classSymbol.GetMembers("Bind").OfType<IMethodSymbol>().First();
        var converterParam = methodSymbol.Parameters[0];

        var result = SymbolHelpers.DetectHasConverterOverride(converterParam);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies DetectHasConverterOverride returns false when a parameter has a Func type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectHasConverterOverride_WithFuncParam_ReturnsFalse()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public static class Ext
                                  {
                                      public static void Bind(Func<string, int> converter) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = (await tree.GetRootAsync()).DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(static c => c.Identifier.Text == "Ext");
        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl)!;
        var methodSymbol = classSymbol.GetMembers("Bind").OfType<IMethodSymbol>().First();
        var converterParam = methodSymbol.Parameters[0];

        var result = SymbolHelpers.DetectHasConverterOverride(converterParam);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies ResolveNamedType returns null when the lambda has a block body.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNamedType_BlockBodyLambda_ReturnsNull()
    {
        const string source = """
                              using System;
                              using System.Linq.Expressions;
                              namespace TestApp
                              {
                                  public class MyVm { public string Name { get; set; } = ""; }
                                  public class Usage
                                  {
                                      public string Test(MyVm vm)
                                      {
                                          Func<MyVm, string> fn = (x) => { return x.Name; };
                                          return fn(vm);
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var blockLambda = (await tree.GetRootAsync()).DescendantNodes()
            .OfType<ParenthesizedLambdaExpressionSyntax>()
            .First();

        var result = SymbolHelpers.ResolveNamedType(semanticModel, blockLambda, default);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies ResolveNamedType returns null when the expression is not a lambda expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNamedType_NonLambdaExpression_ReturnsNull()
    {
        const string source = """
                              using System;
                              using System.Linq.Expressions;
                              namespace TestApp
                              {
                                  public class MyVm { public string Name { get; set; } = ""; }
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyVm, string>> expr = x => x.Name;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();

        // Pass the lambda BODY (a member access expression, not a lambda) as argExpression
        var result = SymbolHelpers.ResolveNamedType(semanticModel, (ExpressionSyntax)lambda.Body, default);

        await Assert.That(result).IsNull();
    }

    /// <summary>An observable held in a field is unwrapped like one held in a property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInnerObservableType_Field_ReturnsInnerType()
    {
        var (model, lambda) = LambdaIn(ObservableMembersSource, "x => x.ObservableField");

        var innerType = SymbolHelpers.ExtractInnerObservableType(FallbackSegment, model, lambda, default);

        await Assert.That(innerType).IsEqualTo("int");
    }

    /// <summary>A leaf that is not a property or field, or whose type is not a named observable, falls back to the leaf type.</summary>
    /// <param name="selector">The selector.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Method")]
    [Arguments("x => x.ObservableArray")]
    [Arguments("x => x.Name")]
    [Arguments("x => x.Name!")]
    [Arguments("x => null")]
    [Arguments("x => { return null; }")]
    public async Task ExtractInnerObservableType_NoObservableLeaf_ReturnsFallbackType(string selector)
    {
        var (model, lambda) = LambdaIn(ObservableMembersSource, selector);

        var innerType = SymbolHelpers.ExtractInnerObservableType(FallbackSegment, model, lambda, default);

        await Assert.That(innerType).IsEqualTo(FallbackSegment.PropertyTypeFullName);
    }

    /// <summary>A leaf field of a named type resolves to that type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNamedType_Field_ReturnsFieldType()
    {
        var (model, lambda) = LambdaIn(ObservableMembersSource, "x => x.ObservableField");

        var result = SymbolHelpers.ResolveNamedType(model, lambda, default);

        await Assert.That(result?.Name).IsEqualTo("IObservable");
    }

    /// <summary>A leaf that is not a member access, or not a named-type property or field, resolves to nothing.</summary>
    /// <param name="selector">The selector.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => null")]
    [Arguments("x => x.Method")]
    [Arguments("x => x.ObservableArray")]
    public async Task ResolveNamedType_NoNamedLeaf_ReturnsNull(string selector)
    {
        var (model, lambda) = LambdaIn(ObservableMembersSource, selector);

        var result = SymbolHelpers.ResolveNamedType(model, lambda, default);

        await Assert.That(result).IsNull();
    }

    /// <summary>A generic type that only resembles <c>IInteraction&lt;,&gt;</c> is not an interaction type.</summary>
    /// <param name="propertyName">The property whose type is checked.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Lookalike")]
    [Arguments("OtherPair")]
    [Arguments("Single")]
    public async Task IsInteractionType_Lookalike_ReturnsFalse(string propertyName)
    {
        var compilation = TestHelper.CreateCompilation(InteractionLookalikeSource, LanguageVersion.CSharp10);
        var prop = GetNamedTypeSymbol(compilation, "MyVm").GetMembers(propertyName).OfType<IPropertySymbol>().First();

        var result = SymbolHelpers.IsInteractionType((INamedTypeSymbol)prop.Type);

        await Assert.That(result).IsFalse();
    }

    /// <summary>A type that is not a named type carries no interaction type arguments.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractInteractionTypeArguments_ArrayType_ReturnsFalse()
    {
        var compilation = TestHelper.CreateCompilation(string.Empty);
        var arrayType = compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32));

        var result = SymbolHelpers.ExtractInteractionTypeArguments(arrayType, out var inputType, out var outputType);

        using (Assert.Multiple())
        {
            await Assert.That(result).IsFalse();
            await Assert.That(inputType).IsEqualTo(string.Empty);
            await Assert.That(outputType).IsEqualTo(string.Empty);
        }
    }

    /// <summary>Compiles a lambda over <c>MyVm</c> and returns it with its semantic model.</summary>
    /// <param name="members">The members of <c>MyVm</c>.</param>
    /// <param name="selector">The lambda text.</param>
    /// <returns>The semantic model and the lambda.</returns>
    private static (SemanticModel Model, LambdaExpressionSyntax Lambda) LambdaIn(string members, string selector)
    {
        var compilation = TestHelper.CreateCompilation(
            $$"""
              using System;
              namespace TestApp
              {
                  public class MyVm
                  {
              {{members}}
                  }

                  public static class Usage
                  {
                      public static void Take<T>(Func<MyVm, T> selector)
                      {
                      }

                      public static void Test() => Take<object>({{selector}});
                  }
              }
              """,
            LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var lambda = tree.GetRoot().DescendantNodes().OfType<LambdaExpressionSyntax>().First();
        return (compilation.GetSemanticModel(tree), lambda);
    }

    /// <summary>Gets a named type symbol from a compilation.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="typeName">The type name.</param>
    /// <returns>The named type symbol.</returns>
    private static INamedTypeSymbol GetNamedTypeSymbol(Compilation compilation, string typeName)
    {
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);
        var classDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == typeName);
        return semanticModel.GetDeclaredSymbol(classDecl)!;
    }
}
