// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>Contains implementation details for the <see cref="IViewForGenerator"/> source generator.</summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class IViewForGenerator
{
    /// <summary>Gets the assembly version used in generated-code metadata.</summary>
    internal static readonly string GeneratorVersion = typeof(IViewForGenerator).Assembly.GetName().Version.ToString();

    /// <summary>Gets the fully qualified name used in generated-code metadata.</summary>
    internal static readonly string GeneratorName = typeof(IViewForGenerator).FullName!;

    /// <summary>The namespace every view file imports <c>IViewFor</c> from.</summary>
    private const string ReactiveUINamespace = "ReactiveUI";

    /// <summary>The modifier that opens each generated public member.</summary>
    private const string PublicModifier = "public ";

    /// <summary>The <c>GeneratedCode</c> attribute stamped on the generated view model property, built once.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <summary>Creates the generation model for an annotated class declaration.</summary>
    /// <param name="context">The Roslyn context for the annotated declaration.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The generation model, or <see langword="null"/> when the target is unsupported.</returns>
    private static IViewForInfo? GetClassInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        var attributeData = context.Attributes[0];

        token.ThrowIfCancellationRequested();
        if (symbol is not INamedTypeSymbol classSymbol)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        using var constructorArguments = attributeData.GetConstructorArguments<string>().GetEnumerator();
        var constructorArgument = constructorArguments.MoveNext() ? constructorArguments.Current : null;

        var genericArgument = attributeData.GetGenericType();
        token.ThrowIfCancellationRequested();
        var viewModelTypeName = string.IsNullOrWhiteSpace(constructorArgument) ? genericArgument : constructorArgument;
        if (string.IsNullOrWhiteSpace(viewModelTypeName))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        var viewForBaseType = GetBaseType(classSymbol);

        // Get the containing type info
        var targetInfo = TargetInfo.From(classSymbol);

        token.ThrowIfCancellationRequested();

        return new(
            targetInfo,
            viewModelTypeName!,
            viewForBaseType);
    }

    /// <summary>Identifies the supported UI framework represented by a class symbol.</summary>
    /// <param name="classSymbol">The class symbol to inspect.</param>
    /// <returns>The matching supported base type, or <see cref="IViewForBaseType.None"/>.</returns>
    private static IViewForBaseType GetBaseType(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Forms"))
        {
            return IViewForBaseType.WinForms;
        }

        if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows")
            || classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Controls"))
        {
            return IViewForBaseType.Wpf;
        }

        if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml")
            || classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml.Controls"))
        {
            return IViewForBaseType.WinUI;
        }

        if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.Maui"))
        {
            return IViewForBaseType.Maui;
        }

        if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Avalonia"))
        {
            return IViewForBaseType.Avalonia;
        }

        return classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml")
            || classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml.Controls")
            ? IViewForBaseType.Uno
            : IViewForBaseType.None;
    }

    /// <summary>Generates the partial type source for a supported <c>IViewFor</c> target.</summary>
    /// <param name="info">The generation model.</param>
    /// <returns>The generated source, or <see langword="null"/> for unsupported types.</returns>
    private static string? GenerateSource(IViewForInfo info)
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
        WriteTypeDeclaration(writer, info, info.BaseType != IViewForBaseType.Maui);
        if (info.BaseType == IViewForBaseType.WinForms)
        {
            WriteWinFormsMembers(writer, info.ViewModelTypeName);
        }
        else if (info.BaseType == IViewForBaseType.Avalonia)
        {
            WriteAvaloniaMembers(writer, info);
        }
        else if (info.BaseType == IViewForBaseType.Maui)
        {
            WriteMauiMembers(writer, info.ViewModelTypeName);
        }
        else
        {
            WriteDependencyPropertyMembers(writer, info);
        }

        return writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Rents a writer and writes the auto-generated marker every view file starts with.</summary>
    /// <returns>The writer.</returns>
    private static SourceWriter BeginFile() => SourceWriter.Rent().AutoGenerated();

    /// <summary>Starts a WPF, WinUI or Uno view file, whose imports differ only in the XAML namespace.</summary>
    /// <param name="xamlNamespace">The namespace declaring <c>DependencyProperty</c>.</param>
    /// <returns>The writer, after the file's header.</returns>
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
    /// <param name="includeSummary">Whether the declaration carries a summary doc comment.</param>
    private static void WriteTypeDeclaration(SourceWriter writer, IViewForInfo info, bool includeSummary)
    {
        var target = info.TargetInfo;
        if (includeSummary)
        {
            _ = writer.Line("/// <summary>")
                .Append("/// Partial class for the ").Append(target.TargetName).Line(" which contains ReactiveUI IViewFor initialization.")
                .Line("/// </summary>");
        }

        foreach (var attribute in AttributeDefinitions.ExcludeFromCodeCoverage)
        {
            _ = writer.Line(attribute);
        }

        _ = writer.BeginPartialType(target).Append(" : IViewFor<").Append(info.ViewModelTypeName).Line(">").OpenBlock();
    }

    /// <summary>Writes the members of a WPF, WinUI or Uno view, backed by a dependency property.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="info">The generation model.</param>
    private static void WriteDependencyPropertyMembers(SourceWriter writer, IViewForInfo info)
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
        WriteViewModelMembers(writer, viewModelType, string.Empty);
    }

    /// <summary>Writes the members of a Windows Forms view, backed by an auto-property.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="viewModelType">The view model's type name.</param>
    private static void WriteWinFormsMembers(SourceWriter writer, string viewModelType) =>
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
            .Append("object? IViewFor.ViewModel {get => ViewModel; set => ViewModel = (").Append(viewModelType).Line("? )value; }");

    /// <summary>Writes the members of an Avalonia view, backed by a styled property kept in step with the data context.</summary>
    /// <param name="writer">The writer, at the level of the view's members.</param>
    /// <param name="info">The generation model.</param>
    private static void WriteAvaloniaMembers(SourceWriter writer, IViewForInfo info)
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
        WriteViewModelMembers(writer, viewModelType, "?");
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
    private static void WriteMauiMembers(SourceWriter writer, string viewModelType)
    {
        _ = writer.Append("public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(")
            .Append(viewModelType).Append("), typeof(IViewFor<").Append(viewModelType).Append(">), default(").Append(viewModelType)
            .Line("), BindingMode.OneWay, propertyChanged: OnViewModelChanged);")
            .BlankLine();
        WriteViewModelMembers(writer, viewModelType, "?");
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
    private static void WriteViewModelMembers(SourceWriter writer, string viewModelType, string nullableSuffix) =>
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
            .Append("object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (").Append(viewModelType).Append(nullableSuffix).Line(")value; }");
}
