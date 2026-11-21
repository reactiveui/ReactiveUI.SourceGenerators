// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Android;

/// <summary>Generates a compile-time <c>WireUpControls</c> method for Android classes marked with <c>[WireUpControls]</c>.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class WireUpControlsGenerator : IIncrementalGenerator
{
    /// <summary>The metadata name of <c>Android.App.Activity</c>.</summary>
    private const string AndroidActivityTypeName = "Android.App.Activity";

    /// <summary>The metadata name of <c>Android.Views.View</c>.</summary>
    private const string AndroidViewTypeName = "Android.Views.View";

    /// <summary>The fully qualified generator name written to generated-code attributes.</summary>
    private static readonly string GeneratorName = typeof(WireUpControlsGenerator).FullName!;

    /// <summary>The generator assembly version written to generated-code attributes.</summary>
    private static readonly string GeneratorVersion = typeof(WireUpControlsGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The <c>GeneratedCode</c> attribute stamped on generated members.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeDefinitions.WireUpControlsAttributeType,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, token) => ExtractInfo(in ctx, token))
                .Where(static x => x is not null)
                .Select(static (x, _) => x!)
                .WithTrackingName(TrackingNames.WireUpControlsTypes);

        context.RegisterSourceOutput(
            targets,
            static (spc, info) => spc.AddSource(
                $"{info.TargetInfo.FileHintName}.WireUpControls.g.cs",
                GenerateSource(info)));
    }

    /// <summary>Extracts Android view properties to wire from an attributed class.</summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The wire-up metadata, or <see langword="null"/> when the class is not partial.</returns>
    private static WireUpControlsInfo? ExtractInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (!((ClassDeclarationSyntax)context.TargetNode).Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return null;
        }

        token.ThrowIfCancellationRequested();

        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var explicitOptIn = GetExplicitOptIn(context.Attributes[0]);
        var isActivity = DerivesFrom(classSymbol, AndroidActivityTypeName);
        var isView = DerivesFrom(classSymbol, AndroidViewTypeName);

        using var controls = ArrayBuilder<WiredControlInfo>.Rent();

        foreach (var member in classSymbol.GetMembers())
        {
            token.ThrowIfCancellationRequested();

            if (member is IPropertySymbol { IsStatic: false, SetMethod: not null } property
                && TryCreateWiredControl(property, explicitOptIn) is { } wired)
            {
                controls.Add(wired);
            }
        }

        return new(TargetInfo.From(classSymbol), isActivity, isView, controls.ToEquatableArray());
    }

    /// <summary>Reads the <c>ExplicitOptIn</c> named argument from <c>[WireUpControls]</c>.</summary>
    /// <param name="attribute">The attribute data.</param>
    /// <returns><see langword="true"/> if explicit opt-in is enabled; otherwise <see langword="false"/>.</returns>
    private static bool GetExplicitOptIn(AttributeData attribute)
    {
        foreach (var named in attribute.NamedArguments)
        {
            if (named.Key == "ExplicitOptIn" && named.Value.Value is bool value)
            {
                return value;
            }
        }

        return false;
    }

    /// <summary>Determines whether a property should be wired and builds its <see cref="WiredControlInfo"/>.</summary>
    /// <param name="property">The property symbol.</param>
    /// <param name="explicitOptIn">Whether explicit opt-in mode is enabled.</param>
    /// <returns>The wired control info, or <see langword="null"/> if the property should be skipped.</returns>
    private static WiredControlInfo? TryCreateWiredControl(IPropertySymbol property, bool explicitOptIn)
    {
        if (HasAttribute(property, AttributeDefinitions.IgnoreResourceAttributeType))
        {
            return null;
        }

        var hasWireUpResource = TryGetWireUpResourceName(property, out var resourceOverride);
        if (explicitOptIn && !hasWireUpResource)
        {
            return null;
        }

        if (!explicitOptIn && !hasWireUpResource && property.Type is INamedTypeSymbol propType && !DerivesFrom(propType, AndroidViewTypeName))
        {
            return null;
        }

        var nonNullableType = property.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        var typeName = nonNullableType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var resourceName = string.IsNullOrEmpty(resourceOverride) ? property.Name : resourceOverride!;

        return new(property.Name, typeName, resourceName);
    }

    /// <summary>Checks whether a property carries <c>[WireUpResource]</c> and returns its optional resource name override.</summary>
    /// <param name="property">The property symbol.</param>
    /// <param name="resourceOverride">The resource name override, if specified.</param>
    /// <returns><see langword="true"/> if <c>[WireUpResource]</c> is present; otherwise <see langword="false"/>.</returns>
    private static bool TryGetWireUpResourceName(IPropertySymbol property, out string? resourceOverride)
    {
        resourceOverride = null;
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != AttributeDefinitions.WireUpResourceAttributeType)
            {
                continue;
            }

            if (!attribute.ConstructorArguments.IsEmpty && attribute.ConstructorArguments[0].Value is string name)
            {
                resourceOverride = name;
            }

            return true;
        }

        return false;
    }

    /// <summary>Checks whether a symbol directly carries the named attribute.</summary>
    /// <param name="symbol">The symbol to inspect.</param>
    /// <param name="attributeMetadataName">The full metadata name of the attribute.</param>
    /// <returns><see langword="true"/> if present; otherwise <see langword="false"/>.</returns>
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

    /// <summary>Checks whether <paramref name="typeSymbol"/> equals or derives from the named type.</summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="baseMetadataName">The full metadata name of the base type.</param>
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

    /// <summary>Generates the partial class containing compile-time <c>WireUpControls</c>.</summary>
    /// <param name="info">The wire-up metadata.</param>
    /// <returns>The generated C# source.</returns>
    private static string GenerateSource(WireUpControlsInfo info)
    {
        var writer = SourceWriter.Rent();
        _ = writer.AutoGenerated()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(info.TargetInfo.TargetNamespace)
            + writer.OpenContainingTypes(info.TargetInfo.ParentInfo);

        _ = writer.OpenPartialType(info.TargetInfo);

        if (info.IsActivity || info.IsView)
        {
            _ = writer.Summary("Wires child Android views to properties using FindViewById without runtime reflection.")
                .Line(GeneratedCodeAttribute)
                .Line(SourceWriterExtensions.ExcludeFromCodeCoverageAttribute)
                .Line("private void WireUpControls()")
                .OpenBlock();

            foreach (var control in info.Controls)
            {
                _ = writer.Append("this.").Append(control.PropertyName)
                    .Append(" = this.FindViewById<").Append(control.PropertyTypeName)
                    .Append(">(Resource.Id.").Append(control.ResourceName).Line(")!;");
            }

            _ = writer.CloseBlock();
        }
        else
        {
            _ = writer.Summary("Wires child Android views from the inflated root view using FindViewById without runtime reflection.")
                .Line(GeneratedCodeAttribute)
                .Line(SourceWriterExtensions.ExcludeFromCodeCoverageAttribute)
                .Line("private void WireUpControls(global::Android.Views.View rootView)")
                .OpenBlock()
                .Line("if (rootView is null) { throw new global::System.ArgumentNullException(nameof(rootView)); }");

            foreach (var control in info.Controls)
            {
                _ = writer.Append("this.").Append(control.PropertyName)
                    .Append(" = rootView.FindViewById<").Append(control.PropertyTypeName)
                    .Append(">(Resource.Id.").Append(control.ResourceName).Line(")!;");
            }

            _ = writer.CloseBlock();
        }

        _ = writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings();

        return writer.ToStringAndReturn();
    }
}
