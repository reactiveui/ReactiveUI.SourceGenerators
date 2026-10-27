// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Analyzer.CodeFixes;

/// <summary>Rewrites the marked fields, methods and observable properties of one type as partial properties.</summary>
internal static class ObservableAsPropertyRewriter
{
    /// <summary>The annotation marking a property the rewrite wrote, so its containing types can be made partial.</summary>
    internal const string ConvertedKind = "ReactiveUI.Binding.ObservableAsPropertyConverted";

    /// <summary>The name of the method that assigns the helpers of methods and observable properties.</summary>
    internal const string InitializeMethodName = "InitializeOAPH";

    /// <summary>The prefix some field names start with, which the property name drops.</summary>
    private const string MemberPrefix = "m_";

    /// <summary>One level of indentation inside a method body.</summary>
    private const string BodyIndent = "    ";

    /// <summary>The attribute arguments a field keeps; its initializer replaces InitialValue, and Inheritance becomes a modifier.</summary>
    private static readonly string[] FieldArguments = ["ReadOnly", "UseProtected"];

    /// <summary>The attribute arguments a method or observable property keeps; its helper was never readonly.</summary>
    private static readonly string[] SourceArguments = ["UseProtected", "InitialValue"];

    /// <summary>The type format a written property repeats, nullable annotation included.</summary>
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.MinimallyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>Rewrites every marked field, method and observable property of a type.</summary>
    /// <param name="type">The type declaration.</param>
    /// <param name="model">The semantic model of the document.</param>
    /// <param name="newLine">The line ending the document uses.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rewritten declaration.</returns>
    internal static TypeDeclarationSyntax RewriteType(TypeDeclarationSyntax type, SemanticModel model, string newLine, CancellationToken ct)
    {
        var context = new RewriteContext(model, newLine, IndentOf(type), ct);
        var members = new List<MemberDeclarationSyntax>(type.Members.Count + 1);
        for (var i = 0; i < type.Members.Count; i++)
        {
            var member = type.Members[i];
            var converted = member switch
            {
                FieldDeclarationSyntax field => ConvertField(field, context),
                MethodDeclarationSyntax or PropertyDeclarationSyntax => ConvertSource(member, context),
                _ => null,
            };

            if (converted is null)
            {
                members.Add(member);
                continue;
            }

            members.AddRange(converted);
        }

        if (context.Statements.Count != 0)
        {
            AddInitializeStatements(members, context);
        }

        var rewritten = type.WithMembers(SyntaxFactory.List(members));
        return context.Renames.Count == 0 ? rewritten : (TypeDeclarationSyntax)new IdentifierRenamer(context.Renames).Visit(rewritten);
    }

    /// <summary>Names the property a field becomes: without its <c>_</c> or <c>m_</c> prefix, and starting upper case.</summary>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The property name, or an empty string when nothing is left of the name.</returns>
    internal static string PropertyNameFromField(string fieldName)
    {
        var name = fieldName.StartsWith(MemberPrefix, StringComparison.Ordinal) ? fieldName[MemberPrefix.Length..] : fieldName.TrimStart('_');
        return name.Length == 0 ? name : $"{char.ToUpperInvariant(name[0])}{name[1..]}";
    }

    /// <summary>Names the helper field the generator writes for a property.</summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns><c>_{name}Helper</c>, with the first letter lowered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string HelperName(string propertyName) =>
        $"_{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}Helper";

    /// <summary>Rewrites a marked field as the partial property it backs.</summary>
    /// <param name="field">The field declaration.</param>
    /// <param name="context">The rewrite's shared state.</param>
    /// <returns>The property, or null when the field is not marked or cannot be rewritten.</returns>
    private static MemberDeclarationSyntax[]? ConvertField(FieldDeclarationSyntax field, RewriteContext context)
    {
        if (field.Declaration.Variables is not [var variable]
            || context.Model.GetDeclaredSymbol(variable, context.Ct) is not IFieldSymbol { IsStatic: false } symbol
            || context.FindAttribute(symbol) is not { } attribute)
        {
            return null;
        }

        var name = variable.Identifier.ValueText;
        var propertyName = PropertyNameFromField(name);
        if (propertyName.Length == 0 || propertyName == name
            || InitialValueArgument(variable.Initializer?.Value, symbol.Type) is not { } initialValue)
        {
            return null;
        }

        var arguments = KeptArguments(attribute, FieldArguments);
        if (initialValue.Length != 0)
        {
            arguments.Add($"InitialValue = {initialValue}");
        }

        var lines = new List<string> { AttributeText(attribute, arguments) };
        AddMovedAttributes(lines, field.AttributeLists, attribute, includeUntargeted: true);
        var declaration = $"public {InheritanceModifier(attribute)}partial {field.Declaration.Type.ToString().Trim()}";
        var property = BuildProperty(lines, declaration, propertyName, context)
            .WithLeadingTrivia(field.GetLeadingTrivia())
            .WithTrailingTrivia(field.GetTrailingTrivia());

        context.Renames[name] = propertyName;
        var oldHelper = $"{name}Helper";
        var newHelper = HelperName(propertyName);
        if (oldHelper != newHelper)
        {
            context.Renames[oldHelper] = newHelper;
        }

        return [property];
    }

    /// <summary>Keeps a marked method or observable property, and adds the partial property its values back.</summary>
    /// <param name="member">The method or property declaration.</param>
    /// <param name="context">The rewrite's shared state.</param>
    /// <returns>The member without the attribute and the new property, or null when it is not a form to rewrite.</returns>
    private static MemberDeclarationSyntax[]? ConvertSource(MemberDeclarationSyntax member, RewriteContext context)
    {
        var (symbol, returnType, call) = context.Model.GetDeclaredSymbol(member, context.Ct) switch
        {
            IMethodSymbol { IsStatic: false, Parameters.Length: 0 } method => ((ISymbol?)method, method.ReturnType, $"{method.Name}()"),
            IPropertySymbol { IsStatic: false } candidate when !ObservableAsPropertyAnalyzer.IsGeneratedForm(candidate) => (candidate, candidate.Type, candidate.Name),
            _ => (null, null, null),
        };

        if (symbol is null
            || context.FindAttribute(symbol) is not { } attribute
            || ObservableAsPropertyAnalyzer.ObservedType(returnType!, context.Observable) is not { } valueType)
        {
            return null;
        }

        var propertyName = NamedString(attribute, "PropertyName") ?? $"{symbol.Name}Property";
        var lines = new List<string> { AttributeText(attribute, KeptArguments(attribute, SourceArguments)) };
        AddMovedAttributes(lines, member.AttributeLists, attribute, includeUntargeted: false);
        var typeText = valueType.ToMinimalDisplayString(context.Model, member.SpanStart, TypeFormat);
        var property = BuildProperty(lines, $"public partial {typeText}", propertyName, context)
            .WithLeadingTrivia(SyntaxFactory.EndOfLine(context.NewLine), SyntaxFactory.Whitespace(context.Indent))
            .WithTrailingTrivia(SyntaxFactory.EndOfLine(context.NewLine));

        context.Statements.Add($"{HelperName(propertyName)} = {call}.ToProperty(this, nameof({propertyName}));");
        return [WithoutMovedAttributes(member, attribute), property];
    }

    /// <summary>Writes a property declaration below its attribute lines.</summary>
    /// <param name="attributeLines">The attribute lists, one per line.</param>
    /// <param name="modifiersAndType">The modifiers and the property type.</param>
    /// <param name="name">The property name.</param>
    /// <param name="context">The rewrite's shared state.</param>
    /// <returns>The parsed declaration, marked as converted.</returns>
    private static MemberDeclarationSyntax BuildProperty(List<string> attributeLines, string modifiersAndType, string name, RewriteContext context)
    {
        var separator = context.NewLine + context.Indent;
        var text = $"{string.Join(separator, attributeLines)}{separator}{modifiersAndType} {name} {{ get; }}";
        return SyntaxFactory.ParseMemberDeclaration(text)!.WithAdditionalAnnotations(new SyntaxAnnotation(ConvertedKind));
    }

    /// <summary>Adds the helper assignments to the type's <c>InitializeOAPH</c>, writing the method when the type has none.</summary>
    /// <param name="members">The type's members so far.</param>
    /// <param name="context">The rewrite's shared state.</param>
    private static void AddInitializeStatements(List<MemberDeclarationSyntax> members, RewriteContext context)
    {
        for (var i = 0; i < members.Count; i++)
        {
            if (members[i] is not MethodDeclarationSyntax { Identifier.ValueText: InitializeMethodName, ParameterList.Parameters.Count: 0, Body: { } body } existing)
            {
                continue;
            }

            var statements = body.Statements;
            for (var s = 0; s < context.Statements.Count; s++)
            {
                statements = statements.Add(SyntaxFactory.ParseStatement(context.Statements[s])
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(context.Indent + BodyIndent))
                    .WithTrailingTrivia(SyntaxFactory.EndOfLine(context.NewLine)));
            }

            members[i] = existing.WithBody(body.WithStatements(statements));
            return;
        }

        var separator = context.NewLine + context.Indent;
        var text = new System.Text.StringBuilder()
            .Append("/// <summary>Assigns the helpers of the properties built from observables. Call it from the constructor.</summary>")
            .Append(separator).Append("protected void ").Append(InitializeMethodName).Append("()")
            .Append(separator).Append('{');
        for (var s = 0; s < context.Statements.Count; s++)
        {
            _ = text.Append(separator).Append(BodyIndent).Append(context.Statements[s]);
        }

        _ = text.Append(separator).Append('}').Append(context.NewLine);

        // The doc comment parses as the method's leading trivia; a blank line and the indentation go in front of it.
        var method = SyntaxFactory.ParseMemberDeclaration(text.ToString())!;
        var leading = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(context.NewLine), SyntaxFactory.Whitespace(context.Indent))
            .AddRange(method.GetLeadingTrivia());
        members.Add(method.WithLeadingTrivia(leading));
    }

    /// <summary>Writes the value of the <c>InitialValue</c> argument a field's initializer becomes.</summary>
    /// <param name="initializer">The initializer, or null.</param>
    /// <param name="type">The field type.</param>
    /// <returns>
    /// The argument's expression, an empty string for no initializer, or null when a string field is initialised with
    /// something other than a literal, which <c>InitialValue</c> cannot hold.
    /// </returns>
    private static string? InitialValueArgument(ExpressionSyntax? initializer, ITypeSymbol type)
    {
        if (initializer is null)
        {
            return string.Empty;
        }

        if (type.SpecialType != SpecialType.System_String)
        {
            return SymbolDisplay.FormatLiteral(initializer.ToString(), true);
        }

        return initializer is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal
            ? SymbolDisplay.FormatLiteral(literal.Token.ValueText, true)
            : null;
    }

    /// <summary>Returns the modifier a field's <c>Inheritance</c> argument names, read from its syntax so either enum works.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns><c>virtual </c>, <c>override </c>, <c>new </c>, or an empty string.</returns>
    private static string InheritanceModifier(AttributeData attribute) =>
        FindArgument(attribute, "Inheritance")?.Expression switch
        {
            MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Virtual" } => "virtual ",
            MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Override" } => "override ",
            MemberAccessExpressionSyntax { Name.Identifier.ValueText: "New" } => "new ",
            _ => string.Empty,
        };

    /// <summary>Reads a named string argument of the attribute.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="name">The argument name.</param>
    /// <returns>The value, or null when the argument is absent.</returns>
    private static string? NamedString(AttributeData attribute, string name)
    {
        var arguments = attribute.NamedArguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].Key == name && arguments[i].Value.Value is string value)
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>Finds a named argument's syntax on the attribute.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument, or null when it is absent.</returns>
    private static AttributeArgumentSyntax? FindArgument(AttributeData attribute, string name)
    {
        var arguments = SyntaxOf(attribute).ArgumentList?.Arguments ?? default;
        for (var i = 0; i < arguments.Count; i++)
        {
            if (arguments[i].NameEquals?.Name.Identifier.ValueText == name)
            {
                return arguments[i];
            }
        }

        return null;
    }

    /// <summary>Copies the named arguments the new attribute keeps, as written.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="kept">The names of the arguments to keep.</param>
    /// <returns>The arguments' text.</returns>
    private static List<string> KeptArguments(AttributeData attribute, string[] kept)
    {
        var result = new List<string>();
        var arguments = SyntaxOf(attribute).ArgumentList?.Arguments ?? default;
        for (var i = 0; i < arguments.Count; i++)
        {
            if (arguments[i].NameEquals is { } name && Array.IndexOf(kept, name.Name.Identifier.ValueText) >= 0)
            {
                result.Add(arguments[i].ToString().Trim());
            }
        }

        return result;
    }

    /// <summary>Writes the attribute as it is spelled, with the given arguments.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The attribute list's text.</returns>
    private static string AttributeText(AttributeData attribute, List<string> arguments)
    {
        var name = SyntaxOf(attribute).Name.ToString();
        return arguments.Count == 0 ? $"[{name}]" : $"[{name}({string.Join(", ", arguments)})]";
    }

    /// <summary>
    /// Adds the attributes that move onto the new property: those targeted at <c>property:</c> or <c>field:</c>, and for a
    /// field, the untargeted ones too, since the field is gone.
    /// </summary>
    /// <param name="lines">The attribute lines of the new property.</param>
    /// <param name="lists">The member's attribute lists.</param>
    /// <param name="attribute">The <c>[ObservableAsProperty]</c>, which is rewritten separately.</param>
    /// <param name="includeUntargeted">Whether untargeted attributes move as well.</param>
    private static void AddMovedAttributes(List<string> lines, SyntaxList<AttributeListSyntax> lists, AttributeData attribute, bool includeUntargeted)
    {
        var own = SyntaxOf(attribute);
        foreach (var list in lists)
        {
            if (list.Target is null && !includeUntargeted)
            {
                continue;
            }

            var moved = new List<string>(list.Attributes.Count);
            foreach (var candidate in list.Attributes)
            {
                if (candidate != own)
                {
                    moved.Add(candidate.ToString());
                }
            }

            if (moved.Count != 0)
            {
                lines.Add($"[{string.Join(", ", moved)}]");
            }
        }
    }

    /// <summary>Removes the <c>[ObservableAsProperty]</c> and the targeted attributes that moved from a member.</summary>
    /// <param name="member">The member.</param>
    /// <param name="attribute">The <c>[ObservableAsProperty]</c>.</param>
    /// <returns>The member without them, keeping its leading trivia.</returns>
    private static MemberDeclarationSyntax WithoutMovedAttributes(MemberDeclarationSyntax member, AttributeData attribute)
    {
        var own = SyntaxOf(attribute);
        var kept = new List<AttributeListSyntax>();
        foreach (var list in member.AttributeLists)
        {
            if (list.Target is not null)
            {
                continue;
            }

            var remaining = new List<AttributeSyntax>(list.Attributes.Count);
            foreach (var candidate in list.Attributes)
            {
                if (candidate != own)
                {
                    remaining.Add(candidate);
                }
            }

            if (remaining.Count != 0)
            {
                kept.Add(list.WithAttributes(SyntaxFactory.SeparatedList(remaining)));
            }
        }

        return member.WithAttributeLists(SyntaxFactory.List(kept)).WithLeadingTrivia(member.GetLeadingTrivia());
    }

    /// <summary>Returns the syntax an attribute was written with.</summary>
    /// <param name="attribute">The attribute, which is always applied in source here.</param>
    /// <returns>The attribute syntax.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AttributeSyntax SyntaxOf(AttributeData attribute) =>
        (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax();

    /// <summary>Reads the indentation of a type's members.</summary>
    /// <param name="type">The type declaration, which holds at least the marked member.</param>
    /// <returns>The whitespace before its first member.</returns>
    private static string IndentOf(TypeDeclarationSyntax type)
    {
        var leading = type.Members[0].GetLeadingTrivia();
        return leading.Count != 0 && leading[leading.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) ? leading[leading.Count - 1].ToString() : string.Empty;
    }

    /// <summary>The state one rewrite of a type shares between its members.</summary>
    /// <param name="model">The semantic model of the document.</param>
    /// <param name="newLine">The line ending the document uses.</param>
    /// <param name="indent">The indentation of the type's members.</param>
    /// <param name="ct">Cancellation token.</param>
    private sealed class RewriteContext(SemanticModel model, string newLine, string indent, CancellationToken ct)
    {
        /// <summary>Gets the semantic model of the document.</summary>
        public SemanticModel Model { get; } = model;

        /// <summary>Gets the line ending the document uses.</summary>
        public string NewLine { get; } = newLine;

        /// <summary>Gets the indentation of the type's members.</summary>
        public string Indent { get; } = indent;

        /// <summary>Gets the cancellation token.</summary>
        public CancellationToken Ct { get; } = ct;

        /// <summary>Gets the open <c>IObservable&lt;T&gt;</c>.</summary>
        public INamedTypeSymbol? Observable { get; } = model.Compilation.GetTypeByMetadataName("System.IObservable`1");

        /// <summary>Gets the helper assignments <c>InitializeOAPH</c> makes.</summary>
        public List<string> Statements { get; } = [];

        /// <summary>Gets the identifiers to rename: a field to its property, and its old helper name to the new one.</summary>
        public Dictionary<string, string> Renames { get; } = [with(StringComparer.Ordinal)];

        /// <summary>Gets the lean runtime's attribute type, or null when it is not referenced.</summary>
        private INamedTypeSymbol? Lean { get; } = model.Compilation.GetTypeByMetadataName(ObservableAsPropertyAnalyzer.LeanAttributeName);

        /// <summary>Gets the System.Reactive runtime's attribute type, or null when it is not referenced.</summary>
        private INamedTypeSymbol? Reactive { get; } = model.Compilation.GetTypeByMetadataName(ObservableAsPropertyAnalyzer.ReactiveAttributeName);

        /// <summary>Finds the <c>[ObservableAsProperty]</c> on a member.</summary>
        /// <param name="symbol">The member.</param>
        /// <returns>The attribute, or null when the member has none.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AttributeData? FindAttribute(ISymbol symbol) => ObservableAsPropertyAnalyzer.FindAttribute(symbol, Lean, Reactive);
    }

    /// <summary>Renames identifiers that named a removed field or its helper.</summary>
    /// <param name="renames">The old names and the names that replace them.</param>
    private sealed class IdentifierRenamer(Dictionary<string, string> renames) : CSharpSyntaxRewriter
    {
        /// <inheritdoc/>
        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node) =>
            renames.TryGetValue(node.Identifier.ValueText, out var name)
                ? node.WithIdentifier(SyntaxFactory.Identifier(name).WithTriviaFrom(node.Identifier))
                : node;
    }
}
