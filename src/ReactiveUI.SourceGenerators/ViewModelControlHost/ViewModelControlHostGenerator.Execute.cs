﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Input.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>
/// IViewForGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class ViewModelControlHostGenerator
{
    internal static class Execute
    {
        internal static CompilationUnitSyntax GetViewModelControlHost(ViewModelControlHostInfo vmcInfo)
        {
            UsingDirectiveSyntax[] usings =
                  [
                      UsingDirective(ParseName("ReactiveUI"))
                    .WithUsingKeyword(
            Token(
                TriviaList(
                    [
                        Comment($"// Copyright (c) {DateTime.Now.Year} .NET Foundation and Contributors. All rights reserved."),
                        Comment("// Licensed to the .NET Foundation under one or more agreements."),
                        Comment("// The .NET Foundation licenses this file to you under the MIT license."),
                        Comment("// See the LICENSE file in the project root for full license information.")
                    ]),
                SyntaxKind.UsingKeyword,
                TriviaList())),
                    UsingDirective(ParseName("System.ComponentModel")),
                    UsingDirective(ParseName("System.Reactive.Disposables")),
                    UsingDirective(ParseName("System.Reactive.Linq")),
                    UsingDirective(ParseName("System.Windows.Forms")),
                ];

            var code = CompilationUnit()
            .WithUsings(List(usings))
            .WithTrailingTrivia(TriviaList(CarriageReturnLineFeed))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    NamespaceDeclaration(IdentifierName(vmcInfo.ClassNamespace))
                                .WithLeadingTrivia(TriviaList(
                                    Comment("// <auto-generated/>"),
                                    Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
                                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            ClassDeclaration(vmcInfo.ClassName)
                            .WithAttributeLists(
                                SingletonList(
                                    AttributeList(
                                        SingletonSeparatedList(
                                            Attribute(IdentifierName("DefaultProperty"))
                                            .WithArgumentList(
                                                AttributeArgumentList(
                                                    SingletonSeparatedList(
                                                        AttributeArgument(
                                                            LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression,
                                                                Literal("ViewModel"))))))))))
                            .WithModifiers(
                                TokenList(
                                    [
                                        Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.PartialKeyword)]))
                            .WithBaseList(
                                BaseList(
                                    SeparatedList<BaseTypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                SimpleBaseType(IdentifierName(vmcInfo.ViewModelTypeName)),
                                Token(SyntaxKind.CommaToken),
                                SimpleBaseType(IdentifierName("IReactiveObject")),
                                Token(SyntaxKind.CommaToken),
                                SimpleBaseType(IdentifierName("IViewFor"))
                                        })))))))
            .NormalizeWhitespace().ToFullString();

            // Remove the last 4 characters to remove the closing brackets
            var baseCode = code.Remove(code.Length - 4);

            // Prepare all necessary type names with type arguments
            using var stringStream = new StringWriter();
            using var writer = new IndentedTextWriter(stringStream, "\t");
            writer.WriteLine(baseCode);
            writer.Indent++;
            writer.Indent++;

            var body = """
                private readonly CompositeDisposable _disposables = [];
                private Control? _defaultContent;
                private IObservable<string>? _viewContractObservable;
                private object? _viewModel;
                private object? _content;
                private bool _cacheViews;

                /// <summary>
                /// Initializes a new instance of the <see cref="####REPLACEME####"/> class.
                /// </summary>
                public ####REPLACEME####()
                {
                    InitializeComponent();
                    _cacheViews = DefaultCacheViewsEnabled;
                    foreach (var d in SetupBindings())
                    {
                        _disposables.Add(d);
                    }
                }

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
                public Control? DefaultContent
                {
                    get => _defaultContent;
                    set => this.RaiseAndSetIfChanged(ref _defaultContent, value);
                }

                /// <summary>
                /// Gets or sets the view contract observable.
                /// </summary>
                /// <value>
                /// The view contract observable.
                /// </value>
                [Browsable(false)]
                public IObservable<string>? ViewContractObservable
                {
                    get => _viewContractObservable;
                    set => this.RaiseAndSetIfChanged(ref _viewContractObservable, value);
                }

                /// <summary>
                /// Gets or sets the view locator.
                /// </summary>
                [Browsable(false)]
                public IViewLocator? ViewLocator { get; set; }

                /// <inheritdoc/>
                [Category("ReactiveUI")]
                [Description("The viewmodel to host.")]
                [Bindable(true)]
                public object? ViewModel
                {
                    get => _viewModel;
                    set => this.RaiseAndSetIfChanged(ref _viewModel, value);
                }

                /// <summary>
                /// Gets or sets the content.
                /// </summary>
                [Category("ReactiveUI")]
                [Description("The Current View")]
                [Bindable(true)]
                public object? Content
                {
                    get => _content;
                    protected set => this.RaiseAndSetIfChanged(ref _content, value);
                }

                /// <summary>
                /// Gets or sets a value indicating whether to cache views.
                /// </summary>
                [Category("ReactiveUI")]
                [Description("Cache Views")]
                [Bindable(true)]
                [DefaultValue(true)]
                public bool CacheViews
                {
                    get => _cacheViews;
                    set => this.RaiseAndSetIfChanged(ref _cacheViews, value);
                }

                /// <inheritdoc/>
                void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);

                /// <inheritdoc/>
                void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

                /// <summary>
                /// Clean up any resources being used.
                /// </summary>
                /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
                protected override void Dispose(bool disposing)
                {
                    if (disposing && components is not null)
                    {
                        components.Dispose();
                        _disposables.Dispose();
                    }

                    base.Dispose(disposing);
                }

                private IEnumerable<IDisposable> SetupBindings()
                {
                    var viewChanges =
                        this.WhenAnyValue(x => x!.Content)
                            .WhereNotNull()
                            .OfType<Control>()
                            .Subscribe(x =>
                            {
                                // change the view in the ui
                                SuspendLayout();

                                // clear out existing visible control view
                                foreach (Control? c in Controls)
                                {
                                    c?.Dispose();
                                    Controls.Remove(c);
                                }

                                x!.Dock = DockStyle.Fill;
                                Controls.Add(x);
                                ResumeLayout();
                            });

                    yield return viewChanges!;

                    yield return this.WhenAnyValue(x => x.DefaultContent).Subscribe(x =>
                    {
                        if (x is not null)
                        {
                            Content = DefaultContent;
                        }
                    });

                    ViewContractObservable = Observable.Return(string.Empty);

                    var vmAndContract =
                        this.WhenAnyValue(x => x.ViewModel)
                            .CombineLatest(
                                           this.WhenAnyObservable(x => x.ViewContractObservable!),
                                           (vm, contract) => new { ViewModel = vm, Contract = contract });

                    yield return vmAndContract.Subscribe(
                                                         x =>
                                                         {
                                                             // set content to default when viewmodel is null
                                                             if (ViewModel is null)
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
                                                                 var c = _content as IViewFor;

                                                                 if (c?.ViewModel is not null && c.ViewModel.GetType() == x.ViewModel!.GetType())
                                                                 {
                                                                     c.ViewModel = x.ViewModel;

                                                                     // return early here after setting the viewmodel
                                                                     // allowing the view to update it's bindings
                                                                     return;
                                                                 }
                                                             }

                                                             var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                                                             var view = viewLocator.ResolveView(x.ViewModel, x.Contract);
                                                             if (view is not null)
                                                             {
                                                                 view.ViewModel = x.ViewModel;
                                                                 Content = view;
                                                             }
                                                         },
                                                         RxApp.DefaultExceptionHandler!.OnNext);
                }
                """.Replace("####REPLACEME####", vmcInfo.ClassName);
            writer.WriteLine(body);
            writer.Indent--;
            writer.WriteLine(Token(SyntaxKind.CloseBraceToken));
            writer.Indent--;
            writer.WriteLine(Token(SyntaxKind.CloseBraceToken));
            writer.WriteLine(TriviaList(
                                Trivia(NullableDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true)),
                                Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true)))
                                .NormalizeWhitespace());

            var output = stringStream.ToString();
            return ParseCompilationUnit(output).NormalizeWhitespace();
        }
    }
}
