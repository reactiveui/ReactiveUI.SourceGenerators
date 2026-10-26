// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Constants shared between the source generator and analyzer projects.
/// Contains well-known metadata names for platform detection and API stub definitions.
/// </summary>
internal static class Constants
{
    /// <summary>Metadata name for <see cref="System.ComponentModel.INotifyPropertyChanged"/>.</summary>
    internal const string INotifyPropertyChangedMetadataName = "System.ComponentModel.INotifyPropertyChanged";

    /// <summary>Metadata name for <see cref="System.ComponentModel.INotifyPropertyChanging"/>.</summary>
    internal const string INotifyPropertyChangingMetadataName = "System.ComponentModel.INotifyPropertyChanging";

    /// <summary>Metadata name for ReactiveUI's <c>IReactiveObject</c> interface.</summary>
    internal const string IReactiveObjectMetadataName = "ReactiveUI.IReactiveObject";

    /// <summary>Metadata name for the class declaring ReactiveUI's public <c>RaisePropertyChanged</c> extension.</summary>
    internal const string IReactiveObjectExtensionsMetadataName = "ReactiveUI.IReactiveObjectExtensions";

    /// <summary>Metadata name for the same class in ReactiveUI's System.Reactive flavour.</summary>
    internal const string ReactiveIReactiveObjectExtensionsMetadataName = "ReactiveUI.Reactive.IReactiveObjectExtensions";

    /// <summary>Metadata name for the attribute that ranks one applicable overload above another (C# 13).</summary>
    internal const string OverloadResolutionPriorityAttributeMetadataName =
        "System.Runtime.CompilerServices.OverloadResolutionPriorityAttribute";

    /// <summary>Metadata name for <see cref="System.ComponentModel.PropertyChangedEventArgs"/>.</summary>
    internal const string PropertyChangedEventArgsMetadataName = "System.ComponentModel.PropertyChangedEventArgs";

    /// <summary>Metadata name for <see cref="System.ComponentModel.PropertyChangingEventArgs"/>.</summary>
    internal const string PropertyChangingEventArgsMetadataName = "System.ComponentModel.PropertyChangingEventArgs";

    /// <summary>Metadata name for <see cref="System.ComponentModel.PropertyChangedEventHandler"/>.</summary>
    internal const string PropertyChangedEventHandlerMetadataName = "System.ComponentModel.PropertyChangedEventHandler";

    /// <summary>Metadata name for <see cref="System.ComponentModel.PropertyChangingEventHandler"/>.</summary>
    internal const string PropertyChangingEventHandlerMetadataName = "System.ComponentModel.PropertyChangingEventHandler";

    /// <summary>Metadata name for WPF's <c>DependencyObject</c> base class.</summary>
    internal const string WpfDependencyObjectMetadataName = "System.Windows.DependencyObject";

    /// <summary>Metadata name for WinUI's <c>DependencyObject</c> base class.</summary>
    internal const string WinUIDependencyObjectMetadataName = "Microsoft.UI.Xaml.DependencyObject";

    /// <summary>Metadata name for Apple's <c>NSObject</c> base class (KVO support).</summary>
    internal const string NSObjectMetadataName = "Foundation.NSObject";

    /// <summary>Metadata name for WinForms' <c>Component</c> base class.</summary>
    internal const string WinFormsComponentMetadataName = "System.ComponentModel.Component";

    /// <summary>Metadata name for Android's <c>View</c> base class.</summary>
    internal const string AndroidViewMetadataName = "Android.Views.View";

    /// <summary>Metadata name for <see cref="System.ComponentModel.INotifyDataErrorInfo"/>.</summary>
    internal const string INotifyDataErrorInfoMetadataName = "System.ComponentModel.INotifyDataErrorInfo";

    /// <summary>Class name for the runtime API stub extension class emitted as post-initialization output.</summary>
    internal const string StubExtensionClassName = "ReactiveUIBindingExtensions";

    /// <summary>Class name for the scheduler-related extension methods stub class.</summary>
    internal const string SchedulerExtensionClassName = "ReactiveSchedulerExtensions";

    /// <summary>Class name for the generated per-invocation dispatch class.</summary>
    internal const string GeneratedExtensionClassName = "__ReactiveUIGeneratedBindings";

    /// <summary>
    /// The namespace the dispatch overloads are emitted into on consumers that predate global usings, where
    /// there is no way to reach a compilation-private namespace.
    /// </summary>
    internal const string SharedGeneratedNamespace = "ReactiveUI.Binding";

    /// <summary>
    /// The root under which each assembly gets its own generated namespace, used when the build exposes no root
    /// namespace to emit into.
    /// </summary>
    internal const string GeneratedNamespaceRoot = "ReactiveUI.Binding.Generated";

    /// <summary>The namespace interceptors are emitted into.</summary>
    /// <remarks>
    /// Fixed rather than derived from the consumer, because nothing has to reach it: an interceptor claims its
    /// call site by name and is never found by lookup. Being fixed is what lets the shipped props opt exactly
    /// this namespace into interception without knowing anything about the project it is opting in.
    /// </remarks>
    internal const string InterceptorNamespace = "ReactiveUI.Binding.Generated.Interceptors";

    /// <summary>The build property a consumer's compiler reads the interception opt-in from.</summary>
    internal const string InterceptorsNamespacesFeature = "InterceptorsNamespaces";

    /// <summary>Metadata name of the attribute that exposes an assembly's internals to another.</summary>
    internal const string InternalsVisibleToAttributeName = "System.Runtime.CompilerServices.InternalsVisibleToAttribute";

    /// <summary>The namespace the System.Reactive flavour of the runtime library lives in.</summary>
    internal const string ReactiveRuntimeNamespace = "ReactiveUI.Binding.Reactive";

    /// <summary>Metadata name of the runtime stub class in the lean runtime package.</summary>
    internal const string LeanStubMetadataName = "ReactiveUI.Binding.ReactiveUIBindingExtensions";

    /// <summary>Metadata name of the runtime stub class in the System.Reactive runtime package.</summary>
    internal const string ReactiveStubMetadataName = "ReactiveUI.Binding.Reactive.ReactiveUIBindingExtensions";

    /// <summary>
    /// Metadata name of a type the System.Reactive flavour of ReactiveUI.Primitives declares, used to reach that
    /// namespace and read the names it offers.
    /// </summary>
    internal const string PrimitivesReactiveAnchorMetadataName = "ReactiveUI.Primitives.Reactive.LinqExtensions";

    /// <summary>The scheduler abstraction the lean runtime library schedules on.</summary>
    internal const string LeanSchedulerTypeName = "global::ReactiveUI.Primitives.Concurrency.ISequencer";

    /// <summary>The scheduler abstraction the System.Reactive flavour schedules on.</summary>
    internal const string ReactiveSchedulerTypeName = "global::System.Reactive.Concurrency.IScheduler";

    /// <summary>The generated namespace segment used when the compilation has no assembly name.</summary>
    internal const string AnonymousAssemblyNamespaceSegment = "Anonymous";

    /// <summary>Method name for after-change property observation (<c>WhenChanged</c>).</summary>
    internal const string WhenChangedMethodName = "WhenChanged";

    /// <summary>Method name for before-change property observation (<c>WhenChanging</c>).</summary>
    internal const string WhenChangingMethodName = "WhenChanging";

    /// <summary>Method name for source-generated one-way binding (<c>BindOneWay</c>).</summary>
    internal const string BindOneWayMethodName = "BindOneWay";

    /// <summary>Method name for source-generated two-way binding (<c>BindTwoWay</c>).</summary>
    internal const string BindTwoWayMethodName = "BindTwoWay";

    /// <summary>Method name for ReactiveUI-compatible one-way binding (<c>OneWayBind</c>).</summary>
    internal const string OneWayBindMethodName = "OneWayBind";

    /// <summary>Method name for ReactiveUI-compatible two-way binding (<c>Bind</c>).</summary>
    internal const string BindMethodName = "Bind";

    /// <summary>Method name for multi-property value observation (<c>WhenAnyValue</c>).</summary>
    internal const string WhenAnyValueMethodName = "WhenAnyValue";

    /// <summary>Method name for multi-property observation with <c>IObservedChange</c> wrappers (<c>WhenAny</c>).</summary>
    internal const string WhenAnyMethodName = "WhenAny";

    /// <summary>Method name for observable-of-observables merging (<c>WhenAnyObservable</c>).</summary>
    internal const string WhenAnyObservableMethodName = "WhenAnyObservable";

    /// <summary>Method name for interaction binding (<c>BindInteraction</c>).</summary>
    internal const string BindInteractionMethodName = "BindInteraction";

    /// <summary>Method name for command binding (<c>BindCommand</c>).</summary>
    internal const string BindCommandMethodName = "BindCommand";

    /// <summary>Method name for binding an observable stream to a target property (<c>BindTo</c>).</summary>
    internal const string BindToMethodName = "BindTo";

    /// <summary>Method name for executing a command with each value a stream produces (<c>InvokeCommand</c>).</summary>
    internal const string InvokeCommandMethodName = "InvokeCommand";

    /// <summary>Method name for backing a read-only property with an observable (<c>ToProperty</c>).</summary>
    internal const string ToPropertyMethodName = "ToProperty";

    /// <summary>Metadata name for the open generic <c>IViewFor&lt;T&gt;</c> interface used for view resolution.</summary>
    internal const string IViewForGenericMetadataName = "ReactiveUI.Binding.IViewFor`1";

    /// <summary>Metadata name for the System.Reactive flavour's open generic <c>IViewFor&lt;T&gt;</c> interface.</summary>
    internal const string ReactiveIViewForGenericMetadataName = "ReactiveUI.Binding.Reactive.IViewFor`1";

    /// <summary>The namespace the lean runtime declares its view types in, with the trailing dot.</summary>
    internal const string LeanViewNamespacePrefix = "ReactiveUI.Binding.";

    /// <summary>The namespace the System.Reactive runtime declares its view types in, with the trailing dot.</summary>
    internal const string ReactiveViewNamespacePrefix = "ReactiveUI.Binding.Reactive.";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' generic <c>[IViewFor&lt;T&gt;]</c> attribute.</summary>
    internal const string SourceGeneratorsGenericIViewForAttributeMetadataName = "ReactiveUI.SourceGenerators.IViewForAttribute`1";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' <c>[IViewFor("...")]</c> attribute.</summary>
    internal const string SourceGeneratorsIViewForAttributeMetadataName = "ReactiveUI.SourceGenerators.IViewForAttribute";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' <c>[Reactive]</c> attribute.</summary>
    internal const string SourceGeneratorsReactiveAttributeMetadataName = "ReactiveUI.SourceGenerators.ReactiveAttribute";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' <c>[ReactiveCollection]</c> attribute.</summary>
    internal const string SourceGeneratorsReactiveCollectionAttributeMetadataName = "ReactiveUI.SourceGenerators.ReactiveCollectionAttribute";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' <c>[BindableDerivedList]</c> attribute.</summary>
    internal const string SourceGeneratorsBindableDerivedListAttributeMetadataName = "ReactiveUI.SourceGenerators.BindableDerivedListAttribute";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' <c>[ReactiveCommand]</c> attribute.</summary>
    internal const string SourceGeneratorsReactiveCommandAttributeMetadataName = "ReactiveUI.SourceGenerators.ReactiveCommandAttribute";

    /// <summary>Metadata name of ReactiveUI.SourceGenerators' <c>[IReactiveObject]</c> attribute.</summary>
    internal const string SourceGeneratorsIReactiveObjectAttributeMetadataName = "ReactiveUI.SourceGenerators.IReactiveObjectAttribute";

    /// <summary>Metadata name for the <c>ExcludeFromViewRegistrationAttribute</c> used to skip view auto-registration.</summary>
    internal const string ExcludeFromViewRegistrationAttributeMetadataName =
        "ReactiveUI.Binding.ExcludeFromViewRegistrationAttribute";

    /// <summary>Metadata name for the <c>SingleInstanceViewAttribute</c> used for singleton view caching.</summary>
    internal const string SingleInstanceViewAttributeMetadataName = "ReactiveUI.Binding.SingleInstanceViewAttribute";

    /// <summary>Metadata name for the <c>ViewContractAttribute</c> used for contract-based view registration.</summary>
    internal const string ViewContractAttributeMetadataName = "ReactiveUI.Binding.ViewContractAttribute";

    /// <summary>Metadata name for the <c>IBindingTypeConverter</c> interface used for custom type conversions.</summary>
    internal const string IBindingTypeConverterMetadataName = "ReactiveUI.Binding.IBindingTypeConverter";

    /// <summary>Metadata name for the open generic <c>IInteraction&lt;TInput, TOutput&gt;</c> interface.</summary>
    internal const string IInteractionMetadataName = "ReactiveUI.Binding.IInteraction`2";

    /// <summary>Metadata name for the <c>System.Windows.Input.ICommand</c> interface.</summary>
    internal const string ICommandMetadataName = "System.Windows.Input.ICommand";

    /// <summary>Metadata name for the <c>CallerFilePathAttribute</c> used in dispatch stubs.</summary>
    internal const string CallerFilePathAttributeName = "System.Runtime.CompilerServices.CallerFilePathAttribute";

    /// <summary>Metadata name for the <c>CallerLineNumberAttribute</c> used in dispatch stubs.</summary>
    internal const string CallerLineNumberAttributeName = "System.Runtime.CompilerServices.CallerLineNumberAttribute";

    /// <summary>Metadata name for the <c>CallerArgumentExpressionAttribute</c> used in dispatch stubs for C# 10+ projects.</summary>
    internal const string CallerArgumentExpressionAttributeMetadataName =
        "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute";

    /// <summary>Metadata name for the <c>ModuleInitializerAttribute</c> the generated view dispatch registers itself with.</summary>
    internal const string ModuleInitializerAttributeMetadataName =
        "System.Runtime.CompilerServices.ModuleInitializerAttribute";
}
