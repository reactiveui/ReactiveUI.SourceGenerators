// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Setup;
using ReactiveUI.Binding.Documentation.Todo;

BuilderExamples.CreateBuilder();

BuilderExamples.CreateBuilderForResolver();

BuilderExamples.EnsureInitializedNeedsBuildApp();

TodoItemObservableForProperty provider = new();

ButtonCommandBinder binder = new();

ReactiveUI.Binding.Builder.IReactiveUIBindingInstance app = BuilderExamples.BuildTodoApplication(provider, binder);

BuilderExamples.EnsureInitializedAfterBuildApp(app);

ServiceRegistrationExamples.ListObservationProviders(app);

ServiceRegistrationExamples.ResolveMauiViewThreadInvoker(app);

ITodoStore store = ServiceRegistrationExamples.ResolveModuleService(app);

TodoListViewModel viewModel = new(store);

await viewModel.LoadAsync();

TodoView view = new() { ViewModel = viewModel };

ServiceRegistrationExamples.ObserveThroughRegisteredProvider(viewModel.Items[0], provider);

ServiceRegistrationExamples.ConvertTagsWithRegisteredConverter(viewModel, view);

ServiceRegistrationExamples.ConvertPriorityWithRegisteredFallbackConverter();

ServiceRegistrationExamples.SetIndexedTagWithRegisteredSetMethodConverter();

ServiceRegistrationExamples.AttachCommandWithRegisteredBinder(viewModel, view, binder);

ServiceRegistrationExamples.ResolveMappedView(viewModel);

ServiceRegistrationExamples.RegisterCoreObservationModule();

ServiceRegistrationExamples.RegisterConvertersThroughAppBuilder();

ServiceRegistrationExamples.RegisterFallbackAndSetMethodConvertersThroughAppBuilder();

ServiceRegistrationExamples.ConfigureThroughAppBuilder();

BuilderExamples.BuildWithBuilderMembers();

BuilderExamples.BuildThroughAppBuilder();

AnalyzerPatternExamples.WritePathInTheCall();

AnalyzerPatternExamples.ObservePublicProperty();

await AnalyzerPatternExamples.ObservePathOfProperties();

AnalyzerPatternExamples.ObserveNotifyingType();

await AnalyzerPatternExamples.ObserveMirroredProperty();

AnalyzerPatternExamples.ObserveBeforeChange();

AnalyzerPatternExamples.BindCommandToButton();

AnalyzerPatternExamples.BindValidationSummary();

await AnalyzerPatternExamples.BindInteractionProperty();

await AnalyzerPatternExamples.ImportThePackageNamespace();

await AnalyzerPatternExamples.ObserveFromRootNamespace();
