// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Mechanisms;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

const string MediaBucket = "acme-media";

const string PhotoFolder = "photos/2026/";

InMemoryTodoStore todoStore = InMemoryTodoStore.CreateSeeded();

TodoListViewModel todoViewModel = new(todoStore);

await todoViewModel.LoadAsync();

TodoItem item = todoViewModel.Items[0];

TodoView todoView = new() { ViewModel = todoViewModel };

InMemoryObjectStorage storage = InMemoryObjectStorage.CreateSeeded();

IReadOnlyList<StorageObject> photos = await storage.ListObjectsAsync(MediaBucket, PhotoFolder);

StorageBrowserViewModel browser = new(storage);

StorageBrowserView browserView = new() { ViewModel = browser };

await browser.LoadBucketsAsync();

browser.SelectedBucket = browser.Buckets[0];

NotifyPropertyChangedExamples.RegisterCoreObservationServices();

NotifyPropertyChangedExamples.ObserveNotifyingClassThroughWhenChanged(item);

NotifyPropertyChangedExamples.ObserveNotifyingClassWithInpcProvider(item);

NotifyPropertyChangedExamples.ObservePlainClassWithInpcProvider(photos[0].Clone());

NotifyPropertyChangedExamples.ObservePlainClassWithPocoProvider(photos[0].Clone());

NotifyPropertyChangedExamples.AskFallbackProviderBeforeAndAfterChange();

NotifyPropertyChangedExamples.ObservePlainClassThroughWhenChanged(browser, photos[0].Clone(), photos[1].Clone());

NotifyPropertyChangingExamples.ObserveTitleBeforeItChanges();

NotifyPropertyChangingExamples.AskProviderAboutBeforeChangeNotifications();

BindableObjectExamples.ObserveEntryTextWithWhenChanged();

BindableObjectExamples.AskProvidersAboutEntry();

EventNotificationExamples.ObserveConnectionOpenWithoutRepeats();

EventNotificationExamples.ObserveConnectionOpenOnEveryEvent();

EventNotificationExamples.ObserveConnectionOpenThroughAnObserver();

ObservableTypesExamples.ObserveTodoUrgencyWithoutRepeats();

ObservableTypesExamples.ObserveTodoUrgencyOnEveryChange();

ObservableTypesExamples.ObserveTitleBeforeEachEdit();

ObservableTypesExamples.ObserveDraftTitleAfterChange();

ObservableTypesExamples.ObserveDraftTitleBeforeChange();

ObservableTypesExamples.ObserveStorageObjectKey();

ObservableTypesExamples.ReportAppliedChanges();

ObservableTypesExamples.ReadTwoWayBindingChanges();

ObservableTypesExamples.ObserveTodoUrgencyThroughAnObserver();

ObservableTypesExamples.ObserveTitleBeforeAnEditThroughAnObserver();

ObservableTypesExamples.ObserveDraftTitleThroughAnObserver();

ObservableTypesExamples.ObserveStorageObjectKeyThroughAnObserver();

ObservableTypesExamples.ReportAppliedChangesToAnObserver();

ObservableTypesExamples.KeepGeneratedObservationWhenNoProviderOutranksIt(item);

CombineLatestExamples.CombineTwoSources();

CombineLatestExamples.CombineThreeSources();

CombineLatestExamples.CombineFourFields();

CombineLatestExamples.CombineFiveFields();

CombineLatestExamples.CombineSixFields();

CombineLatestExamples.CombineSevenFields();

CombineLatestExamples.CombineEightFields();

CombineLatestExamples.CombineNineFields();

CombineLatestExamples.CombineTenFields();

CombineLatestExamples.CombineElevenFields();

CombineLatestExamples.CombineTwelveFields();

CombineLatestExamples.CombineThirteenFields();

CombineLatestExamples.CombineFourteenFields();

CombineLatestExamples.CombineFifteenFields();

CombineLatestExamples.CombineSixteenFields();

ClickCommandBinder binder = new();

CommandBinderExamples.BindButtonWithoutRegisteredBinder(todoViewModel, todoView);

CommandBinderExamples.RegisterBinderThroughBuilder(binder);

CommandBinderExamples.CompareBinderWithGeneratedBinding();

await CommandBinderExamples.BindButtonWithRegisteredBinder(todoViewModel, todoView);

using Signal<UploadRequest> uploads = new();

await CommandBinderExamples.BindButtonWithParameterStream(browser, browserView, uploads);

await CommandBinderExamples.BindButtonToNamedEvent(browser, browserView);

await CommandBinderExamples.BindWithDefaultEvent(browser, browserView, binder);

await CommandBinderExamples.BindWithExplicitEventHandlers(browser, browserView, storage, binder);

await CommandBinderExamples.InvokeCommandForEachValue(browser);

using Signal<UploadRequest> queuedUploads = new();

using Signal<ICommand?> commands = new();

await CommandBinderExamples.InvokeLatestCommand(browser, queuedUploads, commands);

TodoPropertyObservableForProperty tied = new(nameof(TodoItem.Notes), BindingAffinity.Explicit);

TodoPropertyObservableForProperty outranking = new(nameof(TodoItem.Title), BindingAffinity.Explicit + 1);

AffinityExamples.RegisterProviders(tied, outranking);

AffinityExamples.ObserveNotifyingClass(item);

AffinityExamples.ObservePlainClass(photos[0].Clone());

AffinityExamples.PickWinningMechanism();

AffinityExamples.CompareEveryAffinityScore();

AffinityExamples.ObserveWithTiedAndOutrankingProviders(item);

StorageConnectionObservableForProperty provider = new();

CustomProviderExamples.RegisterProviderThroughBuilder(provider);

await CustomProviderExamples.ObserveConnectionThroughProvider(browser, storage);

CustomProviderExamples.ObserveConnectionWithWhenChanged(browser, storage);

CustomProviderExamples.CallProviderThroughMixins(storage.Connection, provider);

CustomProviderExamples.CallProviderThroughItsInterface(storage.Connection);

CustomProviderExamples.FindProviderThatOutranksGeneratedMechanism();

CustomProviderExamples.ObserveThroughPluginPropertyObservable(storage.Connection, provider);

CustomProviderExamples.ObserveThroughPluginPropertyObservableWithAnObserver(storage.Connection, provider);

CustomProviderExamples.ChooseBetweenGeneratedObservationAndProvider(storage.Connection);

CustomProviderExamples.ObserveThroughExpressionChain(browser);

CustomProviderExamples.ObserveThroughExpressionChainWithAnObserver(browser);

CustomProviderExamples.ObserveThroughObservableForPropertySink(storage.Connection, provider);

CustomProviderExamples.ObserveThroughObservableForPropertySinkWithAnObserver(storage.Connection, provider);

ObservableTypesExamples.KeepGeneratedObservationOnEqualAffinity(provider);
