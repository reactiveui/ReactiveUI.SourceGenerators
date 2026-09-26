// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The compilation symbol that keeps the generator attributes in a consumer's metadata.</summary>
/// <remarks>
/// Every attribute in this assembly is <see cref="System.Diagnostics.ConditionalAttribute"/> on this symbol. The
/// generators read the attributes from source, so a consumer that does not define it keeps no reference to this
/// assembly, and nothing about the attributes reaches its dependents.
/// </remarks>
internal static class KeepAttributes
{
    /// <summary>The symbol a consumer defines to keep the attributes in its metadata.</summary>
    internal const string Symbol = "REACTIVEUI_SOURCEGENERATORS_KEEP_ATTRIBUTES";
}
