// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers properties whose declaring type carries a notification mechanism that the property itself does not
/// participate in. A dependency object can declare a plain CLR property, and a component can declare one with
/// no change event, so the mechanism a type advertises does not settle how any one of its properties notifies.
/// </summary>
public class PropertyObservationCapabilityTests
{
    /// <summary>A dependency object whose property has no companion dependency-property field.</summary>
    private const string DependencyObjectClrPropertySource = """
                                                             using System;
                                                             using ReactiveUI.Binding;

                                                             namespace System.Windows
                                                             {
                                                                 public class DependencyObject { }
                                                             }

                                                             namespace Consumer
                                                             {
                                                                 public class MyControl : System.Windows.DependencyObject
                                                                 {
                                                                     public string Caption { get; set; }
                                                                 }

                                                                 public static class Usage
                                                                 {
                                                                     public static IObservable<string> Observe(MyControl control)
                                                                     {
                                                                         return control.WhenChanged(x => x.Caption);
                                                                     }
                                                                 }
                                                             }
                                                             """;

    /// <summary>A component whose property has no companion change event, but which does notify through INPC.</summary>
    private const string ComponentWithoutChangeEventSource = """
                                                             using System;
                                                             using System.ComponentModel;
                                                             using ReactiveUI.Binding;

                                                             namespace Consumer
                                                             {
                                                                 public class MyPanel : System.ComponentModel.Component, INotifyPropertyChanged
                                                                 {
                                                                     private string _caption;

                                                                     public event PropertyChangedEventHandler PropertyChanged;

                                                                     public string Caption
                                                                     {
                                                                         get { return _caption; }
                                                                         set
                                                                         {
                                                                             _caption = value;
                                                                             var handler = PropertyChanged;
                                                                             if (handler != null)
                                                                             {
                                                                                 handler(this, new PropertyChangedEventArgs("Caption"));
                                                                             }
                                                                         }
                                                                     }
                                                                 }

                                                                 public static class Usage
                                                                 {
                                                                     public static IObservable<string> Observe(MyPanel panel)
                                                                     {
                                                                         return panel.WhenChanged(x => x.Caption);
                                                                     }
                                                                 }
                                                             }
                                                             """;

    /// <summary>A dependency object observed through a property its base declares, not the type itself.</summary>
    private const string InheritedDependencyPropertySource = """
                                                             using System;
                                                             using ReactiveUI.Binding;

                                                             namespace System.Windows
                                                             {
                                                                 public class DependencyObject { }
                                                                 public class DependencyProperty { }
                                                             }

                                                             namespace Consumer
                                                             {
                                                                 public class BaseControl : System.Windows.DependencyObject
                                                                 {
                                                                     public static readonly System.Windows.DependencyProperty CaptionProperty = new System.Windows.DependencyProperty();

                                                                     public string Caption { get; set; }
                                                                 }

                                                                 public class DerivedControl : BaseControl
                                                                 {
                                                                 }

                                                                 public static class Usage
                                                                 {
                                                                     public static IObservable<string> Observe(DerivedControl control)
                                                                     {
                                                                         return control.WhenChanged(x => x.Caption);
                                                                     }
                                                                 }
                                                             }
                                                             """;

    /// <summary>A dependency object inheriting a plain CLR property from a base the consumer wrote.</summary>
    private const string InheritedClrPropertySource = """
                                                      using System;
                                                      using ReactiveUI.Binding;

                                                      namespace System.Windows
                                                      {
                                                          public class DependencyObject { }
                                                      }

                                                      namespace Consumer
                                                      {
                                                          public class BaseControl : System.Windows.DependencyObject
                                                          {
                                                              public string Caption { get; set; }
                                                          }

                                                          public class DerivedControl : BaseControl
                                                          {
                                                          }

                                                          public static class Usage
                                                          {
                                                              public static IObservable<string> Observe(DerivedControl control)
                                                              {
                                                                  return control.WhenChanged(x => x.Caption);
                                                              }
                                                          }
                                                      }
                                                      """;

    /// <summary>
    /// A plain property inherited from a base the consumer wrote does not take the dependency-property path.
    /// The base is readable, so whether the property has a companion field is a question with an answer, and
    /// emitting the field name anyway names one that was never declared.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnAnInheritedClrProperty_DoesNotTakeTheDependencyPropertyPath()
    {
        var result = TestHelper.RunGenerator(InheritedClrPropertySource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceDoesNotContain("WhenChangedDispatch.g.cs", "CaptionProperty");
    }

    /// <summary>
    /// An inherited property stays observable. The observed type lists only what it declares, so a property it
    /// inherits is unknown to that list rather than absent from the mechanism, and withdrawing observation over
    /// a question the list cannot answer would silently drop the subscription.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <remarks>
    /// Asserted on the emitted text rather than by compiling it: the dependency-property mechanism's output
    /// names a descriptor type that only a real WPF reference set carries, which this harness does not have.
    /// The assertion still pins the decision under test - that the mechanism was chosen for the property.
    /// </remarks>
    [Test]
    public async Task WhenChanged_OnAnInheritedDependencyProperty_StaysObservable()
    {
        var result = TestHelper.RunGenerator(InheritedDependencyPropertySource, LanguageVersion.CSharp10);

        await result.GeneratedSourceContains("WhenChangedDispatch.g.cs", "CaptionProperty");
    }

    /// <summary>
    /// A plain CLR property on a dependency object still generates code that compiles. The dependency-property
    /// field the mechanism reads does not exist for it, so naming one is a build error in the consumer's project.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnADependencyObjectClrProperty_Compiles()
    {
        var result = TestHelper.RunGenerator(DependencyObjectClrPropertySource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
    }

    /// <summary>
    /// A component property with no change event is observed through the notification interface it does
    /// implement. The component mechanism outranks that interface, so choosing on the type alone reaches for an
    /// event this property never declares.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnAComponentPropertyWithoutAChangeEvent_Compiles()
    {
        var result = TestHelper.RunGenerator(ComponentWithoutChangeEventSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
    }
}
