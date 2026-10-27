// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Converters;

// The converters format and parse with the current culture; the outputs are for the invariant culture.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

BooleanGuidUriExamples.ParseFeatureToggleState();

BooleanGuidUriExamples.FormatFeatureToggleState();

BooleanGuidUriExamples.FormatOptionalFeatureToggleState();

BooleanGuidUriExamples.ParseSessionCorrelationId();

BooleanGuidUriExamples.FormatSessionCorrelationId();

BooleanGuidUriExamples.ParseRepositoryUrl();

BooleanGuidUriExamples.FormatRepositoryUrl();

NumberExamples.ParseGitHubIssueNumber();

NumberExamples.FormatGitHubIssueNumber();

NumberExamples.ParseCloudStorageFileSize();

NumberExamples.FormatCloudStorageFileSize();

NumberExamples.ParseNetworkPortNumber();

NumberExamples.FormatNetworkPortNumber();

NumberExamples.ParseScientificConstant();

NumberExamples.FormatStudentScore();

CultureExamples.FormatSingleWithCurrentCulture();

NumberExamples.ParseEulersNumber();

NumberExamples.FormatEulersNumber();

NumberExamples.ParseBankTransferAmount();

NumberExamples.FormatBankTransferAmount();

NumberExamples.ParseRGBColorChannel();

NumberExamples.FormatRGBColorChannel();

NullableExamples.WrapGitHubIssueNumber();

NullableExamples.UnwrapGitHubIssueNumber();

NullableExamples.WrapCloudStorageFileSize();

NullableExamples.UnwrapCloudStorageFileSize();

NullableExamples.WrapNetworkPortNumber();

NullableExamples.UnwrapNetworkPortNumber();

NullableExamples.WrapScientificConstant();

NullableExamples.UnwrapScientificConstant();

NullableExamples.WrapEulersNumber();

NullableExamples.UnwrapEulersNumber();

NullableExamples.WrapBankTransferAmount();

NullableExamples.UnwrapBankTransferAmount();

NullableExamples.WrapRGBColorChannel();

NullableExamples.UnwrapRGBColorChannel();

NullableExamples.ParseOptionalGitHubIssueNumber();

NullableExamples.ParseOptionalBankTransferAmount();

NullableExamples.FormatOptionalGitHubIssueNumber();

NullableExamples.FormatOptionalBankTransferAmount();

NullableExamples.ParseOptionalBankBalance();

NullableExamples.WrapStudentScore();

NullableExamples.ParseOptionalStudentScore();

NullableExamples.ParseOptionalRGBColorChannel();

NullableExamples.FormatOptionalRGBColorChannel();

NullableExamples.ParseOptionalNetworkPortNumber();

NullableExamples.FormatOptionalNetworkPortNumber();

NullableExamples.ParseOptionalCloudStorageFileSize();

NullableExamples.FormatOptionalCloudStorageFileSize();

NullableExamples.ParseOptionalScientificConstant();

NullableExamples.FormatOptionalScientificConstant();

NullableExamples.ParseOptionalEulersNumber();

NullableExamples.FormatOptionalEulersNumber();

NullableExamples.ParseOptionalFeatureToggleBoolean();

NullableExamples.FormatOptionalFeatureToggleBoolean();

NullableExamples.ParseOptionalTodoDueDate();

NullableExamples.FormatOptionalTodoDueDate();

NullableExamples.ParseOptionalCloudStorageModificationTime();

NullableExamples.FormatOptionalCloudStorageModificationTime();

NullableExamples.ParseOptionalDateTime();

NullableExamples.FormatOptionalDateTime();

NullableExamples.ParseOptionalAppointmentTime();

NullableExamples.FormatOptionalAppointmentTime();

NullableExamples.ParseOptionalProjectDuration();

NullableExamples.FormatOptionalProjectDuration();

NullableExamples.ParseOptionalSessionCorrelationId();

NullableExamples.FormatOptionalSessionCorrelationId();

DateTimeExamples.ParseTodoDueDateTime();

DateTimeExamples.FormatTodoDueDateTime();

DateTimeExamples.ParseCloudStorageModificationTime();

DateTimeExamples.FormatCloudStorageModificationTime();

DateTimeExamples.ParseTodoDueDate();

DateTimeExamples.FormatTodoDueDate();

DateTimeExamples.ParseAppointmentStartTime();

DateTimeExamples.FormatAppointmentStartTime();

DateTimeExamples.ParseProjectDuration();

DateTimeExamples.FormatProjectDuration();

StringAndEqualityExamples.DemonstrateStringConverter();

StringAndEqualityExamples.DemonstrateEqualityConverter();

StringAndEqualityExamples.DemonstrateBooleanConverters();

ConverterRegistriesExamples.AccessConverterService();

ConverterRegistriesExamples.ResolveBuiltInConverters();

NumberExamples.ParseEveryNumberType();

NumberExamples.FormatEveryNumberType();

NullableExamples.ParseEveryOptionalNumberType();

NullableExamples.FormatEveryOptionalNumberType();

NullableWrapperExamples.WrapWholeNumbers();

NullableWrapperExamples.WrapFractionalNumbers();

NullableWrapperExamples.UnwrapWholeNumbers();

NullableWrapperExamples.UnwrapFractionalNumbers();

BooleanGuidUriExamples.ConvertFeatureToggleBothWays();

BooleanGuidUriExamples.ConvertSessionCorrelationIdBothWays();

BooleanGuidUriExamples.ConvertRepositoryUrlBothWays();

DateTimeExamples.ConvertTodoDueDateTimeBothWays();

DateTimeExamples.ConvertCloudStorageModificationTimeBothWays();

DateTimeExamples.ConvertTodoDueDateBothWays();

DateTimeExamples.ConvertAppointmentStartTimeBothWays();

DateTimeExamples.ConvertProjectDurationBothWays();

StringAndEqualityExamples.ShowStringConverterAffinity();

StringAndEqualityExamples.ShowEqualityConverterTypePair();

MauiVisibilityExamples.ConvertIsDoneToVisibility();

MauiVisibilityExamples.ConvertVisibilityToIsDone();
