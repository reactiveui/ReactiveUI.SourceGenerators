// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Unit tests for all observation plugins, covering emit methods not exercised by snapshot tests.</summary>
public class ObservationPluginTests
{
    /// <summary>The <c>EventObservable</c> name these tests generate against.</summary>
    private const string EventObservableName = "EventObservable";

    /// <summary>The runtime observable every native after-change mechanism is wrapped in.</summary>
    private const string CallbackObservableName = "global::ReactiveUI.Binding.Observables.CallbackPropertyObservable<";

    /// <summary>The <c>false)</c> fragment these tests expect in the generated source.</summary>
    private const string FalseFragment = "false)";

    /// <summary>The fully qualified name of the <c>Address</c> type used by these tests.</summary>
    private const string AddressTypeName = "global::TestApp.Address";

    /// <summary>The fully qualified name of the <c>Inner</c> type used by these tests.</summary>
    private const string InnerTypeName = "global::TestApp.Inner";

    /// <summary>The fully qualified name of the <c>MyAndroidView</c> type used by these tests.</summary>
    private const string MyAndroidViewTypeName = "global::TestApp.MyAndroidView";

    /// <summary>The fully qualified name of the <c>MyControl</c> type used by these tests.</summary>
    private const string MyControlTypeName = "global::TestApp.MyControl";

    /// <summary>The fully qualified name of the <c>MyTextBox</c> type used by these tests.</summary>
    private const string MyTextBoxTypeName = "global::TestApp.MyTextBox";

    /// <summary>The fully qualified name of the <c>MyView</c> type used by these tests.</summary>
    private const string MyViewTypeName = "global::TestApp.MyView";

    /// <summary>The runtime key-value observing observable the generated code is expected to emit.</summary>
    private const string KVOObservableLocal = "global::ReactiveUI.Binding.Observables.KvoPropertyObservable<";

    /// <summary>The <c>__obs0</c> local the generated code is expected to emit.</summary>
    private const string Obs0Local = "__obs0";

    /// <summary>The <c>__obs1</c> local the generated code is expected to emit.</summary>
    private const string Obs1Local = "__obs1";

    /// <summary>The <c>ImmediateReturnSignal</c> name these tests generate against.</summary>
    private const string ImmediateReturnSignalName = "ImmediateReturnSignal";

    /// <summary>The <c>ImmutableEmptySignal</c> name a suppressing inner segment emits for a missing parent.</summary>
    private const string ImmutableEmptySignalName = "ImmutableEmptySignal";

    /// <summary>The observation a property with no mechanism for the asked timing gets, without completing.</summary>
    private const string UnchangingPropertyObservableName = "UnchangingPropertyObservable";

    /// <summary>A property an Android widget raises an event for.</summary>
    private const string TextPropertyName = "Text";

    /// <summary>The event an Android widget raises when its text changes.</summary>
    private const string TextChangedEventName = "TextChanged";

    /// <summary>A property no Android widget reports a change for.</summary>
    private const string UnreportedPropertyName = "ContentDescription";

    /// <summary>The <c>source</c> name these tests generate against.</summary>
    private const string SourceName = "source";

    /// <summary>The <c>sourceObs</c> name these tests generate against.</summary>
    private const string SourceObsName = "sourceObs";

    /// <summary>The <c>string</c> name these tests generate against.</summary>
    private const string StringName = "string";

    /// <summary>The <c>Switch</c> name these tests generate against.</summary>
    private const string SwitchName = "Switch";

    /// <summary>The <c>TextChanged</c> name these tests generate against.</summary>
    private const string TextChangedName = "TextChanged";

    /// <summary>The <c>true)</c> fragment these tests expect in the generated source.</summary>
    private const string TrueFragment = "true)";

    /// <summary>The <c>var __obs0</c> local the generated code is expected to emit.</summary>
    private const string Obs0Declaration = "var __obs0";

    /// <summary>The <c>var sourceObs</c> local the generated code is expected to emit.</summary>
    private const string SourceObsDeclaration = "var sourceObs";

    // ========== WpfObservationPlugin ==========
    /// <summary>Verifies WPF plugin shallow observation emits EventObservable with DependencyPropertyDescriptor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains("DependencyPropertyDescriptor");
        await Assert.That(result).Contains("TextProperty");
    }

    /// <summary>
    /// A dependency property has one change stream and hands it over whatever the caller asked for, so a
    /// before-change observation keeps tracking rather than reading the property once.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_BeforeChange_KeepsReportingFromTheLiveStream()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, true, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).DoesNotContain(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WPF plugin shallow observation variable emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservationVariable_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(Obs0Declaration);
        await Assert.That(result).Contains(EventObservableName);
    }

    /// <summary>The same holds when the observation is assigned to a local.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservationVariable_BeforeChange_KeepsReportingFromTheLiveStream()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).DoesNotContain(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WPF plugin deep chain root segment after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainRootSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>The chain's first link keeps reporting too, so the links below it keep switching.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainRootSegment_BeforeChange_KeepsReportingFromTheLiveStream()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).DoesNotContain(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WPF plugin deep chain inner segment after-change emits EventObservable with Switch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>An inner link keeps reporting as well, so a change below the root is still seen.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_BeforeChange_KeepsReportingFromTheLiveStream()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, true, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>A WPF inner segment that is not the leaf pushes the leaf's default value down the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitDeepChainInnerSegment_EmittingDefaults_PushesDefaultForAMissingParent()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.EmitDefault);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).DoesNotContain(ImmutableEmptySignalName);
    }

    /// <summary>Verifies WPF plugin inline observation variable emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitInlineObservationVariable_EmitsEventObservable()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyControlTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(EventObservableName);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies WPF plugin properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_Properties_AreCorrect()
    {
        const int ExpectedPluginAffinity = 4;
        var plugin = new WpfObservationPlugin();

        await Assert.That(plugin.Affinity).IsEqualTo(ExpectedPluginAffinity);
        await Assert.That(plugin.ObservationKind).IsEqualTo("WpfDP");
        await Assert.That(plugin.SupportsBeforeChanged).IsFalse();
    }

    /// <summary>Verifies WPF plugin matches WPF DependencyObject types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_IsAMatch_WpfDependencyObject()
    {
        var plugin = new WpfObservationPlugin();
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true);

        await Assert.That(plugin.IsAMatch(classInfo)).IsTrue();
    }

    // ========== WinFormsObservation ==========
    /// <summary>Verifies WinForms plugin shallow observation variable after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservationVariable_AfterChange_EmitsEventObservable()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyTextBoxTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(TextChangedName);
    }

    /// <summary>Verifies WinForms plugin shallow observation variable before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservationVariable_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyTextBoxTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WinForms plugin shallow observation before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservation_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyTextBoxTypeName, true, true);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WinForms plugin deep chain root segment after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainRootSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyTextBoxTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(TextChangedName);
    }

    /// <summary>Verifies WinForms plugin deep chain root segment before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyTextBoxTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WinForms plugin deep chain inner segment after-change emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsEventObservable()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies WinForms plugin deep chain inner segment before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsUnchangingValue()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, true, NullParentObservationBehavior.SuppressEmission);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>A WinForms inner segment that is not the leaf pushes the leaf's default value down the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitDeepChainInnerSegment_EmittingDefaults_PushesDefaultForAMissingParent()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.EmitDefault);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).DoesNotContain(ImmutableEmptySignalName);
    }

    /// <summary>Verifies WinForms plugin inline observation variable emits EventObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitInlineObservationVariable_EmitsEventObservable()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyTextBoxTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(TextChangedName);
    }

    // ========== WinUIObservation ==========
    /// <summary>Verifies WinUI plugin shallow observation before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservation_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, true, true);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WinUI plugin shallow observation variable after-change emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservationVariable_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>Verifies WinUI plugin shallow observation variable before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservationVariable_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WinUI plugin deep chain root segment after-change emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainRootSegment_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, false, Obs0Local);

        await Assert.That(sb.ToString()).Contains(CallbackObservableName);
    }

    /// <summary>Verifies WinUI plugin deep chain root segment before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyControlTypeName, true, Obs0Local);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>Verifies WinUI plugin deep chain inner segment after-change emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsWinUIDPObservable()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies WinUI plugin deep chain inner segment before-change emits ImmediateReturnSignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsUnchangingValue()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, true, NullParentObservationBehavior.SuppressEmission);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>A WinUI inner segment that is not the leaf pushes the leaf's default value down the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitDeepChainInnerSegment_EmittingDefaults_PushesDefaultForAMissingParent()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName, InnerTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.EmitDefault);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).DoesNotContain(ImmutableEmptySignalName);
    }

    /// <summary>Verifies WinUI plugin inline observation variable emits WinUIDPObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitInlineObservationVariable_EmitsWinUIDPObservable()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyControlTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    // ========== KVOObservationPlugin ==========
    /// <summary>Verifies KVO plugin shallow observation variable after-change emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservationVariable_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("\"text\"");
    }

    /// <summary>Verifies KVO plugin shallow observation variable before-change emits KVOObservable with beforeChange true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservationVariable_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyViewTypeName, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(TrueFragment);
    }

    /// <summary>Verifies KVO plugin deep chain root segment emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainRootSegment_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("\"text\"");
    }

    /// <summary>Verifies KVO plugin deep chain root segment before-change emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainRootSegment_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyViewTypeName, true, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(TrueFragment);
    }

    /// <summary>Verifies KVO plugin deep chain inner segment emits KVOObservable with Switch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_AfterChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>Verifies KVO plugin deep chain inner segment before-change emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, true, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(TrueFragment);
    }

    /// <summary>A KVO inner segment that is not the leaf pushes the leaf's default value down the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitDeepChainInnerSegment_EmittingDefaults_PushesDefaultForAMissingParent()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.EmitDefault);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).DoesNotContain(ImmutableEmptySignalName);
    }

    /// <summary>Verifies KVO plugin inline observation variable emits KVOObservable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitInlineObservationVariable_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyViewTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies KVO key path for boolean property uses "is" prefix.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_BooleanProperty_UsesIsPrefix()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Enabled", "bool");

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains("\"isEnabled\"");
    }

    /// <summary>Verifies KVO key path for boolean property already starting with "Is" does not double-prefix.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_BooleanPropertyAlreadyStartingWithIs_DoesNotDoublePrefix()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("IsEnabled", "bool");

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains("\"isEnabled\"");
    }

    /// <summary>Verifies KVO key path for empty property name returns empty string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmptyPropertyName_ReturnsEmpty()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment(string.Empty, StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains("\"\"");
    }

    /// <summary>Verifies KVO plugin shallow observation emits both before/after change variants correctly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservation_BeforeChange_EmitsKVOObservable()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, true, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(KVOObservableLocal);
        await Assert.That(result).Contains("true, true");
    }

    // ========== AndroidObservation ==========
    /// <summary>The variable form subscribes the same widget event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservationVariable_SubscribesTheWidgetEvent()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, TextPropertyName, StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyAndroidViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(TextChangedEventName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>The variable form of a property no widget reports reads once and stays open.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservationVariable_UnreportedProperty_EmitsTheUnchangingValue()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, UnreportedPropertyName, StringName);

        plugin.EmitShallowObservationVariable(sb, "obj", segment, MyAndroidViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(UnchangingPropertyObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>A chain rooted on a reported widget property subscribes that property's event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainRootSegment_SubscribesTheWidgetEvent()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, TextPropertyName, StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyAndroidViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(TextChangedEventName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>A chain rooted on a property no widget reports reads once and stays open.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainRootSegment_UnreportedProperty_EmitsTheUnchangingValue()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, UnreportedPropertyName, StringName);

        plugin.EmitDeepChainRootSegment(sb, "obj", segment, MyAndroidViewTypeName, false, Obs0Local);

        var result = sb.ToString();
        await Assert.That(result).Contains(UnchangingPropertyObservableName);
        await Assert.That(result).Contains(Obs0Declaration);
    }

    /// <summary>Verifies Android plugin deep chain inner segment emits ImmediateReturnSignal with Switch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainInnerSegment_EmitsUnchangingValue()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.SuppressEmission);

        var result = sb.ToString();
        await Assert.That(result).Contains(UnchangingPropertyObservableName);
        await Assert.That(result).Contains(SwitchName);
    }

    /// <summary>An Android inner segment that is not the leaf pushes the leaf's default value down the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitDeepChainInnerSegment_EmittingDefaults_PushesDefaultForAMissingParent()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "City", StringName, AddressTypeName);

        plugin.EmitDeepChainInnerSegment(sb, new(Obs0Local, Obs1Local, "__p1"), segment, false, NullParentObservationBehavior.EmitDefault);

        var result = sb.ToString();
        await Assert.That(result).Contains(ImmediateReturnSignalName);
        await Assert.That(result).DoesNotContain(ImmutableEmptySignalName);
    }

    /// <summary>The inline form subscribes the same widget event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitInlineObservationVariable_SubscribesTheWidgetEvent()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, TextPropertyName, StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyAndroidViewTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(TextChangedEventName);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>The inline form of a property no widget reports reads once and stays open.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitInlineObservationVariable_UnreportedProperty_EmitsTheUnchangingValue()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, UnreportedPropertyName, StringName);

        plugin.EmitInlineObservationVariable(sb, SourceName, segment, MyAndroidViewTypeName, SourceObsName);

        var result = sb.ToString();
        await Assert.That(result).Contains(UnchangingPropertyObservableName);
        await Assert.That(result).Contains(SourceObsDeclaration);
    }

    /// <summary>Verifies Android plugin properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_Properties_AreCorrect()
    {
        const int ExpectedPluginAffinity = 5;
        var plugin = AndroidObservation.Plugin;

        await Assert.That(plugin.Affinity).IsEqualTo(ExpectedPluginAffinity);
        await Assert.That(plugin.ObservationKind).IsEqualTo("Android");
        await Assert.That(plugin.SupportsBeforeChanged).IsFalse();
    }

    // ========== Shallow observation with includeStartWith=false ==========
    /// <summary>Verifies INPC plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPCPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new INPCObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Name", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyViewModel", false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies ReactiveObject plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObjectPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new ReactiveObjectObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Name", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, "global::TestApp.MyViewModel", false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies WPF plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new WpfObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies WinForms plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = WinFormsObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyTextBoxTypeName, false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies WinUI plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = WinUIObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, "Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyControlTypeName, false, false);

        await Assert.That(sb.ToString()).Contains(FalseFragment);
    }

    /// <summary>Verifies KVO plugin shallow observation with includeStartWith=false emits "false".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = new KVOObservationPlugin();
        var sb = new SourceWriter();
        var segment = ModelFactory.CreatePropertyPathSegment("Text", StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyViewTypeName, false, false);

        await Assert.That(sb.ToString()).Contains("false, false)");
    }

    /// <summary>A widget property that raises an event is observed through that event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservation_SubscribesTheWidgetEvent()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, TextPropertyName, StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyAndroidViewTypeName, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).Contains(TextChangedEventName);
    }

    /// <summary>A property no widget reports has nothing to subscribe, so it reads once and stays open.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservation_UnreportedProperty_EmitsTheUnchangingValue()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, UnreportedPropertyName, StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyAndroidViewTypeName, false, true);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    /// <summary>
    /// A caller that only wants subsequent changes gets the observation without its opening value, so a
    /// chain stage does not re-emit what the stage above it has already delivered.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservation_NoStartWith_EmitsFalse()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, TextPropertyName, StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyAndroidViewTypeName, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(CallbackObservableName);
        await Assert.That(result).EndsWith(", false)");
    }

    /// <summary>A widget reports a change once it has happened, so before-change has nothing to subscribe.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_EmitShallowObservation_BeforeChange_EmitsTheUnchangingValue()
    {
        var plugin = AndroidObservation.Plugin;
        var sb = new SourceWriter();
        var segment = NativeObservationTestModels.CreateSegment(plugin, TextPropertyName, StringName);

        plugin.EmitShallowObservation(sb, "obj", segment, MyAndroidViewTypeName, true, true);

        await Assert.That(sb.ToString()).Contains(UnchangingPropertyObservableName);
    }

    // ========== INPCObservationPlugin ==========
    // ========== ReactiveObjectObservationPlugin ==========
    // ========== ObservationPluginRegistry ==========
    /// <summary>Verifies GetPlugin returns the correct plugin by index.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_GetPlugin_ReturnsCorrectPlugin()
    {
        const int ExpectedPlugin0Affinity = 15;
        var plugin0 = ObservationPluginRegistry.GetPlugin(0);

        await Assert.That(plugin0.Affinity).IsEqualTo(ExpectedPlugin0Affinity); // KVO has highest affinity
    }

    /// <summary>Verifies GetPluginByKind returns null for unknown kind.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_GetPluginByKind_UnknownKind_ReturnsNull()
    {
        var plugin = ObservationPluginRegistry.GetPluginByKind("NonExistent");

        await Assert.That(plugin).IsNull();
    }

    /// <summary>Verifies Count returns the correct number of plugins.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Registry_Count_MatchesSupportedMechanisms()
    {
        const int ExpectedPluginCount = 12;
        await Assert.That(ObservationPluginRegistry.Count).IsEqualTo(ExpectedPluginCount);
    }
}
