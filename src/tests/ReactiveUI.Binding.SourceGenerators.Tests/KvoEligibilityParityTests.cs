// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Checks native assembly membership and exported selectors for KVO eligibility.</summary>
public class KvoEligibilityParityTests
{
    /// <summary>Native declarations use Cocoa naming while consumer CLR properties fall through.</summary>
    /// <param name="name">The selected property.</param>
    /// <param name="expected">The native key, or null for an ordinary consumer property.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Title", "title")]
    [Arguments("Enabled", "isEnabled")]
    [Arguments("IsHidden", "isHidden")]
    [Arguments("Count", "nativeCount")]
    [Arguments("Managed", null)]
    public async Task InspectProperty_RespectsNativeDeclaration(string name, string? expected)
    {
        var framework = TestHelper.CompileToReference(
            """
            namespace Foundation
            {
                public class NSObject {}
                public class ExportAttribute : System.Attribute { public ExportAttribute(string name) {} }
                public class Native : NSObject
                {
                    public string Title { get; set; }
                    public bool Enabled { get; set; }
                    public bool IsHidden { get; set; }
                    public int Count { [Export("nativeCount")] get; set; }
                }
            }
            """,
            "AppleFramework",
            LanguageVersion.CSharp10);
        var compilation = TestHelper.CreateCompilation(
            "public class Consumer : Foundation.Native { public int Managed { get; set; } }",
            LanguageVersion.CSharp10,
            false,
            "ConsumerAssembly",
            [framework]);
        var owner = compilation.GetTypeByMetadataName("Consumer")!;
        var property = (IPropertySymbol)PlatformSymbols.FindMember(owner, name)!;
        var selected = new KVOObservationPlugin().InspectProperty(owner, property);
        await Assert.That(selected?.KvoKeyPath).IsEqualTo(expected);
    }

    /// <summary>
    /// A consumer property is only observed through key-value observing when it exports a getter selector, on the
    /// getter or on the property itself: a setter selector, an empty one and an export with no selector fall through.
    /// </summary>
    /// <param name="name">The selected property.</param>
    /// <param name="expected">The native key, or null when the property falls through.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Exported", "exported")]
    [Arguments("SetterSelector", null)]
    [Arguments("EmptySelector", null)]
    [Arguments("NoSelector", null)]
    [Arguments("NullSelector", null)]
    [Arguments("WriteOnly", "writeOnly")]
    public async Task InspectProperty_OnlyAcceptsAnExportedGetterSelector(string name, string? expected)
    {
        var framework = TestHelper.CompileToReference(
            """
            namespace Foundation
            {
                public class NSObject {}
                public class ExportAttribute : System.Attribute
                {
                    public ExportAttribute() {}
                    public ExportAttribute(string name) {}
                }
            }
            """,
            "AppleFramework",
            LanguageVersion.CSharp10);
        var compilation = TestHelper.CreateCompilation(
            """
            public class Consumer : Foundation.NSObject
            {
                [Foundation.Export("exported")]
                public string Exported { get; set; }

                [Foundation.Export("setSetterSelector:")]
                public string SetterSelector { get; set; }

                [Foundation.Export("")]
                public string EmptySelector { get; set; }

                [Foundation.Export]
                public string NoSelector { get; set; }

                [Foundation.Export(null)]
                public string NullSelector { get; set; }

                [Foundation.Export("writeOnly")]
                public string WriteOnly { set { } }
            }
            """,
            LanguageVersion.CSharp10,
            false,
            "ConsumerAssembly",
            [framework]);
        var owner = compilation.GetTypeByMetadataName("Consumer")!;
        var property = (IPropertySymbol)PlatformSymbols.FindMember(owner, name)!;

        var selected = new KVOObservationPlugin().InspectProperty(owner, property);

        await Assert.That(selected?.KvoKeyPath).IsEqualTo(expected);
    }

    /// <summary>A type outside the Foundation hierarchy declares nothing native.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IsNativeDeclaration_OutsideNSObject_IsFalse()
    {
        var compilation = TestHelper.CreateCompilation(
            "public class Plain { public string Name { get; set; } }",
            LanguageVersion.CSharp10);
        var owner = compilation.GetTypeByMetadataName("Plain")!;
        var property = (IPropertySymbol)PlatformSymbols.FindMember(owner, "Name")!;

        await Assert.That(KVOObservationPlugin.IsNativeDeclaration(owner, property)).IsFalse();
    }
}
