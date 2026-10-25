// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindTwoWayCodeGenerator"/> helper methods.</summary>
public class BindTwoWayCodeGeneratorHelperTests
{
    /// <summary>The <c>BindTwoWay</c> name these tests generate against.</summary>
    private const string BindTwoWayName = "BindTwoWay";

    /// <summary>The <c>__BindTwoWay_</c> local the generated code is expected to emit.</summary>
    private const string BindTwoWayLocal = "__BindTwoWay_";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the <c>MyView</c> type used by these tests.</summary>
    private const string MyViewTypeName = "global::TestApp.MyView";

    /// <summary>The fully qualified name of the <c>MyViewModel</c> type used by these tests.</summary>
    private const string MyViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>The fully qualified name of the <c>View</c> type used by these tests.</summary>
    private const string ViewTypeName = "global::TestApp.View";

    /// <summary>The fully qualified name of the <c>VM</c> type used by these tests.</summary>
    private const string VMTypeName = "global::TestApp.VM";

    /// <summary>The <c>MyView</c> name these tests generate against.</summary>
    private const string MyViewName = "MyView";

    /// <summary>The <c>sourceToTargetConv</c> name these tests generate against.</summary>
    private const string SourceToTargetConvName = "sourceToTargetConv";

    /// <summary>The <c>targetToSourceConv</c> name these tests generate against.</summary>
    private const string TargetToSourceConvName = "targetToSourceConv";

    /// <summary>The <c>TEST00000000TEST</c> name these tests generate against.</summary>
    private const string TEST00000000TESTName = "TEST00000000TEST";

    /// <summary>Verifies GroupByTypeSignature groups invocations with the same type signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        const int ExpectedInvocationCount = 2;
        var inv1 = ModelFactory.CreateBindingInvocationInfo(
            callerLineNumber: 10,
            isTwoWay: true,
            methodName: BindTwoWayName);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(
            callerLineNumber: 20,
            isTwoWay: true,
            methodName: BindTwoWayName);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(ExpectedInvocationCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with different target types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentTargetTypes_SeparateGroups()
    {
        const int ExpectedGroupCount = 2;
        var inv1 = ModelFactory.CreateBindingInvocationInfo(
            targetTypeFullName: "global::TestApp.ViewA",
            isTwoWay: true);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(
            targetTypeFullName: "global::TestApp.ViewB",
            isTwoWay: true);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerArgExpr when supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprSupported_GeneratesCallerArgExprOverload()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: BindTwoWayName);
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindTwoWayCodeGenerator.DispatchApi, true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains(BindTwoWayLocal);
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprNotSupported_GeneratesCallerFilePathOverload()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: BindTwoWayName);
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindTwoWayCodeGenerator.DispatchApi, false, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerFilePath");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>Verifies GenerateCallerArgExprOverload generates dual expression dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerArgExprOverload_SingleInvocation_GeneratesDualExpressionDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            isTwoWay: true,
            methodName: BindTwoWayName);
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindTwoWayCodeGenerator.DispatchApi, true, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("sourcePropertyExpression == ");
        await Assert.That(result).Contains("targetPropertyExpression == ");
        await Assert.That(result).Contains(BindTwoWayLocal);
    }

    /// <summary>Verifies GenerateCallerFilePathOverload generates file path dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_SingleInvocation_GeneratesFilePathDispatch()
    {
        const int CallerLineNumber = 55;
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            "/src/Views/MyView.cs",
            CallerLineNumber,
            isTwoWay: true,
            methodName: BindTwoWayName);
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindTwoWayCodeGenerator.DispatchApi, false, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber == 55");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains(BindTwoWayLocal);
    }

    /// <summary>Verifies GenerateBindTwoWayMethod generates PropertyObservable + CompositeDisposable pattern.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_StandardInvocation_GeneratesMultipleDisposable()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: BindTwoWayName);
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            MyViewTypeName,
            MyViewName,
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindTwoWay_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("MultipleDisposable");
        await Assert.That(result).Contains("target.Text = value");
        await Assert.That(result).Contains("source.Name = value");
    }

    /// <summary>Verifies FormatExtraArgs returns empty when no conversion or scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_NoConversionNoScheduler_ReturnsEmpty()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            []);

        var result = BindTwoWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraArgs includes two-way converter args when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesTwoWayConverterArgs()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            true,
            false,
            []);

        var result = BindTwoWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains(SourceToTargetConvName);
        await Assert.That(result).Contains(TargetToSourceConvName);
    }

    /// <summary>Verifies FormatExtraArgs includes scheduler when HasScheduler is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithScheduler_IncludesSchedulerArg()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            true,
            []);

        var result = BindTwoWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>Verifies FormatExtraMethodParams returns empty when no conversion or scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: false,
            hasScheduler: false,
            isTwoWay: true,
            methodName: BindTwoWayName);

        var result = BindTwoWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraMethodParams includes two-way Func params when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesTwoWayFuncParams()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: true,
            isTwoWay: true,
            methodName: BindTwoWayName);

        var result = BindTwoWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains(SourceToTargetConvName);
        await Assert.That(result).Contains(TargetToSourceConvName);
        await Assert.That(result).Contains("global::System.Func<");
    }

    /// <summary>Verifies GenerateBindTwoWayMethod with conversion includes .Select chains.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_WithConversion_IncludesMapChains()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: true,
            isTwoWay: true,
            methodName: BindTwoWayName);
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            MyViewTypeName,
            MyViewName,
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).Contains("MapSignal<");
        await Assert.That(result).Contains(SourceToTargetConvName);
        await Assert.That(result).Contains(TargetToSourceConvName);
    }

    /// <summary>Both binding directions route through the latest-wins sequencer stage and bypass the immediate scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_WithScheduler_RoutesBothDirectionsThroughTheLatestWinsStage()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasScheduler: true,
            isTwoWay: true,
            methodName: BindTwoWayName);
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            MyViewTypeName,
            MyViewName,
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::ReactiveUI.Binding.BindingSchedulers.ObserveOnSequencer<global::System.String>(sourceObs, scheduler)");
        await Assert.That(result).Contains("global::ReactiveUI.Binding.BindingSchedulers.ObserveOnSequencer<global::System.String>(targetObs, scheduler)");
        await Assert.That(result).DoesNotContain("WitnessOnSignal");
        await Assert.That(result).Contains("scheduler == global::ReactiveUI.Primitives.Concurrency.Sequencer.Immediate ? (global::System.IObservable<global::System.String>)sourceObs");
        await Assert.That(result).Contains("scheduler == global::ReactiveUI.Primitives.Concurrency.Sequencer.Immediate ? (global::System.IObservable<global::System.String>)targetObs");
    }

    /// <summary>
    /// The target's first value is weighed rather than dropped by position, and both directions are routed.
    /// Dropping the first value positionally eats a real change wherever the target reports nothing to begin
    /// with, and routing only the source direction leaves the write back running wherever the view raised it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindTwoWayMethod_RoutesBothDirectionsAndWeighsTheTargetsFirstValue()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(isTwoWay: true, methodName: BindTwoWayName);
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            MyViewTypeName,
            MyViewName,
            implementsINPC: true);

        BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(sb, inv, sourceClassInfo, targetClassInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).DoesNotContain("Skip");
        await Assert.That(result).Contains("targetThreadObs");
        await Assert.That(result).Contains("sourceThreadObs");
    }

    /// <summary>Verifies AppendExtraParameters appends conversion parameters with correct types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithConversion_AppendsConverterParamsWithTypes()
    {
        var sb = new SourceWriter();
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            "global::System.Int32",
            StringTypeName,
            true,
            false,
            []);

        BindTwoWayCodeGenerator.AppendExtraParameters(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("global::System.Func<global::System.Int32, global::System.String>");
        await Assert.That(result).Contains("global::System.Func<global::System.String, global::System.Int32>");
    }
}
