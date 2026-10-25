// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Identifies one view registration: the view model it serves, under which contract.</summary>
/// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
/// <param name="Contract">The contract the view is registered under, or null for the default.</param>
[DebuggerDisplay("ViewRegistrationKey: {ViewModelTypeFullName,nq} ({Contract,nq})")]
internal readonly record struct ViewRegistrationKey(string ViewModelTypeFullName, string? Contract);
