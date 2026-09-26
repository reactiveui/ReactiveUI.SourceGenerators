// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Verifies that an edit reruns only the per-type steps whose models it changed.</summary>
/// <remarks>
/// Each generator groups its models per type and writes each type's file from its own output, so the driver can compare a
/// type's models with the last run's and skip the type when they are equal. The tests run a driver with step tracking,
/// edit one tree, and read the reason each tracked output was produced.
/// </remarks>
public class IncrementalCachingTests
{
    /// <summary>A file no generator reads, edited to show unrelated edits cache every step.</summary>
    private const string UnrelatedSource = "namespace Other { public class Unrelated { public int Value => 1; } }";

    /// <summary>The number of types each test source declares for the generator.</summary>
    private const int TypeCount = 2;

    /// <summary>An unrelated edit leaves every <c>[Reactive]</c> type cached; editing one type reruns only that type.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ReactiveCachesPerType() => AssertCachesPerType(
        new ReactiveGenerator(),
        TrackingNames.ReactiveFieldTypes,
        """
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;
        namespace TestNs;
        public partial class First : ReactiveObject { [Reactive] private int _alpha; }
        public partial class Second : ReactiveObject { [Reactive] private int _beta; }
        """,
        "_alpha;",
        "_gamma;");

    /// <summary>An unrelated edit leaves every <c>[ReactiveCommand]</c> type cached; editing one type reruns only that type.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ReactiveCommandCachesPerType() => AssertCachesPerType(
        new ReactiveCommandGenerator(),
        TrackingNames.ReactiveCommandTypes,
        """
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;
        namespace TestNs;
        public partial class First : ReactiveObject { [ReactiveCommand] private void Save() { } }
        public partial class Second : ReactiveObject { [ReactiveCommand] private void Load() { } }
        """,
        "Save()",
        "Store()");

    /// <summary>An unrelated edit leaves every <c>[ReactiveCollection]</c> type cached; editing one type reruns only that type.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ReactiveCollectionCachesPerType() => AssertCachesPerType(
        new ReactiveCollectionGenerator(),
        TrackingNames.ReactiveCollectionTypes,
        """
        using System.Collections.ObjectModel;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;
        namespace TestNs;
        public partial class First : ReactiveObject { [ReactiveCollection] private ObservableCollection<int> _alpha = []; }
        public partial class Second : ReactiveObject { [ReactiveCollection] private ObservableCollection<int> _beta = []; }
        """,
        "_alpha",
        "_gamma");

    /// <summary>An unrelated edit leaves every <c>[BindableDerivedList]</c> type cached; editing one type reruns only that type.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task BindableDerivedListCachesPerType() => AssertCachesPerType(
        new BindableDerivedListGenerator(),
        TrackingNames.BindableDerivedListTypes,
        """
        using System.Collections.ObjectModel;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;
        namespace TestNs;
        public partial class First : ReactiveObject { [BindableDerivedList] private ReadOnlyObservableCollection<int>? _alpha; }
        public partial class Second : ReactiveObject { [BindableDerivedList] private ReadOnlyObservableCollection<int>? _beta; }
        """,
        "_alpha",
        "_gamma");

    /// <summary>An unrelated edit leaves every <c>[IReactiveObject]</c> type cached; editing one type reruns only that type.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task ReactiveObjectCachesPerType() => AssertCachesPerType(
        new ReactiveObjectGenerator(),
        TrackingNames.ReactiveObjectTypes,
        """
        using ReactiveUI.SourceGenerators;
        namespace TestNs;
        [IReactiveObject] public partial class First { }
        [IReactiveObject] public partial class Second { }
        """,
        "partial class First",
        "partial class Third");

    /// <summary>An unrelated edit leaves every <c>[RoutedControlHost]</c> host cached.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task RoutedControlHostCachesPerType() => AssertCachesPerType(
        new RoutedControlHostGenerator(),
        TrackingNames.RoutedControlHosts,
        """
        using ReactiveUI.SourceGenerators.WinForms;
        namespace TestNs;
        [RoutedControlHost("System.Windows.Forms.UserControl")] public partial class FirstHost { }
        [RoutedControlHost("System.Windows.Forms.UserControl")] public partial class SecondHost { }
        """,
        "partial class FirstHost",
        "partial class ThirdHost");

    /// <summary>Runs a generator, then reruns it after an unrelated edit and after an edit to one of two types.</summary>
    /// <param name="generator">The generator.</param>
    /// <param name="stepName">The tracking name of the generator's per-type step.</param>
    /// <param name="source">Source declaring two types the generator writes for, the first of which is edited.</param>
    /// <param name="find">Text in the first type to change.</param>
    /// <param name="replace">Its replacement, which changes the first type's model.</param>
    /// <returns>A task to monitor the async.</returns>
    private static async Task AssertCachesPerType(IIncrementalGenerator generator, string stepName, string source, string find, string replace)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var generated = CSharpSyntaxTree.ParseText(source, parseOptions, path: "Types.cs");
        var unrelated = CSharpSyntaxTree.ParseText(UnrelatedSource, parseOptions, path: "Unrelated.cs");
        var stubs = CSharpSyntaxTree.ParseText(TestCompilationReferences.WindowsDesktopStubs, parseOptions, path: "Stubs.cs");
        Compilation compilation = CSharpCompilation.Create(
            "IncrementalCaching",
            [generated, unrelated, stubs],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            parseOptions: parseOptions,
            driverOptions: new(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        driver = driver.RunGenerators(compilation);
        await Assert.That(Reasons(driver, stepName).Count).IsEqualTo(TypeCount);

        // An edit to a tree no generator reads: every per-type output is reused as it was.
        var editedUnrelated = Microsoft.CodeAnalysis.Text.SourceText.From(UnrelatedSource.Replace("=> 1", "=> 2", StringComparison.Ordinal));
        var unrelatedEdit = compilation.ReplaceSyntaxTree(unrelated, unrelated.WithChangedText(editedUnrelated));
        var afterUnrelated = Reasons(driver.RunGenerators(unrelatedEdit), stepName);
        await Assert.That(CountReused(afterUnrelated)).IsEqualTo(TypeCount);

        // An edit that changes the first type's model: the second type's output is still reused.
        var relatedEdit = compilation.ReplaceSyntaxTree(generated, generated.WithChangedText(Microsoft.CodeAnalysis.Text.SourceText.From(source.Replace(find, replace, StringComparison.Ordinal))));
        var afterRelated = Reasons(driver.RunGenerators(relatedEdit), stepName);
        await Assert.That(CountReused(afterRelated)).IsEqualTo(TypeCount - 1);
    }

    /// <summary>Counts the outputs the driver reused rather than produced again.</summary>
    /// <param name="reasons">The reasons each output was produced.</param>
    /// <returns>The number that were cached or unchanged.</returns>
    private static int CountReused(List<IncrementalStepRunReason> reasons)
    {
        var reused = 0;
        foreach (var reason in reasons)
        {
            if (reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged)
            {
                reused++;
            }
        }

        return reused;
    }

    /// <summary>Reads the reason each output of a tracked step was produced in the driver's last run.</summary>
    /// <param name="driver">The driver.</param>
    /// <param name="stepName">The step's tracking name.</param>
    /// <returns>One reason per output.</returns>
    private static List<IncrementalStepRunReason> Reasons(GeneratorDriver driver, string stepName)
    {
        var reasons = new List<IncrementalStepRunReason>();
        foreach (var step in driver.GetRunResult().Results[0].TrackedSteps[stepName])
        {
            foreach (var (_, reason) in step.Outputs)
            {
                reasons.Add(reason);
            }
        }

        return reasons;
    }
}
