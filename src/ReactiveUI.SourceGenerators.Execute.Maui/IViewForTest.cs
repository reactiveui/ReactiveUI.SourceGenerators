// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test.Maui;

/// <summary>Provides the MAUI test view used to exercise generated <c>IViewFor</c> members.</summary>
/// <seealso cref="NavigationPage" />
[IViewFor<TestViewModel>]
public partial class IViewForTest : Shell;
