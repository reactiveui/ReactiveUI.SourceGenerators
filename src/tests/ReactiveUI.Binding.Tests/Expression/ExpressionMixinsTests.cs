// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>Tests for the <see cref="ExpressionMixins"/> class.</summary>
public class ExpressionMixinsTests
{
    /// <summary>The constant value embedded in the expression under test.</summary>
    private const int SampleConstant = 42;

    /// <summary>The expected number of expressions for a two-member chain.</summary>
    private const int TwoMemberChainCount = 2;

    /// <summary>The expected number of expressions for a three-member chain.</summary>
    private const int ThreeMemberChainCount = 3;

    /// <summary>The index argument used in indexer expression tests.</summary>
    private const int IndexerArgument = 2;

    /// <summary>Verifies that GetExpressionChain returns a single member for a simple property access.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_SimpleProperty_ReturnsSingleExpression()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that GetExpressionChain returns multiple members for a nested property chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_NestedProperty_ReturnsMultipleExpressions()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(TwoMemberChainCount);
    }

    /// <summary>Verifies that GetExpressionChain returns three members for a 3-level deep property chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_ThreeLevelDeepChain_ReturnsThreeExpressions()
    {
        Expression<Func<ObjChain1, HostTestFixture?>> expr = x => x.Chain2!.Chain3!.Host;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(ThreeMemberChainCount);

        var firstMember = chain[0].GetMemberInfo();
        var secondMember = chain[1].GetMemberInfo();
        var thirdMember = chain[2].GetMemberInfo();

        await Assert.That(firstMember).IsNotNull();
        await Assert.That(firstMember!.Name).IsEqualTo("Chain2");
        await Assert.That(secondMember).IsNotNull();
        await Assert.That(secondMember!.Name).IsEqualTo("Chain3");
        await Assert.That(thirdMember).IsNotNull();
        await Assert.That(thirdMember!.Name).IsEqualTo("Host");
    }

    /// <summary>Verifies that GetMemberInfo returns the correct member for a property expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberInfo_PropertyExpression_ReturnsMemberInfo()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();
        var memberInfo = chain[0].GetMemberInfo();

        await Assert.That(memberInfo).IsNotNull();
        await Assert.That(memberInfo!.Name).IsEqualTo("Name");
    }

    /// <summary>Verifies that GetMemberInfo unwraps Convert expressions (boxing scenarios) and returns the underlying member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberInfo_ConvertExpression_UnwrapsToUnderlyingMember()
    {
        // Boxing int to object creates a Convert expression wrapping MemberAccess
        Expression<Func<TestViewModel, object>> expr = x => x.Age;
        var body = expr.Body;

        // The body should be a Convert node (boxing int -> object)
        await Assert.That(body.NodeType).IsEqualTo(ExpressionType.Convert);

        var memberInfo = body.GetMemberInfo();

        await Assert.That(memberInfo).IsNotNull();
        await Assert.That(memberInfo!.Name).IsEqualTo("Age");
        await Assert.That(memberInfo.MemberType).IsEqualTo(MemberTypes.Property);
    }

    /// <summary>Verifies that GetArgumentsArray returns null for a non-index member access expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetArgumentsArray_NonIndexExpression_ReturnsNull()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var args = body.GetArgumentsArray();

        await Assert.That(args).IsNull();
    }

    /// <summary>Verifies that GetParent returns the correct parent expression for a simple member access.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetParent_MemberExpression_ReturnsParent()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body) as MemberExpression;

        var parent = body!.GetParent();

        await Assert.That(parent).IsNotNull();
        await Assert.That(parent!.NodeType).IsEqualTo(ExpressionType.Parameter);
    }

    /// <summary>Verifies that GetParent returns the intermediate expression for a nested member access.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetParent_NestedMemberExpression_ReturnsIntermediateExpression()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body) as MemberExpression;

        // body is City, parent should be Address (a MemberAccess on the parameter)
        var parent = body!.GetParent();

        await Assert.That(parent).IsNotNull();
        await Assert.That(parent!.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        await Assert.That(((MemberExpression)parent).Member.Name).IsEqualTo("Address");
    }

    /// <summary>Verifies that GetExpressionChain throws NotSupportedException for a ConstantExpression with a helpful message.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetExpressionChain_ConstantExpression_ThrowsWithHelpfulMessage()
    {
        var constant = System.Linq.Expressions.Expression.Constant("hello");

        var ex = Assert.Throws<NotSupportedException>(() => _ = constant.GetExpressionChain().ToList());
        await Assert.That(ex!.Message).Contains("Did you miss the member access prefix");
    }

    /// <summary>Verifies that GetMemberInfo throws NotSupportedException for an unsupported expression type.</summary>
    [Test]
    public void GetMemberInfo_UnsupportedExpression_ThrowsNotSupportedException()
    {
        var constant = System.Linq.Expressions.Expression.Constant(SampleConstant);

        _ = Assert.Throws<NotSupportedException>(() => constant.GetMemberInfo());
    }

    /// <summary>Verifies that GetParent throws NotSupportedException for an unsupported expression type.</summary>
    [Test]
    public void GetParent_UnsupportedExpression_ThrowsNotSupportedException()
    {
        var constant = System.Linq.Expressions.Expression.Constant(SampleConstant);

        _ = Assert.Throws<NotSupportedException>(() => constant.GetParent());
    }

    /// <summary>Verifies that GetArgumentsArray returns constant arguments for an index expression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetArgumentsArray_IndexExpression_ReturnsConstantArguments()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[IndexerArgument];
        var body = Reflection.Rewrite(expr.Body);

        var args = body.GetArgumentsArray();

        await Assert.That(args).IsNotNull();
        await Assert.That(args!.Length).IsEqualTo(1);
        await Assert.That(args[0]).IsEqualTo(IndexerArgument);
    }

    /// <summary>Verifies that GetExpressionChain handles index expressions in chains.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_WithIndexExpression_ReturnsChainIncludingIndex()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        await Assert.That(chain.Count).IsEqualTo(TwoMemberChainCount);
    }

    /// <summary>Verifies that GetMemberInfo returns indexer info for an IndexExpression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetMemberInfo_IndexExpression_ReturnsIndexerInfo()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        // body is an IndexExpression for Items[0]
        var memberInfo = body.GetMemberInfo();

        await Assert.That(memberInfo).IsNotNull();
        await Assert.That(memberInfo!.Name).IsEqualTo("Item");
    }

    /// <summary>Verifies that GetParent returns the object expression for an IndexExpression.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetParent_IndexExpression_ReturnsObjectExpression()
    {
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var parent = body.GetParent();

        await Assert.That(parent).IsNotNull();
        await Assert.That(parent!.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>
    /// Verifies that GetExpressionChain correctly handles an IndexExpression whose Object is not a Parameter
    /// (nested indexer access like x.Items[0]) by producing an updated IndexExpression with a Parameter node.
    /// Covers ExpressionMixins.cs line 33 (TRUE path for nested Index with non-Parameter Object).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_NestedIndexExpression_HandlesNonParameterObject()
    {
        // x => x.Items[0] produces a chain: [Items (MemberAccess), Item[0] (Index)]
        // The IndexExpression's Object is MemberAccess (not Parameter), triggering the nested branch
        Expression<Func<IndexTestClass, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var chain = body.GetExpressionChain().ToList();

        // Should return 2 expressions: MemberAccess for Items, then Index for [0]
        await Assert.That(chain.Count).IsEqualTo(TwoMemberChainCount);

        // First expression should be MemberAccess (Items property)
        await Assert.That(chain[0].NodeType).IsEqualTo(ExpressionType.MemberAccess);

        // Second expression should be Index
        await Assert.That(chain[1].NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>
    /// Verifies that GetExpressionChain correctly handles an IndexExpression where Object IS a Parameter
    /// (direct indexer on parameter like x[0]) by adding the IndexExpression directly.
    /// Covers ExpressionMixins.cs line 33 (FALSE/else path for Index with Parameter Object).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetExpressionChain_DirectIndexOnParameter_AddsIndexDirectly()
    {
        // Build an IndexExpression directly on the parameter: param["key"]
        var param = System.Linq.Expressions.Expression.Parameter(typeof(DirectIndexableClass), "x");
        var indexer = typeof(DirectIndexableClass).GetProperty("Item")!;
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(
            param,
            indexer,
            [System.Linq.Expressions.Expression.Constant("key")]);

        var chain = indexExpr.GetExpressionChain().ToList();

        // Should return 1 expression: the Index expression itself
        await Assert.That(chain.Count).IsEqualTo(1);
        await Assert.That(chain[0].NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>Test class with a list property for testing index expression chains.</summary>
    public sealed class DirectIndexableClass
    {
        /// <summary>Backing dictionary for the string indexer.</summary>
        private readonly Dictionary<string, string> _data = new() { ["key"] = "value" };

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value associated with the key, or an empty string if not found.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used for testing")]
        public string this[string key]
        {
            get => _data.TryGetValue(key, out var val) ? val : string.Empty;
            set => _data[key] = value;
        }
    }

    /// <summary>Test class with a list property for testing index expression chains.</summary>
    private sealed class IndexTestClass
    {
        /// <summary>The first value stored in the test items list.</summary>
        private const int FirstItemValue = 10;

        /// <summary>The second value stored in the test items list.</summary>
        private const int SecondItemValue = 20;

        /// <summary>The third value stored in the test items list.</summary>
        private const int ThirdItemValue = 30;

        /// <summary>Gets a list of integers for testing index expressions.</summary>
        public List<int> Items { get; } = [FirstItemValue, SecondItemValue, ThirdItemValue];
    }
}
