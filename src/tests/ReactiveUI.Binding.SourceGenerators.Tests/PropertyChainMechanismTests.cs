// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers property chains whose segments notify by different mechanisms. Each link is declared by its own
/// type, so the mechanism that reaches one segment says nothing about the next: a chain rooted on a notifying
/// type can walk into one that does not notify at all, and observation has to follow the type it is actually
/// standing on.
/// </summary>
public class PropertyChainMechanismTests
{
    /// <summary>The dispatch file the WhenChanged call site is generated into.</summary>
    private const string DispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>The cast the generator emits when it believes a segment raises PropertyChanged.</summary>
    private const string NotifyingCast = "(global::System.ComponentModel.INotifyPropertyChanged)__parent";

    /// <summary>
    /// A notifying root whose chain walks into a sealed type that does not notify. Sealed is the sharp case:
    /// the compiler rejects a cast to an interface the type cannot implement, so a mis-chosen mechanism is a
    /// build error in the consumer's project rather than a cast that fails later.
    /// </summary>
    private const string SealedPocoIntermediateSource = """
                                                        using System;
                                                        using System.ComponentModel;
                                                        using ReactiveUI.Binding;

                                                        namespace Consumer
                                                        {
                                                            public sealed class Address
                                                            {
                                                                public string City { get; set; }
                                                            }

                                                            public class PersonViewModel : INotifyPropertyChanged
                                                            {
                                                                public event PropertyChangedEventHandler PropertyChanged;

                                                                public Address Address { get; set; }
                                                            }

                                                            public static class Usage
                                                            {
                                                                public static IObservable<string> Observe(PersonViewModel viewModel)
                                                                {
                                                                    return viewModel.WhenChanged(x => x.Address.City);
                                                                }
                                                            }
                                                        }
                                                        """;

    /// <summary>
    /// A chain that leaves its root's notification mechanism still produces code that compiles. The
    /// intermediate cannot raise PropertyChanged, so the segment that reads it must not be emitted as though
    /// it could.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ThroughANonNotifyingIntermediate_Compiles()
    {
        var result = TestHelper.RunGenerator(SealedPocoIntermediateSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
    }

    /// <summary>
    /// The non-notifying segment is not cast to the notification interface. An unsealed intermediate would let
    /// that cast compile and then throw on the first parent value, which is the silent half of the same defect.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ThroughANonNotifyingIntermediate_DoesNotCastItToTheNotifyingInterface()
    {
        var result = RunAgainstUnsealedIntermediate();

        await result.GeneratedSourceDoesNotContain(DispatchFileName, NotifyingCast);
    }

    /// <summary>The unsealed variant, where a wrong cast compiles and fails at runtime instead.</summary>
    /// <returns>The generator result for the unsealed-intermediate scenario.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static GeneratorTestResult RunAgainstUnsealedIntermediate() =>
        TestHelper.RunGenerator(
            SealedPocoIntermediateSource.Replace("public sealed class Address", "public class Address"),
            LanguageVersion.CSharp10);
}
