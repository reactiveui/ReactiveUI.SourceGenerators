// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers observation of a source type the consumer references rather than declares. A referenced type reaches
/// the generator only as a symbol, never as a class declaration, so detection that reads declarations alone
/// finds nothing for it and the call site silently loses its notification mechanism.
/// </summary>
public class ReferencedAssemblyObservationTests
{
    /// <summary>The dispatch file the WhenChanged call site is generated into.</summary>
    private const string DispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>The observable the generator emits when it knows the type raises PropertyChanged.</summary>
    private const string SubscribingObservable = "PropertyObservable";

    /// <summary>The observable the generator emits when it has no notification mechanism for the type.</summary>
    private const string SingleValueObservable = "ImmediateReturnSignal";

    /// <summary>A view model in another assembly, notifying exactly as one in the consumer's own would.</summary>
    private const string ReferencedAssemblySource = """
                                                    using System.ComponentModel;

                                                    namespace ExternalLib
                                                    {
                                                        public class ExternalViewModel : INotifyPropertyChanged
                                                        {
                                                            private string _name;

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
                                                    }
                                                    """;

    /// <summary>A consumer that observes the referenced view model and declares no view model of its own.</summary>
    private const string ConsumerSource = """
                                          using System;
                                          using ReactiveUI.Binding;

                                          namespace Consumer
                                          {
                                              public static class Usage
                                              {
                                                  public static IObservable<string> Observe(ExternalLib.ExternalViewModel viewModel)
                                                  {
                                                      return viewModel.WhenChanged(x => x.Name);
                                                  }
                                              }
                                          }
                                          """;

    /// <summary>The property a view-first binding walks when it follows the view's current view model.</summary>
    private const string ObservedViewModelProperty = "\"ViewModel\"";

    /// <summary>A view and its view model in another assembly, reachable only as symbols.</summary>
    private const string ReferencedViewSource = """
                                                using System.ComponentModel;
                                                using ReactiveUI.Binding;

                                                namespace ExternalUi
                                                {
                                                    public class ExternalViewModel : INotifyPropertyChanged
                                                    {
                                                        public event PropertyChangedEventHandler PropertyChanged;

                                                        public string Name { get; set; }

                                                        public Interaction<string, string> Confirm { get; set; } = new Interaction<string, string>();
                                                    }

                                                    public class ExternalView : INotifyPropertyChanged, IViewFor<ExternalViewModel>
                                                    {
                                                        private ExternalViewModel _viewModel;

                                                        public event PropertyChangedEventHandler PropertyChanged;

                                                        public string Text { get; set; }

                                                        public ExternalViewModel ViewModel
                                                        {
                                                            get { return _viewModel; }
                                                            set
                                                            {
                                                                _viewModel = value;
                                                                var handler = PropertyChanged;
                                                                if (handler != null)
                                                                {
                                                                    handler(this, new PropertyChangedEventArgs("ViewModel"));
                                                                }
                                                            }
                                                        }

                                                        object IViewFor.ViewModel
                                                        {
                                                            get { return ViewModel; }
                                                            set { ViewModel = (ExternalViewModel)value; }
                                                        }
                                                    }
                                                }
                                                """;

    /// <summary>A consumer binding a property against a view it references rather than declares.</summary>
    private const string ReferencedViewConsumerSource = """
                                                        using ReactiveUI.Binding;

                                                        namespace Consumer
                                                        {
                                                            public static class Usage
                                                            {
                                                                public static void Bind(ExternalUi.ExternalView view, ExternalUi.ExternalViewModel viewModel)
                                                                {
                                                                    view.OneWayBind(viewModel, x => x.Name, x => x.Text);
                                                                }
                                                            }
                                                        }
                                                        """;

    /// <summary>A consumer binding an interaction against a view it references rather than declares.</summary>
    private const string ReferencedInteractionConsumerSource = """
                                                               using System.Threading.Tasks;
                                                               using ReactiveUI.Binding;

                                                               namespace Consumer
                                                               {
                                                                   public static class Usage
                                                                   {
                                                                       public static void Bind(ExternalUi.ExternalView view, ExternalUi.ExternalViewModel viewModel)
                                                                       {
                                                                           view.BindInteraction(viewModel, x => x.Confirm, ctx =>
                                                                           {
                                                                               ctx.SetOutput("done");
                                                                               return Task.CompletedTask;
                                                                           });
                                                                       }
                                                                   }
                                                               }
                                                               """;

    /// <summary>A referenced INotifyPropertyChanged type is observed through its PropertyChanged event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnAReferencedType_SubscribesToPropertyChanged()
    {
        var result = RunAgainstReferencedViewModel();

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFileName, SubscribingObservable);
    }

    /// <summary>
    /// A referenced type does not degrade to a single value. That degradation is silent at the call site, and
    /// the single-value observable completes, which terminates every operator downstream of it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnAReferencedType_DoesNotDegradeToASingleValue()
    {
        var result = RunAgainstReferencedViewModel();

        await result.GeneratedSourceDoesNotContain(DispatchFileName, SingleValueObservable);
    }

    /// <summary>
    /// A binding on a referenced view still follows the view model the view holds. The view reaches the
    /// generator only as a symbol, so resolving its mechanism from the declaration scan alone would leave the
    /// binding holding whichever view model the call site was handed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_OnAReferencedView_FollowsTheViewModelTheViewHolds()
    {
        var result = RunAgainstReferencedView(ReferencedViewConsumerSource);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("OneWayBindDispatch.g.cs", ObservedViewModelProperty);
    }

    /// <summary>
    /// An interaction binding on a referenced view follows it too. This API takes no lambda rooted on the
    /// view, so the view's mechanism has to be carried by the call site rather than recovered from a path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_OnAReferencedView_FollowsTheViewModelTheViewHolds()
    {
        var result = RunAgainstReferencedView(ReferencedInteractionConsumerSource);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("BindInteractionDispatch.g.cs", ObservedViewModelProperty);
    }

    /// <summary>Runs the generator over a consumer whose view model lives in a referenced assembly.</summary>
    /// <returns>The generator result for the referenced-view-model scenario.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static GeneratorTestResult RunAgainstReferencedViewModel() =>
        TestHelper.RunGenerator(
            ConsumerSource,
            LanguageVersion.CSharp10,
            null,
            false,
            [TestHelper.CompileToReference(ReferencedAssemblySource, "ExternalLib", LanguageVersion.CSharp10)]);

    /// <summary>Runs the generator over a consumer binding against a view declared in a referenced assembly.</summary>
    /// <param name="consumerSource">The consumer source making the binding call.</param>
    /// <returns>The generator result for the referenced-view scenario.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static GeneratorTestResult RunAgainstReferencedView(string consumerSource) =>
        TestHelper.RunGenerator(
            consumerSource,
            LanguageVersion.CSharp10,
            null,
            false,
            [TestHelper.CompileToReference(ReferencedViewSource, "ExternalUi", LanguageVersion.CSharp10)]);
}
