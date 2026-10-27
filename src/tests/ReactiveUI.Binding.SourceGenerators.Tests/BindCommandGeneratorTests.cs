// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for BindCommand invocation generation.</summary>
public class BindCommandGeneratorTests
{
    /// <summary>Compiles a binding for a command whose ICommand members are explicit and whose CanExecute name is a property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConcreteCommandWithExplicitICommandMembersCompiles()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Windows.Input;
            using ReactiveUI.Binding;

            public sealed class ExplicitCommand : ICommand
            {
                public bool CanExecute => true;
                event EventHandler ICommand.CanExecuteChanged { add { } remove { } }
                bool ICommand.CanExecute(object parameter) => true;
                void ICommand.Execute(object parameter) { }
            }

            public sealed class Model : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;
                public ExplicitCommand Command { get; } = new();
            }

            public sealed class Button
            {
                public event EventHandler Click;
                public bool Enabled { get; set; }
            }

            public sealed class View : IViewFor<Model>
            {
                public Model ViewModel { get; set; }
                object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (Model)value; }
                public Button Control { get; } = new();
            }

            public static class Usage
            {
                public static IDisposable Bind(View view, Model model) =>
                    view.BindCommand(model, x => x.Command, x => x.Control);
            }
            """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A view model passed as a bare <c>null</c> has no type to generate for, so the call stays on the stub.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UntypedNullViewModel_GeneratesNothing()
    {
        const string source = """
            using System;
            using System.ComponentModel;
            using System.Windows.Input;
            using ReactiveUI.Binding;

            public sealed class Model : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;
                public ICommand Command { get; set; }
            }

            public sealed class Button
            {
                public event EventHandler Click;
            }

            public sealed class View : IViewFor<Model>
            {
                public Model ViewModel { get; set; }
                object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (Model)value; }
                public Button Control { get; } = new();
            }

            public static class Usage
            {
                public static IDisposable Bind(View view) =>
                    view.BindCommand<View, Model, ICommand, Button>(null, x => x.Command, x => x.Control);
            }
            """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource("BindCommandDispatch.g.cs");
    }

    /// <summary>Verifies BindCommand with a basic button and no parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BasicNoParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/BasicNoParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindCommand with an IObservable parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/ObservableParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindCommand with an expression-based parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/ExpressionParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindCommand with a control that has no default event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoEvent()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/NoEvent");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand generates CallerFilePath-based dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/BasicNoParam");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindCommandGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand generates CallerFilePath-based dispatch for the ObservableParam scenario.
    /// Covers the CallerFilePath overload branch with HasObservableParameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_ObservableParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/ObservableParam");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindCommandGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand generates CallerFilePath-based dispatch for the ExpressionParam scenario.
    /// Covers the CallerFilePath overload branch with HasExpressionParameter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_ExpressionParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/ExpressionParam");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindCommandGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a deep command property path (Child.SaveCommand).
    /// Covers deep property chain observation in the BindCommand method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepCommandPath()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/DeepCommandPath");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with an explicit toEvent parameter.
    /// Covers the explicit event name resolution in the extraction helpers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CustomEvent()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/CustomEvent");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a control that has Command+CommandParameter properties (no param).
    /// Routes through CommandPropertyBindingPlugin — command-only variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/CommandProperty");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a Command property control and observable parameter.
    /// Routes through CommandPropertyBindingPlugin — observable parameter variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyObsParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/CommandPropertyObsParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a Command property control and expression parameter.
    /// Routes through CommandPropertyBindingPlugin — expression parameter variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyExprParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/CommandPropertyExprParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a Click+Enabled control (no Command property).
    /// Routes through EventEnabledBindingPlugin — no parameter variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabled()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/EventEnabled");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a Click+Enabled control and observable parameter.
    /// Routes through EventEnabledBindingPlugin — observable parameter variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledObsParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/EventEnabledObsParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindCommand with a Click+Enabled control and expression parameter.
    /// Routes through EventEnabledBindingPlugin — expression parameter variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledExprParam()
    {
        var source = SharedSourceReader.ReadScenario("BindCommand/EventEnabledExprParam");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindCommandGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}
