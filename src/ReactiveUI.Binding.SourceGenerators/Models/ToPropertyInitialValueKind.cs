// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>How a <c>ToProperty</c> call site supplies the property's initial value.</summary>
internal enum ToPropertyInitialValueKind
{
    /// <summary>No initial value; the property starts at the default value.</summary>
    None = 0,

    /// <summary>An <c>initialValue</c> argument.</summary>
    Value = 1,

    /// <summary>A <c>getInitialValue</c> factory argument.</summary>
    Factory = 2,
}
