// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Call sites whose receiver or target names no type a generated overload could declare: an array, a <c>null</c>
/// literal, or a type parameter. The generator leaves each on the runtime stub, and the consumer still compiles.
/// An interaction read through a field, which the path reader accepts, is covered here too.
/// </summary>
public class UndeclarableTypeInvocationTests
{
    /// <summary>The <c>BindOneWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindOneWayDispatchName = "BindOneWayDispatch.g.cs";

    /// <summary>The <c>BindToDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindToDispatchName = "BindToDispatch.g.cs";

    /// <summary>The <c>InvokeCommandDispatch.g.cs</c> name these tests generate against.</summary>
    private const string InvokeCommandDispatchName = "InvokeCommandDispatch.g.cs";

    /// <summary>The <c>BindInteractionDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindInteractionDispatchName = "BindInteractionDispatch.g.cs";

    /// <summary>The target type shared by the scenarios.</summary>
    private const string TargetSource = """
                                        public sealed class Target : INotifyPropertyChanged
                                        {
                                            public event PropertyChangedEventHandler? PropertyChanged;
                                            public int Count { get; set; }
                                            public string? Name { get; set; }
                                            public ICommand? Command { get; set; }
                                        }
                                        """;

    /// <summary>A one-way binding whose source is an array generates nothing, and the consumer still compiles.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ArraySource_GeneratesNothing()
    {
        const string usage = """
                             public static class Usage
                             {
                                 public static IDisposable Run(int[] source, Target target) =>
                                     source.BindOneWay(target, x => x.Length, x => x.Count);
                             }
                             """;

        var result = TestHelper.RunGenerator(Wrap(usage), LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchName);
    }

    /// <summary>A one-way binding onto a <c>null</c> literal target generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_NullLiteralTarget_GeneratesNothing()
    {
        const string usage = """
                             public static class Usage
                             {
                                 public static IDisposable Run(Target source) =>
                                     source.BindOneWay<Target, Target, int>(null!, x => x.Count, x => x.Count);
                             }
                             """;

        var result = TestHelper.RunGenerator(Wrap(usage), LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchName);
    }

    /// <summary>A BindTo onto a <c>null</c> literal target generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_NullLiteralTarget_GeneratesNothing()
    {
        const string usage = """
                             public static class Usage
                             {
                                 public static IDisposable Run(IObservable<string?> source) =>
                                     source.BindTo<string?, Target, string?>(null, x => x.Name);
                             }
                             """;

        var result = TestHelper.RunGenerator(Wrap(usage), LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindToDispatchName);
    }

    /// <summary>An InvokeCommand on a <c>null</c> literal target generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_NullLiteralTarget_GeneratesNothing()
    {
        const string usage = """
                             public static class Usage
                             {
                                 public static IDisposable Run(IObservable<string> source) =>
                                     source.InvokeCommand<string, Target>(null, x => x.Command);
                             }
                             """;

        var result = TestHelper.RunGenerator(Wrap(usage), LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchName);
    }

    /// <summary>A BindInteraction made on a type-parameter view, with the view type named explicitly, generates nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_TypeParameterView_GeneratesNothing()
    {
        const string usage = """
                             public sealed class Vm : INotifyPropertyChanged
                             {
                                 public event PropertyChangedEventHandler? PropertyChanged;
                                 public Interaction<string, bool> Confirm { get; } = new Interaction<string, bool>();
                             }

                             public static class Usage
                             {
                                 public static IDisposable Run<TView>(TView view, Vm vm)
                                     where TView : class, IViewFor =>
                                     view.BindInteraction<Vm, IViewFor, string, bool>(vm, x => x.Confirm, static _ => Task.CompletedTask);
                             }
                             """;

        var result = TestHelper.RunGenerator(Wrap(usage), LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchName);
    }

    /// <summary>A BindInteraction on an interaction held in a field generates its dispatch rather than failing the generator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_FieldInteraction_Generates()
    {
        const string usage = """
                             public sealed class Vm : INotifyPropertyChanged
                             {
                                 public Interaction<string, bool> Confirm = new Interaction<string, bool>();
                                 public event PropertyChangedEventHandler? PropertyChanged;
                             }

                             public sealed class View : IViewFor
                             {
                                 public object? ViewModel { get; set; }
                             }

                             public static class Usage
                             {
                                 public static IDisposable Run(View view, Vm vm) =>
                                     view.BindInteraction(vm, x => x.Confirm, static _ => Task.CompletedTask);
                             }
                             """;

        var result = TestHelper.RunGenerator(Wrap(usage), LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(BindInteractionDispatchName, "global::ReactiveUI.Binding.IInteraction<string, bool>");
    }

    /// <summary>Places a usage and the shared target in one compilation unit.</summary>
    /// <param name="usage">The usage declarations.</param>
    /// <returns>The compilation unit source.</returns>
    private static string Wrap(string usage) =>
        $$"""
          using System;
          using System.ComponentModel;
          using System.Threading.Tasks;
          using System.Windows.Input;
          using ReactiveUI.Binding;

          namespace TestApp
          {
          {{TargetSource}}

          {{usage}}
          }
          """;
}
