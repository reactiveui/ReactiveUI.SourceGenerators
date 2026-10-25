// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts <see cref="ToPropertyInvocationInfo"/> from <c>ToProperty</c> invocations.</summary>
/// <remarks>
/// The property argument is read from syntax before anything asks the semantic model. Resolving the call is the
/// expensive step - it runs overload resolution and generic inference over the stub's overloads - and a selector
/// that is not <c>x =&gt; x.Property</c> can never produce a dispatch, so it is turned away first.
/// </remarks>
internal static class ToPropertyExtractor
{
    /// <summary>The minimum number of arguments a ToProperty invocation has (source, property).</summary>
    private const int MinimumArgumentCount = 2;

    /// <summary>The position of the property argument when it is not named.</summary>
    private const int PropertyArgumentPosition = 1;

    /// <summary>The stub parameter naming the property.</summary>
    private const string PropertyParameterName = "property";

    /// <summary>The number of type arguments on every stub overload (<c>TObj</c>, <c>TRet</c>).</summary>
    private const int StubTypeArgumentCount = 2;

    /// <summary>The fully qualified format with nullable reference annotations, which the generated signature repeats.</summary>
    private static readonly SymbolDisplayFormat AnnotatedFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>Pipeline B transform: extracts <see cref="ToPropertyInvocationInfo"/> from a <c>ToProperty</c> invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The call-site model, or null when the call cannot be generated.</returns>
    internal static ToPropertyInvocationInfo? ExtractToPropertyInvocation(CallSiteContext context, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var args = invocation.ArgumentList.Arguments;
        if (!ExtractorValidation.HasMinimumArguments(args.Count, MinimumArgumentCount))
        {
            return null;
        }

        var propertyArgument = FindPropertyArgument(args);

        // Syntax first: a selector is only usable as x => x.Property, and its name is read straight off it.
        var selectorName = ReadSelectorName(propertyArgument);
        if (selectorName is null && propertyArgument is LambdaExpressionSyntax)
        {
            return null;
        }

        var semanticModel = context.SemanticModel;
        var method = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (method is not { TypeArguments.Length: StubTypeArgumentCount }
            || !ExtractorValidation.IsRecognizedExtensionClass(method.ContainingType)
            || !CallSiteHosting.TryResolve(context, method, ct, out var scope))
        {
            return null;
        }

        var shape = ReadShape(method);
        var propertyName = shape.NamesPropertyByString
            ? ReadConstantName(propertyArgument, semanticModel, ct)
            : selectorName;
        var target = ReadTarget(method, semanticModel.Compilation);

        return string.IsNullOrWhiteSpace(propertyName) || target is not { } resolved
            ? null
            : new(
                invocation.SyntaxTree.FilePath,
                SyntaxHelpers.CallerLineNumber(invocation, ct),
                resolved.SourceTypeFullName,
                resolved.ValueTypeFullName,
                resolved.ValueTypeDisplay,
                propertyName!,
                propertyArgument.ToString(),
                shape,
                resolved.Raise,
                InterceptableLocationReader.Read(semanticModel, invocation, ct),
                scope.Call);
    }

    /// <summary>Names the stub's type arguments and chooses how the source type's notifications are raised.</summary>
    /// <param name="method">The resolved stub method.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The target, or null when an overload could not name the types or raise the notifications.</returns>
    /// <remarks>A type whose notifications generated code cannot raise leaves nothing to generate, so the stub throws.</remarks>
    internal static Target? ReadTarget(IMethodSymbol method, Compilation compilation)
    {
        if (method.TypeArguments[0] is not INamedTypeSymbol { IsStatic: false } sourceType
            || method.TypeArguments[1] is { TypeKind: TypeKind.TypeParameter or TypeKind.Error })
        {
            return null;
        }

        var raise = PropertyRaisePluginRegistry.Select(sourceType, compilation);
        if (raise is null)
        {
            return null;
        }

        // The annotated name is only different when the call site inferred a nullable reference type; otherwise the
        // one string serves both, so the model does not hold two copies of the same text.
        var valueType = method.TypeArguments[1];
        var plain = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var annotated = valueType.NullableAnnotation == NullableAnnotation.Annotated && valueType.IsReferenceType
            ? valueType.ToDisplayString(AnnotatedFormat)
            : plain;
        return new Target(sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), plain, annotated, raise);
    }

    /// <summary>Reads the property name from a selector of the form <c>x =&gt; x.Property</c>.</summary>
    /// <param name="expression">The property argument.</param>
    /// <returns>The property name, or null when the argument is not such a selector.</returns>
    /// <remarks>
    /// Only the syntax is needed: the member must be read off the lambda's own parameter, one level deep, which is
    /// the only shape a property of the source can take. A deeper path names a property of something else.
    /// </remarks>
    internal static string? ReadSelectorName(ExpressionSyntax expression)
    {
        var parameterName = expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.ValueText,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesized =>
                parenthesized.ParameterList.Parameters[0].Identifier.ValueText,
            _ => null,
        };

        if (parameterName is null || ((LambdaExpressionSyntax)expression).Body is not ExpressionSyntax body)
        {
            return null;
        }

        while (body is ParenthesizedExpressionSyntax or PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression })
        {
            body = body is ParenthesizedExpressionSyntax parenthesized ? parenthesized.Expression : ((PostfixUnaryExpressionSyntax)body).Operand;
        }

        return body is MemberAccessExpressionSyntax
        {
            RawKind: (int)SyntaxKind.SimpleMemberAccessExpression,
            Expression: IdentifierNameSyntax receiver,
        } memberAccess
            && receiver.Identifier.ValueText == parameterName
            ? memberAccess.Name.Identifier.ValueText
            : null;
    }

    /// <summary>Reads the property name passed as a string.</summary>
    /// <param name="expression">The property argument.</param>
    /// <param name="semanticModel">The semantic model, for a constant that is not a literal.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The name, or null when the argument is not a compile-time constant.</returns>
    /// <remarks>A literal is read from its token; <c>nameof</c> and constants need the model, which already bound them.</remarks>
    internal static string? ReadConstantName(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken ct)
    {
        if (expression is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal)
        {
            return literal.Token.ValueText;
        }

        return semanticModel.GetConstantValue(expression, ct) is { HasValue: true, Value: string name } ? name : null;
    }

    /// <summary>Reads which stub overload a call resolved to from its parameter names.</summary>
    /// <param name="method">The resolved stub method.</param>
    /// <returns>The overload shape.</returns>
    internal static ToPropertyOverloadShape ReadShape(IMethodSymbol method)
    {
        var namesByString = false;
        var hasResult = false;
        var initial = ToPropertyInitialValueKind.None;
        var hasDefer = false;
        var hasScheduler = false;

        var parameters = method.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            switch (parameter.Name)
            {
                case PropertyParameterName:
                {
                    namesByString = parameter.Type.SpecialType == SpecialType.System_String;
                    break;
                }

                case "result":
                {
                    hasResult = true;
                    break;
                }

                case "initialValue":
                {
                    initial = ToPropertyInitialValueKind.Value;
                    break;
                }

                case "getInitialValue":
                {
                    initial = ToPropertyInitialValueKind.Factory;
                    break;
                }

                case "deferSubscription":
                {
                    hasDefer = true;
                    break;
                }

                case "scheduler":
                {
                    hasScheduler = true;
                    break;
                }
            }
        }

        return new(namesByString, hasResult, initial, hasDefer, hasScheduler);
    }

    /// <summary>Finds the argument that names the property.</summary>
    /// <param name="args">The call's arguments, excluding the receiver.</param>
    /// <returns>The argument passed for <c>property</c>, by name when it is named.</returns>
    private static ExpressionSyntax FindPropertyArgument(SeparatedSyntaxList<ArgumentSyntax> args)
    {
        for (var i = 0; i < args.Count; i++)
        {
            if (args[i].NameColon?.Name.Identifier.ValueText == PropertyParameterName)
            {
                return args[i].Expression;
            }
        }

        return args[PropertyArgumentPosition].Expression;
    }

    /// <summary>The named types of a call site and how its source raises notifications.</summary>
    /// <param name="SourceTypeFullName">The fully qualified source type.</param>
    /// <param name="ValueTypeFullName">The fully qualified value type, without nullable annotations.</param>
    /// <param name="ValueTypeDisplay">The value type with its nullable annotations.</param>
    /// <param name="Raise">How generated code raises the source type's notifications.</param>
    internal readonly record struct Target(string SourceTypeFullName, string ValueTypeFullName, string ValueTypeDisplay, PropertyRaiseInfo Raise);
}
