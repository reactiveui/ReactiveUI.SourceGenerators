// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports an <c>[ObservableAsProperty]</c> the generator writes nothing for (RXUIBIND018), and a method marked with it
/// that takes parameters (RXUIBIND019).
/// </summary>
/// <remarks>
/// The generator only writes the body of a partial get-only instance property. ReactiveUI's older source generator also
/// accepted a field, a method and an observable property; the diagnostic for each of those carries the form in its
/// properties, so the code fix knows how to rewrite it.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ObservableAsPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic property naming the form the code fix rewrites.</summary>
    internal const string FormProperty = "Form";

    /// <summary>The form of a field, whose value the property takes over.</summary>
    internal const string FieldForm = "Field";

    /// <summary>The form of a method returning the observable the property is built from.</summary>
    internal const string MethodForm = "Method";

    /// <summary>The form of a property returning the observable the property is built from.</summary>
    internal const string ObservablePropertyForm = "ObservableProperty";

    /// <summary>The attribute's metadata name in the lean runtime.</summary>
    internal const string LeanAttributeName = "ReactiveUI.Binding.ObservableAsPropertyAttribute";

    /// <summary>The attribute's metadata name in the System.Reactive runtime.</summary>
    internal const string ReactiveAttributeName = "ReactiveUI.Binding.Reactive.ObservableAsPropertyAttribute";

    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.ObservableAsPropertyNeedsPartialProperty, DiagnosticWarnings.ObservableAsPropertyMethodHasParameters }.ToImmutableArray();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ReportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            var lean = start.Compilation.GetTypeByMetadataName(LeanAttributeName);
            var reactive = start.Compilation.GetTypeByMetadataName(ReactiveAttributeName);
            if (lean is null && reactive is null)
            {
                return;
            }

            var observable = start.Compilation.GetTypeByMetadataName("System.IObservable`1");
            start.RegisterSymbolAction(
                symbolContext => AnalyzeSymbol(in symbolContext, lean, reactive, observable),
                SymbolKind.Field,
                SymbolKind.Method,
                SymbolKind.Property);
        });
    }

    /// <summary>Finds the <c>[ObservableAsProperty]</c> on a member, in either runtime flavour.</summary>
    /// <param name="symbol">The member.</param>
    /// <param name="lean">The lean runtime's attribute type, or null when it is not referenced.</param>
    /// <param name="reactive">The System.Reactive runtime's attribute type, or null when it is not referenced.</param>
    /// <returns>The attribute, or null when the member has none.</returns>
    internal static AttributeData? FindAttribute(ISymbol symbol, INamedTypeSymbol? lean, INamedTypeSymbol? reactive)
    {
        var attributes = symbol.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            var type = attributes[i].AttributeClass;
            if (SymbolEqualityComparer.Default.Equals(type, lean) || SymbolEqualityComparer.Default.Equals(type, reactive))
            {
                return attributes[i];
            }
        }

        return null;
    }

    /// <summary>Returns the value type of an <c>IObservable&lt;T&gt;</c>.</summary>
    /// <param name="type">The type to check.</param>
    /// <param name="observable">The open <c>IObservable&lt;T&gt;</c>, or null when it does not resolve.</param>
    /// <returns>The type argument, or null when the type is not an <c>IObservable&lt;T&gt;</c>.</returns>
    internal static ITypeSymbol? ObservedType(ITypeSymbol type, INamedTypeSymbol? observable) =>
        type is INamedTypeSymbol { TypeArguments: [var argument] } named
        && SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, observable)
            ? argument
            : null;

    /// <summary>Checks whether a property is declared the way the generator implements: a partial get-only instance property.</summary>
    /// <param name="property">The property.</param>
    /// <returns><see langword="true"/> when the generator writes its body.</returns>
    internal static bool IsGeneratedForm(IPropertySymbol property)
    {
        if (property.IsStatic)
        {
            return false;
        }

        var references = property.DeclaringSyntaxReferences;
        for (var i = 0; i < references.Length; i++)
        {
            if (references[i].GetSyntax() is PropertyDeclarationSyntax
                {
                    ExpressionBody: null,
                    AccessorList.Accessors: [{ RawKind: (int)SyntaxKind.GetAccessorDeclaration, Body: null, ExpressionBody: null }],
                } declaration
                && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reports a marked member the generator writes nothing for.</summary>
    /// <param name="context">The symbol analysis context.</param>
    /// <param name="lean">The lean runtime's attribute type, or null when it is not referenced.</param>
    /// <param name="reactive">The System.Reactive runtime's attribute type, or null when it is not referenced.</param>
    /// <param name="observable">The open <c>IObservable&lt;T&gt;</c>, or null when it does not resolve.</param>
    private static void AnalyzeSymbol(in SymbolAnalysisContext context, INamedTypeSymbol? lean, INamedTypeSymbol? reactive, INamedTypeSymbol? observable)
    {
        var symbol = context.Symbol;
        if (FindAttribute(symbol, lean, reactive) is null)
        {
            return;
        }

        string? form;
        switch (symbol)
        {
            case IPropertySymbol property when IsGeneratedForm(property):
            {
                return;
            }

            case IPropertySymbol property:
            {
                form = !property.IsStatic && ObservedType(property.Type, observable) is not null ? ObservablePropertyForm : null;
                break;
            }

            case IMethodSymbol { Parameters.Length: > 0 }:
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticWarnings.ObservableAsPropertyMethodHasParameters, symbol.Locations[0], symbol.Name));
                return;
            }

            case IMethodSymbol method:
            {
                form = !method.IsStatic && ObservedType(method.ReturnType, observable) is not null ? MethodForm : null;
                break;
            }

            default:
            {
                form = symbol.IsStatic ? null : FieldForm;
                break;
            }
        }

        var properties = form is null
            ? ImmutableDictionary<string, string?>.Empty
            : ImmutableDictionary<string, string?>.Empty.Add(FormProperty, form);
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticWarnings.ObservableAsPropertyNeedsPartialProperty, symbol.Locations[0], properties, symbol.Name));
    }
}
