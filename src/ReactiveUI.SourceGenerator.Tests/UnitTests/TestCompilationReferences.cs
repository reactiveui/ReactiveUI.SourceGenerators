// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Creates metadata-reference sets for in-memory test compilations.</summary>
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
            public class DependencyProperty
            {
                public static DependencyProperty Register(string name, global::System.Type propertyType, global::System.Type ownerType, PropertyMetadata typeMetadata) => null!;
            }
            public class PropertyMetadata
            {
                public PropertyMetadata(object? defaultValue) { }
            }
            public class DependencyObject
            {
                public object GetValue(DependencyProperty dp) => null!;
                public void SetValue(DependencyProperty dp, object value) { }
            }
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
            public enum DockStyle
            {
                None,
                Fill,
            }

            public class Control : global::System.ComponentModel.Component
            {
                public ControlCollection Controls { get; } = new();
                public DockStyle Dock { get; set; }
                public void SuspendLayout() { }
                public void ResumeLayout() { }
            }

            public sealed class ControlCollection : global::System.Collections.Generic.IEnumerable<Control>
            {
                private readonly global::System.Collections.Generic.List<Control> controls = new();

                public int Count => controls.Count;
                public void Add(Control control) => controls.Add(control);
                public void Clear() => controls.Clear();
                public void Remove(Control? control)
                {
                    if (control is not null)
                    {
                        controls.Remove(control);
                    }
                }

                public global::System.Collections.Generic.IEnumerator<Control> GetEnumerator() => controls.GetEnumerator();
                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public class Form : Control { }
            public class UserControl : Control { }
        }
        """;

    /// <summary>The shared-framework directory name for Windows desktop assemblies.</summary>
    private const string WindowsDesktopAppDirectoryName = "Microsoft.WindowsDesktop.App";

    /// <summary>
    /// Cache the default references so that the expensive assembly-scanning/file-I/O is only
    /// performed once per process, not on every test invocation.
    /// </summary>
    private static readonly Lazy<ImmutableArray<MetadataReference>> defaultReferences =
        new(
            static () => CreateDefaultCore(includeWindowsDesktop: true),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Cache the platform-neutral references used with source stubs for deterministic
    /// Windows desktop generator tests on every operating system.
    /// </summary>
    private static readonly Lazy<ImmutableArray<MetadataReference>> portableDefaultReferences =
        new(
            static () => CreateDefaultCore(includeWindowsDesktop: false),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Returns metadata references for all assemblies required by the in-memory test compilations.
    /// Uses an explicit transitive closure rooted at the assemblies required by the test sources.
    /// It deliberately does not sweep all loaded assemblies, so compatibility tests can model a
    /// ReactiveUI 24 base application without accidentally importing System.Reactive.
    /// </summary>
    /// <returns>The cached default metadata-reference closure.</returns>
    internal static ImmutableArray<MetadataReference> CreateDefault() => defaultReferences.Value;

    /// <summary>
    /// Returns the default metadata-reference closure without platform-specific Windows
    /// desktop assemblies so source stubs can be used consistently on every operating system.
    /// </summary>
    /// <returns>The cached platform-neutral metadata-reference closure.</returns>
    internal static ImmutableArray<MetadataReference> CreatePortableDefault() => portableDefaultReferences.Value;

    /// <summary>Creates an isolated transitive metadata-reference closure from the supplied assembly roots.</summary>
    /// <param name="assemblies">The assemblies whose dependency closures should be included.</param>
    /// <returns>The isolated metadata references.</returns>
    internal static ImmutableArray<MetadataReference> CreateForAssemblies(params Assembly[] assemblies)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var assembly in assemblies)
        {
            AddTransitive(assembly, visited, result);
        }

        return result.ToImmutable();
    }

    /// <summary>Builds the default metadata-reference closure.</summary>
    /// <returns>The default metadata-reference closure.</returns>
    /// <param name="includeWindowsDesktop">
    /// Whether to add installed Windows desktop shared-framework assemblies on Windows.
    /// </param>
    private static ImmutableArray<MetadataReference> CreateDefaultCore(bool includeWindowsDesktop)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = ImmutableArray.CreateBuilder<MetadataReference>();

        // Seed with the key assemblies whose transitive closure covers the dependencies used by
        // the existing generator snapshots. Profile-specific tests supply their own references.
        var seeds = new[]
        {
            typeof(object).Assembly, // System.Private.CoreLib
            typeof(Enumerable).Assembly, // System.Linq
            typeof(System.ComponentModel.INotifyPropertyChanged).Assembly, // System.ObjectModel
            typeof(ReactiveObject).Assembly, // ReactiveUI
            typeof(System.Reactive.Unit).Assembly, // System.Reactive test inputs
            typeof(DynamicData.SourceList<>).Assembly, // Bindable derived list test inputs
            typeof(ReactiveGenerator).Assembly, // ReactiveUI.SourceGenerators
            typeof(PropertyToReactiveFieldAnalyzer).Assembly, // analyzer assembly
            typeof(Splat.Locator).Assembly, // Splat
        };

        foreach (var seed in seeds)
        {
            AddTransitive(seed, visited, result);
        }

        // Add WPF and WinForms assemblies on Windows so test source strings that inherit from
        // Window or use Windows Forms controls compile correctly.
        if (includeWindowsDesktop && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
    /// <param name="visited">The set of metadata-reference paths already added.</param>
    /// <param name="result">The metadata-reference collection to populate.</param>
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
            "System.Private.Windows.Core.dll",
        };

        // WinForms assemblies required for tests that use Windows Forms controls.
        var winFormsAssemblies = new[]
        {
            "System.Windows.Forms.dll",
            "System.Windows.Forms.Primitives.dll",
        };

        AddWindowsDesktopAssemblies(versionDir, wpfAssemblies, visited, result);
        AddWindowsDesktopAssemblies(versionDir, winFormsAssemblies, visited, result);
    }

    /// <summary>Adds Windows desktop assembly references from a shared-framework directory.</summary>
    /// <param name="versionDir">The Windows desktop shared-framework version directory.</param>
    /// <param name="assemblyNames">The assembly file names to add.</param>
    /// <param name="visited">The set of metadata-reference paths already added.</param>
    /// <param name="result">The metadata-reference collection to populate.</param>
    private static void AddWindowsDesktopAssemblies(
        string versionDir,
        IEnumerable<string> assemblyNames,
        HashSet<string> visited,
        ImmutableArray<MetadataReference>.Builder result)
    {
        foreach (var name in assemblyNames)
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
    /// <returns>The directory that contains the best matching Windows desktop reference assemblies, if found.</returns>
    private static string? FindWindowsDesktopAppVersionDir()
    {
        var runtimeVersion = Environment.Version;
        var majorMinor = $"{runtimeVersion.Major}.{runtimeVersion.Minor}";

        var runtimeDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var candidateRoots = new[]
        {
            Path.GetDirectoryName(runtimeDirectory?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            Path.GetDirectoryName(typeof(object).Assembly.Location),
        };
        var runtimeRelativeDirectory = FindWindowsDesktopAppVersionDir(candidateRoots, majorMinor);
        if (runtimeRelativeDirectory is not null)
        {
            return runtimeRelativeDirectory;
        }

        // Strategy 2: DOTNET_ROOT environment variable.
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT")
                      ?? Environment.GetEnvironmentVariable("DOTNET_ROOT(x64)");
        if (!string.IsNullOrEmpty(dotnetRoot))
        {
            var candidate = Path.Combine(dotnetRoot, "shared", WindowsDesktopAppDirectoryName);
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

            var candidate = Path.Combine(programFiles, "dotnet", "shared", WindowsDesktopAppDirectoryName);
            var dir = PickBestVersionDir(candidate, majorMinor);
            if (dir is not null)
            {
                return dir;
            }
        }

        return null;
    }

    /// <summary>Searches candidate runtime roots for the best Windows desktop version directory.</summary>
    /// <param name="candidateRoots">The runtime-relative candidate roots to inspect.</param>
    /// <param name="majorMinor">The runtime major and minor version to prefer.</param>
    /// <returns>The best matching Windows desktop directory, if found.</returns>
    private static string? FindWindowsDesktopAppVersionDir(IEnumerable<string?> candidateRoots, string majorMinor)
    {
        foreach (var root in candidateRoots)
        {
            if (string.IsNullOrEmpty(root))
            {
                continue;
            }

            var candidate = Path.GetFullPath(Path.Combine(root, "..", WindowsDesktopAppDirectoryName));
            var directory = PickBestVersionDir(candidate, majorMinor);
            if (directory is not null)
            {
                return directory;
            }

            candidate = Path.GetFullPath(Path.Combine(root, "..", "..", WindowsDesktopAppDirectoryName));
            directory = PickBestVersionDir(candidate, majorMinor);
            if (directory is not null)
            {
                return directory;
            }
        }

        return null;
    }

    /// <summary>Returns the best version directory under <paramref name="sharedRoot"/> that matches <paramref name="majorMinor"/> (e.g., "9.0"), falling back to the newest available.</summary>
    /// <param name="sharedRoot">The Windows desktop shared-framework directory.</param>
    /// <param name="majorMinor">The runtime major and minor version to prefer.</param>
    /// <returns>The best matching version directory, if it contains WPF assemblies.</returns>
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

        Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
        Array.Reverse(dirs);
        var best = dirs[0];
        foreach (var directory in dirs)
        {
            var version = Path.GetFileName(directory);
            if (version.StartsWith($"{majorMinor}.", StringComparison.Ordinal) || version.Equals(majorMinor, StringComparison.Ordinal))
            {
                best = directory;
                break;
            }
        }

        // Validate it actually contains PresentationFramework.dll
        return File.Exists(Path.Combine(best, "PresentationFramework.dll"))
            ? best
            : null;
    }

    /// <summary>Adds an assembly and its loadable transitive references to the collection.</summary>
    /// <param name="assembly">The assembly whose references should be added.</param>
    /// <param name="visited">The set of assembly paths that have already been added.</param>
    /// <param name="result">The metadata-reference collection to populate.</param>
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
            catch (FileNotFoundException)
            {
                // Best-effort — system assemblies not found in some environments are skipped.
            }
            catch (FileLoadException)
            {
                // Best-effort — system assemblies not found in some environments are skipped.
            }
            catch (BadImageFormatException)
            {
                // Best-effort — system assemblies not found in some environments are skipped.
            }
        }
    }
}
