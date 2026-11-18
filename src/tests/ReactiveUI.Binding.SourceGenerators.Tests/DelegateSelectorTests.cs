// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests for selectors taken as delegates: the call site builds no expression tree, a null-conditional path is read
/// as its members, and a lambda that is not a path gets no generated binding.
/// </summary>
public class DelegateSelectorTests
{
    /// <summary>The generated file that holds the after-change observations.</summary>
    private const string WhenChangedDispatchHint = "WhenChangedDispatch.g.cs";

    /// <summary>The namespace of the expression tree types.</summary>
    private const string ExpressionsNamespace = "System.Linq.Expressions";

    /// <summary>The name of the method the selector is written in.</summary>
    private const string CallerName = "Observe";

    /// <summary>The IL opcode of a direct call.</summary>
    private const byte CallOpCode = 0x28;

    /// <summary>The IL opcode of a virtual call.</summary>
    private const byte CallVirtualOpCode = 0x6F;

    /// <summary>The size in bytes of the token that follows a call opcode.</summary>
    private const int TokenSize = 4;

    /// <summary>A view model with a nullable child, and a call left open for one selector.</summary>
    private const string Source = """
                                  using System;
                                  using System.ComponentModel;
                                  using ReactiveUI.Binding;

                                  namespace TestApp
                                  {
                                      public class Child : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler? PropertyChanged;
                                          public string Name { get; set; } = "";
                                          public int Count { get; set; }
                                          public int? Limit { get; set; }
                                          public Child? Next { get; set; }
                                      }

                                      public class Parent : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler? PropertyChanged;
                                          public string Name { get; set; } = "";
                                          public Child? Child { get; set; }
                                      }

                                      public static class Usage
                                      {
                                          public static object Observe(Parent parent) => parent.WhenChanged(SELECTOR);
                                      }
                                  }
                                  """;

    /// <summary>Verifies a path written with <c>?.</c> generates the observation its members name.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <param name="leaf">The property the path ends at.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Child?.Name", "Name")]
    [Arguments("x => x.Child?.Next?.Name", "Name")]
    [Arguments("x => x.Child?.Next!.Name", "Name")]
    [Arguments("x => (x.Child?.Next)!.Name", "Name")]
    [Arguments("x => x.Child?.Limit", "Limit")]
    [Arguments("x => x.Child?.Next", "Next")]
    public async Task ConditionalPath_IsGenerated(string selector, string leaf)
    {
        var result = TestHelper.RunGenerator(WithSelector(selector), LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(WhenChangedDispatchHint, $"\"{selector}\"");
        await result.GeneratedSourceContains(WhenChangedDispatchHint, $"\"{leaf}\"");
    }

    /// <summary>Verifies a path written with <c>?.</c> generates the same observation as the path written with <c>!.</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConditionalPath_GeneratesWhatTheNullForgivingPathDoes()
    {
        const string Conditional = "x => x.Child?.Next?.Name";
        const string Forgiving = "x => x.Child!.Next!.Name";

        var conditional = TestHelper.RunGenerator(WithSelector(Conditional), LanguageVersion.CSharp10).GeneratedSources[WhenChangedDispatchHint];
        var forgiving = TestHelper.RunGenerator(WithSelector(Forgiving), LanguageVersion.CSharp10).GeneratedSources[WhenChangedDispatchHint];

        await Assert.That(BodyOfObservation(conditional)).IsEqualTo(BodyOfObservation(forgiving));
    }

    /// <summary>Verifies a selector that is not a property path gets no generated observation.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Child?.Count")]
    [Arguments("x => x.Child?.Next?.Count")]
    [Arguments("x => { return x.Name; }")]
    [Arguments("x => x.Name + \"!\"")]
    [Arguments("x => x")]
    public async Task NotAPropertyPath_IsNotGenerated(string selector)
    {
        var result = TestHelper.RunGenerator(WithSelector(selector), LanguageVersion.CSharp10);

        await Assert.That(result.GeneratedSources.ContainsKey(WhenChangedDispatchHint)).IsFalse();
    }

    /// <summary>Verifies the method that calls <c>WhenChanged</c> builds no expression tree for its selector.</summary>
    /// <param name="intercept">Whether the call site is claimed by an interceptor.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task CallSite_BuildsNoExpressionTree(bool intercept)
    {
        var parseOptions = intercept
            ? TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11)
            : TestHelper.ParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(WithSelector("static x => x.Child!.Name"), parseOptions, false, "TestAssembly", []);
        var result = TestHelper.RunGenerator(compilation, parseOptions, "TestAssembly", intercept);
        await result.CompilationSucceeds();

        await using var image = new MemoryStream();
        var emitted = result.OutputCompilation.Emit(image);
        await Assert.That(emitted.Success).IsTrue();
        image.Position = 0;

        var expressionCalls = ExpressionCallsIn(image, CallerName);

        await Assert.That(expressionCalls).IsEqualTo(0);
    }

    /// <summary>Counts the calls a method makes to the expression tree types.</summary>
    /// <param name="image">The assembly.</param>
    /// <param name="methodName">The name of the method to read.</param>
    /// <returns>The number of calls into <c>System.Linq.Expressions</c>, or -1 when the method is not found.</returns>
    private static int ExpressionCallsIn(Stream image, string methodName)
    {
        using var pe = new PEReader(image);
        var reader = pe.GetMetadataReader();
        foreach (var handle in reader.MethodDefinitions)
        {
            var method = reader.GetMethodDefinition(handle);
            if (reader.GetString(method.Name) != methodName)
            {
                continue;
            }

            var il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
            var calls = 0;
            for (var i = 0; i + TokenSize < il.Length; i++)
            {
                if (il[i] is CallOpCode or CallVirtualOpCode && IsExpressionMember(reader, BitConverter.ToInt32(il, i + 1)))
                {
                    calls++;
                }
            }

            return calls;
        }

        return -1;
    }

    /// <summary>Determines whether a call token names a member of an expression tree type.</summary>
    /// <param name="reader">The metadata reader.</param>
    /// <param name="token">The token that follows a call opcode.</param>
    /// <returns><see langword="true"/> when the member's type is in <c>System.Linq.Expressions</c>.</returns>
    private static bool IsExpressionMember(MetadataReader reader, int token)
    {
        var entity = MetadataTokens.EntityHandle(token);
        if (entity.Kind == HandleKind.MethodSpecification)
        {
            entity = reader.GetMethodSpecification((MethodSpecificationHandle)entity).Method;
        }

        return entity.Kind == HandleKind.MemberReference
            && reader.GetMemberReference((MemberReferenceHandle)entity).Parent is { Kind: HandleKind.TypeReference } parent
            && reader.GetString(reader.GetTypeReference((TypeReferenceHandle)parent).Namespace) == ExpressionsNamespace;
    }

    /// <summary>Cuts the generated observation method out of a dispatch file, without the name that carries the selector's text.</summary>
    /// <param name="generated">The generated dispatch file.</param>
    /// <returns>The text from the method's opening brace to the end of the file.</returns>
    private static string BodyOfObservation(string generated)
    {
        var method = generated.IndexOf("private static", StringComparison.Ordinal);
        return generated[generated.IndexOf('{', method)..];
    }

    /// <summary>Writes a selector into the call of <see cref="Source"/>.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <returns>The source to compile.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static string WithSelector(string selector) => Source.Replace("SELECTOR", selector, StringComparison.Ordinal);
}
