// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Input.Models;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// IViewForGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class IViewForGenerator
{
    internal static readonly string GeneratorName = typeof(IViewForGenerator).FullName!;
    internal static readonly string GeneratorVersion = typeof(IViewForGenerator).Assembly.GetName().Version.ToString();

    private static readonly string[] excludeFromCodeCoverage = ["[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]"];

    private static IViewForInfo? GetClassInfo(in GenericGeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (!(context.TargetNode is ClassDeclarationSyntax declaredClass && declaredClass.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            return default;
        }

        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.IViewForAttributeType, out var attributeData))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();
        if (symbol is not INamedTypeSymbol classSymbol)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        var constructorArgument = attributeData.GetConstructorArguments<string>().FirstOrDefault();
        var genericArgument = attributeData.GetGenericType();
        token.ThrowIfCancellationRequested();
        var viewModelTypeName = string.IsNullOrWhiteSpace(constructorArgument) ? genericArgument : constructorArgument;
        if (string.IsNullOrWhiteSpace(viewModelTypeName))
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        var viewForBaseType = IViewForBaseType.None;
        if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Forms"))
        {
            viewForBaseType = IViewForBaseType.WinForms;
        }
        else if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows") || classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("System.Windows.Controls"))
        {
            viewForBaseType = IViewForBaseType.Wpf;
        }
        else if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml") || classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.UI.Xaml.Controls"))
        {
            viewForBaseType = IViewForBaseType.WinUI;
        }
        else if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Microsoft.Maui"))
        {
            viewForBaseType = IViewForBaseType.Maui;
        }
        else if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Avalonia"))
        {
            viewForBaseType = IViewForBaseType.Avalonia;
        }
        else if (classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml") || classSymbol.InheritsFromFullyQualifiedMetadataNameStartingWith("Windows.UI.Xaml.Controls"))
        {
            viewForBaseType = IViewForBaseType.Uno;
        }

        // Get the containing type info
        var targetInfo = TargetInfo.From(classSymbol);

        token.ThrowIfCancellationRequested();

        // Get RegistrationType enum value from the attribute
        attributeData.TryGetNamedArgument("RegistrationType", out int splatRegistrationType);
        var registrationType = splatRegistrationType switch
        {
            1 => "RegisterLazySingleton",
            2 => "RegisterConstant",
            3 => "Register",
            _ => string.Empty,
        };

        token.ThrowIfCancellationRequested();

        // Get RegistrationType enum value from the attribute
        attributeData.TryGetNamedArgument("ViewModelRegistrationType", out int splatViewModelRegistrationType);
        var viewModelRegistrationType = splatViewModelRegistrationType switch
        {
            1 => "RegisterLazySingleton",
            2 => "RegisterConstant",
            3 => "Register",
            _ => string.Empty,
        };

        return new(
            targetInfo,
            viewModelTypeName!,
            viewForBaseType,
            registrationType,
            viewModelRegistrationType);
    }

    private static string GenerateSource(string containingTypeName, string containingNamespace, string containingClassVisibility, string containingType, IViewForInfo iviewForInfo)
    {
        // Prepare any forwarded property attributes
        var forwardedAttributesString = string.Join("\n        ", excludeFromCodeCoverage);

        switch (iviewForInfo.BaseType)
        {
            case IViewForBaseType.None:
                break;
            case IViewForBaseType.Wpf:
            case IViewForBaseType.WinUI:
            case IViewForBaseType.Uno:
                var usings = iviewForInfo.BaseType switch
                {
                    IViewForBaseType.Wpf => """
                        using ReactiveUI;
                        using System.Windows;
                        """,
                    IViewForBaseType.WinUI => """
                        using ReactiveUI;
                        using Microsoft.UI.Xaml;
                        """,
                    IViewForBaseType.Uno => """
                        using ReactiveUI;
                        using Windows.UI.Xaml;
                        """,
                    _ => string.Empty,
                };
                return
$$"""
// <auto-generated/>
{{usings}}

#pragma warning disable
#nullable enable

namespace {{containingNamespace}}
{
    /// <summary>
    /// Partial class for the {{containingTypeName}} which contains ReactiveUI IViewFor initialization.
    /// </summary>
    {{forwardedAttributesString}}
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}} : IViewFor<{{iviewForInfo.ViewModelTypeName}}>
    {
        /// <summary>
        /// The view model dependency property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof({{iviewForInfo.ViewModelTypeName}}), typeof({{containingTypeName}}), new PropertyMetadata(null));

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public {{iviewForInfo.ViewModelTypeName}} BindingRoot => ViewModel;

        /// <inheritdoc/>
        public {{iviewForInfo.ViewModelTypeName}} ViewModel { get => ({{iviewForInfo.ViewModelTypeName}})GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        /// <inheritdoc/>
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = ({{iviewForInfo.ViewModelTypeName}})value; }
    }
}
#nullable restore
#pragma warning restore
""";
            case IViewForBaseType.WinForms:
                return
$$"""
// <auto-generated/>
using ReactiveUI;
using System.ComponentModel;
#nullable restore
#pragma warning disable

namespace {{containingNamespace}}
{
    /// <summary>
    /// Partial class for the {{containingTypeName}} which contains ReactiveUI IViewFor initialization.
    /// </summary>
    {{forwardedAttributesString}}
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}} : IViewFor<{{iviewForInfo.ViewModelTypeName}}>
    {
        /// <inheritdoc/>
        [Category("ReactiveUI")]
        [Description("The ViewModel.")]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        public {{iviewForInfo.ViewModelTypeName}}? ViewModel {get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel {get => ViewModel; set => ViewModel = ({{iviewForInfo.ViewModelTypeName}}? )value; }
    }
}
#nullable restore
#pragma warning restore
""";
            case IViewForBaseType.Avalonia:
                return
$$"""
// <auto-generated/>
using System;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
#nullable restore
#pragma warning disable

namespace {{containingNamespace}}
{
    /// <summary>
    /// Partial class for the {{containingTypeName}} which contains ReactiveUI IViewFor initialization.
    /// </summary>
    {{forwardedAttributesString}}
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}} : IViewFor<{{iviewForInfo.ViewModelTypeName}}>
    {
        /// <summary>
        /// The view model dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002", Justification = "Generic avalonia property is expected here.")]
        public static readonly StyledProperty<{{iviewForInfo.ViewModelTypeName}}?> ViewModelProperty = AvaloniaProperty.Register<{{containingTypeName}}, {{iviewForInfo.ViewModelTypeName}}>(nameof(ViewModel));

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public {{iviewForInfo.ViewModelTypeName}}? BindingRoot => ViewModel;

        /// <inheritdoc/>
        public {{iviewForInfo.ViewModelTypeName}}? ViewModel { get => ({{iviewForInfo.ViewModelTypeName}}?)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        /// <inheritdoc/>
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = ({{iviewForInfo.ViewModelTypeName}}?)value; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == DataContextProperty)
            {
                if (ReferenceEquals(change.OldValue, ViewModel) && change.NewValue is null or {{iviewForInfo.ViewModelTypeName}})
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
    }
}
#nullable restore
#pragma warning restore
""";
            case IViewForBaseType.Maui:
                return
$$"""
// <auto-generated/>
using System;
using ReactiveUI;
using Microsoft.Maui.Controls;
#nullable restore
#pragma warning disable

namespace {{containingNamespace}}
{
    {{forwardedAttributesString}}
    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}} : IViewFor<{{iviewForInfo.ViewModelTypeName}}>
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof({{iviewForInfo.ViewModelTypeName}}), typeof(IViewFor<{{iviewForInfo.ViewModelTypeName}}>), default({{iviewForInfo.ViewModelTypeName}}), BindingMode.OneWay, propertyChanged: OnViewModelChanged);

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public {{iviewForInfo.ViewModelTypeName}}? BindingRoot => ViewModel;

        /// <inheritdoc/>
        public {{iviewForInfo.ViewModelTypeName}}? ViewModel { get => ({{iviewForInfo.ViewModelTypeName}}?)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        /// <inheritdoc/>
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = ({{iviewForInfo.ViewModelTypeName}}?)value; }

        /// <inheritdoc/>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            ViewModel = BindingContext as {{iviewForInfo.ViewModelTypeName}};
        }

        private static void OnViewModelChanged(BindableObject bindableObject, object oldValue, object newValue) => bindableObject.BindingContext = newValue;
    }
}
#nullable restore
#pragma warning restore
""";
        }

        return string.Empty;
    }

    private static string GenerateRegistrationExtensions(in ImmutableArray<IViewForInfo> iviewForInfo)
    {
        // Collapse to unique registrations and skip entries with no registration
        var registrations = iviewForInfo
            .Where(static x => !string.IsNullOrWhiteSpace(x.SplatRegistrationType))
            .GroupBy(static x => (x.TargetInfo.TargetNamespaceWithNamespace, x.ViewModelTypeName, x.SplatRegistrationType))
            .Select(static g => g.First())
            .ToImmutableArray();

        var viewModelRegistrations = iviewForInfo
            .Where(static x => !string.IsNullOrWhiteSpace(x.SplatViewModelRegistrationType))
            .GroupBy(static x => (x.TargetInfo.TargetNamespaceWithNamespace, x.ViewModelTypeName, x.SplatRegistrationType))
            .Select(static g => g.First())
            .ToImmutableArray();

        var sb = new StringBuilder();
        sb.AppendLine("if (resolver is null) throw new global::System.ArgumentNullException(nameof(resolver));");
        foreach (var item in registrations)
        {
            var vmType = item.ViewModelTypeName;
            if (!string.IsNullOrEmpty(vmType) && !vmType.StartsWith("global::", System.StringComparison.Ordinal))
            {
                vmType = "global::" + vmType;
            }

            var serviceType = "global::ReactiveUI.IViewFor<" + vmType + ">";
            var viewType = item.TargetInfo.TargetNamespaceWithNamespace; // already fully-qualified

            // resolver.Register*/<IViewFor<VM>, View>();
            switch (item.SplatRegistrationType)
            {
                case "RegisterLazySingleton":
                    sb.AppendLine($"            resolver.{item.SplatRegistrationType}<{serviceType}>(() => new {viewType}());");
                    break;
                case "Register":
                    sb.AppendLine($"            resolver.{item.SplatRegistrationType}<{serviceType}, {viewType}>();");
                    break;
                case "RegisterConstant":
                    sb.AppendLine($"            resolver.{item.SplatRegistrationType}<{serviceType}>(new {viewType}());");
                    break;
            }
        }

        foreach (var item in viewModelRegistrations)
        {
            var vmType = item.ViewModelTypeName;
            if (!string.IsNullOrEmpty(vmType) && !vmType.StartsWith("global::", System.StringComparison.Ordinal))
            {
                vmType = "global::" + vmType;
            }

            // resolver.Register*/<VM, VM>();
            switch (item.SplatViewModelRegistrationType)
            {
                case "RegisterLazySingleton":
                    sb.AppendLine($"            resolver.{item.SplatViewModelRegistrationType}<{vmType}>(() => new {vmType}());");
                    break;
                case "Register":
                    sb.AppendLine($"            resolver.{item.SplatViewModelRegistrationType}<{vmType}, {vmType}>();");
                    break;
                case "RegisterConstant":
                    sb.AppendLine($"            resolver.{item.SplatViewModelRegistrationType}<{vmType}>(new {vmType}());");
                    break;
            }
        }

        var registrationsBody = sb.ToString().TrimEnd();
        return
        $$"""
// <auto-generated/>
#pragma warning disable
#nullable enable

using global::ReactiveUI;
using global::Splat;

namespace ReactiveUI.SourceGenerators
{
    /// <summary>
    /// Source-generated registration extensions for ReactiveUI views.
    /// </summary>
    internal static class ReactiveUISourceGeneratorsExtensions
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        public static void RegisterViewsForViewModelsSourceGenerated(this global::Splat.IMutableDependencyResolver resolver)
        {
            {{registrationsBody}}
        }
    }
}
#nullable restore
#pragma warning restore
""";
    }
}
