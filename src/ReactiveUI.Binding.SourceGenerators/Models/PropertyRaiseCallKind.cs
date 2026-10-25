// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>How generated code raises one change notification on a type.</summary>
internal enum PropertyRaiseCallKind
{
    /// <summary>A static method taking the object first, such as ReactiveUI's <c>RaisePropertyChanged</c> extension.</summary>
    StaticMethod = 0,

    /// <summary>An instance method on the object.</summary>
    InstanceMethod = 1,

    /// <summary>A field-like event the type declares, invoked from inside the type.</summary>
    EventInvoke = 2,
}
