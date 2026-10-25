// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.Binding.SourceGenerators.Generators;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers the fields declared for the controls a MAUI or Avalonia page names in XAML. Those platforms' generators write the
/// fields, which no other generator sees, so the binding generator reads the page and declares them for call sites.
/// </summary>
public class XamlDeclarationTests
{
    /// <summary>What the probe writes when nothing is declared.</summary>
    private const string None = "// none";

    /// <summary>The metadata MAUI marks its XAML with.</summary>
    private const string MauiKind = "build_metadata.AdditionalFiles.GenKind";

    /// <summary>The metadata Avalonia marks its XAML with.</summary>
    private const string AvaloniaGroup = "build_metadata.AdditionalFiles.SourceItemGroup";

    /// <summary>The item group Avalonia puts its XAML in.</summary>
    private const string AvaloniaXaml = "AvaloniaXaml";

    /// <summary>The code-behind class, and types for elements to resolve to.</summary>
    private const string CodeBehind = """
        [assembly: Maui.XmlnsDefinition("urn:maui", "Maui.Missing")]
        [assembly: Maui.XmlnsDefinition("urn:maui", "Maui.Controls")]
        [assembly: Maui.XmlnsDefinition(1, 2)]
        [assembly: Maui.XmlnsDefinitionAttribute("urn:only")]
        [assembly: Other.Marker("urn:maui", "Wrong.Namespace")]

        namespace Maui
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class XmlnsDefinitionAttribute : System.Attribute
            {
                public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace) { }

                public XmlnsDefinitionAttribute(int xmlNamespace, int clrNamespace) { }

                public XmlnsDefinitionAttribute(string xmlNamespace) { }
            }
        }

        namespace Other
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class Marker : System.Attribute
            {
                public Marker(string a, string b) { }
            }
        }

        namespace Maui.Controls
        {
            public class Entry { }

            public class Button { }
        }

        namespace Local
        {
            public class Gauge { }
        }

        namespace App
        {
            public partial class LoginPage
            {
                private Maui.Controls.Button Declared;
            }

            public class Sealed { }
        }
        """;

    /// <summary>A MAUI page: each named control becomes a field of the class, with the accessibility it names.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MauiPage_DeclaresAFieldPerNamedControl()
    {
        const string page = """
            <ContentPage xmlns="urn:maui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:local="clr-namespace:Local;assembly=App" x:Class="App.LoginPage">
                <Grid>
                    <Grid.Children>
                        <Entry x:Name="UserName" x:FieldModifier="internal" />
                        <Entry x:Name="Password" />
                        <local:Gauge x:Name="Strength" x:FieldModifier="Public" />
                        <Button x:Name="Declared" />
                        <Button Name="NotAName" />
                        <Unknown x:Name="Missing" />
                        <Entry x:Name=" " />
                        <List x:Name="Generic" x:TypeArguments="x:String" />
                        <DataTemplate><Entry x:Name="InTemplate" /></DataTemplate>
                    </Grid.Children>
                </Grid>
            </ContentPage>
            """;

        var declarations = Declarations(page, (MauiKind, "Xaml"));

        await Assert.That(declarations).Contains("internal global::Maui.Controls.Entry UserName;");
        await Assert.That(declarations).Contains("private global::Maui.Controls.Entry Password;");
        await Assert.That(declarations).Contains("public global::Local.Gauge Strength;");
        await Assert.That(declarations).DoesNotContain("Declared");
        await Assert.That(declarations).DoesNotContain("NotAName");
        await Assert.That(declarations).DoesNotContain("Missing");
        await Assert.That(declarations).DoesNotContain("Generic");
        await Assert.That(declarations).DoesNotContain("InTemplate");
    }

    /// <summary>An Avalonia view: a plain <c>Name</c> names a control too, and fields default to the build's modifier.</summary>
    /// <param name="modifier">The build's default field modifier, or null when it sets none.</param>
    /// <param name="accessibility">The accessibility the field is declared with.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(null, "internal")]
    [Arguments("public", "public")]
    public async Task AvaloniaView_DeclaresNamedControls(string? modifier, string accessibility)
    {
        const string page = """
            <UserControl xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:c="using:Maui.Controls" x:Class="App.LoginPage">
                <c:Entry Name="UserName" />
                <c:Entry />
            </UserControl>
            """;

        var global = modifier is null ? [] : new[] { ("build_property.AvaloniaNameGeneratorDefaultFieldModifier", modifier) };

        await Assert.That(Declarations(page, [(AvaloniaGroup, AvaloniaXaml)], global))
            .Contains($"{accessibility} global::Maui.Controls.Entry UserName;");
    }

    /// <summary>Files no XAML generator reads, and pages with nothing to declare, declare nothing.</summary>
    /// <param name="page">The page.</param>
    /// <param name="key">The metadata the file carries.</param>
    /// <param name="value">The metadata's value.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("<Page xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.LoginPage\"><Entry x:Name=\"A\" /></Page>", MauiKind, "Css")]
    [Arguments("<Page xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.LoginPage\"><Entry x:Name=\"A\" /></Page>", AvaloniaGroup, "AvaloniaResource")]
    [Arguments("<Page xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.LoginPage\"", MauiKind, "Xaml")]
    [Arguments("<Page xmlns=\"urn:maui\"><Entry xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Name=\"A\" /></Page>", MauiKind, "Xaml")]
    [Arguments("<Page xmlns=\"urn:maui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.Nowhere\"><Entry x:Name=\"A\" /></Page>", MauiKind, "Xaml")]
    [Arguments("<Page xmlns=\"urn:maui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.Sealed\"><Entry x:Name=\"A\" /></Page>", MauiKind, "Xaml")]
    [Arguments("<Page xmlns=\"urn:other\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.LoginPage\"><Entry x:Name=\"A\" /></Page>", MauiKind, "Xaml")]
    [Arguments("<Page xmlns=\"urn:only\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.LoginPage\"><Entry x:Name=\"A\" /></Page>", MauiKind, "Xaml")]
    public async Task NothingToDeclare_DeclaresNothing(string page, string key, string value) =>
        await Assert.That(Declarations(page, (key, value))).IsEqualTo(None);

    /// <summary>Avalonia's name generator can be turned off, and then declares no fields.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaGeneratorTurnedOff_DeclaresNothing() =>
        await Assert.That(Declarations(
                "<UserControl xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:c=\"using:Maui.Controls\" x:Class=\"App.LoginPage\"><c:Entry Name=\"A\" /></UserControl>",
                [(AvaloniaGroup, AvaloniaXaml)],
                [("build_property.AvaloniaNameGeneratorIsEnabled", "false")]))
            .IsEqualTo(None);

    /// <summary>A file whose text cannot be read declares nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnreadableFile_DeclaresNothing() =>
        await Assert.That(Declarations(null, [(MauiKind, "Xaml")], [])).IsEqualTo(None);

    /// <summary>Every <c>x:FieldModifier</c> spelling maps to the accessibility it names.</summary>
    /// <param name="modifier">The modifier.</param>
    /// <param name="accessibility">The accessibility.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Public", "public")]
    [Arguments("internal", "internal")]
    [Arguments("NotPublic", "internal")]
    [Arguments("Assembly", "internal")]
    [Arguments("Protected", "protected")]
    [Arguments("Family", "protected")]
    [Arguments("ProtectedInternal", "protected internal")]
    [Arguments("protected internal", "protected internal")]
    [Arguments("FamOrAssem", "protected internal")]
    [Arguments("PrivateProtected", "private protected")]
    [Arguments("private protected", "private protected")]
    [Arguments("FamAndAssem", "private protected")]
    [Arguments(" Private ", "private")]
    [Arguments("Unknown", "fallback")]
    [Arguments(null, "fallback")]
    public async Task FieldModifier_MapsToAccessibility(string? modifier, string accessibility) =>
        await Assert.That(XamlPageReader.Accessibility(modifier, "fallback")).IsEqualTo(accessibility);

    /// <summary>Avalonia's name generator, left on explicitly, still declares fields.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaGeneratorTurnedOn_DeclaresFields() =>
        await Assert.That(Declarations(
                "<UserControl xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:c=\"using:Maui.Controls\" x:Class=\"App.LoginPage\"><c:Entry Name=\"A\" /></UserControl>",
                [(AvaloniaGroup, AvaloniaXaml)],
                [("build_property.AvaloniaNameGeneratorIsEnabled", "true")]))
            .Contains("internal global::Maui.Controls.Entry A;");

    /// <summary>An attribute whose class could not be read is not an <c>XmlnsDefinition</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnreadAttributeClass_IsNotAnXmlnsDefinition() =>
        await Assert.That(XamlMemberResolver.IsXmlnsDefinition(null)).IsFalse();

    /// <summary>A compilation with no XAML pages resolves nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoPages_ResolveNothing() =>
        await Assert.That(XamlMemberResolver.Resolve([], CSharpCompilation.Create("Empty"), CancellationToken.None)).IsEmpty();

    /// <summary>Runs the declarations pipeline over the code-behind and one page, and returns the declarations it wrote.</summary>
    /// <param name="page">The page's text.</param>
    /// <param name="metadata">The metadata the page's file carries.</param>
    /// <returns>The declarations, or <see cref="None"/> when the pipeline wrote none.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Declarations(string page, (string Key, string Value) metadata) => Declarations(page, [metadata], []);

    /// <summary>Runs the declarations pipeline over the code-behind and one page, and returns the declarations it wrote.</summary>
    /// <param name="page">The page's text, or null for a file that cannot be read.</param>
    /// <param name="metadata">The metadata the page's file carries.</param>
    /// <param name="global">The build's properties.</param>
    /// <returns>The declarations, or <see cref="None"/> when the pipeline wrote none.</returns>
    private static string Declarations(string? page, (string Key, string Value)[] metadata, (string Key, string Value)[] global)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "App",
            [CSharpSyntaxTree.ParseText(CodeBehind, parseOptions)],
            Basic.Reference.Assemblies.Net100.References.All,
            new(OutputKind.DynamicallyLinkedLibrary));

        var file = new PageFile(page);
        var driver = CSharpGeneratorDriver.Create(
                [new DeclarationsProbe().AsSourceGenerator()],
                [file],
                parseOptions,
                new Options(file, metadata, global))
            .RunGenerators(compilation);

        return driver.GetRunResult().Results[0].GeneratedSources[0].SourceText.ToString();
    }

    /// <summary>A page file whose text the test supplies.</summary>
    /// <param name="text">The text, or null for a file that cannot be read.</param>
    private sealed class PageFile(string? text) : AdditionalText
    {
        /// <summary>The path the page is read from.</summary>
        private const string PagePath = "/App/LoginPage.xaml";

        /// <inheritdoc/>
        public override string Path => PagePath;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SourceText? GetText(CancellationToken cancellationToken = default) =>
            text is null ? null : SourceText.From(text);
    }

    /// <summary>Answers the page's metadata and the build's properties.</summary>
    /// <param name="file">The page.</param>
    /// <param name="metadata">The page's metadata.</param>
    /// <param name="global">The build's properties.</param>
    private sealed class Options(AdditionalText file, (string Key, string Value)[] metadata, (string Key, string Value)[] global) : AnalyzerConfigOptionsProvider
    {
        /// <inheritdoc/>
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Values(global);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new Values([]);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            new Values(ReferenceEquals(textFile, file) ? metadata : []);
    }

    /// <summary>A set of analyzer config values.</summary>
    /// <param name="values">The values.</param>
    private sealed class Values((string Key, string Value)[] values) : AnalyzerConfigOptions
    {
        /// <inheritdoc/>
        public override bool TryGetValue(string key, out string value)
        {
            foreach (var (candidate, found) in values)
            {
                if (!string.Equals(candidate, key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = found;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }

    /// <summary>Writes the declarations the pipeline adds to the compilation, as a generated file the test can read.</summary>
    private sealed class DeclarationsProbe : IIncrementalGenerator
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterSourceOutput(
                SourceGeneratorsCompilation.Register(in context),
                static (output, compilation) => output.AddSource(
                    "Declarations.g.cs",
                    compilation is null ? None : compilation.SyntaxTrees.Last().ToString()));
    }
}
