// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Properties;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

ToPropertyExamples.BackPropertiesOnATypeWithItsOwnEvent();

ToPropertyExamples.BackPropertiesOnATypeWithAPublicRaiseMethod();

ToPropertyExamples.BackPropertiesWithDeferredSubscriptionAndAScheduler();

ObservableAsPropertyHelperExamples.ConstructAHelperWithChangeCallbacks();

ObservableAsPropertyHelperExamples.ReadThrownExceptions();

ObservableAsPropertyHelperExamples.CreateAHelperThatNeverChanges();

ObservableAsPropertyAttributeExamples.DeclareAPropertyWithTheAttribute();

ObservableAsPropertyAttributeExamples.StartFromAnInitialValue();
