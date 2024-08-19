// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Input.Models;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class IViewForGenerator : IIncrementalGenerator
{
    private const string GeneratedCode = "global::System.CodeDom.Compiler.GeneratedCode";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource($"{AttributeDefinitions.IViewForAttributeType}.g.cs", SourceText.From(AttributeDefinitions.IViewForAttribute, Encoding.UTF8)));

        // Gather info for all annotated IViewFor Classes
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<IViewForInfo> Info)> iViewForInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.IViewForAttributeType,
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
                        if (symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.IViewForAttributeType, out var attributeData))
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
                    context.AddSource($"{item.Hierarchy.FilenameHint}.IViewFor.g.cs", Execute.GetIViewForWpfWinUiUno(item.Info.Value));
                    break;
                case IViewForBaseType.WinForms:
                    context.AddSource($"{item.Hierarchy.FilenameHint}.IViewFor.g.cs", Execute.GetIViewForWinForms(item.Info.Value));
                    break;
                case IViewForBaseType.Avalonia:
                    context.AddSource($"{item.Hierarchy.FilenameHint}.IViewFor.g.cs", Execute.GetIViewForAvalonia(item.Info.Value));
                    break;
                case IViewForBaseType.Maui:
                    context.AddSource($"{item.Hierarchy.FilenameHint}.IViewFor.g.cs", Execute.GetIViewForMaui(item.Info.Value));
                    break;
            }
        });
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
