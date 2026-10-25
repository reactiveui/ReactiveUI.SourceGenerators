// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="OneWayBindCodeGenerator"/> helper methods.</summary>
public class OneWayBindCodeGeneratorHelperTests
{
    /// <summary>The fully qualified name of the <c>Int32</c> type used by these tests.</summary>
    private const string Int32TypeName = "global::System.Int32";

    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the <c>View</c> type used by these tests.</summary>
    private const string ViewTypeName = "global::TestApp.View";

    /// <summary>The fully qualified name of the <c>VM</c> type used by these tests.</summary>
    private const string VMTypeName = "global::TestApp.VM";

    /// <summary>The <c>IReactiveBinding</c> name these tests generate against.</summary>
    private const string IReactiveBindingName = "IReactiveBinding";

    /// <summary>The <c>OneWayBind</c> name these tests generate against.</summary>
    private const string OneWayBindName = "OneWayBind";

    /// <summary>Verifies GroupByTypeSignature groups invocations with the same type signature.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameSignature_GroupedTogether()
    {
        const int ExpectedInvocationCount = 2;
        var inv1 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 10, methodName: OneWayBindName);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(callerLineNumber: 20, methodName: OneWayBindName);
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
            methodName: OneWayBindName);
        var inv2 = ModelFactory.CreateBindingInvocationInfo(
            targetTypeFullName: "global::TestApp.ViewB",
            methodName: OneWayBindName);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = BindingEmitterHelpers.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
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

        var result = OneWayBindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraArgs includes selector when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraArgs_WithConversion_IncludesSelectorArg()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            StringTypeName,
            StringTypeName,
            true,
            false,
            []);

        var result = OneWayBindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("selector");
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

        var result = OneWayBindCodeGenerator.FormatExtraArgs(group);

        await Assert.That(result).Contains("scheduler");
    }

    /// <summary>
    /// Verifies FormatReturnType uses the view property's type even with no converter supplied. That is what the
    /// runtime stub declares; the source type only agrees with it while both sides happen to match.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatReturnType_NoConversion_UsesTargetPropertyType()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            Int32TypeName,
            StringTypeName,
            false,
            false,
            []);

        var result = OneWayBindCodeGenerator.FormatReturnType(group);

        await Assert.That(result).Contains(IReactiveBindingName);
        await Assert.That(result).Contains(StringTypeName);
    }

    /// <summary>Verifies FormatReturnType with conversion uses target property type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatReturnType_WithConversion_UsesTargetPropertyType()
    {
        var group = new BindingTypeGroup(
            VMTypeName,
            ViewTypeName,
            Int32TypeName,
            StringTypeName,
            true,
            false,
            []);

        var result = OneWayBindCodeGenerator.FormatReturnType(group);

        await Assert.That(result).Contains(IReactiveBindingName);
        await Assert.That(result).Contains(StringTypeName);
    }

    /// <summary>Verifies FormatMethodReturnType uses target property type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatMethodReturnType_ReturnsTargetPropertyType()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            targetPropertyTypeFullName: Int32TypeName,
            methodName: OneWayBindName);

        var result = OneWayBindCodeGenerator.FormatMethodReturnType(inv);

        await Assert.That(result).Contains(IReactiveBindingName);
        await Assert.That(result).Contains(Int32TypeName);
    }

    /// <summary>Verifies FormatExtraMethodParams returns empty when no conversion or scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_NoConversionNoScheduler_ReturnsEmpty()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(
            hasConversion: false,
            hasScheduler: false,
            methodName: OneWayBindName);

        var result = OneWayBindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies FormatExtraMethodParams includes Func selector when HasConversion is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FormatExtraMethodParams_WithConversion_IncludesFuncSelector()
    {
        var inv = ModelFactory.CreateBindingInvocationInfo(hasConversion: true, methodName: OneWayBindName);

        var result = OneWayBindCodeGenerator.FormatExtraMethodParams(inv);

        await Assert.That(result).Contains("global::System.Func<");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>Verifies GenerateConcreteOverload with CallerArgExpr generates expression dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesExpressionDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: OneWayBindName);
        var group = new BindingTypeGroup(
            "global::TestApp.MyViewModel",
            "global::TestApp.MyView",
            StringTypeName,
            StringTypeName,
            false,
            false,
            [inv]);

        BindingEmitterHelpers.GenerateDispatchOverload(sb, group, OneWayBindCodeGenerator.DispatchApi, true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains("__OneWayBind_");
    }

    /// <summary>Verifies GenerateOneWayBindMethod generates PropertyObservable + Subscribe + ReactiveBinding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateOneWayBindMethod_StandardInvocation_GeneratesReactiveBinding()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateBindingInvocationInfo(methodName: OneWayBindName);
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        OneWayBindCodeGenerator.GenerateOneWayBindMethod(sb, inv, classInfo, null, "TEST00000000TEST");

        var result = sb.ToString();
        await Assert.That(result).Contains("__OneWayBind_TEST00000000TEST");
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("INotifyPropertyChanged");
        await Assert.That(result).Contains("ReactiveBinding");
        await Assert.That(result).Contains("BindingDirection.OneWay");
        await Assert.That(result).Contains("view.Text = value");
    }
}
