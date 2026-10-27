// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts <see cref="ViewRegistrationInfo"/> from class declarations implementing <c>IViewFor&lt;T&gt;</c>.</summary>
/// <remarks>
/// Both runtime flavours are recognised: the lean package's <c>ReactiveUI.Binding.IViewFor&lt;T&gt;</c> and the
/// System.Reactive package's <c>ReactiveUI.Binding.Reactive.IViewFor&lt;T&gt;</c>. The view attributes are read from the
/// same flavour as the interface the view implements.
/// </remarks>
internal static class ViewRegistrationExtractor
{
    /// <summary>The short name of the attribute that leaves a view out of registration.</summary>
    private const string ExcludeAttributeName = "ExcludeFromViewRegistrationAttribute";

    /// <summary>The short name of the attribute that caches one instance of a view.</summary>
    private const string SingleInstanceAttributeName = "SingleInstanceViewAttribute";

    /// <summary>The short name of the attribute that registers a view under a contract.</summary>
    private const string ContractAttributeName = "ViewContractAttribute";

    /// <summary>The <c>RegistrationType</c> values that keep one instance: <c>LazySingleton</c> and <c>Constant</c>.</summary>
    private const int LazySingletonRegistration = 1;

    /// <summary>The <c>RegistrationType</c> value that creates the instance up front, which is resolved lazily here.</summary>
    private const int ConstantRegistration = 2;

    /// <summary>Extracts a <see cref="ViewRegistrationInfo"/> from a class that implements <c>IViewFor&lt;T&gt;</c>.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="ViewRegistrationInfo"/> if the class implements <c>IViewFor&lt;T&gt;</c>; otherwise, <see langword="null"/>.</returns>
    internal static ViewRegistrationInfo? ExtractFromIViewForImplementation(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDecl, ct) is not INamedTypeSymbol typeSymbol
            || typeSymbol.IsAbstract
            || !CanBeNamed(typeSymbol, semanticModel.Compilation))
        {
            return null;
        }

        var compilation = semanticModel.Compilation;
        var lean = compilation.GetTypeByMetadataName(Constants.IViewForGenericMetadataName);
        var reactive = compilation.GetTypeByMetadataName(Constants.ReactiveIViewForGenericMetadataName);
        if (lean is null && reactive is null)
        {
            return null;
        }

        // Walk AllInterfaces to find IViewFor<T> in either flavour.
        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var iface = allInterfaces[i];
            if (iface is { IsGenericType: true, TypeArguments.Length: 1 }
                && ViewNamespaceOf(iface.OriginalDefinition, lean, reactive) is { } prefix)
            {
                return Describe(typeSymbol, iface.TypeArguments[0], prefix, compilation, isSingleInstance: false, isDeclaredByAttribute: false);
            }
        }

        return null;
    }

    /// <summary>Extracts a <see cref="ViewRegistrationInfo"/> from a class marked with ReactiveUI.SourceGenerators' <c>[IViewFor]</c>.</summary>
    /// <param name="context">The attribute syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The registration, or <see langword="null"/> when the view model cannot be resolved or named.</returns>
    /// <remarks>
    /// That generator adds <c>IViewFor&lt;T&gt;</c> to the class in output no other generator can see, so the view is found
    /// from the attribute instead. The attribute is declared in each consumer's assembly, so it is matched by metadata
    /// name alone. A <c>RegistrationType</c> of <c>LazySingleton</c> or <c>Constant</c> keeps one instance, created on first
    /// resolution.
    /// </remarks>
    internal static ViewRegistrationInfo? ExtractFromIViewForAttribute(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol { IsAbstract: false } typeSymbol
            || !CanBeNamed(typeSymbol, context.SemanticModel.Compilation))
        {
            return null;
        }

        var compilation = context.SemanticModel.Compilation;
        var attribute = context.Attributes[0];
        return ReferencedViewNamespace(compilation) is { } prefix
            && ResolveViewModel(attribute, context.SemanticModel, context.TargetNode.SpanStart, ct) is { } viewModel
            && ExtractorValidation.IsReachableFromGeneratedCode(viewModel, compilation)
            ? Describe(typeSymbol, viewModel, prefix, compilation, KeepsOneInstance(attribute), isDeclaredByAttribute: true)
            : null;
    }

    /// <summary>Resolves the view model an <c>[IViewFor]</c> names, by type argument or by the type name it was given.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="semanticModel">The semantic model of the class declaration.</param>
    /// <param name="position">Where the class is declared, so the name binds with that file's usings and namespace.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The view model type, or <see langword="null"/> when the name is empty or does not bind.</returns>
    internal static ITypeSymbol? ResolveViewModel(AttributeData attribute, SemanticModel semanticModel, int position, CancellationToken ct)
    {
        if (attribute.AttributeClass is { IsGenericType: true, TypeArguments: [var typeArgument] })
        {
            return typeArgument;
        }

        if (attribute.ConstructorArguments is not [{ Value: string name }] || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var typeSyntax = SyntaxFactory.ParseTypeName(name);
        var type = semanticModel.GetSpeculativeTypeInfo(position, typeSyntax, SpeculativeBindingOption.BindAsTypeOrNamespace).Type;
        ct.ThrowIfCancellationRequested();
        return type is null or IErrorTypeSymbol ? null : type;
    }

    /// <summary>Checks whether an <c>[IViewFor]</c>'s <c>RegistrationType</c> keeps one instance of the view.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns><see langword="true"/> for <c>LazySingleton</c> or <c>Constant</c>.</returns>
    internal static bool KeepsOneInstance(AttributeData attribute)
    {
        var arguments = attribute.NamedArguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            // An enum argument arrives as its underlying value.
            if (arguments[i].Key == "RegistrationType")
            {
                return arguments[i].Value.Value is LazySingletonRegistration or ConstantRegistration;
            }
        }

        return false;
    }

    /// <summary>Names the flavour an <c>IViewFor&lt;T&gt;</c> definition belongs to.</summary>
    /// <param name="definition">The interface's open definition.</param>
    /// <param name="lean">The lean runtime's <c>IViewFor&lt;T&gt;</c>, or null when it is not referenced.</param>
    /// <param name="reactive">The System.Reactive runtime's <c>IViewFor&lt;T&gt;</c>, or null when it is not referenced.</param>
    /// <returns>The flavour's view namespace with the trailing dot, or null when the interface is neither.</returns>
    private static string? ViewNamespaceOf(INamedTypeSymbol definition, INamedTypeSymbol? lean, INamedTypeSymbol? reactive)
    {
        if (SymbolEqualityComparer.Default.Equals(definition, lean))
        {
            return Constants.LeanViewNamespacePrefix;
        }

        return SymbolEqualityComparer.Default.Equals(definition, reactive) ? Constants.ReactiveViewNamespacePrefix : null;
    }

    /// <summary>Names the flavour of the runtime the compilation references.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The flavour's view namespace with the trailing dot, or null when neither runtime is referenced.</returns>
    private static string? ReferencedViewNamespace(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(Constants.IViewForGenericMetadataName) is not null)
        {
            return Constants.LeanViewNamespacePrefix;
        }

        return compilation.GetTypeByMetadataName(Constants.ReactiveIViewForGenericMetadataName) is not null
            ? Constants.ReactiveViewNamespacePrefix
            : null;
    }

    /// <summary>Builds the registration for a view, reading the view attributes of the flavour it belongs to.</summary>
    /// <param name="view">The view type.</param>
    /// <param name="viewModel">The view model type.</param>
    /// <param name="prefix">The namespace of the flavour's view types, with the trailing dot.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="isSingleInstance">Whether the view keeps one instance regardless of <c>[SingleInstanceView]</c>.</param>
    /// <param name="isDeclaredByAttribute">Whether the view was found through <c>[IViewFor]</c>.</param>
    /// <returns>The registration, or <see langword="null"/> when the view is marked <c>[ExcludeFromViewRegistration]</c>.</returns>
    private static ViewRegistrationInfo? Describe(
        INamedTypeSymbol view,
        ITypeSymbol viewModel,
        string prefix,
        Compilation compilation,
        bool isSingleInstance,
        bool isDeclaredByAttribute)
    {
        if (FindAttribute(view, prefix + ExcludeAttributeName, compilation) is not null)
        {
            return null;
        }

        var contract = FindAttribute(view, prefix + ContractAttributeName, compilation) is { ConstructorArguments: [{ Value: string name }] }
            ? name
            : null;

        return new(
            viewModel.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            view.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            HasAccessibleParameterlessConstructor(view),
            contract,
            isSingleInstance || FindAttribute(view, prefix + SingleInstanceAttributeName, compilation) is not null,
            isDeclaredByAttribute);
    }

    /// <summary>Checks whether a generated resolver can name the view type.</summary>
    /// <param name="type">The type to check.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when the type is reachable and closed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The resolver lives in a class of its own, so a private or protected nested view is out of its reach and is
    /// left to the runtime locator. Its view model is at least as accessible, or the view could not implement
    /// <c>IViewFor&lt;T&gt;</c> of it.
    /// </remarks>
    private static bool CanBeNamed(INamedTypeSymbol type, Compilation compilation) =>
        !IsOpenGeneric(type) && ExtractorValidation.IsReachableFromGeneratedCode(type, compilation);

    /// <summary>Checks whether the type, or a type it is nested in, still has type parameters to close.</summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> when the type cannot be named without type arguments; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// A generated resolver names the view and constructs it, neither of which an open generic type allows.
    /// The closed subclasses that derive from it are declared classes of their own and carry the closed
    /// <c>IViewFor&lt;T&gt;</c> through their interface list, so each is registered in its own right.
    /// </remarks>
    private static bool IsOpenGeneric(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (!current.TypeParameters.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds an attribute of the given type on the view.</summary>
    /// <param name="type">The type to check.</param>
    /// <param name="attributeMetadataName">The metadata name of the attribute.</param>
    /// <param name="compilation">The compilation for symbol resolution.</param>
    /// <returns>The attribute, or <see langword="null"/> when it is absent.</returns>
    /// <remarks>The attribute types live beside the flavour's <c>IViewFor&lt;T&gt;</c>, so they resolve whenever the flavour does.</remarks>
    private static AttributeData? FindAttribute(INamedTypeSymbol type, string attributeMetadataName, Compilation compilation)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(attributeMetadataName);
        var attributes = type.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(attributes[i].AttributeClass, attributeSymbol))
            {
                return attributes[i];
            }
        }

        return null;
    }

    /// <summary>Checks whether the type has an accessible parameterless constructor (public or internal).</summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if a parameterless constructor is accessible; otherwise, <see langword="false"/>.</returns>
    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol type)
    {
        var constructors = type.InstanceConstructors;
        for (var i = 0; i < constructors.Length; i++)
        {
            var ctor = constructors[i];
            if (ctor.Parameters.IsEmpty
                && (ctor.DeclaredAccessibility == Accessibility.Public
                    || ctor.DeclaredAccessibility == Accessibility.Internal))
            {
                return true;
            }
        }

        return false;
    }
}
