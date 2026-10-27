// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>Makes a partial UI control implement <c>IViewFor&lt;TViewModel&gt;</c>, with a bindable <c>ViewModel</c> property.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class IViewForGenerator : IIncrementalGenerator
{
    /// <summary>Gets the fully qualified name used in generated-code metadata.</summary>
    internal static readonly string GeneratorName = typeof(IViewForGenerator).FullName!;

    /// <summary>Gets the assembly version used in generated-code metadata.</summary>
    internal static readonly string GeneratorVersion = typeof(IViewForGenerator).Assembly.GetName().Version.ToString();

    /// <summary>The <c>using</c> every view file starts with; <c>IViewFor</c> itself is named in full.</summary>
    private const string ReactiveUINamespace = "ReactiveUI";

    /// <summary>The modifier that opens each generated public member.</summary>
    private const string PublicModifier = "public ";

    /// <summary>The <c>GeneratedCode</c> attribute stamped on the generated view model property, built once.</summary>
    private static readonly string GeneratedCodeAttribute = GeneratorName.GeneratedCodeAttribute(GeneratorVersion);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // The generic IViewFor<T> and the IViewFor(string) forms are different metadata names, so each has its own
        // ForAttributeWithMetadataName; both share one predicate and one transform. Only a partial class can take the
        // generated members, so anything else is rejected from syntax before any binding.
        Func<SyntaxNode, CancellationToken, bool> predicate = static (node, _) =>
            node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        Func<GeneratorAttributeSyntaxContext, CancellationToken, IViewForInfo?> transform = static (context, token) => GetClassInfo(context, token);
        var named = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeDefinitions.IViewForAttributeType, predicate, transform).Collect();
        var generic = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeDefinitions.IViewForGenericAttributeType, predicate, transform).Collect();
        var types = named.Combine(generic)
            .SelectMany(static (pair, _) => Join(pair.Left, pair.Right))
            .GroupByTarget(static info => info.TargetInfo)
            .WithTrackingName(TrackingNames.ViewForTypes);

        // View registration is not generated here; ReactiveUI.Binding's view locator registers views. The interface is
        // named in full from the compilation's references, so it binds without a using for ReactiveUI.Binding.
        context.RegisterSourceOutput(types.Combine(context.ReactiveUiIntegration()), static (context, input) =>
        {
            var info = input.Left[0];

            // Only a supported UI framework base type gets a source.
            if (GenerateSource(info, input.Right.ViewNamespace) is not { } source)
            {
                return;
            }

            context.AddSource($"{info.TargetInfo.FileHintName}.IViewFor.g.cs", source);
        });
    }

    /// <summary>Joins the views found through each attribute form, dropping the targets that were declined.</summary>
    /// <param name="named">The views marked <c>[IViewFor("...")]</c>.</param>
    /// <param name="generic">The views marked <c>[IViewFor&lt;T&gt;]</c>.</param>
    /// <returns>The views.</returns>
    private static IViewForInfo[] Join(ImmutableArray<IViewForInfo?> named, ImmutableArray<IViewForInfo?> generic)
    {
        var views = new List<IViewForInfo>(named.Length + generic.Length);
        foreach (var info in named)
        {
            if (info is not null)
            {
                views.Add(info);
            }
        }

        foreach (var info in generic)
        {
            if (info is not null)
            {
                views.Add(info);
            }
        }

        return views.ToArray();
    }

    /// <summary>Creates the generation model for an annotated class declaration.</summary>
    /// <param name="context">The Roslyn context for the annotated declaration.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The generation model, or <see langword="null"/> when the target is unsupported.</returns>
    private static IViewForInfo? GetClassInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return default;
        }

        var viewModelTypeName = GetViewModelTypeName(context.Attributes[0]);
        if (string.IsNullOrWhiteSpace(viewModelTypeName))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();
        return new(TargetInfo.From(classSymbol), viewModelTypeName!, GetBaseType(classSymbol));
    }

    /// <summary>Reads the view model type an <c>[IViewFor]</c> names, by type argument or by the text it was given.</summary>
    /// <param name="attributeData">The attribute.</param>
    /// <returns>The view model type name, or <see langword="null"/> when the attribute names none.</returns>
    private static string? GetViewModelTypeName(AttributeData attributeData)
    {
        if (attributeData.AttributeClass is { IsGenericType: true, TypeArguments.Length: > 0 } attributeClass)
        {
            return attributeClass.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        using var arguments = attributeData.GetConstructorArguments<string>().GetEnumerator();
        return arguments.MoveNext() ? arguments.Current : null;
    }

    /// <summary>Identifies the supported UI framework represented by a class symbol.</summary>
    /// <param name="classSymbol">The class symbol to inspect.</param>
    /// <returns>The matching supported base type, or <see cref="IViewForBaseType.None"/>.</returns>
    private static IViewForBaseType GetBaseType(INamedTypeSymbol classSymbol)
    {
        var baseType = classSymbol.BaseType;
        if (baseType is null)
        {
            return IViewForBaseType.None;
        }

        if (baseType.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Forms"))
        {
            return IViewForBaseType.WinForms;
        }

        if (baseType.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows"))
        {
            return IViewForBaseType.Wpf;
        }

        if (baseType.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml"))
        {
            return IViewForBaseType.WinUI;
        }

        if (baseType.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.Maui"))
        {
            return IViewForBaseType.Maui;
        }

        if (baseType.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("Avalonia"))
        {
            return IViewForBaseType.Avalonia;
        }

        return baseType.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml")
            ? IViewForBaseType.Uno
            : IViewForBaseType.None;
    }

    /// <summary>Generates the partial type source for a supported <c>IViewFor</c> target.</summary>
    /// <param name="info">The generation model.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    /// <returns>The generated source, or <see langword="null"/> for unsupported types.</returns>
    private static string? GenerateSource(IViewForInfo info, string viewNamespace)
    {
        var writer = info.BaseType switch
        {
            IViewForBaseType.Wpf => BeginDependencyPropertyFile("System.Windows"),
            IViewForBaseType.WinUI => BeginDependencyPropertyFile("Microsoft.UI.Xaml"),
            IViewForBaseType.Uno => BeginDependencyPropertyFile("Windows.UI.Xaml"),
            IViewForBaseType.WinForms => BeginFile().Using(ReactiveUINamespace).Using("System.ComponentModel"),
            IViewForBaseType.Avalonia => BeginFile().Using("System").Using(ReactiveUINamespace).Using("Avalonia").Using("Avalonia.Controls"),
            IViewForBaseType.Maui => BeginFile().Using("System").Using(ReactiveUINamespace).Using("Microsoft.Maui.Controls"),
            _ => null,
        };

        if (writer is null)
        {
            return null;
        }

        // Only the dependency-property file opens with the usual pragma pair; the others restore nullability first.
        if (info.BaseType is not (IViewForBaseType.Wpf or IViewForBaseType.WinUI or IViewForBaseType.Uno))
        {
            _ = writer.Line("#nullable restore").Line("#pragma warning disable").BlankLine();
        }

        var target = info.TargetInfo;
        var depth = writer.OpenNamespace(target.TargetNamespace) + writer.OpenContainingTypes(target.ParentInfo);
        WriteTypeDeclaration(writer, info, viewNamespace, info.BaseType != IViewForBaseType.Maui);
        if (info.BaseType == IViewForBaseType.WinForms)
        {
            WriteWinFormsMembers(writer, info.ViewModelTypeName, viewNamespace);
        }
        else if (info.BaseType == IViewForBaseType.Avalonia)
        {
            WriteAvaloniaMembers(writer, info, viewNamespace);
        }
        else if (info.BaseType == IViewForBaseType.Maui)
        {
            WriteMauiMembers(writer, info.ViewModelTypeName, viewNamespace);
        }
        else
        {
            WriteDependencyPropertyMembers(writer, info, viewNamespace);
        }

        return writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Rents a writer and writes the auto-generated marker every view file starts with.</summary>
    /// <returns>The writer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SourceWriter BeginFile() => SourceWriter.Rent().AutoGenerated();

    /// <summary>Starts a WPF, WinUI or Uno view file, whose imports differ only in the XAML namespace.</summary>
    /// <param name="xamlNamespace">The namespace declaring <c>DependencyProperty</c>.</param>
    /// <returns>The writer, after the file's header.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SourceWriter BeginDependencyPropertyFile(string xamlNamespace) =>
        BeginFile()
            .Using(ReactiveUINamespace)
            .Using(xamlNamespace)
            .BlankLine()
            .DisableWarningsEnableNullable()
            .BlankLine();

    /// <summary>Opens the view's partial declaration implementing <c>IViewFor</c>.</summary>
    /// <param name="writer">The writer, at the level of the view's declaration.</param>
    /// <param name="info">The generation model.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    /// <param name="includeSummary">Whether the declaration carries a summary doc comment.</param>
    private static void WriteTypeDeclaration(SourceWriter writer, IViewForInfo info, string viewNamespace, bool includeSummary)
    {
        var target = info.TargetInfo;
        if (includeSummary)
        {
            _ = writer.Line("/// <summary>")
                .Append("/// Partial class for the ").Append(target.TargetName).Line(" which contains ReactiveUI IViewFor initialization.")
                .Line("/// </summary>");
        }

        _ = writer.ExcludeFromCodeCoverage()
            .BeginPartialType(target).Append(" : ").Append(viewNamespace).Append(".IViewFor<").Append(info.ViewModelTypeName).Line(">")
            .OpenBlock();
    }

    /// <summary>Writes the members of a WPF, WinUI or Uno view, backed by a dependency property.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="info">The generation model.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    private static void WriteDependencyPropertyMembers(SourceWriter writer, IViewForInfo info, string viewNamespace)
    {
        var viewModelType = info.ViewModelTypeName;
        _ = writer.Lines("""
                /// <summary>
                /// The view model dependency property.
                /// </summary>
                """)
            .Line(GeneratedCodeAttribute)
            .Append("public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(")
            .Append(viewModelType).Append("), typeof(").Append(info.TargetInfo.TargetName).Line("), new PropertyMetadata(null));")
            .BlankLine();
        WriteViewModelMembers(writer, viewModelType, string.Empty, viewNamespace);
    }

    /// <summary>Writes the members of a Windows Forms view, backed by an auto-property.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="viewModelType">The view model's type name.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    private static void WriteWinFormsMembers(SourceWriter writer, string viewModelType, string viewNamespace) =>
        _ = writer.Lines("""
                /// <inheritdoc/>
                [Category("ReactiveUI")]
                [Description("The ViewModel.")]
                [Bindable(true)]
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                """)
            .Line(GeneratedCodeAttribute)
            .Append(PublicModifier).Append(viewModelType).Line("? ViewModel {get; set; }")
            .BlankLine()
            .InheritDoc()
            .Append("object? ").Append(viewNamespace).Append(".IViewFor.ViewModel {get => ViewModel; set => ViewModel = (").Append(viewModelType).Line("? )value; }");

    /// <summary>Writes the members of an Avalonia view, backed by a styled property kept in step with the data context.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="info">The generation model.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    private static void WriteAvaloniaMembers(SourceWriter writer, IViewForInfo info, string viewNamespace)
    {
        var viewModelType = info.ViewModelTypeName;
        _ = writer.Lines("""
                /// <summary>
                /// The view model dependency property.
                /// </summary>
                [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002", Justification = "Generic avalonia property is expected here.")]
                """)
            .Append("public static readonly StyledProperty<").Append(viewModelType)
            .Append("?> ViewModelProperty = AvaloniaProperty.Register<").Append(info.TargetInfo.TargetName).Append(", ").Append(viewModelType)
            .Line(">(nameof(ViewModel));")
            .BlankLine();
        WriteViewModelMembers(writer, viewModelType, "?", viewNamespace);
        _ = writer.BlankLine()
            .Lines("""
                protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
                {
                    base.OnPropertyChanged(change);

                    if (change.Property == DataContextProperty)
                    {
                """)
            .Indent().Indent()
            .Append("if (ReferenceEquals(change.OldValue, ViewModel) && change.NewValue is null or ").Append(viewModelType).Line(")")
            .Outdent().Outdent()
            .Lines("""
                        {
                            SetCurrentValue(ViewModelProperty, change.NewValue);
                        }
                    }
                    else if (change.Property == ViewModelProperty)
                    {
                        if (ReferenceEquals(change.OldValue, DataContext))
                        {
                            SetCurrentValue(DataContextProperty, change.NewValue);
                        }
                    }
                }
                """);
    }

    /// <summary>Writes the members of a MAUI view, backed by a bindable property kept in step with the binding context.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="viewModelType">The view model's type name.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    private static void WriteMauiMembers(SourceWriter writer, string viewModelType, string viewNamespace)
    {
        _ = writer.Append("public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(")
            .Append(viewModelType).Append("), typeof(").Append(viewNamespace).Append(".IViewFor<").Append(viewModelType).Append(">), default(").Append(viewModelType)
            .Line("), BindingMode.OneWay, propertyChanged: OnViewModelChanged);")
            .BlankLine();
        WriteViewModelMembers(writer, viewModelType, "?", viewNamespace);
        _ = writer.BlankLine()
            .InheritDoc()
            .Line("protected override void OnBindingContextChanged()")
            .OpenBlock()
            .Line("base.OnBindingContextChanged();")
            .Append("ViewModel = BindingContext as ").Append(viewModelType).EndStatement()
            .CloseBlock()
            .BlankLine()
            .Line("private static void OnViewModelChanged(BindableObject bindableObject, object oldValue, object newValue) => bindableObject.BindingContext = newValue;");
    }

    /// <summary>Writes the binding root and the typed and untyped <c>ViewModel</c> properties over <c>ViewModelProperty</c>.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="viewModelType">The view model's type name.</param>
    /// <param name="nullableSuffix">The annotation after the view model type: <c>?</c>, or empty.</param>
    /// <param name="viewNamespace">The qualified namespace declaring <c>IViewFor</c>.</param>
    private static void WriteViewModelMembers(SourceWriter writer, string viewModelType, string nullableSuffix, string viewNamespace) =>
        _ = writer.Lines("""
                /// <summary>
                /// Gets the binding root view model.
                /// </summary>
                """)
            .Append(PublicModifier).Append(viewModelType).Append(nullableSuffix).Line(" BindingRoot => ViewModel;")
            .BlankLine()
            .InheritDoc()
            .Append(PublicModifier).Append(viewModelType).Append(nullableSuffix).Append(" ViewModel { get => (").Append(viewModelType).Append(nullableSuffix)
            .Line(")GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }")
            .BlankLine()
            .InheritDoc()
            .Append("object? ").Append(viewNamespace).Append(".IViewFor.ViewModel { get => ViewModel; set => ViewModel = (").Append(viewModelType).Append(nullableSuffix).Line(")value; }");
}
