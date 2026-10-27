// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Views;

ViewForExamples.AssignTypedViewModel();

ViewForExamples.ObserveViewModelOfView();

ViewForExamples.AssignThroughNonGenericInterface();

ViewForExamples.RejectWrongViewModelType();

ViewForExamples.RecognizeActivatableViews();

await ViewForExamples.BindTodoView();

await ViewForExamples.BindBeforeViewModelArrives();

await ViewForExamples.BindAccountsView();

ViewLocatorExamples.RequireLocatorBeforeRegistration();

ViewLocatorExamples.RegisterDefaultLocator();

ViewLocatorExamples.ResolveTodoView();

ViewLocatorExamples.ResolveGitHubViewFromObject();

ViewLocatorExamples.ResolveTransferViewFromObject();

ViewLocatorExamples.ResolveObjectThroughInterface();

ViewLocatorExamples.ResolveMissingView();

ViewLocatorExamples.ResolveServiceLocatorViewFromObject();

ViewLocatorExamples.MapServiceLocatorViewForObjects();

ViewLocatorExamples.ResolveNullViewModel();

ViewLocatorExamples.ExplainScreenThatFailsToBuild();

ViewLocatorExamples.CreateNotFoundExceptionWithDefaultMessage();

ViewLocatorExamples.ResolveWithCustomLocator();

ViewContractExamples.ReadViewContract();

ViewContractExamples.ResolveViewByContract();

ViewContractExamples.ResolveBankingViewByContract();

ViewContractExamples.ResolveUnclaimedContract();

GeneratedViewDispatchExamples.ResolveSingleInstanceView();

GeneratedViewDispatchExamples.ResolveViewLeftOutOfRegistration();

GeneratedViewDispatchExamples.ResolveViewWithoutParameterlessConstructor();

GeneratedViewDispatchExamples.ResolveViewRegisteredForInterface();

GeneratedViewDispatchExamples.ResolveInterfaceViewBeforeClassView();

GeneratedViewDispatchExamples.RegisterHandWrittenDispatch();

ViewMappingExamples.MapView();

ViewMappingExamples.MapViewWithContract();

ViewMappingExamples.MapViewWithFactory();

ViewMappingExamples.MapViewWithFactoryAndContract();

ViewMappingExamples.UnmapView();

ViewMappingExamples.MapViewsWithFluentBuilder();

ViewMappingExamples.MapFactoryWithFluentBuilder();

ViewMappingExamples.ResolveViewByType();

ViewMappingExamples.MapViewFromServiceLocator();

ViewMappingExamples.MapContractedViewFromServiceLocator();

ViewMappingExamples.ResolveInOrder();

ViewMappingExamples.ConfigureViewLocatorInBuilder();

ViewMappingExamples.ConfigureViewLocatorFromAppBuilder();
