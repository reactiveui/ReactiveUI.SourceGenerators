// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Makes a partial UI control implement <c>IViewFor&lt;TViewModel&gt;</c>, with a bindable <c>ViewModel</c> property.</summary>
/// <typeparam name="T">The view model type the control shows.</typeparam>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("IViewForAttribute<{ViewModelType.Name}>")]
public sealed class IViewForAttribute<T> : Attribute
{
    /// <summary>Gets the view model type the control shows.</summary>
    public Type ViewModelType => typeof(T);
}
