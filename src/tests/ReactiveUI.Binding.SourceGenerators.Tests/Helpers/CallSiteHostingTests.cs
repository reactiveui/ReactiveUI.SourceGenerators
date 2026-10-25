// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Tests for <see cref="CallSiteHosting"/>.</summary>
public class CallSiteHostingTests
{
    /// <summary>A method that uses every kind of constraint, and a type parameter that has none.</summary>
    private const string ConstrainedSource = """
                                             #nullable enable
                                             public static class Constrained
                                             {
                                                 public static void M<TA, TB, TC, TD, TE, TF>()
                                                     where TA : class?
                                                     where TB : unmanaged
                                                     where TC : struct
                                                     where TD : notnull
                                                     where TE : class, System.IDisposable, new()
                                                 {
                                                 }
                                             }
                                             """;

    /// <summary>The clauses the method above declares, written as a generic interceptor repeats them.</summary>
    private static readonly string[] ExpectedClauses =
    [
        "where TA : class?",
        "where TB : unmanaged",
        "where TC : struct",
        "where TD : notnull",
        "where TE : class, global::System.IDisposable, new()",
    ];

    /// <summary>A type with no type parameter in it, such as <c>dynamic</c>, names only type parameters in scope.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UsesOnly_DynamicType_ReturnsTrue() =>
        await Assert.That(CallSiteHosting.UsesOnly(TestHelper.CreateCompilation(string.Empty).DynamicType, [])).IsTrue();

    /// <summary>Combining a weaker kind with a stronger one keeps the stronger, and the types of both appear once.</summary>
    /// <param name="first">The first kind, or null.</param>
    /// <param name="second">The second kind, or null.</param>
    /// <param name="expected">The combined constraints.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(null, "class", "class, IA, IB")]
    [Arguments("class", null, "class, IA, IB")]
    [Arguments("class", "class", "class, IA, IB")]
    [Arguments("notnull", "class", "class, IA, IB")]
    [Arguments("class?", "class", "class, IA, IB")]
    [Arguments("class", "notnull", "class, IA, IB")]
    public async Task TypeParameterConstraints_With_KeepsTheStrongerKind(string? first, string? second, string expected)
    {
        var combined = new CallSiteHosting.TypeParameterConstraints(first, ["IA"], false)
            .With(new(second, ["IA", "IB"], false));

        await Assert.That(combined.Format()).IsEqualTo(expected);
    }

    /// <summary>A constructor constraint on either side is kept.</summary>
    /// <param name="first">Whether the first set has a constructor constraint.</param>
    /// <param name="second">Whether the second set has one.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(true, false)]
    [Arguments(false, true)]
    public async Task TypeParameterConstraints_With_KeepsAConstructorConstraintFromEither(bool first, bool second) =>
        await Assert.That(new CallSiteHosting.TypeParameterConstraints("class", [], first).With(new("class", [], second)).Format())
            .IsEqualTo("class, new()");

    /// <summary>A constructor constraint is written last, and not at all beside a value-type kind, which C# refuses.</summary>
    /// <param name="kind">The kind constraint, or null.</param>
    /// <param name="expected">The written constraints.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("class", "class, new()")]
    [Arguments("struct", "struct")]
    [Arguments("unmanaged", "unmanaged")]
    public async Task TypeParameterConstraints_Format_PlacesTheConstructorConstraint(string kind, string expected) =>
        await Assert.That(new CallSiteHosting.TypeParameterConstraints(kind, [], true).Format()).IsEqualTo(expected);

    /// <summary>A type parameter with no constraint at all writes nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TypeParameterConstraints_Format_WithoutConstraints_ReturnsNull() =>
        await Assert.That(new CallSiteHosting.TypeParameterConstraints(null, [], false).Format()).IsNull();

    /// <summary>Every kind of constraint is repeated, and a type parameter with none gets no clause.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConstraintClauses_RepeatsEveryKindOfConstraint()
    {
        var method = TestHelper.CreateCompilation(ConstrainedSource, LanguageVersion.CSharp11)
            .GetTypeByMetadataName("Constrained")!
            .GetMembers("M")
            .OfType<IMethodSymbol>()
            .Single();

        var names = new Dictionary<ITypeParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var typeParameter in method.TypeParameters)
        {
            names[typeParameter] = typeParameter.Name;
        }

        var clauses = CallSiteHosting.ConstraintClauses(method.TypeParameters, names);

        await Assert.That(clauses.ToArray()).IsEquivalentTo(ExpectedClauses);
    }
}
