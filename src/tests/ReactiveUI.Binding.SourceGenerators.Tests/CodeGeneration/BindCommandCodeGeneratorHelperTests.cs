// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindCommandCodeGenerator"/> helper methods and command binding plugins.</summary>
public class BindCommandCodeGeneratorHelperTests
{
    /// <summary>The <c>Click</c> name these tests generate against.</summary>
    private const string ClickName = "Click";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The <c>IObservable&lt;global::System.String&gt; withParameter</c> fragment these tests expect in the generated source.</summary>
    private const string IObservableGlobalSystemStringWithParameterFragment = "IObservable<global::System.String> withParameter";

    /// <summary>The <c>Param</c> name these tests generate against.</summary>
    private const string ParamName = "Param";

    /// <summary>The <c>TESTSUFFIX</c> name these tests generate against.</summary>
    private const string TESTSUFFIXName = "TESTSUFFIX";

    /// <summary>The <c>view.SaveButton</c> name these tests generate against.</summary>
    private const string ViewSaveButtonName = "view.SaveButton";

    /// <summary>The <c>view.SaveButton.Click += __Handler</c> fragment these tests expect in the generated source.</summary>
    private const string ViewSaveButtonClickHandlerFragment = "view.SaveButton.Click += __Handler";

    /// <summary>The <c>view.SaveButton.Command = cmd</c> fragment these tests expect in the generated source.</summary>
    private const string ViewSaveButtonCommandCmdFragment = "view.SaveButton.Command = cmd";

    /// <summary>The <c>Volatile</c> name these tests generate against.</summary>
    private const string VolatileName = "Volatile";

    /// <summary>The stream a command parameter reaches a registered binder as, whichever form the call site used.</summary>
    private const string MappedParameterStreamFragment = "MapSignal<global::System.String, object>(withParameter";

    /// <summary>The subscription that keeps the control's parameter following the observed property.</summary>
    private const string ParameterStreamSubscriptionFragment =
        "withParameter, __p => view.SaveButton.CommandParameter = __p";

    /// <summary>A control that takes both a command and a parameter is driven by assigning them.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_CanHandle_WithCommandAndParameterProperties_ReturnsTrue()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// A control that takes a command but no parameter cannot carry one the call site supplied, so this
    /// binder steps aside rather than binding the command and dropping the parameter silently.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_CanHandle_WithoutParameterProperty_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(hasCommandProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies CommandPropertyBindingPlugin.CanHandle returns false when HasCommandProperty is false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_CanHandle_WithoutCommandProperty_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        var plugin = new CommandPropertyBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies CommandPropertyBindingPlugin emits Command+CommandParameter+observable parameter code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_ObservableParam_EmitsVolatilePattern()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterIsReferenceType: true,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("Volatile.Write(ref __latestParam, p)");
        await Assert.That(result).Contains(ViewSaveButtonCommandCmdFragment);
        await Assert.That(result).Contains("view.SaveButton.CommandParameter = param");
        await Assert.That(result).Contains("MultipleDisposable");
    }

    /// <summary>
    /// A value-type parameter is recorded by a plain write. <c>Volatile</c> offers no overload for an
    /// arbitrary value type, so emitting one would leave the consumer with code that does not compile.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_ValueTypeObservableParam_RecordsWithAPlainWrite()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.Guid",
            parameterIsReferenceType: false,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Guid __latestParam = default;");
        await Assert.That(result).Contains("withParameter, p => __latestParam = p);");
        await Assert.That(result).Contains("var param = __latestParam;");
        await Assert.That(result).DoesNotContain(VolatileName);
    }

    /// <summary>
    /// A parameter named as a property is followed for as long as the command is bound, so the control's
    /// parameter tracks it rather than freezing on the value it held when the command arrived.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_ExpressionParam_FollowsTheParameterStream()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment(ParamName)]);
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterPropertyPath: paramPath,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(ViewSaveButtonCommandCmdFragment);
        await Assert.That(result).Contains(ParameterStreamSubscriptionFragment);
        await Assert.That(result).DoesNotContain(VolatileName);
    }

    /// <summary>With no parameter supplied, only the command is assigned as values arrive.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_NoParam_AssignsOnlyTheCommand()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(ViewSaveButtonCommandCmdFragment);
        await Assert.That(result).DoesNotContain("CommandParameter = viewModel");
        await Assert.That(result).DoesNotContain(VolatileName);
    }

    /// <summary>
    /// A disposed binding leaves the control as it found it. Without that, a view rebound to a second view
    /// model keeps executing the first one's command.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_RestoresTheControlOnDispose()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        var plugin = new CommandPropertyBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("var __originalCommand = view.SaveButton.Command;");
        await Assert.That(result).Contains("var __originalParameter = view.SaveButton.CommandParameter;");
        await Assert.That(result).Contains("view.SaveButton.CommandParameter = __originalParameter;");
        await Assert.That(result).Contains("view.SaveButton.Command = __originalCommand;");
    }

    /// <summary>Verifies EventEnabledBindingPlugin.CanHandle returns true when event and Enabled property exist.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_CanHandle_WithEventAndEnabled_ReturnsTrue()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies EventEnabledBindingPlugin.CanHandle returns false when no event is resolved.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_CanHandle_WithoutEvent_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: null,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies EventEnabledBindingPlugin.CanHandle returns false when no Enabled property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_CanHandle_WithoutEnabled_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        var plugin = new EventEnabledBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies EventEnabledBindingPlugin emits event+Enabled with observable parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_ObservableParam_EmitsCanExecuteSync()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterIsReferenceType: true,
            resolvedEventName: ClickName,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Enabled = false");
        await Assert.That(result).Contains("view.SaveButton.Enabled = cmd.CanExecute(param)");
        await Assert.That(result).Contains("cmd.CanExecuteChanged += __canExecHandler");
        await Assert.That(result).Contains(ViewSaveButtonClickHandlerFragment);
        await Assert.That(result).Contains("lock (__parameterGate)");
        await Assert.That(result).Contains("__ReadParameter()");
    }

    /// <summary>Expression parameters use their observed stream when synchronizing enabled state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_ExpressionParam_ReadsObservedParameter()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment(ParamName)]);
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterPropertyPath: paramPath,
            resolvedEventName: ClickName,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Enabled = cmd.CanExecute(__ReadParameter())");
        await Assert.That(result).Contains(ViewSaveButtonClickHandlerFragment);
        await Assert.That(result).DoesNotContain(VolatileName);
    }

    /// <summary>Verifies EventEnabledBindingPlugin emits event+Enabled with no parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_NoParam_EmitsNullParam()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("view.SaveButton.Enabled = cmd.CanExecute(null)");
        await Assert.That(result).Contains("cmd.Execute(null)");
        await Assert.That(result).Contains(ViewSaveButtonClickHandlerFragment);
    }

    /// <summary>Verifies EventEnabledBindingPlugin uses fallback EventArgs type when null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_NullEventArgsType_UsesFallback()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            resolvedEventArgsTypeFullName: null,
            hasEnabledProperty: true);

        var plugin = new EventEnabledBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.EventArgs");
    }

    /// <summary>Verifies DefaultEventBindingPlugin.CanHandle returns true when event is resolved.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_CanHandle_WithEvent_ReturnsTrue()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        var plugin = new DefaultEventBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies DefaultEventBindingPlugin.CanHandle returns false when no event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_CanHandle_WithoutEvent_ReturnsFalse()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(resolvedEventName: null);

        var plugin = new DefaultEventBindingPlugin();
        var result = plugin.CanHandle(inv);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies DefaultEventBindingPlugin.EmitBinding uses fallback EventArgs when ResolvedEventArgsTypeFullName is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_NullEventArgsType_UsesFallback()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            resolvedEventArgsTypeFullName: null);

        var plugin = new DefaultEventBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.EventArgs");
    }

    /// <summary>Verifies DefaultEventBindingPlugin.EmitBinding uses specific EventArgs when provided.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_WithEventArgsType_UsesSpecificType()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            resolvedEventArgsTypeFullName: "global::System.Windows.RoutedEventArgs");

        var plugin = new DefaultEventBindingPlugin();
        plugin.EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Windows.RoutedEventArgs");
    }

    /// <summary>Verifies GroupByTypeSignature groups BindCommand invocations with the same type signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        const int ExpectedInvocationCount = 2;
        var inv1 = ModelFactory.CreateBindCommandInvocationInfo(callerLineNumber: 10);
        var inv2 = ModelFactory.CreateBindCommandInvocationInfo(callerLineNumber: 20);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCommandCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(ExpectedInvocationCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with different view types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentViewTypes_SeparateGroups()
    {
        const int ExpectedGroupCount = 2;
        var inv1 = ModelFactory.CreateBindCommandInvocationInfo(viewTypeFullName: "global::TestApp.ViewA");
        var inv2 = ModelFactory.CreateBindCommandInvocationInfo(viewTypeFullName: "global::TestApp.ViewB");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindCommandCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
    }

    /// <summary>Verifies GenerateConcreteOverload generates CallerArgumentExpression dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            false,
            null,
            [inv]);

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__BindCommand_");
    }

    /// <summary>Verifies GenerateConcreteOverload generates CallerFilePath dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            false,
            null,
            [inv]);

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, false, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>Verifies GenerateConcreteOverload with observable parameter includes withParameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerArgExprOverload_WithObservableParam_IncludesWithParameter()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName);
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            true,
            false,
            StringTypeName,
            [inv]);

        BindCommandCodeGenerator.GenerateCallerArgExprOverload(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(IObservableGlobalSystemStringWithParameterFragment);
        await Assert.That(result).Contains(", withParameter)");
    }

    /// <summary>Verifies GenerateConcreteOverload with expression parameter includes withParameter expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerArgExprOverload_WithExpressionParam_IncludesWithParameterExpr()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName);
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            true,
            StringTypeName,
            [inv]);

        BindCommandCodeGenerator.GenerateCallerArgExprOverload(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("Expression<global::System.Func<");
        await Assert.That(result).Contains("withParameterExpression");
    }

    /// <summary>Verifies CallerFilePath overload with observable parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_WithObservableParam_IncludesWithParameter()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName);
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            true,
            false,
            StringTypeName,
            [inv]);

        BindCommandCodeGenerator.GenerateCallerFilePathOverload(sb, group, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(IObservableGlobalSystemStringWithParameterFragment);
    }

    /// <summary>Verifies CallerFilePath overload with expression parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_WithExpressionParam_IncludesWithParameter()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName);
        var group = new BindCommandCodeGenerator.BindCommandTypeGroup(
            inv.ViewTypeFullName,
            inv.ViewModelTypeFullName,
            inv.CommandTypeFullName,
            inv.ControlTypeFullName,
            false,
            true,
            StringTypeName,
            [inv]);

        BindCommandCodeGenerator.GenerateCallerFilePathOverload(sb, group, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("Expression<global::System.Func<");
    }

    /// <summary>Verifies GenerateBindCommandMethod with CommandProperty plugin path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_CommandPropertyPlugin_EmitsCommandBinding()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            hasCommandParameterProperty: true);
        var viewModelClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, viewModelClassInfo, null, TESTSUFFIXName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindCommand_TESTSUFFIX");
        await Assert.That(result).Contains(".Command = cmd");
    }

    /// <summary>Verifies GenerateBindCommandMethod with EventEnabled plugin path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_EventEnabledPlugin_EmitsEnabledSync()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            hasCommandProperty: false,
            hasEnabledProperty: true);
        var viewModelClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, viewModelClassInfo, null, TESTSUFFIXName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindCommand_TESTSUFFIX");
        await Assert.That(result).Contains(".Enabled =");
        await Assert.That(result).Contains("CanExecuteChanged");
        await Assert.That(result).Contains("HasHigherAffinityPlugin");
    }

    /// <summary>Verifies GenerateBindCommandMethod with no plugin match falls through to custom binder + throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_NoPlugin_EmitsThrow()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: null);
        var viewModelClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, viewModelClassInfo, null, TESTSUFFIXName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("No bindable event found");
        await Assert.That(result).Contains("HasHigherAffinityPlugin");
    }

    /// <summary>Verifies EmitCommandAffinityCheck with observable parameter emits Select wrapper.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_ObservableParam_EmitsSelectWrapper()
    {
        const int GeneratedAffinity = 5;
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName);

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, ViewSaveButtonName, GeneratedAffinity, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("MapSignal<global::System.String, object>");
        await Assert.That(result).Contains("HasHigherAffinityPlugin<global::TestApp.MyButton>(5, true)");
        await Assert.That(result).Contains("GetBinder<global::TestApp.MyButton>(true)");
    }

    /// <summary>An event whose args type was not resolved hands the custom binder the base event args type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_EventWithoutArgsType_UsesEventArgs()
    {
        const int GeneratedAffinity = 5;
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo() with { ResolvedEventArgsTypeFullName = null };

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, ViewSaveButtonName, GeneratedAffinity, true);

        await Assert.That(sb.ToString()).Contains("BindCommandToObject<global::TestApp.MyButton, global::System.EventArgs>(");
    }

    /// <summary>A registered binder is handed the observed parameter, not the value it held at one moment.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_ExpressionParam_HandsTheBinderTheParameterStream()
    {
        const int GeneratedAffinity = 3;
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment(ParamName)]);
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterPropertyPath: paramPath);

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, ViewSaveButtonName, GeneratedAffinity, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(MappedParameterStreamFragment);
        await Assert.That(result).Contains("HasHigherAffinityPlugin<global::TestApp.MyButton>(3, true)");
    }

    /// <summary>Verifies EmitCommandAffinityCheck with no parameter emits ImmutableEmptySignal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmitCommandAffinityCheck_NoParam_EmitsImmutableEmptySignal()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        BindCommandCodeGenerator.EmitCommandAffinityCheck(sb, inv, ViewSaveButtonName, -1, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ImmutableEmptySignal<object>.Instance");
        await Assert.That(result).Contains("HasHigherAffinityPlugin<global::TestApp.MyButton>(-1, false)");
        await Assert.That(result).Contains("GetBinder<global::TestApp.MyButton>(false)");
    }

    /// <summary>Verifies AppendParameterObservable returns MapSignal for observable parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendParameterObservable_ObservableParam_ReturnsMapSignal()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName);

        var result = BindCommandCodeGenerator.AppendParameterObservable(new(), inv).ToString();

        await Assert.That(result).Contains("MapSignal<global::System.String, object>");
    }

    /// <summary>An expression parameter reaches a binder as the same mapped stream a supplied observable does.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendParameterObservable_ExpressionParam_ReturnsTheObservedParameter()
    {
        var paramPath = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment(ParamName)]);
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterPropertyPath: paramPath);

        var result = BindCommandCodeGenerator.AppendParameterObservable(new(), inv).ToString();

        await Assert.That(result).Contains(MappedParameterStreamFragment);
    }

    /// <summary>Verifies AppendParameterObservable returns ImmutableEmptySignal when no parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendParameterObservable_NoParam_ReturnsImmutableEmptySignal()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        var result = BindCommandCodeGenerator.AppendParameterObservable(new(), inv).ToString();

        await Assert.That(result).Contains("ImmutableEmptySignal<object>.Instance");
    }

    /// <summary>Verifies Generate returns null when invocations are empty.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_EmptyInvocations_ReturnsNull()
    {
        var result = BindCommandCodeGenerator.Generate(
            [],
            [],
            new(true, true, true));

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies Generate returns null when invocations are default.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_DefaultInvocations_ReturnsNull()
    {
        var result = BindCommandCodeGenerator.Generate(
            default,
            [],
            new(true, true, true));

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies GenerateBindCommandMethod with observable parameter generates correct method params.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_WithObservableParam_IncludesObservableParam()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName);
        var viewModelClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, viewModelClassInfo, null, TESTSUFFIXName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(IObservableGlobalSystemStringWithParameterFragment);
    }

    /// <summary>
    /// The worker observes the compile-time-extracted parameter path into <c>withParameter</c>, so both the
    /// generated binding and a registered binder consume the same stream. The
    /// <c>Expression&lt;Func&lt;...&gt;&gt;</c> parameter itself lives on the public overload (covered by
    /// <see cref="GenerateCallerArgExprOverload_WithExpressionParam_IncludesWithParameterExpr"/>).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_WithExpressionParam_ObservesTheParameterProperty()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterPropertyPath: new EquatableArray<PropertyPathSegment>(
                [ModelFactory.CreatePropertyPathSegment(ParamName)]));
        var viewModelClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, viewModelClassInfo, null, TESTSUFFIXName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("var withParameterMechanism = new global::ReactiveUI.Binding.Observables.PropertyObservable<global::System.String>");
        await Assert.That(result).Contains("var withParameter = withParameterRegistration == null");
        await Assert.That(result).Contains(MappedParameterStreamFragment);
    }

    /// <summary>
    /// A parameter whose type the extraction could not resolve is still observed. The stream is typed as
    /// <c>object</c> so the binding compiles, rather than the parameter being dropped.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindCommandMethod_ExpressionParamWithNoResolvedType_ObservesItAsObject()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasExpressionParameter: true,
            parameterTypeFullName: null,
            parameterPropertyPath: new EquatableArray<PropertyPathSegment>(
                [ModelFactory.CreatePropertyPathSegment(ParamName)]));

        BindCommandCodeGenerator.GenerateBindCommandMethod(sb, inv, null, null, TESTSUFFIXName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("var withParameterMechanism = new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<object>");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // Nullability annotations (C# 8+ targets emit nullable-aware syntax; C# 7.3 does not)
    // ───────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Verifies the DefaultEvent plugin emits a nullable handler <c>sender</c> parameter (<c>object?</c>)
    /// under nullable support, so the generated local function matches the <c>EventHandler</c> delegate.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_SupportsNullable_EmitsNullableSender()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        new DefaultEventBindingPlugin().EmitBinding(sb, inv, ViewSaveButtonName, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("void __Handler(object? sender,");
        await Assert.That(result).DoesNotContain("void __Handler(object sender,");
    }

    /// <summary>
    /// Verifies the DefaultEvent plugin emits a plain <c>object sender</c> (no nullable annotation) when the
    /// target does not support nullable reference types (C# 7.3, where <c>object?</c> is a compile error).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_EmitBinding_NoNullable_EmitsPlainSender()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo();

        new DefaultEventBindingPlugin().EmitBinding(sb, inv, ViewSaveButtonName, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("void __Handler(object sender,");
        await Assert.That(result).DoesNotContain("object? sender");
    }

    /// <summary>Verifies the EventEnabled plugin also emits a nullable handler <c>sender</c> under nullable support.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_EmitBinding_SupportsNullable_EmitsNullableSender()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            resolvedEventName: ClickName,
            hasEnabledProperty: true);

        new EventEnabledBindingPlugin().EmitBinding(sb, inv, ViewSaveButtonName, true);

        await Assert.That(sb.ToString()).Contains("void __Handler(object? sender,");
    }

    /// <summary>
    /// Verifies a reference-typed observable command parameter declares <c>__latestParam</c> as nullable
    /// under nullable support, so its <c>= default</c> initializer (which is null for reference types) is
    /// null-clean.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_SupportsNullable_ReferenceParam_EmitsNullableLatestParam()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: StringTypeName,
            parameterIsReferenceType: true,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        new CommandPropertyBindingPlugin().EmitBinding(sb, inv, ViewSaveButtonName, true);

        await Assert.That(sb.ToString()).Contains("global::System.String? __latestParam = default;");
    }

    /// <summary>
    /// Verifies a value-typed observable command parameter keeps a non-nullable <c>__latestParam</c> even
    /// under nullable support — a value-type <c>default</c> is not null, and annotating it would change the
    /// declared type's semantics.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_EmitBinding_SupportsNullable_ValueParam_KeepsNonNullableLatestParam()
    {
        var sb = WorkerBody();
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasObservableParameter: true,
            parameterTypeFullName: "global::System.Int32",
            parameterIsReferenceType: false,
            hasCommandProperty: true,
            hasCommandParameterProperty: true);

        new CommandPropertyBindingPlugin().EmitBinding(sb, inv, ViewSaveButtonName, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Int32 __latestParam = default;");
        await Assert.That(result).DoesNotContain("global::System.Int32? __latestParam");
    }

    /// <summary>
    /// Verifies the concrete BindCommand overload emits a nullable optional <c>toEvent</c> parameter
    /// (<c>string?</c>) under nullable support.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_SupportsNullable_EmitsNullableToEvent()
    {
        var sb = new SourceWriter();
        var group = BindCommandCodeGenerator.GroupByTypeSignature(
            [ModelFactory.CreateBindCommandInvocationInfo()])[0];

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, true, true, true);

        await Assert.That(sb.ToString()).Contains("string? toEvent = null");
    }

    /// <summary>Verifies the concrete BindCommand overload emits a non-nullable <c>toEvent</c> parameter on C# 7.3.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_NoNullable_EmitsPlainToEvent()
    {
        var sb = new SourceWriter();
        var group = BindCommandCodeGenerator.GroupByTypeSignature(
            [ModelFactory.CreateBindCommandInvocationInfo()])[0];

        BindCommandCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("string toEvent = null");
        await Assert.That(result).DoesNotContain("string? toEvent");
    }

    /// <summary>
    /// Verifies the full BindCommand generation emits a <c>#nullable enable</c> directive when the consumer
    /// compilation supports nullable reference types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_SupportsNullable_EmitsNullableEnableDirective()
    {
        var result = BindCommandCodeGenerator.Generate(
            [ModelFactory.CreateBindCommandInvocationInfo()],
            [],
            new(true, true, true));

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).Contains("#nullable enable");
    }

    /// <summary>Verifies the full BindCommand generation omits the <c>#nullable enable</c> directive on C# 7.3.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Generate_NoNullable_OmitsNullableEnableDirective()
    {
        var result = BindCommandCodeGenerator.Generate(
            [ModelFactory.CreateBindCommandInvocationInfo()],
            [],
            new(true, false, true));

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).DoesNotContain("#nullable enable");
    }

    /// <summary>Creates a writer positioned inside a generated worker's body, where a command binding plugin writes.</summary>
    /// <returns>A writer three levels deep: namespace, class, then member body.</returns>
    /// <remarks>A plugin finishes the worker it writes into, so it has to be handed a writer inside one.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SourceWriter WorkerBody() => new SourceWriter().Indent().Indent().Indent();
}
