// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>
/// Generates the AOT-safe view dispatch code for <see cref="ViewRegistrationInfo"/> entries.
/// Emits a type-switch function that resolves views without reflection.
/// </summary>
internal static class ViewLocatorDispatchGenerator
{
    /// <summary>Opens the test that narrows a resolved instance to a view type.</summary>
    private const string InstanceTypeTestOpen = "if (instance is ";

    /// <summary>Closes a call that passes the requested contract through to a resolver.</summary>
    private const string ContractResolverCall = "(contract);";

    /// <summary>
    /// The test that limits a view with no contract to a request with no contract. A request under a contract no
    /// generated view claims falls through, so the locator tries its mappings and the service locator with that contract.
    /// </summary>
    private const string DefaultContractTest = "contract.Length == 0";

    /// <summary>The identifier prefix used for the per-view resolver methods emitted in the generated source.</summary>
    private const string ResolverMethodNamePrefix = "__ResolveView_";

    /// <summary>
    /// Registers the view locator dispatch pipeline into the incremental generator.
    /// Scans for classes implementing <c>IViewFor&lt;T&gt;</c> and generates a dispatch method.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<LanguageFeatures> languageFeatures)
    {
        var viewRegistrations = context.SyntaxProvider
            .CreateSyntaxProvider(
                RoslynHelpers.IsClassWithBaseList,
                ViewRegistrationExtractor.ExtractFromIViewForImplementation)
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        // ReactiveUI.SourceGenerators adds IViewFor<T> in output this generator cannot see, so its attribute is read too.
        Func<SyntaxNode, CancellationToken, bool> isClass = static (node, _) => node is ClassDeclarationSyntax;
        Func<GeneratorAttributeSyntaxContext, CancellationToken, ViewRegistrationInfo?> fromAttribute = ViewRegistrationExtractor.ExtractFromIViewForAttribute;
        var genericAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(Constants.SourceGeneratorsGenericIViewForAttributeMetadataName, isClass, fromAttribute)
            .Collect();
        var namedAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(Constants.SourceGeneratorsIViewForAttributeMetadataName, isClass, fromAttribute)
            .Collect();

        // Views found through the interface come first, so deduplication keeps them over the attribute's.
        context.RegisterSourceOutput(
            viewRegistrations.Collect().Combine(genericAttribute).Combine(namedAttribute).Combine(languageFeatures),
            static (ctx, data) => Generate(ctx, Concatenate(data.Left.Left.Left, data.Left.Left.Right, data.Left.Right), data.Right));
    }

    /// <summary>Joins the registrations each pipeline found, interface registrations first.</summary>
    /// <param name="fromInterfaces">The views found through <c>IViewFor&lt;T&gt;</c>.</param>
    /// <param name="fromGenericAttribute">The views found through <c>[IViewFor&lt;T&gt;]</c>, with nulls for declined ones.</param>
    /// <param name="fromNamedAttribute">The views found through <c>[IViewFor("...")]</c>, with nulls for declined ones.</param>
    /// <returns>Every registration, in that order.</returns>
    internal static ImmutableArray<ViewRegistrationInfo> Concatenate(
        ImmutableArray<ViewRegistrationInfo> fromInterfaces,
        ImmutableArray<ViewRegistrationInfo?> fromGenericAttribute,
        ImmutableArray<ViewRegistrationInfo?> fromNamedAttribute)
    {
        if (fromGenericAttribute.IsEmpty && fromNamedAttribute.IsEmpty)
        {
            return fromInterfaces;
        }

        var all = ImmutableArray.CreateBuilder<ViewRegistrationInfo>(fromInterfaces.Length + fromGenericAttribute.Length + fromNamedAttribute.Length);
        all.AddRange(fromInterfaces);
        AddFound(all, fromGenericAttribute);
        AddFound(all, fromNamedAttribute);
        return all.ToImmutable();
    }

    /// <summary>Generates the ViewDispatch.g.cs source file from collected view registrations.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="registrations">All detected view registration infos.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    internal static void Generate(in SourceProductionContext context, ImmutableArray<ViewRegistrationInfo> registrations, in LanguageFeatures features)
    {
        if (!ExtractorValidation.HasItems(registrations))
        {
            return;
        }

        // Deduplicate by (ViewModel FQN, Contract) pair — first occurrence wins
        var deduplicated = Deduplicate(registrations);

        const int DispatchPreambleCapacity = 2_048;
        const int PerRegistrationCapacity = 512;

        var sb = SourceWriter.Rent(DispatchPreambleCapacity + (deduplicated.Count * PerRegistrationCapacity));
        GenerateSource(sb, deduplicated, features);
        CodeGeneratorHelpers.AddGeneratedSource(
            context,
            "ViewDispatch.g.cs",
            sb.ToStringAndReturn(),
            features);
    }

    /// <summary>Generates the full source output into the writer.</summary>
    /// <param name="sb">The writer, at the start of the file.</param>
    /// <param name="registrations">The deduplicated registrations.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    internal static void GenerateSource(SourceWriter sb, List<ViewRegistrationInfo> registrations, in LanguageFeatures features)
    {
        var supportsNullable = features.SupportsNullable;
        CodeGeneratorHelpers.OpenGeneratedClass(
            sb.FileHeader(features.EmitGeneratedCodeMarkers, supportsNullable)
                .BlankLine()
                .OpenNamespace(features.GeneratedNamespace),
            features);

        // Singleton cache fields for [SingleInstanceView] views
        EmitSingletonFields(sb, registrations);

        EmitRegistrationHook(sb, features);

        // Emit the per-view-model dispatch branches into the dispatch function body.
        EmitDispatchBranches(sb, registrations);

        _ = sb.BlankLine()
            .Comment("No compile-time mapping found; fall back to runtime resolution.")
            .Return("null")
            .CloseBlock();

        // Per-view resolver methods
        for (var i = 0; i < registrations.Count; i++)
        {
            GenerateResolverMethod(sb, registrations[i], i, supportsNullable);
        }

        _ = sb.CloseBlock()
            .CloseBlock();

        if (features.DeclaresModuleInitializerAttribute)
        {
            EmitModuleInitializerAttribute(sb);
        }
    }

    /// <summary>Adds the registrations a pipeline found, skipping the ones it declined.</summary>
    /// <param name="all">The registrations so far.</param>
    /// <param name="found">The pipeline's results, with nulls for declined ones.</param>
    private static void AddFound(ImmutableArray<ViewRegistrationInfo>.Builder all, ImmutableArray<ViewRegistrationInfo?> found)
    {
        for (var i = 0; i < found.Length; i++)
        {
            if (found[i] is { } registration)
            {
                all.Add(registration);
            }
        }
    }

    /// <summary>Deduplicates view registrations by (view model fully qualified name, contract) pair.</summary>
    /// <param name="registrations">The raw registrations.</param>
    /// <returns>A deduplicated list of registrations.</returns>
    private static List<ViewRegistrationInfo> Deduplicate(ImmutableArray<ViewRegistrationInfo> registrations)
    {
        var seen = new HashSet<ViewRegistrationKey>();
        var result = new List<ViewRegistrationInfo>(registrations.Length);

        for (var i = 0; i < registrations.Length; i++)
        {
            var reg = registrations[i];
            if (seen.Add(new(reg.ViewModelFullyQualifiedName, reg.Contract)))
            {
                result.Add(reg);
            }
        }

        return result;
    }

    /// <summary>Declares the module initializer attribute for a consumer whose framework does not ship it.</summary>
    /// <param name="sb">The writer, at the start of a line outside any namespace.</param>
    /// <remarks>
    /// File-local, so the declaration belongs to this file alone: a shared internal one would be a type visible to
    /// every assembly granted <c>InternalsVisibleTo</c>, colliding with that assembly's own.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitModuleInitializerAttribute(SourceWriter sb) =>
        sb.BlankLine()
            .OpenNamespace("System.Runtime.CompilerServices")
            .Summary("Marks a method the runtime calls when its module loads.")
            .Attribute($"{GeneratedTypeNames.AttributeUsage}({GeneratedTypeNames.AttributeTargets}.Method, AllowMultiple = false)")
            .OpenType($"file sealed class ModuleInitializerAttribute : {GeneratedTypeNames.Attribute}")
            .CloseBlock()
            .CloseBlock();

    /// <summary>Emits what registers the dispatch function, and the signature of the dispatch function itself.</summary>
    /// <param name="sb">The writer, at the class's member level; left inside the dispatch function's body.</param>
    /// <param name="features">The consumer compilation's language-feature and generation-option snapshot.</param>
    /// <remarks>
    /// From C# 9 the registration is a module initializer, which runs before any code in the assembly, so a
    /// view resolves whether or not a binding has run. An older consumer has no such hook. Its registration is
    /// a static constructor, which runs when the class is first used; a field initializer would not do, because
    /// the runtime only runs one when a static field is read, and no generated member reads one.
    /// </remarks>
    private static void EmitRegistrationHook(SourceWriter sb, in LanguageFeatures features)
    {
        _ = sb.OpenSummary()
            .DocLine("Registers the source-generated view dispatch function with");

        if (features.SupportsModuleInitializer)
        {
            _ = sb.DocLine($"<see cref=\"{GeneratedTypeNames.DefaultViewLocator}\"/> when the module loads.")
                .CloseSummary()
                .Attribute("global::System.Runtime.CompilerServices.ModuleInitializer")
                .Line("internal static void __RegisterViewDispatch()");
        }
        else
        {
            _ = sb.DocLine($"<see cref=\"{GeneratedTypeNames.DefaultViewLocator}\"/> when this class is first used.")
                .CloseSummary()
                .Append("static ").Append(features.GeneratedClassName).Line("()");
        }

        _ = sb.OpenBlock()
            .Line($"{GeneratedTypeNames.DefaultViewLocator}.SetGeneratedViewDispatch(")
            .Indent()
            .Line("__TryResolveView);")
            .Outdent()
            .CloseBlock()
            .BlankLine();

        EmitDispatchSignature(sb, features.SupportsNullable ? "?" : string.Empty);
    }

    /// <summary>Emits the documentation and signature of the dispatch function, and opens its body.</summary>
    /// <param name="sb">The writer, at the class's member level; left inside the dispatch function's body.</param>
    /// <param name="nullable">The nullable annotation to emit, or an empty string when unsupported.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitDispatchSignature(SourceWriter sb, string nullable) =>
        sb.OpenSummary()
            .DocLine("Compile-time generated type-switch dispatch for view resolution.")
            .DocLine("Attempts to resolve a view for the given view model instance without reflection.")
            .CloseSummary()
            .DocLine("<param name=\"instance\">The view model instance to resolve a view for.</param>")
            .DocLine("<param name=\"contract\">The contract string (empty string for default).</param>")
            .DocLine("<returns>The resolved view, or <see langword=\"null\"/> if no generated mapping exists.</returns>")
            .Append($"private static {GeneratedTypeNames.IViewFor}").Append(nullable).Line(" __TryResolveView(")
            .Indent()
            .Line("object instance, string contract)")
            .Outdent()
            .OpenBlock();

    /// <summary>Emits the singleton cache fields for <c>[SingleInstanceView]</c> views with a parameterless constructor.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="registrations">The deduplicated registrations.</param>
    private static void EmitSingletonFields(SourceWriter sb, List<ViewRegistrationInfo> registrations)
    {
        for (var i = 0; i < registrations.Count; i++)
        {
            var reg = registrations[i];
            if (reg.IsSingleInstance && reg.HasParameterlessConstructor)
            {
                _ = sb.OpenSummary()
                    .BeginDocLine().Append("Cached singleton instance for <see cref=\"").Append(reg.ViewFullyQualifiedName)
                    .Line("\"/> (marked with [SingleInstanceView]).")
                    .CloseSummary()
                    .Append("private static ").Append(reg.ViewFullyQualifiedName).Append(" __singletonView_").Append(i).EndStatement()
                    .BlankLine();
            }
        }
    }

    /// <summary>
    /// Groups registrations by ViewModel FQN and emits the dispatch branches in order.
    /// Contract-specific checks are emitted before the default (no-contract) branch within a single
    /// type-switch block. Without grouping, a default branch emitted first would unconditionally
    /// match and shadow contract-specific branches.
    /// </summary>
    /// <param name="sb">The writer, inside the dispatch function's body.</param>
    /// <param name="registrations">The deduplicated registrations.</param>
    private static void EmitDispatchBranches(SourceWriter sb, List<ViewRegistrationInfo> registrations)
    {
        var viewModelOrder = new List<string>(registrations.Count);
        var viewModelGroupIndices = new Dictionary<string, List<int>>(registrations.Count);
        for (var i = 0; i < registrations.Count; i++)
        {
            var viewModelFqn = registrations[i].ViewModelFullyQualifiedName;
            if (!viewModelGroupIndices.TryGetValue(viewModelFqn, out var indices))
            {
                indices = [];
                viewModelGroupIndices[viewModelFqn] = indices;
                viewModelOrder.Add(viewModelFqn);
            }

            indices.Add(i);
        }

        for (var g = 0; g < viewModelOrder.Count; g++)
        {
            var viewModelFqn = viewModelOrder[g];
            var indices = viewModelGroupIndices[viewModelFqn];

            if (indices.Count == 1)
            {
                // Single registration per VM: emit the compact form.
                EmitSingleRegistrationDispatch(sb, registrations, indices[0]);
            }
            else
            {
                // Multiple registrations for same VM: group into single type-switch.
                EmitGroupedDispatch(sb, registrations, viewModelFqn, indices);
            }
        }
    }

    /// <summary>
    /// Emits a dispatch branch for a single registration (one view per VM type).
    /// Preserves the compact output format used by existing tests.
    /// </summary>
    /// <param name="sb">The writer, inside the dispatch function's body.</param>
    /// <param name="registrations">All registrations.</param>
    /// <param name="index">The registration index.</param>
    private static void EmitSingleRegistrationDispatch(
        SourceWriter sb,
        List<ViewRegistrationInfo> registrations,
        int index)
    {
        var reg = registrations[index];

        _ = sb.BeginComment().Append(reg.ViewModelFullyQualifiedName).Append(" -> ").Append(reg.ViewFullyQualifiedName);
        if (reg.Contract is not null)
        {
            var escapedLiteral = SymbolDisplay.FormatLiteral(reg.Contract, true);
            _ = sb.Append(" [contract: ").Append(escapedLiteral).Line("]")
                .Append(InstanceTypeTestOpen).Append(reg.ViewModelFullyQualifiedName).CloseCondition()
                .BeginIf().Append("contract == ").Append(escapedLiteral).CloseCondition();
            AppendResolverReturn(sb, index);
            _ = sb.CloseBlock();
        }
        else
        {
            _ = sb.EndLine()
                .Append(InstanceTypeTestOpen).Append(reg.ViewModelFullyQualifiedName).Append(" && ").Append(DefaultContractTest).CloseCondition();
            AppendResolverReturn(sb, index);
        }

        _ = sb.CloseBlock();
    }

    /// <summary>
    /// Emits a grouped dispatch branch for a VM type with multiple registrations.
    /// Contract-specific checks are emitted first, with the default (no-contract) branch last.
    /// </summary>
    /// <param name="sb">The writer, inside the dispatch function's body.</param>
    /// <param name="registrations">All registrations.</param>
    /// <param name="viewModelFqn">The fully qualified VM type name.</param>
    /// <param name="indices">The registration indices for this VM type.</param>
    private static void EmitGroupedDispatch(
        SourceWriter sb,
        List<ViewRegistrationInfo> registrations,
        string viewModelFqn,
        List<int> indices)
    {
        _ = sb.BeginComment().Append(viewModelFqn).Line(" — multiple views")
            .Append(InstanceTypeTestOpen).Append(viewModelFqn).CloseCondition();

        // Contract-specific branches first
        for (var j = 0; j < indices.Count; j++)
        {
            var idx = indices[j];
            var reg = registrations[idx];
            if (reg.Contract is null)
            {
                continue;
            }

            var escapedLiteral = SymbolDisplay.FormatLiteral(reg.Contract, true);
            _ = sb.BeginComment().Append("-> ").Append(reg.ViewFullyQualifiedName).Append(" [contract: ").Append(escapedLiteral).Line("]")
                .BeginIf().Append("contract == ").Append(escapedLiteral).CloseCondition();
            AppendResolverReturn(sb, idx);
            _ = sb.CloseBlock();
        }

        // Default (no-contract) branch last
        for (var j = 0; j < indices.Count; j++)
        {
            var idx = indices[j];
            var reg = registrations[idx];
            if (reg.Contract is not null)
            {
                continue;
            }

            _ = sb.BeginComment().Append("-> ").Append(reg.ViewFullyQualifiedName).Line(" (default)")
                .If(DefaultContractTest);
            AppendResolverReturn(sb, idx);
            _ = sb.CloseBlock();
            break; // Only one default per VM (deduplicated earlier)
        }

        _ = sb.CloseBlock();
    }

    /// <summary>Writes the return that hands the requested contract to one view's resolver.</summary>
    /// <param name="sb">The writer, inside the branch that matched the view.</param>
    /// <param name="index">The registration index, which names the resolver.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendResolverReturn(SourceWriter sb, int index) =>
        _ = sb.BeginReturn().Append(ResolverMethodNamePrefix).Append(index).Line(ContractResolverCall);

    /// <summary>Generates a per-view-model resolver method.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="reg">The view registration info.</param>
    /// <param name="index">The unique index for method naming.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    private static void GenerateResolverMethod(SourceWriter sb, ViewRegistrationInfo reg, int index, bool supportsNullable)
    {
        var nullable = supportsNullable ? "?" : string.Empty;

        var strategyDoc = (reg.IsSingleInstance, reg.HasParameterlessConstructor) switch
        {
            (true, true) => "Returns a cached singleton instance (marked with [SingleInstanceView]).",
            (true, false) => "Service locator only — [SingleInstanceView] without parameterless constructor.",
            (false, true) => "Tries the service locator first, then falls back to direct construction.",
            (false, false) => "Service locator only — no direct construction available."
        };

        _ = sb.BlankLine()
            .OpenSummary()
            .BeginDocLine().Append("Resolves a view for <see cref=\"").Append(reg.ViewModelFullyQualifiedName).Line("\"/>.")
            .DocLine(strategyDoc)
            .CloseSummary()
            .DocLine("<param name=\"contract\">The contract string (empty string for default).</param>")
            .DocLine("<returns>The resolved view, or <see langword=\"null\"/> if resolution fails.</returns>")
            .Append($"private static {GeneratedTypeNames.IViewFor}").Append(nullable).Append(' ').Append(ResolverMethodNamePrefix).Append(index)
            .Line("(string contract)")
            .OpenBlock()
            .Comment("Normalize contract: empty string means no contract (null for Splat lookup).")
            .Append("string").Append(nullable).Line(" svcContract = contract.Length == 0 ? null : contract;")
            .BlankLine()
            .Comment("Prefer service-locator-registered view (supports DI-configured instances).")
            .Line("var view = global::Splat.AppLocator.Current")
            .Indent()
            .Append($".GetService<{GeneratedTypeNames.IViewFor}<").Append(reg.ViewModelFullyQualifiedName).Line(">>(")
            .Indent()
            .Line("svcContract);")
            .Outdent()
            .Outdent()
            .If("view != null")
            .Return("view")
            .CloseBlock();

        EmitResolverFallback(sb, reg, index);

        _ = sb.CloseBlock();
    }

    /// <summary>
    /// Emits what the resolver does when the service locator has nothing registered: construct the
    /// view directly, cache a singleton, or give up.
    /// </summary>
    /// <param name="sb">The writer, inside the resolver's body.</param>
    /// <param name="reg">The view registration being emitted.</param>
    /// <param name="index">The registration's index, used to name the singleton field.</param>
    private static void EmitResolverFallback(SourceWriter sb, ViewRegistrationInfo reg, int index)
    {
        _ = sb.BlankLine();
        if (!reg.HasParameterlessConstructor)
        {
            _ = sb.Return("null");
            return;
        }

        if (!reg.IsSingleInstance)
        {
            _ = sb.BeginComment().Append("Fallback: direct construction (").Append(reg.ViewFullyQualifiedName).Line(" has a parameterless constructor).")
                .BeginReturn();
            _ = AppendViewResult(sb, reg, $"new {reg.ViewFullyQualifiedName}()").EndStatement();
            return;
        }

        var fieldName = $"__singletonView_{index}";
        _ = sb.BeginComment().Append("Fallback: singleton construction (").Append(reg.ViewFullyQualifiedName).Line(" has [SingleInstanceView]).")
            .BeginIf().Append(fieldName).Append(" == null").CloseCondition()
            .Line("System.Threading.Interlocked.CompareExchange(")
            .Indent()
            .Append("ref ").Append(fieldName).Line(",")
            .Append("new ").Append(reg.ViewFullyQualifiedName).Line("(),")
            .Line("null);")
            .Outdent()
            .CloseBlock()
            .BlankLine()
            .BeginReturn();
        _ = AppendViewResult(sb, reg, fieldName).EndStatement();
    }

    /// <summary>Writes a constructed or cached view as the resolver's result.</summary>
    /// <param name="sb">The writer, after <c>return </c>.</param>
    /// <param name="reg">The view registration.</param>
    /// <param name="view">The expression holding the view.</param>
    /// <returns>The writer, for chaining.</returns>
    /// <remarks>
    /// A view found through <c>[IViewFor]</c> may not implement <c>IViewFor</c>: ReactiveUI.SourceGenerators only adds
    /// the interface to a class deriving from a UI type it supports. Casting through <see cref="object"/> keeps the
    /// resolver compiling either way, and a view without the interface resolves to null.
    /// </remarks>
    private static SourceWriter AppendViewResult(SourceWriter sb, ViewRegistrationInfo reg, string view) =>
        reg.IsDeclaredByAttribute
            ? sb.Append("(object)").Append(view).Append($" as {GeneratedTypeNames.IViewFor}")
            : sb.Append(view);
}
