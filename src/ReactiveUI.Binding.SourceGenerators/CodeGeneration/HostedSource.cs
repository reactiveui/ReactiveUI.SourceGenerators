// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>The members of an API's file, read back so they can move into the caller's class.</summary>
/// <param name="Members">The member lines, without the indentation of the class they were written in.</param>
/// <param name="Claims">The interceptor each claimed call site's data names.</param>
/// <param name="Trailing">Text the file declares after the generated namespace, which stays at namespace level.</param>
internal readonly record struct HostedSource(List<string> Members, Dictionary<string, string> Claims, string Trailing);
