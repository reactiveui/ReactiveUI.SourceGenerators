// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Shared code generation logic for property observation APIs (WhenChanged, WhenChanging, WhenAnyValue).
/// Generates concrete typed extension method overloads and per-invocation observation methods.
/// Uses the plugin system to emit platform-specific observation code.
/// </summary>
internal static class ObservationCodeGenerator
{
    /// <summary>Opens the lambda that reads the property back off a before-change notification.</summary>
    private const string ChangingReaderLambdaOpen = $"({INotifyPropertyChanging} __o) => ((";

    /// <summary>Passes the observed object to a before-change observation.</summary>
    private const string ChangingSourceArgument = $"({INotifyPropertyChanging})obj,";

    /// <summary>Opens a before-change observation of a property.</summary>
    private const string ChangingObservableOpen = $"new {PropertyChangingObservable}<";

    /// <summary>Opens a cast of an observation to the interface a chain stage is typed as.</summary>
    private const string ObservableCastOpen = $"({IObservable}<";

    /// <summary>The name the generated overloads give the observed object.</summary>
    private const string MonitoredObjectName = "objectToMonitor";

    /// <summary>Names the local holding the observation the generator's own mechanism builds.</summary>
    private const string MechanismVariableSuffix = "Mechanism";

    /// <summary>Names the local holding the registration that outranked that mechanism.</summary>
    private const string RegistrationVariableSuffix = "Registration";

    /// <summary>Opens a fully qualified default equality comparer.</summary>
    private const string EqualityComparerOpen = $"{EqualityComparer}<";

    /// <summary>
    /// Returns the fully qualified type name for casting the observer parameter back to the
    /// concrete source type. Falls back to <c>"object"</c> when <paramref name="classInfo"/>
    /// is null — a branch that is unreachable in normal generator pipelines but testable directly.
    /// </summary>
    /// <param name="classInfo">The class binding info, or null.</param>
    /// <returns>The fully qualified type name string.</returns>
    internal static string GetTypeCastName(ClassBindingInfo? classInfo) =>
        classInfo?.FullyQualifiedName ?? "object";

    /// <summary>
    /// Determines whether the given class supports after-change property observation
    /// via <see cref="System.ComponentModel.INotifyPropertyChanged"/> (either directly
    /// or through <c>IReactiveObject</c>).
    /// Returns <see langword="false"/> when <paramref name="classInfo"/> is null.
    /// </summary>
    /// <param name="classInfo">The class binding info, or null.</param>
    /// <returns><see langword="true"/> if INPC observation is supported.</returns>
    internal static bool IsINPC(ClassBindingInfo? classInfo) =>
        classInfo is not null && (classInfo.ImplementsIReactiveObject || classInfo.ImplementsINPC);

    /// <summary>
    /// Determines whether the given class supports before-change property observation
    /// via <see cref="System.ComponentModel.INotifyPropertyChanging"/> (either directly
    /// or through <c>IReactiveObject</c>).
    /// Returns <see langword="false"/> when <paramref name="classInfo"/> is null.
    /// </summary>
    /// <param name="classInfo">The class binding info, or null.</param>
    /// <returns><see langword="true"/> if INPChanging observation is supported.</returns>
    internal static bool IsINPChanging(ClassBindingInfo? classInfo) =>
        classInfo is not null && (classInfo.ImplementsIReactiveObject || classInfo.ImplementsINPChanging);

    /// <summary>Generates concrete typed overloads and observation methods for property observation invocations.</summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <param name="methodPrefix">The method name prefix ("WhenChanged", "WhenChanging", or "WhenAnyValue").</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features,
        string methodPrefix) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            InterceptorEmitter.ClaimableBy(invocations, static x => x.GenericResult, features),
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => GenerateGroup(
                sb,
                group,
                allClasses,
                in snapshot,
                methodPrefix));

    /// <summary>Generates an observation method for a single invocation.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="suffix">The stable method name suffix (hex hash).</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="prefix">The method name prefix ("WhenChanged", "WhenChanging", or "WhenAnyValue").</param>
    internal static void GenerateObservationMethod(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        string suffix,
        bool isBeforeChange,
        string prefix)
    {
        _ = sb.Append($"private static {IObservable}<").Append(inv.ReturnTypeFullName).Append("> __").Append(prefix).Append('_')
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
            var path = inv.PropertyPaths[0];

            if (path.Length > 1)
            {
                GenerateDeepChainObservation(sb, inv, classInfo, isBeforeChange);
            }
            else
            {
                var propertyAccessChain = CodeGeneratorHelpers.BuildPropertyAccessChain("obj", path);
                var leafPropertyName = path[0].PropertyName;

                if (inv.HasSelector)
                {
                    _ = sb.BeginReturn().Append("new ").Append(MapSignal).Append('<')
                        .Append(path[0].PropertyTypeFullName).Append(", ").Append(inv.ReturnTypeFullName).Append(">(");
                    GenerateShallowPathObservation(sb, path, classInfo, isBeforeChange);
                    _ = sb.Line(", selector);");
                }
                else
                {
                    GenerateSinglePropertyObservation(
                        sb,
                        inv,
                        classInfo,
                        propertyAccessChain,
                        leafPropertyName,
                        isBeforeChange);
                }
            }
        }
        else
        {
            GenerateMultiPropertyObservation(sb, inv, classInfo, isBeforeChange);
        }

        _ = sb.CloseBlock()
            .BlankLine();
    }

    /// <summary>Gets the Func type signature for a selector parameter.</summary>
    /// <param name="inv">The invocation info containing property path and return type information.</param>
    /// <returns>A fully qualified Func type string like <c>global::System.Func&lt;T1, T2, TReturn&gt;</c>.</returns>
    internal static string GetSelectorType(InvocationInfo inv)
    {
        var sb = new PooledStringBuilder().Append($"{Func}<");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            _ = sb.Append(path[path.Length - 1].PropertyTypeFullName).Append(", ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToStringAndReturn();
    }

    /// <summary>Starts a typed pair constructor or the wide-arity factory, leaving the writer on its argument level.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="paths">The observed property paths.</param>
    /// <param name="returnType">The projected result type.</param>
    internal static void AppendCombineLatestConstruction(
        SourceWriter sb,
        EquatableArray<EquatableArray<PropertyPathSegment>> paths,
        string returnType)
    {
        if (paths.Length == 2)
        {
            var left = paths[0];
            var right = paths[1];
            _ = sb.BeginReturn().Append("new ").Append(CombineLatestSignal).Append('<')
                .Append(left[left.Length - 1].PropertyTypeFullName).Append(", ")
                .Append(right[right.Length - 1].PropertyTypeFullName).Append(", ")
                .Append(returnType).Append(">(");
        }
        else
        {
            _ = sb.BeginReturn().Append($"{LinqExtensions}.CombineLatest(");
        }

        _ = sb.OpenContinuation();
    }

    /// <summary>
    /// Generates a multi-property observation method body using CombineLatest.
    /// Each property path observable is pre-declared as a local variable with properly
    /// formatted multi-line code, then referenced by name inside CombineLatest.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateMultiPropertyObservation(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        bool isBeforeChange)
    {
        // Pre-declare an observable variable for each property path.
        // Both shallow (single-segment) and deep (multi-segment) paths get their own
        // properly formatted local variable, then are referenced by name in CombineLatest.
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            GenerateObservedPropertyVariable(sb, inv.PropertyPaths[i], classInfo, isBeforeChange, $"__propObs{i}");

            // Blank line between variable declarations for readability
            _ = sb.BlankLine();
        }

        AppendCombineLatestConstruction(sb, inv.PropertyPaths, inv.ReturnTypeFullName);
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append("__propObs").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.Line(",");
            }
        }

        if (inv.HasSelector)
        {
            _ = sb.Line(",")
                .Line("selector);");
        }
        else
        {
            EmitCombineLatestValuesProjection(sb, inv);
        }

        _ = sb.Outdent();
    }

    /// <summary>
    /// Generates a shallow (single-segment) path observation as an inline expression.
    /// Uses plugin dispatch to emit platform-specific observation code.
    /// </summary>
    /// <param name="sb">The writer, part way through the line the expression belongs to; the line is left open.</param>
    /// <param name="path">The single-segment property path.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateShallowPathObservation(
        SourceWriter sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange)
    {
        var segment = path[0];
        var plugin = ResolveRootPlugin(classInfo, segment, isBeforeChange);

        ChainRegistrationEmitter.AppendChoiceOpen(
            sb,
            "obj",
            segment,
            plugin?.Affinity ?? 0,
            isBeforeChange,
            string.Empty);

        if (plugin is not null)
        {
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, true);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.Append(ChangingObservableOpen).Append(segment.PropertyTypeFullName).Append(">((")
                .Append($"{INotifyPropertyChanging})obj, ").AppendQuoted(segment.PropertyName)
                .Append($", ({INotifyPropertyChanging} __o) => (").Append('(').Append(GetTypeCastName(classInfo))
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(')');
        }
        else
        {
            _ = sb.Append(OpenUnchangingProperty).Append(segment.PropertyTypeFullName)
                .Append(">(obj.").Append(segment.PropertyName).Append(')');
        }

        _ = ChainRegistrationEmitter.AppendChoiceClose(sb);
    }

    /// <summary>Emits the variable holding one observed property.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="classInfo">The class binding info for the declaring type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="varName">The name to give the variable.</param>
    /// <remarks>
    /// A chain re-subscribes when an intermediate object is replaced, so it is emitted differently from a single
    /// property. Which of the two applies follows from the path's length alone, so every emitter asks here
    /// rather than testing the length itself.
    /// </remarks>
    internal static void GenerateObservedPropertyVariable(
        SourceWriter sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange,
        string varName)
    {
        if (path.Length > 1)
        {
            GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange, varName);
            return;
        }

        GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange, varName);
    }

    /// <summary>
    /// Generates a shallow (single-segment) path observable as a properly formatted local variable
    /// declaration. Uses plugin dispatch to emit platform-specific observation code.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="path">The single-segment property path.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="varName">The variable name to assign the observable to.</param>
    internal static void GenerateShallowObservableVariable(
        SourceWriter sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange,
        string varName)
    {
        var segment = path[0];
        var plugin = ResolveRootPlugin(classInfo, segment, isBeforeChange);
        var mechanismVariable = varName + MechanismVariableSuffix;

        if (plugin is not null)
        {
            plugin.EmitShallowObservationVariable(
                sb,
                "obj",
                segment,
                GetTypeCastName(classInfo),
                isBeforeChange,
                mechanismVariable);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            _ = sb.BeginVar(mechanismVariable).Append(ChangingObservableOpen).Append(segment.PropertyTypeFullName).Append(">(")
                .OpenContinuation()
                .Line(ChangingSourceArgument)
                .AppendQuoted(segment.PropertyName).Line(",")
                .Append(ChangingReaderLambdaOpen).Append(GetTypeCastName(classInfo))
                .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Line(");")
                .Outdent();
        }
        else
        {
            _ = sb.BeginVar(mechanismVariable).Append(OpenUnchangingProperty)
                .Append(segment.PropertyTypeFullName).Append(">(obj.").Append(segment.PropertyName).Line(");");
        }

        EmitInlinePluginChoice(
            sb,
            "obj",
            segment,
            plugin?.Affinity ?? 0,
            isBeforeChange,
            new(mechanismVariable, varName));
    }

    /// <summary>
    /// Generates a deep chain observable as a properly formatted local variable declaration.
    /// Uses plugin dispatch for the root segment and inner segments.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="path">The multi-segment property path.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    /// <param name="varName">The variable name to assign the final observable to.</param>
    internal static void GenerateDeepChainVariable(
        SourceWriter sb,
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? classInfo,
        bool isBeforeChange,
        string varName)
    {
        // First segment: observe root object for first property
        var seg0 = path[0];
        var rootPlugin = ResolveRootPlugin(classInfo, seg0);

        EmitChainRootWithChoice(sb, "obj", seg0, classInfo, rootPlugin, $"{varName}_s0");

        EmitDeepChainInnerSegments(sb, path, isBeforeChange, varName);

        // Distinct on both timings. The runtime engine asks for it whichever way it observes, so a
        // before-change stream that repeated a value would emit where the runtime engine stayed quiet.
        AppendUniqueObservation(sb.BeginVar(varName), path[path.Length - 1].PropertyTypeFullName, $"{varName}_s{path.Length - 1}");
        _ = sb.EndStatement();
    }

    /// <summary>
    /// Groups invocations by their source type, return type, property count, and property types.
    /// Invocations sharing the same type signature share a concrete overload.
    /// </summary>
    /// <param name="invocations">All detected invocations.</param>
    /// <returns>A list of type groups, each containing invocations with the same signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<TypeGroup> GroupByTypeSignature(ImmutableArray<InvocationInfo> invocations) =>
        SignatureGrouping.Group(invocations, AppendSignatureKey, static (first, members) => new TypeGroup(first, members));

    /// <summary>Generates a concrete typed extension method overload with its dispatch table.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    internal static void GenerateConcreteOverload(
        SourceWriter sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters,
        string methodPrefix)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;
        var hasSelector = first.HasSelector;

        EmitOverloadSignature(sb, first, supportsCallerArgExpr, supportsNullable, stubHasExpressionParameters, methodPrefix);

        // A registration that outranks the generated mechanism is honoured where the observation is built,
        // one property at a time, so the dispatch itself has nothing to decide.
        EmitDispatchTable(sb, group, supportsCallerArgExpr, methodPrefix, propCount, hasSelector);

        GenerateRuntimeFallback(sb, first, methodPrefix, propCount, hasSelector);

        _ = sb.CloseBlock();
    }

    /// <summary>Ends the overload where the stub it displaces would have ended: at the runtime engine.</summary>
    /// <param name="sb">The writer, inside the overload's body.</param>
    /// <param name="first">The invocation whose types the whole group shares.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of observed properties.</param>
    /// <param name="hasSelector">Whether the overload takes a selector.</param>
    internal static void GenerateRuntimeFallback(
        SourceWriter sb,
        InvocationInfo first,
        string methodPrefix,
        int propCount,
        bool hasSelector)
    {
        var typeArguments = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);
        var arguments = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        _ = typeArguments.Append(first.SourceTypeFullName);
        _ = arguments.Append(MonitoredObjectName);

        for (var i = 0; i < propCount; i++)
        {
            var path = first.PropertyPaths[i];
            _ = typeArguments.Append(", ").Append(path[path.Length - 1].PropertyTypeFullName);
            _ = arguments.Append(", property").Append(i + 1);
        }

        if (hasSelector)
        {
            _ = typeArguments.Append(", ").Append(first.ReturnTypeFullName);
            _ = arguments.Append(", selector");
        }

        CodeGeneratorHelpers.AppendStubFallbackCall(
            sb,
            methodPrefix,
            typeArguments.ToStringAndReturn(),
            arguments.ToStringAndReturn());
    }

    /// <summary>Generates a single-property observation method body using plugin dispatch.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="propertyAccess">The dotted property access expression (e.g. "obj.Name").</param>
    /// <param name="propertyName">The leaf property name for event filtering.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateSinglePropertyObservation(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        string propertyAccess,
        string propertyName,
        bool isBeforeChange)
    {
        var segment = inv.PropertyPaths[0][0];
        var plugin = ResolveRootPlugin(classInfo, segment, isBeforeChange);

        ChainRegistrationEmitter.AppendChoiceOpen(
            sb,
            "obj",
            segment,
            plugin?.Affinity ?? 0,
            isBeforeChange,
            "return ");

        if (plugin is not null)
        {
            plugin.EmitShallowObservation(sb, "obj", segment, GetTypeCastName(classInfo), isBeforeChange, true);
        }
        else if (IsINPChanging(classInfo) && isBeforeChange)
        {
            // INPChanging-only type (no INPC, no IReactiveObject) — can observe before-change
            _ = sb.Append(ChangingObservableOpen).Append(inv.ReturnTypeFullName).Append(">(")
                .OpenContinuation()
                .Line(ChangingSourceArgument)
                .AppendQuoted(propertyName).Line(",")
                .Append(ChangingReaderLambdaOpen).Append(inv.SourceTypeFullName)
                .Append(GeneratedSyntax.ObserverCastClose).Append(propertyName).Append(')')
                .Outdent();
        }
        else
        {
            _ = sb.Append(OpenUnchangingProperty)
                .Append(inv.ReturnTypeFullName).Append(">(").Append(propertyAccess).Append(')');
        }

        _ = ChainRegistrationEmitter.AppendChoiceClose(sb).EndStatement();
    }

    /// <summary>Generates a deep chain observation method body using plugin dispatch for the root segment and inner segments.</summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change), false for WhenChanged (after-change).</param>
    internal static void GenerateDeepChainObservation(
        SourceWriter sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        bool isBeforeChange)
    {
        var path = inv.PropertyPaths[0];
        var seg0 = path[0];
        var rootPlugin = ResolveRootPlugin(classInfo, seg0);

        // First segment: observe root object for first property
        EmitChainRootWithChoice(sb, "obj", seg0, classInfo, rootPlugin, "__obs0");

        EmitObservationChainInnerSegments(sb, path, isBeforeChange);

        var leafType = path[path.Length - 1].PropertyTypeFullName;
        _ = sb.BeginReturn();
        if (inv.HasSelector)
        {
            _ = sb.Append("new ").Append(MapSignal).Append('<').Append(leafType).Append(", ")
                .Append(inv.ReturnTypeFullName).Append(">(");
        }

        AppendUniqueObservation(sb, leafType, $"__obs{path.Length - 1}");
        if (inv.HasSelector)
        {
            _ = sb.Append(", selector)");
        }

        _ = sb.EndStatement();
    }

    /// <summary>
    /// Emits an inline observation expression as a variable assignment using plugin dispatch.
    /// Used by binding generators to emit direct observation code
    /// instead of delegating to WhenChanged dispatch.
    /// </summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="rootVar">The root variable name (e.g., "source", "target").</param>
    /// <param name="propertyPath">The property path segments.</param>
    /// <param name="propertyTypeFullName">The fully qualified type of the leaf property.</param>
    /// <param name="classInfo">The class binding info for the observed type, or null.</param>
    /// <param name="variableName">The name for the resulting observable variable (e.g., "sourceObs").</param>
    /// <param name="brokenChainBehavior">What a deep path's leaf emits while a parent on the path is null.</param>
    internal static void EmitInlineObservation(
        SourceWriter sb,
        string rootVar,
        EquatableArray<PropertyPathSegment> propertyPath,
        string propertyTypeFullName,
        ClassBindingInfo? classInfo,
        string variableName,
        NullParentObservationBehavior brokenChainBehavior = NullParentObservationBehavior.EmitDefault)
    {
        var plugin = ResolveRootPlugin(classInfo, propertyPath[0]);

        if (propertyPath.Length == 1)
        {
            var segment = propertyPath[0];
            var mechanismVariable = variableName + MechanismVariableSuffix;

            if (plugin is not null)
            {
                plugin.EmitInlineObservationVariable(sb, rootVar, segment, GetTypeCastName(classInfo), mechanismVariable);
            }
            else
            {
                _ = sb.BeginVar(mechanismVariable).Append(OpenUnchangingProperty).Append(propertyTypeFullName).Append(">(")
                    .Append(rootVar).Append('.').Append(segment.PropertyName).Line(");");
            }

            EmitInlinePluginChoice(
                sb,
                rootVar,
                segment,
                plugin?.Affinity ?? 0,
                false,
                new(mechanismVariable, variableName),
                propertyTypeFullName);
        }
        else
        {
            EmitInlineDeepChain(sb, rootVar, propertyPath, classInfo, plugin, variableName, brokenChainBehavior);
        }
    }

    /// <summary>Selects a property's mechanism using its declaring type when the property is inherited.</summary>
    /// <param name="classInfo">The type named by the call site, or null when unavailable.</param>
    /// <param name="segment">The observed property and its declaring type.</param>
    /// <param name="isBeforeChange">Whether the requested notification precedes the change.</param>
    /// <returns>The winning observation plugin, or null when no mechanism reaches the property.</returns>
    internal static IObservationPlugin? ResolveRootPlugin(ClassBindingInfo? classInfo, PropertyPathSegment segment, bool isBeforeChange = false)
    {
        var owner = segment.DeclaringTypeInfo ?? classInfo;
        return owner is null || segment.IsField ? null : ObservationPluginRegistry.GetBestPlugin(owner, segment.PropertyName, isBeforeChange);
    }

    /// <summary>Writes the parts of a call site that decide its overload: the types and each observed leaf's type.</summary>
    /// <param name="key">The key being built.</param>
    /// <param name="inv">The call site.</param>
    private static void AppendSignatureKey(PooledStringBuilder key, InvocationInfo inv)
    {
        _ = key
            .Append(inv.SourceTypeFullName).Append('|')
            .Append(inv.ReturnTypeFullName).Append('|')
            .Append(inv.PropertyPaths.Length).Append('|')
            .Append(inv.HasSelector);
        for (var p = 0; p < inv.PropertyPaths.Length; p++)
        {
            var path = inv.PropertyPaths[p];
            _ = key.Append('|').Append(path[path.Length - 1].PropertyTypeFullName);
        }
    }

    /// <summary>Emits the choice between the mechanism the generator picked and a registration that outranks it.</summary>
    /// <param name="sb">The writer, inside the body the observation is built in.</param>
    /// <param name="rootVar">The variable holding the observed object.</param>
    /// <param name="segment">The property being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism the generator picked.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="locals">The local holding the mechanism's observation, and the local the choice is assigned to.</param>
    /// <param name="valueTypeOverride">
    /// The element type the surrounding code expects, where it is not the property's own declared type.
    /// </param>
    /// <remarks>
    /// The registration is resolved where the observation is built rather than at the call site's dispatch, so a
    /// binding keeps its generated write and only its reading changes. The property is named as a literal and
    /// read through an emitted accessor, and the expression handed to the registration is a lambda the compiler
    /// built, so nothing on this path is resolved by name and an ahead-of-time consumer carries no expression
    /// engine for it.
    /// <para>
    /// The element type is overridable because a caller can want the property's interface rather than its
    /// declared type - an interaction property declared as <c>Interaction</c> is observed as
    /// <c>IInteraction</c> - and the choice has to be typed the same way as the mechanism it wraps.
    /// </para>
    /// </remarks>
    private static void EmitInlinePluginChoice(
        SourceWriter sb,
        string rootVar,
        PropertyPathSegment segment,
        int generatedAffinity,
        bool isBeforeChange,
        in PluginChoiceLocals locals,
        string? valueTypeOverride = null)
    {
        var valueType = valueTypeOverride ?? segment.PropertyTypeFullName;
        var pluginVariable = locals.VariableName + RegistrationVariableSuffix;

        _ = sb.BeginVar(pluginVariable).Append(ObservationAffinityChecker)
            .Append(".FindHigherAffinityPlugin(").Append(rootVar).Append(".GetType(), ").AppendQuoted(segment.PropertyName)
            .Append(", ").Append(generatedAffinity).Append(", ").AppendLiteral(isBeforeChange).Line(");")
            .BeginVar(locals.VariableName).Append(pluginVariable).Append(" == null")
            .OpenContinuation()
            .Append("? ").Append(ObservableCastOpen).Append(valueType).Append(">)").Line(locals.MechanismVariable)
            .Append(": ").Append(ObservableCastOpen).Append(valueType).Append(">)new ")
            .Append(PluginPropertyObservable).Append('<').Append(valueType).Append(">(")
            .OpenContinuation();
        _ = ChainRegistrationEmitter.AppendPluginObservableArguments(sb, pluginVariable, rootVar, segment, valueType, isBeforeChange)
            .Line(",")
            .Line("true);")
            .Outdent()
            .Outdent();
    }

    /// <summary>Emits the first link of a chain into a local, and the choice a registration can win for it.</summary>
    /// <param name="sb">The writer, inside the body the chain is built in.</param>
    /// <param name="rootVar">The variable holding the chain root.</param>
    /// <param name="seg0">The first segment of the path.</param>
    /// <param name="classInfo">The root type's binding info, when known.</param>
    /// <param name="rootPlugin">The plugin for the root type, when one matched.</param>
    /// <param name="obsVar">The local the link's observation is assigned to.</param>
    /// <remarks>
    /// The first link is offered to a registration on the same terms as every later one. Without this the root
    /// of a chain was the one link a registered plugin could not take, which made the honouring depend on where
    /// in a path the property sat. The first link of a chain is never its leaf, so it is always observed after
    /// the change, even for a before-change observation.
    /// </remarks>
    private static void EmitChainRootWithChoice(
        SourceWriter sb,
        string rootVar,
        PropertyPathSegment seg0,
        ClassBindingInfo? classInfo,
        IObservationPlugin? rootPlugin,
        string obsVar)
    {
        var mechanismVariable = obsVar + MechanismVariableSuffix;

        if (rootPlugin is not null)
        {
            rootPlugin.EmitDeepChainRootSegment(sb, rootVar, seg0, GetTypeCastName(classInfo), false, mechanismVariable);
        }
        else
        {
            _ = sb.BeginVar(mechanismVariable).Append(ObservableCastOpen).Append(seg0.PropertyTypeFullName).Append(">)")
                .Append(OpenUnchangingProperty).Append(seg0.PropertyTypeFullName).Append(">(").Append(rootVar).Append('.')
                .Append(seg0.PropertyName).Line(");");
        }

        EmitInlinePluginChoice(
            sb,
            rootVar,
            seg0,
            rootPlugin?.Affinity ?? 0,
            false,
            new(mechanismVariable, obsVar));
    }

    /// <summary>Picks the observation plugin for the type that declares a chain segment's property.</summary>
    /// <param name="segment">The chain segment, which carries how its declaring type notifies.</param>
    /// <param name="isBeforeChange">Whether the property is observed before it changes.</param>
    /// <returns>The plugin for that type, or null to fall back to reading the property.</returns>
    /// <remarks>
    /// Each link of a chain is declared by its own type and notifies - or does not - on its own terms, so the
    /// mechanism is resolved per segment. Reusing the root's plugin emits its cast against whatever the segment
    /// declares, which is a build error where the two are unrelated and a failed cast at runtime where they are
    /// merely different.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservationPlugin? ResolveSegmentPlugin(PropertyPathSegment segment, bool isBeforeChange = false) =>
        segment.DeclaringTypeInfo is null || segment.IsField
            ? null
            : ObservationPluginRegistry.GetBestPlugin(segment.DeclaringTypeInfo, segment.PropertyName, isBeforeChange);

    /// <summary>
    /// Chains the segments after the root for the standalone observation method, which names its
    /// stages <c>__obsN</c> rather than deriving them from a caller-supplied prefix.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <remarks>
    /// Missing parents propagate through intermediate stages so downstream subscriptions detach,
    /// while the leaf suppresses the default value to match the runtime expression-chain fallback.
    /// </remarks>
    private static void EmitObservationChainInnerSegments(
        SourceWriter sb,
        EquatableArray<PropertyPathSegment> path,
        bool isBeforeChange)
    {
        for (var s = 1; s < path.Length; s++)
        {
            EmitInnerSegment(sb, path[s], new($"__obs{s - 1}", $"__obs{s}", $"__parent{s}"), s == path.Length - 1, isBeforeChange);
        }
    }

    /// <summary>
    /// Chains the segments after the root with Select + Switch, so the observation re-subscribes when
    /// an intermediate value changes.
    /// </summary>
    /// <param name="sb">The writer, inside the observation method's body.</param>
    /// <param name="path">The property path being observed.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="varName">The variable-name prefix for the emitted stages.</param>
    /// <remarks>
    /// Missing parents propagate through intermediate stages so downstream subscriptions detach,
    /// while the leaf suppresses the default value to match the runtime expression-chain fallback.
    /// </remarks>
    private static void EmitDeepChainInnerSegments(
        SourceWriter sb,
        EquatableArray<PropertyPathSegment> path,
        bool isBeforeChange,
        string varName)
    {
        for (var s = 1; s < path.Length; s++)
        {
            EmitInnerSegment(sb, path[s], new($"{varName}_s{s - 1}", $"{varName}_s{s}", $"{varName}_p{s}"), s == path.Length - 1, isBeforeChange);
        }
    }

    /// <summary>Emits the stage that switches one segment after the root onto its parent's latest value.</summary>
    /// <param name="sb">The writer, inside the body the chain is built in.</param>
    /// <param name="seg">The segment the stage observes.</param>
    /// <param name="variables">The names of the parent stage, this stage and the lambda's parent parameter.</param>
    /// <param name="isLeaf">Whether the segment is the last in the path.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <remarks>
    /// Only the leaf suppresses. Inner segments keep pushing the null downstream so the stage below re-parents onto
    /// null and drops its subscription on the detached subtree.
    /// </remarks>
    private static void EmitInnerSegment(
        SourceWriter sb,
        PropertyPathSegment seg,
        ChainStageVariables variables,
        bool isLeaf,
        bool isBeforeChange)
    {
        var beforeLeaf = isBeforeChange && isLeaf;
        var segPlugin = ResolveSegmentPlugin(seg, beforeLeaf);
        var nullParentBehavior = isLeaf
            ? NullParentObservationBehavior.SuppressEmission
            : NullParentObservationBehavior.EmitDefault;

        if (segPlugin is not null)
        {
            segPlugin.EmitDeepChainInnerSegment(sb, variables, seg, beforeLeaf, nullParentBehavior);
            return;
        }

        EmitReadOnceStage(sb, seg, variables, nullParentBehavior);
    }

    /// <summary>Emits a chain stage for a segment whose declaring type is unknown, reading the value once per parent.</summary>
    /// <param name="sb">The writer, inside the body the chain is built in.</param>
    /// <param name="seg">The segment the stage observes.</param>
    /// <param name="variables">The names of the parent stage, this stage and the lambda's parent parameter.</param>
    /// <param name="nullParentBehavior">What the stage emits while its parent is null.</param>
    /// <remarks>
    /// Every known type resolves a plugin, the POCO fallback at worst, so only a segment whose declaring type is
    /// unknown reaches here. Nothing about such a type says it notifies, so the stage reads the value once.
    /// </remarks>
    private static void EmitReadOnceStage(
        SourceWriter sb,
        PropertyPathSegment seg,
        in ChainStageVariables variables,
        NullParentObservationBehavior nullParentBehavior)
    {
        var segType = seg.PropertyTypeFullName;
        ChainRegistrationEmitter.AppendStageOpen(sb, variables, seg, segType);
        _ = sb.Append("? ").Append(ObservableCastOpen).Append(segType).Line(">)")
            .Indent()
            .Append($"new {ImmediateReturnSignal}<").Append(segType).Append(">(((")
            .Append(seg.DeclaringTypeFullName).Append(')').Append(variables.ParentParameter).Append(").").Append(seg.PropertyName).Line(")")
            .Outdent();
        ChainRegistrationEmitter.AppendStageClose(sb, segType, nullParentBehavior);
    }

    /// <summary>
    /// Emits the Select/Switch chain for a multi-segment property path, one stage per segment, and
    /// the distinct-until-changed gate that terminates it.
    /// </summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="rootVar">The variable holding the chain root.</param>
    /// <param name="propertyPath">The property path being observed.</param>
    /// <param name="classInfo">The root type's binding info, when known.</param>
    /// <param name="plugin">The observation plugin for the root type, when one matched.</param>
    /// <param name="variableName">The name of the variable the chain result is assigned to.</param>
    /// <param name="brokenChainBehavior">What the leaf emits while its parent is null.</param>
    /// <remarks>
    /// Inner segments emit defaults so the stage below re-parents onto null and drops its subscription on the
    /// detached subtree. The leaf follows <paramref name="brokenChainBehavior"/>: emitting its default lets a consumer
    /// clear its target, suppressing leaves the target as it was until the path is whole again.
    /// </remarks>
    private static void EmitInlineDeepChain(
        SourceWriter sb,
        string rootVar,
        EquatableArray<PropertyPathSegment> propertyPath,
        ClassBindingInfo? classInfo,
        IObservationPlugin? plugin,
        string variableName,
        NullParentObservationBehavior brokenChainBehavior)
    {
        EmitChainRootWithChoice(sb, rootVar, propertyPath[0], classInfo, plugin, $"__{variableName}_s0");

        for (var s = 1; s < propertyPath.Length; s++)
        {
            var seg = propertyPath[s];
            var variables = new ChainStageVariables($"__{variableName}_s{s - 1}", $"__{variableName}_s{s}", $"__p{s}");
            var segPlugin = ResolveSegmentPlugin(seg);
            var nullParentBehavior = s == propertyPath.Length - 1 ? brokenChainBehavior : NullParentObservationBehavior.EmitDefault;

            if (segPlugin is not null)
            {
                segPlugin.EmitDeepChainInnerSegment(sb, variables, seg, isBeforeChange: false, nullParentBehavior);
                continue;
            }

            EmitReadOnceStage(sb, seg, variables, nullParentBehavior);
        }

        AppendUniqueObservation(sb.BeginVar(variableName), propertyPath[propertyPath.Length - 1].PropertyTypeFullName, $"__{variableName}_s{propertyPath.Length - 1}");
        _ = sb.EndStatement();
    }

    /// <summary>Constructs typed distinct-value filtering with the default comparer.</summary>
    /// <param name="sb">The writer, part way through the line the expression belongs to.</param>
    /// <param name="valueType">The observed value type.</param>
    /// <param name="source">The source variable.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendUniqueObservation(SourceWriter sb, string valueType, string source) =>
        sb.Append("new ").Append(UniqueSignal).Append('<').Append(valueType).Append(">(")
            .Append(source).Append(", ").Append(EqualityComparerOpen).Append(valueType).Append(">.Default)");

    /// <summary>
    /// Computes the worker-method suffix for an observation invocation, keyed only by the source type and
    /// property expression(s) — not the call site. Call sites that share the same type and expression(s)
    /// produce an identical worker, so they resolve to a single generated method.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>The stable, call-site-independent method-name suffix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MethodSuffix(InvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            string.Empty,
            0,
            inv.ExpressionTexts);

    /// <summary>Generates the concrete overload and per-invocation observation methods for a single type group.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The type group to generate code for.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot, which settles the dispatch.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    private static void GenerateGroup(
        SourceWriter sb,
        TypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features,
        string methodPrefix)
    {
        // Either claim each call site outright, or emit the overload that competes for them all.
        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, group, methodPrefix, in features);
        }
        else
        {
            GenerateConcreteOverload(
                sb,
                group,
                features.SupportsCallerArgExpr,
                features.SupportsNullable,
                features.StubHasExpressionParameters,
                methodPrefix);
        }

        _ = sb.BlankLine();

        // Generate the observation methods for each invocation in this group. Call sites that share the
        // same source type and property expression(s) produce an identical worker, so the method is keyed
        // by (type, expressions) and emitted only once — avoiding duplicate, identical methods.
        var emittedMethods = new HashSet<string>();
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var suffix = MethodSuffix(inv);
            if (!emittedMethods.Add(suffix))
            {
                continue;
            }

            var classInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(
                allClasses,
                inv.SourceTypeFullName,
                inv.PropertyPaths[0]);

            GenerateObservationMethod(sb, inv, classInfo, suffix, inv.IsBeforeChange, methodPrefix);
        }
    }

    /// <summary>Emits one interceptor per generated observation, claiming every call site that reaches it.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The type group whose call sites are being claimed.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <remarks>
    /// Call sites that share a source type and the same expressions produce one observation between them, and
    /// the attribute may be applied repeatedly, so they are claimed by a single method carrying one attribute
    /// each. A call site the compiler declined to describe is left alone: it keeps whatever the call already
    /// resolved to, which is the same outcome an unreachable dispatch overload produces.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateInterceptors(
        SourceWriter sb,
        TypeGroup group,
        string methodPrefix,
        in LanguageFeatures features) =>
        InterceptorEmitter.GenerateInterceptors(
            sb,
            group,
            methodPrefix,
            MethodSuffix,
            in features,
            static (SourceWriter builder, InvocationInfo first, in LanguageFeatures snapshot) => AppendParameterList(
                builder,
                first,
                snapshot.SupportsCallerArgExpr,
                snapshot.SupportsNullable,
                snapshot.StubHasExpressionParameters,
                first.PropertyPaths.Length,
                first.HasSelector));

    /// <summary>Emits the trailing projection lambda that gathers a selector-less <c>CombineLatest</c> into one emission.</summary>
    /// <param name="sb">The writer, after the last observed property's argument.</param>
    /// <param name="inv">The invocation, whose return type is the emission being constructed.</param>
    private static void EmitCombineLatestValuesProjection(SourceWriter sb, InvocationInfo inv)
    {
        var propertyCount = inv.PropertyPaths.Length;

        _ = sb.Line(",").Append('(');
        AppendProjectionParameters(sb, propertyCount);
        _ = sb.Append(") => new ").Append(inv.ReturnTypeFullName).Append('(');
        AppendProjectionParameters(sb, propertyCount);
        _ = sb.Line("));");
    }

    /// <summary>Writes the projection's parameters, <c>p1</c> onwards, comma separated.</summary>
    /// <param name="sb">The writer, part way through the projection.</param>
    /// <param name="propertyCount">How many observed properties the projection takes.</param>
    private static void AppendProjectionParameters(SourceWriter sb, int propertyCount)
    {
        for (var i = 0; i < propertyCount; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append('p').Append(i + 1);
        }
    }

    /// <summary>
    /// Emits the XML doc comment, method signature, property expression parameters, optional selector,
    /// CallerArgumentExpression parameters, and the caller-info parameters that open the overload body.
    /// </summary>
    /// <param name="sb">The writer, at the class's member level; left inside the overload's body.</param>
    /// <param name="first">The first invocation in the group, used for type information.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    private static void EmitOverloadSignature(
        SourceWriter sb,
        InvocationInfo first,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters,
        string methodPrefix)
    {
        _ = sb.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for ").Append(methodPrefix).Append(" on ").Append(first.SourceTypeFullName).Line(".")
            .CloseSummary()
            .Append($"public static {IObservable}<").Append(first.ReturnTypeFullName).Append("> ").Append(methodPrefix)
            .OpenParameterList();

        AppendParameterList(
            sb,
            first,
            supportsCallerArgExpr,
            supportsNullable,
            stubHasExpressionParameters,
            first.PropertyPaths.Length,
            first.HasSelector);

        _ = sb.OpenBlock();
    }

    /// <summary>Writes the parameters an observation member declares, closing the list.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="first">The invocation whose types the parameters are written from.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters.</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature: the
    /// overload only wins resolution against a candidate it is otherwise indistinguishable from, and an
    /// interceptor is refused outright unless its signature is the intercepted method's.
    /// </remarks>
    private static void AppendParameterList(
        SourceWriter sb,
        InvocationInfo first,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters,
        int propCount,
        bool hasSelector)
    {
        _ = sb.Append("this ").Append(first.SourceTypeFullName).Line(" objectToMonitor,");

        for (var i = 0; i < propCount; i++)
        {
            var type = CodeGeneratorHelpers.NullableSelectorLeafType(first.PropertyPaths[i], supportsNullable);
            _ = sb.Append(GeneratedSyntax.SelectorTypeOpen).Append(first.SourceTypeFullName).Append(", ")
                .Append(type).Append(">> property").Append(i + 1).Line(",");
        }

        if (hasSelector)
        {
            _ = sb.Append(GetSelectorType(first)).Line(" selector,");
        }

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

    /// <summary>Emits the if/else-if dispatch table that routes each matched invocation to its generated method.</summary>
    /// <param name="sb">The writer, inside the overload's body.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="methodPrefix">The method name prefix.</param>
    /// <param name="propCount">The number of property expressions.</param>
    /// <param name="hasSelector">Whether a selector function is present.</param>
    private static void EmitDispatchTable(
        SourceWriter sb,
        TypeGroup group,
        bool supportsCallerArgExpr,
        string methodPrefix,
        int propCount,
        bool hasSelector)
    {
        // CallerArgumentExpression dispatch keys solely on the expression text, so call sites that share
        // the same expression(s) collapse to one branch (duplicates would be identical and unreachable).
        // CallerFilePath dispatch keeps one branch per call site (distinct file/line) but routes to the
        // same expression-keyed worker. 'branchIndex' tracks emitted branches so the first uses "if".
        var emittedConditions = new HashSet<EquatableArray<string>>();
        var branchIndex = 0;
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            if (supportsCallerArgExpr)
            {
                if (!emittedConditions.Add(inv.ExpressionTexts))
                {
                    continue;
                }

                CodeGeneratorHelpers.AppendSelectorTextCondition(sb, branchIndex, "property", inv.ExpressionTexts, propCount);
            }
            else
            {
                CodeGeneratorHelpers.AppendInlineCallerInfoCondition(
                    sb,
                    branchIndex,
                    inv.CallerLineNumber,
                    CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            }

            branchIndex++;
            _ = sb.BeginReturn().Append("__").Append(methodPrefix).Append('_').Append(MethodSuffix(inv)).Append('(').Append(MonitoredObjectName)
                .Append(hasSelector ? ", selector" : string.Empty).Line(");")
                .CloseBlock();
        }
    }

    /// <summary>The two locals a plugin choice names: the mechanism's observation and the observation chosen.</summary>
    /// <param name="MechanismVariable">The local holding the observation the generator's own mechanism built.</param>
    /// <param name="VariableName">The local the chosen observation is assigned to.</param>
    internal readonly record struct PluginChoiceLocals(string MechanismVariable, string VariableName);

    /// <summary>Groups invocations by source and return type signature for overload generation.</summary>
    /// <param name="First">The first invocation in the group, used for type information.</param>
    /// <param name="Invocations">All invocations sharing the same type signature.</param>
    internal sealed record TypeGroup(
        InvocationInfo First,
        InvocationInfo[] Invocations)
    {
        /// <summary>Gets the fully qualified name of the source type.</summary>
        internal string SourceTypeFullName => First.SourceTypeFullName;

        /// <summary>Gets the fully qualified name of the return type.</summary>
        internal string ReturnTypeFullName => First.ReturnTypeFullName;
    }
}
