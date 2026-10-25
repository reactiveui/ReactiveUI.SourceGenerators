// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Shared utility methods for code generation: property path building, string escaping, and class info lookup.</summary>
internal static class CodeGeneratorHelpers
{
    /// <summary>Buffer capacity to reserve per invocation when building a generated source file.</summary>
    internal const int PerInvocationBufferCapacity = 1_024;

    /// <summary>Buffer capacity for a dispatch key or other short generated fragment.</summary>
    internal const int FragmentBufferCapacity = 128;

    /// <summary>Opens the comparison of a captured expression against the text a call site spelled.</summary>
    internal const string ExpressionTextComparison = " == \"";

    /// <summary>Completes the name of the parameter that captures a selector's expression text.</summary>
    internal const string ExpressionParameterSuffix = "Expression";

    /// <summary>The caller-file parameter every runtime stub declares before the caller-line one.</summary>
    private const string CallerFilePathParameter =
        $"[{GeneratedTypeNames.CallerFilePath}] string callerFilePath = \"\"";

    /// <summary>The caller-line parameter every runtime stub ends with.</summary>
    private const string CallerLineNumberParameter =
        $"[{GeneratedTypeNames.CallerLineNumber}] int callerLineNumber = 0";

    /// <summary>The ordinal comparison a caller-file test matches the path with.</summary>
    private const string OrdinalIgnoreCaseArgument = $"\", {GeneratedTypeNames.OrdinalIgnoreCase})";

    /// <summary>Buffer capacity to reserve per property-path segment when building an access chain.</summary>
    private const int PerPathSegmentCapacity = 16;

    /// <summary>The name a guarded assignment gives the local holding one walked intermediate.</summary>
    private const string ParentLocalPrefix = "__parent";

    /// <summary>Extra buffer capacity for the escaping a string literal adds.</summary>
    private const int EscapeOverheadCapacity = 4;

    /// <summary>The initial seed value for the polynomial hash used by <see cref="ComputeStableMethodSuffix(string, string, int, string)"/>.</summary>
    private const long HashSeed = 17L;

    /// <summary>The multiplier applied at each step of the polynomial hash used by <see cref="ComputeStableMethodSuffix(string, string, int, string)"/>.</summary>
    private const long HashMultiplier = 31L;

    /// <summary>The FNV-1a 32-bit offset basis used by <see cref="StableStringHash"/>.</summary>
    private const uint FnvOffsetBasis = 2_166_136_261;

    /// <summary>The FNV-1a 32-bit prime used by <see cref="StableStringHash"/>.</summary>
    private const int FnvPrime = 16_777_619;

    /// <summary>
    /// Returns the leaf property type of a path for use as a generated <c>Expression&lt;Func&lt;…, T&gt;&gt;</c>
    /// selector parameter, annotated nullable (<c>T?</c>) when the target supports nullable reference types and
    /// the leaf is a reference type. This lets the generated selector accept lambdas over nullable
    /// reference-typed properties without a CS8603 mismatch; value-type leaves are left unchanged (annotating
    /// them would insert a Convert node and break expression-path extraction).
    /// </summary>
    /// <param name="path">The property path; its last segment is the selector's leaf.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <returns>The leaf type name, suffixed with <c>?</c> where appropriate.</returns>
    internal static string NullableSelectorLeafType(EquatableArray<PropertyPathSegment> path, bool supportsNullable)
    {
        var leaf = path[path.Length - 1];
        return NullableSelectorType(leaf.PropertyTypeFullName, leaf.IsReferenceType, supportsNullable);
    }

    /// <summary>Annotates a selector's produced type the way the stub declares it.</summary>
    /// <param name="typeFullName">The fully qualified produced type.</param>
    /// <param name="isReferenceType">Whether that type is a reference type.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <returns>The type name, suffixed with <c>?</c> where appropriate.</returns>
    /// <remarks>
    /// Taken as the type rather than as a path, because a selector the compiler could not read names no path and
    /// its produced type comes from the method the call resolved to instead.
    /// </remarks>
    internal static string NullableSelectorType(string typeFullName, bool isReferenceType, bool supportsNullable) =>
        supportsNullable && isReferenceType ? $"{typeFullName}?" : typeFullName;

    /// <summary>Builds a dotted property access chain from a root variable and property path segments.</summary>
    /// <param name="root">The root variable name (e.g., "obj", "source").</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "obj.Address.City".</returns>
    internal static string BuildPropertyAccessChain(string root, EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            return root;
        }

        var chain = root;
        for (var i = 0; i < path.Length; i++)
        {
            chain = AppendSegmentRead(chain, path[i]);
        }

        return chain;
    }

    /// <summary>Reads one segment from the expression that produced its parent, narrowing where the path says to.</summary>
    /// <param name="parent">The expression producing the object to read from.</param>
    /// <param name="segment">The property being read.</param>
    /// <returns>The expression producing the segment's value.</returns>
    /// <remarks>
    /// A view exposing its view model as a base or an interface still holds the view model the call site
    /// named, so the read narrows to it and the rest of the path continues from there.
    /// </remarks>
    internal static string AppendSegmentRead(string parent, PropertyPathSegment segment)
    {
        var sb = new PooledStringBuilder(parent.Length + PerPathSegmentCapacity);
        _ = sb.Append(parent).Append('.').Append(segment.PropertyName);

        if (segment.ReadCastTypeFullName is null)
        {
            return sb.ToStringAndReturn();
        }

        var read = sb.ToStringAndReturn();
        var cast = new PooledStringBuilder(read.Length + segment.ReadCastTypeFullName.Length + PerPathSegmentCapacity);

        return cast.Append("((").Append(segment.ReadCastTypeFullName).Append(")(object)")
            .Append(read).Append(')').ToStringAndReturn();
    }

    /// <summary>Builds a property access expression for use in a lambda body.</summary>
    /// <param name="param">The lambda parameter name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "x.Address.City".</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildPropertyAccessLambda(string param, EquatableArray<PropertyPathSegment> path) =>
        BuildPropertyAccessChain(param, path);

    /// <summary>Builds a property setter chain for assignment (e.g., target.Header.Title).</summary>
    /// <param name="root">The root variable name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain suitable for the left side of an assignment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildPropertySetterChain(string root, EquatableArray<PropertyPathSegment> path) =>
        BuildPropertyAccessChain(root, path);

    /// <summary>Writes the statements that assign to the end of a property path, skipping writes that would not change it.</summary>
    /// <param name="writer">The writer, at the level the statements belong at.</param>
    /// <param name="root">The root variable name.</param>
    /// <param name="path">The property path segments.</param>
    /// <param name="valueExpression">The expression producing the value to assign, evaluated more than once.</param>
    /// <remarks>
    /// <para>
    /// A write that would not change the property is dropped. That is what keeps a two-way binding from
    /// oscillating: writing the view raises the view's own change notification, which writes the view model,
    /// which writes the view again. Comparing first breaks the loop at the first repetition, and it also
    /// spares every binding the notifications a redundant write would raise. The comparison is the target's
    /// own equality, so a type that overrides it decides what "unchanged" means.
    /// </para>
    /// <para>
    /// The read side of a chain tolerates a missing parent and still delivers a value, so a binding into a path
    /// whose intermediate is null would otherwise assign through it and throw inside the call that established
    /// the binding. Dropping the write instead matches what the runtime engine does when a chain getter fails.
    /// Emitted as early returns rather than nesting so a long path stays flat.
    /// </para>
    /// </remarks>
    internal static void AppendGuardedAssignment(
        SourceWriter writer,
        string root,
        EquatableArray<PropertyPathSegment> path,
        string valueExpression)
    {
        var leaf = path[path.Length - 1];
        var parent = root;

        for (var i = 0; i < path.Length - 1; i++)
        {
            var local = ParentLocalPrefix + i.ToString(CultureInfo.InvariantCulture);
            _ = writer.BeginVar(local).Append(AppendSegmentRead(parent, path[i])).EndStatement()
                .BeginIf().Append(local).Append(" == null").CloseCondition()
                .Line("return;")
                .CloseBlock()
                .BlankLine();

            parent = local;
        }

        _ = writer.BeginIf().Append($"{GeneratedTypeNames.EqualityComparer}<")
            .Append(leaf.PropertyTypeFullName).Append(">.Default.Equals(")
            .Append(parent).Append('.').Append(leaf.PropertyName).Append(", ")
            .Append(valueExpression).Append(')').CloseCondition()
            .Line("return;")
            .CloseBlock()
            .BlankLine()
            .Append(parent).Append('.').Append(leaf.PropertyName)
            .Append(" = ").Append(valueExpression).EndStatement();
    }

    /// <summary>Writes a human-readable dotted property path, such as <c>Address.City</c>.</summary>
    /// <param name="writer">The writer, part way through a line.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>The writer, for chaining.</returns>
    internal static SourceWriter AppendPropertyPath(SourceWriter writer, EquatableArray<PropertyPathSegment> path)
    {
        for (var i = 0; i < path.Length; i++)
        {
            if (i > 0)
            {
                _ = writer.Append('.');
            }

            _ = writer.Append(path[i].PropertyName);
        }

        return writer;
    }

    /// <summary>Builds a human-readable dotted property path string for comments.</summary>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted string like "Address.City".</returns>
    internal static string BuildPropertyPathString(EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder(path.Length * PerPathSegmentCapacity);
        for (var i = 0; i < path.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append('.');
            }

            _ = sb.Append(path[i].PropertyName);
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>
    /// Computes a path suffix for CallerFilePath dispatch matching.
    /// Takes the last 2 path segments (e.g., "ViewModels/MyViewModel.cs").
    /// </summary>
    /// <param name="filePath">The full caller file path.</param>
    /// <returns>The last 2 path segments normalized with forward slashes.</returns>
    internal static string ComputePathSuffix(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        // Normalize separators (skip allocation on Unix paths that have no backslashes)
        if (filePath.IndexOf('\\') >= 0)
        {
            filePath = filePath.Replace('\\', '/');
        }

        // Take the last 2 segments (e.g., "src/MyFile.cs" or "ViewModels/MyViewModel.cs")
        var lastSlash = filePath.LastIndexOf('/');
        if (lastSlash < 0)
        {
            return filePath;
        }

        var secondLastSlash = filePath.LastIndexOf('/', lastSlash - 1);
        return secondLastSlash < 0 ? filePath : filePath.Substring(secondLastSlash + 1);
    }

    /// <summary>Escapes a string for embedding in a C# string literal.</summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    internal static string EscapeString(string value)
    {
        // Fast path: most lambda expressions contain no backslashes or quotes
        var needsEscape = false;
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (character is not ('\\' or '"'))
            {
                continue;
            }

            needsEscape = true;
            break;
        }

        if (!needsEscape)
        {
            return value;
        }

        var sb = new PooledStringBuilder(value.Length + EscapeOverheadCapacity);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (character == '\\')
            {
                _ = sb.Append("\\\\");
            }
            else if (character == '"')
            {
                _ = sb.Append("\\\"");
            }
            else
            {
                _ = sb.Append(character);
            }
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Hands finished source to the compilation, retargeted onto the consumer's runtime flavour.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="hintName">The generated file name.</param>
    /// <param name="source">The generated source, written against the lean runtime library.</param>
    /// <param name="features">The consumer compilation's snapshot, naming the flavour.</param>
    /// <remarks>
    /// Every generated file goes out through here so no emitter can forget the retargeting, which would only
    /// show up as generated code that does not compile for consumers of the System.Reactive flavour. A file that
    /// intercepts call sites also gets its own file-local declaration of the interception attribute here.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddGeneratedSource(
        in SourceProductionContext context,
        string hintName,
        string source,
        in LanguageFeatures features) =>
        context.AddSource(
            hintName,
            RuntimeFlavourRewriter.Retarget(features.SupportsInterceptors ? InterceptorEmitter.AppendAttributeDeclaration(source) : source, features));

    /// <summary>
    /// Writes one optional expression parameter of a generated overload's parameter list, mirroring the one the
    /// runtime stub declares for the same argument.
    /// </summary>
    /// <param name="writer">The writer, inside the parameter list.</param>
    /// <param name="sourceParameterName">The parameter whose expression text this one captures.</param>
    /// <param name="expressionParameterName">The name of the expression parameter itself.</param>
    /// <param name="withAttribute">
    /// Whether to attribute the parameter so the compiler fills it in. Off below C# 10, where the compiler would
    /// not populate it: the parameter is then inert and only present so the parameter lists match, which is what
    /// lets this concrete overload win against the generic stub instead of tying with it.
    /// </param>
    internal static void AppendExpressionParameter(
        SourceWriter writer,
        string sourceParameterName,
        string expressionParameterName,
        bool withAttribute)
    {
        if (withAttribute)
        {
            _ = writer.Append('[')
                .Append(GeneratedTypeNames.CallerArgumentExpression)
                .Append("(\"")
                .Append(sourceParameterName)
                .Append("\")] ");
        }

        _ = writer.Append("string ").Append(expressionParameterName).Line(" = \"\",");
    }

    /// <summary>Writes the caller-info parameters every runtime stub ends with, closing the parameter list.</summary>
    /// <param name="writer">The writer, inside the parameter list.</param>
    /// <remarks>
    /// Emitted by the interceptors as well as the dispatch overloads. The stub declares them, so both have to:
    /// an overload that omits them is merely applicable rather than better, and an interceptor that omits them
    /// is refused as a signature that is not the intercepted method's.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendCallerInfoParameters(SourceWriter writer) =>
        writer.Parameter(CallerFilePathParameter).LastParameter(CallerLineNumberParameter);

    /// <summary>Writes the standard auto-generated file header and opens the extension partial class.</summary>
    /// <param name="writer">The writer, at the start of the file.</param>
    /// <param name="features">
    /// The consumer compilation's language-feature and generation-option snapshot. Controls whether the
    /// <c>// &lt;auto-generated/&gt;</c> + <c>#pragma warning disable</c> markers and the <c>#nullable enable</c>
    /// directive are emitted.
    /// </param>
    /// <remarks>Leaves the writer inside the class, at the level its members are written at.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtensionClassHeader(SourceWriter writer, in LanguageFeatures features) =>
        OpenGeneratedClass(
            writer.FileHeader(features.EmitGeneratedCodeMarkers, features.SupportsNullable)
                .BlankLine()
                .Using("System")
                .BlankLine()
                .OpenNamespace(features.GeneratedNamespace),
            features);

    /// <summary>Opens the partial class every generated file of this assembly adds to.</summary>
    /// <param name="writer">The writer, inside the generated namespace.</param>
    /// <param name="features">The consumer compilation's snapshot, which names the class.</param>
    /// <remarks>Leaves the writer inside the class, at the level its members are written at.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void OpenGeneratedClass(SourceWriter writer, in LanguageFeatures features) =>
        writer.Append("internal static partial class ").Line(features.GeneratedClassName).OpenBlock();

    /// <summary>Closes the extension partial class and its namespace.</summary>
    /// <param name="writer">The writer, at the level the class's members are written at.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtensionClassFooter(SourceWriter writer) => writer.CloseBlock().CloseBlock();

    /// <summary>Opens a consumer type's partial declaration: its namespace, then each enclosing type, outermost first.</summary>
    /// <param name="writer">The writer, at the start of a line outside any namespace.</param>
    /// <param name="declaration">The partial declarations that reach the type.</param>
    /// <remarks>Leaves the writer inside the innermost type, at the level its members are written at.</remarks>
    internal static void OpenPartialDeclaration(SourceWriter writer, PartialTypeDeclaration declaration)
    {
        if (declaration.Namespace is { } ns)
        {
            _ = writer.OpenNamespace(ns);
        }

        var headers = declaration.TypeHeaders;
        for (var i = 0; i < headers.Length; i++)
        {
            _ = writer.OpenType(headers[i]);
        }
    }

    /// <summary>Closes what <see cref="OpenPartialDeclaration"/> opened.</summary>
    /// <param name="writer">The writer, at the innermost type's member level.</param>
    /// <param name="declaration">The partial declarations that reach the type.</param>
    internal static void ClosePartialDeclaration(SourceWriter writer, PartialTypeDeclaration declaration)
    {
        var blocks = declaration.TypeHeaders.Length + (declaration.Namespace is null ? 0 : 1);
        for (var i = 0; i < blocks; i++)
        {
            _ = writer.CloseBlock();
        }
    }

    /// <summary>
    /// Computes a stable method suffix based on source type, caller file path, caller line number,
    /// and an additional discriminator (typically expression text).
    /// Uses a deterministic hash (FNV-1a) so that method names remain stable across compilations
    /// and do not shift when new invocations are added above or below.
    /// </summary>
    /// <param name="sourceType">The fully qualified source type name.</param>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="discriminator">Additional discriminator for uniqueness (e.g., expression text).</param>
    /// <returns>A 16-character uppercase hex string suitable for use as a method name suffix.</returns>
    internal static string ComputeStableMethodSuffix(
        string sourceType,
        string callerFilePath,
        int callerLineNumber,
        string discriminator = "")
    {
        unchecked
        {
            var hash = HashSeed;
            hash = (hash * HashMultiplier) + StableStringHash(sourceType);
            hash = (hash * HashMultiplier) + StableStringHash(callerFilePath);
            hash = (hash * HashMultiplier) + callerLineNumber;
            hash = (hash * HashMultiplier) + StableStringHash(discriminator);
            return (hash & long.MaxValue).ToString("X16");
        }
    }

    /// <summary>
    /// Computes the same stable method suffix as <see cref="ComputeStableMethodSuffix(string, string, int, string)"/>
    /// would for a discriminator built by joining <paramref name="discriminatorParts"/> with <c>'|'</c>, without
    /// allocating that joined string first. Callers such as <c>WhenAnyCodeGenerator</c> hash a call site's
    /// expression texts once per invocation and would otherwise pay for a throwaway <c>string.Join</c> only to
    /// re-walk its characters inside <see cref="StableStringHash"/>.
    /// </summary>
    /// <param name="sourceType">The fully qualified source type name.</param>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="discriminatorParts">The discriminator's parts, hashed as if joined with <c>'|'</c>.</param>
    /// <returns>A 16-character uppercase hex string suitable for use as a method name suffix.</returns>
    internal static string ComputeStableMethodSuffix(
        string sourceType,
        string callerFilePath,
        int callerLineNumber,
        EquatableArray<string> discriminatorParts)
    {
        unchecked
        {
            var hash = HashSeed;
            hash = (hash * HashMultiplier) + StableStringHash(sourceType);
            hash = (hash * HashMultiplier) + StableStringHash(callerFilePath);
            hash = (hash * HashMultiplier) + callerLineNumber;
            hash = (hash * HashMultiplier) + StableJoinedStringHash(discriminatorParts, '|');
            return (hash & long.MaxValue).ToString("X16");
        }
    }

    /// <summary>Finds a <see cref="ClassBindingInfo"/> by fully qualified type name.</summary>
    /// <param name="allClasses">All detected class binding infos.</param>
    /// <param name="fullyQualifiedName">The fully qualified name to match.</param>
    /// <returns>The matching class info, or null if not found.</returns>
    internal static ClassBindingInfo? FindClassInfo(
        ImmutableArray<ClassBindingInfo> allClasses,
        string fullyQualifiedName)
    {
        for (var i = 0; i < allClasses.Length; i++)
        {
            if (allClasses[i].FullyQualifiedName == fullyQualifiedName)
            {
                return allClasses[i];
            }
        }

        return null;
    }

    /// <summary>Resolves the observed type's binding info, falling back to what the property path carries.</summary>
    /// <param name="allClasses">All detected class binding infos.</param>
    /// <param name="fullyQualifiedName">The observed type's fully qualified name.</param>
    /// <param name="path">The property path, whose first segment names the type that declares it.</param>
    /// <returns>The observed type's binding info, or null when nothing resolved it.</returns>
    /// <remarks>
    /// The declaration scan only sees types the consumer writes, so a type from a referenced assembly is absent
    /// from it. The path's first segment was built from the property's own symbol and carries the mechanism
    /// whatever assembly declares it, which is what keeps a referenced view model observable rather than
    /// silently reduced to a single read.
    /// </remarks>
    internal static ClassBindingInfo? ResolveObservedTypeInfo(
        ImmutableArray<ClassBindingInfo> allClasses,
        string fullyQualifiedName,
        EquatableArray<PropertyPathSegment> path) =>
        FindClassInfo(allClasses, fullyQualifiedName)
            ?? (path.Length > 0 ? path[0].DeclaringTypeInfo : null);

    /// <summary>Computes a deterministic hash for a string using FNV-1a. Unlike <see cref="string.GetHashCode()"/>, this is stable across processes and .NET versions.</summary>
    /// <param name="s">The string to hash.</param>
    /// <returns>A deterministic 32-bit hash code.</returns>
    internal static int StableStringHash(string s)
    {
        if (s is null)
        {
            return 0;
        }

        unchecked
        {
            var hash = (int)FnvOffsetBasis;
            for (var i = 0; i < s.Length; i++)
            {
                hash = (hash ^ s[i]) * FnvPrime;
            }

            return hash;
        }
    }

    /// <summary>
    /// Returns the conditional keyword for an if/else-if chain based on the loop index.
    /// The first iteration (index 0) emits <c>"if"</c>; subsequent iterations emit <c>"else if"</c>.
    /// </summary>
    /// <param name="index">The zero-based loop index.</param>
    /// <returns><c>"if"</c> when <paramref name="index"/> is 0; otherwise <c>"else if"</c>.</returns>
    internal static string ConditionKeyword(int index) =>
        index == 0 ? "if" : "else if";

    /// <summary>Drops the call sites that a dispatch keyed on expression text cannot tell apart.</summary>
    /// <typeparam name="T">The per-call-site model this API detects.</typeparam>
    /// <param name="invocations">The call sites sharing one generated overload.</param>
    /// <param name="dispatchKey">Produces the expression text a call site is dispatched on.</param>
    /// <returns>One call site per distinct key, or the original array when they were all distinct.</returns>
    /// <remarks>
    /// Expression-text dispatch keys on the lambdas as written, so call sites that spell them the same way all
    /// produce the same condition: the first wins and every later one is unreachable, yet each still drags its
    /// own generated method along. Calling an API on the same properties from more than one place is ordinary,
    /// so that dead weight scales with the consumer rather than staying a curiosity.
    /// Dropping the later ones rather than merging them is sound because a group already fixes the types
    /// involved, so a shared key means a shared set of property paths and an identical body.
    /// Only expression-text dispatch may do this - file-and-line dispatch reaches each call site separately and
    /// needs every one of them, or the collapsed sites fall through to the stub's runtime throw.
    /// </remarks>
    internal static T[] CollapseIndistinguishableCallSites<T>(T[] invocations, Func<T, string> dispatchKey)
    {
        if (invocations.Length < 2)
        {
            return invocations;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var kept = new List<T>(invocations.Length);

        for (var i = 0; i < invocations.Length; i++)
        {
            if (seen.Add(dispatchKey(invocations[i])))
            {
                kept.Add(invocations[i]);
            }
        }

        return kept.Count == invocations.Length ? invocations : [.. kept];
    }

    /// <summary>
    /// Collapses a group to one call site per distinct dispatch key, the same way as
    /// <see cref="CollapseIndistinguishableCallSites{T}(T[], Func{T, string})"/>, but for callers whose key is
    /// already an <see cref="EquatableArray{T}"/> of strings - typically a call site's expression texts. Comparing
    /// the array's cached structural hash and equality avoids building a joined string per call site solely to
    /// dedupe it.
    /// </summary>
    /// <typeparam name="T">The invocation type being collapsed.</typeparam>
    /// <param name="invocations">The call sites sharing an overload.</param>
    /// <param name="dispatchKey">Projects a call site to the expression texts its dispatch condition compares.</param>
    /// <returns>The distinct call sites, in original order, or <paramref name="invocations"/> unchanged when none collapse.</returns>
    internal static T[] CollapseIndistinguishableCallSites<T>(T[] invocations, Func<T, EquatableArray<string>> dispatchKey)
    {
        if (invocations.Length < 2)
        {
            return invocations;
        }

        var seen = new HashSet<EquatableArray<string>>();
        var kept = new List<T>(invocations.Length);

        for (var i = 0; i < invocations.Length; i++)
        {
            if (seen.Add(dispatchKey(invocations[i])))
            {
                kept.Add(invocations[i]);
            }
        }

        return kept.Count == invocations.Length ? invocations : [.. kept];
    }

    /// <summary>Emits the tail of a generated overload as a call to the stub the overload displaces.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    /// <param name="methodName">The API being generated, which names the stub method to call.</param>
    /// <param name="typeArguments">The stub's type arguments, or empty to let them be inferred.</param>
    /// <param name="arguments">The arguments to forward, in the stub's parameter order.</param>
    /// <remarks>
    /// A generated overload wins overload resolution for every call site of its types, including the ones whose
    /// lambdas it could not read - an expression held in a variable, or built at run time. Generating an overload
    /// therefore takes the stub out of reach, so the overload has to end where the stub would have: at the runtime
    /// engine, for the APIs whose stub offers one. Naming the class makes it a static call rather than an
    /// extension call, so it binds to the stub instead of recursing into the overload emitting it.
    /// The type arguments are stated wherever a generated parameter carries the leaf type's nullable annotation
    /// while the selector alongside it does not, which is enough to defeat inference.
    /// </remarks>
    internal static void AppendStubFallbackCall(
        SourceWriter writer,
        string methodName,
        string typeArguments,
        string arguments)
    {
        _ = writer.BeginReturn().Append("global::").Append(Constants.SharedGeneratedNamespace).Append('.')
            .Append(Constants.StubExtensionClassName).Append('.').Append(methodName);

        if (typeArguments.Length > 0)
        {
            _ = writer.Append('<').Append(typeArguments).Append('>');
        }

        _ = writer.Append('(').Append(arguments).Line(");");
    }

    /// <summary>Emits a whole dispatch file: the extension class, and one overload per group of call sites.</summary>
    /// <typeparam name="TInvocation">The call-site model this API extracts.</typeparam>
    /// <typeparam name="TGroup">The group of call sites that share one overload.</typeparam>
    /// <param name="invocations">The detected call sites for this API.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="groupByTypeSignature">Collects the call sites into the groups that share an overload.</param>
    /// <param name="emitGroup">Emits the overload and the workers for one group, at the class's member level.</param>
    /// <returns>The generated source, or <see langword="null"/> when there are no call sites.</returns>
    /// <remarks>
    /// Every API's file has the same outline - header, a run of groups, footer - and differs only in how call
    /// sites group and what each group emits. Both are handed in, so the outline is written once.
    /// </remarks>
    internal static string? GenerateDispatchFile<TInvocation, TGroup>(
        ImmutableArray<TInvocation> invocations,
        in LanguageFeatures features,
        Func<ImmutableArray<TInvocation>, List<TGroup>> groupByTypeSignature,
        Action<SourceWriter, TGroup, LanguageFeatures> emitGroup)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var snapshot = features;
        var writer = SourceWriter.Rent(invocations.Length * PerInvocationBufferCapacity);
        AppendExtensionClassHeader(writer, snapshot);

        var groups = groupByTypeSignature(invocations);
        for (var g = 0; g < groups.Count; g++)
        {
            emitGroup(writer, groups[g], snapshot);
        }

        AppendExtensionClassFooter(writer);

        return writer.ToStringAndReturn();
    }

    /// <summary>Writes the documentation comment on a generated dispatch overload.</summary>
    /// <param name="writer">The writer, at the class's member level.</param>
    /// <param name="apiName">The binding API the overload stands in for.</param>
    /// <param name="sourceTypeFullName">The fully qualified type the binding reads from.</param>
    /// <param name="targetTypeFullName">The fully qualified type the binding writes to.</param>
    /// <param name="dispatchesOnExpressionText">Whether the overload keys on expression text rather than file and line.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendDispatchSummary(
        SourceWriter writer,
        string apiName,
        string sourceTypeFullName,
        string targetTypeFullName,
        bool dispatchesOnExpressionText) =>
        writer.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for ").Append(apiName)
            .Append(" from ").Append(sourceTypeFullName).Append(" to ").Append(targetTypeFullName).Line(".")
            .DocLine(dispatchesOnExpressionText
                ? "Uses CallerArgumentExpression for dispatch."
                : "Uses CallerFilePath + CallerLineNumber for dispatch.")
            .CloseSummary();

    /// <summary>Opens the branch that matches a call site by the text of both its selectors.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    /// <param name="index">The branch's position, which decides whether it opens with <c>if</c> or <c>else if</c>.</param>
    /// <param name="firstParameterName">The parameter holding the first selector's text.</param>
    /// <param name="firstExpressionText">The first selector as the call site spelled it.</param>
    /// <param name="secondParameterName">The parameter holding the second selector's text.</param>
    /// <param name="secondExpressionText">The second selector as the call site spelled it.</param>
    /// <remarks>Leaves the writer inside the branch's block.</remarks>
    internal static void AppendExpressionDispatchCondition(
        SourceWriter writer,
        int index,
        string firstParameterName,
        string firstExpressionText,
        string secondParameterName,
        string secondExpressionText)
    {
        _ = AppendExpressionTextTest(writer.BeginBranch(index), firstParameterName, firstExpressionText)
            .OpenContinuation()
            .Append("&& ");
        _ = AppendExpressionTextTest(writer, secondParameterName, secondExpressionText)
            .Outdent()
            .CloseCondition();
    }

    /// <summary>Writes the test that a captured expression parameter holds the text a call site spelled.</summary>
    /// <param name="writer">The writer, part way through a condition.</param>
    /// <param name="parameterName">The parameter holding the captured text.</param>
    /// <param name="expressionText">The text the call site spelled.</param>
    /// <returns>The writer, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter AppendExpressionTextTest(SourceWriter writer, string parameterName, string expressionText) =>
        writer.Append(parameterName).Append(ExpressionTextComparison).AppendEscaped(expressionText).Append('"');

    /// <summary>Opens the branch that matches a call site by the file and line it sits on.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    /// <param name="index">The branch's position, which decides whether it opens with <c>if</c> or <c>else if</c>.</param>
    /// <param name="callerLineNumber">The line the call site sits on.</param>
    /// <param name="pathSuffix">The tail of the path the call site's file ends with.</param>
    /// <remarks>Leaves the writer inside the branch's block.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendCallerInfoDispatchCondition(
        SourceWriter writer,
        int index,
        int callerLineNumber,
        string pathSuffix) =>
        AppendCallerFilePathTest(
                writer.BeginBranch(index).Append("callerLineNumber == ").Append(callerLineNumber)
                    .OpenContinuation()
                    .Append("&& "),
                pathSuffix)
            .Outdent()
            .CloseCondition();

    /// <summary>Writes a whole branch that matches a call site by its file and line and hands the binding to its worker.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    /// <param name="index">The branch's position in the overload, which decides whether it opens with <c>if</c> or <c>else if</c>.</param>
    /// <param name="callerLineNumber">The line the call site sits on.</param>
    /// <param name="callerFilePath">The file the call site sits in.</param>
    /// <param name="workerName">The generated method the branch dispatches to.</param>
    /// <param name="arguments">The argument list to forward, in the worker's own parameter order.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendCallerInfoDispatchBranch(
        SourceWriter writer,
        int index,
        int callerLineNumber,
        string callerFilePath,
        string workerName,
        string arguments)
    {
        AppendCallerInfoDispatchCondition(writer, index, callerLineNumber, ComputePathSuffix(callerFilePath));
        AppendDispatchReturn(writer, workerName, arguments);
    }

    /// <summary>Writes the call a matched branch hands the binding to, and closes the branch.</summary>
    /// <param name="writer">The writer, inside the branch's block.</param>
    /// <param name="workerName">The generated method the branch dispatches to.</param>
    /// <param name="arguments">The argument list to forward, in the worker's own parameter order.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendDispatchReturn(SourceWriter writer, string workerName, string arguments) =>
        writer.BeginReturn().Append(workerName).Append('(').Append(arguments).Line(");")
            .CloseBlock();

    /// <summary>Opens the branch that matches a call site by the text of each of its selectors.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    /// <param name="index">The branch's position, which decides whether it opens with <c>if</c> or <c>else if</c>.</param>
    /// <param name="selectorParameterPrefix">What the overload names its selectors before their index.</param>
    /// <param name="expressionTexts">The selectors as the call site spelled them.</param>
    /// <param name="count">How many of them the overload takes.</param>
    /// <remarks>Leaves the writer inside the branch's block.</remarks>
    internal static void AppendSelectorTextCondition(
        SourceWriter writer,
        int index,
        string selectorParameterPrefix,
        EquatableArray<string> expressionTexts,
        int count)
    {
        _ = writer.BeginBranch(index);

        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                _ = writer.Append(" && ");
            }

            _ = writer.Append(selectorParameterPrefix).Append(i + 1).Append(ExpressionParameterSuffix)
                .Append(ExpressionTextComparison).AppendEscaped(expressionTexts[i]).Append('"');
        }

        _ = writer.CloseCondition();
    }

    /// <summary>Opens the one-line branch that matches a call site by the file and line it sits on.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    /// <param name="index">The branch's position, which decides whether it opens with <c>if</c> or <c>else if</c>.</param>
    /// <param name="callerLineNumber">The line the call site sits on.</param>
    /// <param name="pathSuffix">The tail of the path the call site's file ends with.</param>
    /// <remarks>Leaves the writer inside the branch's block.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendInlineCallerInfoCondition(
        SourceWriter writer,
        int index,
        int callerLineNumber,
        string pathSuffix) =>
        AppendCallerFilePathTest(
                writer.BeginBranch(index).Append("callerLineNumber == ").Append(callerLineNumber).Append(" && "),
                pathSuffix)
            .CloseCondition();

    /// <summary>Writes the test that the caller's file ends with a path suffix, whichever separator the path uses.</summary>
    /// <param name="writer">The writer, part way through a condition.</param>
    /// <param name="pathSuffix">The last two segments of the call site's path, joined with a forward slash.</param>
    /// <returns>The writer, for chaining.</returns>
    /// <remarks>
    /// <c>callerFilePath</c> holds the path exactly as the compiler saw it: backslashes on Windows, and after a path map
    /// possibly a forward-slash root with backslashes below it. Only the one separator inside the two-segment suffix
    /// can differ, so the suffix is tested with each; comparing both spellings allocates nothing at run time, where
    /// normalising <c>callerFilePath</c> would allocate on every call.
    /// </remarks>
    internal static SourceWriter AppendCallerFilePathTest(SourceWriter writer, string pathSuffix)
    {
        var escaped = EscapeString(pathSuffix);
        var separator = escaped.IndexOf('/');
        return separator < 0
            ? writer.Append("callerFilePath.EndsWith(\"").Append(escaped).Append(OrdinalIgnoreCaseArgument)
            : writer.Append("(callerFilePath.EndsWith(\"").Append(escaped).Append(OrdinalIgnoreCaseArgument)
                .Append(" || callerFilePath.EndsWith(\"").Append(escaped, 0, separator).Append(@"\\").Append(escaped, separator + 1, escaped.Length - separator - 1)
                .Append(OrdinalIgnoreCaseArgument).Append(')');
    }

    /// <summary>Writes the throw that closes a binding dispatch overload when no call site matched, and closes the overload.</summary>
    /// <param name="writer">The writer, inside the overload's body.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendBindingDispatchFallthrough(SourceWriter writer) =>
        writer.Line($"throw new {GeneratedTypeNames.InvalidOperationException}(")
            .Indent()
            .Line("\"No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.\");")
            .Outdent()
            .CloseBlock();

    /// <summary>
    /// Computes the FNV-1a hash <see cref="StableStringHash(string)"/> would produce for <c>string.Join(separator, parts)</c>,
    /// by streaming each part's characters (and the separator between parts) through the same hash accumulator,
    /// so no joined string is ever allocated.
    /// </summary>
    /// <param name="parts">The strings that would have been joined.</param>
    /// <param name="separator">The separator that would have sat between them.</param>
    /// <returns>The same 32-bit hash <see cref="StableStringHash(string)"/> would return for the joined text.</returns>
    private static int StableJoinedStringHash(EquatableArray<string> parts, char separator)
    {
        unchecked
        {
            var hash = (int)FnvOffsetBasis;
            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    hash = (hash ^ separator) * FnvPrime;
                }

                var part = parts[i];
                for (var j = 0; j < part.Length; j++)
                {
                    hash = (hash ^ part[j]) * FnvPrime;
                }
            }

            return hash;
        }
    }
}
