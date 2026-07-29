// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.CodeFixers.Extensions;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>Reports properties that can be converted to ReactiveUI reactive fields.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PropertyToReactiveFieldAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Gets the supported diagnostics.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(PropertyToReactiveFieldRule);

    /// <summary>Gets the type names excluded from property conversion.</summary>
    private static IReadOnlyList<string> IgnoredTypeNames { get; } = ["ReactiveCommand", "ReactiveProperty", "ViewModelActivator"];

    /// <summary>Initializes this analyzer.</summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static nodeContext => AnalyzeNode(in nodeContext), SyntaxKind.PropertyDeclaration);
    }

    /// <summary>Analyzes a property declaration.</summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeNode(in SyntaxNodeAnalysisContext context)
    {
        if (context.ContainingSymbol is not IPropertySymbol propertySymbol
            || context.Node is not PropertyDeclarationSyntax propertyDeclaration
            || !IsCandidate(propertySymbol, propertyDeclaration))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(PropertyToReactiveFieldRule, propertyDeclaration.GetLocation()));
    }

    /// <summary>Determines whether a property declaration is suitable for conversion.</summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when the property can be converted.</returns>
    private static bool IsCandidate(IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertyDeclaration) =>
        IsReactiveType(propertySymbol, propertyDeclaration)
        && propertySymbol.SetMethod is not null
        && !HasReactiveAttribute(propertySymbol, propertyDeclaration)
        && IsPublicInstanceAutoProperty(propertyDeclaration)
        && !HasRestrictedSetter(propertyDeclaration)
        && !HasIgnoredTypeName(propertyDeclaration);

    /// <summary>Determines whether a property belongs to a ReactiveUI-compatible type.</summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when the containing type is ReactiveUI-compatible.</returns>
    private static bool IsReactiveType(IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertyDeclaration) =>
        propertySymbol.IsTargetTypeValid() || HasReactiveBaseOrInterface(propertyDeclaration);

    /// <summary>Determines whether a type declaration explicitly names a ReactiveUI base type or interface.</summary>
    /// <param name="propertyDeclaration">The property declaration.</param>
    /// <returns><see langword="true"/> when a ReactiveUI base type or interface is declared.</returns>
    private static bool HasReactiveBaseOrInterface(PropertyDeclarationSyntax propertyDeclaration)
    {
        var baseTypes = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>()?.BaseList?.Types;
        if (baseTypes is null)
        {
            return false;
        }

        foreach (var baseType in baseTypes)
        {
            if (IsReactiveTypeName(baseType.Type))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type syntax names a ReactiveUI base type or interface.</summary>
    /// <param name="typeSyntax">The type syntax to inspect.</param>
    /// <returns><see langword="true"/> when the type syntax has a recognized ReactiveUI name.</returns>
    private static bool IsReactiveTypeName(TypeSyntax typeSyntax)
    {
        var typeName = typeSyntax switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
            _ => null
        };

        return typeName is "ReactiveObject" or "IReactiveObject";
    }

    /// <summary>Determines whether a property has a ReactiveUI attribute.</summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when the property has a ReactiveUI attribute.</returns>
    private static bool HasReactiveAttribute(IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertyDeclaration) =>
        HasSemanticReactiveAttribute(propertySymbol) || HasReactiveSyntaxAttribute(propertyDeclaration);

    /// <summary>Determines whether a property symbol has a ReactiveUI attribute.</summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <returns><see langword="true"/> when the property has a ReactiveUI attribute.</returns>
    private static bool HasSemanticReactiveAttribute(IPropertySymbol propertySymbol)
    {
        foreach (var attribute in propertySymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is "ReactiveAttribute" or "ObservableAsProperty")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether property syntax has a ReactiveUI attribute.</summary>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when the property syntax has a ReactiveUI attribute.</returns>
    private static bool HasReactiveSyntaxAttribute(PropertyDeclarationSyntax propertyDeclaration)
    {
        foreach (var attributeList in propertyDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (attribute.Name is IdentifierNameSyntax { Identifier.ValueText: "Reactive" }
                    or QualifiedNameSyntax { Right.Identifier.ValueText: "Reactive" })
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Determines whether a declaration is a public instance auto-property.</summary>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when the declaration is a public instance auto-property.</returns>
    private static bool IsPublicInstanceAutoProperty(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.ExpressionBody is not null
            || !propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
            || propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return false;
        }

        if (propertyDeclaration.AccessorList is null)
        {
            return true;
        }

        foreach (var accessor in propertyDeclaration.AccessorList.Accessors)
        {
            if (accessor.Body is not null || accessor.ExpressionBody is not null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether an accessor has a private or internal setter.</summary>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when a setter has restricted visibility.</returns>
    private static bool HasRestrictedSetter(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.AccessorList is null)
        {
            return false;
        }

        foreach (var accessor in propertyDeclaration.AccessorList.Accessors)
        {
            if (accessor.Modifiers.Any(SyntaxKind.PrivateKeyword)
                || accessor.Modifiers.Any(SyntaxKind.InternalKeyword))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a property has a type name excluded from conversion.</summary>
    /// <param name="propertyDeclaration">The property declaration syntax.</param>
    /// <returns><see langword="true"/> when the property type is excluded from conversion.</returns>
    private static bool HasIgnoredTypeName(PropertyDeclarationSyntax propertyDeclaration)
    {
        var typeName = propertyDeclaration.Type.ToString();
        foreach (var ignoredName in IgnoredTypeNames)
        {
            if (typeName.Contains(ignoredName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
