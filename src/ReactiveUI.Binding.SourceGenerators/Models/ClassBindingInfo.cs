// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Shared type-level value-equatable POCO for the incremental generator pipeline.
/// Contains flags for notification mechanism detection across all supported platforms.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="FullyQualifiedName">The fully qualified name of the class (global:: prefixed).</param>
/// <param name="MetadataName">The metadata name of the class (without namespace or global:: prefix).</param>
/// <param name="ImplementsIReactiveObject">Whether the class implements <c>IReactiveObject</c>.</param>
/// <param name="ImplementsINPC">Whether the class implements <c>INotifyPropertyChanged</c>.</param>
/// <param name="ImplementsINPChanging">Whether the class implements <c>INotifyPropertyChanging</c>.</param>
/// <param name="InheritsWpfDependencyObject">Whether the class inherits from WPF <c>DependencyObject</c>.</param>
/// <param name="InheritsWinUIDependencyObject">Whether the class inherits from WinUI <c>DependencyObject</c>.</param>
/// <param name="InheritsNSObject">Whether the class inherits from Apple <c>NSObject</c> (KVO support).</param>
/// <param name="InheritsWinFormsComponent">Whether the class inherits from WinForms <c>Component</c>.</param>
/// <param name="InheritsAndroidView">Whether the class inherits from Android <c>View</c>.</param>
/// <param name="Properties">The observable properties declared on the class.</param>
internal sealed record ClassBindingInfo(
    string FullyQualifiedName,
    string MetadataName,
    bool ImplementsIReactiveObject,
    bool ImplementsINPC,
    bool ImplementsINPChanging,
    bool InheritsWpfDependencyObject,
    bool InheritsWinUIDependencyObject,
    bool InheritsNSObject,
    bool InheritsWinFormsComponent,
    bool InheritsAndroidView,
    EquatableArray<ObservablePropertyInfo> Properties);
