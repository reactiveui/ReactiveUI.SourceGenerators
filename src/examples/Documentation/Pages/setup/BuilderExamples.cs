// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Maui.Builder;
using ReactiveUI.Binding.Mixins;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Creates, configures and builds the ReactiveUI.Binding builder that an application starts with.</summary>
public static class BuilderExamples
{
    /// <summary>Creates a builder over the application-wide Splat locator.</summary>
    public static void CreateBuilder()
    {
        ReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        Console.WriteLine(builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(string)) is null);

        // Output:
        // True
    }

    /// <summary>Creates a builder over a resolver you own, through the resolver extension or the constructor.</summary>
    public static void CreateBuilderForResolver()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingBuilder fromResolver = resolver.CreateReactiveUIBindingBuilder();

        Console.WriteLine(ReferenceEquals(resolver, fromResolver.CurrentMutable));

        using ModernDependencyResolver other = new();
        ReactiveUIBindingBuilder direct = new(other, null);

        Console.WriteLine(ReferenceEquals(other, direct.CurrentMutable));

        // Output:
        // True
        // True
    }

    /// <summary>Shows that binding refuses to run before <c>BuildApp</c>.</summary>
    public static void EnsureInitializedNeedsBuildApp()
    {
        try
        {
            RxBindingBuilder.EnsureInitialized();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Call BuildApp first");
        }

        // Output:
        // Call BuildApp first
    }

    /// <summary>Builds the to-do application: core services, the MAUI module, a module of its own, converters, a command binder, a provider and view mappings.</summary>
    /// <param name="provider">The provider that observes <see cref="TodoItem"/> properties.</param>
    /// <param name="binder">The binder that attaches commands to buttons.</param>
    /// <returns>The built application instance.</returns>
    public static IReactiveUIBindingInstance BuildTodoApplication(TodoItemObservableForProperty provider, ButtonCommandBinder binder)
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        return builder
            .WithCoreServices()
            .WithMaui()
            .WithPlatformModule(new TodoModule())
            .WithConverter(new TagListToStringConverter())
            .WithFallbackConverter(new EnumNameFallbackConverter())
            .WithSetMethodConverter(new TagListSetMethodConverter())
            .WithCommandBinder(binder)
            .WithRegistration(resolver => resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => provider))
            .ConfigureViewLocator(static mappings => mappings.Map<TodoListViewModel, TodoView>())
            .BuildApp();
    }

    /// <summary>Configures a <see cref="ReactiveUIBindingBuilder"/> held by its own type, one member at a time; each registration reaches the resolver at once.</summary>
    public static void BuildWithBuilderMembers()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingBuilder builder = resolver.CreateReactiveUIBindingBuilder();
        TodoItemObservableForProperty provider = new();
        ButtonCommandBinder binder = new();

        IReactiveUIBindingBuilder core = ((IReactiveUIBindingBuilder)builder).WithCoreServices();
        IReactiveUIBindingBuilder module = builder.WithPlatformModule(new TodoModule());
        _ = builder.WithFallbackConverter(new EnumNameFallbackConverter());
        _ = builder.WithSetMethodConverter(new TagListSetMethodConverter());
        _ = builder.WithCommandBinder(binder);
        _ = builder.WithRegistration(registry => registry.RegisterLazySingleton<ICreatesObservableForProperty>(() => provider));
        _ = builder.ConfigureViewLocator(static mappings => mappings.Map<TodoListViewModel, TodoView>());

        IReactiveUIBindingInstance app = builder.BuildApp();

        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());

        Console.WriteLine(ReferenceEquals(core, builder));
        Console.WriteLine(ReferenceEquals(module, builder));
        Console.WriteLine(builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(string)) is not null);
        Console.WriteLine(builder.ConverterService.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string)) is EnumNameFallbackConverter);
        Console.WriteLine(builder.ConverterService.SetMethodConverters.TryGetConverter(typeof(List<string>), typeof(string)) is TagListSetMethodConverter);
        Console.WriteLine(ReferenceEquals(resolver.GetService<ICreatesCommandBinding>(), binder));
        Console.WriteLine(resolver.GetServices<ICreatesObservableForProperty>().Any(service => ReferenceEquals(service, provider)));
        Console.WriteLine(resolver.GetService<IViewLocator>()!.ResolveView(viewModel) is TodoView);
        Console.WriteLine(app.Current is not null);

        // Output:
        // True
        // True
        // True
        // True
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Shows that <c>BuildApp</c> marks binding as initialized.</summary>
    /// <param name="app">The application returned by <see cref="BuildTodoApplication"/>.</param>
    public static void EnsureInitializedAfterBuildApp(IReactiveUIBindingInstance app)
    {
        RxBindingBuilder.EnsureInitialized();

        Console.WriteLine(app.Current is not null);

        // Output:
        // True
    }

    /// <summary>Builds an application through the <see cref="IAppBuilder"/> extension.</summary>
    public static void BuildThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingInstance app = appBuilder.BuildApp();

        Console.WriteLine(app.Current is not null);

        // Output:
        // True
    }
}
