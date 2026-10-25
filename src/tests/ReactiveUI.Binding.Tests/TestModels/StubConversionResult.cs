// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>The answer a stub converter is configured to give.</summary>
/// <param name="Success">Whether the conversion reports success.</param>
/// <param name="Result">The value the conversion hands back.</param>
[DebuggerDisplay("StubConversionResult: success={Success}, result={Result}")]
public readonly record struct StubConversionResult(bool Success, object? Result);
