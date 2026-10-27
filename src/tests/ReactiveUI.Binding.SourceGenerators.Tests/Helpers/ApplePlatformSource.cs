// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Builds compilation sources for the Apple key-value-observing path. The KVO observation code the generator
/// emits instantiates the runtime's key-value observing observable, which only the runtime's Apple heads declare,
/// so a scenario declares a stand-in for it alongside enough of <c>Foundation</c> for it to bind. A stub stands in
/// for the real framework so the scenarios run on every target, not just Apple ones.
/// </summary>
internal static class ApplePlatformSource
{
    /// <summary>The runtime observable generated KVO observation instantiates.</summary>
    internal const string KvoObservable = "global::ReactiveUI.Binding.Observables.KvoPropertyObservable<";

    /// <summary>
    /// The members of <c>Foundation</c> key-value observing binds against: the observer callback it overrides and the
    /// add/remove observer pair it subscribes through, with a stand-in for the runtime's Apple-head observable.
    /// </summary>
    private const string FoundationStub = """
                                          namespace Foundation
                                          {
                                              public sealed class ExportAttribute : Attribute
                                              {
                                                  public ExportAttribute(string selector) {}
                                              }
                                              public class NSString
                                              {
                                                  private readonly string _value;
                                                  public NSString(string value) { _value = value; }
                                                  public static explicit operator NSString(string value) => new NSString(value);
                                              }
                                              public class NSDictionary {}
                                              public enum NSKeyValueObservingOptions { New = 1, Old = 2 }
                                              public class NSObject
                                              {
                                                  public virtual void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context) {}
                                                  public void AddObserver(NSObject observer, NSString keyPath, NSKeyValueObservingOptions options, IntPtr context) {}
                                                  public void RemoveObserver(NSObject observer, NSString keyPath) {}
                                              }
                                          }

                                          namespace ReactiveUI.Binding.Observables
                                          {
                                              public sealed class KvoPropertyObservable<T> : IObservable<T>
                                              {
                                                  private readonly Foundation.NSObject _source;
                                                  private readonly Func<Foundation.NSObject, T> _getter;

                                                  public KvoPropertyObservable(Foundation.NSObject source, string keyPath, Func<Foundation.NSObject, T> getter, bool distinct, bool beforeChange)
                                                  {
                                                      _source = source;
                                                      _getter = getter;
                                                  }

                                                  public IDisposable Subscribe(IObserver<T> observer)
                                                  {
                                                      observer.OnNext(_getter(_source));
                                                      return ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;
                                                  }
                                              }
                                          }
                                          """;

    /// <summary>Builds a source that only declares an <c>NSObject</c>-derived view, with no binding call.</summary>
    /// <returns>The compilation source.</returns>
    internal static string TypeDetectionScenario() => $$"""
        using System;

        {{FoundationStub}}

        namespace TestApp
        {
            public class MyAppleView : Foundation.NSObject
            {
                [Foundation.Export("text")]
                public string Text { get; set; }
            }
        }
        """;

    /// <summary>Builds a source with an <c>NSObject</c>-derived view and a plain view model, bound by the given call.</summary>
    /// <param name="bindingCall">
    /// The binding expression, with <c>vm</c> and <c>view</c> in scope - for example
    /// <c>view.BindOneWay(vm, x =&gt; x.Text, x =&gt; x.Name)</c>.
    /// </param>
    /// <returns>The compilation source.</returns>
    internal static string BindingScenario(string bindingCall) => $$"""
        using System;
        using System.ComponentModel;

        using ReactiveUI.Binding;

        {{FoundationStub}}

        namespace TestApp
        {
            public class MyAppleView : Foundation.NSObject
            {
                [Foundation.Export("text")]
                public string Text { get; set; }
            }

            public class MyViewModel : INotifyPropertyChanged
            {
                private string _name = string.Empty;
                public event PropertyChangedEventHandler PropertyChanged;
                public string Name
                {
                    get => _name;
                    set
                    {
                        if (_name != value)
                        {
                            _name = value;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                        }
                    }
                }
            }

            public static class Scenario
            {
                public static IDisposable Execute(MyViewModel vm, MyAppleView view)
                    => {{bindingCall}};
            }
        }
        """;
}
