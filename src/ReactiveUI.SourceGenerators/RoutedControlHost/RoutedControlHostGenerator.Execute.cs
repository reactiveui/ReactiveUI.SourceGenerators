﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Input.Models;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>
/// IViewForGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class RoutedControlHostGenerator
{
    private static readonly string GeneratorName = typeof(RoutedControlHostGenerator).FullName!;
    private static readonly string GeneratorVersion = typeof(RoutedControlHostGenerator).Assembly.GetName().Version.ToString();

    private static readonly string[] excludeFromCodeCoverage = ["[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]"];

    private static RoutedControlHostInfo? GetClassInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (!(context.TargetNode is ClassDeclarationSyntax declaredClass && declaredClass.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        var symbol = context.TargetSymbol;
        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.RoutedControlHostAttributeType, out var attributeData))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        var constructorArgument = attributeData.GetConstructorArguments<string>().First();
        if (constructorArgument is not string baseTypeName)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        if (symbol is not INamedTypeSymbol classSymbol)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        var compilation = context.SemanticModel.Compilation;
        var semanticModel = compilation.GetSemanticModel(context.SemanticModel.SyntaxTree);
        attributeData.GatherForwardedAttributesFromClass(semanticModel, declaredClass, token, out var attributesInfo);
        var classAttributesInfo = attributesInfo.Select(x => x.ToString()).ToImmutableArray();

        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(classSymbol);

        token.ThrowIfCancellationRequested();

        return new RoutedControlHostInfo(
            targetInfo.FileHintName,
            targetInfo.TargetName,
            targetInfo.TargetNamespace,
            targetInfo.TargetNamespaceWithNamespace,
            targetInfo.TargetVisibility,
            targetInfo.TargetType,
            baseTypeName,
            classAttributesInfo);
    }

    private static string GetRoutedControlHost(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, RoutedControlHostInfo vmcInfo)
    {
        // Prepare any forwarded property attributes
        var forwardedAttributesString = string.Join("\n        ", excludeFromCodeCoverage.Concat(vmcInfo.ForwardedAttributes));

        return
$$"""
// Copyright (c) {{DateTime.Now.Year}} .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;

// <auto-generated/>
#pragma warning disable
#nullable enable
namespace {{containingNamespace}}
{
    {{forwardedAttributesString}}
    [DefaultProperty("ViewModel")]
    [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
    {{containingClassVisibility}} partial class {{containingTypeName}} : {{vmcInfo.BaseTypeName}}, IReactiveObject
    {
        private readonly CompositeDisposable _disposables = [];
        private RoutingState? _router;
        private Control? _defaultContent;
        private IObservable<string>? _viewContractObservable;

        /// <summary>
        /// Initializes a new instance of the <see cref = "{{containingTypeName}}"/> class.
        /// </summary>
        public {{containingTypeName}}()
        {
            InitializeComponent();
            _disposables.Add(this.WhenAny(x => x.DefaultContent, x => x.Value).Subscribe(x =>
            {
                if (x is not null && Controls.Count == 0)
                {
                    Controls.Add(InitView(x));
                    components?.Add(DefaultContent);
                }
            }));
            ViewContractObservable = Observable.Return(default(string)!);
            var vmAndContract = this.WhenAnyObservable(x => x.Router!.CurrentViewModel!).CombineLatest(this.WhenAnyObservable(x => x.ViewContractObservable!), (vm, contract) => new { ViewModel = vm, Contract = contract });
            Control? viewLastAdded = null;
            _disposables.Add(vmAndContract.Subscribe(x =>
            {
                // clear all hosted controls (view or default content)
                SuspendLayout();
                Controls.Clear();
                viewLastAdded?.Dispose();
                if (x.ViewModel is null)
                {
                    if (DefaultContent is not null)
                    {
                        InitView(DefaultContent);
                        Controls.Add(DefaultContent);
                    }

                    ResumeLayout();
                    return;
                }

                var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                var view = viewLocator.ResolveView(x.ViewModel, x.Contract);
                if (view is not null)
                {
                    view.ViewModel = x.ViewModel;
                    viewLastAdded = InitView((Control)view);
                }

                if (viewLastAdded is not null)
                {
                    Controls.Add(viewLastAdded);
                }

                ResumeLayout();
            }, RxApp.DefaultExceptionHandler!.OnNext));
        }

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the default content.
        /// </summary>
        /// <value>
        /// The default content.
        /// </value>
        [Category("ReactiveUI")]
        [Description("The default control when no viewmodel is specified")]
        public Control? DefaultContent { get => _defaultContent; set => this.RaiseAndSetIfChanged(ref _defaultContent, value); }

        /// <summary>
        /// Gets or sets the <see cref = "RoutingState"/> of the view model stack.
        /// </summary>
        [Category("ReactiveUI")]
        [Description("The router.")]
        public RoutingState? Router { get => _router; set => this.RaiseAndSetIfChanged(ref _router, value); }

        /// <summary>
        /// Gets or sets the view contract observable.
        /// </summary>
        [Browsable(false)]
        public IObservable<string>? ViewContractObservable { get => _viewContractObservable; set => this.RaiseAndSetIfChanged(ref _viewContractObservable, value); }

        /// <summary>
        /// Gets or sets the view locator.
        /// </summary>
        [Browsable(false)]
        public IViewLocator? ViewLocator { get; set; }

        /// <inheritdoc/>
        void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);

        /// <inheritdoc/>
        void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name = "disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components is not null)
            {
                components.Dispose();
                _disposables.Dispose();
            }

            base.Dispose(disposing);
        }

        private static Control InitView(Control view)
        {
            view.Dock = DockStyle.Fill;
            return view;
        }
    }
}
#nullable restore
#pragma warning restore                
""";
    }
}
