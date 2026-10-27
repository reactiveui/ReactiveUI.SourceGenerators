// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Tests for <see cref="BindToExtractor"/>, which reads the element type a <c>BindTo</c> receiver carries.</summary>
public class BindToExtractorTests
{
    /// <summary>The receiver's own type answers when it is the observable itself.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverIsObservable_ReturnsElementType()
    {
        var result = BindToExtractor.GetObservableValueType(FieldType("System.IObservable<string>"));

        await Assert.That(result?.ToDisplayString()).IsEqualTo("string");
    }

    /// <summary>A type implementing the interface answers through it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverImplementsObservable_ReturnsElementType()
    {
        var result = BindToExtractor.GetObservableValueType(FieldType("Probe.Feed"));

        await Assert.That(result?.ToDisplayString()).IsEqualTo("int");
    }

    /// <summary>A type that neither is nor implements the interface carries no element type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverIsUnrelated_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(FieldType("System.String"))).IsNull();

    /// <summary>A generic type of another name is not mistaken for the interface.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverIsAnotherSingleArgGeneric_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(FieldType("System.Collections.Generic.List<string>"))).IsNull();

    /// <summary>Nothing to walk yields nothing, the same as a type implementing nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_NullReceiver_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(null)).IsNull();

    /// <summary>An interface of the same name from another namespace is a different contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverIsLookalikeInterface_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(FieldType("Probe.IObservable<string>"))).IsNull();

    /// <summary>Implementing a lookalike of the same name does not make a type observable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverImplementsLookalikeInterface_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(FieldType("Probe.Lookalike"))).IsNull();

    /// <summary>
    /// An array is no named type, so it is neither the interface nor a candidate for implementing it, and the
    /// walk over its interfaces finds only the ones every array carries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverIsArray_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(FieldType("string[]"))).IsNull();

    /// <summary>
    /// The framework interface takes exactly one type argument, so one of the same name and namespace taking
    /// another number is a different contract and carries no element type to bind.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetObservableValueType_ReceiverIsSameNameWithTwoTypeArguments_ReturnsNull() =>
        await Assert.That(BindToExtractor.GetObservableValueType(FieldType("System.IObservable<int, string>"))).IsNull();

    /// <summary>Resolves a type by declaring a field of it in a probe compilation.</summary>
    /// <param name="declaredType">The type to resolve, as it is written in source.</param>
    /// <returns>The resolved type symbol.</returns>
    /// <exception cref="InvalidOperationException">The probe did not compile.</exception>
    private static ITypeSymbol FieldType(string declaredType)
    {
        var compilation = TestHelper.CreateCompilation(
            $$"""
              namespace System
              {
                  public interface IObservable<T1, T2>
                  {
                  }
              }

              namespace Probe
              {
                  public class Feed : System.IObservable<int>
                  {
                      public System.IDisposable Subscribe(System.IObserver<int> observer) => null!;
                  }

                  public interface IObservable<T>
                  {
                  }

                  public class Lookalike : IObservable<int>
                  {
                  }

                  public class Holder
                  {
                      public {{declaredType}} Member = default!;
                  }
              }
              """,
            LanguageVersion.CSharp10);

        var holder = compilation.GetTypeByMetadataName("Probe.Holder")
            ?? throw new InvalidOperationException("The probe type did not compile.");

        return ((IFieldSymbol)holder.GetMembers("Member")[0]).Type;
    }
}
