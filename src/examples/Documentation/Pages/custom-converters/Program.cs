// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Converters;

// The converters format and parse with the current culture; the outputs are for the invariant culture.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

ConverterRegistrationExamples.RegisterAndListConverters();

ConverterRegistrationExamples.DemonstrateAffinitySelection();

ConverterRegistrationExamples.OverrideBuiltInConverter();

CustomConvertersExamples.RegisterAndUseCustomConverter();

CustomConvertersExamples.DisplayPriorityColors();

CustomConvertersExamples.WriteConverterFromScratch();

CustomConvertersExamples.ConvertTagsBothWays();

CustomConvertersExamples.RegisterInConverterService();

CustomConvertersExamples.BindPriorityWithConverter();

CustomConvertersExamples.BindTagsBothWays();

FallbackConvertersExamples.RegisterFallbackConverter();

FallbackConvertersExamples.RegisterSetMethodConverter();

FallbackConvertersExamples.DemonstrateFallbackSelection();

FallbackConvertersExamples.ResolveTypedConverterBeforeFallback();

FallbackConvertersExamples.ResolveSetMethodConverter();

BindingConverterOverloadsExamples.BindOneWayWithConverter();

BindingConverterOverloadsExamples.BindOneWayWithConversionHint();

await BindingConverterOverloadsExamples.BindTwoWayWithConverters();

await BindingConverterOverloadsExamples.OneWayBindWithConverter();

BindingConverterOverloadsExamples.BindWithConverters();

BindingConverterOverloadsExamples.BindToWithConversionHintAndConverter();

await ViewListConverterExamples.ShowAccountsAsSummaryViews();

MigrationExamples.ExtractConvertersFromLegacyResolver();

MigrationExamples.ImportLegacyConvertersIntoService();

MigrationExamples.BindToWithMigratedConverterOverride();

MigrationExamples.BindToResolvesMigratedConverter();

CustomConvertersExamples.RegisterWithBuilder();

CustomConvertersExamples.ConvertAmountThroughBaseClass();

CustomConvertersExamples.ConvertAmountThroughInterfaces();

ConverterRegistrationExamples.CreateRegistriesWithoutService();

FallbackConvertersExamples.ScoreSetMethodConverter();

MigrationExamples.GroupConvertersAndDeconstruct();

MigrationExamples.CompareExtractedConverters();

TwoWayConverterPairExamples.DeconstructPriorityConverterPair();

TwoWayConverterPairExamples.ComparePriorityConverterPairs();
