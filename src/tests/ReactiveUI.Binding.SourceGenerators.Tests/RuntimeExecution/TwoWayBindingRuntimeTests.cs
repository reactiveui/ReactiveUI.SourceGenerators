// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Covers what a two-way binding seeds, writes and reports. Both directions run through one pipeline, so the
/// order the initial values arrive in, the values the equality guard refuses, and what a subscriber to the
/// binding sees are all decided together.
/// </summary>
public class TwoWayBindingRuntimeTests
{
    /// <summary>The value the view produces once its intermediate exists.</summary>
    private const string ValueFromView = "edited";

    /// <summary>A view whose bound property sits behind an intermediate that is null when the binding is made.</summary>
    private const string LateIntermediateSource = """
                                                  using System;
                                                  using System.ComponentModel;
                                                  using ReactiveUI.Binding;

                                                  namespace TestApp
                                                  {
                                                      public class Inner : INotifyPropertyChanged
                                                      {
                                                          private string _text = "";

                                                          public event PropertyChangedEventHandler PropertyChanged;

                                                          public string Text
                                                          {
                                                              get { return _text; }
                                                              set
                                                              {
                                                                  _text = value;
                                                                  var handler = PropertyChanged;
                                                                  if (handler != null)
                                                                  {
                                                                      handler(this, new PropertyChangedEventArgs("Text"));
                                                                  }
                                                              }
                                                          }
                                                      }

                                                      public class MyViewModel : INotifyPropertyChanged
                                                      {
                                                          private string _name = "";

                                                          public event PropertyChangedEventHandler PropertyChanged;

                                                          public string Name
                                                          {
                                                              get { return _name; }
                                                              set
                                                              {
                                                                  _name = value;
                                                                  var handler = PropertyChanged;
                                                                  if (handler != null)
                                                                  {
                                                                      handler(this, new PropertyChangedEventArgs("Name"));
                                                                  }
                                                              }
                                                          }
                                                      }

                                                      public class MyView : INotifyPropertyChanged
                                                      {
                                                          private Inner _inner;

                                                          public event PropertyChangedEventHandler PropertyChanged;

                                                          public Inner Inner
                                                          {
                                                              get { return _inner; }
                                                              set
                                                              {
                                                                  _inner = value;
                                                                  var handler = PropertyChanged;
                                                                  if (handler != null)
                                                                  {
                                                                      handler(this, new PropertyChangedEventArgs("Inner"));
                                                                  }
                                                              }
                                                          }
                                                      }

                                                      public static class Usage
                                                      {
                                                          public static string Run()
                                                          {
                                                              var viewModel = new MyViewModel { Name = "start" };
                                                              var view = new MyView();

                                                              var binding = viewModel.BindTwoWay(view, x => x.Name, x => x.Inner.Text);

                                                              // The intermediate arrives already carrying the edit, so the chain's
                                                              // very first emission is the change itself.
                                                              view.Inner = new Inner { Text = "edited" };

                                                              return viewModel.Name;
                                                          }
                                                      }
                                                  }
                                                  """;

    /// <summary>A two-way binding whose change stream is subscribed to after it is made.</summary>
    private const string ReportedChangeSource = """
                                                using System;
                                                using System.Collections.Generic;
                                                using System.ComponentModel;
                                                using ReactiveUI.Binding;

                                                namespace TestApp
                                                {
                                                    public class MyViewModel : INotifyPropertyChanged
                                                    {
                                                        private string _name = "";

                                                        public event PropertyChangedEventHandler PropertyChanged;

                                                        public string Name
                                                        {
                                                            get { return _name; }
                                                            set
                                                            {
                                                                _name = value;
                                                                var handler = PropertyChanged;
                                                                if (handler != null)
                                                                {
                                                                    handler(this, new PropertyChangedEventArgs("Name"));
                                                                }
                                                            }
                                                        }
                                                    }

                                                    public class MyView : INotifyPropertyChanged, IViewFor<MyViewModel>
                                                    {
                                                        private string _text = "";

                                                        public event PropertyChangedEventHandler PropertyChanged;

                                                        public MyViewModel ViewModel { get; set; }

                                                        object IViewFor.ViewModel
                                                        {
                                                            get { return ViewModel; }
                                                            set { ViewModel = (MyViewModel)value; }
                                                        }

                                                        public string Text
                                                        {
                                                            get { return _text; }
                                                            set
                                                            {
                                                                _text = value;
                                                                var handler = PropertyChanged;
                                                                if (handler != null)
                                                                {
                                                                    handler(this, new PropertyChangedEventArgs("Text"));
                                                                }
                                                            }
                                                        }
                                                    }

                                                    public static class Usage
                                                    {
                                                        public static string Run()
                                                        {
                                                            var viewModel = new MyViewModel { Name = "start" };
                                                            var view = new MyView();
                                                            view.ViewModel = viewModel;

                                                            var binding = view.Bind(viewModel, x => x.Name, x => x.Text);

                                                            var reported = new List<string>();
                                                            using (binding.Changed.Subscribe(new Recorder(reported)))
                                                            {
                                                                viewModel.Name = "start";
                                                                viewModel.Name = "moved";
                                                            }

                                                            return string.Join(",", reported);
                                                        }
                                                    }

                                                    public class Recorder : IObserver<BindingChange>
                                                    {
                                                        private readonly List<string> _reported;

                                                        public Recorder(List<string> reported)
                                                        {
                                                            _reported = reported;
                                                        }

                                                        public void OnNext(BindingChange value)
                                                        {
                                                            _reported.Add(value.Value == null ? "null" : value.Value.ToString());
                                                        }

                                                        public void OnError(Exception error)
                                                        {
                                                        }

                                                        public void OnCompleted()
                                                        {
                                                        }
                                                    }
                                                }
                                                """;

    /// <summary>
    /// A target behind an intermediate that arrives after the binding still writes back. The target's own
    /// first value is weighed against what the source already put there rather than dropped by position, so
    /// nothing depends on how many values the chain happened to produce before the edit.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_WhenTheTargetsIntermediateArrivesLate_WritesTheEditBack() =>
        await Assert.That(await RunScenarioAsync(LateIntermediateSource)).IsEqualTo(ValueFromView);

    /// <summary>
    /// The change stream reports what the binding wrote, and only that. A value equal to the one already held
    /// is refused by the guard, so reporting it would tell a subscriber about a write that never happened.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SubscribingToChanged_ReportsOnlyTheValuesItWrote() =>
        await Assert.That(await RunScenarioAsync(ReportedChangeSource)).IsEqualTo("moved");

    /// <summary>Generates, compiles and runs a scenario, returning what its entry point reports.</summary>
    /// <param name="source">The scenario source.</param>
    /// <returns>The value the scenario's entry point returned.</returns>
    private static async Task<string?> RunScenarioAsync(string source)
    {
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;

        var outcome = (string?)run.Invoke(null, null);

        context.Unload();

        return outcome;
    }
}
