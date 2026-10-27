// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace ReactiveUI.Binding.Documentation.SourceGenerators;

/// <summary>
/// A class that cannot derive from <c>ReactiveObject</c>, so <c>[IReactiveObject]</c> makes ReactiveUI.SourceGenerators
/// implement the notifications instead.
/// </summary>
[IReactiveObject]
[System.Diagnostics.DebuggerDisplay("ProfileCard: Title = {Title}")]
public partial class ProfileCard
{
    /// <summary>The card's title, behind the generated <c>Title</c> property.</summary>
    [Reactive]
    private string _title = "Engineer";
}
