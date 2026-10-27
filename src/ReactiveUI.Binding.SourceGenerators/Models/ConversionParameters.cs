// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The caller-supplied conversion inputs used by a generated stage.</summary>
/// <param name="Hint">The expression carrying the conversion hint.</param>
/// <param name="Override">The expression carrying an explicit converter.</param>
internal readonly record struct ConversionParameters(string Hint, string Override);
