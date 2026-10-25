// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Generators;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests how the generated view dispatch registers itself with the view locator.</summary>
public class ViewDispatchRegistrationTests
{
    /// <summary>The hint name of the generated view dispatch file.</summary>
    private const string DispatchHintName = "ViewDispatch.g.cs";

    /// <summary>The attribute that marks a module initializer.</summary>
    private const string ModuleInitializerUse = "[global::System.Runtime.CompilerServices.ModuleInitializer]";

    /// <summary>
    /// The static constructor a consumer below C# 9 registers with. Such a consumer shares a namespace other assemblies
    /// may emit into, so its generated class carries the assembly's name.
    /// </summary>
    private const string SharedTierStaticConstructor = "static __ReactiveUIGeneratedBindings_TestAssembly()";

    /// <summary>The marker in the consumer template that a binding statement replaces.</summary>
    private const string BindingSlot = "//BINDING//";

    /// <summary>The consumer types the dispatch text refers to.</summary>
    private const string ConsumerViews = """
        public class TodoViewModel
        {
        }

        public class TodoView : global::ReactiveUI.Binding.IViewFor<TodoViewModel>
        {
            public TodoViewModel ViewModel { get; set; }

            object global::ReactiveUI.Binding.IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TodoViewModel)value;
            }
        }
        """;

    /// <summary>The consumer views and the code that resolves one, with an optional binding call ahead of it.</summary>
    private const string ConsumerTemplate = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        public class TodoViewModel : INotifyPropertyChanged
        {
            private string _title;

            public event PropertyChangedEventHandler PropertyChanged;

            public string Title
            {
                get => _title;
                set
                {
                    _title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        public class ViewBase<TViewModel> : IViewFor<TViewModel>
            where TViewModel : class
        {
            public TViewModel ViewModel { get; set; }

            object IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TViewModel)value;
            }
        }

        public class TodoView : ViewBase<TodoViewModel>
        {
        }

        public static class Usage
        {
            public static bool Run()
            {
                var viewModel = new TodoViewModel();
                //BINDING//
                var view = new DefaultViewLocator().ResolveView(viewModel, null);
                return view is TodoView && ReferenceEquals(view.ViewModel, viewModel);
            }
        }
        """;

    /// <summary>
    /// A view model with a default view and a contract view, and one with a default view only. A contract answers
    /// only with a view registered under it.
    /// </summary>
    private const string ContractConsumer = """
        using ReactiveUI.Binding;

        public class ViewBase<TViewModel> : IViewFor<TViewModel>
            where TViewModel : class
        {
            public TViewModel ViewModel { get; set; }

            object IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TViewModel)value;
            }
        }

        public class PanelViewModel
        {
        }

        public class PanelView : ViewBase<PanelViewModel>
        {
        }

        [ViewContract("ContractA")]
        public class PanelViewA : ViewBase<PanelViewModel>
        {
        }

        public class ToolViewModel
        {
        }

        public class ToolView : ViewBase<ToolViewModel>
        {
        }

        public static class Usage
        {
            public static bool Run()
            {
                var locator = new DefaultViewLocator();
                var panel = new PanelViewModel();
                var tool = new ToolViewModel();

                return locator.ResolveView(panel, null) is PanelView
                    && locator.ResolveView(panel, "") is PanelView
                    && locator.ResolveView(panel, "ContractA") is PanelViewA
                    && locator.ResolveView(panel, "ContractB") == null
                    && locator.ResolveView(tool, null) is ToolView
                    && locator.ResolveView(tool, "ContractB") == null;
            }
        }
        """;

    /// <summary>Verifies the dispatch is in place before any code in the assembly runs, with no binding call made.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ModuleInitializerRegistersDispatchBeforeAnyBindingRuns()
    {
        var result = TestHelper.RunGenerator(ConsumerTemplate.Replace(BindingSlot, string.Empty), LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchHintName, ModuleInitializerUse);
        await result.GeneratedSourceDoesNotContain(DispatchHintName, "__viewDispatchRegistered");
        await AssertUsageResolvesView(result);
    }

    /// <summary>Verifies a consumer older than C# 9 registers the dispatch when the generated class is first used.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StaticConstructorRegistersDispatchWhenGeneratedClassIsFirstUsed()
    {
        var source = ConsumerTemplate.Replace(BindingSlot, "var observed = viewModel.WhenChanged(x => x.Title);");
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp7_3);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchHintName, SharedTierStaticConstructor);
        await result.GeneratedSourceDoesNotContain(DispatchHintName, "ModuleInitializer");
        await AssertUsageResolvesView(result);
    }

    /// <summary>Verifies C# 9 is the first language version that registers as a module initializer.</summary>
    /// <param name="version">The language version the consumer compiles with.</param>
    /// <param name="usesModuleInitializer">Whether the dispatch registers as a module initializer.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(LanguageVersion.CSharp8, false)]
    [Arguments(LanguageVersion.CSharp9, true)]
    public async Task ModuleInitializerStartsAtCSharp9(LanguageVersion version, bool usesModuleInitializer)
    {
        var result = TestHelper.RunGenerator(ConsumerTemplate.Replace(BindingSlot, string.Empty), version);

        await result.CompilationSucceeds();

        var dispatch = result.GeneratedSources[DispatchHintName];
        await Assert.That(dispatch.Contains(ModuleInitializerUse, StringComparison.Ordinal)).IsEqualTo(usesModuleInitializer);
        await Assert.That(dispatch.Contains(SharedTierStaticConstructor, StringComparison.Ordinal)).IsEqualTo(!usesModuleInitializer);
    }

    /// <summary>
    /// Verifies a view with no contract answers only a request with no contract, both where it is the view model's
    /// only view and where a contract view sits beside it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultViewAnswersOnlyARequestWithNoContract()
    {
        var result = TestHelper.RunGenerator(ContractConsumer, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await AssertUsageResolvesView(result);
    }

    /// <summary>Verifies a framework without the module initializer attribute gets a file-local declaration of it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ModuleInitializerAttributeIsDeclaredWhenTheFrameworkLacksIt()
    {
        var registrations = new List<ViewRegistrationInfo> { new("global::TodoViewModel", "global::TodoView", true, null, false) };
        var features = new LanguageFeatures(
            SupportsCallerArgExpr: true,
            SupportsNullable: true,
            EmitGeneratedCodeMarkers: true,
            SupportsModuleInitializer: true,
            DeclaresModuleInitializerAttribute: true);
        var builder = new SourceWriter();

        ViewLocatorDispatchGenerator.GenerateSource(builder, registrations, features);
        var generated = builder.ToString();

        var compilation = TestHelper.CreateCompilation(
            $"{generated}\n{ConsumerViews}",
            TestHelper.ParseOptionsFor(LanguageVersion.CSharp11),
            false,
            "TestAssembly",
            []);
        var errors = compilation.GetDiagnostics().Where(static d => d.Severity == DiagnosticSeverity.Error).ToArray();

        await Assert.That(generated).Contains("file sealed class ModuleInitializerAttribute");
        await Assert.That(errors.Length).IsEqualTo(0);
    }

    /// <summary>Verifies a framework that ships the attribute has none declared for it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ModuleInitializerAttributeIsNotDeclaredWhenTheFrameworkHasIt()
    {
        var result = TestHelper.RunGenerator(ConsumerTemplate.Replace(BindingSlot, string.Empty), LanguageVersion.CSharp10);

        await result.GeneratedSourceDoesNotContain(DispatchHintName, "class ModuleInitializerAttribute");
    }

    /// <summary>Loads the generated consumer with a private runtime and checks its usage resolves the view.</summary>
    /// <param name="result">The generated consumer.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task AssertUsageResolvesView(GeneratorTestResult result)
    {
        var (assembly, context) = TestHelper.EmitAndLoad(result, true);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }
}
