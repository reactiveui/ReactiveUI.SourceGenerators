// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>
/// Covers how an Apple control's change notification is found: a public static constant on the control, in an
/// assembly that also declares the notification center.
/// </summary>
public class AppleNotificationTests
{
    /// <summary>A control whose notification constant is usable, beside ones whose constant is not.</summary>
    private const string Controls = """
        namespace Foundation
        {
            public class NSObject {}
            public class NSNotificationCenter {}
        }
        namespace UIKit
        {
            public class UIControl : Foundation.NSObject
            {
                public event System.EventHandler ValueChanged;
            }
            public class UISearchBar : UIControl
            {
                public string Text { get; set; }
            }
            public class Usable
            {
                public static string Changed => "changed";
            }
            public class Instance
            {
                public string Changed => "changed";
            }
            public class Hidden
            {
                private static string Changed => "changed";
            }
        }
        """;

    /// <summary>A public static constant in an assembly with a notification center names the notification.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNotification_PublicStaticConstant_NamesIt()
    {
        var compilation = TestHelper.CreateCompilation(Controls);

        var result = AppleNotificationEmitter.ResolveNotification(compilation.GetTypeByMetadataName("UIKit.Usable")!, "Changed");

        await Assert.That(result).IsEqualTo("global::UIKit.Usable.Changed");
    }

    /// <summary>A constant that is missing, an instance member or not public cannot be named from generated code.</summary>
    /// <param name="owner">The control's metadata name.</param>
    /// <param name="member">The constant looked for.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("UIKit.Usable", "Missing")]
    [Arguments("UIKit.Instance", "Changed")]
    [Arguments("UIKit.Hidden", "Changed")]
    public async Task ResolveNotification_UnusableConstant_ReturnsNull(string owner, string member)
    {
        var compilation = TestHelper.CreateCompilation(Controls);

        var result = AppleNotificationEmitter.ResolveNotification(compilation.GetTypeByMetadataName(owner)!, member);

        await Assert.That(result).IsNull();
    }

    /// <summary>Without a notification center in the constant's assembly there is nothing to observe it through.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveNotification_WithoutANotificationCenter_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation("""
            namespace UIKit
            {
                public class Usable
                {
                    public static string Changed => "changed";
                }
            }
            """);

        var result = AppleNotificationEmitter.ResolveNotification(compilation.GetTypeByMetadataName("UIKit.Usable")!, "Changed");

        await Assert.That(result).IsNull();
    }

    /// <summary>A search bar whose text has no change event offers no native observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Inspect_SearchBarWithoutTextChanged_ReturnsNull()
    {
        var compilation = TestHelper.CreateCompilation(Controls);
        var searchBar = compilation.GetTypeByMetadataName("UIKit.UISearchBar")!;
        var text = (IPropertySymbol)searchBar.GetMembers("Text")[0];

        var result = UIKitObservation.Inspect(searchBar, text);

        await Assert.That(result).IsNull();
    }
}
