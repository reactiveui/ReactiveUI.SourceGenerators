// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Unit tests for the <see cref="CommandBindingPluginRegistry"/> and command binding plugins.</summary>
public class CommandBindingPluginTests
{
    /// <summary>A control that takes a command and a parameter is bound by assigning them, over any event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_CommandAndParameterProperties_ReturnsCommandPropertyPlugin()
    {
        const int Expected = 5;
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasCommandProperty: true,
            hasCommandParameterProperty: true,
            hasEnabledProperty: true);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNotNull();
        await Assert.That(plugin).IsTypeOf<CommandPropertyBindingPlugin>();
        await Assert.That(plugin!.Affinity).IsEqualTo(Expected);
    }

    /// <summary>Verifies that GetBestPlugin returns EventEnabledBindingPlugin when event and Enabled exist but no Command.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_EventAndEnabled_ReturnsEventEnabledPlugin()
    {
        const int Expected = 4;
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: "Click",
            hasCommandProperty: false,
            hasEnabledProperty: true);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNotNull();
        await Assert.That(plugin).IsTypeOf<EventEnabledBindingPlugin>();
        await Assert.That(plugin!.Affinity).IsEqualTo(Expected);
    }

    /// <summary>Verifies that GetBestPlugin returns DefaultEventBindingPlugin when event exists but no Command or Enabled.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_EventOnly_ReturnsDefaultEventPlugin()
    {
        const int Expected = 3;
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNotNull();
        await Assert.That(plugin).IsTypeOf<DefaultEventBindingPlugin>();
        await Assert.That(plugin!.Affinity).IsEqualTo(Expected);
    }

    /// <summary>Verifies that GetBestPlugin returns null when no plugin can handle the invocation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_NoEventNoCommand_ReturnsNull()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: null);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNull();
    }

    /// <summary>Verifies CommandPropertyBindingPlugin.RequiresCustomBinderFallback is false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_RequiresCustomBinderFallback_IsFalse()
    {
        var plugin = new CommandPropertyBindingPlugin();
        await Assert.That(plugin.RequiresCustomBinderFallback).IsFalse();
    }

    /// <summary>The native platform plugins leave a registered custom binder able to take over the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NativePlatformPlugins_RequireCustomBinderFallback()
    {
        await Assert.That(new AndroidCommandBindingPlugin().RequiresCustomBinderFallback).IsTrue();
        await Assert.That(new UIKitCommandBindingPlugin().RequiresCustomBinderFallback).IsTrue();
        await Assert.That(new UIKitControlCommandBindingPlugin().RequiresCustomBinderFallback).IsTrue();
    }

    /// <summary>Verifies EventEnabledBindingPlugin.RequiresCustomBinderFallback is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_RequiresCustomBinderFallback_IsTrue()
    {
        var plugin = new EventEnabledBindingPlugin();
        await Assert.That(plugin.RequiresCustomBinderFallback).IsTrue();
    }

    /// <summary>Verifies DefaultEventBindingPlugin.RequiresCustomBinderFallback is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_RequiresCustomBinderFallback_IsTrue()
    {
        var plugin = new DefaultEventBindingPlugin();
        await Assert.That(plugin.RequiresCustomBinderFallback).IsTrue();
    }
}
