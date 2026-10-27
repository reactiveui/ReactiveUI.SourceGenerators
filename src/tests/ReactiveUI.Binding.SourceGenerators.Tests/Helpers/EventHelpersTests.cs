// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests for <see cref="EventHelpers"/> methods.</summary>
public class EventHelpersTests
{
    /// <summary>The <c>Click</c> name these tests generate against.</summary>
    private const string ClickName = "Click";

    /// <summary>The <c>MyButton</c> name these tests generate against.</summary>
    private const string MyButtonName = "MyButton";

    /// <summary>Verifies FindEventArgsType returns EventArgs when the event's delegate has a non-standard parameter count.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindEventArgsType_ActionDelegate_ReturnsEventArgs()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class MyButton { public event Action Clicked; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyButtonName);

        var result = EventHelpers.FindEventArgsType(typeSymbol, "Clicked");

        await Assert.That(result).IsEqualTo("global::System.EventArgs");
    }

    /// <summary>Verifies FindEventArgsType returns null when the type has no matching event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindEventArgsType_NoMatchingEvent_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class MyButton { }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyButtonName);

        var result = EventHelpers.FindEventArgsType(typeSymbol, ClickName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies FindDefaultEvent returns the first matching default event name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindDefaultEvent_ControlWithClickEvent_ReturnsClick()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class MyButton { public event EventHandler Click; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, MyButtonName);

        var eventName = EventHelpers.FindDefaultEvent(typeSymbol, out var argsType);

        await Assert.That(eventName).IsEqualTo(ClickName);
        await Assert.That(argsType).IsNotNull();
    }

    /// <summary>Verifies FindDefaultEvent returns TouchUpInside for controls with that event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindDefaultEvent_ControlWithTouchUpInsideEvent_ReturnsTouchUpInside()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class TouchControl { public event EventHandler TouchUpInside; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "TouchControl");

        var eventName = EventHelpers.FindDefaultEvent(typeSymbol, out _);

        await Assert.That(eventName).IsEqualTo("TouchUpInside");
    }

    /// <summary>Verifies FindDefaultEvent returns null when no default event matches.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindDefaultEvent_ControlWithNoMatchingEvent_ReturnsNull()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class PlainControl { public event EventHandler Hover; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10);
        var typeSymbol = GetNamedTypeSymbol(compilation, "PlainControl");

        var eventName = EventHelpers.FindDefaultEvent(typeSymbol, out var argsType);

        await Assert.That(eventName).IsNull();
        await Assert.That(argsType).IsNull();
    }

    /// <summary>Verifies FindEventArgsType returns null when the member named Click is a property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindEventArgsType_PropertyNamedClick_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class ControlWithClickProperty
                                  {
                                      public bool Click { get; set; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "ControlWithClickProperty");

        var result = EventHelpers.FindEventArgsType(typeSymbol, ClickName);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies FindEventArgsType returns null when the member named Click is a method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindEventArgsType_MethodNamedClick_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class ControlWithClickMethod
                                  {
                                      public void Click() { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var typeSymbol = GetNamedTypeSymbol(compilation, "ControlWithClickMethod");

        var result = EventHelpers.FindEventArgsType(typeSymbol, ClickName);

        await Assert.That(result).IsNull();
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
