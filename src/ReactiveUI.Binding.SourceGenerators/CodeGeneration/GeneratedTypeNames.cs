// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>The fully qualified type names, and builders for their closed generic forms, written into generated source.</summary>
internal static class GeneratedTypeNames
{
    /// <summary><c>System.IObservable</c> (open generic; use <see cref="ObservableOf"/>).</summary>
    internal const string IObservable = "global::System.IObservable";

    /// <summary>The projection sink, which maps each value of a source sequence (open generic).</summary>
    internal const string MapSignal = "global::ReactiveUI.Primitives.Signals.MapSignal";

    /// <summary>The typed two-source combination operator.</summary>
    internal const string CombineLatestSignal = "global::ReactiveUI.Primitives.Advanced.CombineLatestSignal";

    /// <summary>The typed distinct-value operator.</summary>
    internal const string UniqueSignal = "global::ReactiveUI.Primitives.Advanced.UniqueSignal";

    /// <summary>The typed scheduled-delivery operator.</summary>
    internal const string WitnessOnSignal = "global::ReactiveUI.Primitives.Advanced.WitnessOnSignal";

    /// <summary>The projecting switch sink, which maps each value onto an inner sequence and follows only the latest (open generic).</summary>
    internal const string SwitchMapSignal = "global::ReactiveUI.Primitives.Advanced.SwitchMapSignal";

    /// <summary>The interleaving sink, which relays every source sequence at once (open generic).</summary>
    internal const string MergeSignal = "global::ReactiveUI.Primitives.Advanced.MergeSignal";

    /// <summary><c>System.Func</c> (open generic; use <see cref="FuncOf"/>).</summary>
    internal const string Func = "global::System.Func";

    /// <summary><c>System.Linq.Expressions.Expression</c> (open generic; use <see cref="PropertyExpression"/>).</summary>
    internal const string Expression = "global::System.Linq.Expressions.Expression";

    /// <summary>The fully qualified name of <c>System.IDisposable</c>.</summary>
    internal const string IDisposable = "global::System.IDisposable";

    /// <summary>The fully qualified name of <c>System.InvalidOperationException</c>.</summary>
    internal const string InvalidOperationException = "global::System.InvalidOperationException";

    /// <summary>The fully qualified name of the <c>StringComparison.OrdinalIgnoreCase</c> member.</summary>
    internal const string OrdinalIgnoreCase = "global::System.StringComparison.OrdinalIgnoreCase";

    /// <summary>The fully qualified name of <c>System.ComponentModel.INotifyPropertyChanging</c>.</summary>
    internal const string INotifyPropertyChanging = "global::System.ComponentModel.INotifyPropertyChanging";

    /// <summary>The fully qualified name of <c>System.ComponentModel.INotifyPropertyChanged</c>.</summary>
    internal const string INotifyPropertyChanged = "global::System.ComponentModel.INotifyPropertyChanged";

    /// <summary>The <c>[CallerArgumentExpression]</c> attribute type.</summary>
    internal const string CallerArgumentExpression = "global::System.Runtime.CompilerServices.CallerArgumentExpression";

    /// <summary>The <c>[CallerFilePath]</c> attribute type.</summary>
    internal const string CallerFilePath = "global::System.Runtime.CompilerServices.CallerFilePath";

    /// <summary>The <c>[CallerLineNumber]</c> attribute type.</summary>
    internal const string CallerLineNumber = "global::System.Runtime.CompilerServices.CallerLineNumber";

    /// <summary><c>ReactiveUI.Binding.IInteraction</c> (open generic).</summary>
    internal const string IInteraction = "global::ReactiveUI.Binding.IInteraction";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IBindingTypeConverter</c>.</summary>
    internal const string IBindingTypeConverter = "global::ReactiveUI.Binding.IBindingTypeConverter";

    /// <summary>The fully qualified name of <c>System.Windows.Input.ICommand</c>.</summary>
    internal const string ICommand = "global::System.Windows.Input.ICommand";

    /// <summary>The subscription that holds nothing, handed back where there is nothing to disconnect.</summary>
    internal const string EmptyDisposable = "global::ReactiveUI.Primitives.Disposables.EmptyDisposable";

    /// <summary>The type a value is carried as when nothing more specific is known.</summary>
    internal const string ObjectType = "object";

    /// <summary>The shared <c>EmptyDisposable</c> instance a generated member returns when it has nothing to release.</summary>
    internal const string EmptyDisposableInstance = $"{EmptyDisposable}.Instance";

    /// <summary>The runtime gate that offers each value to a command and executes the ones it accepts.</summary>
    internal const string CommandInvoker = "global::ReactiveUI.Binding.CommandBinding.CommandInvoker";

    /// <summary>The <c>ReactiveUI.Binding.Observables</c> namespace prefix (no trailing dot).</summary>
    internal const string Observables = "global::ReactiveUI.Binding.Observables";

    /// <summary>Opens the observation of a property with no change mechanism, which emits the current value and never completes.</summary>
    internal const string OpenUnchangingProperty = "new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<";

    /// <summary>Opens the observation of a property read when a subscriber arrives, with no change mechanism.</summary>
    internal const string OpenDeferredProperty = "new global::ReactiveUI.Binding.Observables.DeferredPropertyObservable<";

    /// <summary>Opens the observation of a property through a platform's native change callback.</summary>
    internal const string OpenCallbackProperty = "new global::ReactiveUI.Binding.Observables.CallbackPropertyObservable<";

    /// <summary>Opens the observation of an Apple property through key-value observing.</summary>
    internal const string OpenKvoProperty = "new global::ReactiveUI.Binding.Observables.KvoPropertyObservable<";

    /// <summary>The fully qualified AppKit target/action receiver that generated AppKit command bindings install.</summary>
    internal const string AppKitCommandTarget = "global::ReactiveUI.Binding.CommandBinding.AppKitCommandTarget";

    /// <summary>The metadata name of the key-value observing observable in the lean runtime.</summary>
    internal const string KvoPropertyObservableMetadataName = "ReactiveUI.Binding.Observables.KvoPropertyObservable`1";

    /// <summary>The metadata name of the key-value observing observable in the System.Reactive runtime.</summary>
    internal const string ReactiveKvoPropertyObservableMetadataName = "ReactiveUI.Binding.Reactive.Observables.KvoPropertyObservable`1";

    /// <summary>The metadata name of the AppKit command target in the lean runtime.</summary>
    internal const string AppKitCommandTargetMetadataName = "ReactiveUI.Binding.CommandBinding.AppKitCommandTarget";

    /// <summary>The metadata name of the AppKit command target in the System.Reactive runtime.</summary>
    internal const string ReactiveAppKitCommandTargetMetadataName = "ReactiveUI.Binding.Reactive.CommandBinding.AppKitCommandTarget";

    /// <summary>The scheduler type the generated scheduler-taking overloads declare.</summary>
    internal const string ISequencer = "global::ReactiveUI.Primitives.Concurrency.ISequencer";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.LinqExtensions</c>.</summary>
    internal const string LinqExtensions = "global::ReactiveUI.Primitives.LinqExtensions";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.SubscribeExtensions</c>.</summary>
    internal const string RxBindingExtensions = "global::ReactiveUI.Primitives.SubscribeExtensions";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingHooks</c>.</summary>
    internal const string BindingHooks = "global::ReactiveUI.Binding.BindingHooks";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.ObservedChange</c> (open generic).</summary>
    internal const string ObservedChange = "global::ReactiveUI.Binding.ObservedChange";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IObservedChange</c> (open generic).</summary>
    internal const string IObservedChange = "global::ReactiveUI.Binding.IObservedChange";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingDirection</c>.</summary>
    internal const string BindingDirection = "global::ReactiveUI.Binding.BindingDirection";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingChange</c>.</summary>
    internal const string BindingChange = "global::ReactiveUI.Binding.BindingChange";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.PropertyValues</c>, the multi-property emission.</summary>
    internal const string PropertyValues = "global::ReactiveUI.Binding.PropertyValues";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingErrors</c>, which every binding write subscribes through.</summary>
    internal const string BindingErrors = "global::ReactiveUI.Binding.BindingErrors";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Fallback.RuntimeBindingConverter</c>.</summary>
    internal const string RuntimeBindingConverter = "global::ReactiveUI.Binding.Fallback.RuntimeBindingConverter";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Fallback.ObservationAffinityChecker</c>.</summary>
    internal const string ObservationAffinityChecker = "global::ReactiveUI.Binding.Fallback.ObservationAffinityChecker";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Observables.PluginPropertyObservable</c>.</summary>
    internal const string PluginPropertyObservable = "global::ReactiveUI.Binding.Observables.PluginPropertyObservable";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingSchedulers</c>.</summary>
    internal const string BindingSchedulers = "global::ReactiveUI.Binding.BindingSchedulers";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IViewThreadInvoker</c>.</summary>
    internal const string IViewThreadInvoker = "global::ReactiveUI.Binding.IViewThreadInvoker";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.Disposables.MultipleDisposable</c>.</summary>
    internal const string MultipleDisposable = "global::ReactiveUI.Primitives.Disposables.MultipleDisposable";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.Disposables.SwapDisposable</c>.</summary>
    internal const string SwapDisposable = "global::ReactiveUI.Primitives.Disposables.SwapDisposable";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.Disposables.ActionDisposable</c>.</summary>
    internal const string ActionDisposable = "global::ReactiveUI.Primitives.Disposables.ActionDisposable";

    /// <summary>The fully qualified subscription extension a generated binding subscribes a callback with.</summary>
    internal const string Subscribe = $"{RxBindingExtensions}.Subscribe";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.Advanced.ImmutableEmptySignal</c>.</summary>
    internal const string ImmutableEmptySignal = "global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.Advanced.ImmediateReturnSignal</c>.</summary>
    internal const string ImmediateReturnSignal = "global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Observables.PropertyObservable</c>.</summary>
    internal const string PropertyObservable = "global::ReactiveUI.Binding.Observables.PropertyObservable";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Observables.PropertyChangingObservable</c>.</summary>
    internal const string PropertyChangingObservable = "global::ReactiveUI.Binding.Observables.PropertyChangingObservable";

    /// <summary>The fully qualified name of <c>System.Collections.Generic.EqualityComparer</c>.</summary>
    internal const string EqualityComparer = "global::System.Collections.Generic.EqualityComparer";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IInteractionContext</c>.</summary>
    internal const string IInteractionContext = "global::ReactiveUI.Binding.IInteractionContext";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.ReactiveBinding</c>.</summary>
    internal const string ReactiveBinding = "global::ReactiveUI.Binding.ReactiveBinding";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IReactiveBinding</c>.</summary>
    internal const string IReactiveBinding = "global::ReactiveUI.Binding.IReactiveBinding";

    /// <summary>The fully qualified name of <c>System.EventHandler</c>.</summary>
    internal const string EventHandler = "global::System.EventHandler";

    /// <summary>The fully qualified name of <c>System.EventArgs</c>.</summary>
    internal const string EventArgs = "global::System.EventArgs";

    /// <summary>The fully qualified name of <c>System.Threading.Tasks.Task</c>.</summary>
    internal const string Task = "global::System.Threading.Tasks.Task";

    /// <summary>The fully qualified name of <c>System.Threading.Volatile</c>.</summary>
    internal const string Volatile = "global::System.Threading.Volatile";

    /// <summary>The fully qualified name of <c>System.IObserver</c>.</summary>
    internal const string IObserver = "global::System.IObserver";

    /// <summary>The fully qualified name of <c>System.Action</c>.</summary>
    internal const string Action = "global::System.Action";

    /// <summary>The fully qualified name of <c>System.ArgumentNullException</c>.</summary>
    internal const string ArgumentNullException = "global::System.ArgumentNullException";

    /// <summary>The fully qualified name of <c>System.AttributeUsage</c>.</summary>
    internal const string AttributeUsage = "global::System.AttributeUsage";

    /// <summary>The fully qualified name of <c>System.AttributeTargets</c>.</summary>
    internal const string AttributeTargets = "global::System.AttributeTargets";

    /// <summary>The fully qualified name of <c>System.Attribute</c>.</summary>
    internal const string Attribute = "global::System.Attribute";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.ObservableAsPropertyHelper</c>.</summary>
    internal const string ObservableAsPropertyHelper = "global::ReactiveUI.Binding.ObservableAsPropertyHelper";

    /// <summary>The fully qualified immediate sequencer, which a binding treats the same as no scheduler.</summary>
    internal const string ImmediateSequencer = "global::ReactiveUI.Primitives.Concurrency.Sequencer.Immediate";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IViewFor</c>.</summary>
    internal const string IViewFor = "global::ReactiveUI.Binding.IViewFor";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.DefaultViewLocator</c>.</summary>
    internal const string DefaultViewLocator = "global::ReactiveUI.Binding.DefaultViewLocator";

    /// <summary>The fully qualified name of <c>System.Windows.Forms.Control</c>.</summary>
    internal const string WinFormsControl = "global::System.Windows.Forms.Control";

    /// <summary>The fully qualified name of <c>Foundation.NSString</c>.</summary>
    internal const string NSString = "global::Foundation.NSString";

    /// <summary>The fully qualified name of <c>Foundation.NSObject</c>.</summary>
    internal const string NSObject = "global::Foundation.NSObject";

    /// <summary>The message a generated overload throws when no binding matches the call site.</summary>
    internal const string NoBindingFoundMessage =
        "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.";

    /// <summary>Builds the closed <c>System.IObservable&lt;T&gt;</c> type name.</summary>
    /// <param name="typeArg">The fully-qualified value type.</param>
    /// <returns>The closed generic type name.</returns>
    internal static string ObservableOf(string typeArg) => $"{IObservable}<{typeArg}>";

    /// <summary>Builds the closed <c>System.Func&lt;TFrom, TTo&gt;</c> type name.</summary>
    /// <param name="from">The fully-qualified input type.</param>
    /// <param name="to">The fully-qualified result type.</param>
    /// <returns>The closed generic delegate type name.</returns>
    internal static string FuncOf(string from, string to) => $"{Func}<{from}, {to}>";

    /// <summary>Builds <c>Expression&lt;Func&lt;TDeclaring, TProperty&gt;&gt;</c> for a property selector.</summary>
    /// <param name="declaringType">The fully-qualified declaring type.</param>
    /// <param name="propertyType">The fully-qualified property type.</param>
    /// <returns>The property-selector expression type name.</returns>
    internal static string PropertyExpression(string declaringType, string propertyType) =>
        $"{Expression}<{FuncOf(declaringType, propertyType)}>";

    /// <summary>Renders the read of a segment's property from an object that has to be cast to reach it.</summary>
    /// <param name="segment">The property being read.</param>
    /// <param name="castTypeName">The type the object is cast to before the read.</param>
    /// <param name="objectExpression">The expression producing the object.</param>
    /// <returns>The rendered read.</returns>
    internal static string ReadProperty(
        Models.PropertyPathSegment segment,
        string castTypeName,
        string objectExpression)
    {
        var read = $"(({castTypeName}){objectExpression}).{segment.PropertyName}";

        // A view exposing its view model as a base or an interface narrows the read to the view model the call site named.
        return segment.ReadCastTypeFullName is null
            ? read
            : $"(({segment.ReadCastTypeFullName})(object){read})";
    }
}
