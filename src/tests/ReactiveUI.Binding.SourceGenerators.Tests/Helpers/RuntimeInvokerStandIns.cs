// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Source for the platform packages' view-thread invokers, for test compilations that declare a platform's types
/// themselves rather than referencing the platform package.
/// </summary>
/// <remarks>
/// Generated bindings route their writes through the invoker the platform package ships, and only reference it when
/// the consumer's compilation can see it. Each stand-in carries the same name and logic as the package's invoker,
/// written against the platform types a test declares, so a test observes what a consumer of the package would.
/// </remarks>
internal static class RuntimeInvokerStandIns
{
    /// <summary>The WPF package's invoker, over a declared <c>System.Windows.Threading.DispatcherObject</c>.</summary>
    internal const string Wpf = """

                                #nullable disable
                                namespace ReactiveUI.Binding.Wpf
                                {
                                    public sealed class DispatcherViewThreadInvoker : global::ReactiveUI.Binding.IViewThreadInvoker
                                    {
                                        public static DispatcherViewThreadInvoker Instance { get; } = new DispatcherViewThreadInvoker();

                                        public bool Claims(object target)
                                        {
                                            return target is global::System.Windows.Threading.DispatcherObject;
                                        }

                                        public bool CheckAccess(object target)
                                        {
                                            return ((global::System.Windows.Threading.DispatcherObject)target).CheckAccess();
                                        }

                                        public void Post(object target, global::System.Action<object> callback, object state)
                                        {
                                            var dispatcher = ((global::System.Windows.Threading.DispatcherObject)target).Dispatcher;
                                            if (dispatcher == null)
                                            {
                                                callback(state);
                                                return;
                                            }

                                            dispatcher.BeginInvoke(global::System.Windows.Threading.DispatcherPriority.Normal, callback, state);
                                        }
                                    }
                                }
                                #nullable restore
                                """;

    /// <summary>The WinForms package's invoker, over a declared <c>System.Windows.Forms.Control</c>.</summary>
    internal const string WinForms = """

                                     #nullable disable
                                     namespace ReactiveUI.Binding.WinForms
                                     {
                                         public sealed class ControlViewThreadInvoker : global::ReactiveUI.Binding.IViewThreadInvoker
                                         {
                                             public static ControlViewThreadInvoker Instance { get; } = new ControlViewThreadInvoker();

                                             public bool Claims(object target)
                                             {
                                                 return target is global::System.Windows.Forms.Control;
                                             }

                                             public bool CheckAccess(object target)
                                             {
                                                 return !((global::System.Windows.Forms.Control)target).InvokeRequired;
                                             }

                                             public void Post(object target, global::System.Action<object> callback, object state)
                                             {
                                                 var control = (global::System.Windows.Forms.Control)target;
                                                 if (!control.InvokeRequired)
                                                 {
                                                     callback(state);
                                                     return;
                                                 }

                                                 control.BeginInvoke(callback, new[] { state });
                                             }
                                         }
                                     }
                                     #nullable restore
                                     """;

    /// <summary>The MAUI package's invoker, over a declared <c>Microsoft.Maui.Controls.BindableObject</c>.</summary>
    internal const string Maui = """

                                 #nullable disable
                                 namespace ReactiveUI.Binding.Maui
                                 {
                                     public sealed class DispatcherViewThreadInvoker : global::ReactiveUI.Binding.IViewThreadInvoker
                                     {
                                         public static DispatcherViewThreadInvoker Instance { get; } = new DispatcherViewThreadInvoker();

                                         public bool Claims(object target)
                                         {
                                             return target is global::Microsoft.Maui.Controls.BindableObject;
                                         }

                                         public bool CheckAccess(object target)
                                         {
                                             var dispatcher = FindDispatcher((global::Microsoft.Maui.Controls.BindableObject)target);
                                             return dispatcher == null || !dispatcher.IsDispatchRequired;
                                         }

                                         public void Post(object target, global::System.Action<object> callback, object state)
                                         {
                                             var dispatcher = FindDispatcher((global::Microsoft.Maui.Controls.BindableObject)target);
                                             if (dispatcher == null)
                                             {
                                                 callback(state);
                                                 return;
                                             }

                                             dispatcher.Dispatch(() => callback(state));
                                         }

                                         private static global::Microsoft.Maui.Dispatching.IDispatcher FindDispatcher(global::Microsoft.Maui.Controls.BindableObject owner)
                                         {
                                             try
                                             {
                                                 return owner.Dispatcher;
                                             }
                                             catch (global::System.InvalidOperationException)
                                             {
                                                 return null;
                                             }
                                         }
                                     }
                                 }
                                 #nullable restore
                                 """;

    /// <summary>The Avalonia package's invoker, over a declared <c>Avalonia.AvaloniaObject</c>.</summary>
    /// <remarks>The package routes through the dispatcher's sequencer; the stand-in asks the object directly, which answers the same.</remarks>
    internal const string Avalonia = """

                                     #nullable disable
                                     namespace ReactiveUI.Binding.Avalonia
                                     {
                                         public sealed class AvaloniaViewThreadInvoker : global::ReactiveUI.Binding.IViewThreadInvoker
                                         {
                                             public static AvaloniaViewThreadInvoker Instance { get; } = new AvaloniaViewThreadInvoker();

                                             public bool Claims(object target)
                                             {
                                                 return target is global::Avalonia.AvaloniaObject;
                                             }

                                             public bool CheckAccess(object target)
                                             {
                                                 return ((global::Avalonia.AvaloniaObject)target).CheckAccess();
                                             }

                                             public void Post(object target, global::System.Action<object> callback, object state)
                                             {
                                                 ((global::Avalonia.AvaloniaObject)target).Dispatcher.Post(callback, state);
                                             }
                                         }
                                     }
                                     #nullable restore
                                     """;

    /// <summary>The Uno package's invoker, over a declared <c>Microsoft.UI.Xaml.DependencyObject</c>.</summary>
    /// <remarks>The package routes through the dispatcher queue's sequencer; the stand-in asks the queue directly.</remarks>
    internal const string Uno = """

                                #nullable disable
                                namespace ReactiveUI.Binding.Uno
                                {
                                    public sealed class UnoViewThreadInvoker : global::ReactiveUI.Binding.IViewThreadInvoker
                                    {
                                        public static UnoViewThreadInvoker Instance { get; } = new UnoViewThreadInvoker();

                                        public bool Claims(object target)
                                        {
                                            return target is global::Microsoft.UI.Xaml.DependencyObject;
                                        }

                                        public bool CheckAccess(object target)
                                        {
                                            return ((global::Microsoft.UI.Xaml.DependencyObject)target).DispatcherQueue.HasThreadAccess;
                                        }

                                        public void Post(object target, global::System.Action<object> callback, object state)
                                        {
                                            ((global::Microsoft.UI.Xaml.DependencyObject)target).DispatcherQueue.TryEnqueue(() => callback(state));
                                        }
                                    }
                                }
                                #nullable restore
                                """;
}
