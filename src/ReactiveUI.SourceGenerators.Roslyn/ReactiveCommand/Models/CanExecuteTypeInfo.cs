// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes the supported kinds of can-execute members.</summary>
internal enum CanExecuteTypeInfo
{
    /// <summary>A property that provides an observable Boolean value.</summary>
    PropertyObservable,

    /// <summary>A method that returns an observable Boolean value.</summary>
    MethodObservable,

    /// <summary>A field that provides an observable Boolean value.</summary>
    FieldObservable,
}
