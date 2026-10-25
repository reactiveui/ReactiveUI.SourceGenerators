// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>Tests for the <see cref="Reflection"/> class.</summary>
public class ReflectionTests
{
    /// <summary>The name of the public field the reflection tests resolve.</summary>
    private const string PublicFieldName = "PublicField";

    /// <summary>The first argument of the two-dimensional indexer under test.</summary>
    private const int FirstIndexArgument = 3;

    /// <summary>A sample value used by these tests.</summary>
    private const int SampleValue = 42;

    /// <summary>The second argument of the two-dimensional indexer under test.</summary>
    private const int SecondIndexArgument = 5;

    /// <summary>A sample name value used across reflection tests.</summary>
    private const string SampleName = "Alice";

    /// <summary>The name of a method member used to exercise non-property reflection paths.</summary>
    private const string HashCodeMethodName = "GetHashCode";

    /// <summary>A replacement value assigned through a reflection-based setter.</summary>
    private const string NewFieldValue = "NewValue";

    /// <summary>The expected number of values returned for a two-element property chain.</summary>
    private const int ExpectedTwoValues = 2;

    /// <summary>The index of the third value in a property chain result.</summary>
    private const int ThirdValueIndex = 2;

    /// <summary>Verifies that ExpressionToPropertyNames returns the correct name for a simple property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_SimpleProperty_ReturnsName()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Name");
    }

    /// <summary>Verifies that ExpressionToPropertyNames returns a dotted path for nested properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_NestedProperty_ReturnsDottedPath()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Address.City");
    }

    /// <summary>Verifies that GetValueFetcherOrThrow returns a working getter for a property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherOrThrow_PropertyMember_ReturnsGetter()
    {
        var memberInfo = typeof(TestViewModel).GetProperty("Name")!;
        var getter = Reflection.GetValueFetcherOrThrow(memberInfo);

        var vm = new TestViewModel { Name = "Test" };
        var value = getter(vm, null);

        await Assert.That(value).IsEqualTo("Test");
    }

    /// <summary>Verifies that GetValueSetterOrThrow returns a working setter for a property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterOrThrow_PropertyMember_ReturnsSetter()
    {
        var memberInfo = typeof(TestViewModel).GetProperty("Name")!;
        var setter = Reflection.GetValueSetterOrThrow(memberInfo);

        var vm = new TestViewModel();
        setter(vm, "Hello", null);

        await Assert.That(vm.Name).IsEqualTo("Hello");
    }

    /// <summary>Verifies that TryGetValueForPropertyChain returns the correct value for a simple chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_SimpleProperty_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Name = SampleName };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo(SampleName);
    }

    /// <summary>Verifies that TryGetValueForPropertyChain traverses nested properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NestedProperty_TraversesChain()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Address = new() { City = "Seattle" } };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("Seattle");
    }

    /// <summary>Verifies that TryGetValueForPropertyChain returns false when a null is in the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullInChain_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TryGetValueForPropertyChain<string>(out _, vm, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that TrySetValueToPropertyChain sets a value on a simple property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_SimpleProperty_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel();
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "Bob");

        await Assert.That(result).IsTrue();
        await Assert.That(vm.Name).IsEqualTo("Bob");
    }

    /// <summary>Verifies that TryGetAllValuesForPropertyChain returns intermediate ObservedChange objects for a simple property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_SimpleProperty_ReturnsObservedChanges()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Name = SampleName };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(changeValues.Length).IsEqualTo(1);
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[0].Sender).IsEqualTo(vm);
        await Assert.That(changeValues[0].Value).IsEqualTo(SampleName);
    }

    /// <summary>Verifies that TryGetAllValuesForPropertyChain returns multiple ObservedChange objects for a deep chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_DeepChain_ReturnsMultipleValues()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var address = new TestAddress { City = "Portland" };
        var vm = new TestViewModel { Address = address };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(changeValues.Length).IsEqualTo(ExpectedTwoValues);
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[0].Sender).IsEqualTo(vm);
        await Assert.That(changeValues[0].Value).IsEqualTo(address);
        await Assert.That(changeValues[1]).IsNotNull();
        await Assert.That(changeValues[1].Sender).IsEqualTo(address);
        await Assert.That(changeValues[1].Value).IsEqualTo("Portland");
    }

    /// <summary>Verifies that TrySetValueToPropertyChain sets a value through a deep property chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_DeepChain_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var address = new TestAddress { City = "Old" };
        var vm = new TestViewModel { Address = address };
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New");

        await Assert.That(result).IsTrue();
        await Assert.That(address.City).IsEqualTo("New");
    }

    /// <summary>Verifies that TrySetValueToPropertyChain returns false when a null intermediate is in the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_NullIntermediate_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", false);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that Rewrite strips a boxing conversion and returns the underlying member access.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_ConvertExpression_StripsConvertAndReturnsMemberAccess()
    {
        // x => (object)x.Age creates a Convert expression wrapping MemberAccess
        Expression<Func<TestViewModel, object>> expr = x => x.Age;
        await Assert.That(expr.Body.NodeType).IsEqualTo(ExpressionType.Convert);

        var rewritten = Reflection.Rewrite(expr.Body);

        // After rewriting, the Convert should be stripped, leaving MemberAccess
        await Assert.That(rewritten.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        await Assert.That(((MemberExpression)rewritten).Member.Name).IsEqualTo("Age");
    }

    /// <summary>Verifies that ExpressionToPropertyNames joins every hop of a four-level chain into one dotted path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_DeeperChain_ReturnsDottedPath()
    {
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Chain2.Chain3.Host.SomeOtherParam");
    }

    /// <summary>Verifies that GetValueFetcherForProperty returns null for a member that is neither a field nor a property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_MethodMember_ReturnsNull()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;
        var fetcher = Reflection.GetValueFetcherForProperty(methodInfo);

        await Assert.That(fetcher).IsNull();
    }

    /// <summary>Verifies that GetValueFetcherForProperty returns a working getter for a field member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_FieldMember_ReturnsGetter()
    {
        var fieldInfo = typeof(FieldTestModel).GetField(PublicFieldName)!;
        var getter = Reflection.GetValueFetcherForProperty(fieldInfo);

        await Assert.That(getter).IsNotNull();

        var model = new FieldTestModel { PublicField = "FieldValue" };
        var value = getter!(model, null);

        await Assert.That(value).IsEqualTo("FieldValue");
    }

    /// <summary>Verifies that GetValueSetterForProperty returns a working setter for a field member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_FieldMember_ReturnsSetter()
    {
        var fieldInfo = typeof(FieldTestModel).GetField(PublicFieldName)!;
        var setter = Reflection.GetValueSetterForProperty(fieldInfo);

        await Assert.That(setter).IsNotNull();

        var model = new FieldTestModel();
        setter!(model, NewFieldValue, null);

        await Assert.That(model.PublicField).IsEqualTo(NewFieldValue);
    }

    /// <summary>Verifies that GetValueSetterForProperty returns null for a method member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_MethodMember_ReturnsNull()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;
        var setter = Reflection.GetValueSetterForProperty(methodInfo);

        await Assert.That(setter).IsNull();
    }

    /// <summary>Verifies that GetValueFetcherOrThrow throws for a method member.</summary>
    [Test]
    public void GetValueFetcherOrThrow_MethodMember_ThrowsArgumentException()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;

        _ = Assert.Throws<ArgumentException>(() => Reflection.GetValueFetcherOrThrow(methodInfo));
    }

    /// <summary>Verifies that GetValueSetterOrThrow throws for a method member.</summary>
    [Test]
    public void GetValueSetterOrThrow_MethodMember_ThrowsArgumentException()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;

        _ = Assert.Throws<ArgumentException>(() => Reflection.GetValueSetterOrThrow(methodInfo));
    }

    /// <summary>Verifies that TryGetAllValuesForPropertyChain returns false when null is in the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_NullInChain_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TryGetAllValuesForPropertyChain(out _, vm, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that ExpressionToPropertyNames handles index expression paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_IndexExpression_ReturnsPathWithIndex()
    {
        Expression<Func<IndexedModel, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).Contains("Items");
        await Assert.That(name).Contains("[0]");
    }

    /// <summary>Verifies that GetValueSetterForProperty returns a working setter for a property member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_PropertyMember_ReturnsSetter()
    {
        var propertyInfo = typeof(TestViewModel).GetProperty("Name")!;
        var setter = Reflection.GetValueSetterForProperty(propertyInfo);

        await Assert.That(setter).IsNotNull();

        var vm = new TestViewModel();
        setter!(vm, "Test", null);

        await Assert.That(vm.Name).IsEqualTo("Test");
    }

    /// <summary>Verifies that MaterializeExpressions returns the same array when given an array.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_ArrayInput_ReturnsSameArray()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();
        var array = chain as System.Linq.Expressions.Expression[] ?? [.. chain];

        var result = Reflection.MaterializeExpressions(array);

        await Assert.That(ReferenceEquals(result, array)).IsTrue();
    }

    /// <summary>Verifies that MaterializeExpressions returns empty array for an empty ICollection.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_EmptyCollection_ReturnsEmptyArray()
    {
        var emptyList = new List<System.Linq.Expressions.Expression>();

        var result = Reflection.MaterializeExpressions(emptyList);

        await Assert.That(result.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that MaterializeExpressions copies from a non-empty ICollection.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_NonEmptyCollection_CopiesElements()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();
        var list = new List<System.Linq.Expressions.Expression>(chain);

        var result = Reflection.MaterializeExpressions(list);

        await Assert.That(result.Length).IsEqualTo(list.Count);
    }

    /// <summary>Verifies that MaterializeExpressions handles a general IEnumerable (not array or ICollection).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_GeneralEnumerable_MaterializesToArray()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Wrap in a pure IEnumerable using a generator method
        static IEnumerable<System.Linq.Expressions.Expression> AsEnumerable(
            IEnumerable<System.Linq.Expressions.Expression> src)
        {
            foreach (var item in src)
            {
                yield return item;
            }
        }

        var result = Reflection.MaterializeExpressions(AsEnumerable(chain));

        await Assert.That(result.Length).IsGreaterThan(0);
    }

    /// <summary>Verifies that TrySetValueToPropertyChain with shouldThrow=true returns false when the final property's parent is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowTrue_NullFinalParent_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // A two-hop chain reaches the null parent after the traversal loop, so this returns rather than throws.
        var vm = new TestViewModel();
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", true);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that GetValueFetcherForProperty for a field throws when the field value is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_FieldMember_NullValue_Throws()
    {
        var fieldInfo = typeof(FieldTestModel).GetField(PublicFieldName)!;
        var getter = Reflection.GetValueFetcherForProperty(fieldInfo);

        await Assert.That(getter).IsNotNull();

        var model = new FieldTestModel { PublicField = null! };
        var act = () => getter!(model, null);

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that TryGetValueForPropertyChain throws for empty expression chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = static () => Reflection.TryGetValueForPropertyChain<string>(out _, new TestViewModel(), []);

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that TryGetAllValuesForPropertyChain throws for empty expression chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = static () => Reflection.TryGetAllValuesForPropertyChain(out _, new TestViewModel(), []);

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that TrySetValueToPropertyChain throws for empty expression chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = static () => Reflection.TrySetValueToPropertyChain(new TestViewModel(), [], "value");

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that TryGetValueForPropertyChain returns false when the root object is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullRoot_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var result = Reflection.TryGetValueForPropertyChain<string>(out _, null, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that TryGetAllValuesForPropertyChain returns false when the last object in chain is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_NullAtLastStep_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var result = Reflection.TryGetAllValuesForPropertyChain(out _, null, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that TrySetValueToPropertyChain returns false when target is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_NullTarget_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var result = Reflection.TrySetValueToPropertyChain(null, chain, "value");

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that TryGetValueForPropertyChain returns false when an intermediate of a four-level chain is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullIntermediate_In4LevelChain_ReturnsFalse()
    {
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Chain3 is null, so the walk meets a null inside the loop rather than at the last hop.
        var obj = new ObjChain1 { Chain2 = new() };
        var result = Reflection.TryGetValueForPropertyChain<int>(out _, obj, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that TryGetAllValuesForPropertyChain returns false and stops recording when an intermediate is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_NullIntermediate_In4LevelChain_ReturnsFalse()
    {
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Chain3 is null, so the walk meets a null inside the loop rather than at the last hop.
        var obj = new ObjChain1 { Chain2 = new() };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, obj, chain);

        await Assert.That(result).IsFalse();
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[1]).IsNotNull();
        await Assert.That(changeValues[ThirdValueIndex]).IsNull();
    }

    /// <summary>Verifies that TrySetValueToPropertyChain with shouldThrow=true throws when an intermediate target is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowTrue_NullIntermediate_ThrowsArgumentNullException()
    {
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Chain3 is null, so the walk meets a null inside the loop rather than at the last hop.
        var obj = new ObjChain1 { Chain2 = new() };
        var action = () => Reflection.TrySetValueToPropertyChain(obj, chain, SampleValue);

        await Assert.That(action).ThrowsException();
    }

    /// <summary>Verifies that TrySetValueToPropertyChain with shouldThrow=false sets the final property of a populated chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowFalse_PopulatedChain_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var address = new TestAddress { City = "Old" };
        var vm = new TestViewModel { Address = address };
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", false);

        await Assert.That(result).IsTrue();
        await Assert.That(address.City).IsEqualTo("New");
    }

    /// <summary>Verifies that ExpressionToPropertyNames formats an indexer on a member as its name and argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_MultiArgIndexer_ReturnsPathWithCommaSeparatedArgs()
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(MultiArgIndexedModel), "x");
        var itemsProperty = System.Linq.Expressions.Expression.Property(parameter, "Items");
        var indexer = typeof(Dictionary<string, int>).GetProperty("Item")!;
        var arg = System.Linq.Expressions.Expression.Constant("key1");
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(itemsProperty, indexer, [arg]);

        var rewritten = Reflection.Rewrite(indexExpr);
        var name = Reflection.ExpressionToPropertyNames(rewritten);

        await Assert.That(name).Contains("Item[key1]");
    }

    /// <summary>Verifies that ExpressionToPropertyNames separates a multi-parameter indexer's arguments with commas.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_TrueMultiArgIndexer_ReturnsCommaSeparatedArgs()
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TrueMultiArgIndexedModel), "x");
        var indexer = typeof(TrueMultiArgIndexedModel).GetProperty("Item")!;
        var arg0 = System.Linq.Expressions.Expression.Constant(FirstIndexArgument);
        var arg1 = System.Linq.Expressions.Expression.Constant(SecondIndexArgument);
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(
            parameter,
            indexer,
            [arg0, arg1]);

        var rewritten = Reflection.Rewrite(indexExpr);
        var name = Reflection.ExpressionToPropertyNames(rewritten);

        await Assert.That(name).Contains("Item[3,5]");
    }

    /// <summary>Verifies that TrySetValueToPropertyChain with shouldThrow=true throws for a read-only property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ReadOnlyProperty_ShouldThrowTrue_Throws()
    {
        Expression<Func<ReadOnlyModel, string>> expr = x => x.ReadOnlyValue;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var model = new ReadOnlyModel();
        var act = () => Reflection.TrySetValueToPropertyChain(model, chain, NewFieldValue);

        await Assert.That(act).ThrowsException();
    }

    /// <summary>Verifies that TrySetValueToPropertyChain with shouldThrow=false still throws for a read-only property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ReadOnlyProperty_ShouldThrowFalse_Throws()
    {
        Expression<Func<ReadOnlyModel, string>> expr = x => x.ReadOnlyValue;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var model = new ReadOnlyModel();
        var act = () => Reflection.TrySetValueToPropertyChain(model, chain, NewFieldValue, false);

        // The non-throwing lookup still hands back a setter for any property; invoking it on a getter-only one throws.
        await Assert.That(act).ThrowsException();
    }

    /// <summary>A test model with an indexed property for testing index expressions.</summary>
    public sealed class TrueMultiArgIndexedModel
    {
        /// <summary>Backing dictionary for the multi-parameter indexer.</summary>
        private readonly Dictionary<GridCell, int> _data = [];

        /// <summary>Gets or sets the value at the specified row and column.</summary>
        /// <param name="row">The row index.</param>
        /// <param name="col">The column index.</param>
        /// <returns>The value at the specified position.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used for testing")]
        public int this[int row, int col]
        {
            get => _data.TryGetValue(new(row, col), out var val) ? val : 0;
            set => _data[new GridCell(row, col)] = value;
        }

        /// <summary>One addressable cell of the model's grid.</summary>
        /// <param name="Row">The row index.</param>
        /// <param name="Col">The column index.</param>
        internal readonly record struct GridCell(int Row, int Col);
    }

    /// <summary>A test model with a public field for testing field-based reflection.</summary>
    internal sealed class FieldTestModel
    {
        /// <summary>A public field for testing field-based reflection.</summary>
        [SuppressMessage("Design", "SST1401:Fields should be private", Justification = "A public field is what the field-reflection path under test resolves.")]
        public string PublicField = string.Empty;
    }

    /// <summary>A test model with an indexed property for testing index expressions.</summary>
    private sealed class IndexedModel
    {
        /// <summary>The first sample value stored in <see cref="Items"/>.</summary>
        private const int FirstItemValue = 10;

        /// <summary>The second sample value stored in <see cref="Items"/>.</summary>
        private const int SecondItemValue = 20;

        /// <summary>The third sample value stored in <see cref="Items"/>.</summary>
        private const int ThirdItemValue = 30;

        /// <summary>Gets a list of integers for testing index expressions.</summary>
        public List<int> Items { get; } = [FirstItemValue, SecondItemValue, ThirdItemValue];
    }

    /// <summary>A test model with a dictionary for testing multi-argument index expressions.</summary>
    private sealed class MultiArgIndexedModel
    {
        /// <summary>The sample value mapped to the seeded dictionary key.</summary>
        private const int SeededValue = 42;

        /// <summary>Gets a dictionary of string-to-int mappings for testing multi-argument index expressions.</summary>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used for testing")]
        public Dictionary<string, int> Items { get; } = new() { ["key1"] = SeededValue };
    }

    /// <summary>A test model with a read-only property.</summary>
    private sealed class ReadOnlyModel
    {
        /// <summary>Gets the read-only value. This property has no setter.</summary>
        public string ReadOnlyValue { get; } = "Immutable";
    }
}
