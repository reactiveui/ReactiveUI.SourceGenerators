// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.WinForms;

namespace TestWinFormsApplication;

/// <summary>Verifies the routed host generator against the ReactiveUI base package.</summary>
[RoutedControlHost(nameof(UserControl))]
public partial class BasePackageRoutedHost
{
    /// <summary>Gets a value indicating whether this host uses the base ReactiveUI package.</summary>
    internal static bool UsesBaseReactiveUiPackage => true;
}
