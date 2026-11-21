// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Registers a view for <typeparamref name="TViewModel"/> with <c>GeneratedViewRegistrations.RegisterGeneratedViews</c> without runtime assembly scanning.</summary>
/// <typeparam name="TViewModel">The view model type associated with the view.</typeparam>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
[DebuggerDisplay("RegisterViewAttribute<{ViewModelType.Name}>: Contract = {Contract}, IsSingleton = {IsSingleton}")]
public sealed class RegisterViewAttribute<TViewModel> : Attribute
    where TViewModel : class
{
    /// <summary>Gets the view model type associated with the view.</summary>
    public Type ViewModelType => typeof(TViewModel);

    /// <summary>Gets or sets the optional view contract.</summary>
    public string? Contract { get; set; }

    /// <summary>Gets or sets a value indicating whether to register the view as a singleton.</summary>
    public bool IsSingleton { get; set; }
}
