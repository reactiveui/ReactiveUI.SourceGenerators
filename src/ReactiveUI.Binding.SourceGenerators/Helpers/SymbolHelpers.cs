// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Provides well-known symbol caching and type-checking helpers shared across all extractors.</summary>
internal static class SymbolHelpers
{
    /// <summary>The symbol cache for the compilation.</summary>
    private static readonly ConditionalWeakTable<Compilation, WellKnownSymbolsBox> SymbolCache = new();

    /// <summary>Gets the well-known symbols for a compilation.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The well-known symbols box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static WellKnownSymbolsBox GetWellKnownSymbols(Compilation compilation) =>
        SymbolCache.GetValue(
            compilation,
            static c => new()
            {
                INPC = c.GetTypeByMetadataName(Constants.INotifyPropertyChangedMetadataName),
                INPChanging = c.GetTypeByMetadataName(Constants.INotifyPropertyChangingMetadataName),
                IReactiveObject = c.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName),
                WpfDependencyObject = c.GetTypeByMetadataName(Constants.WpfDependencyObjectMetadataName),
                WinUIDependencyObject = c.GetTypeByMetadataName(Constants.WinUIDependencyObjectMetadataName),
                NSObject = ResolveKvoOwner(c),
                WinFormsComponent = c.GetTypeByMetadataName(Constants.WinFormsComponentMetadataName),
                AndroidView = c.GetTypeByMetadataName(Constants.AndroidViewMetadataName),
            });

    /// <summary>Resolves <c>NSObject</c> when the runtime's key-value observing observable is in reach.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The <c>NSObject</c> symbol, or null when either it or the runtime's KVO support is missing.</returns>
    /// <remarks>
    /// Key-value observing runs through the runtime's Apple heads. Without them there is nothing for a generated
    /// observation to reference, so no type is treated as observable through KVO.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static INamedTypeSymbol? ResolveKvoOwner(Compilation compilation) =>
        ResolvesEither(compilation, CodeGeneration.GeneratedTypeNames.KvoPropertyObservableMetadataName, CodeGeneration.GeneratedTypeNames.ReactiveKvoPropertyObservableMetadataName)
            ? compilation.GetTypeByMetadataName(Constants.NSObjectMetadataName)
            : null;

    /// <summary>Determines whether a compilation resolves a runtime type in either the lean or the System.Reactive flavour.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="leanMetadataName">The type's metadata name in the lean runtime.</param>
    /// <param name="reactiveMetadataName">The type's metadata name in the System.Reactive runtime.</param>
    /// <returns><see langword="true"/> when either flavour declares the type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ResolvesEither(Compilation compilation, string leanMetadataName, string reactiveMetadataName) =>
        compilation.GetTypeByMetadataName(leanMetadataName) is not null
        || compilation.GetTypeByMetadataName(reactiveMetadataName) is not null;

    /// <summary>
    /// Extracts the inner type T from a property whose type is IObservable&lt;T&gt; or IObservable&lt;T&gt;?.
    /// Falls back to the leaf property type if the IObservable unwrapping fails.
    /// </summary>
    /// <param name="leafSegment">The leaf property path segment.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="argExpression">The argument expression (the lambda).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fully qualified inner type T.</returns>
    internal static string ExtractInnerObservableType(
        Models.PropertyPathSegment leafSegment,
        SemanticModel semanticModel,
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax argExpression,
        CancellationToken ct)
    {
        // The leaf property type string looks like "global::System.IObservable<int>?"
        // We need to get the actual INamedTypeSymbol to extract the type argument.
        // Re-resolve the lambda body to get the actual property symbol.
        if (argExpression is Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax lambda)
        {
            var body = SyntaxHelpers.GetLambdaBody(lambda);
            if (body is not null)
            {
                body = SyntaxHelpers.UnwrapNullForgiving(body);
                if (body is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccess)
                {
                    var memberSymbol = semanticModel.GetSymbolInfo(memberAccess, ct).Symbol;
                    if (((memberSymbol as IPropertySymbol)?.Type ?? (memberSymbol as IFieldSymbol)?.Type) is INamedTypeSymbol namedType)
                    {
                        var observableType = TryGetObservableTypeArgument(namedType);
                        if (observableType is not null)
                        {
                            return observableType;
                        }
                    }
                }
            }
        }

        // Fallback: use the leaf type as-is
        return leafSegment.PropertyTypeFullName;
    }

    /// <summary>Checks if a named type symbol is IObservable&lt;T&gt;.</summary>
    /// <param name="type">The type symbol to check.</param>
    /// <returns>A value indicating whether the type is IObservable&lt;T&gt;.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsIObservable(INamedTypeSymbol type) =>
        NativeTypeIdentity.Matches(type, "System.IObservable`1");

    /// <summary>Checks if a type is IInteraction&lt;TInput, TOutput&gt;.</summary>
    /// <param name="type">The type symbol to check.</param>
    /// <returns>A value indicating whether the type is IInteraction&lt;TInput, TOutput&gt;.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInteractionType(INamedTypeSymbol type) =>
        NativeTypeIdentity.Matches(type, "ReactiveUI.Binding.IInteraction`2")
        || NativeTypeIdentity.Matches(type, $"{Constants.ReactiveRuntimeNamespace}.IInteraction`2");

    /// <summary>Extracts TInput and TOutput type arguments from a type that implements IInteraction&lt;TInput, TOutput&gt;.</summary>
    /// <param name="type">The type symbol.</param>
    /// <param name="inputType">The resulting TInput type name.</param>
    /// <param name="outputType">The resulting TOutput type name.</param>
    /// <returns>A value indicating whether the type arguments were extracted.</returns>
    internal static bool ExtractInteractionTypeArguments(ITypeSymbol type, out string inputType, out string outputType)
    {
        inputType = string.Empty;
        outputType = string.Empty;

        // Check direct implementation
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        if (IsInteractionType(namedType))
        {
            inputType = namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            outputType = namedType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return true;
        }

        // Check interfaces
        for (var i = 0; i < namedType.AllInterfaces.Length; i++)
        {
            var iface = namedType.AllInterfaces[i];
            if (!IsInteractionType(iface))
            {
                continue;
            }

            inputType = iface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            outputType = iface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Detects whether a method parameter is an <c>IBindingTypeConverter</c>-typed converter override.
    /// Extracted as a helper to allow direct unit testing of this branch.
    /// </summary>
    /// <param name="parameter">The parameter symbol to inspect.</param>
    /// <returns>
    /// <see langword="true"/> if the parameter has a converter-override name and is typed as
    /// <c>IBindingTypeConverter</c>; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool DetectHasConverterOverride(IParameterSymbol parameter) =>
        parameter.Name is "converter" or "sourceToTargetConverter" or "vmToViewConverter" or "viewModelToViewConverter"
        && parameter.Type.Name == "IBindingTypeConverter";

    /// <summary>Resolves a path's leaf property or field type to its INamedTypeSymbol using the semantic model.</summary>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="lambdaExpression">The lambda expression.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved named type symbol, or null if it could not be resolved.</returns>
    internal static INamedTypeSymbol? ResolveNamedType(
        SemanticModel semanticModel,
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax lambdaExpression,
        CancellationToken ct)
    {
        if (lambdaExpression is not Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax lambda)
        {
            return null;
        }

        var body = SyntaxHelpers.GetLambdaBody(lambda);

        if (body is null)
        {
            return null;
        }

        body = SyntaxHelpers.UnwrapNullForgiving(body);
        if (body is not Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        var memberSymbol = semanticModel.GetSymbolInfo(memberAccess, ct).Symbol;
        return memberSymbol switch
        {
            IPropertySymbol { Type: INamedTypeSymbol propertyType } => propertyType,
            IFieldSymbol { Type: INamedTypeSymbol fieldType } => fieldType,
            _ => null,
        };
    }

    /// <summary>
    /// Returns the fully qualified <c>T</c> of <c>IObservable&lt;T&gt;</c> for the given type,
    /// checking the type itself and all implemented interfaces.
    /// </summary>
    /// <param name="namedType">The candidate type symbol.</param>
    /// <returns>The fully qualified observable type argument, or null if the type is not observable.</returns>
    private static string? TryGetObservableTypeArgument(INamedTypeSymbol namedType)
    {
        // Check if it's IObservable<T> directly
        if (IsIObservable(namedType))
        {
            return namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        // Check interfaces for IObservable<T>
        for (var j = 0; j < namedType.AllInterfaces.Length; j++)
        {
            if (IsIObservable(namedType.AllInterfaces[j]))
            {
                return namedType.AllInterfaces[j].TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        return null;
    }

    /// <summary>
    /// Cached well-known symbol references for a compilation.
    /// Uses ConditionalWeakTable so the compilation can be garbage collected.
    /// </summary>
    internal sealed class WellKnownSymbolsBox
    {
        /// <summary>Gets or sets the INPC symbol.</summary>
        internal INamedTypeSymbol? INPC { get; set; }

        /// <summary>Gets or sets the INPChanging symbol.</summary>
        internal INamedTypeSymbol? INPChanging { get; set; }

        /// <summary>Gets or sets the IReactiveObject symbol.</summary>
        internal INamedTypeSymbol? IReactiveObject { get; set; }

        /// <summary>Gets or sets the WpfDependencyObject symbol.</summary>
        internal INamedTypeSymbol? WpfDependencyObject { get; set; }

        /// <summary>Gets or sets the WinUIDependencyObject symbol.</summary>
        internal INamedTypeSymbol? WinUIDependencyObject { get; set; }

        /// <summary>Gets or sets the NSObject symbol.</summary>
        internal INamedTypeSymbol? NSObject { get; set; }

        /// <summary>Gets or sets the WinFormsComponent symbol.</summary>
        internal INamedTypeSymbol? WinFormsComponent { get; set; }

        /// <summary>Gets or sets the AndroidView symbol.</summary>
        internal INamedTypeSymbol? AndroidView { get; set; }
    }
}
