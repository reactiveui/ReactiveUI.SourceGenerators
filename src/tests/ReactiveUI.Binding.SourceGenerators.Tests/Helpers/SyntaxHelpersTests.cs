// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests for <see cref="SyntaxHelpers"/> methods.</summary>
public class SyntaxHelpersTests
{
    /// <summary>Verifies ExtractPropertyPathFromLambda extracts a single property path from a lambda expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_SingleProperty_ExtractsPath()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyViewModel, string>> expr = x => x.Name;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Length).IsEqualTo(1);
        await Assert.That(path[0].PropertyName).IsEqualTo("Name");
    }

    /// <summary>Verifies ExtractPropertyPathFromLambda extracts a chained property path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_ChainedProperty_ExtractsFullPath()
    {
        const int Expected = 2;
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;

                              namespace TestApp
                              {
                                  public class Address
                                  {
                                      public string City { get; set; } = "";
                                  }

                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public Address Address { get; set; } = new();
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyViewModel, string>> expr = x => x.Address.City;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Length).IsEqualTo(Expected);
        await Assert.That(path[0].PropertyName).IsEqualTo("Address");
        await Assert.That(path[1].PropertyName).IsEqualTo("City");
    }

    /// <summary>Verifies ExtractPropertyPathFromLambda returns null for non-lambda expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_NonLambda_ReturnsNull()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var x = 42;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        // Pass a non-lambda expression (the literal 42)
        var literal = (await tree.GetRootAsync()).DescendantNodes().OfType<LiteralExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(literal, semanticModel, default);

        await Assert.That(path).IsNull();
    }

    /// <summary>Verifies GetLambdaBody returns the body for a simple lambda expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLambdaBody_SimpleLambda_ReturnsBody()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          System.Func<int, int> f = x => x;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        await Assert.That(body).IsNotNull();
        await Assert.That(body).IsTypeOf<IdentifierNameSyntax>();
    }

    /// <summary>Verifies GetLambdaBody returns the body for a parenthesized lambda expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLambdaBody_ParenthesizedLambda_ReturnsBody()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          System.Func<int, int, int> f = (x, y) => x + y;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        await Assert.That(body).IsNotNull();
        await Assert.That(body).IsTypeOf<BinaryExpressionSyntax>();
    }

    /// <summary>Verifies GetLambdaBody returns null for a parenthesized lambda with a block body.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLambdaBody_BlockBody_ReturnsNull()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          System.Func<int, int> f = (x) => { return x; };
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        await Assert.That(body).IsNull();
    }

    /// <summary>Verifies ExtractPropertyPathFromLambda works with parenthesized lambda expressions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_ParenthesizedLambda_ExtractsPath()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyViewModel, string>> expr = (x) => x.Name;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Length).IsEqualTo(1);
        await Assert.That(path[0].PropertyName).IsEqualTo("Name");
    }

    /// <summary>Verifies ExtractPropertyPathFromLambda declines a lambda with more than one parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_TwoParameters_ReturnsNull()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          Expression<Func<MyViewModel, MyViewModel, string>> expr = (x, y) => x.Name;
                                      }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, semanticModel, default);

        await Assert.That(path).IsNull();
    }

    /// <summary>A link that is a method, an event, a static or constant field, or a read-only field at the leaf is not a path.</summary>
    /// <param name="selector">The selector over <c>MyViewModel</c>. A parameter named after its type reads static members legally.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Compute")]
    [Arguments("x => x.Changed")]
    [Arguments("MyViewModel => MyViewModel.StaticField")]
    [Arguments("MyViewModel => MyViewModel.ConstantField")]
    [Arguments("x => x.ReadOnlyField")]
    public async Task ExtractPropertyPathFromLambda_UnreadableLink_ReturnsNull(string selector)
    {
        var (model, lambda) = LambdaIn($"public static void Test() => Take({selector});");

        await Assert.That(SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, model, default)).IsNull();
    }

    /// <summary>A path read off a type parameter is owned by the member's declaring type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractPropertyPathFromLambda_TypeParameterReceiver_UsesDeclaringType()
    {
        var (model, lambda) = LambdaIn(
            "public static void Test<T>() where T : MyViewModel => TakeFrom<T>(x => x.Name);");

        var path = SyntaxHelpers.ExtractPropertyPathFromLambda(lambda, model, default);

        await Assert.That(path).IsNotNull();
        await Assert.That(path![0].DeclaringTypeFullName).IsEqualTo("global::TestApp.MyViewModel");
    }

    /// <summary>A conditional call reports the line of the member binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerLineNumber_ConditionalCall_ReportsMemberLine()
    {
        const int ExpectedLine = 2;
        var invocation = InvocationIn("value\n    ?.Call()");

        await Assert.That(SyntaxHelpers.CallerLineNumber(invocation, default)).IsEqualTo(ExpectedLine);
    }

    /// <summary>A call through a plain name reports the line the call starts on.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerLineNumber_PlainCall_ReportsInvocationLine()
    {
        var invocation = InvocationIn("Call()");

        await Assert.That(SyntaxHelpers.CallerLineNumber(invocation, default)).IsEqualTo(1);
    }

    /// <summary>Parses an expression and returns the outermost invocation in it.</summary>
    /// <param name="expression">The expression text.</param>
    /// <returns>The invocation.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static InvocationExpressionSyntax InvocationIn(string expression) =>
        Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(expression)
            .DescendantNodesAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

    /// <summary>Compiles a usage method over <c>MyViewModel</c> and returns its first lambda with the semantic model.</summary>
    /// <param name="usage">The usage member, calling <c>Take</c> or <c>TakeFrom</c>.</param>
    /// <returns>The semantic model and the lambda.</returns>
    private static (Microsoft.CodeAnalysis.SemanticModel Model, LambdaExpressionSyntax Lambda) LambdaIn(string usage)
    {
        var compilation = TestHelper.CreateCompilation(
            $$"""
              using System;
              using System.ComponentModel;
              using System.Linq.Expressions;

              namespace TestApp
              {
                  public class MyViewModel : INotifyPropertyChanged
                  {
                      public const string ConstantField = "";
                      public static string StaticField = "";
                      public readonly string ReadOnlyField = "";

                      public event PropertyChangedEventHandler? PropertyChanged;

                      public event EventHandler? Changed;

                      public string Name { get; set; } = "";

                      public string Compute() => Name;
                  }

                  public static class Usage
                  {
                      public static void Take(Expression<Func<MyViewModel, object?>> selector)
                      {
                      }

                      public static void TakeFrom<T>(Expression<Func<T, object?>> selector)
                      {
                      }

                      {{usage}}
                  }
              }
              """);
        var tree = compilation.SyntaxTrees.First();
        var lambda = tree.GetRoot().DescendantNodes().OfType<LambdaExpressionSyntax>().First();
        return (compilation.GetSemanticModel(tree), lambda);
    }
}
