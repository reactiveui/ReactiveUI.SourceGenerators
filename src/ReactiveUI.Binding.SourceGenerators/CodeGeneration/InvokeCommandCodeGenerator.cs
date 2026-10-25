// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates the typed overloads, interceptors and workers for <c>InvokeCommand</c> invocations.</summary>
internal static class InvokeCommandCodeGenerator
{
    /// <summary>The generated worker each dispatch branch hands the invocation to.</summary>
    private const string WorkerMethodPrefix = "__InvokeCommand_";

    /// <summary>The variable the observed command is assigned to inside a worker.</summary>
    private const string CommandVariable = "commandObs";

    /// <summary>The parameter carrying the text of the selector naming the command.</summary>
    private const string CommandExpressionParameter = "commandPropertyExpression";

    /// <summary>The objects a worker takes, in its own parameter order.</summary>
    private const string WorkerArguments = "source, target";

    /// <summary>The declaration of the parameter a worker takes its values from.</summary>
    private const string SourceParameter = " source,";

    /// <summary>Generates concrete typed overloads and workers for <c>InvokeCommand</c> invocations.</summary>
    /// <param name="invocations">All detected <c>InvokeCommand</c> invocations.</param>
    /// <param name="allClasses">All detected class binding info, for the observed type's mechanism.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot.</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<InvokeCommandInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Groups invocations by the types their overload declares.</summary>
    /// <param name="invocations">The <c>InvokeCommand</c> invocations to group.</param>
    /// <returns>A list of groups, each sharing one overload signature.</returns>
    /// <remarks>The selector is typed as the stub's ICommand whatever the property declares, so the command type is not part of the key.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<InvokeCommandTypeGroup> GroupByTypeSignature(
        ImmutableArray<InvokeCommandInvocationInfo> invocations) =>
        SignatureGrouping.Group(
            invocations,
            static (key, inv) => _ = key.Append(inv.SourceValueTypeFullName).Append('|').Append(inv.TargetTypeFullName),
            static (first, members) => new InvokeCommandTypeGroup(first.SourceValueTypeFullName, first.TargetTypeFullName, members));

    /// <summary>Emits one group: the way its call sites are reached, and one worker per distinct command path.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group being emitted.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        SourceWriter sb,
        InvokeCommandTypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var collapsible = features.CollapsesIndistinguishableCallSites;
        var emitted = collapsible
            ? group with
            {
                Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                    group.Invocations,
                    static x => x.CommandExpressionText),
            }
            : group;

        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, emitted, in features);
        }
        else
        {
            GenerateConcreteOverload(sb, emitted, in features);
        }

        _ = sb.BlankLine();

        // Call sites spelling the same selector against the same type share one worker.
        var emittedWorkers = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < emitted.Invocations.Length; i++)
        {
            var inv = emitted.Invocations[i];
            var suffix = WorkerSuffix(inv);
            if (!emittedWorkers.Add(suffix))
            {
                continue;
            }

            GenerateWorker(sb, inv, allClasses, suffix);
        }
    }

    /// <summary>Emits the concrete overload the call sites of one group resolve to.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group being emitted.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateConcreteOverload(
        SourceWriter sb,
        InvokeCommandTypeGroup group,
        in LanguageFeatures features)
    {
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;

        CodeGeneratorHelpers.AppendDispatchSummary(
            sb,
            Constants.InvokeCommandMethodName,
            ObservableOf(group.SourceValueTypeFullName),
            group.TargetTypeFullName,
            dispatchesOnExpressionText);

        _ = sb.Append("public static ").Append(GeneratedTypeNames.IDisposable).Append(' ')
            .Append(Constants.InvokeCommandMethodName).OpenParameterList();

        AppendParameterList(sb, group, dispatchesOnExpressionText, features.SupportsNullable, features.StubHasExpressionParameters);

        _ = sb.OpenBlock();

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            if (dispatchesOnExpressionText)
            {
                _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb.BeginBranch(i), CommandExpressionParameter, inv.CommandExpressionText)
                    .CloseCondition();
            }
            else
            {
                CodeGeneratorHelpers.AppendCallerInfoDispatchCondition(
                    sb,
                    i,
                    inv.CallerLineNumber,
                    CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            }

            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + WorkerSuffix(inv), WorkerArguments);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Emits one interceptor per worker, claiming every call site that reaches it.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group whose call sites are being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(
        SourceWriter sb,
        InvokeCommandTypeGroup group,
        in LanguageFeatures features)
    {
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, WorkerSuffix))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor);
            }

            _ = sb.Append("internal static ").Append(GeneratedTypeNames.IDisposable).Append(" __Intercept_")
                .Append(Constants.InvokeCommandMethodName).Append('_').Append(entry.Key).OpenParameterList();

            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            _ = sb.Indent()
                .Append("=> ").Append(WorkerMethodPrefix).Append(entry.Key).Append('(').Append(WorkerArguments).Line(");")
                .Outdent()
                .BlankLine();
        }
    }

    /// <summary>Writes the stub's parameter list, which the overload and the interceptor both have to match exactly.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameter.</param>
    private static void AppendParameterList(
        SourceWriter sb,
        InvokeCommandTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        // The stub's ICommand, whatever the command property is declared as.
        var commandType = supportsNullable ? $"{ICommand}?" : ICommand;

        _ = sb.Append("this ").Append(ObservableOf(group.SourceValueTypeFullName)).Line(SourceParameter)
            .Append(CodeGeneratorHelpers.NullableSelectorType(group.TargetTypeFullName, true, supportsNullable)).Line(" target,")
            .Append(PropertyExpression(group.TargetTypeFullName, commandType)).Line(" commandProperty,");

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(
                sb,
                "commandProperty",
                CommandExpressionParameter,
                dispatchesOnExpressionText);
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Emits the worker that observes the command and offers it each value.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="suffix">The stable method-name suffix for this worker.</param>
    private static void GenerateWorker(
        SourceWriter sb,
        InvokeCommandInvocationInfo inv,
        ImmutableArray<ClassBindingInfo> allClasses,
        string suffix)
    {
        var classInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(
            allClasses,
            inv.TargetTypeFullName,
            inv.CommandPropertyPath);

        _ = sb.Append("private static ").Append(GeneratedTypeNames.IDisposable).Append(' ').Append(WorkerMethodPrefix).Append(suffix).OpenParameterList()
            .Append(ObservableOf(inv.SourceValueTypeFullName)).Line(SourceParameter)
            .Append(inv.TargetTypeFullName).Line(" target)")
            .Outdent()
            .OpenBlock()
            .BeginComment().Append("InvokeCommand: values -> ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.CommandPropertyPath).EndLine()
            .If("target == null")
            .Return(EmptyDisposableInstance)
            .CloseBlock()
            .BlankLine();

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "target",
            inv.CommandPropertyPath,
            inv.CommandPropertyPath[inv.CommandPropertyPath.Length - 1].PropertyTypeFullName,
            classInfo,
            CommandVariable);

        _ = sb.BeginReturn().Append(CommandInvoker).Append(".Invoke(source, ").Append(CommandVariable).Line(");")
            .CloseBlock()
            .BlankLine();
    }

    /// <summary>Names the worker a call site reaches.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable method-name suffix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string WorkerSuffix(InvokeCommandInvocationInfo inv) =>
        // No file or line, so call sites spelling the same selector against the same type share a worker.
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.TargetTypeFullName,
            string.Empty,
            0,
            inv.CommandExpressionText);

    /// <summary>Groups <c>InvokeCommand</c> invocations sharing the observed value type and the target type.</summary>
    /// <param name="SourceValueTypeFullName">The fully qualified observable value type.</param>
    /// <param name="TargetTypeFullName">The fully qualified type declaring the command property.</param>
    /// <param name="Invocations">All invocations sharing this overload signature.</param>
    internal sealed record InvokeCommandTypeGroup(
        string SourceValueTypeFullName,
        string TargetTypeFullName,
        InvokeCommandInvocationInfo[] Invocations);
}
