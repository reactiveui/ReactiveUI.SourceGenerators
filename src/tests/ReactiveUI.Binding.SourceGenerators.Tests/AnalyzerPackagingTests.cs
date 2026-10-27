// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Tests the analyzer layout of the runtime packages.</summary>
/// <remarks>
/// What the generator can do differs between compiler generations, so each runtime package ships one
/// <c>analyzers/dotnet/roslyn&lt;version&gt;/cs</c> slot per generation. Only the .NET SDK narrows that down to
/// one: its <c>ResolvePackageAssets</c> task picks the highest slot at or below <c>$(CompilerApiVersion)</c>. A
/// build that never runs that task receives every slot at once, loads the generator twice and emits every
/// dispatch file twice, which the shipped targets prevent by dropping the slots the compiler is not served by.
/// The versions in the layout and in those targets therefore have to agree, which is asserted here rather than
/// left to a convention a later change can quietly break.
/// </remarks>
public class AnalyzerPackagingTests
{
    /// <summary>The property in the shipped targets naming the oldest slot.</summary>
    private const string MinimumCompilerVersionProperty = "_ReactiveUIBindingMinimumCompilerVersion";

    /// <summary>The property in the shipped targets naming the slot that can intercept.</summary>
    private const string InterceptorCompilerVersionProperty = "_ReactiveUIBindingInterceptorCompilerVersion";

    /// <summary>The shipped targets, relative to the source root.</summary>
    private const string TargetsPath =
        "ReactiveUI.Binding.SourceGenerators/build/ReactiveUI.Binding.SourceGenerators.targets";

    /// <summary>The separators a project spells a path with, whichever platform reads it.</summary>
    private static readonly SearchValues<char> PathSeparators = SearchValues.Create(['\\', '/']);

    /// <summary>The assemblies every slot has to carry.</summary>
    private static readonly string[] SlotAssemblies =
    [
        "ReactiveUI.Binding.Analyzer.dll",
        "ReactiveUI.Binding.SourceGenerators.dll",
    ];

    /// <summary>The runtime packages that carry the analyzers.</summary>
    private static readonly string[] RuntimePackages =
    [
        "ReactiveUI.Binding/ReactiveUI.Binding.csproj",
        "ReactiveUI.Binding.Reactive/ReactiveUI.Binding.Reactive.csproj",
    ];

    /// <summary>Every package ships the same slots, and the versions the targets name are exactly those.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PackagesShipTheSlotsTheTargetsSelectBetween()
    {
        var expected = string.Join(
            ", ",
            new[]
            {
                $"analyzers/dotnet/roslyn{ReadTargetsProperty(MinimumCompilerVersionProperty)}/cs",
                $"analyzers/dotnet/roslyn{ReadTargetsProperty(InterceptorCompilerVersionProperty)}/cs",
            }.Order(StringComparer.OrdinalIgnoreCase));

        foreach (var package in RuntimePackages)
        {
            var slots = PackagedAnalyzers(package)
                .Select(static analyzer => analyzer.PackagePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase);

            // Joined rather than compared element-wise so a stray extra slot shows up in the diff.
            await Assert.That(string.Join(", ", slots)).IsEqualTo(expected);
        }
    }

    /// <summary>A slot missing an assembly would leave that compiler generation without it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EverySlotCarriesTheGeneratorAndTheAnalyzer()
    {
        foreach (var package in RuntimePackages)
        {
            foreach (var slot in PackagedAnalyzers(package).GroupBy(static analyzer => analyzer.PackagePath, StringComparer.OrdinalIgnoreCase))
            {
                var assemblies = slot.Select(static analyzer => analyzer.FileName)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                await Assert.That(assemblies).IsEquivalentTo(SlotAssemblies);
            }
        }
    }

    /// <summary>Two copies of one assembly in a package load as two generators, whichever folder they sit in.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoAssemblyIsShippedTwiceWithinASlot()
    {
        foreach (var package in RuntimePackages)
        {
            var duplicated = PackagedAnalyzers(package)
                .GroupBy(static analyzer => $"{analyzer.PackagePath}/{analyzer.FileName}", StringComparer.OrdinalIgnoreCase)
                .Where(static group => group.Count() > 1)
                .Select(static group => group.Key)
                .ToArray();

            await Assert.That(duplicated).IsEmpty();
        }
    }

    /// <summary>The targets and the props both travel with every package that ships a slot.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EveryPackageShippingASlotAlsoShipsTheTargetsThatSelectIt()
    {
        foreach (var package in RuntimePackages)
        {
            var packed = PackedNoneItems(package)
                .Select(static item => FileNameOf(item.FileName))
                .ToArray();

            await Assert.That(packed).Contains("ReactiveUI.Binding.SourceGenerators.targets");
            await Assert.That(packed).Contains("ReactiveUI.Binding.SourceGenerators.props");
        }
    }

    /// <summary>Reads a property value out of the shipped targets.</summary>
    /// <param name="propertyName">The property to read.</param>
    /// <returns>The declared value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ReadTargetsProperty(string propertyName) =>
        XDocument.Load(SourcePath(TargetsPath))
            .Descendants()
            .Where(element => element.Name.LocalName == propertyName)
            .Select(static element => element.Value.Trim())
            .Single();

    /// <summary>Reads the analyzer files a package packs, and the slot each lands in.</summary>
    /// <param name="projectPath">The package project, relative to the source root.</param>
    /// <returns>Each packed file, as the package path it lands in and the file it was packed from.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<(string PackagePath, string FileName)> PackedNoneItems(string projectPath) =>
        XDocument.Load(SourcePath(projectPath))
            .Descendants()
            .Where(static element => element.Name.LocalName == "None"
                && string.Equals(element.Attribute("Pack")?.Value, "true", StringComparison.OrdinalIgnoreCase))
            .Select(static element => (
                PackagePath: element.Attribute("PackagePath")?.Value ?? string.Empty,
                FileName: element.Attribute("Include")?.Value ?? string.Empty));

    /// <summary>Reads the analyzer assemblies a package packs into a slot.</summary>
    /// <param name="projectPath">The package project, relative to the source root.</param>
    /// <returns>Each packed analyzer, as the slot it lands in and the file name it lands under.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<(string PackagePath, string FileName)> PackagedAnalyzers(string projectPath) =>
        PackedNoneItems(projectPath)
            .Where(static item => item.PackagePath.StartsWith("analyzers/", StringComparison.OrdinalIgnoreCase))
            .Select(static item => (item.PackagePath, FileNameOf(item.FileName)));

    /// <summary>
    /// Reads the file name off a path a project wrote, which spells its separators the way MSBuild does rather
    /// than the way the running platform does.
    /// </summary>
    /// <param name="path">The path as the project spells it.</param>
    /// <returns>The file name.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FileNameOf(string path) =>
        path[(path.AsSpan().LastIndexOfAny(PathSeparators) + 1)..];

    /// <summary>Resolves a path under the source root, found by walking out of the test output directory.</summary>
    /// <param name="relativePath">The path relative to the source root.</param>
    /// <returns>The absolute path.</returns>
    /// <exception cref="InvalidOperationException">The source root was not found above the test output.</exception>
    private static string SourcePath(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"'{relativePath}' was not found above '{AppContext.BaseDirectory}'.");
    }
}
