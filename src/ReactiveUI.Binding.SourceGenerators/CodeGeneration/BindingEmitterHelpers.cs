// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// The emission the four property-binding APIs share: grouping call sites, and the optional conversion and
/// scheduler parameters that trail every overload.
/// </summary>
/// <remarks>
/// <c>BindOneWay</c>, <c>BindTwoWay</c>, <c>OneWayBind</c> and <c>Bind</c> each emit a different signature, but
/// the grouping is the same and the trailing parameters differ only in what the conversion argument is called -
/// a converting binding names it <c>conversionFunc</c>, a projecting one names it <c>selector</c>. Passing that
/// name in is the whole of the difference, so the emission lives here once rather than four times.
/// </remarks>
internal static class BindingEmitterHelpers
{
    /// <summary>The name a view exposes its view model under.</summary>
    private const string ViewModelPropertyName = "ViewModel";

    /// <summary>The type the non-generic view interface declares the view model as, which names no view model.</summary>
    private const string WeaklyTypedViewModel = "object";

    /// <summary>Opens a delegate parameter, ready for the two type arguments and the parameter name.</summary>
    private const string FuncParameterPrefix = $", {GeneratedTypeNames.Func}<";

    /// <summary>The stream carrying values converted for the target.</summary>
    private const string ConvertedForwardName = "__convertedForward";

    /// <summary>The stream carrying values converted for the source.</summary>
    private const string ConvertedReverseName = "__convertedReverse";

    /// <summary>Follows a scheduler's type where a worker declares it.</summary>
    private const string SchedulerDeclarationSuffix = " scheduler";

    /// <summary>What a converter overload calls the hint it hands its converters.</summary>
    private const string ConversionHintName = "conversionHint";

    /// <summary>The list a hook reader collects the changes along its path into.</summary>
    private const string HookChangesName = "__hookChanges";

    /// <summary>What a hook reader returns: the changes it reached.</summary>
    private const string HookChangesResult = "__hookChanges.ToArray()";

    /// <summary>Names the local holding the owner a hook reader reads one segment from, before its index.</summary>
    private const string HookOwnerPrefix = "__hookOwner";

    /// <summary>Names the local holding the value a hook reader read for one segment, before its index.</summary>
    private const string HookValuePrefix = "__hookValue";

    /// <summary>Emits a whole binding dispatch file, claiming its call sites through one API's dispatch.</summary>
    /// <param name="invocations">The detected call sites for this API.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="api">The API whose overload or interceptors claim the call sites.</param>
    /// <param name="emitMethod">Emits the binding method for one call site.</param>
    /// <returns>The generated source, or null when there are no call sites.</returns>
    /// <remarks>
    /// Every binding API claims a group the same way, so how the claim is emitted follows from the API rather
    /// than being written out again at each registration.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<BindingInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features,
        BindingDispatchApi api,
        Action<SourceWriter, BindEmitContext> emitMethod) =>
        Generate(
            invocations,
            allClasses,
            in features,
            (sb, group, f) => EmitOverloadOrInterceptors(sb, group, api, in f),
            emitMethod);

    /// <summary>
    /// Emits a whole binding dispatch file: the extension class, one concrete overload per group of
    /// call sites, and one binding method per call site.
    /// </summary>
    /// <param name="invocations">The detected call sites for this API.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="emitOverload">Emits the concrete typed overload for one group.</param>
    /// <param name="emitMethod">Emits the binding method for one call site.</param>
    /// <returns>The generated source, or null when there are no call sites.</returns>
    internal static string? Generate(
        ImmutableArray<BindingInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features,
        Action<SourceWriter, BindingTypeGroup, LanguageFeatures> emitOverload,
        Action<SourceWriter, BindEmitContext> emitMethod)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var snapshot = features;
        var sb = SourceWriter.Rent(invocations.Length * CodeGeneratorHelpers.PerInvocationBufferCapacity);
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb, snapshot);

        var groups = GroupByTypeSignature(invocations);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = snapshot.CollapsesIndistinguishableCallSites && !groups[g].HasConverterOverride
                ? groups[g] with
                {
                    Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                        groups[g].Invocations,
                        static x => $"{x.SourceExpressionText}|{x.TargetExpressionText}"),
                }
                : groups[g];

            emitOverload(sb, group, snapshot);
            _ = sb.BlankLine();

            for (var i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                    inv.SourceTypeFullName,
                    inv.CallerFilePath,
                    inv.CallerLineNumber,
                    $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

                emitMethod(
                    sb,
                    new(
                        inv,
                        CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.SourceTypeFullName, inv.SourcePropertyPath),
                        CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.TargetTypeFullName, inv.TargetPropertyPath),
                        suffix,
                        snapshot));
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);

        return sb.ToStringAndReturn();
    }

    /// <summary>Emits the guard that lets a registered binding hook refuse this binding.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceVar">The generated name of the source object.</param>
    /// <param name="sourcePath">The source property path supplied to hooks.</param>
    /// <param name="targetVar">The generated name of the target object.</param>
    /// <param name="targetPath">The target property path supplied to hooks.</param>
    /// <param name="direction">The binding direction reported to the hook.</param>
    /// <param name="earlyReturn">What the generated method returns when a hook refuses.</param>
    /// <remarks>
    /// The <c>Any</c> test comes first so the closures are only built once a hook is registered. Each reader
    /// walks its path when requested, returning the changes reached before a null intermediate. The readers
    /// capture copies declared inside the guard's block rather than the method's parameters: a captured parameter
    /// shares its closure with every other lambda in the method, so the binding's subscription would keep the
    /// source object alive for as long as the target lives.
    /// </remarks>
    internal static void EmitBindingHookGuard(
        SourceWriter sb,
        string sourceVar,
        EquatableArray<PropertyPathSegment> sourcePath,
        string targetVar,
        EquatableArray<PropertyPathSegment> targetPath,
        string direction,
        string earlyReturn)
    {
        const string HookSource = "__hookSource";
        const string HookTarget = "__hookTarget";

        _ = sb.BeginIf().Append(GeneratedTypeNames.BindingHooks).Append(".Any").CloseCondition()
            .Var(HookSource, sourceVar)
            .Var(HookTarget, targetVar)
            .BeginIf().Append('!').Append(GeneratedTypeNames.BindingHooks).Append(".ShouldBind(").OpenContinuation()
            .Append(HookSource).Line(",")
            .Append(HookTarget).Line(",");

        AppendHookPropertyReader(sb, HookSource, sourcePath);
        _ = sb.Line(",");
        AppendHookPropertyReader(sb, HookTarget, targetPath);

        _ = sb.Line(",")
            .Append(GeneratedTypeNames.BindingDirection).Append('.').Append(direction).Append(')')
            .Outdent().CloseCondition()
            .BeginReturn().Append(earlyReturn).EndStatement()
            .CloseBlock()
            .CloseBlock();
    }

    /// <summary>Emits a lazy reader whose changes retain each segment's owner, expression and value.</summary>
    /// <param name="sb">The writer, at the level of the argument the reader is passed as.</param>
    /// <param name="root">The object at the start of the path.</param>
    /// <param name="path">The property path, empty when the source is already an observable.</param>
    /// <remarks>Leaves the line open after the reader, for the caller to separate or close the argument list.</remarks>
    internal static void AppendHookPropertyReader(SourceWriter sb, string root, EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            _ = sb.Append("() => global::System.Array.Empty<")
                .Append(GeneratedTypeNames.IObservedChange).Append("<object, object>>()");
            return;
        }

        _ = sb.Line("() =>")
            .OpenBlock()
            .BeginVar(HookChangesName).Append("new global::System.Collections.Generic.List<")
            .Append(GeneratedTypeNames.IObservedChange).Append("<object, object>>(").Append(path.Length).Line(");");

        for (var i = 0; i < path.Length; i++)
        {
            AppendHookPropertySegment(sb, root, path, i);
        }

        _ = sb.Return(HookChangesResult)
            .CloseBlockInline();
    }

    /// <summary>Emits one guarded property read and the change describing it.</summary>
    /// <param name="sb">The writer, inside the reader's body.</param>
    /// <param name="root">The first owner in the path.</param>
    /// <param name="path">The property path being read.</param>
    /// <param name="index">The segment to emit.</param>
    internal static void AppendHookPropertySegment(
        SourceWriter sb,
        string root,
        EquatableArray<PropertyPathSegment> path,
        int index)
    {
        var segment = path[index];

        _ = sb.Append("var ").Append(HookOwnerPrefix).Append(index).Append(" = ");
        _ = index == 0 ? sb.Append(root) : sb.Append(HookValuePrefix).Append(index - 1);
        _ = sb.EndStatement()
            .BeginIf().Append(HookOwnerPrefix).Append(index).Append(" is null").CloseCondition()
            .Return(HookChangesResult)
            .CloseBlock()
            .Append("var ").Append(HookValuePrefix).Append(index).Append(" = ")
            .Append(HookOwnerPrefix).Append(index).Append('.').Append(segment.PropertyName).EndStatement()
            .Append(HookChangesName).Append(".Add(new ").Append(GeneratedTypeNames.ObservedChange).Append("<object, object>(")
            .Append(HookOwnerPrefix).Append(index).Append($", (({GeneratedTypeNames.Expression}<{GeneratedTypeNames.Func}<")
            .Append(segment.DeclaringTypeFullName).Append(", ").Append(segment.PropertyTypeFullName)
            .Append(">>)(__property => __property.").Append(segment.PropertyName).Append(")).Body, ")
            .Append(HookValuePrefix).Append(index).Line("));");
    }

    /// <summary>Groups call sites that can share one generated overload.</summary>
    /// <param name="invocations">The detected call sites.</param>
    /// <returns>The groups, in the order their signatures were first seen.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<BindingTypeGroup> GroupByTypeSignature(ImmutableArray<BindingInvocationInfo> invocations) =>
        SignatureGrouping.Group(
            invocations,
            static (key, inv) => _ = key
                .Append(inv.SourceTypeFullName).Append('|')
                .Append(inv.TargetTypeFullName).Append('|')
                .Append(inv.SourcePropertyTypeFullName).Append('|')
                .Append(inv.TargetPropertyTypeFullName).Append('|')
                .Append(inv.HasConversion).Append('|')
                .Append(inv.HasScheduler).Append('|')
                .Append(inv.HasConverterOverride),
            static (first, members) => new BindingTypeGroup(
                first.SourceTypeFullName,
                first.TargetTypeFullName,
                first.SourcePropertyTypeFullName,
                first.TargetPropertyTypeFullName,
                first.HasConversion,
                first.HasScheduler,
                members) { HasConverterOverride = first.HasConverterOverride });

    /// <summary>Writes the optional converters and scheduler of a two-way overload's parameter list.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="forwardName">What this API calls the source-to-target converter.</param>
    /// <param name="reverseName">What this API calls the target-to-source converter.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void AppendTwoWayExtraParameters(
        SourceWriter sb,
        BindingTypeGroup group,
        string forwardName,
        string reverseName,
        bool supportsNullable)
    {
        if (group.HasConversion)
        {
            AppendFuncParameter(sb, group.SourcePropertyTypeFullName, group.TargetPropertyTypeFullName, forwardName);
            AppendFuncParameter(sb, group.TargetPropertyTypeFullName, group.SourcePropertyTypeFullName, reverseName);
        }

        if (!group.HasScheduler)
        {
            return;
        }

        AppendSchedulerParameter(sb, supportsNullable);
    }

    /// <summary>Formats the extra arguments a two-way overload forwards to its generated method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <param name="forwardName">What this API calls the source-to-target converter.</param>
    /// <param name="reverseName">What this API calls the target-to-source converter.</param>
    /// <returns>The argument list fragment, or empty when there is nothing extra to forward.</returns>
    internal static string FormatTwoWayExtraArgs(BindingTypeGroup group, string forwardName, string reverseName)
    {
        if (!group.HasConversion && !group.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (group.HasConversion)
        {
            _ = sb.Append(", ").Append(forwardName).Append(", ").Append(reverseName);
        }

        if (group.HasScheduler)
        {
            _ = sb.Append(", scheduler");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Formats the extra parameters a two-way generated method declares.</summary>
    /// <param name="inv">The call site, whose own flags decide what the method takes.</param>
    /// <param name="forwardName">What this API calls the source-to-target converter.</param>
    /// <param name="reverseName">What this API calls the target-to-source converter.</param>
    /// <returns>The parameter list fragment, or empty when there is nothing extra to declare.</returns>
    internal static string FormatTwoWayExtraMethodParams(
        BindingInvocationInfo inv,
        string forwardName,
        string reverseName)
    {
        if (!inv.HasConversion && !inv.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (inv.HasConversion)
        {
            _ = sb.Append(FuncParameterPrefix)
                .Append(inv.SourcePropertyTypeFullName).Append(", ").Append(inv.TargetPropertyTypeFullName)
                .Append("> ").Append(forwardName)
                .Append(FuncParameterPrefix)
                .Append(inv.TargetPropertyTypeFullName).Append(", ").Append(inv.SourcePropertyTypeFullName)
                .Append("> ").Append(reverseName);
        }

        if (inv.HasScheduler)
        {
            _ = sb.Append(", ").Append(GeneratedTypeNames.ISequencer).Append(SchedulerDeclarationSuffix);
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Writes the optional conversion and scheduler parameters of an overload's parameter list.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="conversionParameterName">What this API calls its conversion argument.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void AppendExtraParameters(
        SourceWriter sb,
        BindingTypeGroup group,
        string conversionParameterName,
        bool supportsNullable)
    {
        if (group.HasConversion)
        {
            AppendFuncParameter(sb, group.SourcePropertyTypeFullName, group.TargetPropertyTypeFullName, conversionParameterName);
        }

        if (!group.HasScheduler)
        {
            return;
        }

        AppendSchedulerParameter(sb, supportsNullable);
    }

    /// <summary>Writes the required scheduler parameter, which the stub declares nullable.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendSchedulerParameter(SourceWriter sb, bool supportsNullable) =>
        sb.Append(GeneratedTypeNames.ISequencer).Line(supportsNullable ? "? scheduler," : " scheduler,");

    /// <summary>Writes a conversion delegate parameter that another parameter follows.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="fromType">The type the delegate takes.</param>
    /// <param name="toType">The type the delegate returns.</param>
    /// <param name="name">The parameter's name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendFuncParameter(SourceWriter sb, string fromType, string toType, string name) =>
        sb.Append(GeneratedSyntax.FuncTypeOpen).Append(fromType).Append(", ").Append(toType).Append("> ").Append(name).Line(",");

    /// <summary>Formats the extra arguments for forwarding to the generated binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <param name="conversionParameterName">What this API calls its conversion argument.</param>
    /// <returns>The argument list fragment, or empty when there is nothing extra to forward.</returns>
    internal static string FormatExtraArgs(BindingTypeGroup group, string conversionParameterName)
    {
        if (!group.HasConversion && !group.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (group.HasConversion)
        {
            _ = sb.Append(", ").Append(conversionParameterName);
        }

        if (group.HasScheduler)
        {
            _ = sb.Append(", scheduler");
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Formats the extra parameters for the generated binding method's own signature.</summary>
    /// <param name="inv">The call site, whose own flags decide what the method takes.</param>
    /// <param name="conversionParameterName">What this API calls its conversion argument.</param>
    /// <returns>The parameter list fragment, or empty when there is nothing extra to declare.</returns>
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv, string conversionParameterName)
    {
        if (!inv.HasConversion && !inv.HasScheduler)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder();

        if (inv.HasConversion)
        {
            _ = sb.Append(FuncParameterPrefix)
                .Append(inv.SourcePropertyTypeFullName)
                .Append(", ")
                .Append(inv.TargetPropertyTypeFullName)
                .Append("> ")
                .Append(conversionParameterName);
        }

        if (inv.HasScheduler)
        {
            _ = sb.Append(", ").Append(GeneratedTypeNames.ISequencer).Append(SchedulerDeclarationSuffix);
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Determines whether the two sides differ in type with no converter supplied to reconcile them.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns><see langword="true"/> when the conversion has to come from the registry.</returns>
    /// <remarks>
    /// A number bound to a text property is the archetypal binding, and the converter registry exists to serve
    /// it. Assigning straight across instead is a type error inside a generated file, where the consumer can
    /// neither see nor fix it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool RequiresRegistryConversion(BindingInvocationInfo inv) =>
        !inv.HasConversion
        && (inv.ForwardConversion is not null || !string.Equals(inv.SourcePropertyTypeFullName, inv.TargetPropertyTypeFullName, StringComparison.Ordinal));

    /// <summary>
    /// Writes the subscription that assigns each value to the end of a property path, reporting a faulting write
    /// against the expression the call site bound.
    /// </summary>
    /// <param name="sb">The writer, part way through the statement the subscription belongs to.</param>
    /// <param name="observable">The local holding the values to write.</param>
    /// <param name="root">The object the path is walked from.</param>
    /// <param name="path">The property path the values are written to.</param>
    /// <param name="reportedExpression">The expression a faulting write is reported against.</param>
    /// <remarks>Ends the statement.</remarks>
    internal static void AppendWriteSubscription(
        SourceWriter sb,
        string observable,
        string root,
        EquatableArray<PropertyPathSegment> path,
        string reportedExpression)
    {
        _ = sb.Append(GeneratedTypeNames.BindingErrors).Append(".Subscribe(").Append(observable).Line(", value =>").OpenBlock();
        CodeGeneratorHelpers.AppendGuardedAssignment(sb, root, path, "value");
        _ = sb.CloseBlockInline().Append(", ").AppendQuoted(reportedExpression).Line(");");
    }

    /// <summary>Emits the stage that delivers a write on the owning thread of the object it lands on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceVar">The variable holding the values being written.</param>
    /// <param name="resultVar">The name to give the routed observable.</param>
    /// <param name="targetVar">The worker parameter naming the object the write lands on.</param>
    /// <param name="invoker">The invoker class that object's type carries, or null for none.</param>
    /// <returns>The variable to subscribe the write to.</returns>
    internal static string EmitViewThreadStage(
        SourceWriter sb,
        BindingInvocationInfo inv,
        string sourceVar,
        string resultVar,
        string targetVar,
        string? invoker)
    {
        // A scheduler named at the call site decides where the write lands. The parameter is nullable, and a call
        // site that passes null or leaves it out leaves the write to the thread that owns the target.
        if (inv.HasScheduler)
        {
            _ = AppendViewThreadCall(sb.BeginVar(resultVar).Append("scheduler == null ? "), sourceVar, targetVar, invoker)
                .Append(" : ").Append(sourceVar).EndStatement();

            return resultVar;
        }

        _ = AppendViewThreadCall(sb.BeginVar(resultVar), sourceVar, targetVar, invoker).EndStatement();

        return resultVar;
    }

    /// <summary>Appends the call that routes an observable onto the owning thread of the object it writes to.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceVar">The variable holding the values being written.</param>
    /// <param name="targetVar">The variable naming the object the write lands on.</param>
    /// <param name="invoker">The invoker class that object's type carries, or null for none.</param>
    /// <returns>The string builder.</returns>
    internal static SourceWriter AppendViewThreadCall(SourceWriter sb, string sourceVar, string targetVar, string? invoker)
    {
        _ = sb.Append(GeneratedTypeNames.BindingSchedulers).Append(".ObserveOnViewThread(").Append(sourceVar).Append(", ").Append(targetVar);

        return invoker is null
            ? sb.Append(')')
            : sb.Append(", ").Append(invoker).Append(".Instance)");
    }

    /// <summary>Decides what a view-first binding observes: the view model it was handed, or the view's own.</summary>
    /// <param name="inv">The call site being emitted.</param>
    /// <param name="sourceClassInfo">The view model type's binding info, when it was detected.</param>
    /// <param name="targetClassInfo">The view type's binding info, which lists the properties it declares.</param>
    /// <returns>The root to observe from, the path to walk, and the root type's binding info.</returns>
    /// <remarks>
    /// A view that exposes the view model is observed through that property, so the binding follows whichever
    /// view model the view currently holds. Setting the view model after construction and replacing it later is
    /// the ordinary view lifecycle, and a binding that captured the instance it was handed goes on driving the
    /// view from the replaced one with nothing raised to say so.
    /// <para>
    /// The rewritten path is the same shape the chain emitters already handle - one more segment, whose parent
    /// may be null - so the switch onto the current view model, and the detach from the previous one, come from
    /// the machinery a deep chain already uses.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ViewModelObservation ResolveViewModelObservation(
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo) =>
        ResolveViewModelObservation(
            inv.SourceTypeFullName,
            inv.TargetTypeFullName,
            inv.SourcePropertyPath,
            sourceClassInfo,
            targetClassInfo);

    /// <summary>Decides what a view-first API observes: the view model it was handed, or the view's own.</summary>
    /// <param name="viewModelTypeFullName">The fully qualified view model type the call site named.</param>
    /// <param name="viewTypeFullName">The fully qualified view type the call site was made on.</param>
    /// <param name="viewModelPropertyPath">The path the call site walks from the view model.</param>
    /// <param name="viewModelClassInfo">The view model type's binding info, when it was detected.</param>
    /// <param name="viewClassInfo">The view type's binding info, which lists the properties it declares.</param>
    /// <returns>The root to observe from, the path to walk, and the root type's binding info.</returns>
    /// <remarks>
    /// Every view-first API resolves this the same way, so the property binders, the command binder and the
    /// interaction binder share one answer rather than each deciding for itself. A registration or a view model
    /// swap that reached a bound property but not a bound command would be the kind of divergence nobody could
    /// see from the call site.
    /// </remarks>
    internal static ViewModelObservation ResolveViewModelObservation(
        string viewModelTypeFullName,
        string viewTypeFullName,
        EquatableArray<PropertyPathSegment> viewModelPropertyPath,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo)
    {
        var declaredType = viewClassInfo is null ? null : FindViewModelPropertyType(viewClassInfo);
        if (declaredType is null)
        {
            return new("viewModel", viewModelPropertyPath, viewModelClassInfo);
        }

        // The stage below is typed as the view model the call site named, so a view exposing it as a base or
        // an interface has to narrow on the way out. Observables convert the other way, so the read does it.
        var readCast = string.Equals(declaredType, viewModelTypeFullName, StringComparison.Ordinal)
            ? null
            : viewModelTypeFullName;

        // The view declares this property, so the view's mechanism is the one that reports it changing. Taking
        // the view model's would observe the wrong type - a dependency-object view holding a notifying view
        // model would be watched as though the view notified the way its view model does.
        var viewModelSegment = new PropertyPathSegment(
            ViewModelPropertyName,
            viewModelTypeFullName,
            viewTypeFullName,
            true,
            viewClassInfo,
            readCast);

        var rooted = new PropertyPathSegment[viewModelPropertyPath.Length + 1];
        rooted[0] = viewModelSegment;
        for (var i = 0; i < viewModelPropertyPath.Length; i++)
        {
            rooted[i + 1] = viewModelPropertyPath[i];
        }

        return new("view", new(rooted), viewClassInfo);
    }

    /// <summary>Emits the whole concrete typed overload one binding API dispatches through.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group, which fixes the bound types for the whole overload.</param>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    /// <param name="dispatchesOnExpressionText">Whether dispatch keys on expression text rather than file and line.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    /// <remarks>
    /// The four property-binding APIs emit one overload shape between them: a receiver, the object it binds
    /// against, a selector per side, whatever conversion and scheduler arguments the API takes, and then the
    /// dispatch table. What actually differs is the naming and the trailing arguments, which
    /// <paramref name="api"/> carries, so the shape is written here once.
    /// </remarks>
    internal static void GenerateDispatchOverload(
        SourceWriter sb,
        BindingTypeGroup group,
        BindingDispatchApi api,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        CodeGeneratorHelpers.AppendDispatchSummary(
            sb,
            api.Name,
            group.SourceTypeFullName,
            group.TargetTypeFullName,
            dispatchesOnExpressionText);

        _ = sb.Append("public static ").Append(api.FormatReturnType(group)).Append(' ').Append(api.Name).OpenParameterList();

        AppendParameterList(sb, group, api, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

        _ = sb.OpenBlock();

        if (dispatchesOnExpressionText)
        {
            AppendExpressionDispatchBody(sb, group, api);
        }
        else
        {
            AppendCallerInfoDispatchBody(sb, group, api);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Writes the parameters a binding member declares, closing the list.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's output from the other three.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature: the
    /// overload only wins resolution against a candidate it is otherwise indistinguishable from, and an
    /// interceptor is refused outright unless its signature is the intercepted method's.
    /// </remarks>
    internal static void AppendParameterList(
        SourceWriter sb,
        BindingTypeGroup group,
        BindingDispatchApi api,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var first = group.Invocations[0];
        var sourceLeaf = CodeGeneratorHelpers.NullableSelectorLeafType(first.SourcePropertyPath, supportsNullable);
        var targetLeaf = CodeGeneratorHelpers.NullableSelectorLeafType(first.TargetPropertyPath, supportsNullable);

        _ = sb.Append("this ").Append(api.ReceiverIsTarget ? group.TargetTypeFullName : group.SourceTypeFullName)
            .Append(' ').Append(api.ReceiverParameterName).Line(",")
            .Append(api.ReceiverIsTarget
                ? CodeGeneratorHelpers.NullableSelectorType(group.SourceTypeFullName, true, supportsNullable)
                : group.TargetTypeFullName)
            .Append(' ').Append(api.OtherParameterName).Line(",")
            .Append(GeneratedSyntax.SelectorTypeOpen).Append(group.SourceTypeFullName).Append(", ").Append(sourceLeaf)
            .Append(">> ").Append(api.SourceSelectorName).Line(",")
            .Append(GeneratedSyntax.SelectorTypeOpen).Append(group.TargetTypeFullName).Append(", ").Append(targetLeaf)
            .Append(">> ").Append(api.TargetSelectorName).Line(",");

        if (group.HasConverterOverride)
        {
            AppendConverterOverrideParameters(sb, api, supportsNullable);
        }
        else
        {
            api.AppendExtraParameters(sb, group, supportsNullable);

            if (dispatchesOnExpressionText || stubHasExpressionParameters)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(sb, api.SourceSelectorName, api.SourceExpressionParameter, dispatchesOnExpressionText);
                CodeGeneratorHelpers.AppendExpressionParameter(sb, api.TargetSelectorName, api.TargetExpressionParameter, dispatchesOnExpressionText);
            }
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Writes the converter, hint and scheduler parameters a converter overload declares.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="api">What names this API gives its converters.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <remarks>
    /// The stub declares the hint and the scheduler as optional and no expression parameters at all, so the
    /// overload does the same: a call that leaves them out only resolves to this overload if it can leave them
    /// out too, and it can only be told apart from the stub by file and line.
    /// </remarks>
    internal static void AppendConverterOverrideParameters(SourceWriter sb, BindingDispatchApi api, bool supportsNullable)
    {
        var annotation = supportsNullable ? "?" : string.Empty;

        _ = sb.Append(GeneratedTypeNames.IBindingTypeConverter).Append(' ').Append(api.OverrideForwardName).Line(",");

        if (api.OverrideReverseName is not null)
        {
            _ = sb.Append(GeneratedTypeNames.IBindingTypeConverter).Append(' ').Append(api.OverrideReverseName).Line(",");
        }

        _ = sb.Append("object").Append(annotation).Append(' ').Append(ConversionHintName).Line(" = null,")
            .Append(GeneratedTypeNames.ISequencer).Append(annotation).Line(" scheduler = null,");
    }

    /// <summary>Formats the arguments a dispatch forwards to a generated worker beyond the two bound objects.</summary>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    /// <param name="group">The binding type group.</param>
    /// <returns>The argument list fragment, or empty when there is nothing extra to forward.</returns>
    internal static string FormatDispatchArguments(BindingDispatchApi api, BindingTypeGroup group) =>
        group.HasConverterOverride
            ? FormatConverterOverrideArguments(api)
            : api.FormatExtraArguments(group);

    /// <summary>Formats the converter, hint and scheduler arguments a converter overload forwards.</summary>
    /// <param name="api">What names this API gives its converters.</param>
    /// <returns>The argument list fragment.</returns>
    internal static string FormatConverterOverrideArguments(BindingDispatchApi api) =>
        api.OverrideReverseName is null
            ? $", {api.OverrideForwardName}, {ConversionHintName}, scheduler"
            : $", {api.OverrideForwardName}, {api.OverrideReverseName}, {ConversionHintName}, scheduler";

    /// <summary>Formats the converter, hint and scheduler parameters a generated worker declares.</summary>
    /// <param name="api">What names this API gives its converters.</param>
    /// <returns>The parameter list fragment.</returns>
    internal static string FormatConverterOverrideParameters(BindingDispatchApi api)
    {
        var sb = new PooledStringBuilder(CodeGeneratorHelpers.FragmentBufferCapacity);

        _ = sb.Append(", ").Append(GeneratedTypeNames.IBindingTypeConverter).Append(' ').Append(api.OverrideForwardName);

        if (api.OverrideReverseName is not null)
        {
            _ = sb.Append(", ").Append(GeneratedTypeNames.IBindingTypeConverter).Append(' ').Append(api.OverrideReverseName);
        }

        return sb.Append(", object ").Append(ConversionHintName).Append(", ").Append(GeneratedTypeNames.ISequencer).Append(SchedulerDeclarationSuffix)
            .ToStringAndReturn();
    }

    /// <summary>Emits whichever of the two ways this group's call sites are reached.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's output from the other three.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <remarks>
    /// The four APIs choose between the same two shapes on the same condition, so the choice is made here
    /// rather than repeated at each of their four call sites.
    /// </remarks>
    internal static void EmitOverloadOrInterceptors(
        SourceWriter sb,
        BindingTypeGroup group,
        BindingDispatchApi api,
        in LanguageFeatures features)
    {
        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, group, api, in features);
            return;
        }

        GenerateDispatchOverload(
            sb,
            group,
            api,
            features.SupportsCallerArgExpr && !group.HasConverterOverride,
            features.SupportsNullable,
            features.StubHasExpressionParameters);
    }

    /// <summary>Emits one interceptor per generated worker, claiming every call site that reaches it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's output from the other three.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <remarks>
    /// The signature is the overload's, because the compiler refuses an interceptor whose signature is not the
    /// intercepted method's. What the interceptor does with it differs: the call site is already known, so the
    /// dispatch parameters go unread and the body forwards straight to the worker.
    /// </remarks>
    internal static void GenerateInterceptors(
        SourceWriter sb,
        BindingTypeGroup group,
        BindingDispatchApi api,
        in LanguageFeatures features)
    {
        var extraArguments = FormatDispatchArguments(api, group);
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr && !group.HasConverterOverride;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in GroupCallSitesByWorker(group))
        {
            foreach (var callSite in entry.Value)
            {
                InterceptorEmitter.AppendAttribute(sb, callSite.Interceptor);
            }

            _ = sb.Append("internal static ").Append(api.FormatReturnType(group)).Append(" __Intercept_")
                .Append(api.Name).Append('_').Append(entry.Key).OpenParameterList();

            AppendParameterList(sb, group, api, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            _ = sb.Indent()
                .Append("=> ").Append(api.WorkerMethodPrefix).Append(entry.Key).Append('(')
                .Append(api.WorkerArguments).Append(extraArguments).Line(");")
                .Outdent()
                .BlankLine();
        }
    }

    /// <summary>Emits the head of a generated worker: its signature, the path it binds, and the hook guard.</summary>
    /// <param name="sb">The writer, at the class's member level; left inside the worker's body.</param>
    /// <param name="api">What distinguishes this API's worker from the other three.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <param name="suffix">The stable method-name suffix for this call site.</param>
    /// <remarks>
    /// Every binding worker opens the same way - it takes the two objects and whatever conversion and scheduler
    /// arguments its API declares, records the path it binds, and offers the binding to a registered hook. What
    /// varies is the naming and the direction, which <paramref name="api"/> carries.
    /// </remarks>
    internal static void AppendWorkerMethodHeader(
        SourceWriter sb,
        BindingDispatchApi api,
        BindingInvocationInfo inv,
        string suffix)
    {
        _ = sb.Append("private static ").Append(api.FormatWorkerReturnType(inv)).Append(' ').Append(api.WorkerMethodPrefix).Append(suffix)
            .Append('(').Append(inv.SourceTypeFullName).Append(' ').Append(api.WorkerSourceParameterName).Append(", ")
            .Append(inv.TargetTypeFullName).Append(' ').Append(api.WorkerTargetParameterName)
            .Append(inv.HasConverterOverride ? FormatConverterOverrideParameters(api) : api.FormatWorkerParameters(inv))
            .Line(")").OpenBlock()
            .BeginComment().Append(api.Name).Append(": ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.SourcePropertyPath)
            .Append(api.IsTwoWay ? " <-> " : " -> ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.TargetPropertyPath)
            .Append(inv.HasConversion ? " (with conversion)" : string.Empty)
            .Append(inv.HasConverterOverride ? " (with converter)" : string.Empty)
            .Append(inv.HasScheduler ? " (with scheduler)" : string.Empty).EndLine();

        EmitBindingHookGuard(
            sb,
            api.WorkerSourceParameterName,
            inv.SourcePropertyPath,
            api.WorkerTargetParameterName,
            inv.TargetPropertyPath,
            api.IsTwoWay ? "TwoWay" : "OneWay",
            api.HookRefusalValue);
    }

    /// <summary>Emits the conversion and scheduler stages a one-directional binding puts its values through.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The local the subscription should read from.</returns>
    internal static string EmitSingleStreamStages(SourceWriter sb, BindingDispatchApi api, BindingInvocationInfo inv)
    {
        var forward = ForwardStage(api, inv);
        var currentVar = forward.ObservableName;

        if (inv.HasConverterOverride)
        {
            ConversionEmitter.EmitStage(
                sb,
                currentVar,
                ConvertedForwardName,
                inv.SourcePropertyTypeFullName,
                inv.TargetPropertyTypeFullName,
                inv.ForwardConversion,
                new(ConversionHintName, api.OverrideForwardName));
            currentVar = ConvertedForwardName;
        }
        else if (inv.HasConversion)
        {
            currentVar = AppendMapStage(sb, forward, currentVar, inv.HasScheduler);
        }
        else if (inv.SetMethod is null && RequiresRegistryConversion(inv))
        {
            ConversionEmitter.EmitStage(sb, currentVar, ConvertedForwardName, inv.SourcePropertyTypeFullName, inv.TargetPropertyTypeFullName, inv.ForwardConversion);
            currentVar = ConvertedForwardName;
        }

        if (inv.HasScheduler)
        {
            currentVar = AppendObserveOnStage(sb, forward, currentVar, false);
        }

        return currentVar;
    }

    /// <summary>Emits those same stages for a binding that drives both sides.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The locals each direction's subscription should read from.</returns>
    /// <remarks>
    /// Each direction assigns across the same type gap in the opposite sense, so the two run the same stages
    /// with their type arguments and converter swapped.
    /// </remarks>
    internal static BindingObservables EmitDualStreamStages(SourceWriter sb, BindingDispatchApi api, BindingInvocationInfo inv)
    {
        var forward = ForwardStage(api, inv);
        var reverse = ReverseStage(api, inv);
        var sourceVar = forward.ObservableName;
        var targetVar = reverse.ObservableName;

        if (inv.HasConverterOverride)
        {
            ConversionEmitter.EmitStage(
                sb,
                sourceVar,
                ConvertedForwardName,
                inv.SourcePropertyTypeFullName,
                inv.TargetPropertyTypeFullName,
                inv.ForwardConversion,
                new(ConversionHintName, api.OverrideForwardName));
            ConversionEmitter.EmitStage(
                sb,
                targetVar,
                ConvertedReverseName,
                inv.TargetPropertyTypeFullName,
                inv.SourcePropertyTypeFullName,
                inv.ReverseConversion,
                new(ConversionHintName, api.OverrideReverseName ?? api.OverrideForwardName));
            sourceVar = ConvertedForwardName;
            targetVar = ConvertedReverseName;
        }
        else if (inv.HasConversion)
        {
            sourceVar = AppendMapStage(sb, forward, sourceVar, inv.HasScheduler);
            targetVar = AppendMapStage(sb, reverse, targetVar, inv.HasScheduler);
        }
        else if (RequiresRegistryConversion(inv))
        {
            ConversionEmitter.EmitStage(sb, sourceVar, ConvertedForwardName, inv.SourcePropertyTypeFullName, inv.TargetPropertyTypeFullName, inv.ForwardConversion);
            ConversionEmitter.EmitStage(sb, targetVar, ConvertedReverseName, inv.TargetPropertyTypeFullName, inv.SourcePropertyTypeFullName, inv.ReverseConversion);
            sourceVar = ConvertedForwardName;
            targetVar = ConvertedReverseName;
        }

        if (inv.HasScheduler)
        {
            sourceVar = AppendObserveOnStage(sb, forward, sourceVar, true);
            targetVar = AppendObserveOnStage(sb, reverse, targetVar, true);
        }

        return new(sourceVar, targetVar);
    }

    /// <summary>Gathers the call sites of a group under the worker each of them reaches.</summary>
    /// <param name="group">The binding type group whose call sites are being gathered.</param>
    /// <returns>Each generated worker, against every call site that resolves to it.</returns>
    /// <remarks>
    /// A call site the compiler declined to describe is left out: nothing can claim it, and it keeps whatever
    /// the call already resolved to.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<string, List<BindingInvocationInfo>> GroupCallSitesByWorker(BindingTypeGroup group) =>
        InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, BindingMethodSuffix);

    /// <summary>Finds the type a view declares its view model property as.</summary>
    /// <param name="targetClassInfo">The view type's binding info.</param>
    /// <returns>The declared property type, or <see langword="null"/> when the view exposes no such property.</returns>
    /// <remarks>
    /// A view implementing <c>IViewFor&lt;T&gt;</c> declares the property twice - once weakly typed for the
    /// non-generic interface, once as the view model itself. Only the typed declaration counts. The weak one
    /// says nothing about which view model the view holds, and following it would point the binding at a
    /// property the view may never have been given, where the call site handed the view model over directly.
    /// </remarks>
    private static string? FindViewModelPropertyType(ClassBindingInfo targetClassInfo)
    {
        var properties = targetClassInfo.Properties;

        for (var i = 0; i < properties.Length; i++)
        {
            if (!string.Equals(properties[i].PropertyName, ViewModelPropertyName, StringComparison.Ordinal))
            {
                continue;
            }

            var declaredType = properties[i].PropertyTypeFullName;
            if (!string.Equals(declaredType, WeaklyTypedViewModel, StringComparison.Ordinal))
            {
                return declaredType;
            }
        }

        return null;
    }

    /// <summary>Appends the body of an overload that matches a call site by the text of its selectors.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    private static void AppendExpressionDispatchBody(SourceWriter sb, BindingTypeGroup group, BindingDispatchApi api)
    {
        var extraArguments = api.FormatExtraArguments(group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            CodeGeneratorHelpers.AppendExpressionDispatchCondition(
                sb,
                i,
                api.SourceExpressionParameter,
                inv.SourceExpressionText,
                api.TargetExpressionParameter,
                inv.TargetExpressionText);
            CodeGeneratorHelpers.AppendDispatchReturn(
                sb,
                api.WorkerMethodPrefix + BindingMethodSuffix(inv),
                api.WorkerArguments + extraArguments);
        }
    }

    /// <summary>Appends the body of an overload that matches a call site by the file and line it sits on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="api">What distinguishes this API's overload from the other three.</param>
    private static void AppendCallerInfoDispatchBody(
        SourceWriter sb,
        BindingTypeGroup group,
        BindingDispatchApi api)
    {
        var extraArguments = FormatDispatchArguments(api, group);

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            CodeGeneratorHelpers.AppendCallerInfoDispatchBranch(
                sb,
                i,
                inv.CallerLineNumber,
                inv.CallerFilePath,
                api.WorkerMethodPrefix + BindingMethodSuffix(inv),
                api.WorkerArguments + extraArguments);
        }
    }

    /// <summary>Describes the direction that carries the source side's values to the target side.</summary>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The stage description.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BindingStreamStage ForwardStage(BindingDispatchApi api, BindingInvocationInfo inv) =>
        new(
            api.SourceObservableName,
            api.SourceConvertedName,
            api.SourceScheduledName,
            api.ForwardConverterArgument,
            inv.SourcePropertyTypeFullName,
            inv.TargetPropertyTypeFullName);

    /// <summary>Describes the direction that carries the target side's values back to the source side.</summary>
    /// <param name="api">What this API calls the locals along the way.</param>
    /// <param name="inv">The call site being emitted.</param>
    /// <returns>The stage description.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BindingStreamStage ReverseStage(BindingDispatchApi api, BindingInvocationInfo inv) =>
        new(
            api.TargetObservableName,
            api.TargetConvertedName,
            api.TargetScheduledName,
            api.ReverseConverterArgument,
            inv.TargetPropertyTypeFullName,
            inv.SourcePropertyTypeFullName);

    /// <summary>Appends the stage that runs one direction's values through the converter the call site named.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="stage">The direction being emitted.</param>
    /// <param name="sourceVar">The local holding the values to convert.</param>
    /// <param name="hasScheduler">Whether a scheduler stage follows, which decides what the result is called.</param>
    /// <returns>The local holding the converted values.</returns>
    private static string AppendMapStage(SourceWriter sb, in BindingStreamStage stage, string sourceVar, bool hasScheduler)
    {
        var resultVar = hasScheduler ? stage.ConvertedName : stage.ScheduledName;

        _ = sb.BeginVar(resultVar).Append("new ").Append(GeneratedTypeNames.MapSignal).Append('<')
            .Append(stage.FromTypeFullName).Append(", ").Append(stage.ToTypeFullName).Append(">(").Append(sourceVar)
            .Append(", ").Append(stage.ConverterArgument).Line(");");

        return resultVar;
    }

    /// <summary>Appends the stage that delivers one direction's values on the scheduler the call site named.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="stage">The direction being emitted.</param>
    /// <param name="sourceVar">The local holding the values to route.</param>
    /// <param name="latestWins">Whether a value still waiting on the scheduler is replaced by a newer one.</param>
    /// <returns>The local holding the routed values.</returns>
    /// <remarks>
    /// A null scheduler adds no stage here; the view-thread stage that follows routes those values. A two-way
    /// binding asks for the latest to win: each write raises the other side's change, and a queue that replays
    /// every value writes an older one back over a newer one, so the two sides hand each other stale values
    /// without end.
    /// </remarks>
    private static string AppendObserveOnStage(SourceWriter sb, in BindingStreamStage stage, string sourceVar, bool latestWins)
    {
        _ = sb.BeginVar(stage.ScheduledName)
            .Append($"scheduler == null || scheduler == {GeneratedTypeNames.ImmediateSequencer} ? (")
            .Append(GeneratedTypeNames.ObservableOf(stage.ToTypeFullName)).Append(')').Append(sourceVar).Append(" : ");

        _ = latestWins
            ? sb.Append(GeneratedTypeNames.BindingSchedulers).Append(".ObserveOnSequencer<").Append(stage.ToTypeFullName).Append(">(")
            : sb.Append("new ").Append(GeneratedTypeNames.WitnessOnSignal).Append('<').Append(stage.ToTypeFullName).Append(">(");

        _ = sb.Append(sourceVar).Line(", scheduler);");

        return stage.ScheduledName;
    }

    /// <summary>Names the generated worker a binding call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its worker is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BindingMethodSuffix(BindingInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            $"{inv.SourceExpressionText}|{inv.TargetExpressionText}");

    /// <summary>What a view-first binding observes, and from where.</summary>
    /// <param name="RootVariable">The generated method parameter the observation is rooted on.</param>
    /// <param name="Path">The property path walked from that root.</param>
    /// <param name="RootClassInfo">The root type's binding info, when it was detected.</param>
    internal readonly record struct ViewModelObservation(
        string RootVariable,
        EquatableArray<PropertyPathSegment> Path,
        ClassBindingInfo? RootClassInfo);

    /// <summary>One direction of a binding's value flow, and what the generated locals along it are called.</summary>
    /// <param name="ObservableName">The local the direction's observation is held in.</param>
    /// <param name="ConvertedName">The local the converted values are held in, ahead of a scheduler stage.</param>
    /// <param name="ScheduledName">The local the values are held in once the subscription can read them.</param>
    /// <param name="ConverterArgument">What the generated worker calls this direction's converter.</param>
    /// <param name="FromTypeFullName">The type the values arrive as.</param>
    /// <param name="ToTypeFullName">The type the assignment at the far end needs.</param>
    internal readonly record struct BindingStreamStage(
        string ObservableName,
        string ConvertedName,
        string ScheduledName,
        string ConverterArgument,
        string FromTypeFullName,
        string ToTypeFullName);

    /// <summary>Everything a per-call-site binding emitter needs about one resolved call site.</summary>
    /// <param name="Invocation">The call site being emitted.</param>
    /// <param name="SourceClassInfo">The source type's binding info, when it was detected.</param>
    /// <param name="TargetClassInfo">The target type's binding info, when it was detected.</param>
    /// <param name="Suffix">The stable method-name suffix for this call site.</param>
    /// <param name="Features">The consumer compilation's language-feature snapshot.</param>
    internal readonly record struct BindEmitContext(
        BindingInvocationInfo Invocation,
        ClassBindingInfo? SourceClassInfo,
        ClassBindingInfo? TargetClassInfo,
        string Suffix,
        LanguageFeatures Features);

    /// <summary>What distinguishes one binding API's generated dispatch overload from another's.</summary>
    /// <remarks>
    /// Held as an object rather than passed as arguments because the set travels together and would otherwise
    /// widen the emitter's signature past what any one call site can read.
    /// </remarks>
    internal sealed class BindingDispatchApi
    {
        /// <summary>Gets the name of the binding method this overload stands in for.</summary>
        internal string Name { get; init; } = string.Empty;

        /// <summary>Gets what the overload calls the object it extends.</summary>
        internal string ReceiverParameterName { get; init; } = string.Empty;

        /// <summary>Gets what the overload calls the object bound against the receiver.</summary>
        internal string OtherParameterName { get; init; } = string.Empty;

        /// <summary>Gets a value indicating whether the receiver is the side the binding writes to.</summary>
        internal bool ReceiverIsTarget { get; init; }

        /// <summary>Gets what the overload calls the selector for the side it reads from.</summary>
        internal string SourceSelectorName { get; init; } = string.Empty;

        /// <summary>Gets what the overload calls the selector for the side it writes to.</summary>
        internal string TargetSelectorName { get; init; } = string.Empty;

        /// <summary>Gets the prefix the generated worker for each call site is named with.</summary>
        internal string WorkerMethodPrefix { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the object it reads from.</summary>
        internal string WorkerSourceParameterName { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the object it writes to.</summary>
        internal string WorkerTargetParameterName { get; init; } = string.Empty;

        /// <summary>Gets a value indicating whether the binding drives both sides.</summary>
        internal bool IsTwoWay { get; init; }

        /// <summary>Gets what a generated worker returns when a registered hook refuses the binding.</summary>
        internal string HookRefusalValue { get; init; } = string.Empty;

        /// <summary>Gets the local a generated worker holds the source side's observation in.</summary>
        internal string SourceObservableName { get; init; } = string.Empty;

        /// <summary>Gets the local a generated worker holds the target side's observation in.</summary>
        internal string TargetObservableName { get; init; } = string.Empty;

        /// <summary>Gets the local the source side's converted values are held in, ahead of a scheduler stage.</summary>
        internal string SourceConvertedName { get; init; } = string.Empty;

        /// <summary>Gets the local the target side's converted values are held in, ahead of a scheduler stage.</summary>
        internal string TargetConvertedName { get; init; } = string.Empty;

        /// <summary>Gets the local the source side's values are held in once the subscription can read them.</summary>
        internal string SourceScheduledName { get; init; } = string.Empty;

        /// <summary>Gets the local the target side's values are held in once the subscription can read them.</summary>
        internal string TargetScheduledName { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the converter from the source side's type to the target's.</summary>
        internal string ForwardConverterArgument { get; init; } = string.Empty;

        /// <summary>Gets what a generated worker calls the converter from the target side's type to the source's.</summary>
        internal string ReverseConverterArgument { get; init; } = string.Empty;

        /// <summary>Gets what the converter overload calls its converter, or the converter that carries values to the target.</summary>
        internal string OverrideForwardName { get; init; } = string.Empty;

        /// <summary>Gets what the converter overload calls the converter that carries values back, or null for a one-way API.</summary>
        internal string? OverrideReverseName { get; init; }

        /// <summary>Gets the function rendering what the overload returns.</summary>
        internal Func<BindingTypeGroup, string> FormatReturnType { get; init; } =
            static _ => GeneratedTypeNames.IDisposable;

        /// <summary>Gets the function rendering what a generated worker returns.</summary>
        internal Func<BindingInvocationInfo, string> FormatWorkerReturnType { get; init; } =
            static _ => GeneratedTypeNames.IDisposable;

        /// <summary>Gets the function rendering the conversion and scheduler parameters a generated worker declares.</summary>
        internal Func<BindingInvocationInfo, string> FormatWorkerParameters { get; init; } =
            static _ => string.Empty;

        /// <summary>Gets the action appending the conversion and scheduler parameters this API takes.</summary>
        internal Action<SourceWriter, BindingTypeGroup, bool> AppendExtraParameters { get; init; } =
            static (_, _, _) => { };

        /// <summary>Gets the function rendering the conversion and scheduler arguments the worker takes.</summary>
        internal Func<BindingTypeGroup, string> FormatExtraArguments { get; init; } =
            static _ => string.Empty;

        /// <summary>Gets the two objects a generated worker binds, in its own parameter order.</summary>
        internal string WorkerArguments => $"{WorkerSourceParameterName}, {WorkerTargetParameterName}";

        /// <summary>Gets the parameter carrying the text of the selector for the side read from.</summary>
        internal string SourceExpressionParameter => SourceSelectorName + CodeGeneratorHelpers.ExpressionParameterSuffix;

        /// <summary>Gets the parameter carrying the text of the selector for the side written to.</summary>
        internal string TargetExpressionParameter => TargetSelectorName + CodeGeneratorHelpers.ExpressionParameterSuffix;
    }
}
