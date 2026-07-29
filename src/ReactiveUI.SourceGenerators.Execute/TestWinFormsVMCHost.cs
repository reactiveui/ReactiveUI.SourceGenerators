// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.WinForms;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the view-model Windows Forms host generation sample.</summary>
/// <seealso cref="System.Windows.Forms.UserControl" />
/// <seealso cref="ReactiveUI.IReactiveObject" />
/// <seealso cref="ReactiveUI.IViewFor" />
[ViewModelControlHost(nameof(UserControl))]
public partial class TestWinFormsVMCHost
{
    /// <summary>Gets a value indicating whether the view-model host sample is available.</summary>
    internal static bool IsSampleAvailable => true;
}
