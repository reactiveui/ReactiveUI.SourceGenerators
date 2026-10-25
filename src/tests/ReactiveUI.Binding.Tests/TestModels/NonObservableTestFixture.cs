// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A POCO with no change notification support, used to test fallback behavior.</summary>
public class NonObservableTestFixture
{
    /// <summary>Gets or sets a property that cannot be observed for changes.</summary>
    public string NotListeningProperty { get; set; } = string.Empty;
}
