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
public partial class RoutedControlHostGenerator
{
    internal static class Execute
    {
        internal static CompilationUnitSyntax GetRoutedControlHost(RoutedControlHostInfo vmcInfo)
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
                                SimpleBaseType(IdentifierName("IReactiveObject"))
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
                private RoutingState? _router;
                private Control? _defaultContent;
                private IObservable<string>? _viewContractObservable;

                /// <summary>
                /// Initializes a new instance of the <see cref="####REPLACEME####"/> class.
                /// </summary>
                public ####REPLACEME####()
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

                    ViewContractObservable = Observable<string>.Default;

                    var vmAndContract =
                        this.WhenAnyObservable(x => x.Router!.CurrentViewModel!)
                            .CombineLatest(
                                           this.WhenAnyObservable(x => x.ViewContractObservable!),
                                           (vm, contract) => new { ViewModel = vm, Contract = contract });

                    Control? viewLastAdded = null;
                    _disposables.Add(vmAndContract.Subscribe(
                                                             x =>
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
                                                             },
                                                             RxApp.DefaultExceptionHandler!.OnNext));
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
                public Control? DefaultContent
                {
                    get => _defaultContent;
                    set => this.RaiseAndSetIfChanged(ref _defaultContent, value);
                }

                /// <summary>
                /// Gets or sets the <see cref="RoutingState"/> of the view model stack.
                /// </summary>
                [Category("ReactiveUI")]
                [Description("The router.")]
                public RoutingState? Router
                {
                    get => _router;
                    set => this.RaiseAndSetIfChanged(ref _router, value);
                }

                /// <summary>
                /// Gets or sets the view contract observable.
                /// </summary>
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

                private static Control InitView(Control view)
                {
                    view.Dock = DockStyle.Fill;
                    return view;
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
