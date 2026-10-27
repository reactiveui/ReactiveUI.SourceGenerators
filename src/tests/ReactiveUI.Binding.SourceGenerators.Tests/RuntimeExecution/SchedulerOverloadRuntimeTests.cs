// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Covers the scheduler overloads reaching their generated dispatch. A call that carries a scheduler is still an
/// inline lambda at a known call site, so it has to be matched at compile time like any other; falling through to
/// the runtime stub turns a supported overload into a throw.
/// </summary>
public class SchedulerOverloadRuntimeTests
{
    /// <summary>What the scenario returns when the binding was established without throwing.</summary>
    private const string Completed = "ok";

    /// <summary>The dispatch file the BindOneWay call sites are generated into.</summary>
    private const string DispatchFileName = "BindOneWayDispatch.g.cs";

    /// <summary>The parameter a generated overload declares when the call site passes a sequencer.</summary>
    private const string SchedulerParameter = "scheduler";

    /// <summary>A one-way binding that passes a sequencer, the shape the runtime benchmarks drive.</summary>
    private const string SchedulerOverloadSource = """
                                                   using System;
                                                   using System.ComponentModel;
                                                   using ReactiveUI.Binding;
                                                   using ReactiveUI.Primitives.Concurrency;

                                                   namespace TestApp
                                                   {
                                                       public class MyViewModel : INotifyPropertyChanged
                                                       {
                                                           public event PropertyChangedEventHandler PropertyChanged;

                                                           public string Name { get; set; } = "seed";
                                                       }

                                                       public class MyView : INotifyPropertyChanged
                                                       {
                                                           public event PropertyChangedEventHandler PropertyChanged;

                                                           public string DisplayName { get; set; } = "";
                                                       }

                                                       public static class Usage
                                                       {
                                                           public static string Run()
                                                           {
                                                               var viewModel = new MyViewModel();
                                                               var view = new MyView();

                                                               using var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);

                                                               return "ok";
                                                           }
                                                       }
                                                   }
                                                   """;

    /// <summary>The consumer namespace used to reproduce a consumer whose own code sits under this library's.</summary>
    private const string NestedConsumerNamespace = "ReactiveUI.Binding.Consumer";

    /// <summary>
    /// A scheduler overload still dispatches for a consumer that references both runtime packages, which is what
    /// the benchmark project does. Both declare a scheduler extension class of the same name, so whichever the
    /// call resolves to has to be recognised.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithASchedulerAndBothRuntimesReferenced_DispatchesToGeneratedCode()
    {
        var reactiveRuntime = MetadataReference.CreateFromFile(
            typeof(ReactiveUI.Binding.Reactive.ReactiveUIBindingExtensions).Assembly.Location);

        var result = TestHelper.RunGenerator(
            SchedulerOverloadSource,
            LanguageVersion.CSharp10,
            null,
            false,
            [reactiveRuntime]);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, SchedulerParameter);
    }

    /// <summary>
    /// A scheduler overload still dispatches when the consumer's own code sits under this library's namespace.
    /// Extension lookup walks outward from the call site and stops at the first level offering a candidate, so a
    /// consumer nested here reaches the runtime stub before anything a global using could add.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithASchedulerFromANestedNamespace_DispatchesToGeneratedCode()
    {
        var source = SchedulerOverloadSource.Replace("namespace TestApp", $"namespace {NestedConsumerNamespace}");
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, NestedConsumerNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, SchedulerParameter);
    }

    /// <summary>
    /// A scheduler overload keeps its own generated overload when the same type pair is also bound without one
    /// from the same class. The scheduler call site differs from its neighbours only in the extra argument, so
    /// nothing about the property expressions distinguishes it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithASchedulerAlongsideUnscheduledCalls_KeepsItsOwnOverload()
    {
        const string scheduledCall =
            "using var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);";
        const string neighbouredCalls =
            "var plain = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);\n"
            + "            using var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);\n"
            + "            var another = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);";

        var source = SchedulerOverloadSource.Replace(scheduledCall, neighbouredCalls, StringComparison.Ordinal);
        await Assert.That(source).Contains("var plain =");

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, SchedulerParameter);
    }

    /// <summary>
    /// A scheduler overload dispatches on the language version consumers actually build with. The runtime
    /// library declares extension members in the current form, and how a call site resolves them can differ
    /// with the consumer's own language version.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithASchedulerOnTheLatestLanguageVersion_DispatchesToGeneratedCode()
    {
        var result = TestHelper.RunGenerator(SchedulerOverloadSource, LanguageVersion.CSharp14);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, SchedulerParameter);
    }

    /// <summary>A scheduler overload is dispatched to generated code rather than reaching the stub's throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithAScheduler_DispatchesToGeneratedCode()
    {
        var result = TestHelper.RunGenerator(SchedulerOverloadSource, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;

        string? outcome = null;
        Exception? thrown = null;
        try
        {
            outcome = (string?)run.Invoke(null, null);
        }
        catch (TargetInvocationException ex)
        {
            thrown = ex.InnerException;
        }

        context.Unload();

        await Assert.That(thrown).IsNull()
            .Because($"the scheduler overload should reach generated dispatch, but threw {thrown}");
        await Assert.That(outcome).IsEqualTo(Completed);
    }
}
