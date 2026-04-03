// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;

namespace ReactiveUI.SourceGenerator.Tests;

internal static class TestCompilationReferences
{
    /// <summary>
    /// Minimal source stubs for WPF and WinForms types that are only available
    /// via the Microsoft.WindowsDesktop.App shared framework on Windows.
    /// Used in non-Windows test compilations to allow test sources that reference
    /// <c>System.Windows.Window</c> or <c>System.Windows.Forms.UserControl</c>
    /// to compile cross-platform without requiring platform-specific assemblies.
    /// </summary>
    internal const string WindowsDesktopStubs = """
        namespace System.Windows
        {
            public class DependencyObject { }
            public class UIElement : DependencyObject { }
            public class FrameworkElement : UIElement { }
            public class Window : FrameworkElement { }
        }
        namespace System.Windows.Controls
        {
            public class UserControl : System.Windows.FrameworkElement { }
            public class Page : System.Windows.FrameworkElement { }
        }
        namespace System.Windows.Forms
        {
            public class Control { }
            public class Form : Control { }
            public class UserControl : Control { }
        }
        """;

    /// <summary>
    /// Returns metadata references for all assemblies required by the in-memory test compilations.
    /// Uses only runtime assemblies already loaded into the current process — no NuGet downloads,
    /// no Basic.Reference.Assemblies mixing — to avoid CS1704/CS0433/CS0518 duplicate-type errors.
    /// </summary>
    internal static ImmutableArray<MetadataReference> CreateDefault()
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = ImmutableArray.CreateBuilder<MetadataReference>();

        // Seed with the key assemblies whose transitive closure covers BCL + ReactiveUI + Splat +
        // System.Reactive and everything else the test source strings depend on.
        var seeds = new[]
        {
            typeof(object).Assembly,                                                                          // System.Private.CoreLib
            typeof(Enumerable).Assembly,                                                                      // System.Linq
            typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,                                    // System.ObjectModel
            typeof(ReactiveUI.ReactiveObject).Assembly,                                                       // ReactiveUI
            typeof(ReactiveUI.SourceGenerators.ReactiveGenerator).Assembly,                                   // ReactiveUI.SourceGenerators
            typeof(ReactiveUI.SourceGenerators.CodeFixers.PropertyToReactiveFieldAnalyzer).Assembly,          // analyzer assembly
            typeof(Splat.Locator).Assembly,                                                                   // Splat
        };

        foreach (var seed in seeds)
        {
            AddTransitive(seed, visited, result);
        }

        // Also sweep all assemblies already loaded — catches System.Reactive, DynamicData, etc.
        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!loaded.IsDynamic && !string.IsNullOrWhiteSpace(loaded.Location)
                && visited.Add(loaded.Location))
            {
                result.Add(MetadataReference.CreateFromFile(loaded.Location));
            }
        }

        // Add WPF and WinForms assemblies on Windows so test source strings that inherit from
        // Window or use Windows Forms controls compile correctly.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AddWindowsDesktopAssemblies(visited, result);
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Adds WPF (PresentationFramework + dependencies) and Windows Forms assemblies to the
    /// reference set, resolving them from the Microsoft.WindowsDesktop.App shared framework
    /// directory that corresponds to the current runtime version.
    /// </summary>
    private static void AddWindowsDesktopAssemblies(
        HashSet<string> visited,
        ImmutableArray<MetadataReference>.Builder result)
    {
        var versionDir = FindWindowsDesktopAppVersionDir();
        if (versionDir is null)
        {
            return;
        }

        // WPF assemblies required for tests that use Window as a base class.
        var wpfAssemblies = new[]
        {
            "PresentationFramework.dll",
            "PresentationCore.dll",
            "WindowsBase.dll",
            "System.Xaml.dll",
        };

        // WinForms assemblies required for tests that use Windows Forms controls.
        var winFormsAssemblies = new[]
        {
            "System.Windows.Forms.dll",
            "System.Windows.Forms.Primitives.dll",
        };

        foreach (var name in wpfAssemblies.Concat(winFormsAssemblies))
        {
            var path = Path.Combine(versionDir, name);
            if (File.Exists(path) && visited.Add(path))
            {
                result.Add(MetadataReference.CreateFromFile(path));
            }
        }
    }

    /// <summary>
    /// Locates the best matching Microsoft.WindowsDesktop.App version directory.
    /// Uses multiple discovery strategies: runtime-relative path, DOTNET_ROOT env var,
    /// and well-known installation paths.
    /// </summary>
    private static string? FindWindowsDesktopAppVersionDir()
    {
        var runtimeVersion = Environment.Version;
        var majorMinor = $"{runtimeVersion.Major}.{runtimeVersion.Minor}";

        // Collect unique candidate parent directories to try, in priority order.
        var candidateRoots = new List<string?>();

        // Strategy 1a: RuntimeEnvironment.GetRuntimeDirectory() — the most reliable way to
        // locate the actual .NET shared framework even when running under a VS test host
        // or PowerShell where typeof(object).Assembly.Location may point elsewhere.
        // Returns e.g. C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.14\
        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        candidateRoots.Add(Path.GetDirectoryName(runtimeDir?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));

        // Strategy 1b: Walk up from typeof(object).Assembly.Location (works under dotnet CLI).
        var coreLibDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        candidateRoots.Add(coreLibDir);

        // Try each root two levels up to reach shared\Microsoft.WindowsDesktop.App
        foreach (var root in candidateRoots.Where(r => !string.IsNullOrEmpty(r)))
        {
            var candidate = Path.GetFullPath(Path.Combine(root!, "..", "Microsoft.WindowsDesktop.App"));
            var dir = PickBestVersionDir(candidate, majorMinor);
            if (dir is not null)
            {
                return dir;
            }

            // One extra level for layouts where root is already the version directory.
            candidate = Path.GetFullPath(Path.Combine(root!, "..", "..", "Microsoft.WindowsDesktop.App"));
            dir = PickBestVersionDir(candidate, majorMinor);
            if (dir is not null)
            {
                return dir;
            }
        }

        // Strategy 2: DOTNET_ROOT environment variable.
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT")
                      ?? Environment.GetEnvironmentVariable("DOTNET_ROOT(x64)");
        if (!string.IsNullOrEmpty(dotnetRoot))
        {
            var candidate = Path.Combine(dotnetRoot, "shared", "Microsoft.WindowsDesktop.App");
            var dir = PickBestVersionDir(candidate, majorMinor);
            if (dir is not null)
            {
                return dir;
            }
        }

        // Strategy 3: Standard installation paths on Windows.
        foreach (var programFiles in new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        })
        {
            if (string.IsNullOrEmpty(programFiles))
            {
                continue;
            }

            var candidate = Path.Combine(programFiles, "dotnet", "shared", "Microsoft.WindowsDesktop.App");
            var dir = PickBestVersionDir(candidate, majorMinor);
            if (dir is not null)
            {
                return dir;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the best version directory under <paramref name="sharedRoot"/> that matches
    /// <paramref name="majorMinor"/> (e.g., "9.0"), falling back to the newest available.
    /// </summary>
    private static string? PickBestVersionDir(string sharedRoot, string majorMinor)
    {
        if (!Directory.Exists(sharedRoot))
        {
            return null;
        }

        var dirs = Directory.GetDirectories(sharedRoot);
        if (dirs.Length == 0)
        {
            return null;
        }

        // Prefer exact major.minor match, ordered descending (newest patch first).
        var best = dirs
            .Where(d => Path.GetFileName(d).StartsWith(majorMinor + ".", StringComparison.Ordinal)
                     || Path.GetFileName(d).Equals(majorMinor, StringComparison.Ordinal))
            .OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()
            ?? dirs.OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase).FirstOrDefault();

        // Validate it actually contains PresentationFramework.dll
        return best is not null && File.Exists(Path.Combine(best, "PresentationFramework.dll"))
            ? best
            : null;
    }

    private static void AddTransitive(
        Assembly assembly,
        HashSet<string> visited,
        ImmutableArray<MetadataReference>.Builder result)
    {
        if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
        {
            return;
        }

        if (!visited.Add(assembly.Location))
        {
            return;
        }

        result.Add(MetadataReference.CreateFromFile(assembly.Location));

        foreach (var referencedName in assembly.GetReferencedAssemblies())
        {
            try
            {
                var referenced = System.Reflection.Assembly.Load(referencedName);
                AddTransitive(referenced, visited, result);
            }
            catch
            {
                // Best-effort — system assemblies not found in some environments are skipped.
            }
        }
    }
}
