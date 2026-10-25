// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Tests for <see cref="TypeAnalyzer"/> over the APIs whose observed object is not their first type argument,
/// and their <c>Unsafe</c> twins: a stream's value type, and a target that is only written to, are never observed.
/// </summary>
public partial class TypeAnalyzerTests
{
    /// <summary>The stubs, and the view and view model types the calls under test are made with.</summary>
    private const string ObservedTypeArgumentModel = """
        using System;
        using System.ComponentModel;
        using System.Linq.Expressions;
        using System.Windows.Input;

        namespace ReactiveUI.Binding
        {
            public static class __ReactiveUIGeneratedBindings
            {
                public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
                    this IObservable<TValue> source,
                    TTarget target,
                    Expression<Func<TTarget, TTargetValue>> property)
                    where TTarget : class
                    => throw new NotImplementedException();

                public static IDisposable BindToUnsafe<TValue, TTarget, TTargetValue>(
                    this IObservable<TValue> source,
                    TTarget target,
                    Expression<Func<TTarget, TTargetValue>> property)
                    where TTarget : class
                    => throw new NotImplementedException();

                public static IDisposable BindToUnsafe<TValue, TTarget, TTargetValue>(
                    this IObservable<TValue> source,
                    TTarget target,
                    Expression<Func<TTarget, TTargetValue>> property,
                    object conversionHint)
                    where TTarget : class
                    => throw new NotImplementedException();

                public static IDisposable InvokeCommand<T, TTarget>(
                    this IObservable<T> source,
                    TTarget target,
                    Expression<Func<TTarget, ICommand>> commandProperty)
                    where TTarget : class
                    => throw new NotImplementedException();

                public static IDisposable InvokeCommandUnsafe<T, TTarget>(
                    this IObservable<T> source,
                    TTarget target,
                    Expression<Func<TTarget, ICommand>> commandProperty)
                    where TTarget : class
                    => throw new NotImplementedException();

                public static IDisposable OneWayBind<TViewModel, TView, TVMProp, TVProp>(
                    this TView view,
                    TViewModel viewModel,
                    Expression<Func<TViewModel, TVMProp>> viewModelProperty,
                    Expression<Func<TView, TVProp>> viewProperty)
                    where TViewModel : class
                    where TView : class
                    => throw new NotImplementedException();

                public static IDisposable OneWayBindUnsafe<TView, TViewModel, TVMProp, TVProp>(
                    this TView view,
                    TViewModel viewModel,
                    Expression<Func<TViewModel, TVMProp>> viewModelProperty,
                    Expression<Func<TView, TVProp>> viewProperty,
                    object scheduler)
                    where TView : class
                    where TViewModel : class
                    => throw new NotImplementedException();

                public static IDisposable OneWayBindUnsafe<TViewModel, TView, TVMProp, TVProp>(
                    this TView view,
                    TViewModel viewModel,
                    Expression<Func<TViewModel, TVMProp>> viewModelProperty,
                    Expression<Func<TView, TVProp>> viewProperty)
                    where TViewModel : class
                    where TView : class
                    => throw new NotImplementedException();

                public static IDisposable Bind<TSource, TOther>(
                    this TSource source,
                    TOther other,
                    Expression<Func<TSource, string>> property)
                    where TSource : class
                    where TOther : class
                    => throw new NotImplementedException();

                public static object WhenChangedUnsafe<TObj, TReturn>(
                    this TObj obj,
                    Expression<Func<TObj, TReturn>> property)
                    where TObj : class
                    => throw new NotImplementedException();
            }
        }

        namespace TestApp
        {
            public class PlainView
            {
                public decimal Amount { get; set; }

                public string Text { get; set; } = "";
            }

            public class PlainModel
            {
                public string Name { get; set; } = "";

                public ICommand Save { get; set; } = null!;
            }

            public class NotifyingModel : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string Name { get; set; } = "";

                public ICommand Save { get; set; } = null!;
            }

            public class Usage
            {
                public void Test(IObservable<decimal> values)
                {
                    var plainView = new PlainView();
                    var plainModel = new PlainModel();
                    var model = new NotifyingModel();
                    ReactiveUI.Binding.__ReactiveUIGeneratedBindings.__CALL__;
                }
            }
        }
        """;

    /// <summary>Where <see cref="ObservedTypeArgumentModel"/> takes the call under test.</summary>
    private const string ObservedTypeArgumentCallPlaceholder = "__CALL__";

    /// <summary>A stream's value type and a target that is only written to are not reported.</summary>
    /// <param name="call">The call made against the stubs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("BindTo(values, plainView, v => v.Amount)")]
    [Arguments("BindToUnsafe(values, plainView, v => v.Amount)")]
    [Arguments("BindToUnsafe(values, plainView, v => v.Amount, (object)null!)")]
    [Arguments("OneWayBind(plainView, model, x => x.Name, v => v.Text)")]
    [Arguments("OneWayBindUnsafe(plainView, model, x => x.Name, v => v.Text, (object)null!)")]
    [Arguments("OneWayBindUnsafe(plainView, model, x => x.Name, v => v.Text)")]
    [Arguments("InvokeCommand(values, model, x => x.Save)")]
    [Arguments("InvokeCommandUnsafe(values, model, x => x.Save)")]
    public async Task RXUIBIND002_ObjectNothingObserves_NoDiagnostic(string call)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(
            ObservedTypeArgumentModel.Replace(ObservedTypeArgumentCallPlaceholder, call));

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>The object that is observed is the one reported, whichever type argument names it.</summary>
    /// <param name="call">The call made against the stubs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("InvokeCommandUnsafe(values, plainModel, x => x.Save)")]
    [Arguments("OneWayBind(plainView, plainModel, x => x.Name, v => v.Text)")]
    [Arguments("OneWayBindUnsafe(plainView, plainModel, x => x.Name, v => v.Text, (object)null!)")]
    [Arguments("OneWayBindUnsafe(plainView, plainModel, x => x.Name, v => v.Text)")]
    [Arguments("Bind(plainModel, plainView, x => x.Name)")]
    [Arguments("WhenChangedUnsafe(plainModel, x => x.Name)")]
    public async Task RXUIBIND002_ObservedObjectThatDoesNotNotify_ReportsTheObservedType(string call)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(
            ObservedTypeArgumentModel.Replace(ObservedTypeArgumentCallPlaceholder, call));

        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NoObservablePropertiesDiagnosticId);
        await Assert.That(diagnostics[0].GetMessage()).Contains("PlainModel");
    }
}
