// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Initializes the source generator verifiers.</summary>
public static class ModuleInitializer
{
    /// <summary>Initializes the source generators.</summary>
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
