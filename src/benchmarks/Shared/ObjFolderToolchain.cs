// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Toolchains.NetCoreApp;

namespace ReactiveUI.SourceGenerators.Benchmarks.Shared;

/// <summary>The .NET 10 csproj toolchain, building each benchmark partition under the benchmark project's <c>obj</c> folder.</summary>
/// <remarks>
/// The stock toolchain builds in a <c>.bdn</c> folder beside the benchmark project and leaves it behind when a run is
/// stopped, so its sources and binaries show up as changes to the repository. Under <c>obj</c> they are ignored by git
/// and left out of the project's default globs.
/// </remarks>
internal sealed class ObjFolderToolchain : CsProjNetToolchain
{
    /// <summary>The settings every partition is built with: the benchmark projects' own target framework.</summary>
    private static readonly NetCoreAppSettings Settings = NetCoreAppSettings.Default with { TargetFrameworkMoniker = "net10.0" };

    /// <summary>Initializes a new instance of the <see cref="ObjFolderToolchain"/> class.</summary>
    private ObjFolderToolchain()
        : base("CsProjCoreObj", CoreRuntime.Core10_0, Settings, new ObjFolderBuilder(Settings), new DotNetCliExecutor(null))
    {
    }

    /// <summary>Gets the toolchain instance.</summary>
    public static ObjFolderToolchain Instance { get; } = new();

    /// <summary>The csproj builder with its build folder moved under the benchmark project's <c>obj</c> folder.</summary>
    /// <param name="settings">The build settings.</param>
    private sealed class ObjFolderBuilder(DotNetCliSettings settings) : CsProjBuilder(settings)
    {
        /// <inheritdoc/>
        protected override string GetBuildArtifactsDirectoryPath(BuildPartition buildPartition, string programName) =>
            Path.Combine(
                GetProjectFilePath(buildPartition.RepresentativeBenchmarkCase.Descriptor.Type, NullLogger.Instance).DirectoryName!,
                "obj",
                "bdn",
                programName);
    }
}
