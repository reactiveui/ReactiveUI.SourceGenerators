// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Covers how generated code decides to reference the runtime's helpers, and the paths around that decision.</summary>
public class RuntimeHelperReferenceTests
{
    /// <summary>A call site's expression text.</summary>
    private const string FirstCallSite = "x => x.First";

    /// <summary>Another call site's expression text.</summary>
    private const string SecondCallSite = "x => x.Second";

    /// <summary>The control expression the AppKit emitter writes against.</summary>
    private const string ControlAccess = "__control";

    /// <summary>A compilation that declares the runtime's AppKit command target.</summary>
    private const string AppKitRuntimeSource = """
                                               namespace ReactiveUI.Binding.CommandBinding
                                               {
                                                   public sealed class AppKitCommandTarget { }
                                               }
                                               """;

    /// <summary>An AppKit route whose runtime target the compilation cannot see is dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RuntimeSupported_AppKitWithoutTheRuntimeTarget_DropsTheRoute()
    {
        var compilation = TestHelper.CreateCompilation(string.Empty, LanguageVersion.CSharp10);

        await Assert.That(CommandExtractor.RuntimeSupported(AppKitRoute(), compilation)).IsNull();
    }

    /// <summary>An AppKit route is kept when the compilation references the runtime target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RuntimeSupported_AppKitWithTheRuntimeTarget_KeepsTheRoute()
    {
        var compilation = TestHelper.CreateCompilation(AppKitRuntimeSource, LanguageVersion.CSharp10);
        var route = AppKitRoute();

        await Assert.That(CommandExtractor.RuntimeSupported(route, compilation)).IsSameReferenceAs(route);
    }

    /// <summary>A route that needs no runtime type, and no route at all, pass through unchanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RuntimeSupported_OtherRoutes_PassThrough()
    {
        var compilation = TestHelper.CreateCompilation(string.Empty, LanguageVersion.CSharp10);
        var android = new NativeCommandInfo(NativeCommandKind.AndroidClick, "Click", null, true, false);

        await Assert.That(CommandExtractor.RuntimeSupported(android, compilation)).IsSameReferenceAs(android);
        await Assert.That(CommandExtractor.RuntimeSupported(null, compilation)).IsNull();
    }

    /// <summary>A control with neither an enabled flag nor an action selector is only given the runtime target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppKitEmitter_WithoutEnabledOrAction_OnlyAssignsTheTarget()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo() with
        {
            NativeCommand = new(NativeCommandKind.AppKitTargetAction, null, null, false, false),
        };
        var sb = new SourceWriter().Indent().Indent().Indent();

        AppKitCommandEmitter.EmitBinding(sb, inv, ControlAccess);

        var result = sb.ToString();
        await Assert.That(result).Contains($"new {GeneratedTypeNames.AppKitCommandTarget}(");
        await Assert.That(result).Contains($"{ControlAccess}.Target = __target;");
        await Assert.That(result).DoesNotContain(".Enabled");
        await Assert.That(result).DoesNotContain("Selector");
    }

    /// <summary>With no runtime stub to inspect there are no expression parameters to mirror.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StubHasExpressionParameters_WithoutAStub_IsFalse() =>
        await Assert.That(BindingGenerator.StubHasExpressionParameters(null)).IsFalse();

    /// <summary>Call sites a dispatch can tell apart are all kept, and the original array is handed back.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CollapseIndistinguishableCallSites_AllDistinct_ReturnsTheSameArray()
    {
        string[] callSites = [FirstCallSite, SecondCallSite];

        var kept = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(callSites, static site => new EquatableArray<string>([site]));

        await Assert.That(kept).IsSameReferenceAs(callSites);
    }

    /// <summary>Call sites a dispatch cannot tell apart collapse to the first of them.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CollapseIndistinguishableCallSites_Duplicates_KeepsTheFirst()
    {
        string[] callSites = [FirstCallSite, FirstCallSite, SecondCallSite];

        var kept = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(callSites, static site => new EquatableArray<string>([site]));

        await Assert.That(kept).IsEquivalentTo([FirstCallSite, SecondCallSite]);
    }

    /// <summary>Creates an AppKit target/action route with an enabled flag and an action selector.</summary>
    /// <returns>The route.</returns>
    private static NativeCommandInfo AppKitRoute() => new(NativeCommandKind.AppKitTargetAction, null, null, true, true);
}
