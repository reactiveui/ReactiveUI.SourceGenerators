// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates an <c>IReactiveObject</c> implementation for a class that cannot inherit from <c>ReactiveObject</c>.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IReactiveObjectAttribute : Attribute;
