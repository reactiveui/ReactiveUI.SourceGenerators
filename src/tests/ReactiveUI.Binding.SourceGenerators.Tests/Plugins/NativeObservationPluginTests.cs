// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Tests for <see cref="NativeObservationPlugin"/>, driven through a probe mechanism.</summary>
public class NativeObservationPluginTests
{
    /// <summary>The probe mechanism's identity.</summary>
    private const string ProbeKind = "Probe";

    /// <summary>The probe mechanism's affinity.</summary>
    private const int ProbeAffinity = 12;

    /// <summary>The text the probe's subscription writes, so its place in the output can be found.</summary>
    private const string ProbeSubscription = "// probe subscription";

    /// <summary>The property the probe observes.</summary>
    private const string PropertyName = "Text";

    /// <summary>The type of the property the probe observes.</summary>
    private const string PropertyType = "string";

    /// <summary>The probe plugin, which offers no candidates of its own and writes a marker for its subscription.</summary>
    private static readonly NativeObservationPlugin Probe = new(
        ProbeKind,
        ProbeAffinity,
        static (_, _) => null,
        static (sb, _, _) => _ = sb.Line(ProbeSubscription));

    /// <summary>The plugin offers exactly the candidate the mechanism's own inspection returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InspectProperty_ReturnsTheMechanismsCandidate()
    {
        var candidate = new PlatformObservationInfo(ProbeKind, ProbeAffinity, default, null, null, null);
        var plugin = new NativeObservationPlugin(ProbeKind, ProbeAffinity, (_, _) => candidate, static (_, _, _) => { });

        await Assert.That(plugin.InspectProperty(null!, null!)).IsSameReferenceAs(candidate);
    }

    /// <summary>The identity and score the plugin competes with are the ones the mechanism supplied.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Identity_IsTheMechanisms()
    {
        await Assert.That(Probe.ObservationKind).IsEqualTo(ProbeKind);
        await Assert.That(Probe.Affinity).IsEqualTo(ProbeAffinity);
        await Assert.That(Probe.SupportsBeforeChanged).IsFalse();
    }

    /// <summary>A property the mechanism verified scores the mechanism's affinity after a change and nothing before one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForProperty_VerifiedProperty_ScoresOnlyAfterChange()
    {
        var classInfo = NativeObservationTestModels.CreateSegment(Probe, PropertyName, PropertyType).DeclaringTypeInfo!;

        await Assert.That(Probe.IsAMatch(classInfo)).IsTrue();
        await Assert.That(Probe.GetAffinityForProperty(classInfo, PropertyName, false)).IsEqualTo(ProbeAffinity);
        await Assert.That(Probe.GetAffinityForProperty(classInfo, PropertyName, true)).IsEqualTo(0);
        await Assert.That(Probe.GetAffinityForProperty(classInfo, "Other", false)).IsEqualTo(0);
    }

    /// <summary>An after-change observation wraps the mechanism's own subscription in the runtime's callback observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitObservation_AfterChange_WrapsTheMechanismsSubscription()
    {
        var segment = NativeObservationTestModels.CreateSegment(Probe, PropertyName, PropertyType);
        var sb = new SourceWriter();

        Probe.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyControl", false, "__obs0");

        var result = sb.ToString();
        await Assert.That(result).Contains("new global::ReactiveUI.Binding.Observables.CallbackPropertyObservable<");
        await Assert.That(result).Contains(ProbeSubscription);
    }

    /// <summary>A before-change observation never reaches the mechanism's subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitObservation_BeforeChange_LeavesTheSubscriptionOut()
    {
        var segment = NativeObservationTestModels.CreateSegment(Probe, PropertyName, PropertyType);
        var sb = new SourceWriter();

        Probe.EmitShallowObservationVariable(sb, "obj", segment, "global::TestApp.MyControl", true, "__obs0");

        await Assert.That(sb.ToString()).DoesNotContain(ProbeSubscription);
    }
}
