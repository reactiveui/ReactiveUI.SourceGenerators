// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads for WhenAny invocations.
/// Unlike WhenChanged/WhenAnyValue, WhenAny wraps each observed value in an
/// <c>ObservedChange&lt;TSender, T&gt;</c> before passing it to the user's selector function.
/// </summary>
internal static class WhenAnyCodeGenerator
{
    /// <summary>What the generated overload names its selector parameters before their index.</summary>
    private const string SelectorParameterPrefix = "property";

    /// <summary>Generates concrete typed overloads and observation methods for WhenAny invocations.</summary>
    /// <param name="invocations">All detected WhenAny invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            InterceptorEmitter.ClaimableBy(invocations, static x => x.GenericResult, features),
            features,
            ObservationCodeGenerator.GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>
    /// Generates a concrete typed extension method overload with dispatch logic for WhenAny.
    /// The selector parameter takes <c>IObservedChange&lt;TSender, T&gt;</c> values.
    /// </summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        SourceWriter sb,
        ObservationCodeGenerator.TypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;

        _ = sb.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for WhenAny on ").Append(first.SourceTypeFullName).Line(".")
            .CloseSummary()
            .Append($"public static {GeneratedTypeNames.IObservable}<").Append(first.ReturnTypeFullName).Append("> WhenAny").OpenParameterList();

        AppendParameterList(sb, first, supportsCallerArgExpr, supportsNullable, stubHasExpressionParameters);

        _ = sb.OpenBlock();

        EmitDispatchTable(sb, group, supportsCallerArgExpr, propCount);

        GenerateRuntimeFallback(sb, first, propCount);

        _ = sb.CloseBlock();
    }

    /// <summary>Ends the overload where the stub it displaces would have ended: at the runtime engine.</summary>
    /// <param name="sb">The writer, inside the overload's body.</param>
    /// <param name="first">The invocation whose types the whole group shares.</param>
    /// <param name="propCount">The number of observed properties.</param>
    /// <remarks>
    /// WhenAny states its projected type before the observed ones - <c>WhenAny&lt;TSender, TRet, T1...&gt;</c> -
    /// unlike the observation APIs, which put the projection last.
    /// </remarks>
    internal static void GenerateRuntimeFallback(SourceWriter sb, InvocationInfo first, int propCount)
    {
        var typeArguments = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);
        var arguments = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        _ = typeArguments.Append(first.SourceTypeFullName).Append(", ").Append(first.ReturnTypeFullName);
        _ = arguments.Append("objectToMonitor");

        for (var i = 0; i < propCount; i++)
        {
            var path = first.PropertyPaths[i];
            _ = typeArguments.Append(", ").Append(path[path.Length - 1].PropertyTypeFullName);
            _ = arguments.Append(", property").Append(i + 1);
        }

        _ = arguments.Append(", selector");

        CodeGeneratorHelpers.AppendStubFallbackCall(
            sb,
            Constants.WhenAnyMethodName,
            typeArguments.ToStringAndReturn(),
            arguments.ToStringAndReturn());
    }

    /// <summary>
    /// Generates an observation method for a single WhenAny invocation.
    /// The method observes property changes and wraps values in ObservedChange before applying the selector.
    /// </summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="suffix">The stable method suffix.</param>
    internal static void GenerateObservationMethod(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        string suffix)
    {
        _ = sb.Append($"private static {GeneratedTypeNames.IObservable}<").Append(inv.ReturnTypeFullName).Append("> __WhenAny_").Append(suffix);
        InterceptorEmitter.AppendResultTypeParameter(sb, inv.GenericResult);
        _ = sb.Append('(').Append(inv.SourceTypeFullName).Append(" obj, ").Append(GetWhenAnySelectorType(inv)).Line(" selector)")
            .OpenBlock();

        if (inv.PropertyPaths.Length == 1)
        {
            GenerateSinglePropertyWhenAny(sb, inv, classInfo);
        }
        else
        {
            GenerateMultiPropertyWhenAny(sb, inv, classInfo);
        }

        _ = sb.CloseBlock()
            .BlankLine();
    }

    /// <summary>Generates single-property WhenAny observation: observe property, wrap in ObservedChange, apply selector.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateSinglePropertyWhenAny(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        var path = inv.PropertyPaths[0];
        var leafType = path[path.Length - 1].PropertyTypeFullName;

        // Emit the property observable into a local variable
        ObservationCodeGenerator.GenerateObservedPropertyVariable(sb, path, classInfo, false, "__propObs0");

        // Wrap in ObservedChange and apply selector
        _ = sb.BlankLine()
            .BeginReturn().Append("new ").Append(GeneratedTypeNames.MapSignal).Append('<')
            .Append(leafType).Append(", ").Append(inv.ReturnTypeFullName).Append(">(__propObs0,")
            .OpenContinuation()
            .Append($"value => selector(new {GeneratedTypeNames.ObservedChange}<").Append(inv.SourceTypeFullName).Append(", ")
            .Append(leafType).Line(">(obj, null, value)));")
            .Outdent();
    }

    /// <summary>
    /// Generates multi-property WhenAny observation: observe each property, wrap each in ObservedChange,
    /// CombineLatest and apply selector.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiPropertyWhenAny(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        // Pre-declare an observable variable for each property path
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            ObservationCodeGenerator.GenerateObservedPropertyVariable(sb, inv.PropertyPaths[i], classInfo, false, $"__propObs{i}");
            _ = sb.BlankLine();
        }

        ObservationCodeGenerator.AppendCombineLatestConstruction(sb, inv.PropertyPaths, inv.ReturnTypeFullName);
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append("__propObs").Append(i).Line(",");
        }

        // Selector lambda: wrap each value in ObservedChange
        _ = sb.Append('(');
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append('v').Append(i + 1);
        }

        _ = sb.Append(") => selector(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            var path = inv.PropertyPaths[i];
            _ = sb.Append($"new {GeneratedTypeNames.ObservedChange}<").Append(inv.SourceTypeFullName).Append(", ")
                .Append(path[path.Length - 1].PropertyTypeFullName).Append(">(obj, null, v").Append(i + 1).Append(')');
        }

        _ = sb.Line("));").Outdent();
    }

    /// <summary>
    /// Gets the Func type signature for a WhenAny selector parameter.
    /// The selector takes <c>IObservedChange&lt;TSender, T&gt;</c> for each property and returns <c>TRet</c>.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>A fully qualified Func type string.</returns>
    internal static string GetWhenAnySelectorType(InvocationInfo inv)
    {
        var sb = new PooledStringBuilder().Append($"{GeneratedTypeNames.Func}<");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var leafType = path[path.Length - 1].PropertyTypeFullName;
            _ = sb.Append($"{GeneratedTypeNames.IObservedChange}<").Append(inv.SourceTypeFullName).Append(", ").Append(leafType).Append(">, ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToStringAndReturn();
    }

    /// <summary>Emits the overload and the observation methods for one group of call sites.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        SourceWriter sb,
        ObservationCodeGenerator.TypeGroup group,
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
            InterceptorEmitter.GenerateInterceptors(
                sb,
                emitted,
                Constants.WhenAnyMethodName,
                ObservationMethodSuffix,
                in features,
                static (SourceWriter builder, InvocationInfo first, in LanguageFeatures snapshot) => AppendParameterList(
                    builder,
                    first,
                    snapshot.SupportsCallerArgExpr,
                    snapshot.SupportsNullable,
                    snapshot.StubHasExpressionParameters));
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

    /// <summary>Writes the parameters a WhenAny member declares, closing the list.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="first">The invocation whose types the parameters are written from.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature. WhenAny
    /// always projects, so the projection is unconditional rather than optional.
    /// </remarks>
    private static void AppendParameterList(
        SourceWriter sb,
        InvocationInfo first,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var propCount = first.PropertyPaths.Length;

        _ = sb.Append("this ").Append(first.SourceTypeFullName).Line(" objectToMonitor,");

        for (var i = 0; i < propCount; i++)
        {
            var type = CodeGeneratorHelpers.NullableSelectorLeafType(first.PropertyPaths[i], supportsNullable);
            _ = sb.Append(GeneratedSyntax.SelectorTypeOpen).Append(first.SourceTypeFullName).Append(", ")
                .Append(type).Append(">> ").Append(SelectorParameterPrefix).Append(i + 1).Line(",");
        }

        _ = sb.Append(GetWhenAnySelectorType(first)).Line(" selector,");

        if (stubHasExpressionParameters)
        {
            for (var i = 0; i < propCount; i++)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(
                    sb,
                    $"property{i + 1}",
                    $"property{i + 1}Expression",
                    supportsCallerArgExpr);
            }
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Emits the if/else-if dispatch table that routes each matched WhenAny invocation to its generated method.</summary>
    /// <param name="sb">The writer, inside the overload's body.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of property expressions.</param>
    private static void EmitDispatchTable(
        SourceWriter sb,
        ObservationCodeGenerator.TypeGroup group,
        bool supportsCallerArgExpr,
        int propCount)
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

            _ = sb.BeginReturn().Append("__WhenAny_").Append(ObservationMethodSuffix(inv))
                .Line("(objectToMonitor, selector);")
                .CloseBlock();
        }
    }

    /// <summary>Names the generated observation method a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its method is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ObservationMethodSuffix(InvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.ExpressionTexts);
}
