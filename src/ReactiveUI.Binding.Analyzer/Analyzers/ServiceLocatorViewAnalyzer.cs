// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>Reports a view registered only in the service locator, which <c>ResolveView(object)</c> does not ask (RXUIBIND020).</summary>
/// <remarks>
/// <para>
/// <c>ResolveView</c> with a view model held as an object asks the generated view lookup, then the <c>Map</c>
/// registrations. A view model with a generated view is still found through the service locator, because its generated
/// resolver asks the service locator first. So a registration is reported only when this project has no generated view
/// and no <c>Map</c> for the view model.
/// </para>
/// <para>
/// Registrations, views and mappings can each be in any file, so they are collected as the compilation is analysed and
/// compared when it ends. A registration or mapping made in another assembly, or through a container, is not seen, so
/// the diagnostic is information rather than a warning.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServiceLocatorViewAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The metadata name of ReactiveUI.SourceGenerators' generic <c>[IViewFor&lt;T&gt;]</c>, which also generates a view.</summary>
    private const string SourceGeneratorsViewAttribute = "ReactiveUI.SourceGenerators.IViewForAttribute`1";

    /// <summary>The short name of the attribute that leaves a view out of the generated lookup.</summary>
    private const string ExcludeAttributeName = "ExcludeFromViewRegistrationAttribute";

    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.ServiceLocatorOnlyView }.ToImmutableArray();

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
            var lean = start.Compilation.GetTypeByMetadataName(Constants.IViewForGenericMetadataName);
            var reactive = start.Compilation.GetTypeByMetadataName(Constants.ReactiveIViewForGenericMetadataName);
            if (lean is null && reactive is null)
            {
                return;
            }

            var state = new ViewRegistrations(lean, reactive, start.Compilation.GetTypeByMetadataName(SourceGeneratorsViewAttribute))
            {
                LeanExclude = start.Compilation.GetTypeByMetadataName(Constants.LeanViewNamespacePrefix + ExcludeAttributeName),
                ReactiveExclude = start.Compilation.GetTypeByMetadataName(Constants.ReactiveViewNamespacePrefix + ExcludeAttributeName),
            };
            start.RegisterSymbolAction(symbolContext => state.CollectView((INamedTypeSymbol)symbolContext.Symbol), SymbolKind.NamedType);
            start.RegisterOperationAction(operationContext => state.CollectInvocation((IInvocationOperation)operationContext.Operation), OperationKind.Invocation);
            start.RegisterCompilationEndAction(state.Report);
        });
    }

    /// <summary>What one compilation registers, generates and maps, collected from every file.</summary>
    /// <param name="lean">The lean runtime's <c>IViewFor&lt;T&gt;</c>, or null when it is not referenced.</param>
    /// <param name="reactive">The System.Reactive runtime's <c>IViewFor&lt;T&gt;</c>, or null when it is not referenced.</param>
    /// <param name="viewAttribute">ReactiveUI.SourceGenerators' <c>[IViewFor&lt;T&gt;]</c>, or null when it is not declared.</param>
    private sealed class ViewRegistrations(INamedTypeSymbol? lean, INamedTypeSymbol? reactive, INamedTypeSymbol? viewAttribute)
    {
        /// <summary>The service locator methods that register a service by its type argument or a <c>typeof</c> argument.</summary>
        private static readonly string[] RegistrationMethods = ["Register", "RegisterLazySingleton", "RegisterConstant"];

        /// <summary>The view models that have a generated view or a mapping.</summary>
        private readonly ConcurrentDictionary<ITypeSymbol, bool> _covered = new(SymbolEqualityComparer.Default);

        /// <summary>The service locator registrations of a view, with the view model each is for.</summary>
        private readonly ConcurrentQueue<(ITypeSymbol ViewModel, Location Location)> _registrations = new();

        /// <summary>Gets the lean runtime's <c>[ExcludeFromViewRegistration]</c>, or null when it is not referenced.</summary>
        public INamedTypeSymbol? LeanExclude { get; init; }

        /// <summary>Gets the System.Reactive runtime's <c>[ExcludeFromViewRegistration]</c>, or null when it is not referenced.</summary>
        public INamedTypeSymbol? ReactiveExclude { get; init; }

        /// <summary>Records the view model of a class the generator registers a view for.</summary>
        /// <param name="type">The declared type.</param>
        /// <remarks>A class marked <c>[ExcludeFromViewRegistration]</c> gets no generated view, so it is skipped.</remarks>
        public void CollectView(INamedTypeSymbol type)
        {
            if (type is not { TypeKind: TypeKind.Class, IsAbstract: false } || IsExcluded(type))
            {
                return;
            }

            foreach (var iface in type.AllInterfaces)
            {
                if (ViewModelOf(iface) is { } viewModel)
                {
                    _covered[viewModel] = true;
                }
            }

            foreach (var attribute in type.GetAttributes())
            {
                if (attribute.AttributeClass is { TypeArguments: [var viewModel] } attributeClass
                    && SymbolEqualityComparer.Default.Equals(attributeClass.OriginalDefinition, viewAttribute))
                {
                    _covered[viewModel] = true;
                }
            }
        }

        /// <summary>Records a <c>Map</c> of a view model, or a service locator registration of a view.</summary>
        /// <param name="invocation">The invocation.</param>
        public void CollectInvocation(IInvocationOperation invocation)
        {
            var method = invocation.TargetMethod;
            if (method.Name.StartsWith("Map", StringComparison.Ordinal) && method.TypeArguments is [var mapped, ..] && IsViewLocatorType(method.ContainingType))
            {
                _covered[mapped] = true;
            }
            else if (Array.IndexOf(RegistrationMethods, method.Name) >= 0 && method.ContainingNamespace.ToDisplayString() == "Splat")
            {
                CollectRegistration(invocation);
            }
        }

        /// <summary>Reports each registration whose view model this project neither generates a view for nor maps.</summary>
        /// <param name="context">The compilation end context.</param>
        public void Report(CompilationAnalysisContext context)
        {
            foreach (var (viewModel, location) in _registrations)
            {
                if (!_covered.ContainsKey(viewModel))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticWarnings.ServiceLocatorOnlyView,
                        location,
                        viewModel.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                }
            }
        }

        /// <summary>Checks whether a type is the view locator or its mapping builder, which both add mappings.</summary>
        /// <param name="type">The method's containing type.</param>
        /// <returns><see langword="true"/> for <c>DefaultViewLocator</c> or <c>ViewMappingBuilder</c> in either flavour.</returns>
        private static bool IsViewLocatorType(INamedTypeSymbol type) =>
            type.Name is "DefaultViewLocator" or "ViewMappingBuilder"
            && type.ContainingNamespace.ToDisplayString() is "ReactiveUI.Binding" or "ReactiveUI.Binding.Reactive";

        /// <summary>Records a service locator registration of a view, named by type argument or by a <c>typeof</c> argument.</summary>
        /// <param name="invocation">The registration call.</param>
        private void CollectRegistration(IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.TypeArguments is [var service] && ViewModelOf(service) is { } viewModel)
            {
                _registrations.Enqueue((viewModel, invocation.Syntax.GetLocation()));
                return;
            }

            foreach (var argument in invocation.Arguments)
            {
                if (argument.Value is ITypeOfOperation { TypeOperand: var operand } && ViewModelOf(operand) is { } registered)
                {
                    _registrations.Enqueue((registered, invocation.Syntax.GetLocation()));
                }
            }
        }

        /// <summary>Checks whether a view is marked <c>[ExcludeFromViewRegistration]</c> in either flavour.</summary>
        /// <param name="type">The view.</param>
        /// <returns><see langword="true"/> when the generator leaves it out.</returns>
        private bool IsExcluded(INamedTypeSymbol type)
        {
            foreach (var attribute in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, LeanExclude)
                    || SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, ReactiveExclude))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Returns the view model of an <c>IViewFor&lt;T&gt;</c> in either flavour.</summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The type argument, or null when the type is not an <c>IViewFor&lt;T&gt;</c>.</returns>
        private ITypeSymbol? ViewModelOf(ITypeSymbol type) =>
            type is INamedTypeSymbol { TypeArguments: [var viewModel] } named
            && (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, lean) || SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, reactive))
                ? viewModel
                : null;
    }
}
