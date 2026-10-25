// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="ObservationCodeGenerator"/> helper methods exercised via WhenChanging scenarios.</summary>
public class WhenChangingCodeGeneratorHelperTests
{
    /// <summary>The <c>WhenChanging</c> name these tests generate against.</summary>
    private const string WhenChangingName = "WhenChanging";

    /// <summary>The <c>__WhenChanging_</c> local the generated code is expected to emit.</summary>
    private const string WhenChangingLocal = "__WhenChanging_";

    /// <summary>Verifies GroupByTypeSignature groups invocations with the same source and return types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_SameTypes_GroupedTogether()
    {
        const int ExpectedInvocationCount = 2;
        var inv1 = ModelFactory.CreateInvocationInfo(
            callerLineNumber: 10,
            isBeforeChange: true,
            methodName: WhenChangingName);
        var inv2 = ModelFactory.CreateInvocationInfo(
            callerLineNumber: 20,
            isBeforeChange: true,
            methodName: WhenChangingName);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(1);
        await Assert.That(groups[0].Invocations.Length).IsEqualTo(ExpectedInvocationCount);
    }

    /// <summary>Verifies GroupByTypeSignature separates invocations with different source types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupByTypeSignature_DifferentSourceTypes_SeparateGroups()
    {
        const int ExpectedGroupCount = 2;
        var inv1 = ModelFactory.CreateInvocationInfo(
            sourceTypeFullName: "global::TestApp.ViewModelA",
            isBeforeChange: true);
        var inv2 = ModelFactory.CreateInvocationInfo(
            sourceTypeFullName: "global::TestApp.ViewModelB",
            isBeforeChange: true);
        var invocations = ImmutableArray.Create(inv1, inv2);

        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        await Assert.That(groups.Count).IsEqualTo(ExpectedGroupCount);
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerArgExpr when supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprSupported_GeneratesCallerArgExprOverload()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true, methodName: WhenChangingName);
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangingName);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerArgumentExpression");
        await Assert.That(result).Contains(WhenChangingLocal);
    }

    /// <summary>Verifies GenerateConcreteOverload dispatches to CallerFilePath when not supported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExprNotSupported_GeneratesCallerFilePathOverload()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true, methodName: WhenChangingName);
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, false, false, false, WhenChangingName);

        var result = sb.ToString();
        await Assert.That(result).Contains("CallerFilePath");
        await Assert.That(result).Contains("callerLineNumber");
    }

    /// <summary>Verifies GenerateConcreteOverload with CallerArgExpr generates propertyExpression dispatch for WhenChanging.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerArgExpr_GeneratesPropertyExpressionDispatch()
    {
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(isBeforeChange: true, methodName: WhenChangingName);
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, true, false, true, WhenChangingName);

        var result = sb.ToString();
        await Assert.That(result).Contains("Expression ==");
        await Assert.That(result).Contains(WhenChangingName);
        await Assert.That(result).Contains(WhenChangingLocal);
    }

    /// <summary>Verifies GenerateConcreteOverload with CallerFilePath generates file path + line number dispatch for WhenChanging.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_CallerFilePath_GeneratesFilePathDispatch()
    {
        const int CallerLineNumber = 42;
        var sb = new SourceWriter();
        var inv = ModelFactory.CreateInvocationInfo(
            "/src/ViewModels/MyViewModel.cs",
            CallerLineNumber,
            isBeforeChange: true,
            methodName: WhenChangingName);
        var group = new ObservationCodeGenerator.TypeGroup(inv, [inv]);

        ObservationCodeGenerator.GenerateConcreteOverload(sb, group, false, false, false, WhenChangingName);

        var result = sb.ToString();
        await Assert.That(result).Contains("callerLineNumber == 42");
        await Assert.That(result).Contains("callerFilePath.EndsWith");
        await Assert.That(result).Contains(WhenChangingLocal);
    }
}
