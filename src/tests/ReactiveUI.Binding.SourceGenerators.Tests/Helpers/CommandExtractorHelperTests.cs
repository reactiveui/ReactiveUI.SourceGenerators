// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Tests for <see cref="CommandExtractor"/> internal helper methods.</summary>
public class CommandExtractorHelperTests
{
    /// <summary>The event name used by the explicit-event tests.</summary>
    private const string ClickEventName = "Click";

    /// <summary>Verifies that IsToEventArgument returns true for a named argument with "toEvent".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_NamedToEvent_ReturnsTrue()
    {
        const int SampleValue = 5;
        var argument = ParseFirstArgument("Method(toEvent: \"Click\")");

        var result = CommandExtractor.IsToEventArgument(argument, 0, SampleValue);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that IsToEventArgument returns false for a named argument with a different name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_NamedOtherParam_ReturnsFalse()
    {
        const int SampleValue = 5;
        var argument = ParseFirstArgument("Method(scheduler: null)");

        var result = CommandExtractor.IsToEventArgument(argument, 0, SampleValue);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that IsToEventArgument returns true for a positional argument at the correct index.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_PositionalAtCorrectIndex_ReturnsTrue()
    {
        const int SampleValue = 3;
        var argument = ParseFirstArgument("Method(\"Click\")");

        var result = CommandExtractor.IsToEventArgument(argument, SampleValue, SampleValue);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that IsToEventArgument returns false for a positional argument at the wrong index.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsToEventArgument_PositionalAtWrongIndex_ReturnsFalse()
    {
        const int SampleValue = 2;
        const int SampleValue2 = 5;
        var argument = ParseFirstArgument("Method(\"Click\")");

        var result = CommandExtractor.IsToEventArgument(argument, SampleValue, SampleValue2);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that HasCommandProperties returns false for a type with no Command property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_NoCommand_ReturnsFalse()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class PlainControl
                                  {
                                      public string Text { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsFalse();
        await Assert.That(hasParam).IsFalse();
    }

    /// <summary>Verifies that HasCommandProperties returns true for a type with a settable Command (ICommand) property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_WithCommand_ReturnsTrue()
    {
        const string source = """
                              using System.Windows.Input;
                              namespace TestApp
                              {
                                  public class ButtonControl
                                  {
                                      public ICommand Command { get; set; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsTrue();
        await Assert.That(hasParam).IsFalse();
    }

    /// <summary>Verifies that HasCommandProperties returns true with CommandParameter when both exist.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_WithCommandAndParameter_ReturnsTrueWithParam()
    {
        const string source = """
                              using System.Windows.Input;
                              namespace TestApp
                              {
                                  public class ButtonControl
                                  {
                                      public ICommand Command { get; set; }
                                      public object CommandParameter { get; set; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsTrue();
        await Assert.That(hasParam).IsTrue();
    }

    /// <summary>Verifies that HasCommandProperties does not match a readonly Command property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasCommandProperties_ReadOnlyCommand_ReturnsFalse()
    {
        const string source = """
                              using System.Windows.Input;
                              namespace TestApp
                              {
                                  public class ReadOnlyCommandControl
                                  {
                                      public ICommand Command { get; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.HasCommandProperties(classSymbol, out var hasParam);

        await Assert.That(result).IsFalse();
        await Assert.That(hasParam).IsFalse();
    }

    /// <summary>Verifies that HasEnabledProperty returns false when no Enabled property exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasEnabledProperty_NoEnabledProperty_ReturnsFalse()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class PlainControl
                                  {
                                      public string Text { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.EventEnabledBindingPlugin.HasEnabledProperty(classSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that HasEnabledProperty returns true when a settable bool Enabled property exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasEnabledProperty_WithEnabled_ReturnsTrue()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class WinFormsControl
                                  {
                                      public bool Enabled { get; set; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.EventEnabledBindingPlugin.HasEnabledProperty(classSymbol);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that HasEnabledProperty returns false for non-bool Enabled property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HasEnabledProperty_StringEnabled_ReturnsFalse()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class ControlWithStringEnabled
                                  {
                                      public string Enabled { get; set; } = "";
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var classSymbol = GetFirstClassSymbol(tree, model);

        var result = ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.EventEnabledBindingPlugin.HasEnabledProperty(classSymbol);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that FindParameterLambda returns null when there are only 3 arguments.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindParameterLambda_ThreeArgsOnly_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Vm { public string Name { get; set; } = ""; }
                                  public class View { public string Text { get; set; } = ""; }

                                  public class Caller
                                  {
                                      public void Go()
                                      {
                                          Method(new Vm(), new View(), "test");
                                      }

                                      public void Method(Vm vm, View view, string s) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var result =
            CommandExtractor.FindParameterLambda(invocation.ArgumentList.Arguments, model, CancellationToken.None);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that FindParameterLambda returns null when args after index 3 are not valid lambda expressions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindParameterLambda_NonLambdaArgs_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Vm { public string Name { get; set; } = ""; }
                                  public class View { public string Text { get; set; } = ""; }

                                  public class Caller
                                  {
                                      public void Go()
                                      {
                                          Method(new Vm(), new View(), "test", "not a lambda");
                                      }

                                      public void Method(Vm vm, View view, string s1, string s2) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var result =
            CommandExtractor.FindParameterLambda(invocation.ArgumentList.Arguments, model, CancellationToken.None);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that an empty toEvent string resolves to no event name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveExplicitEventName_EmptyString_ReturnsNull() =>
        await Assert.That(await ResolveToEventAsync("\"\"")).IsNull();

    /// <summary>Verifies that a toEvent argument with no compile-time value resolves to no event name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveExplicitEventName_NonConstant_ReturnsNull() =>
        await Assert.That(await ResolveToEventAsync("System.Environment.MachineName")).IsNull();

    /// <summary>Verifies that a non-empty constant toEvent string resolves to that event name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveExplicitEventName_ConstantString_ReturnsTheEventName() =>
        await Assert.That(await ResolveToEventAsync("\"Click\"")).IsEqualTo(ClickEventName);

    /// <summary>Verifies that a method with no toEvent parameter resolves no event name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveExplicitEventName_NoToEventParameter_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Caller
                                  {
                                      public void Go() => Method("cmd");

                                      public void Method(string command) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var invocation = await FirstInvocationAsync(tree);
        var methodSymbol = (Microsoft.CodeAnalysis.IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!;

        var result = CommandExtractor.ResolveExplicitEventName(
            methodSymbol,
            invocation.ArgumentList.Arguments,
            model,
            CancellationToken.None);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that the view and view model sides are read from the receiver and first argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveBindCommandSides_ReadsReceiverAndFirstArgument()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Vm { }
                                  public class View { public void Method(Vm vm) { } }

                                  public class Caller
                                  {
                                      public void Go() => new View().Method(new Vm());
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var invocation = await FirstInvocationAsync(tree);
        var memberAccess = (Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)invocation.Expression;

        var sides = CommandExtractor.ResolveBindCommandSides(
            memberAccess,
            invocation.ArgumentList.Arguments,
            model,
            ReachScope.Assembly(model.Compilation),
            CancellationToken.None);

        await Assert.That(sides!.Value.ViewTypeFullName).IsEqualTo("global::TestApp.View");
        await Assert.That(sides!.Value.ViewModelTypeFullName).IsEqualTo("global::TestApp.Vm");
    }

    /// <summary>Verifies that a receiver a generated overload cannot declare, such as a static class, resolves no sides.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveBindCommandSides_StaticReceiver_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Vm { }
                                  public static class Helper { public static void Method(Vm vm) { } }

                                  public class Caller
                                  {
                                      public void Go() => Helper.Method(new Vm());
                                  }
                              }
                              """;

        await Assert.That(await ResolveSidesAsync(source)).IsNull();
    }

    /// <summary>Verifies that a first argument with no type, such as a bare <c>null</c>, resolves no sides.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveBindCommandSides_UntypedFirstArgument_ReturnsNull()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Vm { }
                                  public class View { public void Method(Vm vm) { } }

                                  public class Caller
                                  {
                                      public void Go() => new View().Method(null);
                                  }
                              }
                              """;

        await Assert.That(await ResolveSidesAsync(source)).IsNull();
    }

    /// <summary>Verifies that the control binding resolves the explicit event, its args type and the capabilities.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveControlBinding_ExplicitEvent_ResolvesEventArgsAndCapabilities()
    {
        const string source = """
                              using System;
                              using System.Windows.Input;
                              namespace TestApp
                              {
                                  public class ButtonControl
                                  {
                                      public event EventHandler Click;
                                      public ICommand Command { get; set; }
                                      public object CommandParameter { get; set; }
                                      public bool Enabled { get; set; }
                                  }

                                  public class View { public ButtonControl Button { get; set; } }

                                  public class Caller
                                  {
                                      public void Go() => Method(v => v.Button, "Click");

                                      public void Method(System.Linq.Expressions.Expression<Func<View, ButtonControl>> controlName, string toEvent) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var invocation = await FirstInvocationAsync(tree);
        var methodSymbol = (Microsoft.CodeAnalysis.IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!;

        var (eventName, eventArgs, capabilities) = CommandExtractor.ResolveControlBinding(
            methodSymbol,
            invocation.ArgumentList.Arguments,
            invocation.ArgumentList.Arguments[0].Expression,
            model,
            CancellationToken.None);

        await Assert.That(eventName).IsEqualTo(ClickEventName);
        await Assert.That(eventArgs).IsNotNull();
        await Assert.That(capabilities.HasCommand).IsTrue();
        await Assert.That(capabilities.HasCommandParameter).IsTrue();
        await Assert.That(capabilities.HasEnabled).IsTrue();
    }

    /// <summary>Verifies that a settable ICommand-typed Command property is recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSettableICommandProperty_SettableCommand_ReturnsTrue() =>
        await Assert.That(ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.IsSettableICommandProperty(
            await PropertyAsync("public ICommand Command { get; set; }", "Command"))).IsTrue();

    /// <summary>Verifies that a read-only Command property is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSettableICommandProperty_ReadOnlyCommand_ReturnsFalse() =>
        await Assert.That(ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.IsSettableICommandProperty(
            await PropertyAsync("public ICommand Command { get; }", "Command"))).IsFalse();

    /// <summary>Verifies that a settable CommandParameter property is recognized.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSettableCommandParameterProperty_Settable_ReturnsTrue() =>
        await Assert.That(ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.IsSettableCommandParameterProperty(
            await PropertyAsync("public object CommandParameter { get; set; }", "CommandParameter"))).IsTrue();

    /// <summary>Verifies that a differently named property is not treated as the command parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsSettableCommandParameterProperty_OtherName_ReturnsFalse() =>
        await Assert.That(ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding.CommandPropertyBindingPlugin.IsSettableCommandParameterProperty(
            await PropertyAsync("public object Tag { get; set; }", "Tag"))).IsFalse();

    /// <summary>Verifies that a method with no withParameter reports neither overload shape.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectParameterOverload_NoWithParameter_ReportsNeitherShape()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class Caller
                                  {
                                      public void Go() => Method("cmd");

                                      public void Method(string command) { }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var invocation = await FirstInvocationAsync(tree);
        var methodSymbol = (Microsoft.CodeAnalysis.IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!;

        var result = CommandExtractor.DetectParameterOverload(
            methodSymbol,
            invocation.ArgumentList.Arguments,
            model,
            CancellationToken.None);

        await Assert.That(result.HasExpressionParameter).IsFalse();
        await Assert.That(result.HasObservableParameter).IsFalse();
        await Assert.That(result.ParameterTypeFullName).IsNull();
    }

    /// <summary>Verifies that a null control type resolves no event args type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveEventArgsTypeFullName_NullControlType_ReturnsNull()
    {
        var eventName = (string?)ClickEventName;

        await Assert.That(CommandExtractor.ResolveEventArgsTypeFullName(null, ref eventName)).IsNull();
    }

    /// <summary>Verifies that an explicit event name resolves that event's args type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveEventArgsTypeFullName_ExplicitEvent_ResolvesItsArgsType()
    {
        const string source = """
                              using System;
                              namespace TestApp
                              {
                                  public class ButtonControl { public event EventHandler Click; }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var eventName = (string?)ClickEventName;

        var result = CommandExtractor.ResolveEventArgsTypeFullName(GetFirstClassSymbol(tree, model), ref eventName);

        await Assert.That(result).IsNotNull();
        await Assert.That(eventName).IsEqualTo(ClickEventName);
    }

    /// <summary>Verifies that a null control type reports no capabilities.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectControlCapabilities_NullControlType_ReportsNone()
    {
        var result = CommandExtractor.DetectControlCapabilities(null);

        await Assert.That(result.HasCommand).IsFalse();
        await Assert.That(result.HasCommandParameter).IsFalse();
        await Assert.That(result.HasEnabled).IsFalse();
    }

    /// <summary>Verifies that a fully featured control reports every capability.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DetectControlCapabilities_FullControl_ReportsAll()
    {
        const string source = """
                              using System.Windows.Input;
                              namespace TestApp
                              {
                                  public class ButtonControl
                                  {
                                      public ICommand Command { get; set; }
                                      public object CommandParameter { get; set; }
                                      public bool Enabled { get; set; }
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var result = CommandExtractor.DetectControlCapabilities(GetFirstClassSymbol(tree, model));

        await Assert.That(result.HasCommand).IsTrue();
        await Assert.That(result.HasCommandParameter).IsTrue();
        await Assert.That(result.HasEnabled).IsTrue();
    }

    /// <summary>Compiles a control declaring the given member and returns one of its properties.</summary>
    /// <param name="memberDeclaration">The property declaration to place on the control.</param>
    /// <param name="propertyName">The name of the property to return.</param>
    /// <returns>The property symbol.</returns>
    private static async Task<Microsoft.CodeAnalysis.IPropertySymbol> PropertyAsync(
        string memberDeclaration,
        string propertyName)
    {
        var source = $$"""
                       using System.Windows.Input;
                       namespace TestApp
                       {
                           public class ButtonControl
                           {
                               {{memberDeclaration}}
                           }
                       }
                       """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        await Task.CompletedTask;

        return GetFirstClassSymbol(tree, model)
            .GetMembers(propertyName)
            .OfType<Microsoft.CodeAnalysis.IPropertySymbol>()
            .First();
    }

    /// <summary>Resolves the BindCommand sides of the first member-access invocation in the source.</summary>
    /// <param name="source">The source to compile.</param>
    /// <returns>The resolved sides, or null.</returns>
    private static async Task<BindCommandSides?> ResolveSidesAsync(string source)
    {
        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var invocation = await FirstInvocationAsync(tree);
        var memberAccess = (Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)invocation.Expression;

        return CommandExtractor.ResolveBindCommandSides(
            memberAccess,
            invocation.ArgumentList.Arguments,
            model,
            ReachScope.Assembly(model.Compilation),
            CancellationToken.None);
    }

    /// <summary>Returns the first invocation expression in a syntax tree.</summary>
    /// <param name="tree">The tree to search.</param>
    /// <returns>The first invocation expression.</returns>
    private static async Task<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax> FirstInvocationAsync(
        Microsoft.CodeAnalysis.SyntaxTree tree) =>
        (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

    /// <summary>Resolves the explicit event name for a call passing the given toEvent argument.</summary>
    /// <param name="toEventArgument">The argument expression to pass as <c>toEvent</c>.</param>
    /// <returns>The resolved event name, or null.</returns>
    private static async Task<string?> ResolveToEventAsync(string toEventArgument)
    {
        var source = $$"""
                       namespace TestApp
                       {
                           public class Caller
                           {
                               public void Go() => Method("cmd", {{toEventArgument}});

                               public void Method(string command, string toEvent) { }
                           }
                       }
                       """;

        var compilation = TestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var invocation = (await tree.GetRootAsync())
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .First();

        var methodSymbol = (Microsoft.CodeAnalysis.IMethodSymbol)model.GetSymbolInfo(invocation).Symbol!;

        return CommandExtractor.ResolveExplicitEventName(
            methodSymbol,
            invocation.ArgumentList.Arguments,
            model,
            CancellationToken.None);
    }

    /// <summary>Parses an invocation expression and returns the first argument.</summary>
    /// <param name="expression">The invocation expression text to parse.</param>
    /// <returns>The first argument syntax node.</returns>
    private static Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax ParseFirstArgument(string expression)
    {
        var parsed = SyntaxFactory.ParseExpression(expression);
        return ((Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)parsed).ArgumentList.Arguments[0];
    }

    /// <summary>Gets the first class symbol from a syntax tree.</summary>
    /// <param name="tree">The syntax tree.</param>
    /// <param name="model">The semantic model.</param>
    /// <returns>The first named type symbol.</returns>
    private static Microsoft.CodeAnalysis.INamedTypeSymbol GetFirstClassSymbol(
        Microsoft.CodeAnalysis.SyntaxTree tree,
        Microsoft.CodeAnalysis.SemanticModel model)
    {
        var classDecl = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First();

        return model.GetDeclaredSymbol(classDecl)!;
    }
}
