// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

/// <summary>Raises notifications from inside a partial type, where its own event and protected members are in reach.</summary>
/// <remarks>
/// <para>
/// Most view models raise <c>PropertyChanged</c> through a protected method, or invoke the event themselves, and
/// neither is callable from generated code outside the type. A partial type can take one more member, and that member
/// can do what the type's own code does: invoke the field-like event it declares, or call a protected raise method it
/// inherits. The generator declares that member and calls it.
/// </para>
/// <para>
/// Every declaration of the type and of each type it is nested in has to be partial, because the generator can only
/// add to a type that is already open to additions.
/// </para>
/// </remarks>
internal sealed class PartialTypeRaisePlugin : IPropertyRaisePlugin
{
    /// <summary>The name this mechanism is recorded under.</summary>
    internal const string MechanismName = "PartialType";

    /// <inheritdoc/>
    public int Affinity => BindingAffinity.Fallback;

    /// <inheritdoc/>
    public PropertyRaiseInfo? Select(INamedTypeSymbol type, Compilation compilation)
    {
        var changed = RaiseMembers.FindMethod(
                type,
                changing: false,
                compilation.GetTypeByMetadataName(Constants.PropertyChangedEventArgsMetadataName),
                type,
                compilation)
            ?? RaiseMembers.FindFieldLikeEvent(
                type,
                RaiseMembers.ChangedEventName,
                compilation.GetTypeByMetadataName(Constants.PropertyChangedEventHandlerMetadataName));
        if (changed is not { } changedCall)
        {
            return null;
        }

        // The syntax is only read once a raise member exists, since it is the one test that walks declarations.
        var declaration = DescribeDeclaration(type);
        if (declaration is null)
        {
            return null;
        }

        var changing = RaiseMembers.FindMethod(
                type,
                changing: true,
                compilation.GetTypeByMetadataName(Constants.PropertyChangingEventArgsMetadataName),
                type,
                compilation)
            ?? RaiseMembers.FindFieldLikeEvent(
                type,
                RaiseMembers.ChangingEventName,
                compilation.GetTypeByMetadataName(Constants.PropertyChangingEventHandlerMetadataName));

        return new(MechanismName, changedCall, changing, declaration);
    }

    /// <summary>Describes the partial declarations that reach the type, outermost first.</summary>
    /// <param name="type">The type to add a member to.</param>
    /// <returns>The declaration headers, or null when the type or a type containing it is not partial.</returns>
    internal static PartialTypeDeclaration? DescribeDeclaration(INamedTypeSymbol type)
    {
        var depth = 0;
        for (var current = type; current is not null; current = current.ContainingType)
        {
            depth++;
        }

        var headers = new string[depth];
        var index = depth - 1;
        var outermost = type;
        for (var current = type; current is not null; current = current.ContainingType)
        {
            var header = DescribePartialClass(current);
            if (header is null)
            {
                return null;
            }

            headers[index] = header;
            index--;
            outermost = current;
        }

        var ns = outermost.ContainingNamespace;
        return new(ns is null || ns.IsGlobalNamespace ? null : ns.ToDisplayString(), new(headers));
    }

    /// <summary>Writes the header of one partial class declaration.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The header, such as <c>partial class Name&lt;T&gt;</c>, or null when the type is not a partial class.</returns>
    private static string? DescribePartialClass(INamedTypeSymbol type)
    {
        var references = type.DeclaringSyntaxReferences;
        if (type.TypeKind != TypeKind.Class || references.IsEmpty)
        {
            return null;
        }

        string? keyword = null;
        for (var i = 0; i < references.Length; i++)
        {
            if (references[i].GetSyntax() is not TypeDeclarationSyntax declaration
                || !declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return null;
            }

            keyword ??= declaration.Keyword.ValueText;
        }

        if (type.TypeParameters.IsEmpty)
        {
            return $"partial {keyword} {type.Name}";
        }

        // A generic header is built in a pooled buffer rather than through an array and a join.
        var header = new PooledStringBuilder().Append("partial ").Append(keyword).Append(' ').Append(type.Name).Append('<');
        for (var i = 0; i < type.TypeParameters.Length; i++)
        {
            _ = (i == 0 ? header : header.Append(", ")).Append(type.TypeParameters[i].Name);
        }

        return header.Append('>').ToStringAndReturn();
    }
}
