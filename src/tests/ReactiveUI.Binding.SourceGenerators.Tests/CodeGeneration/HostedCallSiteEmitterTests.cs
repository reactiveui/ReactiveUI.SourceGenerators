// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests for <see cref="HostedCallSiteEmitter"/>, which moves call sites into the caller's partial class.</summary>
public class HostedCallSiteEmitterTests
{
    /// <summary>The text the stand-in emitter writes for the call sites that are not hosted.</summary>
    private const string MainFile = "MAIN";

    /// <summary>The data of the hosted call site's location.</summary>
    private const string LocationData = "hosted";

    /// <summary>The name of the class the stand-in emitter writes its members in.</summary>
    private const string ClassName = "Generated";

    /// <summary>The file the stand-in emitter writes for hosted call sites, claiming the hosted location.</summary>
    private const string ClaimingFile = """
                                        namespace Gen
                                        {
                                            internal static partial class Generated
                                            {
                                                [global::System.Runtime.CompilerServices.InterceptsLocation(1, "hosted")]
                                                internal static int __Intercept_Probe_A(
                                                    this global::App.Host.Inner value)
                                                    => 0;
                                            }
                                        }
                                        """;

    /// <summary>The same file, claiming a call site that is not hosted.</summary>
    private static readonly string UnclaimedFile = ClaimingFile.Replace("\"hosted\"", "\"elsewhere\"", StringComparison.Ordinal);

    /// <summary>The features of a build that intercepts, with the stand-in emitter's class name.</summary>
    private static readonly LanguageFeatures Intercepting = new(
        true,
        true,
        true,
        "Gen",
        SupportsInterceptors: true,
        GeneratedClassName: ClassName);

    /// <summary>A hosted call to a method with no type parameters and no constraints.</summary>
    private static readonly HostedCall PlainCall = new(
        "global::App.Host",
        new("App", new(["partial class Host"])),
        new([]),
        new([]),
        "int",
        "int",
        new([new HostedParameter("value", "object", "global::App.Host.Inner")]));

    /// <summary>When the emitter writes nothing for the hosted call sites, the file is the one for the others.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Compose_HostedFileNotWritten_ReturnsTheUnhostedFile() =>
        await Assert.That(Compose(static calls => calls.Any(static c => c.Host is not null) ? null : MainFile)).IsEqualTo(MainFile);

    /// <summary>A hosted file without the generated class has nothing to move, so the file is the one for the others.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Compose_HostedFileWithoutTheClass_ReturnsTheUnhostedFile() =>
        await Assert.That(Compose(static calls => calls.Any(static c => c.Host is not null) ? "namespace Gen\n{\n}\n" : MainFile))
            .IsEqualTo(MainFile);

    /// <summary>A hosted file that claims no call site has nothing to intercept, so the file is the one for the others.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Compose_HostedFileClaimingNothing_ReturnsTheUnhostedFile() =>
        await Assert.That(Compose(static calls => calls.Any(static c => c.Host is not null) ? UnclaimedFile : MainFile))
            .IsEqualTo(MainFile);

    /// <summary>A method with no type parameters gets an interceptor and an entry point with none either.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Compose_MethodWithoutTypeParameters_WritesNoTypeArguments()
    {
        var file = Compose(static calls => calls.Any(static c => c.Host is not null) ? ClaimingFile : MainFile)!;

        await Assert.That(file).StartsWith(MainFile);
        await Assert.That(file).Contains("this object value)");
        await Assert.That(file).Contains("=> __ReactiveUIHostedBindings.__Intercept_Probe_A((global::App.Host.Inner)(object)value);");
        await Assert.That(file).DoesNotContain("<");
    }

    /// <summary>Composes one unhosted and one hosted call site through a stand-in emitter.</summary>
    /// <param name="emit">The stand-in emitter.</param>
    /// <returns>The composed file.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? Compose(Func<ImmutableArray<InvocationInfo>, string?> emit) =>
        HostedCallSiteEmitter.Compose(
            [
                ModelFactory.CreateInvocationInfo(),
                ModelFactory.CreateInvocationInfo() with { Interceptor = new(1, LocationData), Host = PlainCall },
            ],
            Intercepting,
            (calls, _) => emit(calls));
}
