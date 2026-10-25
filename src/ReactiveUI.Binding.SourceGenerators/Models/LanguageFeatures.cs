// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Value-equatable snapshot of the consumer compilation's relevant C# language capabilities and generation
/// options, flowed through the incremental pipeline so generated output can adapt to the target language
/// version and consumer configuration.
/// </summary>
/// <param name="SupportsCallerArgExpr">
/// Whether the target supports <c>CallerArgumentExpression</c> dispatch (C# 10+ and the attribute is available).
/// </param>
/// <param name="SupportsNullable">
/// Whether the target supports nullable reference types (C# 8+), in which case generated files emit an explicit
/// <c>#nullable enable</c> directive. Emitting that directive on C# 7.3 would be a compile error, so it is omitted.
/// </param>
/// <param name="EmitGeneratedCodeMarkers">
/// Whether generated files emit the <c>// &lt;auto-generated/&gt;</c> comment and <c>#pragma warning disable</c>
/// header. This is the shipping default (<see langword="true"/>); consumers set the MSBuild property
/// <c>ReactiveUIBindingEmitGeneratedCodeMarkers</c> to <c>false</c> to surface analyzer diagnostics in the
/// generated code (e.g. when diagnosing the generator itself).
/// </param>
/// <param name="GeneratedNamespace">
/// The namespace the generated class is emitted into, always one of this assembly's own. Under interception it is
/// <c>ReactiveUI.Binding.Generated.Interceptors.&lt;assembly&gt;</c>, below the namespace the consumer opts into
/// interception. From C# 10 without interception it is <c>ReactiveUI.Binding.Generated.&lt;assembly&gt;</c>,
/// reached by a generated <c>global using</c>, which is scoped to the compilation that declares it, so two
/// assemblies which both run the generator do not see each other's overloads - identical ones visible across an
/// <c>InternalsVisibleTo</c> boundary would make every matching call site ambiguous (CS0121). Below C# 10 there is
/// no way to scope an import, so the overloads sit in the runtime's namespace, or in the root namespace of an
/// assembly that grants <c>InternalsVisibleTo</c>.
/// </param>
/// <param name="EmitGeneratedNamespaceImport">
/// Whether to emit the <c>global using</c> that brings <see cref="GeneratedNamespace"/> into scope: from C# 10,
/// when call sites reach the overloads through lookup rather than interception.
/// </param>
/// <param name="StubHasExpressionParameters">
/// Whether the referenced runtime stub carries optional expression parameters. The generated overload must take
/// the same parameters whether or not it dispatches on them, because the tie-break that lets a concrete
/// overload beat the generic stub only applies once the parameter lists match: a shorter one leaves both
/// candidates merely applicable, and every call site is then ambiguous (CS0121). This is independent of
/// <see cref="SupportsCallerArgExpr"/>, which turns on the attributes and the expression-text dispatch, and
/// which additionally needs C# 10 - a consumer can perfectly well target a runtime that has the attribute
/// while compiling at an older language version.
/// </param>
/// <param name="UsesReactiveRuntime">
/// Whether the consumer references the System.Reactive flavour of the runtime library rather than the lean one.
/// The two share no type names, so generated code written against one does not compile against the other.
/// </param>
/// <param name="RuntimeNamespaceMembers">
/// The names the referenced runtime library's own namespace declares, types and nested namespaces alike. These
/// anchor the retargeting of generated code onto the other flavour, so that it shifts the library's names and
/// leaves alone a consumer type that merely happens to sit under the same root. Empty for a lean consumer,
/// where nothing is retargeted.
/// </param>
/// <param name="PrimitivesNamespaceMembers">
/// The names the System.Reactive flavour of ReactiveUI.Primitives declares. Generated code names the lean
/// Primitives types, and only the ones that flavour actually offers are shifted onto it - the parts that ship in
/// the shared core, the disposables among them, keep their names in both. Empty for a lean consumer.
/// </param>
/// <param name="SupportsInterceptors">
/// Whether generated code claims each call site outright instead of competing for it. This needs both halves:
/// a compiler that can describe a call site, which is settled by the analyzer slot the package resolved to, and
/// a consumer that has opted the generated namespace into interception. Where it holds, none of the placement
/// this record otherwise describes applies - lookup never sees an interceptor, so there is no namespace to
/// reach, no import to emit, and no expression text to match at run time.
/// </param>
/// <param name="SupportsModuleInitializer">
/// Whether the consumer can mark a method as a module initializer (C# 9+). The generated view dispatch registers
/// itself that way, so it is in place before any code in the assembly runs. An older consumer has no such hook
/// and the dispatch registers when its generated class is first used.
/// </param>
/// <param name="DeclaresModuleInitializerAttribute">
/// Whether the compilation has to declare <c>ModuleInitializerAttribute</c> itself, because no accessible one
/// is in reach. Frameworks from .NET 5 ship it; older ones do not.
/// </param>
/// <param name="SupportsOverloadResolutionPriority">
/// Whether generated overloads can carry <c>OverloadResolutionPriorityAttribute</c>: the consumer compiles as C# 13
/// or later and can reach the attribute, which frameworks from .NET 9 ship.
/// </param>
/// <param name="GeneratedClassName">
/// The name of the generated class. It carries the assembly's name wherever <see cref="GeneratedNamespace"/> is
/// not the assembly's own, so no two assemblies ever declare a type by the same fully qualified name.
/// </param>
internal readonly record struct LanguageFeatures(
    bool SupportsCallerArgExpr,
    bool SupportsNullable,
    bool EmitGeneratedCodeMarkers,
    string GeneratedNamespace = Constants.SharedGeneratedNamespace,
    bool EmitGeneratedNamespaceImport = false,
    bool StubHasExpressionParameters = false,
    bool UsesReactiveRuntime = false,
    EquatableArray<string> RuntimeNamespaceMembers = default,
    EquatableArray<string> PrimitivesNamespaceMembers = default,
    bool SupportsInterceptors = false,
    bool SupportsModuleInitializer = false,
    bool DeclaresModuleInitializerAttribute = false,
    bool SupportsOverloadResolutionPriority = false,
    string GeneratedClassName = Constants.GeneratedExtensionClassName)
{
    /// <summary>Gets a value indicating whether call sites a dispatch cannot tell apart collapse to one.</summary>
    /// <remarks>
    /// Binding the same pair of properties from more than one place is ordinary, and expression-text dispatch
    /// keys on the selectors as written: the first matching branch wins, so the later ones are unreachable and
    /// only drag a binding method along. An interceptor instead names the call site it replaces, so dropping one
    /// leaves it carrying no attribute - on the runtime engine while the call site beside it is generated. The
    /// rule lives here because each API would otherwise decide it separately, and the one that forgot would
    /// silently lose a binding rather than fail to compile.
    /// </remarks>
    internal bool CollapsesIndistinguishableCallSites => SupportsCallerArgExpr && !SupportsInterceptors;
}
