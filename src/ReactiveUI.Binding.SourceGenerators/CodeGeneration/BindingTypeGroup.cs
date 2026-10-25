// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>The call sites that share one generated property-binding overload.</summary>
/// <param name="SourceTypeFullName">The type the value is read from.</param>
/// <param name="TargetTypeFullName">The type the value is written to.</param>
/// <param name="SourcePropertyTypeFullName">The source property's type.</param>
/// <param name="TargetPropertyTypeFullName">The target property's type.</param>
/// <param name="HasConversion">Whether the call sites pass a conversion or selector.</param>
/// <param name="HasScheduler">Whether the call sites pass a scheduler.</param>
/// <param name="Invocations">The call sites in this group.</param>
/// <remarks>
/// <c>BindOneWay</c>, <c>BindTwoWay</c>, <c>OneWayBind</c> and <c>Bind</c> differ in argument order, parameter
/// names and direction, but they all group their call sites by the same signature: the two types, the two
/// property types, and whether a conversion, a converter object and a scheduler are present. One record serves
/// all four.
/// </remarks>
internal sealed record BindingTypeGroup(
    string SourceTypeFullName,
    string TargetTypeFullName,
    string SourcePropertyTypeFullName,
    string TargetPropertyTypeFullName,
    bool HasConversion,
    bool HasScheduler,
    BindingInvocationInfo[] Invocations)
{
    /// <summary>Gets a value indicating whether the call sites pass a converter object in place of a delegate.</summary>
    /// <remarks>
    /// That overload has its own parameter list - the converter or converters, an optional hint and an optional
    /// scheduler - and no expression parameters, so its call sites can only be told apart by file and line.
    /// </remarks>
    internal bool HasConverterOverride { get; init; }
}
