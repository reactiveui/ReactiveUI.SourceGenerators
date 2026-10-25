// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads for WhenAnyObservable invocations.
/// Generates Switch (single), Merge (multi same-type), and CombineLatest (multi with selector)
/// patterns for observing properties that are themselves observables.
/// </summary>
internal static class WhenAnyObservableCodeGenerator
{
    /// <summary>What the generated overload names its selector parameters before their index.</summary>
    private const string SelectorParameterPrefix = "obs";

    /// <summary>The base name used to build emitted local variable identifiers for the raw observable property.</summary>
    private const string ObsPropertyVarName = "__obsProperty";

    /// <summary>Names the local holding one property's observable switched to its latest value, before its index.</summary>
    private const string SwitchedVarName = "__switched";

    /// <summary>The selector forwarded after the observed properties, where the overload takes one.</summary>
    private const string SelectorArgument = ", selector";

    /// <summary>Generates concrete typed overloads and observation methods for WhenAnyObservable invocations.</summary>
    /// <param name="invocations">All detected WhenAnyObservable invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<WhenAnyObservableInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            InterceptorEmitter.ClaimableBy(invocations, static x => x.GenericResult, features),
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Generates a concrete typed extension method overload with dispatch logic for WhenAnyObservable.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        SourceWriter sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;
        var hasSelector = first.HasSelector;

        _ = sb.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for WhenAnyObservable on ").Append(first.SourceTypeFullName).Line(".")
            .CloseSummary()
            .Append($"public static {IObservable}<").Append(first.ReturnTypeFullName).Append("> WhenAnyObservable").OpenParameterList();

        AppendParameterList(sb, first, supportsCallerArgExpr, supportsNullable, stubHasExpressionParameters);

        _ = sb.OpenBlock();

        EmitDispatchTable(sb, group, supportsCallerArgExpr, propCount, hasSelector);

        GenerateRuntimeFallback(sb, propCount, hasSelector);

        _ = sb.CloseBlock();
    }

    /// <summary>Generates an observation method for a single WhenAnyObservable invocation.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="suffix">The stable method suffix.</param>
    internal static void GenerateObservationMethod(
        SourceWriter sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo,
        string suffix)
    {
        _ = sb.Append($"private static {IObservable}<").Append(inv.ReturnTypeFullName).Append("> __WhenAnyObservable_")
            .Append(suffix);
        InterceptorEmitter.AppendResultTypeParameter(sb, inv.GenericResult);
        _ = sb.Append('(').Append(inv.SourceTypeFullName).Append(" obj");
        if (inv.HasSelector)
        {
            _ = sb.Append(", ").Append(GetSelectorType(inv)).Append(" selector");
        }

        _ = sb.Line(")").OpenBlock();

        if (inv.PropertyPaths.Length == 1)
        {
            GenerateSingleObservableSwitch(sb, inv, classInfo);
        }
        else if (!inv.HasSelector)
        {
            GenerateMultiObservableMerge(sb, inv, classInfo);
        }
        else
        {
            GenerateMultiObservableCombineLatest(sb, inv, classInfo);
        }

        _ = sb.CloseBlock()
            .BlankLine();
    }

    /// <summary>Generates a single-property Switch pattern: observe the IObservable property, switch to its latest value.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateSingleObservableSwitch(
        SourceWriter sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        ObservationCodeGenerator.GenerateObservedPropertyVariable(
            sb,
            inv.PropertyPaths[0],
            classInfo,
            false,
            ObsPropertyVarName);

        // Switch pattern: take the observable property value, replace null with Empty, and switch
        AppendSwitchToLatest(sb.BlankLine().BeginReturn(), ObsPropertyVarName, inv.InnerObservableTypeFullNames[0]);
    }

    /// <summary>Generates a multi-property Merge pattern: observe each IObservable property, switch each, then merge.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiObservableMerge(
        SourceWriter sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        EmitSwitchedObservables(sb, inv, classInfo);

        var isArray = inv.PropertyPaths.Length > 2;
        _ = sb.BeginReturn().Append("new ").Append(MergeSignal).Append('<').Append(inv.ReturnTypeFullName).Append(">(");
        if (isArray)
        {
            _ = sb.Append($"new {IObservable}<").Append(inv.ReturnTypeFullName).Append(">[] {");
        }

        _ = sb.OpenContinuation();
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append(SwitchedVarName).Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.Line(",");
            }
        }

        _ = sb.Line(isArray ? " });" : ");").Outdent();
    }

    /// <summary>
    /// Generates a multi-property CombineLatest pattern with selector: observe each IObservable property,
    /// switch each, then CombineLatest with the selector.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiObservableCombineLatest(
        SourceWriter sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        EmitSwitchedObservables(sb, inv, classInfo);

        _ = sb.BeginReturn();
        _ = inv.PropertyPaths.Length == 2
            ? sb.Append("new ").Append(CombineLatestSignal).Append('<')
                .Append(inv.InnerObservableTypeFullNames[0]).Append(", ").Append(inv.InnerObservableTypeFullNames[1]).Append(", ")
                .Append(inv.ReturnTypeFullName).Append(">(")
            : sb.Append($"{LinqExtensions}.CombineLatest(");

        _ = sb.OpenContinuation();
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append(SwitchedVarName).Append(i).Line(",");
        }

        _ = sb.Line("selector);").Outdent();
    }

    /// <summary>Gets the Func type signature for a WhenAnyObservable selector parameter.</summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>A fully qualified Func type string.</returns>
    internal static string GetSelectorType(WhenAnyObservableInvocationInfo inv)
    {
        var sb = new PooledStringBuilder().Append($"{Func}<");
        for (var i = 0; i < inv.InnerObservableTypeFullNames.Length; i++)
        {
            _ = sb.Append(inv.InnerObservableTypeFullNames[i]).Append(", ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToStringAndReturn();
    }

    /// <summary>Groups WhenAnyObservable invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <returns>A list of type groups.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<TypeGroup> GroupByTypeSignature(ImmutableArray<WhenAnyObservableInvocationInfo> invocations) =>
        SignatureGrouping.Group(invocations, AppendSignatureKey, static (first, members) => new TypeGroup(first, members));

    /// <summary>Writes the parts of a call site that decide its overload: the types and each inner observable's type.</summary>
    /// <param name="key">The key being built.</param>
    /// <param name="inv">The call site.</param>
    private static void AppendSignatureKey(PooledStringBuilder key, WhenAnyObservableInvocationInfo inv)
    {
        _ = key
            .Append(inv.SourceTypeFullName).Append('|')
            .Append(inv.ReturnTypeFullName).Append('|')
            .Append(inv.PropertyPaths.Length).Append('|')
            .Append(inv.HasSelector);
        for (var p = 0; p < inv.InnerObservableTypeFullNames.Length; p++)
        {
            _ = key.Append('|').Append(inv.InnerObservableTypeFullNames[p]);
        }
    }

    /// <summary>Emits one variable per observed property, each switched to the latest value its property holds.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <remarks>
    /// What the switched variables are then fed to is the difference between the shapes: merging them hands the
    /// caller whichever produced a value, combining them hands the caller a selector's view of all of them.
    /// </remarks>
    private static void EmitSwitchedObservables(
        SourceWriter sb,
        WhenAnyObservableInvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var rawVar = ObsPropertyVarName + i;

            ObservationCodeGenerator.GenerateObservedPropertyVariable(sb, inv.PropertyPaths[i], classInfo, false, rawVar);

            AppendSwitchToLatest(sb.BlankLine().Append("var ").Append(SwitchedVarName).Append(i).Append(" = "), rawVar, inv.InnerObservableTypeFullNames[i]);
            _ = sb.BlankLine();
        }
    }

    /// <summary>
    /// Writes the switch onto the latest observable a property holds, substituting an empty one while it holds
    /// none, and ends the statement.
    /// </summary>
    /// <param name="sb">The writer, after the declaration or <c>return</c> the switch is assigned by.</param>
    /// <param name="rawVar">The local observing the property that holds the observable.</param>
    /// <param name="innerType">The element type the property's observable carries.</param>
    private static void AppendSwitchToLatest(SourceWriter sb, string rawVar, string innerType) =>
        _ = sb.Append("new ").Append(SwitchMapSignal).Append('<').Append(ObservableOf(innerType)).Append(", ").Append(innerType)
            .Append(">(").Append(rawVar).Append(',')
            .OpenContinuation()
            .Append($"__obs => __obs ?? ({IObservable}<").Append(innerType)
            .Append($">){ImmutableEmptySignal}<").Append(innerType).Line(">.Instance);")
            .Outdent();

    /// <summary>Emits the overload and the observation methods for one group of call sites.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        SourceWriter sb,
        TypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var emitted = features.CollapsesIndistinguishableCallSites
            ? group with
            {
                Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                    group.Invocations,
                    static x => x.ExpressionTexts),
            }
            : group;

        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, emitted, in features);
        }
        else
        {
            GenerateConcreteOverload(
                sb,
                emitted,
                features.SupportsCallerArgExpr,
                features.SupportsNullable,
                features.StubHasExpressionParameters);
        }

        _ = sb.BlankLine();

        for (var i = 0; i < emitted.Invocations.Length; i++)
        {
            var inv = emitted.Invocations[i];
            GenerateObservationMethod(
                sb,
                inv,
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.SourceTypeFullName, inv.PropertyPaths[0]),
                ObservationMethodSuffix(inv));
        }
    }

    /// <summary>Emits one interceptor per generated observation, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(SourceWriter sb, TypeGroup group, in LanguageFeatures features)
    {
        var supportsCallerArgExpr = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(
            group.Invocations,
            static x => x.Interceptor,
            ObservationMethodSuffix))
        {
            var first = entry.Value[0];

            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor);
            }

            _ = sb.Append($"internal static {IObservable}<").Append(first.ReturnTypeFullName)
                .Append("> __Intercept_WhenAnyObservable_").Append(entry.Key);
            InterceptorEmitter.AppendTypeParameterList(sb, first.GenericResult);
            _ = sb.OpenParameterList();

            AppendParameterList(sb, first, supportsCallerArgExpr, supportsNullable, stubHasExpressionParameters);

            _ = sb.Indent()
                .Append("=> __WhenAnyObservable_").Append(entry.Key).Append("(objectToMonitor")
                .Append(first.HasSelector ? SelectorArgument : string.Empty).Line(");")
                .Outdent()
                .BlankLine();
        }
    }

    /// <summary>Writes the parameters a WhenAnyObservable member declares, closing the list.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="first">The invocation whose types the parameters are written from.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature: the
    /// overload only wins resolution against a candidate it is otherwise indistinguishable from, and an
    /// interceptor is refused outright unless its signature is the intercepted method's.
    /// </remarks>
    private static void AppendParameterList(
        SourceWriter sb,
        WhenAnyObservableInvocationInfo first,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var propCount = first.PropertyPaths.Length;

        _ = sb.Append("this ").Append(first.SourceTypeFullName).Line(" objectToMonitor,");

        for (var i = 0; i < propCount; i++)
        {
            _ = sb.Append(GeneratedSyntax.SelectorTypeOpen).Append(first.SourceTypeFullName).Append($", {IObservable}<")
                .Append(first.InnerObservableTypeFullNames[i]).Append(supportsNullable ? ">?" : ">")
                .Append(">> ").Append(SelectorParameterPrefix).Append(i + 1).Line(",");
        }

        if (first.HasSelector)
        {
            _ = sb.Append(GetSelectorType(first)).Line(" selector,");
        }

        if (stubHasExpressionParameters)
        {
            for (var i = 0; i < propCount; i++)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(
                    sb,
                    $"obs{i + 1}",
                    $"obs{i + 1}Expression",
                    supportsCallerArgExpr);
            }
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Ends the overload where the stub it displaces would have ended: at the runtime engine.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="propCount">The number of observed observable properties.</param>
    /// <param name="hasSelector">Whether the overload takes a selector.</param>
    /// <remarks>
    /// The type arguments are left to inference here. WhenAnyObservable states the element type carried by each
    /// observed <c>IObservable&lt;T&gt;</c> rather than the property's own type, and the generated parameters
    /// already spell that out, so inference reaches it without the generator taking the type name apart.
    /// </remarks>
    private static void GenerateRuntimeFallback(SourceWriter sb, int propCount, bool hasSelector)
    {
        var arguments = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);
        _ = arguments.Append("objectToMonitor");

        for (var i = 0; i < propCount; i++)
        {
            _ = arguments.Append(", obs").Append(i + 1);
        }

        if (hasSelector)
        {
            _ = arguments.Append(SelectorArgument);
        }

        CodeGeneratorHelpers.AppendStubFallbackCall(
            sb,
            Constants.WhenAnyObservableMethodName,
            string.Empty,
            arguments.ToStringAndReturn());
    }

    /// <summary>Emits the if/else-if dispatch table that routes each matched invocation to its generated method.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of observable expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    private static void EmitDispatchTable(
        SourceWriter sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        int propCount,
        bool hasSelector)
    {
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            if (supportsCallerArgExpr)
            {
                CodeGeneratorHelpers.AppendSelectorTextCondition(
                    sb,
                    i,
                    SelectorParameterPrefix,
                    inv.ExpressionTexts,
                    propCount);
            }
            else
            {
                CodeGeneratorHelpers.AppendInlineCallerInfoCondition(
                    sb,
                    i,
                    inv.CallerLineNumber,
                    CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            }

            _ = sb.BeginReturn().Append("__WhenAnyObservable_").Append(ObservationMethodSuffix(inv)).Append("(objectToMonitor")
                .Append(hasSelector ? SelectorArgument : string.Empty).Line(");")
                .CloseBlock();
        }
    }

    /// <summary>Names the generated observation method a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its method is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ObservationMethodSuffix(WhenAnyObservableInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.ExpressionTexts);

    /// <summary>Groups invocations by source type and observable type signature for overload generation.</summary>
    /// <param name="First">The first invocation in the group, used for type information.</param>
    /// <param name="Invocations">All invocations sharing the same type signature.</param>
    internal sealed record TypeGroup(
        WhenAnyObservableInvocationInfo First,
        WhenAnyObservableInvocationInfo[] Invocations);
}
