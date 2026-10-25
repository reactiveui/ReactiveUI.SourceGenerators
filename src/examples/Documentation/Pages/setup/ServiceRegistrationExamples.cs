// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Maui;
using ReactiveUI.Binding.Mixins;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Registers services with the builder and shows the built application using them.</summary>
public static class ServiceRegistrationExamples
{
    /// <summary>The title an example gives the first item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>Reads the service that the module registered into the built application.</summary>
    /// <param name="app">The application returned by <see cref="BuilderExamples.BuildTodoApplication"/>.</param>
    /// <returns>The to-do database the module registered.</returns>
    /// <exception cref="InvalidOperationException">The module did not register the to-do database.</exception>
    public static ITodoStore ResolveModuleService(IReactiveUIBindingInstance app)
    {
        if (app.Current?.GetService<ITodoStore>() is not InMemoryTodoStore store)
        {
            throw new InvalidOperationException("The module did not register the to-do database.");
        }

        return store;
    }

    /// <summary>Reads the observation providers of the built application: the two core providers and the one the builder registered.</summary>
    /// <param name="app">The application returned by <see cref="BuilderExamples.BuildTodoApplication"/>.</param>
    public static void ListObservationProviders(IReactiveUIBindingInstance app)
    {
        Console.WriteLine(app.Current!.GetServices<ICreatesObservableForProperty>().Count());

        // Output:
        // 3
    }

    /// <summary>Reads the view thread invoker that <c>WithMaui</c> registered into the built application.</summary>
    /// <param name="app">The application returned by <see cref="BuilderExamples.BuildTodoApplication"/>.</param>
    public static void ResolveMauiViewThreadInvoker(IReactiveUIBindingInstance app)
    {
        Console.WriteLine(app.Current!.GetService<IViewThreadInvoker>() is DispatcherViewThreadInvoker);

        // Output:
        // True
    }

    /// <summary>Observes an item through the provider the builder registered; the provider outranks the generated observation.</summary>
    /// <param name="item">The item to observe.</param>
    /// <param name="provider">The provider registered by <see cref="BuilderExamples.BuildTodoApplication"/>.</param>
    public static void ObserveThroughRegisteredProvider(TodoItem item, TodoItemObservableForProperty provider)
    {
        using (item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(provider.ObservationCount);

        // Output:
        // Renew car registration
        // Renew car registration online
        // 1
    }

    /// <summary>Binds tags to a label; the typed converter the builder registered turns the list into text.</summary>
    /// <param name="viewModel">The view model that holds the selected item.</param>
    /// <param name="view">The view that shows the tags.</param>
    public static void ConvertTagsWithRegisteredConverter(TodoListViewModel viewModel, TodoView view)
    {
        viewModel.SelectedItem = viewModel.Items[0];

        using IReactiveBinding<TodoView, string> binding = view.OneWayBind(viewModel, x => x.SelectedItem!.Tags, v => v.TagsLabel.Text);

        Console.WriteLine(view.TagsLabel.Text);

        // Output:
        // car, admin
    }

    /// <summary>Converts an enumeration with the fallback converter the builder registered.</summary>
    public static void ConvertPriorityWithRegisteredFallbackConverter()
    {
        IBindingFallbackConverter? converter = BindingConverters.Current.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string));

        Console.WriteLine(converter is EnumNameFallbackConverter);

        _ = converter!.TryConvert(typeof(TodoPriority), TodoPriority.High, typeof(string), null, out var name);

        Console.WriteLine(name);

        // Output:
        // True
        // High
    }

    /// <summary>Writes a tag by index with the set-method converter the builder registered.</summary>
    public static void SetIndexedTagWithRegisteredSetMethodConverter()
    {
        ISetMethodBindingConverter? converter = BindingConverters.Current.ResolveSetMethodConverter(typeof(List<string>), typeof(string));

        Console.WriteLine(converter is TagListSetMethodConverter);

        List<string> tags = ["car", "admin"];
        _ = converter!.PerformSet(tags, "money", [1]);

        Console.WriteLine(string.Join(", ", tags));

        // Output:
        // True
        // car, money
    }

    /// <summary>Binds a command to a button; the command binder the builder registered attaches it.</summary>
    /// <param name="viewModel">The view model that owns the command.</param>
    /// <param name="view">The view that owns the button.</param>
    /// <param name="binder">The binder registered by <see cref="BuilderExamples.BuildTodoApplication"/>.</param>
    public static void AttachCommandWithRegisteredBinder(TodoListViewModel viewModel, TodoView view, ButtonCommandBinder binder)
    {
        using IDisposable binding = view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton);

        Console.WriteLine(binder.BindCount);
        Console.WriteLine(ReferenceEquals(viewModel.AddCommand, view.AddButton.Command));

        // Output:
        // 1
        // True
    }

    /// <summary>Finds the view for a view model through the mapping the builder registered.</summary>
    /// <param name="viewModel">The view model to find a view for.</param>
    public static void ResolveMappedView(TodoListViewModel viewModel)
    {
        IViewFor? view = ViewLocator.GetCurrent().ResolveView(viewModel);

        Console.WriteLine(view is TodoView);

        // Output:
        // True
    }

    /// <summary>Registers the core observation providers with the module, without a builder.</summary>
    public static void RegisterCoreObservationModule()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingModule module = new();

        module.Configure(resolver);

        List<Type> providers = resolver.GetServices<ICreatesObservableForProperty>().Select(static service => service.GetType()).ToList();

        Console.WriteLine(providers.Count);
        Console.WriteLine(providers.Contains(typeof(INPCObservableForProperty)));
        Console.WriteLine(providers.Contains(typeof(POCOObservableForProperty)));

        // Output:
        // 2
        // True
        // True
    }

    /// <summary>Registers the three kinds of converter through the <see cref="IAppBuilder"/> extensions.</summary>
    public static void RegisterConvertersThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingBuilder builder = resolver.CreateReactiveUIBindingBuilder();
        IAppBuilder appBuilder = builder;

        _ = appBuilder
            .WithConverter(new TagListToStringConverter())
            .WithFallbackConverter(new EnumNameFallbackConverter())
            .WithSetMethodConverter(new TagListSetMethodConverter());

        Console.WriteLine(builder.ConverterService.TypedConverters.TryGetConverter(typeof(IReadOnlyList<string>), typeof(string)) is TagListToStringConverter);
        Console.WriteLine(builder.ConverterService.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string)) is EnumNameFallbackConverter);
        Console.WriteLine(builder.ConverterService.SetMethodConverters.TryGetConverter(typeof(List<string>), typeof(string)) is TagListSetMethodConverter);

        // Output:
        // True
        // True
        // True
    }

    /// <summary>Registers a fallback converter and a set-method converter through the <see cref="IAppBuilder"/> extensions.</summary>
    public static void RegisterFallbackAndSetMethodConvertersThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingBuilder builder = resolver.CreateReactiveUIBindingBuilder();
        IAppBuilder appBuilder = builder;

        IReactiveUIBindingBuilder fallback = appBuilder.WithFallbackConverter(new EnumNameFallbackConverter());
        IReactiveUIBindingBuilder setMethod = appBuilder.WithSetMethodConverter(new TagListSetMethodConverter());

        Console.WriteLine(ReferenceEquals(builder, fallback));
        Console.WriteLine(ReferenceEquals(builder, setMethod));
        Console.WriteLine(builder.ConverterService.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string)) is EnumNameFallbackConverter);
        Console.WriteLine(builder.ConverterService.SetMethodConverters.TryGetConverter(typeof(List<string>), typeof(string)) is TagListSetMethodConverter);

        // Output:
        // True
        // True
        // True
        // True
    }

    /// <summary>Registers a service and view mappings through the <see cref="IAppBuilder"/> extensions.</summary>
    public static void ConfigureThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingBuilder builder = resolver.CreateReactiveUIBindingBuilder();
        IAppBuilder appBuilder = builder;

        Console.WriteLine(ReferenceEquals(builder, appBuilder.WithPlatformModule(new TodoModule())));

        _ = appBuilder
            .WithRegistration(static registry => registry.RegisterConstant(InMemoryTodoStore.CreateSeeded()))
            .ConfigureViewLocator(static mappings => mappings.Map<TodoListViewModel, TodoView>());

        IViewLocator? locator = resolver.GetService<IViewLocator>();
        TodoListViewModel viewModel = new(resolver.GetService<InMemoryTodoStore>()!);

        Console.WriteLine(locator!.ResolveView(viewModel) is TodoView);

        // Output:
        // True
        // True
    }
}
