// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/>.</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>
    /// Common using directives and the generated stub class that the analyzer recognizes.
    /// The analyzer checks for methods on a class named __ReactiveUIGeneratedBindings.
    /// </summary>
    private const string Preamble = """
                                    using System;
                                    using System.Collections;
                                    using System.ComponentModel;
                                    using System.Linq.Expressions;

                                    namespace ReactiveUI.Binding
                                    {
                                        public static class __ReactiveUIGeneratedBindings
                                        {
                                            public static object WhenChanged<TObj, TReturn>(
                                                this TObj obj,
                                                Expression<Func<TObj, TReturn>> property)
                                                where TObj : class
                                                => throw new NotImplementedException();

                                            public static object WhenChanging<TObj, TReturn>(
                                                this TObj obj,
                                                Expression<Func<TObj, TReturn>> property)
                                                where TObj : class
                                                => throw new NotImplementedException();

                                            public static object BindOneWay<TSource, TTarget, TValue>(
                                                this TSource source,
                                                TTarget target,
                                                Expression<Func<TSource, TValue>> sourceProperty,
                                                Expression<Func<TTarget, TValue>> targetProperty)
                                                where TSource : class
                                                where TTarget : class
                                                => throw new NotImplementedException();

                                            public static object BindTwoWay<TSource, TTarget, TValue>(
                                                this TSource source,
                                                TTarget target,
                                                Expression<Func<TSource, TValue>> sourceProperty,
                                                Expression<Func<TTarget, TValue>> targetProperty)
                                                where TSource : class
                                                where TTarget : class
                                                => throw new NotImplementedException();

                                            public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
                                                this IObservable<TValue> source,
                                                TTarget target,
                                                Expression<Func<TTarget, TTargetValue>> property)
                                                where TTarget : class
                                                => throw new NotImplementedException();
                                        }
                                    }
                                    """;

    /// <summary>Preamble with BindInteraction and BindCommand stub methods.</summary>
    private const string InteractionCommandPreamble = """
                                                      using System;
                                                      using System.ComponentModel;
                                                      using System.Linq.Expressions;
                                                      using System.Threading.Tasks;
                                                      using System.Windows.Input;

                                                      namespace ReactiveUI.Binding
                                                      {
                                                          public interface IViewFor { object? ViewModel { get; set; } }
                                                          public interface IInteraction<TInput, TOutput> { }
                                                          public interface IInteractionContext<TInput, TOutput> { }

                                                          public static class ReactiveUIBindingExtensions
                                                          {
                                                              public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput>(
                                                                  this TView view,
                                                                  TViewModel? viewModel,
                                                                  Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
                                                                  Func<IInteractionContext<TInput, TOutput>, Task> handler)
                                                                  where TViewModel : class
                                                                  where TView : class, IViewFor
                                                                  => throw new NotImplementedException();

                                                              // Unconstrained overload for analyzer testing of RXUIBIND008
                                                              public static IDisposable BindInteraction<TViewModel, TView, TProperty>(
                                                                  this TView view,
                                                                  TViewModel? viewModel,
                                                                  Expression<Func<TViewModel, TProperty>> propertyName,
                                                                  Func<object, Task> handler)
                                                                  where TViewModel : class
                                                                  where TView : class, IViewFor
                                                                  => throw new NotImplementedException();

                                                              public static IDisposable BindCommand<TView, TViewModel, TProp, TControl>(
                                                                  this TView view,
                                                                  TViewModel? viewModel,
                                                                  Expression<Func<TViewModel, TProp?>> propertyName,
                                                                  Expression<Func<TView, TControl>> controlName,
                                                                  string? toEvent = null)
                                                                  where TView : class, IViewFor
                                                                  where TViewModel : class
                                                                  where TProp : ICommand
                                                                  where TControl : class
                                                                  => throw new NotImplementedException();
                                                          }
                                                      }
                                                      """;
}
