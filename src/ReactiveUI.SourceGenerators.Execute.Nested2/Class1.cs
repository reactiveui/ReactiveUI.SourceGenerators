// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Execute.Nested2;

/// <summary>Provides the second nested reactive-property sample.</summary>
[ExcludeFromCodeCoverage]
public partial class Class1 : ReactiveObject
{
    /// <summary>Stores the second generated property value.</summary>
    [Reactive]
    private string? _property1;
}
