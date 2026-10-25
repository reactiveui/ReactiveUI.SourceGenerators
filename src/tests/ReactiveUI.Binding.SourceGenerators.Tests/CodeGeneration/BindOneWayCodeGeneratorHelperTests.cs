// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindOneWayCodeGenerator"/> helper methods.</summary>
public class BindOneWayCodeGeneratorHelperTests
{
    /// <summary>The <c>__BindOneWay_</c> local the generated code is expected to emit.</summary>
    private const string BindOneWayLocal = "__BindOneWay_";

    /// <summary>The <c>conversionFunc</c> name these tests generate against.</summary>
    private const string ConversionFuncName = "conversionFunc";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the <c>MyView</c> type used by these tests.</summary>
    private const string MyViewTypeName = "global::TestApp.MyView";

    /// <summary>The fully qualified name of the <c>MyViewModel</c> type used by these tests.</summary>
    private const string MyViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>The <c>TEST00000000TEST</c> name these tests generate against.</summary>
    private const string TEST00000000TESTName = "TEST00000000TEST";

    /// <summary>Verifies GroupByTypeSignature groups invocations with the same type signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        const int ExpectedInvocationCount = 2;
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(ExpectedInvocationCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with different source types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentSourceTypes_SeparateGroups()
    {
        const int ExpectedGroupCount = 2;
        var inv1 = ModelFactory.CreateBindingInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelA");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(sourceTypeFullName: "global::TestApp.ViewModelB");
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
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindOneWayCodeGenerator.DispatchApi, true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains(BindOneWayLocal);
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprNotSupported_GeneratesCallerFilePathOverload()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindOneWayCodeGenerator.DispatchApi, false, false, false);

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
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindOneWayCodeGenerator.DispatchApi, true, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("sourcePropertyExpression == ");
        await Assert.That(result).Contains("targetPropertyExpression == ");
        await Assert.That(result).Contains(BindOneWayLocal);
    }

    /// <summary>Verifies GenerateCallerFilePathOverload generates file path dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateCallerFilePathOverload_SingleInvocation_GeneratesFilePathDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var group = new BindingTypeGroup(
            MyViewModelTypeName,
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindOneWayCodeGenerator.DispatchApi, false, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber == 50");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains(BindOneWayLocal);
    }

    /// <summary>Verifies GenerateBindOneWayMethod generates PropertyObservable + Subscribe pattern.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindOneWayMethod_StandardInvocation_GeneratesPropertyObservableSubscribe()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo();
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindOneWayCodeGenerator.GenerateBindOneWayMethod(sb, inv, classInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).Contains("__BindOneWay_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("Subscribe");
        await Assert.That(result).Contains("target.Text = value");
    }

    /// <summary>Verifies FormatExtraArgs returns empty when no conversion or scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_NoConversionNoScheduler_ReturnsEmpty()
    {
        var group = new BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            StringTypeName,
            StringTypeName,
            false,
            false,
            []);

        var result = BindOneWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraArgs includes conversion function when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesConversionFunc()
    {
        var group = new BindingTypeGroup(
            "global::TestApp.VM",
            "global::TestApp.View",
            StringTypeName,
            StringTypeName,
            true,
            false,
            []);

        var result = BindOneWayCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains(ConversionFuncName);
    }

    /// <summary>Verifies FormatExtraMethodParams returns empty when no extra params.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo();

        var result = BindOneWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraMethodParams includes Func type when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesFuncParam()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true);

        var result = BindOneWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("global::System.Func<");
        await Assert.That(result).Contains(ConversionFuncName);
    }

    /// <summary>Verifies FormatExtraMethodParams includes ISequencer when HasScheduler is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithScheduler_IncludesSchedulerParam()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasScheduler: true);

        var result = BindOneWayCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("ISequencer");
        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>Verifies GenerateBindOneWayMethod with conversion includes .Select chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindOneWayMethod_WithConversion_IncludesMapChain()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindOneWayCodeGenerator.GenerateBindOneWayMethod(sb, inv, classInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).Contains("MapSignal<");
        await Assert.That(result).Contains(ConversionFuncName);
    }

    /// <summary>A scheduled binding constructs a typed witness operator and bypasses the immediate scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindOneWayMethod_WithScheduler_IncludesWitnessOnSignal()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(hasScheduler: true);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        BindOneWayCodeGenerator.GenerateBindOneWayMethod(sb, inv, classInfo, TEST00000000TESTName);

        var result = sb.ToString();
        await Assert.That(result).Contains("new global::ReactiveUI.Primitives.Advanced.WitnessOnSignal<global::System.String>(sourceObs, scheduler)");
        await Assert.That(result).Contains("scheduler == global::ReactiveUI.Primitives.Concurrency.Sequencer.Immediate ? (global::System.IObservable<global::System.String>)sourceObs");
    }
}
