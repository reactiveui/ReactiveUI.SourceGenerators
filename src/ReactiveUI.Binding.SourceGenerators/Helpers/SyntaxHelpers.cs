// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Provides syntax-level helpers for extracting property paths from lambda expressions.</summary>
internal static class SyntaxHelpers
{
    /// <summary>Gets the line the compiler passes to a <c>CallerLineNumber</c> parameter of an invocation.</summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The one-based line number.</returns>
    /// <remarks>
    /// The compiler reports the line of the invoked member's name, not the line the invocation starts on. The two
    /// differ for a chained call written across lines, where the invocation of <c>.ToProperty(...)</c> on its own
    /// line starts back at the receiver on an earlier line.
    /// </remarks>
    internal static int CallerLineNumber(InvocationExpressionSyntax invocation, CancellationToken ct)
    {
        var anchor = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Span,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Span,
            _ => invocation.Span,
        };

        return invocation.SyntaxTree.GetLineSpan(anchor, ct).StartLinePosition.Line + 1;
    }

    /// <summary>Extracts the property path from a lambda expression, for code declared in a class of its own.</summary>
    /// <param name="expression">The expression syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of property path segments, or null if the expression is not a valid lambda.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static PropertyPathSegment[]? ExtractPropertyPathFromLambda(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken ct) =>
        ExtractPropertyPathFromLambda(expression, semanticModel, ReachScope.Assembly(semanticModel.Compilation), ct);

    /// <summary>Extracts the property path from a lambda expression.</summary>
    /// <param name="expression">The expression syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code that reads the path is declared.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of property path segments, or null if the expression is not a valid lambda.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static PropertyPathSegment[]? ExtractPropertyPathFromLambda(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        ReachScope scope,
        CancellationToken ct)
    {
        // Must be an inline lambda
        if (expression is not LambdaExpressionSyntax lambda)
        {
            return null;
        }

        // Get the lambda body (only SimpleLambda and ParenthesizedLambda exist in Roslyn)
        var body = GetLambdaBody(lambda);

        if (body is null)
        {
            return null;
        }

        // Decompose member access chain: x.A.B.C → [A, B, C]
        // The null-forgiving and null-conditional operators name the same members: x.A!.B?.C → [A, B, C]
        const int TypicalPathDepth = 4;

        var segments = new List<PropertyPathSegment>(TypicalPathDepth);
        var current = PathSyntax.Link(body);
        var isConditional = false;

        while (PathSyntax.IsMember(current))
        {
            ct.ThrowIfCancellationRequested();

            if (ReadPathSegment(current, semanticModel, scope, segments.Count == 0, ct) is not { } segment)
            {
                return null;
            }

            segments.Add(segment);
            isConditional |= PathSyntax.IsConditional(current);
            current = PathSyntax.Link(PathSyntax.Receiver(current));
        }

        // A '?.' ahead of a value makes the lambda produce a nullable value, which is not the type the member has.
        // The generated method is typed from the member, so it would not match the call.
        if (isConditional && LiftsLeafValue(body, semanticModel, ct))
        {
            return null;
        }

        // The chain has to end at the lambda's own parameter. Anything else - an indexer, a method call, a local -
        // is a link the loop could not read, and generating the part after it would observe the wrong path.
        if (segments.Count == 0 || !IsLambdaParameter(current, lambda))
        {
            return null;
        }

        // Reverse so the path goes from root to leaf
        segments.Reverse();
        return [.. segments];
    }

    /// <summary>Extracts the body expression from a <see cref="LambdaExpressionSyntax"/>.</summary>
    /// <param name="lambda">The lambda expression.</param>
    /// <returns>The body as an <see cref="ExpressionSyntax"/>, or null if the body is a block or unsupported form.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExpressionSyntax? GetLambdaBody(LambdaExpressionSyntax lambda) =>
        lambda.Body as ExpressionSyntax;

    /// <summary>Reads one link of an observed property path.</summary>
    /// <param name="memberAccess">The member access naming the link.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code that reads the link is declared.</param>
    /// <param name="isLeaf">Whether the link is the last of the path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The segment, or null when the link is not a property or field generated code can read.</returns>
    private static PropertyPathSegment? ReadPathSegment(
        ExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        ReachScope scope,
        bool isLeaf,
        CancellationToken ct)
    {
        var member = semanticModel.GetSymbolInfo(memberAccess, ct).Symbol;
        var memberType = member switch
        {
            IPropertySymbol property => property.Type,
            IFieldSymbol field when IsReadableFieldLink(field, isLeaf) => field.Type,
            _ => null,
        };

        if (memberType is null)
        {
            return null;
        }

        var link = member!;
        var owner = WithoutNullableValue(semanticModel.GetTypeInfo(PathSyntax.Receiver(memberAccess), ct).Type) as INamedTypeSymbol ?? link.ContainingType;

        // A private or protected member is only in reach of code in the caller's own partial class.
        if (!scope.CanRead(link, owner))
        {
            return null;
        }

        // Generated code names every link's owner and value type, so a link through a type it cannot reach
        // leaves the whole path to the runtime stub.
        if (!scope.CanName(owner) || !scope.CanName(memberType))
        {
            return null;
        }

        // A field raises no notification, so it resolves no mechanism and is read once, like any other link whose
        // owner cannot notify. It still carries its owner, which is how a call site whose first path starts at a
        // field learns how the observed type notifies, and which view model property a view binding follows.
        return link is IPropertySymbol propertySymbol
            ? new(
                propertySymbol.Name,
                memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                memberType.IsReferenceType,
                TypeDetectionExtractor.ExtractPropertyOwner(owner, propertySymbol, semanticModel.Compilation, ct),
                IsNullableReference: IsNullableReference(memberType))
            : new(
                link.Name,
                memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                memberType.IsReferenceType,
                TypeDetectionExtractor.ExtractFieldOwner(owner, semanticModel.Compilation, ct),
                IsField: true,
                IsNullableReference: IsNullableReference(memberType));
    }

    /// <summary>Reads the value type a nullable value holds, which is the type a member after <c>?.</c> is read from.</summary>
    /// <param name="type">The type of the expression before the member.</param>
    /// <returns>The held type for a nullable value, otherwise the type itself.</returns>
    private static ITypeSymbol? WithoutNullableValue(ITypeSymbol? type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
            ? nullable.TypeArguments[0]
            : type;

    /// <summary>Determines whether a path with a <c>?.</c> in it ends at a value the operator makes nullable.</summary>
    /// <param name="body">The lambda body.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when the last member is a value type that is not already nullable.</returns>
    private static bool LiftsLeafValue(ExpressionSyntax body, SemanticModel semanticModel, CancellationToken ct) =>
        semanticModel.GetTypeInfo(PathSyntax.Link(body), ct).Type is { IsValueType: true } leaf
        && leaf.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;

    /// <summary>Determines whether a type is declared as a nullable reference type, such as <c>MyType?</c>.</summary>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true"/> for an annotated reference type.</returns>
    private static bool IsNullableReference(ITypeSymbol type) =>
        type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.Annotated;

    /// <summary>Determines whether a field can stand as a link of a generated path.</summary>
    /// <param name="field">The field.</param>
    /// <param name="isLeaf">Whether the field is the last link, which a binding may write.</param>
    /// <returns><see langword="true"/> for an instance field that generated code may read, and write at the leaf.</returns>
    /// <remarks>
    /// Controls named in XAML or by a designer are exposed as fields, so a view binding usually reads through one,
    /// as in <c>v =&gt; v.NameBox.Text</c>. A read-only field is fine along the way but not at the leaf, which a
    /// two-way binding or <c>BindTo</c> assigns. A constant counts as static, so the static test excludes it too.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsReadableFieldLink(IFieldSymbol field, bool isLeaf) =>
        !field.IsStatic && (!isLeaf || !field.IsReadOnly);

    /// <summary>Determines whether an expression is the single parameter of a lambda.</summary>
    /// <param name="expression">The expression the member access chain ended at.</param>
    /// <param name="lambda">The lambda the chain was read from.</param>
    /// <returns><see langword="true"/> when the expression names the lambda's parameter.</returns>
    private static bool IsLambdaParameter(ExpressionSyntax expression, LambdaExpressionSyntax lambda)
    {
        var parameter = lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesized => parenthesized.ParameterList.Parameters[0],
            _ => null,
        };

        return parameter is not null
            && expression is IdentifierNameSyntax identifier
            && identifier.Identifier.ValueText == parameter.Identifier.ValueText;
    }
}
