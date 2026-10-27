// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;
using LinqExpression = System.Linq.Expressions.Expression;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>Tests for the <see cref="INPCObservableForProperty"/> class.</summary>
public class INPCObservableForPropertyTests
{
    /// <summary>The affinity score returned for INotifyPropertyChanged-compatible types.</summary>
    private const int InpcAffinity = 5;

    /// <summary>A sample name value assigned during notification tests.</summary>
    private const string SampleName = "Alice";

    /// <summary>A sample indexer value used during notification tests.</summary>
    private const string SampleValue = "value";

    /// <summary>The name of the non-indexer property used in PropertyChanging tests.</summary>
    private const string SomePropName = "SomeProp";

    /// <summary>A sample age value assigned to an unrelated property.</summary>
    private const int SampleAge = 30;

    /// <summary>
    /// The expected number of emissions when distinct filtering is disabled and the initial value is not
    /// skipped: the initial value plus the three subsequent property changes.
    /// </summary>
    private const int ExpectedNonDistinctCount = 4;

    /// <summary>Verifies affinity is 5 for types implementing INotifyPropertyChanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCType_Returns5()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name");

        await Assert.That(affinity).IsEqualTo(InpcAffinity);
    }

    /// <summary>Verifies affinity is 5 for INotifyPropertyChanging when beforeChanged is true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCChangingType_Returns5ForBeforeChanged()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name", true);

        await Assert.That(affinity).IsEqualTo(InpcAffinity);
    }

    /// <summary>Verifies affinity is 0 for POCO types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_PocoType_Returns0()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(PocoModel), "Value");

        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>Verifies that GetNotificationForProperty emits when a property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_PropertyChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        vm.Name = SampleName;

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>Verifies that GetNotificationForProperty emits before-change notifications.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_BeforeChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name", true)
            .Subscribe(_ => emitted = true);

        vm.Name = SampleName;

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>Verifies that GetNotificationForProperty filters by property name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_DifferentProperty_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        // Change Age, not Name
        vm.Age = SampleAge;

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>Verifies that GetNotificationForProperty returns Observable.Never for POCO types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_PocoType_ReturnsNever()
    {
        var sut = new INPCObservableForProperty();
        var model = new PocoModel { Value = "test" };

        Expression<Func<PocoModel, string>> expr = x => x.Value;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(model, body, "Value")
            .Subscribe(_ => emitted = true);

        model.Value = "changed";

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>Verifies that null or empty PropertyName in PropertyChanged emits for all listeners.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new NullPropertyNameViewModel();

        Expression<Func<NullPropertyNameViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        vm.RaiseAllPropertiesChanged();

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>Verifies affinity is 0 for before-change on types without INotifyPropertyChanging.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_NonChangingType_Returns0ForBeforeChanged()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(NonChangingViewModel), "Name", true);

        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>Verifies that GetNotificationForProperty emits for indexer property changes (PropertyChanged path).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_PropertyChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        // Create an IndexExpression for vm["key"] — NodeType == ExpressionType.Index
        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item")
            .Subscribe(_ => emitted = true);

        vm["key"] = SampleValue;

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>Verifies that GetNotificationForProperty emits for indexer property changes (PropertyChanging/beforeChanged path).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_BeforeChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item", true)
            .Subscribe(_ => emitted = true);

        vm["key"] = SampleValue;

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>Verifies that indexer property notification does not emit for unrelated property name changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_DifferentPropertyName_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "SomeOtherProperty")
            .Subscribe(_ => emitted = true);

        vm["key"] = SampleValue;

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>Verifies that indexer property notification emits when PropertyChanged fires with null/empty name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item")
            .Subscribe(_ => emitted = true);

        vm.RaiseAllPropertiesChanged();

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name handles an invalid property name gracefully
    /// by catching the Expression.Property exception and using the parameter expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_InvalidProperty_StillCreatesObservable()
    {
        var vm = new TestViewModel();

        var emitted = false;
        using var sub = vm.ObservableForProperty<TestViewModel, string>("NonExistentProperty")
            .Subscribe(_ => emitted = true);

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>Verifies that ObservableForProperty by name with skipInitial=false emits the initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_SkipInitialFalse_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = "Initial" };

        string? receivedValue = null;
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", skipInitial: false)
            .Subscribe(x => receivedValue = x.Value);

        await Assert.That(receivedValue).IsEqualTo("Initial");
    }

    /// <summary>Verifies that ObservableForProperty by name with isDistinct=false allows duplicate values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_IsDistinctFalse_AllowsDuplicates()
    {
        var vm = new TestViewModel();

        var values = new List<string>();
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", beforeChange: false, skipInitial: false, isDistinct: false)
            .Subscribe(x => values.Add(x.Value));

        vm.Name = SampleName;
        vm.Name = string.Empty;
        vm.Name = SampleName;

        await Assert.That(values.Count).IsEqualTo(ExpectedNonDistinctCount);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name handles a nullable property returning null
    /// by returning default in GetCurrentValue.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_NullPropertyValue_ReturnsDefault()
    {
        var vm = new NullablePropertyViewModel();

        string? receivedValue = null;
        var received = false;
        using var sub = vm.ObservableForProperty<NullablePropertyViewModel, string?>("NullableName", skipInitial: false)
            .Subscribe(x =>
            {
                receivedValue = x.Value;
                received = true;
            });

        await Assert.That(received).IsTrue();
        await Assert.That(receivedValue).IsNull();
    }

    /// <summary>
    /// Verifies that indexer property notification emits when PropertyChanging fires with null/empty name.
    /// Covers INPCObservableForProperty line 44 (string.IsNullOrEmpty TRUE branch for Index + BeforeChanged).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_BeforeChanged_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item", true)
            .Subscribe(_ => emitted = true);

        vm.RaiseAllPropertyChanging();

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that indexer property notification does not emit for an unrelated property name
    /// in the BeforeChanged (PropertyChanging) path.
    /// Covers INPCObservableForProperty line 44 (both conditions FALSE in the Where filter).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_BeforeChanged_DifferentName_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "SomeOtherProperty", true)
            .Subscribe(_ => emitted = true);

        vm["key"] = SampleValue;

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>
    /// Verifies that non-index property notification emits when PropertyChanging fires with null property name.
    /// Covers INPCObservableForProperty line 49 (string.IsNullOrEmpty TRUE branch for non-Index + BeforeChanged).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NonIndex_BeforeChanged_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        Expression<Func<IndexableViewModel, string>> expr = x => x.SomeProp;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, SomePropName, true)
            .Subscribe(_ => emitted = true);

        vm.RaiseAllPropertyChanging();

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that non-index property notification does not emit for different property in the
    /// PropertyChanging path.
    /// Covers INPCObservableForProperty line 49 (both conditions FALSE in Where filter).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NonIndex_BeforeChanged_DifferentProperty_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        Expression<Func<IndexableViewModel, string>> expr = x => x.SomeProp;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, SomePropName, true)
            .Subscribe(_ => emitted = true);

        vm["key"] = SampleValue; // raises PropertyChanging for "Item[]", not "SomeProp"

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>
    /// Verifies that indexer property notification emits when PropertyChanged fires with null/empty name.
    /// Covers INPCObservableForProperty line 66 (string.IsNullOrEmpty TRUE branch for Index + PropertyChanged).
    /// Already exists but this ensures explicit coverage of the null name path within Index branch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task
        GetNotificationForProperty_IndexExpression_PropertyChanged_NullPropertyName_EmitsNotification_Variant()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            [LinqExpression.Constant("key")]);

        var count = 0;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item")
            .Subscribe(_ => count++);

        // Fire with empty string property name - should trigger
        vm.RaisePropertyChangedWithName(string.Empty);

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that non-index property notification emits when PropertyChanged fires with null property name.
    /// Covers INPCObservableForProperty line 71 (string.IsNullOrEmpty TRUE branch for non-Index + PropertyChanged).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NonIndex_PropertyChanged_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        Expression<Func<IndexableViewModel, string>> expr = x => x.SomeProp;
        var body = Reflection.Rewrite(expr.Body);

        var count = 0;
        using var sub = sut.GetNotificationForProperty(vm, body, SomePropName)
            .Subscribe(_ => count++);

        vm.RaiseAllPropertiesChanged();

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that PropertyChanged with empty string name matches all property listeners.
    /// The INPC spec says both null and empty string mean "all properties changed".
    /// Covers INPCObservableForProperty line 71 (string.IsNullOrEmpty TRUE branch for empty string on non-Index + PropertyChanged).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyChanged_EmptyPropertyName_MatchesAllProperties()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        Expression<Func<IndexableViewModel, string>> expr = x => x.SomeProp;
        var body = Reflection.Rewrite(expr.Body);

        var count = 0;
        using var sub = sut.GetNotificationForProperty(vm, body, SomePropName)
            .Subscribe(_ => count++);

        // Fire with empty string property name - should trigger (all properties changed)
        vm.RaisePropertyChangedWithName(string.Empty);

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that PropertyChanging with empty string name matches all property listeners.
    /// The INPC spec says both null and empty string mean "all properties changing".
    /// Covers INPCObservableForProperty line 49 (string.IsNullOrEmpty TRUE branch for empty string on non-Index + BeforeChanged).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyChanging_EmptyPropertyName_MatchesAllProperties()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        Expression<Func<IndexableViewModel, string>> expr = x => x.SomeProp;
        var body = Reflection.Rewrite(expr.Body);

        var count = 0;
        using var sub = sut.GetNotificationForProperty(vm, body, SomePropName, true)
            .Subscribe(_ => count++);

        // Fire with empty string property name - should trigger (all properties changing)
        vm.RaisePropertyChangingWithName(string.Empty);

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>A test model with an indexer that implements both INotifyPropertyChanged and INotifyPropertyChanging.</summary>
    public sealed class IndexableViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// A private dictionary field used to store key-value pairs for the indexer implementation
        /// of the IndexableViewModel class. This field is referenced when getting or setting
        /// values through the indexer.
        /// </summary>
        private readonly Dictionary<string, string> _items = [];

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>Gets or sets a non-indexer property for testing non-Index expression paths.</summary>
        public string SomeProp { get; set; } = string.Empty;

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public string this[string key]
        {
            get => _items.TryGetValue(key, out var val) ? val : string.Empty;
            set
            {
                PropertyChanging?.Invoke(this, new("Item[]"));
                _items[key] = value;
                PropertyChanged?.Invoke(this, new("Item[]"));
            }
        }

        /// <summary>Raises PropertyChanged with null property name (all properties changed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaiseAllPropertiesChanged() =>
            PropertyChanged?.Invoke(this, new(null));

        /// <summary>Raises PropertyChanged with a specific property name.</summary>
        /// <param name="propertyName">The property name to raise.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChangedWithName(string? propertyName) =>
            PropertyChanged?.Invoke(this, new(propertyName));

        /// <summary>Raises PropertyChanging with null property name (all properties changing).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaiseAllPropertyChanging() =>
            PropertyChanging?.Invoke(this, new(null));

        /// <summary>Raises PropertyChanging with a specific property name.</summary>
        /// <param name="propertyName">The property name to raise.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChangingWithName(string? propertyName) =>
            PropertyChanging?.Invoke(this, new(propertyName));
    }

    /// <summary>A test model with a nullable property for testing null GetCurrentValue path.</summary>
    private sealed class NullablePropertyViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the nullable name.</summary>
        [SuppressMessage("Design", "SST1440:Unused private member", Justification = "Resolved by name through the property observation path under test.")]
        private string? NullableName
        {
            get => field;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(NullableName)));
            }
        }
    }

    /// <summary>A test model that implements INotifyPropertyChanged but NOT INotifyPropertyChanging.</summary>
    private sealed class NonChangingViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
#pragma warning disable CS0067 // Event is never used
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>A test model that raises PropertyChanged with null PropertyName.</summary>
    private sealed class NullPropertyNameViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Raises PropertyChanged with null property name (all properties changed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaiseAllPropertiesChanged() =>
            PropertyChanged?.Invoke(this, new(null));
    }
}
