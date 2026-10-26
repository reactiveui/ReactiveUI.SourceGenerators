// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Tests.Shared;

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>Tests for <see cref="AnalyzerHelpers"/>.</summary>
public class AnalyzerHelpersTests
{
    /// <summary>Verifies that ExtractFirstTypeArgument returns null when the method has no type arguments.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractFirstTypeArgument_NoTypeArguments_ReturnsNull()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class MyClass
                                  {
                                      public void NonGenericMethod() { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.ExtractFirstTypeArgument(methodSymbol);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that ExtractFirstTypeArgument returns the type when the method has a named type argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractFirstTypeArgument_WithNamedTypeArgument_ReturnsType()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class MyClass
                                  {
                                      public void GenericMethod<T>() { }

                                      public void Usage()
                                      {
                                          GenericMethod<string>();
                                      }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var invocation = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var methodSymbol = (IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!;

        var result = AnalyzerHelpers.ExtractFirstTypeArgument(methodSymbol);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Name).IsEqualTo("String");
    }

    /// <summary>Verifies that IsBindingExtensionMethod returns false when ContainingType is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_NullContainingType_ReturnsFalse()
    {
        var methodSymbol = RoslynSymbolProbe.MethodWithNoContainingType();

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that ExtractFirstTypeArgument returns null for a method that names no type arguments.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtractFirstTypeArgument_MethodNamesNoTypeArguments_ReturnsNull()
    {
        var methodSymbol = RoslynSymbolProbe.MethodWithNoContainingType();

        var result = AnalyzerHelpers.ExtractFirstTypeArgument(methodSymbol);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that IsBindingExtensionMethod returns false for a method outside the generated extension class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_UnrelatedMethod_ReturnsFalse()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class SomeClass
                                  {
                                      public void DoSomething() { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that IsBindingExtensionMethod returns true for a method on the generated extension class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_GeneratedClass_ReturnsTrue()
    {
        const string Source = """
                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static void WhenChanged() { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that IsBindingExtensionMethod returns true for a method on the stub extension class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsBindingExtensionMethod_StubClass_ReturnsTrue()
    {
        const string Source = """
                              namespace ReactiveUI.Binding
                              {
                                  public static class ReactiveUIBindingExtensions
                                  {
                                      public static void WhenChanged() { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var methodSymbol = model.GetDeclaredSymbol(methodDecl)!;

        var result = AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that IsInlineLambda returns true for a SimpleLambdaExpressionSyntax.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInlineLambda_SimpleLambda_ReturnsTrue()
    {
        var expr = SyntaxFactory.ParseExpression("x => x.Name");
        var result = AnalyzerHelpers.IsInlineLambda(expr);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that IsInlineLambda returns true for a ParenthesizedLambdaExpressionSyntax.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInlineLambda_ParenthesizedLambda_ReturnsTrue()
    {
        var expr = SyntaxFactory.ParseExpression("(x) => x.Name");
        var result = AnalyzerHelpers.IsInlineLambda(expr);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that IsInlineLambda returns false for a non-lambda expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsInlineLambda_Identifier_ReturnsFalse()
    {
        var expr = SyntaxFactory.ParseExpression("myVariable");
        var result = AnalyzerHelpers.IsInlineLambda(expr);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that ImplementsInterface returns false when the type does not implement the interface.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsInterface_DoesNotImplement_ReturnsFalse()
    {
        const string Source = """
                              using System.ComponentModel;
                              namespace TestApp
                              {
                                  public class PlainClass { }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var classDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First();

        var classSymbol = model.GetDeclaredSymbol(classDecl)!;
        var inpc = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged")!;

        var result = AnalyzerHelpers.ImplementsInterface(classSymbol, inpc);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that ImplementsInterface returns true when the type implements the interface.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsInterface_Implements_ReturnsTrue()
    {
        const string Source = """
                              using System.ComponentModel;
                              namespace TestApp
                              {
                                  public class ObservableClass : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var classDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First();

        var classSymbol = model.GetDeclaredSymbol(classDecl)!;
        var inpc = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged")!;

        var result = AnalyzerHelpers.ImplementsInterface(classSymbol, inpc);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that InheritsFrom returns false when the type does not inherit from the base type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InheritsFrom_DoesNotInherit_ReturnsFalse()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class BaseClass { }
                                  public class UnrelatedClass { }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var classDecls = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .ToArray();

        var baseSymbol = model.GetDeclaredSymbol(classDecls[0])!;
        var unrelatedSymbol = model.GetDeclaredSymbol(classDecls[1])!;

        var result = AnalyzerHelpers.InheritsFrom(unrelatedSymbol, baseSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that InheritsFrom returns true when the type inherits from the base type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InheritsFrom_Inherits_ReturnsTrue()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class BaseClass { }
                                  public class DerivedClass : BaseClass { }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var classDecls = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .ToArray();

        var baseSymbol = model.GetDeclaredSymbol(classDecls[0])!;
        var derivedSymbol = model.GetDeclaredSymbol(classDecls[1])!;

        var result = AnalyzerHelpers.InheritsFrom(derivedSymbol, baseSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that LacksObservableMechanism returns false for a non-generic method (no type arguments to extract).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksObservableMechanism_NonGenericMethod_ReturnsFalse()
    {
        var (methodSymbol, compilation) = GetNonGenericMethodSymbol();

        var result = AnalyzerHelpers.LacksObservableMechanism(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType).IsNull();
    }

    /// <summary>Verifies that LacksObservableMechanism returns false for a generic method whose first type argument implements INPC.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksObservableMechanism_GenericWithINPC_ReturnsFalse()
    {
        const string Source = """
                              using System.ComponentModel;
                              namespace TestApp
                              {
                                  public class MyVm : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                  }

                                  public class Caller
                                  {
                                      public void M<T>(T obj) where T : class { }
                                      public void Go() { M<MyVm>(new MyVm()); }
                                  }
                              }
                              """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(Source);

        var result = AnalyzerHelpers.LacksObservableMechanism(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType).IsNotNull();
    }

    /// <summary>Verifies that LacksObservableMechanism returns true when the first type argument has no observable mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksObservableMechanism_GenericWithPlainClass_ReturnsTrue()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class PlainVm { public string Name { get; set; } = ""; }

                                  public class Caller
                                  {
                                      public void M<T>(T obj) where T : class { }
                                      public void Go() { M<PlainVm>(new PlainVm()); }
                                  }
                              }
                              """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(Source);

        var result = AnalyzerHelpers.LacksObservableMechanism(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsTrue();
        await Assert.That(sourceType!.Name).IsEqualTo("PlainVm");
    }

    /// <summary>Verifies that LacksBeforeChangeSupport returns false for a non-generic method (no type arguments to extract).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksBeforeChangeSupport_NonGenericMethod_ReturnsFalse()
    {
        var (methodSymbol, compilation) = GetNonGenericMethodSymbol();

        var result =
            AnalyzerHelpers.LacksBeforeChangeSupport(
                methodSymbol,
                compilation,
                out var receiverType,
                out var mechanism);

        await Assert.That(result).IsFalse();
        await Assert.That(receiverType).IsNull();
        await Assert.That(mechanism).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies that LacksBeforeChangeSupport returns true when the first type argument implements only INPC.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LacksBeforeChangeSupport_GenericINPCOnly_ReturnsTrue()
    {
        const string Source = """
                              using System.ComponentModel;
                              namespace TestApp
                              {
                                  public class MyVm : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                  }

                                  public class Caller
                                  {
                                      public void M<T>(T obj) where T : class { }
                                      public void Go() { M<MyVm>(new MyVm()); }
                                  }
                              }
                              """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(Source);

        var result =
            AnalyzerHelpers.LacksBeforeChangeSupport(
                methodSymbol,
                compilation,
                out var receiverType,
                out var mechanism);

        await Assert.That(result).IsTrue();
        await Assert.That(receiverType!.Name).IsEqualTo("MyVm");
        await Assert.That(mechanism).Contains("INotifyPropertyChanged");
    }

    /// <summary>Verifies that ImplementsDataErrorInfo returns false for a non-generic method (no type arguments to extract).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_NonGenericMethod_ReturnsFalse()
    {
        var (methodSymbol, compilation) = GetNonGenericMethodSymbol();

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType).IsNull();
    }

    /// <summary>Verifies that ImplementsDataErrorInfo returns true when the first type argument implements INotifyDataErrorInfo.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_GenericWithDataErrorInfo_ReturnsTrue()
    {
        const string Source = """
                              using System;
                              using System.Collections;
                              using System.ComponentModel;
                              namespace TestApp
                              {
                                  public class MyVm : INotifyDataErrorInfo
                                  {
                                      public bool HasErrors { get { return false; } }
                                      public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
                                      public IEnumerable GetErrors(string propertyName) { return new object[0]; }
                                  }

                                  public class Caller
                                  {
                                      public void M<T>(T obj) where T : class { }
                                      public void Go() { M<MyVm>(new MyVm()); }
                                  }
                              }
                              """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(Source);

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsTrue();
        await Assert.That(sourceType!.Name).IsEqualTo("MyVm");
    }

    /// <summary>Verifies that ImplementsDataErrorInfo returns false when the first type argument does not implement INotifyDataErrorInfo.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_GenericWithoutDataErrorInfo_ReturnsFalse()
    {
        const string Source = """
                              using System.ComponentModel;
                              namespace TestApp
                              {
                                  public class MyVm : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                  }

                                  public class Caller
                                  {
                                      public void M<T>(T obj) where T : class { }
                                      public void Go() { M<MyVm>(new MyVm()); }
                                  }
                              }
                              """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbol(Source);

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType!.Name).IsEqualTo("MyVm");
    }

    /// <summary>Verifies that ImplementsDataErrorInfo returns false when the compilation has no INotifyDataErrorInfo type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ImplementsDataErrorInfo_TypeNotInCompilation_ReturnsFalse()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class MyVm { public string Name { get; set; } }

                                  public class Caller
                                  {
                                      public void M<T>(T obj) where T : class { }
                                      public void Go() { M<MyVm>(new MyVm()); }
                                  }
                              }
                              """;

        var (methodSymbol, compilation) = GetInvocationMethodSymbolMinimal(Source);

        var result = AnalyzerHelpers.ImplementsDataErrorInfo(methodSymbol, compilation, out var sourceType);

        await Assert.That(result).IsFalse();
        await Assert.That(sourceType!.Name).IsEqualTo("MyVm");
    }

    /// <summary>Verifies that HasObservableMechanism returns true when the type implements IReactiveObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasObservableMechanism_IReactiveObject_ReturnsTrue()
    {
        const string Source = """
                              using ReactiveUI;
                              namespace TestApp
                              {
                                  public class MyVm : IReactiveObject
                                  {
                                      public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
                                      public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
                                      public void RaisePropertyChanging(System.ComponentModel.PropertyChangingEventArgs args) { }
                                      public void RaisePropertyChanged(System.ComponentModel.PropertyChangedEventArgs args) { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "MyVm");

        var result = AnalyzerHelpers.HasObservableMechanism(typeSymbol, compilation);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that HasObservableMechanism handles a compilation with no IReactiveObject type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasObservableMechanism_IReactiveObjectNotInCompilation_ReturnsFalseForInterfacedType()
    {
        // Use a minimal compilation that does NOT reference ReactiveUI (no IReactiveObject)
        var (_, compilation) = GetInvocationMethodSymbolMinimal(
            """
            namespace TestApp
            {
                public interface IMyInterface { }
                public class MyVm : IMyInterface
                {
                    public string Name { get; set; }
                }

                public class Caller
                {
                    public void M<T>(T obj) where T : class { }
                    public void Go() { M<MyVm>(new MyVm()); }
                }
            }
            """);

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var classDecl = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(static c => c.Identifier.Text == "MyVm");

        var typeSymbol = model.GetDeclaredSymbol(classDecl)!;

        var result = AnalyzerHelpers.HasObservableMechanism(typeSymbol, compilation);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Gets a named type symbol from a compilation by class name.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="className">The class name to find.</param>
    /// <returns>The named type symbol.</returns>
    private static INamedTypeSymbol GetNamedTypeSymbol(CSharpCompilation compilation, string className)
    {
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == className);

        return model.GetDeclaredSymbol(classDecl)!;
    }

    /// <summary>Creates a compilation from the specified source code.</summary>
    /// <param name="source">The source code.</param>
    /// <returns>A CSharpCompilation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CSharpCompilation CreateCompilation(string source) =>
        AnalyzerTestHelper.CreateCompilation(source);

    /// <summary>Gets a non-generic method symbol and its compilation.</summary>
    /// <returns>The method symbol and compilation.</returns>
    private static MethodSymbolContext GetNonGenericMethodSymbol()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class MyClass
                                  {
                                      public void NonGenericMethod() { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var methodDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        return new(model.GetDeclaredSymbol(methodDecl)!, compilation);
    }

    /// <summary>Gets the resolved method symbol from the first invocation in the source.</summary>
    /// <param name="source">The source code containing an invocation.</param>
    /// <returns>The method symbol and compilation.</returns>
    private static MethodSymbolContext GetInvocationMethodSymbol(string source)
    {
        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        return new((IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!, compilation);
    }

    /// <summary>Gets the resolved method symbol from the first invocation in a compilation with only core references.</summary>
    /// <param name="source">The source code containing an invocation.</param>
    /// <returns>The method symbol and compilation.</returns>
    private static MethodSymbolContext GetInvocationMethodSymbolMinimal(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Only add the core runtime reference — no System.ComponentModel assemblies
        var references = new List<MetadataReference>();
        var addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddReference(System.Reflection.Assembly assembly)
        {
            if (assembly.IsDynamic || !addedPaths.Add(assembly.Location))
            {
                return;
            }

            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        AddReference(typeof(object).Assembly);

        // Add System.Runtime by name
        var systemRuntime = Array.Find(
            AppDomain.CurrentDomain.GetAssemblies(),
            static a => a.GetName().Name == "System.Runtime");
        if (systemRuntime is not null)
        {
            AddReference(systemRuntime);
        }

        var compilation = CSharpCompilation.Create(
            "MinimalTestAssembly",
            [syntaxTree],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));

        var tree = compilation.SyntaxTrees[0];
        var model = compilation.GetSemanticModel(tree);

        var invocation = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        return new((IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!, compilation);
    }
}
