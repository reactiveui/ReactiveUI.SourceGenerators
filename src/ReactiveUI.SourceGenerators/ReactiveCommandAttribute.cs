// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a <c>ReactiveCommand</c> property that executes the annotated method.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ReactiveCommandAttribute : Attribute
{
    /// <summary>Gets or sets the name of the <c>IObservable&lt;bool&gt;</c> member that decides whether the command can execute.</summary>
    public string? CanExecute { get; set; }

    /// <summary>Gets or sets the scheduler the command delivers its results on.</summary>
    public string? OutputScheduler { get; set; }

    /// <summary>Gets or sets a value indicating whether a synchronous method runs on ReactiveUI's background scheduler.</summary>
    /// <value><see langword="true"/> to create the command with <c>ReactiveCommand.CreateRunInBackground</c>.</value>
    public bool RunInBackground { get; set; }

    /// <summary>Gets or sets the accessibility of the generated command property.</summary>
    public PropertyAccessModifier AccessModifier { get; set; }
}
