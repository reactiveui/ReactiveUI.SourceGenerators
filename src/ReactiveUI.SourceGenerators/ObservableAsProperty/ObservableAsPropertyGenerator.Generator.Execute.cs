// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// ReactiveGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ObservableAsPropertyGenerator
{
    /// <summary>
    /// A container for all the logic for <see cref="ObservableAsPropertyGenerator"/>.
    /// </summary>
    internal static partial class Execute
    {
        /// <summary>
        /// Gets the <see cref="MemberDeclarationSyntax"/> instance for the input field.
        /// </summary>
        /// <param name="propertyInfo">The input <see cref="PropertyInfo"/> instance to process.</param>
        /// <returns>The generated <see cref="MemberDeclarationSyntax"/> instance for <paramref name="propertyInfo"/>.</returns>
        internal static ImmutableArray<MemberDeclarationSyntax> GetPropertySyntax(PropertyInfo propertyInfo)
        {
            // Get the property type syntax
            TypeSyntax propertyType = IdentifierName(propertyInfo.TypeNameWithNullabilityAnnotations);

            string getterFieldIdentifierName;

            // In case the backing field is exactly named "value", we need to add the "this." prefix to ensure that comparisons and assignments
            // with it in the generated setter body are executed correctly and without conflicts with the implicit value parameter.
            if (propertyInfo.FieldName == "value")
            {
                // We only need to add "this." when referencing the field in the setter (getter and XML docs are not ambiguous)
                getterFieldIdentifierName = "value";
            }
            else if (SyntaxFacts.GetKeywordKind(propertyInfo.FieldName) != SyntaxKind.None ||
                     SyntaxFacts.GetContextualKeywordKind(propertyInfo.FieldName) != SyntaxKind.None)
            {
                // If the identifier for the field could potentially be a keyword, we must escape it.
                // This usually happens if the annotated field was escaped as well (eg. "@event").
                // In this case, we must always escape the identifier, in all cases.
                getterFieldIdentifierName = $"@{propertyInfo.FieldName}";
            }
            else
            {
                getterFieldIdentifierName = propertyInfo.FieldName;
            }

            ArrowExpressionClauseSyntax getterArrowExpression;

            if (propertyInfo.TypeNameWithNullabilityAnnotations.EndsWith("?"))
            {
                getterArrowExpression = ArrowExpressionClause(ParseExpression($"{getterFieldIdentifierName} = ({getterFieldIdentifierName}Helper == null ? {getterFieldIdentifierName} : {getterFieldIdentifierName}Helper.Value)"));
            }
            else
            {
                getterArrowExpression = ArrowExpressionClause(ParseExpression($"{getterFieldIdentifierName} = {getterFieldIdentifierName}Helper?.Value ?? {getterFieldIdentifierName}"));
            }

            // Prepare the forwarded attributes, if any
            var forwardedAttributes =
                propertyInfo.ForwardedAttributes
                .Select(static a => AttributeList(SingletonSeparatedList(a.GetSyntax())))
                .ToImmutableArray();

            var modifiers = new List<SyntaxToken>();
            var helperTypeName = $"ReactiveUI.ObservableAsPropertyHelper<{propertyType}>";
            if (propertyInfo.AccessModifier == "readonly")
            {
                modifiers.Add(Token(SyntaxKind.PrivateKeyword));
                modifiers.Add(Token(SyntaxKind.ReadOnlyKeyword));
            }
            else
            {
                helperTypeName = $"ReactiveUI.ObservableAsPropertyHelper<{propertyType}>?";
                modifiers.Add(Token(SyntaxKind.PrivateKeyword));
            }

            // Construct the generated property as follows:
            //
            // /// <inheritdoc cref="<FIELD_NAME>"/>
            // [global::System.CodeDom.Compiler.GeneratedCode("...", "...")]
            // [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
            // <FORWARDED_ATTRIBUTES>
            // public <FIELD_TYPE><NULLABLE_ANNOTATION?> <PROPERTY_NAME>
            // {
            //     get => <FIELD_NAME>;
            // }
            return
                ImmutableArray.Create<MemberDeclarationSyntax>(
                    FieldDeclaration(VariableDeclaration(ParseTypeName(helperTypeName)))
                        .AddDeclarationVariables(VariableDeclarator(getterFieldIdentifierName + "Helper"))
                        .AddAttributeLists(
                            AttributeList(SingletonSeparatedList(
                                Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                                .AddArgumentListArguments(
                                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).FullName))),
                                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                            .WithOpenBracketToken(Token(TriviaList(Comment($"/// <inheritdoc cref=\"{propertyInfo.FieldName + "Helper"}\"/>")), SyntaxKind.OpenBracketToken, TriviaList())))
                            .AddModifiers([.. modifiers]),
                    PropertyDeclaration(propertyType, Identifier(propertyInfo.PropertyName))
                        .AddAttributeLists(
                            AttributeList(SingletonSeparatedList(
                                Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                                .AddArgumentListArguments(
                                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).FullName))),
                                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                            .WithOpenBracketToken(Token(TriviaList(Comment($"/// <inheritdoc cref=\"{getterFieldIdentifierName}\"/>")), SyntaxKind.OpenBracketToken, TriviaList())),
                            AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.ExcludeFromCodeCoverage)))))
                        .AddAttributeLists([.. forwardedAttributes])
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithExpressionBody(getterArrowExpression)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))));
        }

        internal static bool GetFieldInfoFromClass(
            FieldDeclarationSyntax fieldSyntax,
            IFieldSymbol fieldSymbol,
            SemanticModel semanticModel,
            bool? isReadonly,
            CancellationToken token,
            [NotNullWhen(true)] out PropertyInfo? propertyInfo,
            out ImmutableArray<DiagnosticInfo> diagnostics)
        {
            using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            // Validate the target type
            if (!IsTargetTypeValid(fieldSymbol))
            {
                builder.Add(
                    InvalidObservableAsPropertyError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);

                propertyInfo = null;
                diagnostics = builder.ToImmutable();

                return false;
            }

            // Get the property type and name
            var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
            var fieldName = fieldSymbol.Name;
            var propertyName = GetGeneratedPropertyName(fieldSymbol);
            var initializer = fieldSyntax.Declaration.Variables.FirstOrDefault()?.Initializer;

            // Check for name collisions
            if (fieldName == propertyName)
            {
                builder.Add(
                    ReactivePropertyNameCollisionError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);

                propertyInfo = null;
                diagnostics = builder.ToImmutable();

                // If the generated property would collide, skip generating it entirely. This makes sure that
                // users only get the helpful diagnostic about the collision, and not the normal compiler error
                // about a definition for "Property" already existing on the target type, which might be confusing.
                return false;
            }

            token.ThrowIfCancellationRequested();

            using var forwardedAttributes = ImmutableArrayBuilder<AttributeInfo>.Rent();

            // Gather attributes info
            foreach (var attributeData in fieldSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                // Track the current attribute for forwarding if it is a validation attribute
                if (attributeData.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ValidationAttribute") == true)
                {
                    forwardedAttributes.Add(AttributeInfo.Create(attributeData));
                }

                // Track the current attribute for forwarding if it is a Json Serialization attribute
                if (attributeData.AttributeClass?.InheritsFromFullyQualifiedMetadataName("System.Text.Json.Serialization.JsonAttribute") == true)
                {
                    forwardedAttributes.Add(AttributeInfo.Create(attributeData));
                }

                // Also track the current attribute for forwarding if it is of any of the following types:
                if (attributeData.AttributeClass?.HasOrInheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.UIHintAttribute") == true ||
                    attributeData.AttributeClass?.HasOrInheritsFromFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute") == true ||
                    attributeData.AttributeClass?.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.DisplayAttribute") == true ||
                    attributeData.AttributeClass?.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.EditableAttribute") == true ||
                    attributeData.AttributeClass?.HasFullyQualifiedMetadataName("System.ComponentModel.DataAnnotations.KeyAttribute") == true ||
                    attributeData.AttributeClass?.HasFullyQualifiedMetadataName("System.Runtime.Serialization.DataMemberAttribute") == true ||
                    attributeData.AttributeClass?.HasFullyQualifiedMetadataName("System.Runtime.Serialization.IgnoreDataMemberAttribute") == true)
                {
                    forwardedAttributes.Add(AttributeInfo.Create(attributeData));
                }
            }

            token.ThrowIfCancellationRequested();

            // Gather explicit forwarded attributes info
            foreach (var attributeList in fieldSyntax.AttributeLists)
            {
                // Only look for attribute lists explicitly targeting the (generated) property. Roslyn will normally emit a
                // CS0657 warning (invalid target), but that is automatically suppressed by a dedicated diagnostic suppressor
                // that recognizes uses of this target specifically to support [ObservableAsProperty].
                if (attributeList.Target?.Identifier is not SyntaxToken(SyntaxKind.PropertyKeyword))
                {
                    continue;
                }

                token.ThrowIfCancellationRequested();

                foreach (var attribute in attributeList.Attributes)
                {
                    // Roslyn ignores attributes in an attribute list with an invalid target, so we can't get the AttributeData as usual.
                    // To reconstruct all necessary attribute info to generate the serialized model, we use the following steps:
                    //   - We try to get the attribute symbol from the semantic model, for the current attribute syntax. In case this is not
                    //     available (in theory it shouldn't, but it can be), we try to get it from the candidate symbols list for the node.
                    //     If there are no candidates or more than one, we just issue a diagnostic and stop processing the current attribute.
                    //     The returned symbols might be method symbols (constructor attribute) so in that case we can get the declaring type.
                    //   - We then go over each attribute argument expression and get the operation for it. This will still be available even
                    //     though the rest of the attribute is not validated nor bound at all. From the operation we can still retrieve all
                    //     constant values to build the AttributeInfo model. After all, attributes only support constant values, typeof(T)
                    //     expressions, or arrays of either these two types, or of other arrays with the same rules, recursively.
                    //   - From the syntax, we can also determine the identifier names for named attribute arguments, if any.
                    // There is no need to validate anything here: the attribute will be forwarded as is, and then Roslyn will validate on the
                    // generated property. Users will get the same validation they'd have had directly over the field. The only drawback is the
                    // lack of IntelliSense when constructing attributes over the field, but this is the best we can do from this end anyway.
                    if (!semanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeTypeSymbol))
                    {
                        builder.Add(
                            InvalidPropertyTargetedAttributeOnObservableAsPropertyField,
                            attribute,
                            fieldSymbol,
                            attribute.Name);

                        continue;
                    }

                    var attributeArguments = attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>();

                    // Try to extract the forwarded attribute
                    if (!AttributeInfo.TryCreate(attributeTypeSymbol, semanticModel, attributeArguments, token, out var attributeInfo))
                    {
                        builder.Add(
                            InvalidPropertyTargetedAttributeExpressionOnObservableAsPropertyField,
                            attribute,
                            fieldSymbol,
                            attribute.Name);

                        continue;
                    }

                    forwardedAttributes.Add(attributeInfo);
                }
            }

            token.ThrowIfCancellationRequested();

            // Get the nullability info for the property
            GetNullabilityInfo(
                fieldSymbol,
                semanticModel,
                out var isReferenceTypeOrUnconstraindTypeParameter,
                out var includeMemberNotNullOnSetAccessor);

            token.ThrowIfCancellationRequested();

            propertyInfo = new PropertyInfo(
                typeNameWithNullabilityAnnotations,
                fieldName,
                propertyName,
                initializer,
                isReferenceTypeOrUnconstraindTypeParameter,
                includeMemberNotNullOnSetAccessor,
                forwardedAttributes.ToImmutable(),
                isReadonly == false ? string.Empty : "readonly");

            diagnostics = builder.ToImmutable();

            return true;
        }

        /// <summary>
        /// Validates the containing type for a given field being annotated.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
        /// <returns>Whether or not the containing type for <paramref name="fieldSymbol"/> is valid.</returns>
        private static bool IsTargetTypeValid(IFieldSymbol fieldSymbol)
        {
            var isObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
            var isIObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
            var hasObservableObjectAttribute = fieldSymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

            return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
        }

        /// <summary>
        /// Gets the nullability info on the generated property.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="isReferenceTypeOrUnconstraindTypeParameter">Whether the property type supports nullability.</param>
        /// <param name="includeMemberNotNullOnSetAccessor">Whether MemberNotNullAttribute should be used on the setter.</param>
        private static void GetNullabilityInfo(
            IFieldSymbol fieldSymbol,
            SemanticModel semanticModel,
            out bool isReferenceTypeOrUnconstraindTypeParameter,
            out bool includeMemberNotNullOnSetAccessor)
        {
            // We're using IsValueType here and not IsReferenceType to also cover unconstrained type parameter cases.
            // This will cover both reference types as well T when the constraints are not struct or unmanaged.
            // If this is true, it means the field storage can potentially be in a null state (even if not annotated).
            isReferenceTypeOrUnconstraindTypeParameter = !fieldSymbol.Type.IsValueType;

            // This is used to avoid nullability warnings when setting the property from a constructor, in case the field
            // was marked as not nullable. Nullability annotations are assumed to always be enabled to make the logic simpler.
            // Consider this example:
            //
            // partial class MyViewModel : ReactiveObject
            // {
            //    public MyViewModel()
            //    {
            //        Name = "Bob";
            //    }
            //
            //    [ObservableAsProperty]
            //    private string name;
            // }
            //
            // The [MemberNotNull] attribute is needed on the setter for the generated Name property so that when Name
            // is set, the compiler can determine that the name backing field is also being set (to a non null value).
            // Of course, this can only be the case if the field type is also of a type that could be in a null state.
            includeMemberNotNullOnSetAccessor =
                isReferenceTypeOrUnconstraindTypeParameter &&
                fieldSymbol.Type.NullableAnnotation != NullableAnnotation.Annotated &&
                semanticModel.Compilation.HasAccessibleTypeWithMetadataName("System.Diagnostics.CodeAnalysis.MemberNotNullAttribute");
        }

        /// <summary>
        /// Get the generated property name for an input field.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
        /// <returns>The generated property name for <paramref name="fieldSymbol"/>.</returns>
        private static string GetGeneratedPropertyName(IFieldSymbol fieldSymbol)
        {
            var propertyName = fieldSymbol.Name;

            if (propertyName.StartsWith("m_"))
            {
                propertyName = propertyName.Substring(2);
            }
            else if (propertyName.StartsWith("_"))
            {
                propertyName = propertyName.TrimStart('_');
            }

            return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
        }
    }
}
