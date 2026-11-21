// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a compile-time <c>ConfigureValidation</c> method for classes marked with <c>[ReactiveValidation]</c>.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class ReactiveValidationGenerator : IIncrementalGenerator
{
    /// <summary>The metadata name of <c>ValidationAttribute</c>.</summary>
    private const string ValidationAttributeName = "System.ComponentModel.DataAnnotations.ValidationAttribute";

    /// <summary>The metadata name of <c>DisplayAttribute</c>.</summary>
    private const string DisplayAttributeName = "System.ComponentModel.DataAnnotations.DisplayAttribute";

    /// <summary>Initial builder capacity padding beyond the attribute type name length.</summary>
    private const int AttributeExpressionPadding = 64;

    /// <summary>The fully qualified generator name written to generated-code attributes.</summary>
    private static readonly string GeneratorName = typeof(ReactiveValidationGenerator).FullName!;

    /// <summary>The generator assembly version written to generated-code attributes.</summary>
    private static readonly string GeneratorVersion = typeof(ReactiveValidationGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The <c>GeneratedCode</c> attribute stamped on generated members.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeDefinitions.ReactiveValidationAttributeType,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, token) => ExtractInfo(in ctx, token))
                .Where(static x => x is not null)
                .Select(static (x, _) => x!)
                .WithTrackingName(TrackingNames.ReactiveValidationTypes);

        context.RegisterSourceOutput(
            targets.Combine(context.ReactiveUiIntegration()),
            static (spc, input) => spc.AddSource(
                $"{input.Left.TargetInfo.FileHintName}.ReactiveValidation.g.cs",
                GenerateSource(input.Left, input.Right)));
    }

    /// <summary>Extracts DataAnnotations validation attributes from <c>ReactiveProperty&lt;T&gt;</c> properties on the target class.</summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The extracted validation model, or <see langword="null"/> when the class is not partial.</returns>
    private static ReactiveValidationInfo? ExtractInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (!((ClassDeclarationSyntax)context.TargetNode).Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return null;
        }

        token.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        using var properties = ArrayBuilder<ValidatedPropertyInfo>.Rent();

        foreach (var member in classSymbol.GetMembers())
        {
            token.ThrowIfCancellationRequested();

            if (member is not IPropertySymbol { IsStatic: false } property || !IsReactivePropertyType(property.Type))
            {
                continue;
            }

            if (TryExtractValidatedProperty(property) is { } validated)
            {
                properties.Add(validated);
            }
        }

        return new(TargetInfo.From(classSymbol), properties.ToEquatableArray());
    }

    /// <summary>Extracts validation attributes and display name for a single <c>ReactiveProperty&lt;T&gt;</c> property.</summary>
    /// <param name="property">The property symbol.</param>
    /// <returns>The validated property info, or <see langword="null"/> if no validation attributes are present.</returns>
    private static ValidatedPropertyInfo? TryExtractValidatedProperty(IPropertySymbol property)
    {
        string? displayName = null;
        using var validationAttributes = ArrayBuilder<AttributeInfo>.Rent();

        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass is not { } attributeClass)
            {
                continue;
            }

            if (attributeClass.ToDisplayString() == DisplayAttributeName)
            {
                displayName = TryGetDisplayName(attribute) ?? displayName;
                continue;
            }

            if (DerivesFrom(attributeClass, ValidationAttributeName))
            {
                validationAttributes.Add(AttributeInfo.Create(attribute));
            }
        }

        return validationAttributes.Count > 0
            ? new(property.Name, displayName ?? property.Name, validationAttributes.ToEquatableArray())
            : null;
    }

    /// <summary>Extracts the <c>Name</c> named argument from a <c>[Display]</c> attribute.</summary>
    /// <param name="attribute">The attribute data.</param>
    /// <returns>The display name, or <see langword="null"/> if not specified.</returns>
    private static string? TryGetDisplayName(AttributeData attribute)
    {
        foreach (var named in attribute.NamedArguments)
        {
            if (named.Key == "Name" && named.Value.Value is string nameValue)
            {
                return nameValue;
            }
        }

        return null;
    }

    /// <summary>Checks whether a type symbol is <c>ReactiveProperty&lt;T&gt;</c>.</summary>
    /// <param name="typeSymbol">The property type symbol.</param>
    /// <returns><see langword="true"/> if the type is <c>ReactiveProperty&lt;T&gt;</c>; otherwise <see langword="false"/>.</returns>
    private static bool IsReactivePropertyType(ITypeSymbol typeSymbol) =>
        typeSymbol is INamedTypeSymbol { Name: "ReactiveProperty", TypeArguments.Length: 1 };

    /// <summary>Checks whether <paramref name="typeSymbol"/> equals or derives from the named base type.</summary>
    /// <param name="typeSymbol">The attribute type symbol.</param>
    /// <param name="baseMetadataName">The base type metadata name.</param>
    /// <returns><see langword="true"/> if it derives from the base type; otherwise <see langword="false"/>.</returns>
    private static bool DerivesFrom(INamedTypeSymbol typeSymbol, string baseMetadataName)
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == baseMetadataName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Generates the partial class containing <c>ConfigureValidation()</c>.</summary>
    /// <param name="info">The validation model.</param>
    /// <param name="integration">The detected ReactiveUI integration.</param>
    /// <returns>The generated C# source.</returns>
    private static string GenerateSource(ReactiveValidationInfo info, ReactiveUiIntegration integration)
    {
        var mixinsType = $"{integration.Namespace}.ReactivePropertyMixins";

        var writer = SourceWriter.Rent();
        _ = writer.AutoGenerated()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(info.TargetInfo.TargetNamespace)
            + writer.OpenContainingTypes(info.TargetInfo.ParentInfo);

        _ = writer.OpenPartialType(info.TargetInfo)
            .Summary("Wires DataAnnotations validation attributes on ReactiveProperty<T> properties without runtime reflection.")
            .Line(GeneratedCodeAttribute)
            .Line(SourceWriterExtensions.ExcludeFromCodeCoverageAttribute)
            .Line("private void ConfigureValidation()")
            .OpenBlock();

        foreach (var property in info.Properties)
        {
            _ = writer.Append("if (this.").Append(property.PropertyName).Line(" is not null)")
                .OpenBlock()
                .Append(mixinsType).Append(".AddValidation(this.").Append(property.PropertyName)
                .Append(", ").Append(SymbolDisplay.FormatLiteral(property.DisplayName, quote: true));

            foreach (var attribute in property.ValidationAttributes)
            {
                _ = writer.Append(", ").Append(FormatAttributeInstantiation(attribute));
            }

            _ = writer.Line(");")
                .CloseBlock();
        }

        _ = writer.CloseBlock()
            .CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings();

        return writer.ToStringAndReturn();
    }

    /// <summary>Formats an <see cref="AttributeInfo"/> as a C# <c>new AttributeType(...) { ... }</c> expression.</summary>
    /// <param name="attribute">The attribute info.</param>
    /// <returns>The C# object-creation expression.</returns>
    private static string FormatAttributeInstantiation(AttributeInfo attribute)
    {
        var ctorArgs = attribute.ConstructorArgumentInfo.AsSpan();
        var namedArgs = attribute.NamedArgumentInfo.AsSpan();
        var builder = PooledBuilder.Rent(attribute.TypeName.Length + AttributeExpressionPadding);

        _ = builder.Append("new ").Append(attribute.TypeName).Append('(');
        for (var i = 0; i < ctorArgs.Length; i++)
        {
            if (i > 0)
            {
                _ = builder.Append(", ");
            }

            _ = builder.Append(ctorArgs[i].Expression);
        }

        _ = builder.Append(')');

        if (!namedArgs.IsEmpty)
        {
            _ = builder.Append(" { ");
            for (var i = 0; i < namedArgs.Length; i++)
            {
                if (i > 0)
                {
                    _ = builder.Append(", ");
                }

                _ = builder.Append(namedArgs[i].Name).Append(" = ").Append(namedArgs[i].Value.Expression);
            }

            _ = builder.Append(" }");
        }

        return PooledBuilder.ToStringAndReturn(builder);
    }
}
