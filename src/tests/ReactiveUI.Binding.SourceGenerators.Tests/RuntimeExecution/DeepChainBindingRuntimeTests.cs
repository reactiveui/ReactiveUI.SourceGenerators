// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Covers binding through a property path whose intermediate is null. The read side of a chain already tolerates
/// a missing parent, so the binding is established and its first value delivered while the target chain cannot be
/// walked - which makes the write, not the subscription, the thing that has to give way.
/// </summary>
public class DeepChainBindingRuntimeTests
{
    /// <summary>What the scenario returns when the binding was established without throwing.</summary>
    private const string Completed = "ok";

    /// <summary>A view whose intermediate is never assigned, bound to from a view model that has a value.</summary>
    private const string NullIntermediateTargetSource = """
                                                        using System;
                                                        using System.ComponentModel;
                                                        using ReactiveUI.Binding;

                                                        namespace TestApp
                                                        {
                                                            public class Child : INotifyPropertyChanged
                                                            {
                                                                public event PropertyChangedEventHandler PropertyChanged;

                                                                public string Text { get; set; } = "";
                                                            }

                                                            public class MyViewModel : INotifyPropertyChanged
                                                            {
                                                                public event PropertyChangedEventHandler PropertyChanged;

                                                                public string Name { get; set; } = "seed";
                                                            }

                                                            public class MyView : INotifyPropertyChanged
                                                            {
                                                                public event PropertyChangedEventHandler PropertyChanged;

                                                                public Child Child { get; set; }
                                                            }

                                                            public static class Usage
                                                            {
                                                                public static string Run()
                                                                {
                                                                    var viewModel = new MyViewModel();
                                                                    var view = new MyView();

                                                                    var binding = viewModel.BindOneWay(view, x => x.Name, x => x.Child.Text);

                                                                    return "ok";
                                                                }
                                                            }
                                                        }
                                                        """;

    /// <summary>
    /// Binding into a chain whose intermediate is null drops the write rather than throwing. The seed value
    /// arrives while the target is unreachable, so an unguarded assignment fails inside the call that
    /// established the binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ThroughANullIntermediate_DoesNotThrow()
    {
        var result = TestHelper.RunGenerator(NullIntermediateTargetSource, LanguageVersion.CSharp10);
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
            .Because($"binding through a null intermediate should drop the write, but threw {thrown}");
        await Assert.That(outcome).IsEqualTo(Completed);
    }
}
