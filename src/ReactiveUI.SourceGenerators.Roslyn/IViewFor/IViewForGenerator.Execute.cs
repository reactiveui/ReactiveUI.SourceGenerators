// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    /// <summary>The attribute value that selects constant registration.</summary>
    private const int RegisterConstantValue = 2;

    /// <summary>The attribute value that selects transient registration.</summary>
    private const int RegisterValue = 3;

    /// <summary>Creates the generation model for an annotated class declaration.</summary>
    /// <param name="context">The Roslyn context for the annotated declaration.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The generation model, or <see langword="null"/> when the target is unsupported.</returns>
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

        // Get RegistrationType enum value from the attribute
        _ = attributeData.TryGetNamedArgument("RegistrationType", out int splatRegistrationType);
        var registrationType = GetRegistrationType(splatRegistrationType);

        token.ThrowIfCancellationRequested();

        // Get ViewModelRegistrationType enum value from the attribute
        _ = attributeData.TryGetNamedArgument("ViewModelRegistrationType", out int splatViewModelRegistrationType);
        var viewModelRegistrationType = GetRegistrationType(splatViewModelRegistrationType);

        return new(
            targetInfo,
            viewModelTypeName!,
            viewForBaseType,
            registrationType,
            viewModelRegistrationType);
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

    /// <summary>Maps an attribute registration value to its Splat registration method name.</summary>
    /// <param name="registrationType">The integer value supplied by the attribute.</param>
    /// <returns>The Splat registration method name, or an empty string for no registration.</returns>
    private static string GetRegistrationType(int registrationType) => registrationType switch
    {
        1 => "RegisterLazySingleton",
        RegisterConstantValue => "RegisterConstant",
        RegisterValue => "Register",
        _ => string.Empty,
    };

    /// <summary>Generates the partial type source for a supported <c>IViewFor</c> target.</summary>
    /// <param name="viewForInfo">The generation model.</param>
    /// <param name="parentInfo">The enclosing type model, when the target is nested.</param>
    /// <returns>The generated source, or an empty string for unsupported types.</returns>
    private static string GenerateSource(
        IViewForInfo viewForInfo,
        TargetInfo? parentInfo = null)
    {
        var forwardedAttributesString = string.Join("\n        ", AttributeDefinitions.ExcludeFromCodeCoverage);
        var (parentDeclarations, parentClosing) = parentInfo is null
            ? (string.Empty, string.Empty)
            : Models.TargetInfo.GenerateParentClassDeclarations([parentInfo]);
        return viewForInfo.BaseType switch
        {
            IViewForBaseType.Wpf or IViewForBaseType.WinUI or IViewForBaseType.Uno => GenerateDependencyPropertyViewSource(viewForInfo, parentDeclarations, parentClosing, forwardedAttributesString),
            IViewForBaseType.WinForms => GenerateWinFormsViewSource(viewForInfo, parentDeclarations, parentClosing, forwardedAttributesString),
            IViewForBaseType.Avalonia => GenerateAvaloniaViewSource(viewForInfo, parentDeclarations, parentClosing, forwardedAttributesString),
            IViewForBaseType.Maui => GenerateMauiViewSource(viewForInfo, parentDeclarations, parentClosing, forwardedAttributesString),
            _ => string.Empty,
        };
    }

    /// <summary>Generates source for WPF, WinUI, and Uno views.</summary>
    /// <param name="info">The IViewFor generation model.</param>
    /// <param name="parents">The enclosing type declarations.</param>
    /// <param name="closingParents">The enclosing type closures.</param>
    /// <param name="attributes">The forwarded attributes.</param>
    /// <returns>The generated source.</returns>
    private static string GenerateDependencyPropertyViewSource(IViewForInfo info, string parents, string closingParents, string attributes)
    {
        var usings = info.BaseType switch
        {
            IViewForBaseType.Wpf => "using ReactiveUI;\nusing System.Windows;",
            IViewForBaseType.WinUI => "using ReactiveUI;\nusing Microsoft.UI.Xaml;",
            IViewForBaseType.Uno => "using ReactiveUI;\nusing Windows.UI.Xaml;",
            _ => string.Empty,
        };
        var viewModelPropertyDeclaration = GetDependencyPropertyDeclaration(info);

        return $$"""
// <auto-generated/>
{{usings}}

#pragma warning disable
#nullable enable

namespace {{info.TargetInfo.TargetNamespace}}
{
{{parents}}    /// <summary>
    /// Partial class for the {{info.TargetInfo.TargetName}} which contains ReactiveUI IViewFor initialization.
    /// </summary>
    {{attributes}}
    {{info.TargetInfo.TargetVisibility}} partial {{info.TargetInfo.TargetType}} {{info.TargetInfo.TargetName}} : IViewFor<{{info.ViewModelTypeName}}>
    {
        /// <summary>
        /// The view model dependency property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{viewModelPropertyDeclaration}}

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public {{info.ViewModelTypeName}} BindingRoot => ViewModel;

        /// <inheritdoc/>
        public {{info.ViewModelTypeName}} ViewModel { get => ({{info.ViewModelTypeName}})GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        /// <inheritdoc/>
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = ({{info.ViewModelTypeName}})value; }
    }
{{closingParents}}}
#nullable restore
#pragma warning restore
""";
    }

    /// <summary>Creates the dependency-property declaration while preserving its generated one-line format.</summary>
    /// <param name="info">The IViewFor generation model.</param>
    /// <returns>The generated dependency-property declaration.</returns>
    private static string GetDependencyPropertyDeclaration(IViewForInfo info)
    {
        var builder = new StringBuilder("        public static readonly DependencyProperty ViewModelProperty = ");
        _ = builder.Append("DependencyProperty.Register(nameof(ViewModel), typeof(");
        _ = builder.Append(info.ViewModelTypeName).Append("), typeof(");
        _ = builder.Append(info.TargetInfo.TargetName).Append("), new PropertyMetadata(null));");
        return builder.ToString();
    }

    /// <summary>Generates source for Windows Forms views.</summary>
    /// <param name="info">The IViewFor generation model.</param>
    /// <param name="parents">The enclosing type declarations.</param>
    /// <param name="closingParents">The enclosing type closures.</param>
    /// <param name="attributes">The forwarded attributes.</param>
    /// <returns>The generated source.</returns>
    private static string GenerateWinFormsViewSource(IViewForInfo info, string parents, string closingParents, string attributes) =>
        $$"""
// <auto-generated/>
using ReactiveUI;
using System.ComponentModel;
#nullable restore
#pragma warning disable

namespace {{info.TargetInfo.TargetNamespace}}
{
{{parents}}    /// <summary>
    /// Partial class for the {{info.TargetInfo.TargetName}} which contains ReactiveUI IViewFor initialization.
    /// </summary>
    {{attributes}}
    {{info.TargetInfo.TargetVisibility}} partial {{info.TargetInfo.TargetType}} {{info.TargetInfo.TargetName}} : IViewFor<{{info.ViewModelTypeName}}>
    {
        /// <inheritdoc/>
        [Category("ReactiveUI")]
        [Description("The ViewModel.")]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        public {{info.ViewModelTypeName}}? ViewModel {get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel {get => ViewModel; set => ViewModel = ({{info.ViewModelTypeName}}? )value; }
    }
{{closingParents}}}
#nullable restore
#pragma warning restore
""";

    /// <summary>Generates source for Avalonia views.</summary>
    /// <param name="info">The IViewFor generation model.</param>
    /// <param name="parents">The enclosing type declarations.</param>
    /// <param name="closingParents">The enclosing type closures.</param>
    /// <param name="attributes">The forwarded attributes.</param>
    /// <returns>The generated source.</returns>
    private static string GenerateAvaloniaViewSource(IViewForInfo info, string parents, string closingParents, string attributes) =>
        $$"""
// <auto-generated/>
using System;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
#nullable restore
#pragma warning disable

namespace {{info.TargetInfo.TargetNamespace}}
{
{{parents}}    /// <summary>
    /// Partial class for the {{info.TargetInfo.TargetName}} which contains ReactiveUI IViewFor initialization.
    /// </summary>
    {{attributes}}
    {{info.TargetInfo.TargetVisibility}} partial {{info.TargetInfo.TargetType}} {{info.TargetInfo.TargetName}} : IViewFor<{{info.ViewModelTypeName}}>
    {
        /// <summary>
        /// The view model dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002", Justification = "Generic avalonia property is expected here.")]
        public static readonly StyledProperty<{{info.ViewModelTypeName}}?> ViewModelProperty = AvaloniaProperty.Register<{{info.TargetInfo.TargetName}}, {{info.ViewModelTypeName}}>(nameof(ViewModel));

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public {{info.ViewModelTypeName}}? BindingRoot => ViewModel;

        /// <inheritdoc/>
        public {{info.ViewModelTypeName}}? ViewModel { get => ({{info.ViewModelTypeName}}?)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        /// <inheritdoc/>
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = ({{info.ViewModelTypeName}}?)value; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == DataContextProperty)
            {
                if (ReferenceEquals(change.OldValue, ViewModel) && change.NewValue is null or {{info.ViewModelTypeName}})
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
{{closingParents}}}
#nullable restore
#pragma warning restore
""";

    /// <summary>Generates source for MAUI views.</summary>
    /// <param name="info">The IViewFor generation model.</param>
    /// <param name="parents">The enclosing type declarations.</param>
    /// <param name="closingParents">The enclosing type closures.</param>
    /// <param name="attributes">The forwarded attributes.</param>
    /// <returns>The generated source.</returns>
    private static string GenerateMauiViewSource(IViewForInfo info, string parents, string closingParents, string attributes)
    {
        var viewModelPropertyDeclaration = GetMauiViewModelPropertyDeclaration(info);
        return $$"""
// <auto-generated/>
using System;
using ReactiveUI;
using Microsoft.Maui.Controls;
#nullable restore
#pragma warning disable

namespace {{info.TargetInfo.TargetNamespace}}
{
{{parents}}    {{attributes}}
    {{info.TargetInfo.TargetVisibility}} partial {{info.TargetInfo.TargetType}} {{info.TargetInfo.TargetName}} : IViewFor<{{info.ViewModelTypeName}}>
    {
{{viewModelPropertyDeclaration}}

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public {{info.ViewModelTypeName}}? BindingRoot => ViewModel;

        /// <inheritdoc/>
        public {{info.ViewModelTypeName}}? ViewModel { get => ({{info.ViewModelTypeName}}?)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        /// <inheritdoc/>
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = ({{info.ViewModelTypeName}}?)value; }

        /// <inheritdoc/>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            ViewModel = BindingContext as {{info.ViewModelTypeName}};
        }

        private static void OnViewModelChanged(BindableObject bindableObject, object oldValue, object newValue) => bindableObject.BindingContext = newValue;
    }
{{closingParents}}}
#nullable restore
#pragma warning restore
""";
    }

    /// <summary>Creates the MAUI bindable-property declaration while preserving its generated one-line format.</summary>
    /// <param name="info">The IViewFor generation model.</param>
    /// <returns>The generated bindable-property declaration.</returns>
    private static string GetMauiViewModelPropertyDeclaration(IViewForInfo info)
    {
        var builder = new StringBuilder("        public static readonly BindableProperty ViewModelProperty = ");
        _ = builder.Append("BindableProperty.Create(nameof(ViewModel), typeof(");
        _ = builder.Append(info.ViewModelTypeName).Append("), typeof(IViewFor<");
        _ = builder.Append(info.ViewModelTypeName).Append(">), default(");
        _ = builder.Append(info.ViewModelTypeName);
        _ = builder.Append("), BindingMode.OneWay, propertyChanged: OnViewModelChanged);");
        return builder.ToString();
    }

    /// <summary>Generates the Splat registration extension source for discovered views.</summary>
    /// <param name="viewForInfo">The discovered view-generation models.</param>
    /// <returns>The generated extension source.</returns>
    private static string GenerateRegistrationExtensions(in ImmutableArray<IViewForInfo> viewForInfo)
    {
        var sb = new StringBuilder();
        _ = sb.AppendLine("if (resolver is null) throw new global::System.ArgumentNullException(nameof(resolver));");
        AppendViewRegistrations(sb, viewForInfo);
        AppendViewModelRegistrations(sb, viewForInfo);

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

    /// <summary>Appends unique view registrations to a generated method body.</summary>
    /// <param name="builder">The generated method-body builder.</param>
    /// <param name="viewForInfo">The discovered view-generation models.</param>
    private static void AppendViewRegistrations(StringBuilder builder, ImmutableArray<IViewForInfo> viewForInfo)
    {
        var registrations = new HashSet<(string ViewType, string ViewModelType, string RegistrationType)>();
        foreach (var item in viewForInfo)
        {
            var registrationType = item.SplatRegistrationType;
            var viewModelType = GetGlobalTypeName(item.ViewModelTypeName);
            if (string.IsNullOrWhiteSpace(registrationType) || viewModelType is null)
            {
                continue;
            }

            var viewType = item.TargetInfo.TargetNamespaceWithNamespace;
            if (registrations.Add((viewType, viewModelType, registrationType)))
            {
                AppendViewRegistration(builder, registrationType, viewType, viewModelType);
            }
        }
    }

    /// <summary>Appends unique view model registrations to a generated method body.</summary>
    /// <param name="builder">The generated method-body builder.</param>
    /// <param name="viewForInfo">The discovered view-generation models.</param>
    private static void AppendViewModelRegistrations(StringBuilder builder, ImmutableArray<IViewForInfo> viewForInfo)
    {
        var registrations = new HashSet<(string ViewModelType, string RegistrationType)>();
        foreach (var item in viewForInfo)
        {
            var registrationType = item.SplatViewModelRegistrationType;
            var viewModelType = GetGlobalTypeName(item.ViewModelTypeName);
            if (string.IsNullOrWhiteSpace(registrationType) || viewModelType is null)
            {
                continue;
            }

            if (registrations.Add((viewModelType, registrationType)))
            {
                AppendViewModelRegistration(builder, registrationType, viewModelType);
            }
        }
    }

    /// <summary>Normalizes a type name for use in generated source.</summary>
    /// <param name="typeName">The type name supplied by the attribute.</param>
    /// <returns>A global-qualified type name, or <see langword="null"/> when no type was supplied.</returns>
    private static string? GetGlobalTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        return typeName.StartsWith("global::", System.StringComparison.Ordinal)
            ? typeName
            : $"global::{typeName}";
    }

    /// <summary>Appends one view registration statement.</summary>
    /// <param name="builder">The generated method-body builder.</param>
    /// <param name="registrationType">The Splat registration method.</param>
    /// <param name="viewType">The generated view type.</param>
    /// <param name="viewModelType">The generated view model type.</param>
    private static void AppendViewRegistration(StringBuilder builder, string registrationType, string viewType, string viewModelType)
    {
        var serviceType = $"global::ReactiveUI.IViewFor<{viewModelType}>";
        var registration = registrationType switch
        {
            "RegisterLazySingleton" => $"resolver.{registrationType}<{serviceType}>(() => new {viewType}());",
            "Register" => $"resolver.{registrationType}<{serviceType}, {viewType}>();",
            "RegisterConstant" => $"resolver.{registrationType}<{serviceType}>(new {viewType}());",
            _ => null,
        };
        if (registration is null)
        {
            return;
        }

        _ = builder.Append("            ").AppendLine(registration);
    }

    /// <summary>Appends one view model registration statement.</summary>
    /// <param name="builder">The generated method-body builder.</param>
    /// <param name="registrationType">The Splat registration method.</param>
    /// <param name="viewModelType">The generated view model type.</param>
    private static void AppendViewModelRegistration(StringBuilder builder, string registrationType, string viewModelType)
    {
        var registration = registrationType switch
        {
            "RegisterLazySingleton" => $"resolver.{registrationType}<{viewModelType}>(() => new {viewModelType}());",
            "Register" => $"resolver.{registrationType}<{viewModelType}, {viewModelType}>();",
            "RegisterConstant" => $"resolver.{registrationType}<{viewModelType}>(new {viewModelType}());",
            _ => null,
        };
        if (registration is null)
        {
            return;
        }

        _ = builder.Append("            ").AppendLine(registration);
    }
}
