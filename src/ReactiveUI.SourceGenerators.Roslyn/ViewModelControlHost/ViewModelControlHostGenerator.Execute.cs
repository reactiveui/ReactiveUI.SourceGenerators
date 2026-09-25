// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>Generates a Windows Forms view-model control host.</summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class ViewModelControlHostGenerator
{
    /// <summary>The fully qualified generator name written to generated-code attributes.</summary>
    private static readonly string GeneratorName = typeof(ViewModelControlHostGenerator).FullName!;

    /// <summary>The generator assembly version written to generated-code attributes.</summary>
    private static readonly string GeneratorVersion = typeof(ViewModelControlHostGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The <c>GeneratedCode</c> attribute stamped on every generated host, built once.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <summary>Gets generation metadata for an attributed view-model control host.</summary>
    private static readonly ViewModelControlHostInfoFactory GetClassInfo = static (
        in GeneratorAttributeSyntaxContext context,
        CancellationToken token) =>
    {
        if (!(context.TargetNode is ClassDeclarationSyntax declaredClass && declaredClass.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();
        var symbol = context.TargetSymbol;
        var attributeData = context.Attributes[0];

        token.ThrowIfCancellationRequested();
        if (symbol is not INamedTypeSymbol classSymbol)
        {
            return default;
        }

        var viewModelTypeName = GetFirstConstructorArgument(attributeData);
        if (viewModelTypeName is null)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(classSymbol);

        return new(
            targetInfo.FileHintName,
            targetInfo.TargetName,
            targetInfo.TargetNamespace,
            targetInfo.TargetNamespaceWithNamespace,
            targetInfo.TargetVisibility,
            targetInfo.TargetType,
            viewModelTypeName!);
    };

    /// <summary>Gets view-model control host metadata from a generator attribute context.</summary>
    /// <param name="context">The generator attribute context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The view-model control host metadata, or <see langword="null"/> when the target is invalid.</returns>
    private delegate ViewModelControlHostInfo? ViewModelControlHostInfoFactory(
        in GeneratorAttributeSyntaxContext context,
        CancellationToken token);

    /// <summary>Gets the first string constructor argument from an attribute.</summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <returns>The first string argument, or <see langword="null"/> when none exists.</returns>
    private static string? GetFirstConstructorArgument(AttributeData attributeData)
    {
        using var arguments = attributeData.GetConstructorArguments<string>().GetEnumerator();
        return arguments.MoveNext() ? arguments.Current : null;
    }

    /// <summary>Generates the partial declaration that turns the target into a view-model control host.</summary>
    /// <param name="info">The view-model control host metadata.</param>
    /// <param name="integration">The detected ReactiveUI integration.</param>
    /// <returns>The file's text.</returns>
    private static string GenerateSource(ViewModelControlHostInfo info, ReactiveUiIntegration integration)
    {
        var exceptionHandler = integration.IsNewerThan22
            ? "RxState.DefaultExceptionHandler!.OnNext"
            : "RxApp.DefaultExceptionHandler!.OnNext";

        var writer = SourceWriter.Rent();
        WriteFileHeader(writer, integration);

        // The namespace is written as-is, even when empty, to keep the historical output.
        _ = writer.Append("namespace ").Line(info.TargetNamespace).OpenBlock();
        foreach (var attribute in AttributeDefinitions.ExcludeFromCodeCoverage)
        {
            _ = writer.Line(attribute);
        }

        _ = writer.Line("[DefaultProperty(\"ViewModel\")]")
            .Line(GeneratedCodeAttribute)
            .Append(info.TargetVisibility).Append(" partial ").Append(info.TargetType).Append(' ').Append(info.TargetName)
            .Append(" : ").Append(info.ViewModelTypeName).Line(", IReactiveObject, IViewFor")
            .OpenBlock();

        WriteConstructor(writer, info.TargetName);
        WriteProperties(writer.BlankLine());
        WriteBindableProperties(writer.BlankLine());
        WriteDispose(writer.BlankLine());
        WriteSetupBindings(writer.BlankLine(), exceptionHandler);
        WriteUpdateContent(writer.BlankLine());
        WriteObservableHelpers(writer.BlankLine());
        WriteCombineLatestSubscription(writer.BlankLine());
        WriteDisposableCollection(writer.BlankLine());
        WriteSubscriptionSlot(writer.BlankLine());
        WriteEmptyDisposable(writer.BlankLine());

        return writer.CloseBlock()
            .CloseBlock()
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Writes the file header: the generated marker, the license, the usings, and the pragmas.</summary>
    /// <param name="writer">The writer, at the start of the file.</param>
    /// <param name="integration">The detected ReactiveUI integration, which picks the ReactiveUI usings.</param>
    private static void WriteFileHeader(SourceWriter writer, ReactiveUiIntegration integration) =>
        _ = writer.AutoGenerated()
            .Append("// Copyright (c) ").Append(DateTime.Now.Year).Line(" .NET Foundation and Contributors. All rights reserved.")
            .Lines("""
                // Licensed to the .NET Foundation under one or more agreements.
                // The .NET Foundation licenses this file to you under the MIT license.
                // See the LICENSE file in the project root for full license information.
                """)
            .BlankLine()
            .Lines(integration.UsingDirectives)
            .Lines("""
                using System;
                using System.Collections.Generic;
                using System.ComponentModel;
                using System.Windows.Forms;
                """)
            .BlankLine()
            .DisableWarningsEnableNullable();

    /// <summary>Writes the host's fields and its constructor.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    /// <param name="typeName">The host type's name.</param>
    private static void WriteConstructor(SourceWriter writer, string typeName) =>
        _ = writer.Lines("""
                private readonly DisposableCollection _disposables = new();
                private Control? _defaultContent;
                private IObservable<string>? _viewContractObservable;
                private object? _viewModel;
                private object? _content;
                private bool _cacheViews;

                /// <summary>
                """)
            .Append("/// Initializes a new instance of the <see cref = \"").Append(typeName).Line("\"/> class.")
            .Line("/// </summary>")
            .Append("public ").Append(typeName).Line("()")
            .OpenBlock()
            .Lines("""
                InitializeComponent();
                _cacheViews = DefaultCacheViewsEnabled;
                SetupBindings();
                """)
            .CloseBlock();

    /// <summary>Writes the host's events and its content and view-location properties.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteProperties(SourceWriter writer) =>
        _ = writer.Lines("""
            /// <inheritdoc/>
            public event PropertyChangingEventHandler? PropertyChanging;

            /// <inheritdoc/>
            public event PropertyChangedEventHandler? PropertyChanged;

            /// <summary>
            /// Gets or sets a value indicating whether [default cache views enabled].
            /// </summary>
            public static bool DefaultCacheViewsEnabled { get; set; }

            /// <summary>
            /// Gets the current view.
            /// </summary>
            public Control? CurrentView => _content as Control;

            /// <summary>
            /// Gets or sets the default content.
            /// </summary>
            [Category("ReactiveUI")]
            [Description("The default control when no viewmodel is specified")]
            public Control? DefaultContent { get => _defaultContent; set => this.RaiseAndSetIfChanged(ref _defaultContent, value); }

            /// <summary>
            /// Gets or sets the view contract observable.
            /// </summary>
            /// <value>
            /// The view contract observable.
            /// </value>
            [Browsable(false)]
            public IObservable<string>? ViewContractObservable { get => _viewContractObservable; set => this.RaiseAndSetIfChanged(ref _viewContractObservable, value); }

            /// <summary>
            /// Gets or sets the view locator.
            /// </summary>
            [Browsable(false)]
            public IViewLocator? ViewLocator { get; set; }
            """);

    /// <summary>Writes the host's bindable view-model properties and its <c>IReactiveObject</c> implementation.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteBindableProperties(SourceWriter writer) =>
        _ = writer.Lines("""
            /// <inheritdoc/>
            [Category("ReactiveUI")]
            [Description("The viewmodel to host.")]
            [Bindable(true)]
            public object? ViewModel { get => _viewModel; set => this.RaiseAndSetIfChanged(ref _viewModel, value); }

            /// <summary>
            /// Gets or sets the content.
            /// </summary>
            [Category("ReactiveUI")]
            [Description("The Current View")]
            [Bindable(true)]
            public object? Content { get => _content; protected set => this.RaiseAndSetIfChanged(ref _content, value); }

            /// <summary>
            /// Gets or sets a value indicating whether to cache views.
            /// </summary>
            [Category("ReactiveUI")]
            [Description("Cache Views")]
            [Bindable(true)]
            [DefaultValue(true)]
            public bool CacheViews { get => _cacheViews; set => this.RaiseAndSetIfChanged(ref _cacheViews, value); }

            /// <inheritdoc/>
            void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);

            /// <inheritdoc/>
            void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
            """);

    /// <summary>Writes the host's disposal.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteDispose(SourceWriter writer) =>
        _ = writer.Lines("""
            /// <summary>
            /// Clean up any resources being used.
            /// </summary>
            /// <param name = "disposing">true if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    components?.Dispose();
                }

                base.Dispose(disposing);
            }
            """);

    /// <summary>Writes the bindings that swap the hosted view as the content, default content, and view model change.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    /// <param name="exceptionHandler">The ReactiveUI default exception handler subscriptions report to.</param>
    private static void WriteSetupBindings(SourceWriter writer, string exceptionHandler) =>
        _ = writer.Line("private void SetupBindings()")
            .OpenBlock()
            .Lines("""
                AddSubscription(this.WhenAnyValue(x => x!.Content), new ValueObserver<object?>(x =>
                {
                    if (x is not Control control)
                    {
                        return;
                    }

                    // change the view in the ui
                    SuspendLayout();
                    // clear out existing visible control view
                    foreach (Control? c in Controls)
                    {
                        c?.Dispose();
                        Controls.Remove(c);
                    }

                    control.Dock = DockStyle.Fill;
                    Controls.Add(control);
                    ResumeLayout();
                """)
            .Append("}, ").Append(exceptionHandler).Line("));")
            .Lines("""
                AddSubscription(this.WhenAnyValue(x => x.DefaultContent), new ValueObserver<Control?>(x =>
                {
                    if (x is not null)
                    {
                        Content = DefaultContent;
                    }
                """)
            .Append("}, ").Append(exceptionHandler).Line("));")
            .Lines("""
                ViewContractObservable = new ReturnObservable<string>(string.Empty);
                var viewModelSubscription = new CombineLatestSubscription<object?, string>(
                    UpdateContent,
                """)
            .Indent().Append(exceptionHandler).Line(");").Outdent()
            .Lines("""
                _disposables.Add(viewModelSubscription);
                viewModelSubscription.Connect(
                    this.WhenAnyValue(x => x.ViewModel),
                    this.WhenAnyObservable(x => x.ViewContractObservable!));
                """)
            .CloseBlock();

    /// <summary>Writes the methods that register a subscription and resolve the view for a new view model.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteUpdateContent(SourceWriter writer) =>
        _ = writer.Lines("""
            private void AddSubscription<T>(IObservable<T> observable, IObserver<T> observer)
            {
                var slot = new SubscriptionSlot();
                _disposables.Add(slot);
                slot.Set(observable.Subscribe(observer));
            }

            private void UpdateContent(object? viewModel, string contract)
            {
                // set content to default when viewmodel is null
                if (viewModel is null)
                {
                    if (DefaultContent is not null)
                    {
                        Content = DefaultContent;
                    }

                    return;
                }

                if (CacheViews)
                {
                    // when caching views, check the current viewmodel and type
                    var currentView = _content as IViewFor;
                    if (currentView?.ViewModel is not null && currentView.ViewModel.GetType() == viewModel.GetType())
                    {
                        currentView.ViewModel = viewModel;
                        // return early here after setting the viewmodel
                        // allowing the view to update its bindings
                        return;
                    }
                }

                var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                var view = viewLocator.ResolveView(viewModel, contract);
                if (view is not null)
                {
                    view.ViewModel = viewModel;
                    Content = view;
                }
            }
            """);

    /// <summary>Writes the nested observable and observer the host subscribes with, so it needs no Rx dependency.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteObservableHelpers(SourceWriter writer) =>
        _ = writer.Lines("""
            private sealed class ReturnObservable<T>(T value) : IObservable<T>
            {
                public IDisposable Subscribe(IObserver<T> observer)
                {
                    observer.OnNext(value);
                    observer.OnCompleted();
                    return EmptyDisposable.Instance;
                }
            }

            private sealed class ValueObserver<T>(Action<T> onNext, Action<Exception> onError, Action? onCompleted = null) : IObserver<T>
            {
                public void OnCompleted() => onCompleted?.Invoke();

                public void OnError(Exception error) => onError(error);

                public void OnNext(T value) => onNext(value);
            }
            """);

    /// <summary>Writes the nested subscription that combines the latest view model and contract values.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteCombineLatestSubscription(SourceWriter writer)
    {
        _ = writer.Line("private sealed class CombineLatestSubscription<TLeft, TRight> : IDisposable").OpenBlock();
        WriteCombineLatestState(writer);
        WriteCombineLatestValues(writer.BlankLine());
        WriteCombineLatestCompletion(writer.BlankLine());
        WriteCombineLatestFailure(writer.BlankLine());
        _ = writer.CloseBlock();
    }

    /// <summary>Writes the combined subscription's state, its constructor, and how it connects and disposes.</summary>
    /// <param name="writer">The writer, at the level of the subscription's members.</param>
    private static void WriteCombineLatestState(SourceWriter writer) =>
        _ = writer.Lines("""
            private readonly object _gate = new();
            private readonly Action<TLeft, TRight> _onNext;
            private readonly Action<Exception> _onError;
            private readonly SubscriptionSlot _leftSubscription = new();
            private readonly SubscriptionSlot _rightSubscription = new();
            private TLeft _left = default!;
            private TRight _right = default!;
            private bool _hasLeft;
            private bool _hasRight;
            private bool _leftCompleted;
            private bool _rightCompleted;
            private bool _isStopped;

            public CombineLatestSubscription(
                Action<TLeft, TRight> onNext,
                Action<Exception> onError)
            {
                _onNext = onNext;
                _onError = onError;
            }

            public void Connect(IObservable<TLeft> left, IObservable<TRight> right)
            {
                _leftSubscription.Set(left.Subscribe(new ValueObserver<TLeft>(SetLeft, Fail, CompleteLeft)));
                if (IsStopped())
                {
                    return;
                }

                _rightSubscription.Set(right.Subscribe(new ValueObserver<TRight>(SetRight, Fail, CompleteRight)));
            }

            public void Dispose()
            {
                lock (_gate)
                {
                    _isStopped = true;
                }

                _leftSubscription.Dispose();
                _rightSubscription.Dispose();
            }
            """);

    /// <summary>Writes the combined subscription's value callbacks, which publish once both sides have a value.</summary>
    /// <param name="writer">The writer, at the level of the subscription's members.</param>
    private static void WriteCombineLatestValues(SourceWriter writer) =>
        _ = writer.Lines("""
            private void SetLeft(TLeft value)
            {
                lock (_gate)
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    _left = value;
                    _hasLeft = true;
                    if (!_hasRight)
                    {
                        return;
                    }

                    _onNext(value, _right);
                }
            }

            private void SetRight(TRight value)
            {
                lock (_gate)
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    _right = value;
                    _hasRight = true;
                    if (!_hasLeft)
                    {
                        return;
                    }

                    _onNext(_left, value);
                }
            }
            """);

    /// <summary>Writes the combined subscription's completion callbacks.</summary>
    /// <param name="writer">The writer, at the level of the subscription's members.</param>
    private static void WriteCombineLatestCompletion(SourceWriter writer) =>
        _ = writer.Lines("""
            private void CompleteLeft()
            {
                var shouldStop = false;
                lock (_gate)
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    _leftCompleted = true;
                    shouldStop = !_hasLeft || _rightCompleted;
                    _isStopped = shouldStop;
                }

                if (shouldStop)
                {
                    _leftSubscription.Dispose();
                    _rightSubscription.Dispose();
                }
            }

            private void CompleteRight()
            {
                var shouldStop = false;
                lock (_gate)
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    _rightCompleted = true;
                    shouldStop = !_hasRight || _leftCompleted;
                    _isStopped = shouldStop;
                }

                if (shouldStop)
                {
                    _leftSubscription.Dispose();
                    _rightSubscription.Dispose();
                }
            }
            """);

    /// <summary>Writes the combined subscription's error callback and its stopped check.</summary>
    /// <param name="writer">The writer, at the level of the subscription's members.</param>
    private static void WriteCombineLatestFailure(SourceWriter writer) =>
        _ = writer.Lines("""
            private void Fail(Exception error)
            {
                lock (_gate)
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    _isStopped = true;
                }

                try
                {
                    _onError(error);
                }
                finally
                {
                    _leftSubscription.Dispose();
                    _rightSubscription.Dispose();
                }
            }

            private bool IsStopped()
            {
                lock (_gate)
                {
                    return _isStopped;
                }
            }
            """);

    /// <summary>Writes the nested collection that disposes the host's subscriptions together.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteDisposableCollection(SourceWriter writer) =>
        _ = writer.Lines("""
            private sealed class DisposableCollection : IDisposable
            {
                private readonly object _gate = new();
                private readonly List<IDisposable> _items = new();
                private bool _isDisposed;

                public void Add(IDisposable item)
                {
                    var disposeItem = false;
                    lock (_gate)
                    {
                        disposeItem = _isDisposed;
                        if (!disposeItem)
                        {
                            _items.Add(item);
                        }
                    }

                    if (disposeItem)
                    {
                        item.Dispose();
                    }
                }

                public void Dispose()
                {
                    IDisposable[] items;
                    lock (_gate)
                    {
                        if (_isDisposed)
                        {
                            return;
                        }

                        _isDisposed = true;
                        items = _items.ToArray();
                        _items.Clear();
                    }

                    foreach (var item in items)
                    {
                        item.Dispose();
                    }
                }
            }
            """);

    /// <summary>Writes the nested slot holding one replaceable subscription.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteSubscriptionSlot(SourceWriter writer) =>
        _ = writer.Lines("""
            private sealed class SubscriptionSlot : IDisposable
            {
                private readonly object _gate = new();
                private IDisposable? _subscription;
                private bool _isDisposed;

                public void Set(IDisposable subscription)
                {
                    IDisposable? previous;
                    var disposeSubscription = false;
                    lock (_gate)
                    {
                        disposeSubscription = _isDisposed;
                        previous = _subscription;
                        if (!disposeSubscription)
                        {
                            _subscription = subscription;
                        }
                    }

                    previous?.Dispose();
                    if (disposeSubscription)
                    {
                        subscription.Dispose();
                    }
                }

                public void Dispose()
                {
                    IDisposable? subscription;
                    lock (_gate)
                    {
                        if (_isDisposed)
                        {
                            return;
                        }

                        _isDisposed = true;
                        subscription = _subscription;
                        _subscription = null;
                    }

                    subscription?.Dispose();
                }
            }
            """);

    /// <summary>Writes the nested disposable that does nothing.</summary>
    /// <param name="writer">The writer, at the level of the host's members.</param>
    private static void WriteEmptyDisposable(SourceWriter writer) =>
        _ = writer.Lines("""
            private sealed class EmptyDisposable : IDisposable
            {
                public static EmptyDisposable Instance { get; } = new();

                public void Dispose()
                {
                }
            }
            """);
}
