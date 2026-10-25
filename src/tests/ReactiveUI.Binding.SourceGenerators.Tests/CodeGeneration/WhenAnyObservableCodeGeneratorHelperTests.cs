// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="WhenAnyObservableCodeGenerator"/> helper methods.</summary>
public class WhenAnyObservableCodeGeneratorHelperTests
{
    /// <summary>The fully qualified name of the <c>String</c> type used by these tests.</summary>
    private const string StringTypeName = "global::System.String";

    /// <summary>The fully qualified name of the observed type used by these tests.</summary>
    private const string ViewModelTypeName = "global::TestApp.MyViewModel";

    /// <summary>The observable parameter as it reads on a consumer that has nullable reference types.</summary>
    private const string NullableObservableParameter = "global::System.IObservable<global::System.String>?>> obs1,";

    /// <summary>The observable parameter as it reads on a consumer that predates nullable reference types.</summary>
    private const string PlainObservableParameter = "global::System.IObservable<global::System.String>>> obs1,";

    /// <summary>The observable property the generated overload takes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_NullableSupported_AnnotatesTheObservableParameter()
    {
        var sb = new SourceWriter();

        WhenAnyObservableCodeGenerator.GenerateConcreteOverload(sb, SingleObservableGroup(), true, true, true);

        await Assert.That(sb.ToString()).Contains(NullableObservableParameter);
    }

    /// <summary>
    /// Nullable reference types arrived in C# 8, so an annotation on the parameter would not compile for an older
    /// consumer - and the overload has to compile there for the call site to reach it at all.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateConcreteOverload_NullableUnsupported_LeavesTheObservableParameterUnannotated()
    {
        var sb = new SourceWriter();

        WhenAnyObservableCodeGenerator.GenerateConcreteOverload(sb, SingleObservableGroup(), true, false, true);

        var result = sb.ToString();
        await Assert.That(result).Contains(PlainObservableParameter);
        await Assert.That(result).DoesNotContain(NullableObservableParameter);
    }

    /// <summary>Builds a group of one invocation observing a single <c>IObservable&lt;string&gt;</c> property.</summary>
    /// <returns>The type group.</returns>
    private static WhenAnyObservableCodeGenerator.TypeGroup SingleObservableGroup()
    {
        var path = new EquatableArray<PropertyPathSegment>(
            [ModelFactory.CreatePropertyPathSegment("Feed", $"global::System.IObservable<{StringTypeName}>")]);

        WhenAnyObservableInvocationInfo invocation = new(
            "Test.cs",
            1,
            ViewModelTypeName,
            new EquatableArray<EquatableArray<PropertyPathSegment>>([path]),
            new EquatableArray<string>([StringTypeName]),
            StringTypeName,
            false,
            new EquatableArray<string>(["x => x.Feed"]));

        return new(invocation, [invocation]);
    }
}
