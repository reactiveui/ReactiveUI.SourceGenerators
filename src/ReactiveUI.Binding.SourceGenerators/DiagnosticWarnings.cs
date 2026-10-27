// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Diagnostic descriptors shared between the source generator and analyzer projects.
/// The generator itself does NOT report diagnostics — the separate analyzer project does.
/// This file is linked into the analyzer project via Compile Include.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class DiagnosticWarnings
{
    /// <summary>The diagnostic category shared by all RXUIBIND descriptors.</summary>
    internal const string UsageCategory = "Usage";

    /// <summary>RXUIBIND001: Expression must be inline lambda for compile-time optimization.</summary>
    internal static readonly DiagnosticDescriptor NonInlineLambda = new(
        "RXUIBIND001",
        "Expression must be inline lambda",
        "Expression argument must be an inline lambda expression for compile-time optimization. A variable or "
        + "method reference is not generated, so the call throws unless it names the Unsafe overload.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        NoneInlineLambdaDescription);

    /// <summary>RXUIBIND002: Type has no observable properties.</summary>
    internal static readonly DiagnosticDescriptor NoObservableProperties = new(
        "RXUIBIND002",
        "Type has no observable properties",
        "Type '{0}' has no observable properties and does not implement any observable notification mechanism",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoObservablePropertiesDescription);

    /// <summary>RXUIBIND010: A type in the middle of an observed path raises no notification.</summary>
    internal static readonly DiagnosticDescriptor SilentPathLink = new(
        "RXUIBIND010",
        "Observed path passes through a type that raises no notification",
        "Type '{0}' raises no notification, so '{1}' is read once and the observation stops following the path there",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        SilentPathLinkDescription);

    /// <summary>RXUIBIND011: A binding call resolved to ReactiveUI's mixin rather than a generated overload.</summary>
    internal static readonly DiagnosticDescriptor MixinShadowsGeneratedBinding = new(
        "RXUIBIND011",
        "Binding call resolved to ReactiveUI's own mixin",
        "'{0}' resolved to ReactiveUI's '{1}', so this call generates nothing and takes the runtime expression engine",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        MixinShadowsGeneratedBindingDescription);

    /// <summary>RXUIBIND012: ToProperty targets a type whose change notifications generated code cannot raise.</summary>
    internal static readonly DiagnosticDescriptor UnraisableToPropertySource = new(
        "RXUIBIND012",
        "ToProperty source raises no notification generated code can reach",
        "Generated code cannot raise change notifications for '{0}', so this ToProperty call generates nothing and throws when it runs",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        UnraisableToPropertySourceDescription);

    /// <summary>RXUIBIND013: ToProperty names its property in a form the generator cannot read.</summary>
    internal static readonly DiagnosticDescriptor UnreadableToPropertyName = new(
        "RXUIBIND013",
        "ToProperty property must be named directly",
        "Name the property as 'x => x.Property' or as a constant such as nameof(Property); this call generates nothing and throws when it runs",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        UnreadableToPropertyNameDescription);

    /// <summary>RXUIBIND014: below C# 13, a ToProperty initial value passed by position makes the call ambiguous.</summary>
    internal static readonly DiagnosticDescriptor UnnamedToPropertyInitialValue = new(
        "RXUIBIND014",
        "Name the ToProperty initial value below C# 13",
        "Below C# 13 this initial value also fits a caller-information parameter, so the call is ambiguous; write it as 'initialValue: {0}'",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        UnnamedToPropertyInitialValueDescription);

    /// <summary>RXUIBIND015: a binding call names a type that generated code cannot reach.</summary>
    internal static readonly DiagnosticDescriptor UnreachableType = new(
        "RXUIBIND015",
        "Binding call names a type generated code cannot reach",
        "'{0}' is anonymous, file-local, private or protected, so no generated code claims this call; use an internal or public type, make the calling class partial, or call the Unsafe overload",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        UnreachableTypeDescription);

    /// <summary>RXUIBIND016: a binding call names a type parameter of the calling code.</summary>
    internal static readonly DiagnosticDescriptor TypeParameterCall = new(
        "RXUIBIND016",
        "Binding call is made through a type parameter",
        "'{0}' is built from a type parameter that no generated code can stand in for, so no generated code claims this call; call the Unsafe overload",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        TypeParameterCallDescription);

    /// <summary>RXUIBIND017: a binding writes to a UI object whose platform package, which marshals the write, is not referenced.</summary>
    internal static readonly DiagnosticDescriptor MissingViewThreadInvoker = new(
        "RXUIBIND017",
        "Binding writes to a UI object without its platform package",
        "'{0}' belongs to a UI thread, but '{1}' is not referenced, so writes from another thread are not marshalled onto it; reference '{1}'",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        MissingViewThreadInvokerDescription);

    /// <summary>RXUIBIND018: <c>[ObservableAsProperty]</c> marks something other than a partial get-only instance property.</summary>
    internal static readonly DiagnosticDescriptor ObservableAsPropertyNeedsPartialProperty = new(
        "RXUIBIND018",
        "ObservableAsProperty needs a partial get-only property",
        "'{0}' is not a partial get-only instance property, so nothing is generated for it; declare the property as 'partial T {0} {{ get; }}'",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        ObservableAsPropertyNeedsPartialPropertyDescription);

    /// <summary>RXUIBIND021: a binding call has no generated binding, so the runtime stub it resolves to throws.</summary>
    internal static readonly DiagnosticDescriptor NoGeneratedBinding = new(
        "RXUIBIND021",
        "Binding call has no generated binding",
        "'{0}' has no generated binding, so it throws at run time; name each member directly in the lambdas, declare a member another source generator adds as a partial property, or call {0}Unsafe",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        NoGeneratedBindingDescription);

    /// <summary>
    /// RXUIBIND020: a view is registered only in the service locator, which <c>ResolveView(object)</c> does not ask.
    /// Reported when the compilation ends, once every registration, view and mapping is known.
    /// </summary>
    internal static readonly DiagnosticDescriptor ServiceLocatorOnlyView = new(
        "RXUIBIND020",
        "View is registered only in the service locator",
        "IViewFor<{0}> is registered only in the service locator, so ResolveView with a view model held as an object does not find it; add it with Map, or call ResolveViewUnsafe",
        UsageCategory,
        DiagnosticSeverity.Info,
        true,
        ServiceLocatorOnlyViewDescription,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>RXUIBIND019: a method marked <c>[ObservableAsProperty]</c> takes parameters.</summary>
    internal static readonly DiagnosticDescriptor ObservableAsPropertyMethodHasParameters = new(
        "RXUIBIND019",
        "ObservableAsProperty method takes parameters",
        "'{0}' takes parameters, so it cannot supply a property's values; remove the parameters or the attribute",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        ObservableAsPropertyMethodHasParametersDescription);

    /// <summary>RXUIBIND003: Expression contains private/protected member.</summary>
    internal static readonly DiagnosticDescriptor PrivateMember = new(
        "RXUIBIND003",
        "Expression contains private or protected member",
        "Expression accesses private or protected member '{0}' which cannot be observed by a generated extension method",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        PrivateMemberDescription);

    /// <summary>RXUIBIND004: Type does not support before-change notifications.</summary>
    internal static readonly DiagnosticDescriptor NoBeforeChangeSupport = new(
        "RXUIBIND004",
        "Type does not support before-change notifications",
        "Type '{0}' does not support before-change notifications via {1}; WhenChanging reads the value once and then stays silent",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoBeforeChangeSupportDescription);

    /// <summary>RXUIBIND005: Source type implements INotifyDataErrorInfo.</summary>
    internal static readonly DiagnosticDescriptor ValidationNotGenerated = new(
        "RXUIBIND005",
        "Validation binding not generated",
        "Source type '{0}' implements INotifyDataErrorInfo; validation state propagation is not generated and requires runtime engine or manual ErrorsChanged subscription",
        UsageCategory,
        DiagnosticSeverity.Info,
        true,
        ValidationNotGeneratedDescription);

    /// <summary>RXUIBIND009: The generated dispatch for this call site is out of reach.</summary>
    internal static readonly DiagnosticDescriptor DispatchOutOfReach = new(
        "RXUIBIND009",
        "Generated binding dispatch is out of reach here",
        "This binding throws rather than reaching its generated code. The assembly exposes its internals, so on "
        + "C# 9 and below the generated dispatch has to live in the root namespace '{0}' to stay unambiguous, and "
        + "this file's namespace '{1}' is not under it. Move the file under the root namespace, raise the language "
        + "version to 10 or later, or name the Unsafe overload to resolve the expression at run time.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        DispatchOutOfReachDescription);

    /// <summary>RXUIBIND006: Expression contains unsupported path segment (indexer, static field, read-only leaf field, or method call).</summary>
    internal static readonly DiagnosticDescriptor UnsupportedPathSegment = new(
        "RXUIBIND006",
        "Expression contains unsupported path segment",
        "Expression contains '{0}' which is not a property or instance field access. Indexers, static fields, a read-only field at the end of "
        + "the path, and method calls are not generated, so the call throws unless it names the Unsafe overload.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true,
        UnsupportedPathSegmentDescription);

    /// <summary>RXUIBIND007: BindCommand control has no bindable event.</summary>
    internal static readonly DiagnosticDescriptor NoBindableEvent = new(
        "RXUIBIND007",
        "Control has no bindable event",
        "Control type '{0}' has no default bindable event (Click, TouchUpInside, Pressed) and no 'toEvent' was specified",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoBindableEventDescription);

    /// <summary>RXUIBIND008: Property does not implement IInteraction.</summary>
    internal static readonly DiagnosticDescriptor InvalidInteractionType = new(
        "RXUIBIND008",
        "Property is not an IInteraction",
        "Property '{0}' does not implement IInteraction<TInput, TOutput>",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        InvalidInteractionTypeDescription);

    /// <summary>The string description of the out-of-reach dispatch warning.</summary>
    private const string DispatchOutOfReachDescription =
        "Compile-time dispatch is chosen by extension-method lookup, which only reaches an overload declared "
        + "in a namespace enclosing the call site. Before C# 10 there is no global using to widen that, and an "
        + "assembly that exposes its internals cannot leave the overloads in the shared namespace without "
        + "risking an ambiguous call against the assembly it exposes them to. A file outside the root namespace "
        + "therefore binds to the stub, which throws and names the Unsafe overload that resolves the expression "
        + "by reflection.";

    /// <summary>The string description of the none inline lambda.</summary>
    private const string NoneInlineLambdaDescription =
        "The source generator can only optimize inline lambda expressions (e.g., x => x.Property). "
        + "A variable reference, a method call, or any other non-inline expression produces no generated dispatch, "
        + "so the call throws. Name the Unsafe overload - WhenChangedUnsafe, BindOneWayUnsafe and so on - to walk "
        + "the chain by reflection instead, which reports the requirement at the call site under trimming.";

    /// <summary>The string description of the no observable properties warning.</summary>
    private const string NoObservablePropertiesDescription =
        "The type used in the binding expression does not implement INotifyPropertyChanged, INotifyPropertyChanging, "
        + "IReactiveObject, or inherit from any known observable base type.";

    /// <summary>The string description of the private member warning.</summary>
    private const string PrivateMemberDescription =
        "The source generator generates extension methods which cannot access private or protected members. "
        + "No dispatch is emitted for the call, so it throws unless it names the Unsafe overload.";

    /// <summary>The string description of the no before-change support warning.</summary>
    private const string NoBeforeChangeSupportDescription =
        "The notification mechanism for this type does not provide before-change events. "
        + "WPF DependencyObjects, WinForms Components, and Android Views only support after-change notifications, "
        + "so the observation reports the value as it stands, once, and then nothing further.";

    /// <summary>The string description of the validation not generated warning.</summary>
    private const string ValidationNotGeneratedDescription =
        "The generated bindings handle value binding only. "
        + "Validation state propagation from INotifyDataErrorInfo requires the runtime ReactiveUI binding engine "
        + "or a manual ErrorsChanged subscription.";

    /// <summary>The string description of the unsupported path segment warning.</summary>
    private const string UnsupportedPathSegmentDescription =
        "The source generator reads chains of properties and instance fields (e.g., x => x.Foo.Bar, or v => v.NameBox.Text "
        + "through a control named in XAML). A field raises no notification, so it is read once. Indexers, static fields, "
        + "a read-only field at the end of the path, and method calls require runtime expression analysis.";

    /// <summary>The string description of the no bindable event warning.</summary>
    private const string NoBindableEventDescription =
        "The source generator could not find a default event to bind on the control type. "
        + "Specify the 'toEvent' parameter explicitly.";

    /// <summary>The string description of the invalid interaction type warning.</summary>
    private const string InvalidInteractionTypeDescription =
        "The property selected in the BindInteraction expression must implement IInteraction<TInput, TOutput>.";

    /// <summary>The string description of the shadowed binding warning.</summary>
    private const string MixinShadowsGeneratedBindingDescription =
        "Which method a binding call reaches is decided by extension-method lookup. Where ReactiveUI's own "
        + "namespace is imported and this package's is not, the call binds to ReactiveUI's mixin and the "
        + "generator never sees it: no dispatch is emitted, and the call takes the runtime expression engine "
        + "that a generated binding exists to avoid. Nothing else reports this, because the call was never "
        + "recognised as one to generate for. Import 'ReactiveUI.Binding' in the file to restore the generated "
        + "overload, which lookup prefers over the generic one.";

    /// <summary>The string description of the unraisable ToProperty source warning.</summary>
    private const string UnraisableToPropertySourceDescription =
        "A type can only raise its own events, so generated code needs a member that does it. Any one of these works: "
        + "derive from ReactiveUI's ReactiveObject or implement IReactiveObject; expose a public or internal "
        + "RaisePropertyChanged or OnPropertyChanged method; or declare the type, and every type it is nested in, "
        + "as partial, so the generator can add a member that raises the type's own event.";

    /// <summary>The string description of the ObservableAsProperty target warning.</summary>
    private const string ObservableAsPropertyNeedsPartialPropertyDescription =
        "The generator writes the body of a partial get-only instance property that you declare, so every generator in "
        + "the build can see the property. A field, a method or an observable property marked with the attribute, as "
        + "ReactiveUI's older source generator allowed, gets nothing. The code fix rewrites those as partial properties, "
        + "which needs C# 13.";

    /// <summary>The string description of the missing generated binding warning.</summary>
    private const string NoGeneratedBindingDescription =
        "The generator writes a binding for each call it can read, and the call runs that binding instead of the runtime "
        + "method it names. A call it cannot read still runs the runtime method, which throws. A generator reads your code "
        + "without any generator's output, so a member another source generator adds cannot be read. The members "
        + "ReactiveUI.SourceGenerators adds are the exception: their rules are known, so they are read. Declare other "
        + "generated members as partial properties, or call the Unsafe overload, which finds them by reflection. A call "
        + "through such a member is reported as a warning, because only the other generator could change it. Every "
        + "other cause is one you can fix, such as a selector that computes a value or a call written through the "
        + "class that declares the method, and fails the build.";

    /// <summary>The string description of the service-locator-only view information.</summary>
    private const string ServiceLocatorOnlyViewDescription =
        "ResolveView with a view model held as an object asks the generated view lookup and then the Map registrations. It "
        + "does not ask the service locator, because that needs a type built at run time. A view model whose view is "
        + "registered only in the service locator, with no generated view and no Map in this project, resolves to null "
        + "there. ResolveView<T> and ResolveViewUnsafe still ask the service locator.";

    /// <summary>The string description of the ObservableAsProperty method parameters warning.</summary>
    private const string ObservableAsPropertyMethodHasParametersDescription =
        "A method marked [ObservableAsProperty] supplies the observable a property is built from, so it is called with "
        + "no arguments.";

    /// <summary>The string description of the unnamed ToProperty initial value error.</summary>
    private const string UnnamedToPropertyInitialValueDescription =
        "The selector overloads of ToProperty take optional caller-information string parameters. For a string property, "
        + "an initial value passed by position fits both the overload with an initial value and the overload without one. "
        + "C# 13 and later prefer the initial-value overload; earlier versions need the argument named initialValue.";

    /// <summary>The string description of the unreadable ToProperty name warning.</summary>
    private const string UnreadableToPropertyNameDescription =
        "The generator reads the property's name from the call. A selector has to read one member straight off its "
        + "parameter, as in x => x.Property, and a string has to be a compile-time constant.";

    /// <summary>The string description of the missing view-thread invoker warning.</summary>
    private const string MissingViewThreadInvokerDescription =
        "A binding writes to a WPF, WinForms or MAUI object on the thread that owns it, through the invoker the "
        + "matching ReactiveUI.Binding platform package ships. Without that package the binding has no invoker for "
        + "the object, and a value arriving on another thread is written on that thread. Reference the platform package.";

    /// <summary>The string description of the type parameter call warning.</summary>
    private const string TypeParameterCallDescription =
        "Generated code names the closed types a call is made with. With Roslyn 4.13 or newer and interceptors on, a "
        + "call built from the calling code's type parameters is still generated, as code generic over them, when each "
        + "type parameter the call uses, and each one its constraints name, is itself one of the called method's type "
        + "arguments. Any other such call would throw when it runs, so the build fails at the call instead. Call the "
        + "Unsafe overload, which resolves the path at run time.";

    /// <summary>The string description of the unreachable type warning.</summary>
    private const string UnreachableTypeDescription =
        "Generated code lives in a class of its own, so it can only name types that are accessible from outside the "
        + "types that declare them. An anonymous type, a file-local type, a private or protected nested type, or a "
        + "type built from one is out of reach. With Roslyn 4.13 or newer and interceptors on, two kinds of call are "
        + "still generated. A "
        + "call whose only such type is its selector's result gets a generic interceptor. A call made inside a "
        + "partial, non-generic class that can name every type gets its code added to that class. Any other call "
        + "would throw when it runs, so the build fails at the call instead. Use an internal or public type, declare "
        + "the calling class and the classes around it partial, or call the Unsafe overload.";

    /// <summary>The string description of the silent path link warning.</summary>
    private const string SilentPathLinkDescription =
        "A type in the middle of an observed path that raises no notification is read once and never again, "
        + "so the observation stops following the path at that link and never sees a later value. "
        + "Give the type a notification mechanism, or observe a path that does not pass through it.";
}
