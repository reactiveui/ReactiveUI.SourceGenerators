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

/// <summary>Generates a compile-time <c>AutoPersistMetadata</c> static property for classes marked with <c>[AutoPersist]</c>.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class AutoPersistGenerator : IIncrementalGenerator
{
    /// <summary>The metadata name of <c>DataContractAttribute</c>.</summary>
    private const string DataContractAttributeName = "System.Runtime.Serialization.DataContractAttribute";

    /// <summary>The metadata name of <c>DataMemberAttribute</c>.</summary>
    private const string DataMemberAttributeName = "System.Runtime.Serialization.DataMemberAttribute";

    /// <summary>The fully qualified generator name written to generated-code attributes.</summary>
    private static readonly string GeneratorName = typeof(AutoPersistGenerator).FullName!;

    /// <summary>The generator assembly version written to generated-code attributes.</summary>
    private static readonly string GeneratorVersion = typeof(AutoPersistGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The <c>GeneratedCode</c> attribute stamped on generated members.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeDefinitions.AutoPersistAttributeType,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, token) => ExtractInfo(in ctx, token))
                .Where(static x => x is not null)
                .Select(static (x, _) => x!)
                .WithTrackingName(TrackingNames.AutoPersistTypes);

        context.RegisterSourceOutput(
            targets.Combine(context.ReactiveUiIntegration()),
            static (spc, input) => spc.AddSource(
                $"{input.Left.TargetInfo.FileHintName}.AutoPersist.g.cs",
                GenerateSource(input.Left, input.Right)));
    }

    /// <summary>Extracts <c>[DataContract]</c> and <c>[DataMember]</c> metadata from an attributed class.</summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The extracted metadata, or <see langword="null"/> when the class is not partial.</returns>
    private static AutoPersistInfo? ExtractInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (!((ClassDeclarationSyntax)context.TargetNode).Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return null;
        }

        token.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var hasDataContract = HasAttributeInHierarchy(classSymbol, DataContractAttributeName);

        using var propertyNames = ArrayBuilder<string>.Rent();
        foreach (var member in classSymbol.GetMembers())
        {
            token.ThrowIfCancellationRequested();

            if (member is not IPropertySymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public } property)
            {
                continue;
            }

            if (HasAttribute(property, DataMemberAttributeName))
            {
                propertyNames.Add(property.Name);
            }
        }

        return new(TargetInfo.From(classSymbol), hasDataContract, propertyNames.ToEquatableArray());
    }

    /// <summary>Checks whether a type or any of its base types carries the specified attribute.</summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="attributeMetadataName">The full metadata name of the attribute.</param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    private static bool HasAttributeInHierarchy(INamedTypeSymbol typeSymbol, string attributeMetadataName)
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (HasAttribute(current, attributeMetadataName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks whether a symbol directly carries the specified attribute.</summary>
    /// <param name="symbol">The symbol to inspect.</param>
    /// <param name="attributeMetadataName">The full metadata name of the attribute.</param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    private static bool HasAttribute(ISymbol symbol, string attributeMetadataName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == attributeMetadataName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Generates the partial class declaring <c>AutoPersistMetadata</c>.</summary>
    /// <param name="info">The extracted persistence metadata.</param>
    /// <param name="integration">The detected ReactiveUI integration.</param>
    /// <returns>The generated C# source.</returns>
    private static string GenerateSource(AutoPersistInfo info, ReactiveUiIntegration integration)
    {
        var metadataType = $"{integration.Namespace}.AutoPersistHelperMixins.AutoPersistMetadata";
        var hasDataContractLiteral = info.HasDataContract ? "true" : "false";

        var writer = SourceWriter.Rent();
        _ = writer.AutoGenerated()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(info.TargetInfo.TargetNamespace)
            + writer.OpenContainingTypes(info.TargetInfo.ParentInfo);

        _ = writer.OpenPartialType(info.TargetInfo)
            .Summary("Gets compile-time AutoPersist metadata generated without runtime reflection.")
            .Line(GeneratedCodeAttribute)
            .Line(SourceWriterExtensions.ExcludeFromCodeCoverageAttribute)
            .Append("public static ").Append(metadataType).Line(" AutoPersistMetadata { get; } =")
            .Indent()
            .Append("new ").Append(metadataType).Append('(').Append(hasDataContractLiteral)
            .Append(", new global::System.Collections.Generic.HashSet<string>(global::System.StringComparer.Ordinal)");

        if (info.PersistablePropertyNames.IsEmpty)
        {
            _ = writer.Line(");");
        }
        else
        {
            _ = writer.EndLine().OpenBlock();
            foreach (var name in info.PersistablePropertyNames)
            {
                _ = writer.Append(SymbolDisplay.FormatLiteral(name, quote: true)).Line(",");
            }

            _ = writer.Outdent().Line("});");
        }

        _ = writer.Outdent()
            .CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings();

        return writer.ToStringAndReturn();
    }
}
