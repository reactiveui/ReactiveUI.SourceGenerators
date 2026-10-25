// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="HostedSourceReader"/>, which reads an API's file back so its members can move.</summary>
public class HostedSourceReaderTests
{
    /// <summary>The name of the class the files in these tests write their members in.</summary>
    private const string ClassName = "Generated";

    /// <summary>A file whose interceptor opens its parameter list on its own line.</summary>
    private const string InlineReceiverFile = """
                                              namespace Gen
                                              {
                                                  internal static partial class Generated
                                                  {
                                                      [global::System.Runtime.CompilerServices.InterceptsLocation(1, "first")]
                                                      [global::System.Runtime.CompilerServices.InterceptsLocation(1, "second")]
                                                      internal static int __Intercept_Probe_A(this int value) => value;

                                                      private static int Helper(int value) => value;
                                                  }
                                              }

                                              namespace Other
                                              {
                                                  partial class Accessor
                                                  {
                                                  }
                                              }
                                              """;

    /// <summary>A file that has no generated class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryRead_WithoutTheGeneratedClass_ReturnsFalse() =>
        await Assert.That(HostedSourceReader.TryRead("namespace Gen\n{\n}\n", ClassName, out _)).IsFalse();

    /// <summary>A file whose generated class never closes has no body to read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryRead_WithoutTheClosingBraces_ReturnsFalse() =>
        await Assert.That(HostedSourceReader.TryRead(
                "namespace Gen\n{\n    internal static partial class Generated\n    {\n        private static int Value;\n",
                ClassName,
                out _))
            .IsFalse();

    /// <summary>Every attribute on an interceptor claims its call site for that interceptor, and the attributes are dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryRead_ClaimsEveryAttributedCallSiteForTheInterceptorThatFollows()
    {
        _ = HostedSourceReader.TryRead(InlineReceiverFile, ClassName, out var read);

        await Assert.That(read.Claims["first"]).IsEqualTo("__Intercept_Probe_A");
        await Assert.That(read.Claims["second"]).IsEqualTo("__Intercept_Probe_A");
        await Assert.That(string.Join("\n", read.Members)).DoesNotContain("InterceptsLocation");
    }

    /// <summary>A receiver on the interceptor's own line loses its <c>this</c>, and later lines keep theirs.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryRead_ReceiverOnTheDeclarationLine_LosesThis()
    {
        _ = HostedSourceReader.TryRead(InlineReceiverFile, ClassName, out var read);

        await Assert.That(read.Members[0]).IsEqualTo("internal static int __Intercept_Probe_A(int value) => value;");
        await Assert.That(read.Members[2]).IsEqualTo("private static int Helper(int value) => value;");
    }

    /// <summary>What the file declares after the generated namespace stays at namespace level.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryRead_TextAfterTheNamespace_IsKeptAsTrailing()
    {
        _ = HostedSourceReader.TryRead(InlineReceiverFile, ClassName, out var read);

        await Assert.That(read.Trailing).StartsWith("namespace Other");
    }
}
