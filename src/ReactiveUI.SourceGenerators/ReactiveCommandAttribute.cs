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

    /// <summary>Gets or sets a value indicating whether the command runs its method off the calling thread.</summary>
    /// <value>
    /// <see langword="true"/> to create a synchronous method's command with <c>ReactiveCommand.CreateRunInBackground</c>,
    /// and to start a task-returning method with <c>Task.Run</c>. Observable-returning methods are unaffected.
    /// </value>
    public bool RunInBackground { get; set; }

    /// <summary>Gets or sets the scheduler a synchronous method runs on in the background.</summary>
    /// <value>
    /// A scheduler member of the containing type, or a built-in ReactiveUI scheduler. Setting it implies
    /// <see cref="RunInBackground"/>; when it is not set, ReactiveUI's background scheduler is used. A task-returning
    /// method always starts on the thread pool.
    /// </value>
    public string? BackgroundScheduler { get; set; }

    /// <summary>Gets or sets the accessibility of the generated command property.</summary>
    public PropertyAccessModifier AccessModifier { get; set; }
}
