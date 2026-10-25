// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="BindCodeGenerator"/> helper methods.</summary>
public class BindCodeGeneratorHelperTests
{
    /// <summary>The fully qualified name of the <c>View</c> type used by these tests.</summary>
    private const string ViewTypeName = "global::TestApp.View";

    /// <summary>The fully qualified name of the <c>VM</c> type used by these tests.</summary>
    private const string VMTypeName = "global::TestApp.VM";

    /// <summary>The <c>IReactiveBinding</c> name these tests generate against.</summary>
    private const string IReactiveBindingName = "IReactiveBinding";

    /// <summary>The <c>vmToViewConverter</c> name these tests generate against.</summary>
    private const string ViewModelToViewConverterName = "viewModelToViewConverter";

    /// <summary>The <c>viewToVmConverter</c> name these tests generate against.</summary>
    private const string ViewToViewModelConverterName = "viewToViewModelConverter";

    /// <summary>The <c>scheduler</c> name these tests generate against.</summary>
    private const string SchedulerName = "scheduler";

    /// <summary>The fully qualified name of the <c>MyView</c> type used by these tests.</summary>
    private const string MyViewTypeName = "global::TestApp.MyView";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>Verifies GroupByTypeSignature groups invocations with the same type signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10, methodName: "Bind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20, methodName: "Bind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        const int expectedInvocationCount = 2;
        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(expectedInvocationCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with different source types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentSourceTypes_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(
            sourceTypeFullName: "global::TestApp.ViewModelA",
            methodName: "Bind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(
            sourceTypeFullName: "global::TestApp.ViewModelB",
            methodName: "Bind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        const int ExpectedGroupCount = 2;
        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with HasConversion difference.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentHasConversion_SeparateGroups()
    {
        var inv1 = ModelFactory.CreateBindingInvocationInfo(hasConversion: false, methodName: "Bind");
        var inv2 = ModelFactory.CreateBindingInvocationInfo(hasConversion: true, methodName: "Bind");
        var invocations = ImmutableArray.Create(inv1, inv2);

        const int ExpectedGroupCount = 2;
        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
    }

    /// <summary>Verifies FormatExtraArgs returns empty string when no conversion or scheduler.</summary>
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

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraArgs includes converter args when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesConverterArgs()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            true,
            false,
            []);

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains(ViewModelToViewConverterName);
        await Assert.That(result).Contains(ViewToViewModelConverterName);
    }

    /// <summary>Verifies FormatExtraArgs includes scheduler arg when HasScheduler is true.</summary>
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

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains(SchedulerName);
    }

    /// <summary>Verifies FormatExtraArgs includes both converter and scheduler when both are true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversionAndScheduler_IncludesBoth()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            true,
            true,
            []);

        var result = BindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains(ViewModelToViewConverterName);
        await Assert.That(result).Contains(SchedulerName);
    }

    /// <summary>Verifies FormatReturnType produces expected IReactiveBinding type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatReturnType_ReturnsIReactiveBindingType()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            []);

        var result = BindCodeGenerator.FormatReturnType(group);

        await Assert.That(result).Contains(IReactiveBindingName);
        await Assert.That(result).Contains(ViewTypeName);
    }

    /// <summary>Verifies FormatMethodReturnType produces expected IReactiveBinding type for invocation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatMethodReturnType_ReturnsIReactiveBindingType()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");

        var result = BindCodeGenerator.FormatMethodReturnType(inv);

        await Assert.That(result).Contains(IReactiveBindingName);
        await Assert.That(result).Contains(MyViewTypeName);
    }

    /// <summary>Verifies FormatExtraMethodParams returns empty when no conversion or scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: false,
            hasScheduler: false,
            methodName: "Bind");

        var result = BindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraMethodParams includes Func types when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesFuncParams()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true, methodName: "Bind");

        var result = BindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("global::System.Func<");
        await Assert.That(result).Contains(ViewModelToViewConverterName);
        await Assert.That(result).Contains(ViewToViewModelConverterName);
    }

    /// <summary>Verifies FormatExtraMethodParams includes ISequencer when HasScheduler is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithScheduler_IncludesSchedulerParam()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasScheduler: true, methodName: "Bind");

        var result = BindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("ISequencer");
        await Assert.That(result).Contains(SchedulerName);
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerArgExpr when supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");
        var group = new BindingTypeGroup(
            "global::TestApp.MyViewModel",
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindCodeGenerator.DispatchApi, true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__Bind_");
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");
        var group = new BindingTypeGroup(
            "global::TestApp.MyViewModel",
            MyViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, BindCodeGenerator.DispatchApi, false, false, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>Verifies GenerateBindMethod generates two-way binding with inline PropertyObservable and view-first parameter ordering.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateBindMethod_StandardInvocation_GeneratesTwoWayBinding()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: "Bind");
        var sourceClassInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);
        var targetClassInfo = ModelFactory.CreateClassBindingInfo(
            MyViewTypeName,
            "MyView",
            implementsINPC: true);

        BindCodeGenerator.GenerateBindMethod(sb, inv, sourceClassInfo, targetClassInfo, "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("__Bind_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("ReactiveBinding");
        await Assert.That(result).Contains("view.Text = value");
        await Assert.That(result).Contains("viewModel.Name = value");
        await Assert.That(result).DoesNotContain("Skip");
    }

    /// <summary>Verifies AppendExtraParameters appends conversion parameters.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithConversion_AppendsConverterParams()
    {
        var sb = new SourceWriter();
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            "global::System.Int32",
            true,
            false,
            []);

        BindCodeGenerator.AppendExtraParameters(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains(ViewModelToViewConverterName);
        await Assert.That(result).Contains(ViewToViewModelConverterName);
        await Assert.That(result).Contains("global::System.Func<global::System.String, global::System.Int32>");
    }

    /// <summary>Verifies AppendExtraParameters appends scheduler parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithScheduler_AppendsSchedulerParam()
    {
        var sb = new SourceWriter();
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            true,
            []);

        BindCodeGenerator.AppendExtraParameters(sb, group, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("ISequencer scheduler,");
    }

    /// <summary>Verifies AppendExtraParameters declares the scheduler nullable when the target supports it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendExtraParameters_WithSchedulerAndNullableSupport_DeclaresSchedulerNullable()
    {
        var sb = new SourceWriter();
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            false,
            true,
            []);

        BindCodeGenerator.AppendExtraParameters(sb, group, true);

        await Assert.That(sb.ToString()).Contains("ISequencer? scheduler,");
    }
}
