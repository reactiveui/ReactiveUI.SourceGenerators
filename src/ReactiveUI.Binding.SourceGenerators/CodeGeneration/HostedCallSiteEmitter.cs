// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Writes the generated code for call sites whose types only the caller's own partial class can name, and the generic
/// interceptors that claim them.
/// </summary>
/// <remarks>
/// <para>
/// Each API writes these call sites exactly as it writes any other, into a file of its own. That file's members move
/// into a private class nested in the caller's partial class, where the private types are in scope. Their interception
/// attributes come off, because an interceptor has to sit in the generated namespace, and so does the receiver's
/// <c>this</c>, because an extension method cannot sit in a nested class.
/// </para>
/// <para>
/// The generated namespace then gets one generic interceptor per hosted method. It repeats the called method's
/// signature with its type parameters, so it names no private type. It forwards to an entry point in the caller's class,
/// which casts each argument to the type the call was closed over and calls the moved method.
/// </para>
/// </remarks>
internal static class HostedCallSiteEmitter
{
    /// <summary>The class, nested in the caller's class, that holds the moved members.</summary>
    internal const string BindingsClassName = "__ReactiveUIHostedBindings";

    /// <summary>The class, nested in the caller's class, that holds the entry points the interceptors call.</summary>
    internal const string EntriesClassName = "__ReactiveUIHostedEntries";

    /// <summary>The modifier that makes a method an extension method.</summary>
    private const string ReceiverModifier = "this ";

    /// <summary>Writes one API's file, with the call sites only the caller's class can name moved there.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="invocations">Every call site of the API.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="emit">The API's emitter, which writes a whole file for the call sites it is given.</param>
    /// <returns>The file's text, or null when the API claimed no call site.</returns>
    /// <remarks>
    /// A build that writes dispatch overloads has no interceptor to claim a hosted call site with, so those call
    /// sites are dropped and RXUIBIND015 fails the build at each of them.
    /// </remarks>
    internal static string? Compose<T>(
        ImmutableArray<T> invocations,
        in LanguageFeatures features,
        Func<ImmutableArray<T>, LanguageFeatures, string?> emit)
        where T : IClaimableCallSite
    {
        if (!AnyHosted(invocations))
        {
            return emit(invocations, features);
        }

        var unhosted = ImmutableArray.CreateBuilder<T>(invocations.Length);
        var hosts = new List<string>();
        var byHost = new Dictionary<string, ImmutableArray<T>.Builder>(StringComparer.Ordinal);
        for (var i = 0; i < invocations.Length; i++)
        {
            var invocation = invocations[i];
            if (invocation.Host is not { } host)
            {
                unhosted.Add(invocation);
                continue;
            }

            var key = GroupKey(host);
            if (!byHost.TryGetValue(key, out var group))
            {
                group = ImmutableArray.CreateBuilder<T>();
                byHost[key] = group;
                hosts.Add(key);
            }

            group.Add(invocation);
        }

        var main = unhosted.Count == 0 ? null : emit(unhosted.ToImmutable(), features);
        if (!features.SupportsInterceptors)
        {
            return main;
        }

        var writer = SourceWriter.Rent();
        var claimsAny = false;
        foreach (var hostName in hosts)
        {
            claimsAny |= AppendHost(writer, byHost[hostName].ToImmutable(), features, emit);
        }

        var hosted = writer.ToStringAndReturn();
        if (!claimsAny)
        {
            return main;
        }

        return main is null ? AppendFileHeader(features) + hosted : main + hosted;
    }

    /// <summary>Determines whether any call site is hosted in the caller's class.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="invocations">The call sites.</param>
    /// <returns><see langword="true"/> when at least one is.</returns>
    private static bool AnyHosted<T>(ImmutableArray<T> invocations)
        where T : IClaimableCallSite
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return false;
        }

        for (var i = 0; i < invocations.Length; i++)
        {
            if (invocations[i].Host is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Names the group of moved call sites that share one destination: the same class, generic over the same type parameters.</summary>
    /// <param name="call">The moved call.</param>
    /// <returns>The group's key.</returns>
    private static string GroupKey(HostedCall call)
    {
        var key = new PooledStringBuilder().Append(call.HostTypeFullName ?? string.Empty);
        foreach (var parameter in call.WrapperTypeParameters)
        {
            _ = key.Append('|').Append(parameter);
        }

        foreach (var clause in call.WrapperConstraints)
        {
            _ = key.Append('|').Append(clause);
        }

        return key.ToStringAndReturn();
    }

    /// <summary>Settles where one group's moved code is declared.</summary>
    /// <param name="call">A call of the group.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <returns>The destination.</returns>
    /// <remarks>
    /// A call that only needs to be generic stays in the generated class. Its entry class is generic over the caller's
    /// type parameters, and named for them, so two groups with different ones never share a declaration.
    /// </remarks>
    private static Destination DestinationOf(HostedCall call, in LanguageFeatures features)
    {
        var host = call.HostTypeFullName ?? $"global::{features.GeneratedNamespace}.{features.GeneratedClassName}";
        var declaration = call.Declaration
            ?? new PartialTypeDeclaration(features.GeneratedNamespace, new([$"static partial class {features.GeneratedClassName}"]));
        if (!call.IsGeneric)
        {
            return new(declaration, EntriesClassName, $"{host}.{EntriesClassName}");
        }

        var entries = $"{EntriesClassName}_{CodeGeneratorHelpers.ComputeStableMethodSuffix(host, string.Empty, 0, GroupKey(call))}";
        return new(
            declaration,
            $"{entries}<{string.Join(", ", call.WrapperTypeParameters)}>",
            $"{host}.{entries}<{string.Join(", ", call.WrapperTypeArguments)}>");
    }

    /// <summary>Writes the start of a file whose only members are hosted.</summary>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <returns>The file header and imports.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AppendFileHeader(in LanguageFeatures features) =>
        SourceWriter.Rent()
            .FileHeader(features.EmitGeneratedCodeMarkers, features.SupportsNullable)
            .BlankLine()
            .Using("System")
            .ToStringAndReturn();

    /// <summary>Writes one caller class's hosted members, their entry points, and the interceptors that reach them.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="writer">The writer, outside any namespace.</param>
    /// <param name="invocations">The call sites hosted in the class.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="emit">The API's emitter.</param>
    /// <returns><see langword="true"/> when at least one call site was claimed.</returns>
    private static bool AppendHost<T>(
        SourceWriter writer,
        ImmutableArray<T> invocations,
        in LanguageFeatures features,
        Func<ImmutableArray<T>, LanguageFeatures, string?> emit)
        where T : IClaimableCallSite
    {
        var source = emit(invocations, features);
        if (source is null || !HostedSourceReader.TryRead(source, features.GeneratedClassName, out var read))
        {
            return false;
        }

        var call = invocations[0].Host!;
        var destination = DestinationOf(call, features);
        var methods = GroupByMethod(invocations, read.Claims, destination.EntriesReference);
        if (methods.Count == 0)
        {
            return false;
        }

        AppendInterceptors(writer, methods, destination.EntriesReference, features);

        _ = writer.BlankLine();
        CodeGeneratorHelpers.OpenPartialDeclaration(writer, destination.Declaration);
        _ = writer.Line($"internal static partial class {destination.EntriesDeclaration}");
        if (call.WrapperConstraints.Length > 0)
        {
            _ = writer.Indent();
            foreach (var clause in call.WrapperConstraints)
            {
                _ = writer.Line(clause);
            }

            _ = writer.Outdent();
        }

        _ = writer.OpenBlock();
        foreach (var method in methods)
        {
            AppendEntry(writer, method);
        }

        _ = writer.OpenType($"private static partial class {BindingsClassName}");
        foreach (var line in read.Members)
        {
            _ = line.Length == 0 ? writer.BlankLine() : writer.Line(line);
        }

        _ = writer.CloseBlock().CloseBlock();
        CodeGeneratorHelpers.ClosePartialDeclaration(writer, destination.Declaration);

        if (read.Trailing.Length > 0)
        {
            _ = writer.BlankLine().Lines(read.Trailing);
        }

        return true;
    }

    /// <summary>Gathers the claimed call sites under the moved method each one reaches.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="invocations">The call sites hosted in one class.</param>
    /// <param name="claims">The method each claimed call site's data names, as read from the API's output.</param>
    /// <param name="entriesReference">The class the entry points are declared in, as the interceptors name it.</param>
    /// <returns>One entry per moved method and signature, in the order the API wrote them.</returns>
    private static List<HostedMethod> GroupByMethod<T>(
        ImmutableArray<T> invocations,
        Dictionary<string, string> claims,
        string entriesReference)
        where T : IClaimableCallSite
    {
        var methods = new List<HostedMethod>();
        var byKey = new Dictionary<string, HostedMethod>(StringComparer.Ordinal);
        for (var i = 0; i < invocations.Length; i++)
        {
            var invocation = invocations[i];
            var location = invocation.Interceptor;
            if (!location.IsAvailable || !claims.TryGetValue(location.Data!, out var methodName))
            {
                continue;
            }

            var call = invocation.Host!;
            var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(entriesReference, methodName, 0, SignatureKey(call));
            if (!byKey.TryGetValue(suffix, out var method))
            {
                method = new(methodName, suffix, call, []);
                byKey[suffix] = method;
                methods.Add(method);
            }

            method.Locations.Add(location);
        }

        return methods;
    }

    /// <summary>Writes a text that tells two signatures of the same moved method apart.</summary>
    /// <param name="call">The hosted call.</param>
    /// <returns>The key.</returns>
    private static string SignatureKey(HostedCall call)
    {
        var key = new PooledStringBuilder().Append(call.DeclaredReturnType);
        foreach (var parameter in call.Parameters)
        {
            _ = key.Append('|').Append(parameter.ClosedType);
        }

        return key.ToStringAndReturn();
    }

    /// <summary>Writes the generic interceptors that claim one class's hosted call sites.</summary>
    /// <param name="writer">The writer, outside any namespace.</param>
    /// <param name="methods">The moved methods and the call sites that reach each.</param>
    /// <param name="entriesReference">The class the entry points are declared in, as the interceptors name it.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void AppendInterceptors(
        SourceWriter writer,
        List<HostedMethod> methods,
        string entriesReference,
        in LanguageFeatures features)
    {
        _ = writer.BlankLine().OpenNamespace(features.GeneratedNamespace);
        CodeGeneratorHelpers.OpenGeneratedClass(writer, features);

        foreach (var method in methods)
        {
            foreach (var location in method.Locations)
            {
                InterceptorEmitter.AppendAttribute(writer, location);
            }

            var call = method.Call;
            _ = writer.Append("internal static ").Append(call.DeclaredReturnType).Append(" __InterceptHosted_").Append(method.Suffix);
            AppendSignature(writer, call, ReceiverModifier);
            _ = writer.Indent()
                .Append("=> ").Append(entriesReference).Append(".__Entry_").Append(method.Suffix);
            AppendTypeArguments(writer, call);
            _ = writer.Append('(');
            for (var i = 0; i < call.Parameters.Length; i++)
            {
                _ = (i == 0 ? writer : writer.Append(", ")).Append(call.Parameters[i].Name);
            }

            _ = writer.Line(");").Outdent().BlankLine();
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(writer);
    }

    /// <summary>Writes the entry point an interceptor calls, which casts to the closed types and calls the moved method.</summary>
    /// <param name="writer">The writer, inside the entries class.</param>
    /// <param name="method">The moved method.</param>
    private static void AppendEntry(SourceWriter writer, HostedMethod method)
    {
        var call = method.Call;
        _ = writer.Append("internal static ").Append(call.DeclaredReturnType).Append(" __Entry_").Append(method.Suffix);
        AppendSignature(writer, call, string.Empty);
        _ = writer.Indent().Append("=> ");
        if (call.ReturnNeedsCast)
        {
            _ = writer.Append('(').Append(call.DeclaredReturnType).Append(")(object)");
        }

        _ = writer.Append(BindingsClassName).Append('.').Append(method.MethodName).Append('(');
        for (var i = 0; i < call.Parameters.Length; i++)
        {
            var parameter = call.Parameters[i];
            if (i > 0)
            {
                _ = writer.Append(", ");
            }

            _ = parameter.NeedsCast
                ? writer.Append('(').Append(parameter.ClosedType).Append(")(object)").Append(parameter.Name)
                : writer.Append(parameter.Name);
        }

        _ = writer.Line(");").Outdent().BlankLine();
    }

    /// <summary>Writes a hosted method's type parameters, parameters and constraints, as the called method declares them.</summary>
    /// <param name="writer">The writer, just after the method's name.</param>
    /// <param name="call">The hosted call.</param>
    /// <param name="receiverModifier">What the first parameter starts with: <c>this </c> for an interceptor, nothing otherwise.</param>
    private static void AppendSignature(SourceWriter writer, HostedCall call, string receiverModifier)
    {
        AppendTypeArguments(writer, call);
        _ = writer.OpenParameterList();
        for (var i = 0; i < call.Parameters.Length; i++)
        {
            var parameter = call.Parameters[i];
            _ = writer.Append(i == 0 ? receiverModifier : string.Empty).Append(parameter.DeclaredType).Append(' ').Append(parameter.Name);
            _ = i == call.Parameters.Length - 1 ? writer.Line(")").Outdent() : writer.Line(",");
        }

        if (call.Constraints.Length == 0)
        {
            return;
        }

        _ = writer.Indent();
        foreach (var clause in call.Constraints)
        {
            _ = writer.Line(clause);
        }

        _ = writer.Outdent();
    }

    /// <summary>Writes the called method's type parameters, in angle brackets.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="call">The hosted call.</param>
    private static void AppendTypeArguments(SourceWriter writer, HostedCall call)
    {
        if (call.TypeParameters.Length == 0)
        {
            return;
        }

        _ = writer.Append('<');
        for (var i = 0; i < call.TypeParameters.Length; i++)
        {
            _ = (i == 0 ? writer : writer.Append(", ")).Append(call.TypeParameters[i]);
        }

        _ = writer.Append('>');
    }

    /// <summary>Where one group of moved call sites is declared.</summary>
    /// <param name="Declaration">The partial declarations that reach that class.</param>
    /// <param name="EntriesDeclaration">The entry class's name as it is declared, with its type parameters.</param>
    /// <param name="EntriesReference">The entry class as the interceptors name it, with the type arguments they hand on.</param>
    private sealed record Destination(PartialTypeDeclaration Declaration, string EntriesDeclaration, string EntriesReference);

    /// <summary>One moved method, one signature of it, and every call site that reaches it.</summary>
    /// <param name="MethodName">The name the API gave the method.</param>
    /// <param name="Suffix">The name suffix of the interceptor and the entry point.</param>
    /// <param name="Call">The signature of the method the call sites call.</param>
    /// <param name="Locations">The call sites.</param>
    private sealed record HostedMethod(string MethodName, string Suffix, HostedCall Call, List<InterceptorLocation> Locations);
}
