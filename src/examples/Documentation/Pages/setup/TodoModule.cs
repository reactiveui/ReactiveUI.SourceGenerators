// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Registers the services of the to-do screen.</summary>
public sealed class TodoModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver) =>
        resolver.RegisterLazySingleton<ITodoStore>(static () => InMemoryTodoStore.CreateSeeded());
}
