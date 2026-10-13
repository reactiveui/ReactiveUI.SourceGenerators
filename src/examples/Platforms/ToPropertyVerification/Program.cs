// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ToPropertyVerification.Common;

const string NotificationCountMessage = "notification count";

const string ChangedLabel = "changed";

var failures = 0;

failures += Run("Selector + out overload, field-event raise mechanism", FieldEventScenario);

failures += Run("Name overload with initial value + selector overload with initial-value factory, public raise-method mechanism", RaiseMethodScenario);

failures += Run("Deferred subscription + explicit scheduler, protected raise-method mechanism", ProtectedBaseScenario);

failures += Run("Deferred+scheduler combo, and out combined with initial value / initial-value factory", CombinedOverloadsScenario);

failures += Run("ReactiveObject (IReactiveObject raise mechanism), generated + unsafe overloads", ReactiveObjectScenario);

failures += Run("[ObservableAsProperty] partial properties", AttributeScenario);

Console.WriteLine();

Console.WriteLine(failures == 0 ? "ALL SCENARIOS PASSED" : $"{failures} SCENARIO(S) FAILED");

return failures == 0 ? 0 : 1;

static int Run(string name, Action scenario)
{
    try
    {
        scenario();
        Console.WriteLine($"PASS: {name}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAIL: {name}");
        Console.WriteLine($"      {ex}");
        return 1;
    }
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException($"Assertion failed: {message}");
    }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Assertion failed: {message}. Expected '{expected}', got '{actual}'.");
    }
}

static List<string> TrackChanges(INotifyPropertyChanged target)
{
    List<string> names = [];
    target.PropertyChanged += (_, e) => names.Add(e.PropertyName ?? string.Empty);
    return names;
}

static void FieldEventScenario()
{
    const int InitialCount = 3;
    const string InitialLabel = "hello";
    const int UpdatedCount = 10;
    const string UpdatedLabel = "world";
    const int InitialDoubledCount = 6;
    const int UpdatedDoubledCount = 20;
    const int ExpectedNotificationCount = 2;

    SourceItem item = new() { Count = InitialCount, Label = InitialLabel };
    FieldEventViewModel vm = new(item);
    var changes = TrackChanges(vm);

    AssertEqual(InitialDoubledCount, vm.DoubledCount, "initial DoubledCount");
    AssertEqual(InitialLabel, vm.LabelText, "initial LabelText");

    item.Count = UpdatedCount;
    AssertEqual(UpdatedDoubledCount, vm.DoubledCount, "updated DoubledCount");

    item.Label = UpdatedLabel;
    AssertEqual(UpdatedLabel, vm.LabelText, "updated LabelText");

    AssertEqual(ExpectedNotificationCount, changes.Count, NotificationCountMessage);
    Assert(changes[0] == nameof(FieldEventViewModel.DoubledCount), "first notification names DoubledCount");
    Assert(changes[1] == nameof(FieldEventViewModel.LabelText), "second notification names LabelText");
}

static void RaiseMethodScenario()
{
    const int InitialCount = 0;
    const string InitialLabel = "start";
    const int UpdatedCount = 42;
    const string InitialCountText = "0";
    const string InitialUpperLabel = "START";
    const string UpdatedCountText = "42";
    const string UpdatedUpperLabel = "CHANGED";
    const int ExpectedNotificationCount = 2;

    SourceItem item = new() { Count = InitialCount, Label = InitialLabel };
    RaiseMethodViewModel vm = new(item);
    var changes = TrackChanges(vm);

    AssertEqual(InitialCountText, vm.CountText, "initial CountText");
    AssertEqual(InitialUpperLabel, vm.UpperLabel, "initial UpperLabel");

    item.Count = UpdatedCount;
    AssertEqual(UpdatedCountText, vm.CountText, "updated CountText");

    item.Label = ChangedLabel;
    AssertEqual(UpdatedUpperLabel, vm.UpperLabel, "updated UpperLabel");

    AssertEqual(ExpectedNotificationCount, changes.Count, NotificationCountMessage);
}

static void ProtectedBaseScenario()
{
    const int InitialCount = 0;
    const string InitialLabel = "one";
    const int UpdatedCount = 5;
    const string UpdatedLabel = "two";
    const int NoNotificationsYet = 0;

    SourceItem item = new() { Count = InitialCount, Label = InitialLabel };
    TestClock clock = new();
    ProtectedBaseViewModel vm = new(item, clock.Sequencer);
    var changes = TrackChanges(vm);

    // Deferred subscription: nothing has been observed yet, so no notification has fired for DeferredFlag.
    AssertEqual(NoNotificationsYet, changes.Count, "no notification before first read");

    item.Count = UpdatedCount;

    // First read subscribes; WhenChanged delivers the item's current value on subscribe, so it is not missed.
    AssertEqual(true, vm.DeferredFlag, "DeferredFlag reflects current value on first read");

    item.Label = UpdatedLabel;

    // The scheduler has not run yet, so ScheduledLabel still holds its default value.
    AssertEqual(default(string), vm.ScheduledLabel, "ScheduledLabel unchanged before the scheduler runs");

    clock.RunPending();

    AssertEqual(UpdatedLabel, vm.ScheduledLabel, "ScheduledLabel updated once the scheduler runs");
}

static void CombinedOverloadsScenario()
{
    const int InitialCount = 0;
    const string InitialLabel = "a";
    const string InitialSeed = "seed";
    const string FactorySeed = "factory-seed";
    const string UpdatedLabel = "b";
    const string UpdatedLabelWithSuffix = "b!";
    const int UpdatedCount = 7;
    const int NoUpdateYet = 0;

    SourceItem item = new() { Count = InitialCount, Label = InitialLabel };
    PendingSource<string> labels = new();
    TestClock clock = new();
    CombinedOverloadsViewModel vm = new(item, labels, clock.Sequencer);

    // The label source has produced nothing yet, so both properties hold their initial values.
    AssertEqual(InitialSeed, vm.OutWithInitial, "OutWithInitial starts at its plain initial value");
    AssertEqual(FactorySeed, vm.OutWithFactory, "OutWithFactory starts at its factory's initial value");

    labels.Push(UpdatedLabel);
    AssertEqual(UpdatedLabel, vm.OutWithInitial, "OutWithInitial follows the source");
    AssertEqual(UpdatedLabelWithSuffix, vm.OutWithFactory, "OutWithFactory follows the source");

    item.Count = UpdatedCount;

    // Deferred + scheduled: the read below subscribes for the first time and the scheduler has not run yet.
    AssertEqual(NoUpdateYet, vm.DeferredScheduled, "DeferredScheduled unchanged before the scheduler runs");

    clock.RunPending();

    AssertEqual(UpdatedCount, vm.DeferredScheduled, "DeferredScheduled updated once the scheduler runs");
}

static void ReactiveObjectScenario()
{
    const int InitialCount = 1;
    const string InitialLabel = "x";
    const int UpdatedCount = 2;
    const string UpdatedLabel = "y";
    const int ExpectedNotificationCount = 3;

    SourceItem item = new() { Count = InitialCount, Label = InitialLabel };
    ReactiveObjectViewModel vm = new(item);

    var changes = TrackChanges(vm);

    AssertEqual(InitialCount, vm.Count, "initial Count");
    AssertEqual(InitialCount, vm.UnsafeCount, "initial UnsafeCount");

    // WhenChanged delivers the item's current label as soon as the helper subscribes, replacing the initial value.
    AssertEqual(InitialLabel, vm.Label, "initial Label from the item's current value");

    item.Count = UpdatedCount;
    AssertEqual(UpdatedCount, vm.Count, "updated Count");
    AssertEqual(UpdatedCount, vm.UnsafeCount, "updated UnsafeCount");

    item.Label = UpdatedLabel;
    AssertEqual(UpdatedLabel, vm.Label, "updated Label");

    AssertEqual(ExpectedNotificationCount, changes.Count, NotificationCountMessage);
}

static void AttributeScenario()
{
    const int InitialCount = 9;
    const string InitialLabel = "attr";
    const int UpdatedCount = 11;
    const int ExpectedNotificationCount = 2;

    SourceItem item = new() { Count = InitialCount, Label = InitialLabel };
    AttributeViewModel vm = new(item);
    var changes = TrackChanges(vm);

    AssertEqual(InitialCount, vm.AttrCount, "initial AttrCount");
    AssertEqual(InitialLabel, vm.AttrLabel, "initial AttrLabel");

    item.Count = UpdatedCount;
    AssertEqual(UpdatedCount, vm.AttrCount, "updated AttrCount");

    item.Label = ChangedLabel;
    AssertEqual(ChangedLabel, vm.AttrLabel, "updated AttrLabel");

    AssertEqual(ExpectedNotificationCount, changes.Count, NotificationCountMessage);
}
