// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Bindings;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

OneWayBindingExamples.BuildApplication();

await OneWayBindingExamples.BindSelectedIssueTitleToLabel();

await OneWayBindingExamples.BindSelectedIssueAssigneeToLabel();

await OneWayBindingExamples.BindSelectedIssueStateToLabel();

await OneWayBindingExamples.BindRemainingCountToLabel();

await OneWayBindingExamples.BindBalanceToLabelWithCurrencyFormat();

await OneWayBindingExamples.OneWayBindFilterTextToBox();

await OneWayBindingExamples.OneWayBindIntoNamedControl();

await OneWayBindingExamples.OneWayBindKeepsViewWhilePathIsBroken();

await OneWayBindingExamples.OneWayBindUploadPercentToProgressBar();

await OneWayBindingExamples.OneWayBindAvailableBalanceWithSelector();

await OneWayBindingExamples.BindOneWayOnSequencer();

await OneWayBindingExamples.BindOneWayConvertedOnSequencer();

await OneWayBindingExamples.BindOneWayWithConverterObject();

await OneWayBindingExamples.BindOneWayWithConverterObjectAndHint();

await OneWayBindingExamples.BindOneWayWithConverterObjectOnSequencer();

OneWayBindingExamples.OneWayBindPriorityToBadgeColour();

OneWayBindingExamples.OneWayBindPriorityToBadgeColourWithHint();

OneWayBindingExamples.OneWayBindPriorityToBadgeColourOnSequencer();

await OneWayBindingExamples.OneWayBindWithSelectorOnSequencer();

await OneWayBindingExamples.BindOneWayUnsafeWithConverterObject();

await OneWayBindingExamples.OneWayBindUnsafeWithConverterObject();

await OneWayBindingExamples.BindOneWayWithNullScheduler();

await OneWayBindingExamples.BindOneWayConvertedWithNullScheduler();

await OneWayBindingExamples.OneWayBindWithSelectorAndNullScheduler();

await TwoWayBindingExamples.BindTodoFilterToTextBox();

TwoWayBindingExamples.BindTransferAmountWithConverters();

TwoWayBindingExamples.BindTokenOnSequencer();

TwoWayBindingExamples.BindReferenceBurstOnSequencer();

TwoWayBindingExamples.BindTransferAmountWithConvertersOnSequencer();

TwoWayBindingExamples.BindTransferAmountWithConverterObjects();

TwoWayBindingExamples.BindTransferAmountWithConverterObjectsAndHint();

TwoWayBindingExamples.BindTransferAmountWithConverterObjectsOnSequencer();

TwoWayBindingExamples.BindTransferAmountUnsafeWithConverterObjects();

await TwoWayBindingExamples.BindFilterToPickerSelection();

await TwoWayBindingExamples.BindTodoFilterInView();

TwoWayBindingExamples.BindTransferAmountInView();

TwoWayBindingExamples.BindScoreInViewOnSequencer();

TwoWayBindingExamples.BindScoreWithConverterObjects();

TwoWayBindingExamples.BindScoreWithConverterObjectsAndHint();

TwoWayBindingExamples.BindScoreWithConverterObjectsOnSequencer();

TwoWayBindingExamples.BindScoreUnsafeWithConverterObjects();

await TwoWayBindingExamples.BindTodoFilterWithNullScheduler();

TwoWayBindingExamples.BindTransferAmountWithConvertersAndNullScheduler();

TwoWayBindingExamples.BindScoreInViewWithNullScheduler();

await BindToExamples.BindUploadPercentToProgressBar();

await BindToExamples.BindRemainingCountToLabel();

await BindToExamples.BindTotalBalanceWithDecimalPlacesHint();

await BindToExamples.BindTotalBalanceWithFormatHint();

await BindToExamples.BindTotalBalanceWithConverterOverride();

await BindToExamples.BindTotalBalanceWithHintAndConverterOverride();

await BindToExamples.BindTypedTextEventsToFilter();

BindToExamples.BindBackgroundConnectionStateToLabel();

await BindCommandExamples.BindAddButton();

await BindCommandExamples.BindCompleteButtonToNamedEvent();

BindCommandExamples.BindExportButtonWithAndWithoutEvent();

await BindCommandExamples.BindSignInButton();

await BindCommandExamples.BindCloseIssueButton();

await BindCommandExamples.BindTransferButton();

await BindCommandExamples.BindEnrolButtonWithObservableParameter();

await BindCommandExamples.BindUploadButtonWithObservableParameterOnNamedEvent();

await BindCommandExamples.BindUploadButtonWithParameterExpressionOnNamedEvent();

await BindCommandExamples.BindUploadButtonWithParameterExpression();

CreatesCommandBindingExamples.AskBinderForAffinity();

CreatesCommandBindingExamples.BindExportButtonToDefaultEvent();

CreatesCommandBindingExamples.BindExportButtonToNamedEvent();

CreatesCommandBindingExamples.BindSearchToTextChangedEvent();

await InvokeCommandExamples.InvokeSignInWhenTokenChanges();

await InvokeCommandExamples.InvokeUploadWithPickedFile();

await InvokeCommandExamples.InvokeLoadIssuesWhenRepositoryChanges();

await InvokeCommandExamples.LoadIssuesWhenRepositoryIsSelected();

await InvokeCommandExamples.OpenCourseWhenTheUserPicksIt();

await InvokeCommandExamples.LoadNextPageWhenTheLastRowAppears();

await InvokeCommandExamples.RefreshTodoItemsOnAnInterval();

await InvokeCommandExamples.SkipTicksWhileTheCommandIsRunning();

await InvokeCommandExamples.SkipValuesWhileTheCommandIsDisabled();

await InvokeCommandExamples.LoadOnceAtStartUp();

await BindInteractionExamples.ConfirmCloseWithTaskHandler();

await BindInteractionExamples.FollowTheSelectedIssueBoard();

await BindInteractionExamples.ConfirmDropWithObservableHandler();

await BindInteractionExamples.ConfirmCloseThroughInterfaceProperty();

await BindInteractionExamples.ApproveLargeTransferInTwoSteps();

await BindInteractionExamples.CancelTransferAtApprovalStep();

await BindInteractionExamples.RegisterHandlersInReverseOrder();

await BindInteractionExamples.InspectInteractionContext();

await BindInteractionExamples.AnswerThroughInteractionContext();

await BindInteractionExamples.CloseIssueWithoutHandler();

await BindInteractionExamples.AuditApprovalWithDerivedInteraction();

await BindInteractionExamples.DisposeBindingRemovesHandler();

BindInteractionExamples.BindInteractionWithoutViewModel();

BindInteractionExamples.ConstructUnhandledInteractionException();

await BindInteractionExamples.HandleThroughInterface();

await BindInteractionExamples.AskAsAnObservable();

await BindInteractionExamples.AskThroughInterfaceAsAnObservable();

await BindInteractionExamples.AnswerOnTheHandlerScheduler();

await ListSelectionBindingExamples.BindBusyIndicator();

await ListSelectionBindingExamples.BindFilterTextBoxesTwoWay();

await ListSelectionBindingExamples.BindFilteredItemsToList();

await ListSelectionBindingExamples.BindSelectedItemTwoWay();

await ListSelectionBindingExamples.BindCommandFollowsSelection();

await ReactiveBindingExamples.ReadOneWayBinding();

await ReactiveBindingExamples.ReadTwoWayBinding();

await ReactiveBindingExamples.ReadBindingExpressions();

await ReactiveBindingExamples.CreateReactiveBinding();

ReactiveBindingExamples.CommitReferenceWhenBoxIsLeft();

ReactiveBindingExamples.CommitAmountWhenBoxIsLeft();

ReactiveBindingExamples.RefreshScoreOnSignal();

await ReactiveBindingExamples.RefreshFilterOnSignal();

await ReactiveBindingExamples.ObserveBothWithoutStream();

ReactiveBindingExamples.ObserveBothWithConvertersAndWithoutStream();

BindingErrorExamples.RegisterConsoleLogger();

BindingErrorExamples.FaultWithoutInnerExceptionIsLogged();

BindingErrorExamples.FaultWithInnerExceptionIsRethrown();

BindingErrorExamples.SetterThatThrowsReachesTheCaller();

BindingErrorExamples.SubscribeAppliesTheFaultContract();

await BindingHookExamples.BindWithoutHook();

await BindingHookExamples.LogEveryBinding();

await BindingHookExamples.VetoTwoWayBinding();

await BindingHookExamples.VetoViewFirstBinding();

await BindingHookExamples.AskHooksDirectly();

await BindingHookExamples.RefreshAfterLateRegistration();

PropertyBindingHookExamples.AskHookDirectly();
