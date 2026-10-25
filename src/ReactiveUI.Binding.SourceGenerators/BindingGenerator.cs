// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Generators;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Invocations;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// The main incremental source generator entry point for ReactiveUI property observation and binding.
/// Invocation extraction captures notification mechanisms from each bound property's owner.
/// View detection supplies the independent IViewFor&lt;T&gt; dispatch mappings.
/// </summary>
[Generator]
public class BindingGenerator : IIncrementalGenerator
{
    /// <summary>Capacity for the small file carrying the namespace declaration and its import.</summary>
    private const int AttributeFileCapacity = 512;

    /// <summary>The generic arity of the single-property WhenAnyValue stub.</summary>
    private const int SinglePropertyObservationArity = 2;

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var languageFeatures = SelectLanguageFeatures(in context);

        RegisterSharedAttributeOutput(in context, languageFeatures);

        // Pipeline C: View locator dispatch (IViewFor<T> scanning)
        ViewLocatorDispatchGenerator.Register(context, languageFeatures);

        // Partial properties marked [ObservableAsProperty]: their bodies and helper fields.
        ObservableAsPropertyGenerator.Register(context, languageFeatures);

        // Pipeline B: Invocation detection, one provider per API. The scan lives here rather than inside each
        // generator so that everything this generator looks at in the consumer's syntax is visible in one
        // place; each generator is handed the invocations it asked for and only turns them into source.
        // The four property-binding APIs differ only in which call sites they match, so one extraction
        // delegate serves all of them rather than a conversion per scan.
        Func<CallSiteContext, CancellationToken, BindingInvocationInfo?> extractBinding =
            BindingExtractor.ExtractBindInvocation;

        // Call sites that bind a member ReactiveUI.SourceGenerators writes are read from a compilation declaring it.
        var sg = SourceGeneratorsCompilation.Register(in context);

        var whenChanged = Detect(in context, RoslynHelpers.IsWhenChangedInvocation, ObservationExtractor.ExtractWhenChangedInvocation, sg);
        var whenChanging = Detect(in context, RoslynHelpers.IsWhenChangingInvocation, ObservationExtractor.ExtractWhenChangingInvocation, sg);
        var whenAnyValue = Detect(in context, RoslynHelpers.IsWhenAnyValueInvocation, ObservationExtractor.ExtractWhenAnyValueInvocation, sg);
        var whenAny = Detect(in context, RoslynHelpers.IsWhenAnyInvocation, ObservationExtractor.ExtractWhenAnyInvocation, sg);
        var whenAnyObservable = Detect(in context, RoslynHelpers.IsWhenAnyObservableInvocation, WhenAnyObservableExtractor.ExtractWhenAnyObservableInvocation, sg);
        var bindOneWay = Detect(in context, RoslynHelpers.IsBindOneWaySpecificInvocation, extractBinding, sg);
        var bindTwoWay = Detect(in context, RoslynHelpers.IsBindTwoWaySpecificInvocation, extractBinding, sg);
        var oneWayBind = Detect(in context, RoslynHelpers.IsOneWayBindSpecificInvocation, extractBinding, sg);
        var bind = Detect(in context, RoslynHelpers.IsBindSpecificInvocation, extractBinding, sg);
        var bindCommand = Detect(in context, RoslynHelpers.IsBindCommandInvocation, CommandExtractor.ExtractBindCommandInvocation, sg);
        var bindInteraction = Detect(in context, RoslynHelpers.IsBindInteractionInvocation, InteractionExtractor.ExtractBindInteractionInvocation, sg);
        var bindTo = Detect(in context, RoslynHelpers.IsBindToInvocation, BindToExtractor.ExtractBindToInvocation, sg);
        var invokeCommand = Detect(in context, RoslynHelpers.IsInvokeCommandInvocation, InvokeCommandExtractor.ExtractInvokeCommandInvocation, sg);
        var toProperty = Detect(in context, RoslynHelpers.IsToPropertyInvocation, ToPropertyExtractor.ExtractToPropertyInvocation, sg);

        // Each invocation generator receives the language-feature snapshot to control dispatch/output
        WhenChangedInvocationGenerator.Register(context, whenChanged, languageFeatures);
        WhenChangingInvocationGenerator.Register(context, whenChanging, languageFeatures);
        BindOneWayInvocationGenerator.Register(context, bindOneWay, languageFeatures);
        BindTwoWayInvocationGenerator.Register(context, bindTwoWay, languageFeatures);
        OneWayBindInvocationGenerator.Register(context, oneWayBind, languageFeatures);
        BindInvocationGenerator.Register(context, bind, languageFeatures);
        WhenAnyValueInvocationGenerator.Register(context, whenAnyValue, languageFeatures);
        WhenAnyInvocationGenerator.Register(context, whenAny, languageFeatures);
        WhenAnyObservableInvocationGenerator.Register(context, whenAnyObservable, languageFeatures);
        BindInteractionInvocationGenerator.Register(context, bindInteraction, languageFeatures);
        BindCommandInvocationGenerator.Register(context, bindCommand, languageFeatures);
        BindToInvocationGenerator.Register(context, bindTo, languageFeatures);
        InvokeCommandInvocationGenerator.Register(context, invokeCommand, languageFeatures);
        ToPropertyInvocationGenerator.Register(context, toProperty, languageFeatures);
    }

    /// <summary>Reads the C# language version the consumer is compiling with.</summary>
    /// <param name="parseOptions">The parse options the compilation was built with.</param>
    /// <returns>The consumer's language version, or the compiler default when the options are not C#'s.</returns>
    /// <remarks>
    /// Everything downstream keys on the language version, so options belonging to another language - or none
    /// at all - resolve to the default rather than failing the whole generation pass.
    /// </remarks>
    internal static LanguageVersion ReadLanguageVersion(ParseOptions? parseOptions) =>
        (parseOptions as CSharpParseOptions)?.LanguageVersion ?? LanguageVersion.Default;

    /// <summary>Determines whether generated code can apply <c>CallerArgumentExpression</c>.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when the attribute is present and the consumer can apply it.</returns>
    /// <remarks>
    /// Accessibility is part of the test: on a target framework without the attribute, the only one in reach
    /// may be the runtime library's own internal copy, which generated code cannot apply.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAccessibleExpressionAttribute(Compilation compilation) =>
        HasAccessibleAttribute(compilation, Constants.CallerArgumentExpressionAttributeMetadataName);

    /// <summary>Reads the expression-parameter shape from the referenced runtime stub.</summary>
    /// <param name="runtimeStub">The runtime extension type selected for this flavor.</param>
    /// <returns>Whether its single-property observation overload declares an expression-text parameter.</returns>
    /// <remarks>
    /// A consumer can see <c>CallerArgumentExpressionAttribute</c> even when it references a net4x runtime
    /// asset built without expression parameters. Interceptors must match the referenced method exactly.
    /// </remarks>
    internal static bool StubHasExpressionParameters(INamedTypeSymbol? runtimeStub)
    {
        if (runtimeStub is null)
        {
            return false;
        }

        foreach (var member in runtimeStub.GetMembers("WhenAnyValue"))
        {
            if (member is not IMethodSymbol { Arity: SinglePropertyObservationArity } method)
            {
                continue;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.Name == "property1Expression")
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Determines whether the consumer can apply an attribute type, wherever it is declared.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="metadataName">The metadata name of the attribute type.</param>
    /// <returns><see langword="true"/> when the type resolves and is accessible from the consumer's assembly.</returns>
    internal static bool HasAccessibleAttribute(Compilation compilation, string metadataName)
    {
        var attribute = compilation.GetTypeByMetadataName(metadataName);

        return attribute is not null
            && compilation.IsSymbolAccessibleWithin(attribute, compilation.Assembly);
    }

    /// <summary>Selects the referenced runtime extension type and its flavor.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="usesReactiveRuntime">Whether only the System.Reactive flavor is referenced.</param>
    /// <returns>The extension type whose method signatures generated interceptors must match.</returns>
    private static INamedTypeSymbol? SelectRuntimeStub(Compilation compilation, out bool usesReactiveRuntime)
    {
        var reactiveStub = compilation.GetTypeByMetadataName(Constants.ReactiveStubMetadataName);
        var leanStub = compilation.GetTypeByMetadataName(Constants.LeanStubMetadataName);
        usesReactiveRuntime = reactiveStub is not null && leanStub is null;
        return usesReactiveRuntime ? reactiveStub : leanStub;
    }

    /// <summary>Picks the namespace and class the generated code is emitted into.</summary>
    /// <param name="configOptions">The analyzer config options, which carry the consumer's root namespace.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="supportsInterceptors">Whether the generated code intercepts its call sites.</param>
    /// <param name="supportsGlobalUsings">Whether the consumer can scope a namespace import to its compilation.</param>
    /// <param name="sharedNamespace">The runtime library's own namespace, which every consumer imports.</param>
    /// <returns>The namespace and the class name.</returns>
    /// <remarks>
    /// Every assembly's generated type has a fully qualified name of its own, so an assembly granted
    /// <c>InternalsVisibleTo</c> never sees two types by one name. Interceptors sit in a per-assembly namespace below
    /// the one the consumer opts into interception. Otherwise the overloads have to be reached by lookup: from C# 10
    /// in the consumer's root namespace or a per-assembly one, and before that in a namespace other assemblies may
    /// share. Wherever the namespace is not the assembly's own, the class name carries the assembly instead.
    /// </remarks>
    private static (string Namespace, string ClassName) SelectGeneratedPlacement(
        AnalyzerConfigOptionsProvider configOptions,
        Compilation compilation,
        bool supportsInterceptors,
        bool supportsGlobalUsings,
        string sharedNamespace)
    {
        var assemblySegments = ToNamespaceSegments(compilation.AssemblyName);
        if (supportsInterceptors)
        {
            return ($"{Constants.InterceptorNamespace}.{assemblySegments}", Constants.GeneratedExtensionClassName);
        }

        var perAssemblyNamespace = $"{Constants.GeneratedNamespaceRoot}.{assemblySegments}";
        var generatedNamespace = supportsGlobalUsings
            ? SelectGeneratedNamespace(configOptions, compilation, perAssemblyNamespace)
            : SelectSharedTierNamespace(configOptions, compilation, sharedNamespace);
        return (
            generatedNamespace,
            string.Equals(generatedNamespace, perAssemblyNamespace, StringComparison.Ordinal)
                ? Constants.GeneratedExtensionClassName
                : $"{Constants.GeneratedExtensionClassName}_{assemblySegments.Replace('.', '_')}");
    }

    /// <summary>
    /// Picks the namespace the dispatch overloads are emitted into: the consumer's own root namespace when the
    /// build exposes one, and a namespace derived from the assembly name when it does not.
    /// </summary>
    /// <param name="configOptions">The analyzer config options, which carry the consumer's root namespace.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="perAssemblyNamespace">The namespace derived from the assembly name.</param>
    /// <returns>The namespace to emit the dispatch overloads into.</returns>
    /// <remarks>
    /// <para>
    /// Extension-method lookup walks the enclosing namespaces of the call site from the inside out and stops at
    /// the first level that yields any candidate. The runtime stub lives in <c>ReactiveUI.Binding</c>, so a
    /// consumer whose own code sits under that namespace reaches the stub at an enclosing level and the lookup
    /// stops there - a namespace brought in by a <c>global using</c> is only ever consulted at the outermost
    /// level, so the generated overload would never be considered and the call would fall through to the stub's
    /// runtime throw. Emitting into the consumer's root namespace puts the overload at a level at or inside
    /// their own code, so it is reached first whatever they have named their namespaces.
    /// </para>
    /// <para>
    /// The <c>global using</c> is still emitted, and still carries files whose namespace sits outside the root
    /// namespace. Both routes land the concrete overload in the same candidate set as the generic stub, where
    /// it wins outright: a non-generic candidate is preferred over a generic one.
    /// </para>
    /// <para>
    /// Two assemblies sharing a root namespace would land in the same place, and if one also exposes its
    /// internals to the other, both would answer the same call (CS0121). That is detectable - the other
    /// assembly's dispatch class is already there to be found - so this steps aside to the per-assembly
    /// namespace when it sees one, at the cost of the reach a root namespace buys. The class in a root namespace
    /// carries the assembly's name, so the type names never collide either way.
    /// </para>
    /// </remarks>
    private static string SelectGeneratedNamespace(
        AnalyzerConfigOptionsProvider configOptions,
        Compilation compilation,
        string perAssemblyNamespace)
    {
        var hasRootNamespace = configOptions.GlobalOptions.TryGetValue(
                "build_property.RootNamespace",
                out var rootNamespace)
            && !string.IsNullOrWhiteSpace(rootNamespace);

        if (!hasRootNamespace)
        {
            return perAssemblyNamespace;
        }

        // Another assembly that shares this root namespace and exposes its internals here has already put its
        // dispatch class where this one would go, and both would then answer the same call. Nothing rules that
        // out at the language level, so ask whether it has actually happened and step aside when it has.
        var candidate = ToNamespaceSegments(rootNamespace);
        return HasGeneratedClassIn(compilation, candidate) ? perAssemblyNamespace : candidate;
    }

    /// <summary>Determines whether a referenced assembly already declares a generated class in a namespace.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="namespaceName">The namespace to look in.</param>
    /// <returns><see langword="true"/> when a type named for this generator is already there.</returns>
    private static bool HasGeneratedClassIn(Compilation compilation, string namespaceName)
    {
        var current = compilation.GlobalNamespace;
        foreach (var segment in namespaceName.Split('.'))
        {
            if (FindChildNamespace(current, segment) is not { } child)
            {
                return false;
            }

            current = child;
        }

        foreach (var type in current.GetTypeMembers())
        {
            if (type.Name.StartsWith(Constants.GeneratedExtensionClassName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds a namespace's child namespace by name.</summary>
    /// <param name="parent">The namespace to look in.</param>
    /// <param name="name">The child namespace's name.</param>
    /// <returns>The child namespace, or null when there is none.</returns>
    private static INamespaceSymbol? FindChildNamespace(INamespaceSymbol parent, string name)
    {
        foreach (var child in parent.GetNamespaceMembers())
        {
            if (string.Equals(child.Name, name, StringComparison.Ordinal))
            {
                return child;
            }
        }

        return null;
    }

    /// <summary>Runs one syntax scan and keeps the call sites it could extract.</summary>
    /// <typeparam name="T">The extracted call-site model.</typeparam>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="predicate">The syntactic filter for this API.</param>
    /// <param name="transform">The semantic extraction for this API.</param>
    /// <param name="sourceGenerators">The compilation that declares ReactiveUI.SourceGenerators' members, or null.</param>
    /// <returns>The extracted call sites, with the unanalyzable ones dropped.</returns>
    /// <remarks>
    /// Each call site is read from the compilation that declares ReactiveUI.SourceGenerators' members when the consumer
    /// uses that generator, so a call site binding one of its properties is read like any other.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IncrementalValuesProvider<T> Detect<T>(
        in IncrementalGeneratorInitializationContext context,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<CallSiteContext, CancellationToken, T?> transform,
        IncrementalValueProvider<Compilation?> sourceGenerators)
        where T : class =>
        context.SyntaxProvider
            .CreateSyntaxProvider(predicate, static (syntaxContext, _) => syntaxContext)
            .Combine(sourceGenerators)
            .Select((data, ct) => transform(CallSiteContext.From(data.Left, data.Right), ct))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

    /// <summary>
    /// Emits the one part of the dispatch class that carries <c>[ExcludeFromCodeCoverage]</c>, plus the
    /// <c>global using</c> that puts the generated namespace in scope for this compilation.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="languageFeatures">The consumer's language-feature snapshot, which names the namespace.</param>
    /// <remarks>
    /// Driven off the compilation rather than post-initialization output because the namespace depends on the
    /// consumer's root namespace, which post-initialization cannot see. The attribute goes on exactly one part:
    /// it is not <c>AllowMultiple</c>, so repeating it across the dispatch files would be a duplicate-attribute
    /// error.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RegisterSharedAttributeOutput(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<LanguageFeatures> languageFeatures) =>
        context.RegisterSourceOutput(
            languageFeatures,
            static (ctx, features) =>
            {
                var sb = SourceWriter.Rent(AttributeFileCapacity)
                    .FileHeader(features.EmitGeneratedCodeMarkers, enableNullable: false);

                if (features.EmitGeneratedNamespaceImport)
                {
                    // Compilation-scoped, so a referencing assembly never gains this import and therefore never
                    // sees these overloads - which is what keeps two generator-running assemblies apart.
                    _ = sb.Append("global using global::")
                        .Append(features.GeneratedNamespace)
                        .Line(";")
                        .BlankLine();
                }

                CodeGeneratorHelpers.OpenGeneratedClass(
                    sb.OpenNamespace(features.GeneratedNamespace).Attribute("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage"),
                    features);
                _ = sb.CloseBlock().CloseBlock();

                CodeGeneratorHelpers.AddGeneratedSource(
                    ctx,
                    "GeneratedBindingsAttributes.g.cs",
                    sb.ToStringAndReturn(),
                    features);
            });

    /// <summary>
    /// Snapshots the consumer's relevant C# language capabilities once, to flow through every
    /// invocation pipeline. CallerArgumentExpression (C# 10+ and the attribute being available)
    /// selects expression-text dispatch over file and line dispatch; nullable reference types
    /// (C# 8+) makes the generated files emit <c>#nullable enable</c>.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    /// <returns>A provider yielding the consumer's language-feature snapshot.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IncrementalValueProvider<LanguageFeatures> SelectLanguageFeatures(
        in IncrementalGeneratorInitializationContext context) =>
        context.ParseOptionsProvider
            .Combine(context.CompilationProvider)
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (data, _) =>
            {
                var parseOptions = data.Left.Left;
                var compilation = data.Left.Right;
                var configOptions = data.Right;

                var languageVersion = ReadLanguageVersion(parseOptions);
                var callerArgExprAvailable = HasAccessibleExpressionAttribute(compilation);

                // A framework that predates ModuleInitializerAttribute gets a file-local declaration, which needs
                // C# 11; below that such a consumer registers its views from a static constructor instead.
                var hasModuleInitializerAttribute = HasAccessibleAttribute(compilation, Constants.ModuleInitializerAttributeMetadataName);
                var supportsModuleInitializer = languageVersion >= LanguageVersion.CSharp9
                    && (hasModuleInitializerAttribute || languageVersion >= LanguageVersion.CSharp11);

                // Generated-file markers (// <auto-generated/> + #pragma warning disable) are emitted by default
                // (the shipping convention); consumers opt out with ReactiveUIBindingEmitGeneratedCodeMarkers=false
                // to surface analyzer diagnostics in the generated code. Absent property => default (markers on).
                var emitGeneratedCodeMarkers = !(configOptions.GlobalOptions.TryGetValue(
                        "build_property.ReactiveUIBindingEmitGeneratedCodeMarkers",
                        out var markersValue)
                    && string.Equals(markersValue, "false", System.StringComparison.OrdinalIgnoreCase));

                // Which of the two runtime packages is referenced decides every type name in the generated
                // output, and where the overloads have to live to be found from a call site.
                var runtimeStub = SelectRuntimeStub(compilation, out var usesReactiveRuntime);
                var stubHasExpressionParameters = StubHasExpressionParameters(runtimeStub);
                var supportsCallerArgExpr = languageVersion >= LanguageVersion.CSharp10
                    && callerArgExprAvailable
                    && stubHasExpressionParameters;
                var runtimeNamespaceMembers = usesReactiveRuntime
                    ? CollectNamespaceMemberNames(runtimeStub!.ContainingNamespace)
                    : default;

                // The Primitives stack ships the same two flavours, so the same shift applies to the operators
                // and signal factories the generated code calls. Anchored on what the reactive flavour offers,
                // which is what leaves the shared core - the disposables among it - unshifted.
                var primitivesAnchor = usesReactiveRuntime
                    ? compilation.GetTypeByMetadataName(Constants.PrimitivesReactiveAnchorMetadataName)
                    : null;
                var primitivesNamespaceMembers = primitivesAnchor is not null
                    ? CollectTypePaths(primitivesAnchor.ContainingNamespace)
                    : default;

                // A global using is scoped to the compilation that declares it and is never exported to a
                // referencing assembly, so it is what lets each assembly reach its own generated overloads
                // and nobody else's. It needs C# 10; older consumers share the runtime library's namespace.
                var supportsGlobalUsings = languageVersion >= LanguageVersion.CSharp10;
                var sharedNamespace = usesReactiveRuntime
                    ? Constants.ReactiveRuntimeNamespace
                    : Constants.SharedGeneratedNamespace;

                // An interceptor claims its call site outright, so where one can be emitted there is no namespace
                // for lookup to reach and no import to scope. Each intercepting file declares its own file-local
                // interception attribute, which needs C# 11.
                var supportsInterceptors = InterceptableLocationReader.IsInterceptionEnabled(parseOptions)
                    && languageVersion >= LanguageVersion.CSharp11;
                var (generatedNamespace, generatedClassName) =
                    SelectGeneratedPlacement(configOptions, compilation, supportsInterceptors, supportsGlobalUsings, sharedNamespace);

                return new LanguageFeatures(
                    supportsCallerArgExpr,
                    languageVersion >= LanguageVersion.CSharp8,
                    emitGeneratedCodeMarkers,
                    generatedNamespace,
                    supportsGlobalUsings && !supportsInterceptors,
                    stubHasExpressionParameters,
                    usesReactiveRuntime,
                    runtimeNamespaceMembers,
                    primitivesNamespaceMembers,
                    supportsInterceptors,
                    supportsModuleInitializer,
                    supportsModuleInitializer && !hasModuleInitializerAttribute,
                    languageVersion > LanguageVersion.CSharp12
                        && HasAccessibleAttribute(compilation, Constants.OverloadResolutionPriorityAttributeMetadataName),
                    generatedClassName);
            });

    /// <summary>
    /// Picks the namespace for a consumer that predates global usings, where there is no way to scope a
    /// namespace to one compilation.
    /// </summary>
    /// <param name="configOptions">The analyzer config options, which carry the consumer's root namespace.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="sharedNamespace">The runtime library's own namespace, which every consumer imports.</param>
    /// <returns>The namespace to emit the dispatch overloads into.</returns>
    /// <remarks>
    /// <para>
    /// The runtime library's namespace is the one place every call site can reach without an import of its own,
    /// so it is where these overloads belong - except that it is also the one place another assembly can see
    /// them. Identical overloads visible from two assemblies make every matching call ambiguous (CS0121), and
    /// without global usings there is nothing to scope them with.
    /// </para>
    /// <para>
    /// The overloads are only visible to another assembly if this one grants it <c>InternalsVisibleTo</c>, so
    /// that is the condition to move them out of the shared namespace for. An assembly that grants it emits
    /// into its own root namespace instead, which its own code sits under and no one else's does. The cost is
    /// that a file declared outside the root namespace no longer reaches them and falls back to the runtime
    /// path; the alternative for those assemblies is not universal reach but a build that does not compile.
    /// </para>
    /// <para>
    /// Everyone else - which is nearly everyone - keeps the shared namespace and reaches the overloads from
    /// any file, exactly as before.
    /// </para>
    /// </remarks>
    private static string SelectSharedTierNamespace(
        AnalyzerConfigOptionsProvider configOptions,
        Compilation compilation,
        string sharedNamespace)
    {
        if (!GrantsInternalsVisibleTo(compilation))
        {
            return sharedNamespace;
        }

        var hasRootNamespace = configOptions.GlobalOptions.TryGetValue(
                "build_property.RootNamespace",
                out var rootNamespace)
            && !string.IsNullOrWhiteSpace(rootNamespace);

        // With no root namespace there is nowhere else the call sites could reach, so the shared namespace and
        // its collision risk is still better than emitting somewhere nothing can see.
        return hasRootNamespace ? ToNamespaceSegments(rootNamespace) : sharedNamespace;
    }

    /// <summary>Determines whether the compilation exposes its internals to another assembly.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when the assembly grants <c>InternalsVisibleTo</c> to anyone.</returns>
    private static bool GrantsInternalsVisibleTo(Compilation compilation)
    {
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            if (NativeTypeIdentity.Matches(attribute.AttributeClass, Constants.InternalsVisibleToAttributeName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Collects the names a namespace declares - its types and its nested namespaces - so generated references
    /// to the runtime library can be told apart from consumer code sitting under the same root.
    /// </summary>
    /// <param name="runtimeNamespace">The referenced runtime library's own namespace.</param>
    /// <returns>The declared names, ordered so the pipeline sees a stable value.</returns>
    private static EquatableArray<string> CollectNamespaceMemberNames(INamespaceSymbol runtimeNamespace)
    {
        var names = new SortedSet<string>(System.StringComparer.Ordinal);
        foreach (var member in runtimeNamespace.GetMembers())
        {
            _ = names.Add(member.Name);
        }

        var ordered = new string[names.Count];
        names.CopyTo(ordered);
        return new(ordered);
    }

    /// <summary>
    /// Collects every public type a namespace offers, keyed by its path below that namespace, so a generated
    /// reference can be matched as a whole rather than by its leading segment.
    /// </summary>
    /// <param name="root">The namespace to walk.</param>
    /// <returns>The type paths, ordered so the pipeline sees a stable value.</returns>
    /// <remarks>
    /// The Primitives stack splits a single namespace across two assemblies: the shared core contributes types
    /// that keep their names in both flavours, while the flavoured package contributes the ones that shift. Only
    /// the whole path tells the two apart, so the leading segment is not enough to decide.
    /// </remarks>
    private static EquatableArray<string> CollectTypePaths(INamespaceSymbol root)
    {
        var names = new SortedSet<string>(System.StringComparer.Ordinal);
        var pending = new Stack<NamespacePath>();
        pending.Push(new(root, string.Empty));

        while (pending.Count > 0)
        {
            var (current, prefix) = pending.Pop();
            foreach (var member in current.GetMembers())
            {
                if (member is INamespaceSymbol child)
                {
                    pending.Push(new(child, $"{prefix}{child.Name}."));
                }
                else if (member is INamedTypeSymbol { DeclaredAccessibility: Accessibility.Public } type)
                {
                    _ = names.Add(prefix + type.MetadataName);
                }
            }
        }

        var ordered = new string[names.Count];
        names.CopyTo(ordered);
        return new(ordered);
    }

    /// <summary>
    /// Renders a root namespace or assembly name as namespace segments, so the generated namespace is one the
    /// consumer's own code sits under and is still a legal namespace.
    /// </summary>
    /// <param name="name">The root namespace or assembly name, either of which may be absent.</param>
    /// <returns>Dot-separated identifier segments.</returns>
    /// <remarks>
    /// Dots are kept as segment separators rather than flattened, so <c>My.App</c> and <c>My_App</c> stay
    /// distinct; collapsing both to one segment would give two assemblies the same generated namespace and
    /// bring back the very ambiguity the per-assembly namespace exists to prevent.
    /// </remarks>
    private static string ToNamespaceSegments(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Constants.AnonymousAssemblyNamespaceSegment;
        }

        var segments = name!.Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = ToIdentifier(segments[i]);
        }

        return string.Join(".", segments);
    }

    /// <summary>Renders one namespace segment as a legal identifier.</summary>
    /// <param name="segment">The raw segment, which may be empty or start with a digit.</param>
    /// <returns>A legal C# identifier.</returns>
    private static string ToIdentifier(string segment)
    {
        var builder = new PooledStringBuilder(segment.Length + 1);

        // An identifier cannot be empty, start with a digit or be a keyword, so lead with an underscore where needed.
        if (segment.Length == 0 || (!char.IsLetter(segment[0]) && segment[0] != '_') || SyntaxFacts.GetKeywordKind(segment) != SyntaxKind.None)
        {
            _ = builder.Append('_');
        }

        foreach (var character in segment)
        {
            _ = builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');
        }

        return builder.ToStringAndReturn();
    }
}
