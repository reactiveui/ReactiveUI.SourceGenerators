// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Input.Models;
using ReactiveUI.SourceGenerators.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class IViewForGenerator : IIncrementalGenerator
{
    private const string GeneratedCode = "global::System.CodeDom.Compiler.GeneratedCode";
    private const string IViewForAttribute = "ReactiveUI.SourceGenerators.IViewForAttribute";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<IViewForInfo> Info)> iViewForInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                IViewForAttribute,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    using var hierarchys = ImmutableArrayBuilder<HierarchyInfo>.Rent();
                    IViewForInfo iViewForInfo = default!;
                    HierarchyInfo hierarchy = default!;

                    if (context.TargetNode is ClassDeclarationSyntax declaredClass && declaredClass.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        token.ThrowIfCancellationRequested();
                        var compilation = context.SemanticModel.Compilation;
                        var semanticModel = compilation.GetSemanticModel(context.SemanticModel.SyntaxTree);
                        var symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, declaredClass, token)!;
                        if (symbol.TryGetAttributeWithFullyQualifiedMetadataName(IViewForAttribute, out var attributeData))
                        {
                            token.ThrowIfCancellationRequested();
                            var classSymbol = symbol as INamedTypeSymbol;
                            var classNamespace = classSymbol?.ContainingNamespace.ToString();
                            var className = declaredClass.Identifier.ValueText;
                            var constructorArgument = attributeData.GetConstructorArguments<string>().First();
                            if (constructorArgument is string viewModelTypeName)
                            {
                                token.ThrowIfCancellationRequested();
                                GatherForwardedAttributes(attributeData, semanticModel, declaredClass, token, out var classAttributesInfo);
                                token.ThrowIfCancellationRequested();

                                var viewForBaseType = IViewForBaseType.None;
                                if (classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Forms") == true)
                                {
                                    viewForBaseType = IViewForBaseType.WinForms;
                                }
                                else if (classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows") == true || classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Controls") == true)
                                {
                                    viewForBaseType = IViewForBaseType.Wpf;
                                }
                                else if (classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml") == true || classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml.Controls") == true)
                                {
                                    viewForBaseType = IViewForBaseType.WinUI;
                                }
                                else if (classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.Maui") == true)
                                {
                                    viewForBaseType = IViewForBaseType.Maui;
                                }
                                else if (classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("Avalonia") == true)
                                {
                                    viewForBaseType = IViewForBaseType.Avalonia;
                                }
                                else if (classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml") == true || classSymbol?.InheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml.Controls") == true)
                                {
                                    viewForBaseType = IViewForBaseType.Uno;
                                }

                                iViewForInfo = new IViewForInfo(
                                    classNamespace!,
                                    className,
                                    viewModelTypeName!,
                                    viewForBaseType,
                                    declaredClass,
                                    classAttributesInfo);

                                hierarchy = HierarchyInfo.From(classSymbol!);
                            }
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    ImmutableArray<DiagnosticInfo> diagnostics = default;
                    return (Hierarchy: hierarchy, new Result<IViewForInfo?>(iViewForInfo, diagnostics));
                })
            .Where(static item => item.Hierarchy is not null)!;

        ////// Output the diagnostics
        ////context.ReportDiagnostics(iViewForInfoWithErrors.Select(static (item, _) => item.Info.Errors));

        // Get the filtered sequence to enable caching
        var iViewForInfo =
            iViewForInfoWithErrors
            .Where(static item => item.Info.Value is not null)!;

        // Generate the requested properties and methods for IViewFor
        context.RegisterSourceOutput(iViewForInfo, static (context, item) =>
        {
            switch (item.Info.Value.BaseType)
            {
                case IViewForBaseType.None:
                    break;
                case IViewForBaseType.Wpf:
                case IViewForBaseType.WinUI:
                case IViewForBaseType.Uno:
                    context.AddSource($"{item.Hierarchy.FilenameHint}.IViewFor.g.cs", GetIViewForWpfWinUiUno(item.Info.Value));
                    break;
                case IViewForBaseType.WinForms:
                    context.AddSource($"{item.Hierarchy.FilenameHint}.IViewFor.g.cs", GetIViewForWinForms(item.Info.Value));
                    break;
                case IViewForBaseType.Avalonia:
                    break;
                case IViewForBaseType.Maui:
                    break;
            }
        });
    }

    private static CompilationUnitSyntax GetIViewForWpfWinUiUno(IViewForInfo iViewForInfo)
    {
        UsingDirectiveSyntax[] usings = [];
        if (iViewForInfo.BaseType == IViewForBaseType.Wpf)
        {
            usings =
                [
                    UsingDirective(ParseName("ReactiveUI")),
                    UsingDirective(ParseName("System.Windows")),
                ];
        }
        else if (iViewForInfo.BaseType == IViewForBaseType.WinUI)
        {
            usings =
                [
                    UsingDirective(ParseName("ReactiveUI")),
                    UsingDirective(ParseName("Microsoft.UI.Xaml")),
                ];
        }
        else if (iViewForInfo.BaseType == IViewForBaseType.Uno)
        {
            usings =
                [
                    UsingDirective(ParseName("ReactiveUI")),
                    UsingDirective(ParseName("Windows.UI.Xaml")),
                ];
        }

        var code = CompilationUnit().AddMembers(
                NamespaceDeclaration(IdentifierName(iViewForInfo.ClassNamespace))
                .WithLeadingTrivia(TriviaList(
                    Comment("// <auto-generated/>"),
                    Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))))
                .AddMembers(
                    ClassDeclaration(iViewForInfo.ClassName)
                    .AddBaseListTypes(
                        SimpleBaseType(
                            GenericName(Identifier("IViewFor"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(iViewForInfo.ViewModelTypeName))))))
                    .AddModifiers([.. iViewForInfo.DeclarationSyntax.Modifiers])
                    .AddAttributeLists(AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName(GeneratedCode))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(IViewForGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(IViewForGenerator).Assembly.GetName().Version.ToString())))))))))
            .WithUsings(List(usings))
            .NormalizeWhitespace().ToFullString();

        // Remove the last 4 characters to remove the closing brackets
        var baseCode = code.Remove(code.Length - 4);

        // Prepare all necessary type names with type arguments
        using var stringStream = new StringWriter();
        using var writer = new IndentedTextWriter(stringStream, "\t");
        writer.WriteLine(baseCode);
        writer.Indent++;
        writer.Indent++;

        // Add the necessary properties and methods for IViewFor.
        writer.WriteLine("/// <summary>");
        writer.WriteLine("/// The view model dependency property.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine("public static readonly DependencyProperty ViewModelProperty =");
        writer.Indent++;
        writer.WriteLine("DependencyProperty.Register(");
        writer.WriteLine("nameof(ViewModel),");
        writer.WriteLine($"typeof({iViewForInfo.ViewModelTypeName}),");
        writer.WriteLine($"typeof(IViewFor<{iViewForInfo.ViewModelTypeName}>),");
        writer.WriteLine("new PropertyMetadata(null));");
        writer.WriteLine();

        writer.Indent--;
        writer.WriteLine("/// <summary>");
        writer.WriteLine("/// Gets the binding root view model.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"public {iViewForInfo.ViewModelTypeName}? BindingRoot => ViewModel;");
        writer.WriteLine();

        writer.WriteLine("/// <inheritdoc/>");
        writer.WriteLine($"public {iViewForInfo.ViewModelTypeName}? ViewModel");
        writer.WriteLine(Token(SyntaxKind.OpenBraceToken));
        writer.Indent++;
        writer.WriteLine($"get => ({iViewForInfo.ViewModelTypeName}?)GetValue(ViewModelProperty);");
        writer.WriteLine("set => SetValue(ViewModelProperty, value);");
        writer.Indent--;
        writer.WriteLine(Token(SyntaxKind.CloseBraceToken));
        writer.WriteLine();

        writer.WriteLine("/// <inheritdoc/>");
        writer.WriteLine("object? IViewFor.ViewModel");
        writer.WriteLine(Token(SyntaxKind.OpenBraceToken));
        writer.Indent++;
        writer.WriteLine("get => ViewModel;");
        writer.WriteLine($"set => ViewModel = ({iViewForInfo.ViewModelTypeName}?)value;");
        writer.Indent--;
        writer.WriteLine(Token(SyntaxKind.CloseBraceToken));
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

    private static CompilationUnitSyntax GetIViewForWinForms(IViewForInfo iViewForInfo)
    {
        UsingDirectiveSyntax[] usings =
                [
                    UsingDirective(ParseName("ReactiveUI")),
                    UsingDirective(ParseName("System.ComponentModel")),
                ];

        var code = CompilationUnit().AddMembers(
                NamespaceDeclaration(IdentifierName(iViewForInfo.ClassNamespace))
                .WithLeadingTrivia(TriviaList(
                    Comment("// <auto-generated/>"),
                    Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))))
                .AddMembers(
                    ClassDeclaration(iViewForInfo.ClassName)
                    .AddBaseListTypes(
                        SimpleBaseType(
                            GenericName(Identifier("IViewFor"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(iViewForInfo.ViewModelTypeName))))))
                    .AddModifiers([.. iViewForInfo.DeclarationSyntax.Modifiers])
                    .AddAttributeLists(AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName(GeneratedCode))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(IViewForGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(IViewForGenerator).Assembly.GetName().Version.ToString())))))))))
            .WithUsings(List(usings))
            .NormalizeWhitespace().ToFullString();

        // Remove the last 4 characters to remove the closing brackets
        var baseCode = code.Remove(code.Length - 4);

        // Prepare all necessary type names with type arguments
        using var stringStream = new StringWriter();
        using var writer = new IndentedTextWriter(stringStream, "\t");
        writer.WriteLine(baseCode);
        writer.Indent++;
        writer.Indent++;

        // Add the necessary properties and methods for IViewFor.
        writer.WriteLine("/// <inheritdoc/>");
        writer.WriteLine("[Category(\"ReactiveUI\")]");
        writer.WriteLine("[Description(\"The ViewModel.\")]");
        writer.WriteLine("[Bindable(true)]");
        writer.WriteLine("[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]");
        writer.WriteLine($"public {iViewForInfo.ViewModelTypeName}? ViewModel " + "{ get; set; }");
        writer.WriteLine();

        writer.WriteLine("/// <inheritdoc/>");
        writer.WriteLine("object? IViewFor.ViewModel");
        writer.WriteLine(Token(SyntaxKind.OpenBraceToken));
        writer.Indent++;
        writer.WriteLine("get => ViewModel;");
        writer.WriteLine($"set => ViewModel = ({iViewForInfo.ViewModelTypeName}?)value;");
        writer.Indent--;
        writer.WriteLine(Token(SyntaxKind.CloseBraceToken));
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

    private static void GatherForwardedAttributes(
            AttributeData attributeData,
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken token,
            out ImmutableArray<AttributeInfo> classAttributesInfo)
    {
        using var classAttributesInfoBuilder = ImmutableArrayBuilder<AttributeInfo>.Rent();

        static void GatherForwardedAttributes(
            AttributeData attributeData,
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken token,
            ImmutableArrayBuilder<AttributeInfo> classAttributesInfo)
        {
            // Gather explicit forwarded attributes info
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (!semanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeTypeSymbol))
                    {
                        continue;
                    }

                    var attributeArguments = attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>();

                    // Try to extract the forwarded attribute
                    if (!AttributeInfo.TryCreate(attributeTypeSymbol, semanticModel, attributeArguments, token, out var attributeInfo))
                    {
                        continue;
                    }

                    var ignoreAttribute = attributeData.AttributeClass?.GetFullyQualifiedMetadataName();
                    if (attributeInfo.TypeName.Contains(ignoreAttribute))
                    {
                        continue;
                    }

                    // Add the new attribute info to the right builder
                    classAttributesInfo.Add(attributeInfo);
                }
            }
        }

        // If the method is not a partial definition/implementation, just gather attributes from the method with no modifications
        GatherForwardedAttributes(attributeData, semanticModel, classDeclaration, token, classAttributesInfoBuilder);

        classAttributesInfo = classAttributesInfoBuilder.ToImmutable();
    }
}
