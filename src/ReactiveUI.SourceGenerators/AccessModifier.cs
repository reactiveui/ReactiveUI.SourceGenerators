// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The accessibility of a generated property's set accessor.</summary>
public enum AccessModifier
{
    /// <summary>A <c>public</c> accessor.</summary>
    Public,

    /// <summary>A <c>protected</c> accessor.</summary>
    Protected,

    /// <summary>An <c>internal</c> accessor.</summary>
    Internal,

    /// <summary>A <c>private</c> accessor.</summary>
    Private,

    /// <summary>A <c>protected internal</c> accessor.</summary>
    InternalProtected,

    /// <summary>A <c>private protected</c> accessor.</summary>
    PrivateProtected,

    /// <summary>An <c>init</c> accessor.</summary>
    Init,
}
