// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>Shared helper methods for analyzers. No LINQ, manual loops.</summary>
internal static class AnalyzerHelpers
{
    /// <summary>The index <see cref="ObservedTypeArgumentIndex"/> returns for an API that observes none of its type arguments.</summary>
    internal const int NoObservedTypeArgument = -1;

    /// <summary>The suffix that marks the runtime-reflection twin of an API.</summary>
    internal const string UnsafeMethodSuffix = "Unsafe";

    /// <summary>The API names this package generates bindings for.</summary>
    internal static readonly ImmutableHashSet<string> GeneratedApiNames = new[]
    {
        SourceGenerators.Constants.WhenChangedMethodName,
        SourceGenerators.Constants.WhenChangingMethodName,
        SourceGenerators.Constants.WhenAnyMethodName,
        SourceGenerators.Constants.WhenAnyValueMethodName,
        SourceGenerators.Constants.WhenAnyObservableMethodName,
        SourceGenerators.Constants.BindOneWayMethodName,
        SourceGenerators.Constants.BindTwoWayMethodName,
        SourceGenerators.Constants.OneWayBindMethodName,
        SourceGenerators.Constants.BindMethodName,
        SourceGenerators.Constants.BindToMethodName,
        SourceGenerators.Constants.BindCommandMethodName,
        SourceGenerators.Constants.BindInteractionMethodName,
        SourceGenerators.Constants.InvokeCommandMethodName,
        SourceGenerators.Constants.ToPropertyMethodName,
    }.ToImmutableHashSet(StringComparer.Ordinal);

    /// <summary>The name of the argument that carries the view model of a view-first binding.</summary>
    private const string ViewModelParameterName = "viewModel";

    /// <summary>Checks if a method symbol belongs to our generated extension class.</summary>
    /// <param name="methodSymbol">The method symbol to check.</param>
    /// <returns>true if the method is from our generated extension class.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindingExtensionMethod(IMethodSymbol methodSymbol) =>
        methodSymbol.ContainingType?.Name is { } name
        && (name is SourceGenerators.Constants.GeneratedExtensionClassName or SourceGenerators.Constants.StubExtensionClassName
            || name.StartsWith($"{SourceGenerators.Constants.GeneratedExtensionClassName}_", StringComparison.Ordinal));

    /// <summary>Checks whether an API deliberately resolves notification providers at run time.</summary>
    /// <param name="methodSymbol">The binding method symbol.</param>
    /// <returns><see langword="true"/> for an Unsafe binding call.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsUnsafeBindingMethod(IMethodSymbol methodSymbol) =>
        methodSymbol.Name.EndsWith(UnsafeMethodSuffix, StringComparison.Ordinal);

    /// <summary>Checks if an expression is an inline lambda (not a variable reference or method call).</summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>true if the expression is an inline lambda.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInlineLambda(ExpressionSyntax expression) =>
        expression is SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax;

    /// <summary>
    /// Steps over the null-forgiving operator and parentheses, which wrap a link of a property path without
    /// changing which member it names: <c>(x.A)!</c> is read as <c>x.A</c>.
    /// </summary>
    /// <param name="expression">The expression to read through.</param>
    /// <returns>The first expression that is neither a null-forgiving suppression nor parenthesized.</returns>
    /// <remarks>
    /// Every walk over a lambda body reads its links through this, so an operator in the middle of a path
    /// cannot end one check early while another carries on. It reads a node's kind and allocates nothing.
    /// </remarks>
    internal static ExpressionSyntax SkipNullForgivingAndParentheses(ExpressionSyntax expression)
    {
        while (true)
        {
            var kind = expression.Kind();
            if (kind == SyntaxKind.ParenthesizedExpression)
            {
                expression = ((ParenthesizedExpressionSyntax)expression).Expression;
            }
            else if (kind == SyntaxKind.SuppressNullableWarningExpression)
            {
                expression = ((PostfixUnaryExpressionSyntax)expression).Operand;
            }
            else
            {
                return expression;
            }
        }
    }

    /// <summary>Checks if a type supports before-change notifications based on its notification mechanism.</summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <param name="mechanism">Output: the name of the detected mechanism.</param>
    /// <returns>true if the type supports before-change notifications.</returns>
    internal static bool HasBeforeChangeSupport(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        out string mechanism)
    {
        // IReactiveObject supports before-change via GetChangingObservable()
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.IReactiveObjectMetadataName, byInterface: true))
        {
            mechanism = "IReactiveObject";
            return true;
        }

        // INotifyPropertyChanging supports before-change
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.INotifyPropertyChangingMetadataName, byInterface: true))
        {
            mechanism = "INotifyPropertyChanging";
            return true;
        }

        // Platform types that DON'T support before-change
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.WpfDependencyObjectMetadataName, byInterface: false))
        {
            mechanism = "WPF DependencyObject";
            return false;
        }

        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.WinUIDependencyObjectMetadataName, byInterface: false))
        {
            mechanism = "WinUI DependencyObject";
            return false;
        }

        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.WinFormsComponentMetadataName, byInterface: false))
        {
            mechanism = "WinForms Component";
            return false;
        }

        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.AndroidViewMetadataName, byInterface: false))
        {
            mechanism = "Android View";
            return false;
        }

        // KVO (NSObject) supports before-change
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.NSObjectMetadataName, byInterface: false))
        {
            mechanism = "KVO";
            return true;
        }

        // INPC without INotifyPropertyChanging
        if (Matches(typeSymbol, compilation, SourceGenerators.Constants.INotifyPropertyChangedMetadataName, byInterface: true))
        {
            mechanism = "INotifyPropertyChanged (without INotifyPropertyChanging)";
            return false;
        }

        mechanism = "unknown";
        return false;
    }

    /// <summary>
    /// Extracts the first type argument from a method symbol as an <see cref="INamedTypeSymbol"/>.
    /// Returns null if the method has no type arguments or the first argument is not a named type.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to extract from.</param>
    /// <returns>The first type argument as <see cref="INamedTypeSymbol"/>, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static INamedTypeSymbol? ExtractFirstTypeArgument(IMethodSymbol methodSymbol) => ExtractTypeArgument(methodSymbol, 0);

    /// <summary>
    /// Extracts one of a method's type arguments as an <see cref="INamedTypeSymbol"/>. Returns null when the
    /// method has fewer arguments than that, or when the one asked for is not a named type.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to extract from.</param>
    /// <param name="index">Which type argument to read.</param>
    /// <returns>The type argument as <see cref="INamedTypeSymbol"/>, or null.</returns>
    /// <remarks>
    /// Which argument names the observed object differs by API: most name it first, while the ones taking a
    /// stream the caller already built name that stream's value type there instead.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static INamedTypeSymbol? ExtractTypeArgument(IMethodSymbol methodSymbol, int index) =>
        (uint)index >= (uint)methodSymbol.TypeArguments.Length ? null : methodSymbol.TypeArguments[index] as INamedTypeSymbol;

    /// <summary>
    /// Finds which type argument of a binding method names the object the method observes for property
    /// notifications. The <c>Unsafe</c> twin of an API answers as the API does.
    /// </summary>
    /// <param name="methodSymbol">The binding method symbol.</param>
    /// <returns>The index of that type argument, or <see cref="NoObservedTypeArgument"/> when the method observes none of them.</returns>
    /// <remarks>
    /// Most APIs name the observed object first. <c>BindTo</c> names the value type of a stream the caller already
    /// built first, and its target is only written to, so nothing is observed. <c>InvokeCommand</c> names the
    /// stream's value type first and the object holding the command second. <c>Bind</c> and <c>OneWayBind</c> name
    /// the view model by the type parameter behind their <c>viewModel</c> argument, which sits first on some
    /// overloads and second on others.
    /// </remarks>
    internal static int ObservedTypeArgumentIndex(IMethodSymbol methodSymbol)
    {
        var name = methodSymbol.Name;

        // ToProperty raises the source's notifications rather than observing them; RXUIBIND012 covers its source.
        if (IsApi(name, SourceGenerators.Constants.BindToMethodName) || IsApi(name, SourceGenerators.Constants.ToPropertyMethodName))
        {
            return NoObservedTypeArgument;
        }

        if (IsApi(name, SourceGenerators.Constants.InvokeCommandMethodName))
        {
            return 1;
        }

        var isViewFirst = IsApi(name, SourceGenerators.Constants.BindMethodName)
            || IsApi(name, SourceGenerators.Constants.OneWayBindMethodName);
        return isViewFirst
            ? ViewModelTypeArgumentIndex(methodSymbol)
            : 0;
    }

    /// <summary>
    /// Determines whether a method's first type argument lacks any observable notification mechanism.
    /// Returns <c>false</c> if the method has no type arguments (non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="sourceType">The resolved source type, if the check matched.</param>
    /// <returns><c>true</c> if the type has no observable mechanism.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool LacksObservableMechanism(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? sourceType) =>
        LacksObservableMechanism(methodSymbol, compilation, 0, out sourceType);

    /// <summary>
    /// Determines whether the type argument naming this API's observed object lacks any observable notification
    /// mechanism. Returns <c>false</c> when the method has no such argument (a non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="typeArgumentIndex">Which type argument names the observed object.</param>
    /// <param name="sourceType">The resolved source type, if the check matched.</param>
    /// <returns><c>true</c> if the type has no observable mechanism.</returns>
    internal static bool LacksObservableMechanism(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        int typeArgumentIndex,
        out INamedTypeSymbol? sourceType)
    {
        sourceType = ExtractTypeArgument(methodSymbol, typeArgumentIndex);
        return sourceType is not null && !HasObservableMechanism(sourceType, compilation);
    }

    /// <summary>
    /// Determines whether a method's first type argument lacks before-change notification support.
    /// Returns <c>false</c> if the method has no type arguments (non-generic dispatch overload).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="receiverType">The resolved receiver type, if the check matched.</param>
    /// <param name="mechanism">The detected notification mechanism name.</param>
    /// <returns><c>true</c> if the type does NOT support before-change notifications.</returns>
    internal static bool LacksBeforeChangeSupport(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? receiverType,
        out string mechanism)
    {
        mechanism = string.Empty;
        receiverType = ExtractFirstTypeArgument(methodSymbol);
        return receiverType is not null && !HasBeforeChangeSupport(receiverType, compilation, out mechanism);
    }

    /// <summary>
    /// Determines whether a binding method's source type implements
    /// <c>INotifyDataErrorInfo</c>, requiring the runtime validation engine.
    /// Returns <c>false</c> if the method has no type arguments or the
    /// <c>INotifyDataErrorInfo</c> type cannot be resolved in the compilation.
    /// </summary>
    /// <param name="methodSymbol">The binding method symbol.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="sourceType">The resolved source type, if the check passed.</param>
    /// <returns><c>true</c> if the source type implements <c>INotifyDataErrorInfo</c>.</returns>
    internal static bool ImplementsDataErrorInfo(
        IMethodSymbol methodSymbol,
        Compilation compilation,
        out INamedTypeSymbol? sourceType)
    {
        sourceType = ExtractFirstTypeArgument(methodSymbol);
        if (sourceType is null)
        {
            return false;
        }

        var dataErrorInfo =
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyDataErrorInfoMetadataName);
        return dataErrorInfo is not null && ImplementsInterface(sourceType, dataErrorInfo);
    }

    /// <summary>
    /// Determines whether a type notifies about property changes at all, through any of the mechanisms this
    /// library observes: INotifyPropertyChanged, IReactiveObject, a WPF or WinUI dependency object, an Apple
    /// NSObject, a WinForms component, or an Android view.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <returns><c>true</c> if the type supports property observation; otherwise, <c>false</c>.</returns>
    internal static bool HasObservableMechanism(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        INamedTypeSymbol?[] interfaces =
        [
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.INotifyPropertyChangedMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.IReactiveObjectMetadataName),
        ];

        for (var i = 0; i < interfaces.Length; i++)
        {
            if (interfaces[i] is { } observable && ImplementsInterface(typeSymbol, observable))
            {
                return true;
            }
        }

        INamedTypeSymbol?[] baseTypes =
        [
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.WpfDependencyObjectMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.WinUIDependencyObjectMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.NSObjectMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.WinFormsComponentMetadataName),
            compilation.GetTypeByMetadataName(SourceGenerators.Constants.AndroidViewMetadataName),
        ];

        for (var i = 0; i < baseTypes.Length; i++)
        {
            if (baseTypes[i] is { } observable && InheritsFromOrImplements(typeSymbol, observable))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type implements a specific interface.</summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="interfaceSymbol">The interface symbol to look for.</param>
    /// <returns><c>true</c> if the type implements the specified interface; otherwise, <c>false</c>.</returns>
    internal static bool ImplementsInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol)
    {
        var allInterfaces = typeSymbol.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(allInterfaces[i], interfaceSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type inherits from a specific base type.</summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="baseTypeSymbol">The base type symbol to look for in the inheritance hierarchy.</param>
    /// <returns><c>true</c> if the type inherits from the specified base type; otherwise, <c>false</c>.</returns>
    internal static bool InheritsFrom(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseTypeSymbol))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>Determines whether a generated interceptor claims a binding call.</summary>
    /// <param name="invocation">The call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when an interceptor replaces the call.</returns>
    internal static bool IsIntercepted(IInvocationOperation invocation, CancellationToken cancellationToken)
    {
#if ROSLYN_4_13
        // A binding method is only ever called through an invocation expression, and an operation handed to an
        // analyzer always carries the model it was bound with.
        return invocation.SemanticModel!.GetInterceptorMethod((InvocationExpressionSyntax)invocation.Syntax, cancellationToken) is not null;
#else

        // The baseline compiler has no interceptors, so no call is ever claimed.
        _ = invocation;
        _ = cancellationToken;
        return false;
#endif
    }

    /// <summary>Checks if a method is an API or its <c>Unsafe</c> twin.</summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="apiName">The name of the API.</param>
    /// <returns>true if the method is the API or its <c>Unsafe</c> twin.</returns>
    private static bool IsApi(string methodName, string apiName) =>
        methodName.StartsWith(apiName, StringComparison.Ordinal)
        && (methodName.Length == apiName.Length
            || (methodName.Length == apiName.Length + UnsafeMethodSuffix.Length
                && methodName.EndsWith(UnsafeMethodSuffix, StringComparison.Ordinal)));

    /// <summary>Finds the type argument behind a method's <c>viewModel</c> argument.</summary>
    /// <param name="methodSymbol">The binding method symbol.</param>
    /// <returns>The index of that type argument, or 0 when the method has no such argument.</returns>
    private static int ViewModelTypeArgumentIndex(IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.OriginalDefinition.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Name == ViewModelParameterName && parameters[i].Type is ITypeParameterSymbol typeParameter)
            {
                return typeParameter.Ordinal;
            }
        }

        return 0;
    }

    /// <summary>
    /// Resolves a well-known type by metadata name and tests whether <paramref name="typeSymbol"/>
    /// matches it, either by interface implementation or base-type inheritance.
    /// </summary>
    /// <param name="typeSymbol">The type to test.</param>
    /// <param name="compilation">The current compilation for type resolution.</param>
    /// <param name="metadataName">The metadata name of the well-known type to resolve.</param>
    /// <param name="byInterface">
    /// <c>true</c> to test interface implementation; <c>false</c> to test base-type inheritance, or implementation when
    /// the resolved type is an interface.
    /// </param>
    /// <returns><c>true</c> if the resolved type exists and matches; otherwise, <c>false</c>.</returns>
    private static bool Matches(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        string metadataName,
        bool byInterface)
    {
        var target = compilation.GetTypeByMetadataName(metadataName);
        if (target is null)
        {
            return false;
        }

        return byInterface
            ? ImplementsInterface(typeSymbol, target)
            : InheritsFromOrImplements(typeSymbol, target);
    }

    /// <summary>Tests base-type inheritance, or implementation when the platform type is an interface.</summary>
    /// <param name="typeSymbol">The type to test.</param>
    /// <param name="platformType">The platform type.</param>
    /// <returns><c>true</c> if the type derives from or implements the platform type; otherwise, <c>false</c>.</returns>
    /// <remarks>Uno declares <c>DependencyObject</c> as an interface on its non-Windows heads.</remarks>
    private static bool InheritsFromOrImplements(INamedTypeSymbol typeSymbol, INamedTypeSymbol platformType) =>
        platformType.TypeKind == TypeKind.Interface
            ? ImplementsInterface(typeSymbol, platformType)
            : InheritsFrom(typeSymbol, platformType);
}
