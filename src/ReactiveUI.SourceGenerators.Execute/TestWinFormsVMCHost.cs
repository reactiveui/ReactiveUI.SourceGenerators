// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators.WinForms;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestWinFormsVMCHost.
/// </summary>
/// <seealso cref="System.Windows.Forms.UserControl" />
/// <seealso cref="ReactiveUI.IReactiveObject" />
/// <seealso cref="ReactiveUI.IViewFor" />
[ViewModelControlHost(nameof(UserControl))]
public partial class TestWinFormsVMCHost;
