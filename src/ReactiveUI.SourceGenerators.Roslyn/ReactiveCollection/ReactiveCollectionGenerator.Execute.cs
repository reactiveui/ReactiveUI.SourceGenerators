// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>Implements ReactiveCollection source generation.</summary>
public sealed partial class ReactiveCollectionGenerator
{
    /// <summary>Gets the generator type name used in generated-code metadata.</summary>
    internal static readonly string GeneratorName = typeof(ReactiveCollectionGenerator).FullName!;

    /// <summary>Gets the generator assembly version used in generated-code metadata.</summary>
    internal static readonly string GeneratorVersion = typeof(ReactiveCollectionGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The suffix of the field that holds a property's collection-changed handler.</summary>
    private const string HandlerFieldSuffix = "CollectionChangedHandler";

    /// <summary>The handler type the generated field holds.</summary>
    private const string HandlerType = "global::System.Collections.Specialized.NotifyCollectionChangedEventHandler";

    /// <summary>The <c>GeneratedCode</c> attribute stamped on the generated properties, built once.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <summary>Creates metadata for a ReactiveCollection-annotated field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ReactiveCollectionFieldInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        // Validate the target type
        if (!fieldSymbol.IsTargetTypeValid())
        {
            builder.Add(
                    InvalidReactiveObjectError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        return CreateResult(context, fieldSymbol, builder, token);
    }

    /// <summary>Creates metadata for a validated ReactiveCollection field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="fieldSymbol">The validated field symbol.</param>
    /// <param name="builder">The diagnostics collected while processing the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics.</returns>
    private static Result<ReactiveCollectionFieldInfo?> CreateResult(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var fieldName = fieldSymbol.Name;
        var propertyName = fieldSymbol.GetGeneratedPropertyName();

        // Check for name collisions
        if (fieldName == propertyName)
        {
            builder.Add(
                ReactivePropertyNameCollisionError,
                fieldSymbol,
                fieldSymbol.ContainingType,
                fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        return CreateMetadata(context, fieldSymbol, fieldName, propertyName, builder, token);
    }

    /// <summary>Creates the generated property metadata for a non-conflicting field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="fieldSymbol">The validated field symbol.</param>
    /// <param name="fieldName">The source field name.</param>
    /// <param name="propertyName">The generated property name.</param>
    /// <param name="builder">The diagnostics collected while processing the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics.</returns>
    private static Result<ReactiveCollectionFieldInfo?> CreateMetadata(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        string fieldName,
        string propertyName,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;
        var initializer = GetInitializer(fieldDeclaration);

        token.ThrowIfCancellationRequested();

        context.GetForwardedAttributes(
                builder,
                fieldSymbol,
                fieldDeclaration.AttributeLists,
                token,
                out var forwardedPropertyAttributes);

        token.ThrowIfCancellationRequested();

        // Get the nullability info for the property
        var (isReferenceTypeOrUnconstraindTypeParameter, includeMemberNotNullOnSetAccessor) =
            GetNullabilityInfo(fieldSymbol, context.SemanticModel);

        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(fieldSymbol.ContainingType);

        return new(
            new(
                targetInfo,
                typeNameWithNullabilityAnnotations,
                fieldName,
                propertyName,
                initializer,
                isReferenceTypeOrUnconstraindTypeParameter,
                includeMemberNotNullOnSetAccessor,
                forwardedPropertyAttributes),
            builder.ToImmutable());
    }

    /// <summary>Gets nullability metadata for a generated reactive collection property.</summary>
    /// <param name="fieldSymbol">The source field symbol.</param>
    /// <param name="semanticModel">The semantic model for the source field.</param>
    /// <returns>The reference-type and member-not-null metadata.</returns>
    private static (bool IsReferenceType, bool IncludeMemberNotNull) GetNullabilityInfo(
        IFieldSymbol fieldSymbol,
        SemanticModel semanticModel)
    {
        fieldSymbol.GetNullabilityInfo(
            semanticModel,
            out var isReferenceTypeOrUnconstraindTypeParameter,
            out var includeMemberNotNullOnSetAccessor);
        return (isReferenceTypeOrUnconstraindTypeParameter, includeMemberNotNullOnSetAccessor);
    }

    /// <summary>Gets the first declared field initializer.</summary>
    /// <param name="fieldDeclaration">The source field declaration.</param>
    /// <returns>The initializer text, when present.</returns>
    private static string? GetInitializer(FieldDeclarationSyntax fieldDeclaration)
    {
        var variables = fieldDeclaration.Declaration.Variables;
        return variables.Count > 0 ? variables[0].Initializer?.ToFullString() : null;
    }

    /// <summary>Generates the partial declaration holding one type's reactive collection properties.</summary>
    /// <param name="properties">The properties the type declares, all sharing one <see cref="TargetInfo"/>.</param>
    /// <param name="integration">The selected ReactiveUI API surface.</param>
    /// <returns>The file's text.</returns>
    private static string GenerateSource(EquatableArray<ReactiveCollectionFieldInfo> properties, ReactiveUiIntegration integration)
    {
        var target = properties[0].TargetInfo;
        var writer = SourceWriter.Rent()
            .AutoGenerated()
            .Lines(integration.UsingDirectives)
            .BlankLine()
            .DisableWarningsEnableNullable();

        var depth = writer.OpenNamespace(target.TargetNamespace) + writer.OpenContainingTypes(target.ParentInfo);

        _ = writer.OpenPartialType(target);
        for (var i = 0; i < properties.Count; i++)
        {
            if (i > 0)
            {
                _ = writer.BlankLine();
            }

            WriteProperty(writer, properties[i]);
        }

        return writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Writes one reactive collection property, which re-raises the collection's changes as property changes.</summary>
    /// <param name="writer">The writer, at the level of the type's members.</param>
    /// <param name="propertyInfo">The property.</param>
    /// <remarks>
    /// The handler is created once per property and kept in a field, so the setter unsubscribes the same instance it
    /// subscribed: from the collection being replaced, not the new one. A handler made per call could never be removed,
    /// so every replacement left one more subscribed and allocated three more.
    /// </remarks>
    private static void WriteProperty(SourceWriter writer, ReactiveCollectionFieldInfo propertyInfo)
    {
        var fieldName = propertyInfo.FieldName;
        var propertyName = propertyInfo.PropertyName;

        _ = writer.Append("private ").Append(HandlerType).Append("? ").Append(fieldName).Append(HandlerFieldSuffix).EndStatement()
            .BlankLine()
            .InheritDoc(fieldName)
            .Line(GeneratedCodeAttribute)
            .ExcludeFromCodeCoverage();
        foreach (var attribute in propertyInfo.ForwardedAttributes.AsImmutableArray())
        {
            _ = writer.Line(attribute);
        }

        _ = writer.Append("public ").Append(propertyInfo.TypeNameWithNullabilityAnnotations).Append(' ').Line(propertyName)
            .OpenBlock()
            .Append("get => ").Append(fieldName).EndStatement()
            .Line("set")
            .OpenBlock()
            .Append("var handler = ").Append(fieldName).Append(HandlerFieldSuffix)
            .Append(" ??= (_, _) => this.RaisePropertyChanged(nameof(").Append(propertyName).Line("));")
            .Append("if (").Append(fieldName).Line(" != null)")
            .OpenBlock()
            .Append(fieldName).Line(".CollectionChanged -= handler;")
            .CloseBlock()
            .BlankLine()
            .Append(fieldName).Line(" = value;")
            .Append("this.RaisePropertyChanged(nameof(").Append(propertyName).Line("));")
            .BlankLine()
            .Line("if (value != null)")
            .OpenBlock()
            .Line("value.CollectionChanged += handler;")
            .CloseBlock()
            .CloseBlock()
            .CloseBlock();
    }
}
