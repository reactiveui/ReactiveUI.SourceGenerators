// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates the typed overloads, interceptors and binding methods for <c>BindTo</c> invocations.</summary>
internal static class BindToCodeGenerator
{
    /// <summary>The worker parameter naming the object a write lands on.</summary>
    private const string TargetParameterName = "target";

    /// <summary>The worker parameter naming the source observable.</summary>
    private const string SourceParameterName = "source";

    /// <summary>Generates concrete typed overloads and binding methods for <c>BindTo</c> invocations.</summary>
    /// <param name="invocations">All detected <c>BindTo</c> invocations.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<BindToInvocationInfo> invocations,
        in LanguageFeatures features)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = SourceWriter.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, features);

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = features.CollapsesIndistinguishableCallSites
                ? groups[g] with
                {
                    Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                        groups[g].Invocations,
                        static x => x.TargetExpressionText),
                }
                : groups[g];

            if (features.SupportsInterceptors)
            {
                GenerateInterceptors(sb, group, in features);
            }
            else
            {
                GenerateConcreteOverload(sb, group, supportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);
            }

            _ = sb.BlankLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.TargetTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    inv.TargetExpressionText);
                GenerateBindToMethod(sb, inv, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);

        return sb.ToStringAndReturn();
    }

    /// <summary>
    /// Groups <c>BindTo</c> invocations by source value type, target type, target property type, and
    /// overload shape (conversion hint / converter override), so each group produces one concrete overload.
    /// </summary>
    /// <param name="invocations">The collection of <c>BindTo</c> invocation details to be grouped.</param>
    /// <returns>A list of grouped invocations, where each group shares the same overload signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<BindToTypeGroup> GroupByTypeSignature(ImmutableArray<BindToInvocationInfo> invocations) =>
        SignatureGrouping.Group(
            invocations,
            static (key, inv) => _ = key
                .Append(inv.SourceValueTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName).Append('|')
                .Append(inv.TargetPropertyTypeFullName).Append('|')
                .Append(inv.HasConversionHint).Append('|')
                .Append(inv.HasConverterOverride),
            static (first, members) => new BindToTypeGroup(
                first.SourceValueTypeFullName,
                first.TargetTypeFullName,
                first.TargetPropertyTypeFullName,
                first.TargetPropertyIsReferenceType,
                first.HasConversionHint,
                first.HasConverterOverride,
                members));

    /// <summary>
    /// Generates the concrete typed <c>BindTo</c> overload for a group, choosing the dispatch strategy
    /// based on whether CallerArgumentExpression is available.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is supported.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        SourceWriter sb,
        BindToTypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group, supportsNullable);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, supportsNullable, stubHasExpressionParameters);
        }
    }

    /// <summary>Generates a concrete <c>BindTo</c> overload that dispatches using CallerArgumentExpression.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(SourceWriter sb, BindToTypeGroup group, bool supportsNullable)
    {
        AppendOverloadOpen(sb, group, true, supportsNullable, true);

        var extraArguments = FormatExtraArgs(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb.BeginBranch(i), "propertyExpression", inv.TargetExpressionText)
                .CloseCondition();
            AppendWorkerInvocation(sb.BeginReturn(), BindToMethodSuffix(inv), extraArguments);
            _ = sb.CloseBlock();
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates a concrete <c>BindTo</c> overload that dispatches using CallerFilePath + CallerLineNumber.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group of invocations sharing one overload signature.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        SourceWriter sb,
        BindToTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        AppendOverloadOpen(sb, group, false, supportsNullable, stubHasExpressionParameters);

        var extraArguments = FormatExtraArgs(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                sb,
                i,
                inv.CallerLineNumber,
                CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            AppendWorkerInvocation(sb.BeginReturn(), BindToMethodSuffix(inv), extraArguments);
            _ = sb.CloseBlock();
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates the binding method that writes each source value to the target property on the target's thread.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="suffix">The stable method-name suffix.</param>
    internal static void GenerateBindToMethod(SourceWriter sb, BindToInvocationInfo inv, string suffix)
    {
        var extraParams = FormatExtraMethodParams(inv);

        // Direct assignment is only safe when the value type matches the property type and the caller did
        // not supply an explicit converter. A conversion hint alone is meaningless for identity assignment.
        var directAssign = inv.SourceValueTypeFullName == inv.TargetPropertyTypeFullName && !inv.HasConverterOverride;
        var sourceVariable = SourceParameterName;

        _ = sb.Append("private static ").Append(GeneratedTypeNames.IDisposable).Append(" __BindTo_").Append(suffix).Append('(')
            .Append(ObservableOf(inv.SourceValueTypeFullName)).Append(" source, ").Append(inv.TargetTypeFullName).Append(" target").Append(extraParams)
            .Line(")").OpenBlock()
            .BeginComment().Append("BindTo: observable -> ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.TargetPropertyPath).EndLine();

        EmitBindingHookGuard(sb, inv);
        _ = sb.BlankLine();

        if (inv.SetMethod is { } setMethod)
        {
            _ = BindingEmitterHelpers.AppendViewThreadCall(sb.BeginVar("__setSource"), "source", TargetParameterName, inv.TargetViewThreadInvoker).EndStatement();
            CollectionSetMethodEmitter.EmitSubscription(sb, new(TargetParameterName, inv.TargetPropertyPath, inv.SourceValueTypeFullName, setMethod, inv.TargetExpressionText, false), "__setSource");
            _ = sb.Return("__setSubscription").CloseBlock();
            return;
        }

        if (inv.Conversion is not null)
        {
            ConversionEmitter.EmitStage(
                sb,
                sourceVariable,
                "__convertedSource",
                inv.SourceValueTypeFullName,
                inv.TargetPropertyTypeFullName,
                inv.Conversion,
                new(inv.HasConversionHint ? "conversionHint" : "null", inv.HasConverterOverride ? "converterOverride" : "null"));
            sourceVariable = "__convertedSource";
            directAssign = true;
        }

        _ = BindingEmitterHelpers.AppendViewThreadCall(sb.BeginReturn().Append(BindingErrors).Append(".Subscribe("), sourceVariable, TargetParameterName, inv.TargetViewThreadInvoker)
            .Line(", value =>").OpenBlock();

        if (directAssign)
        {
            CodeGeneratorHelpers.AppendGuardedAssignment(sb, TargetParameterName, inv.TargetPropertyPath, "value");
        }
        else
        {
            _ = sb.BeginIf().Append(RuntimeBindingConverter).Append(".TryConvert<").Append(inv.SourceValueTypeFullName).Append(", ")
                .Append(inv.TargetPropertyTypeFullName).Append(">(value, ").Append(FormatConversionArguments(inv)).Append(", out var __converted)")
                .CloseCondition();
            CodeGeneratorHelpers.AppendGuardedAssignment(sb, TargetParameterName, inv.TargetPropertyPath, "__converted");
            _ = sb.CloseBlock();
        }

        _ = sb.CloseBlockInline().Append(", ").AppendQuoted(inv.TargetExpressionText).Line(");")
            .CloseBlock()
            .BlankLine();
    }

    /// <summary>Writes the conversion-hint and converter-override parameters of the concrete overload signature.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The binding type group.</param>
    internal static void AppendExtraParameters(SourceWriter sb, BindToTypeGroup group)
    {
        if (group.HasConversionHint)
        {
            _ = sb.Parameter("object conversionHint");
        }

        if (!group.HasConverterOverride)
        {
            return;
        }

        _ = sb.Append(IBindingTypeConverter).Line(" converterOverride,");
    }

    /// <summary>Formats the extra arguments forwarded from the concrete overload to the worker method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>An argument list fragment like ", conversionHint, converterOverride", or empty.</returns>
    internal static string FormatExtraArgs(BindToTypeGroup group)
    {
        var sb = new PooledStringBuilder();
        if (group.HasConversionHint)
        {
            _ = sb.Append(", conversionHint");
        }

        if (group.HasConverterOverride)
        {
            _ = sb.Append(", converterOverride");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Formats the extra parameters for the private worker method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>A parameter list fragment like ", object conversionHint, ... converterOverride", or empty.</returns>
    internal static string FormatExtraMethodParams(BindToInvocationInfo inv)
    {
        var sb = new PooledStringBuilder();
        if (inv.HasConversionHint)
        {
            _ = sb.Append(", object conversionHint");
        }

        if (inv.HasConverterOverride)
        {
            _ = sb.Append(", ").Append(IBindingTypeConverter).Append(" converterOverride");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Offers the target property path to registered hooks before a BindTo subscription is created.</summary>
    /// <param name="sb">The generated source builder.</param>
    /// <param name="inv">The binding invocation being emitted.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitBindingHookGuard(SourceWriter sb, BindToInvocationInfo inv) =>
        BindingEmitterHelpers.EmitBindingHookGuard(
            sb,
            SourceParameterName,
            default,
            TargetParameterName,
            inv.TargetPropertyPath,
            "OneWay",
            EmptyDisposableInstance);

    /// <summary>Emits one interceptor per generated binding, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(SourceWriter sb, BindToTypeGroup group, in LanguageFeatures features)
    {
        var extraArguments = FormatExtraArgs(group);
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, BindToMethodSuffix))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor);
            }

            _ = sb.Append("internal static ").Append(GeneratedTypeNames.IDisposable).Append(" __Intercept_BindTo_")
                .Append(entry.Key).OpenParameterList();

            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            AppendWorkerInvocation(sb.Indent().Append("=> "), entry.Key, extraArguments);
            _ = sb.Outdent().BlankLine();
        }
    }

    /// <summary>Writes the summary and signature of a concrete overload, and opens its body.</summary>
    /// <param name="sb">The writer, at the class's member level; left inside the overload's body.</param>
    /// <param name="group">The group whose types the signature is written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameter.</param>
    private static void AppendOverloadOpen(
        SourceWriter sb,
        BindToTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        _ = sb.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for BindTo of ").Append(IObservable).Append("&lt;")
            .Append(group.SourceValueTypeFullName).Append("&gt; to ").Append(group.TargetTypeFullName).Line(".")
            .DocLine(dispatchesOnExpressionText
                ? "Uses CallerArgumentExpression for dispatch."
                : "Uses CallerFilePath + CallerLineNumber for dispatch.")
            .CloseSummary()
            .Append("public static ").Append(GeneratedTypeNames.IDisposable).Append(" BindTo").OpenParameterList();

        AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

        _ = sb.OpenBlock();
    }

    /// <summary>Writes the stub's parameter list, which the overload and the interceptor both have to match exactly.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameter.</param>
    private static void AppendParameterList(
        SourceWriter sb,
        BindToTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var targetPropType = CodeGeneratorHelpers.NullableSelectorType(
            group.TargetPropertyTypeFullName,
            group.TargetPropertyIsReferenceType,
            supportsNullable);

        _ = sb.Append("this ").Append(ObservableOf(group.SourceValueTypeFullName)).Line(" source,")
            .Append(CodeGeneratorHelpers.NullableSelectorType(group.TargetTypeFullName, true, supportsNullable)).Line(" target,")
            .Append(PropertyExpression(group.TargetTypeFullName, targetPropType)).Line(" property,");

        AppendExtraParameters(sb, group);

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "property", "propertyExpression", dispatchesOnExpressionText);
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Writes the call forwarding the bound stream and target on to the generated worker, ending the statement.</summary>
    /// <param name="sb">The writer, after the keyword or arrow the call sits behind.</param>
    /// <param name="methodSuffix">The stable suffix naming the worker.</param>
    /// <param name="extraArguments">The conversion-hint and converter-override arguments, if any.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendWorkerInvocation(SourceWriter sb, string methodSuffix, string extraArguments) =>
        sb.Append("__BindTo_").Append(methodSuffix).Append("(source, target").Append(extraArguments).Line(");");

    /// <summary>Names the generated binding a call site reaches.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable method-name suffix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BindToMethodSuffix(BindToInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.TargetTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.TargetExpressionText);

    /// <summary>Formats the conversion-hint and converter-override arguments handed to the runtime converter.</summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>The two arguments, comma separated.</returns>
    private static string FormatConversionArguments(BindToInvocationInfo inv) =>
        $"{(inv.HasConversionHint ? "conversionHint" : "null")}, {(inv.HasConverterOverride ? "converterOverride" : "null")}";

    /// <summary>Groups <c>BindTo</c> invocations sharing source value type, target type, target property type, and overload shape.</summary>
    /// <param name="SourceValueTypeFullName">The fully qualified observable value type.</param>
    /// <param name="TargetTypeFullName">The fully qualified target object type.</param>
    /// <param name="TargetPropertyTypeFullName">The fully qualified target property type.</param>
    /// <param name="TargetPropertyIsReferenceType">Whether that type is a reference type, which annotates the selector parameter.</param>
    /// <param name="HasConversionHint">Whether this group's overload takes a conversion hint.</param>
    /// <param name="HasConverterOverride">Whether this group's overload takes an explicit converter.</param>
    /// <param name="Invocations">All invocations sharing this overload signature.</param>
    internal sealed record BindToTypeGroup(
        string SourceValueTypeFullName,
        string TargetTypeFullName,
        string TargetPropertyTypeFullName,
        bool TargetPropertyIsReferenceType,
        bool HasConversionHint,
        bool HasConverterOverride,
        BindToInvocationInfo[] Invocations);
}
