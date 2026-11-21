// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a compile-time <c>RegisterGeneratedViews</c> extension method for all views marked with <c>[RegisterView]</c>.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class RegisterViewGenerator : IIncrementalGenerator
{
    /// <summary>The fully qualified generator name written to generated-code attributes.</summary>
    private static readonly string GeneratorName = typeof(RegisterViewGenerator).FullName!;

    /// <summary>The generator assembly version written to generated-code attributes.</summary>
    private static readonly string GeneratorVersion = typeof(RegisterViewGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The <c>GeneratedCode</c> attribute stamped on the generated class.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var nonGenericViews =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeDefinitions.RegisterViewAttributeType,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, token) => ExtractRegistrations(in ctx, isGeneric: false, token))
                .SelectMany(static (items, _) => items);

        var genericViews =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeDefinitions.RegisterViewGenericAttributeType,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, token) => ExtractRegistrations(in ctx, isGeneric: true, token))
                .SelectMany(static (items, _) => items);

        var allViews = nonGenericViews
            .Collect()
            .Combine(genericViews.Collect())
            .Select(static (pair, _) => MergeAndSort(pair.Left, pair.Right))
            .WithTrackingName(TrackingNames.RegisteredViews);

        context.RegisterSourceOutput(
            allViews.Combine(context.ReactiveUiIntegration()),
            static (spc, input) =>
            {
                if (input.Left.IsEmpty)
                {
                    return;
                }

                spc.AddSource(
                    "GeneratedViewRegistrations.g.cs",
                    GenerateSource(input.Left, input.Right));
            });
    }

    /// <summary>Extracts view registration entries from an attributed view class.</summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <param name="isGeneric">Whether the attribute matched was the generic <c>RegisterViewAttribute&lt;TViewModel&gt;</c>.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The extracted registration entries.</returns>
    private static EquatableArray<RegisteredViewInfo> ExtractRegistrations(
        in GeneratorAttributeSyntaxContext context,
        bool isGeneric,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        if (classSymbol.IsAbstract || !classSymbol.TypeParameters.IsEmpty)
        {
            return default;
        }

        var viewTypeName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        using var builder = ArrayBuilder<RegisteredViewInfo>.Rent();

        foreach (var attribute in context.Attributes)
        {
            token.ThrowIfCancellationRequested();

            if (TryCreateRegistration(classSymbol, viewTypeName, attribute, isGeneric) is { } registration)
            {
                builder.Add(registration);
            }
        }

        return builder.ToEquatableArray();
    }

    /// <summary>Creates a single registration entry from an attribute application.</summary>
    /// <param name="classSymbol">The view class symbol.</param>
    /// <param name="viewTypeName">The fully qualified view type name.</param>
    /// <param name="attribute">The attribute data.</param>
    /// <param name="isGeneric">Whether the attribute is generic.</param>
    /// <returns>The registration info, or <see langword="null"/> if the view model type could not be determined.</returns>
    private static RegisteredViewInfo? TryCreateRegistration(
        INamedTypeSymbol classSymbol,
        string viewTypeName,
        AttributeData attribute,
        bool isGeneric)
    {
        var viewModelTypeName = isGeneric
            ? GetGenericViewModelTypeName(attribute)
            : InferViewModelTypeName(classSymbol);

        if (viewModelTypeName is null)
        {
            return null;
        }

        string? contract = null;
        var isSingleton = false;

        foreach (var named in attribute.NamedArguments)
        {
            if (named.Key == "Contract" && named.Value.Value is string contractValue)
            {
                contract = contractValue;
            }
            else if (named.Key == "IsSingleton" && named.Value.Value is bool singletonValue)
            {
                isSingleton = singletonValue;
            }
        }

        return new(viewTypeName, viewModelTypeName, contract, isSingleton);
    }

    /// <summary>Gets the view model type name from <c>RegisterViewAttribute&lt;TViewModel&gt;</c>.</summary>
    /// <param name="attribute">The attribute data.</param>
    /// <returns>The fully qualified view model type name, or <see langword="null"/>.</returns>
    private static string? GetGenericViewModelTypeName(AttributeData attribute) =>
        attribute.AttributeClass is { TypeArguments.Length: 1 } attrClass
            ? attrClass.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : null;

    /// <summary>Infers the view model type name from an implemented <c>IViewFor&lt;T&gt;</c> interface or <c>[IViewFor&lt;T&gt;]</c> attribute.</summary>
    /// <param name="classSymbol">The view class symbol.</param>
    /// <returns>The fully qualified view model type name, or <see langword="null"/>.</returns>
    private static string? InferViewModelTypeName(INamedTypeSymbol classSymbol)
    {
        foreach (var iface in classSymbol.AllInterfaces)
        {
            if (iface.Name == "IViewFor" && iface.TypeArguments.Length == 1)
            {
                return iface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        foreach (var attr in classSymbol.GetAttributes())
        {
            if (attr.AttributeClass is { Name: "IViewForAttribute", TypeArguments.Length: 1 } attrClass)
            {
                return attrClass.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        return null;
    }

    /// <summary>Merges and deterministically sorts all extracted view registrations.</summary>
    /// <param name="left">Registrations from non-generic <c>[RegisterView]</c>.</param>
    /// <param name="right">Registrations from generic <c>[RegisterView&lt;T&gt;]</c>.</param>
    /// <returns>A sorted, value-equatable list of registrations.</returns>
    private static EquatableArray<RegisteredViewInfo> MergeAndSort(
        ImmutableArray<RegisteredViewInfo> left,
        ImmutableArray<RegisteredViewInfo> right)
    {
        if (left.IsEmpty && right.IsEmpty)
        {
            return default;
        }

        var list = new List<RegisteredViewInfo>(left.Length + right.Length);
        list.AddRange(left);
        list.AddRange(right);
        list.Sort(static (a, b) =>
        {
            var cmp = string.CompareOrdinal(a.ViewTypeName, b.ViewTypeName);
            if (cmp != 0)
            {
                return cmp;
            }

            cmp = string.CompareOrdinal(a.ViewModelTypeName, b.ViewModelTypeName);
            return cmp != 0 ? cmp : string.CompareOrdinal(a.Contract, b.Contract);
        });

        using var builder = ArrayBuilder<RegisteredViewInfo>.Rent();
        foreach (var item in list)
        {
            builder.Add(item);
        }

        return builder.ToEquatableArray();
    }

    /// <summary>Generates the <c>GeneratedViewRegistrations</c> static extension class.</summary>
    /// <param name="views">The collected view registrations.</param>
    /// <param name="integration">The detected ReactiveUI integration.</param>
    /// <returns>The generated C# source.</returns>
    private static string GenerateSource(EquatableArray<RegisteredViewInfo> views, ReactiveUiIntegration integration)
    {
        var builderNamespace = $"{integration.DeclarationNamespace}.Builder";
        var builderInterface = $"{integration.Namespace}.Builder.IReactiveUIBuilder";

        var writer = SourceWriter.Rent();
        _ = writer.AutoGenerated()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(builderNamespace);
        _ = writer.Summary("Compile-time view registration extensions generated for types marked with [RegisterView].")
            .Line(GeneratedCodeAttribute)
            .Line(SourceWriterExtensions.ExcludeFromCodeCoverageAttribute)
            .Line("public static class GeneratedViewRegistrations")
            .OpenBlock()
            .Summary("Registers all views marked with [RegisterView] in this assembly without runtime reflection.")
            .Append("public static ").Append(builderInterface).Append(" RegisterGeneratedViews(this ").Append(builderInterface).Line(" builder)")
            .OpenBlock()
            .Line("if (builder is null) { throw new global::System.ArgumentNullException(nameof(builder)); }");

        foreach (var view in views)
        {
            if (view.Contract is null)
            {
                var methodName = view.IsSingleton ? "RegisterSingletonView" : "RegisterView";
                _ = writer.Append("builder.").Append(methodName)
                    .Append('<').Append(view.ViewTypeName).Append(", ").Append(view.ViewModelTypeName).Line(">();");
            }
            else
            {
                var registerMethod = view.IsSingleton ? "RegisterLazySingleton" : "Register";
                _ = writer.Append("builder.WithRegistration(static resolver => resolver.")
                    .Append(registerMethod)
                    .Append('<').Append(integration.ViewNamespace).Append(".IViewFor<").Append(view.ViewModelTypeName).Append(">>(")
                    .Append("static () => new ").Append(view.ViewTypeName).Append("(), ")
                    .Append(SymbolDisplay.FormatLiteral(view.Contract, quote: true)).Line("));");
            }
        }

        _ = writer.Line("return builder;")
            .CloseBlock()
            .CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings();

        return writer.ToStringAndReturn();
    }
}
