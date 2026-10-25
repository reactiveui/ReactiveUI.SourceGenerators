// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Expressions;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>Tests for the expression rewriter which normalizes expression trees for property path extraction.</summary>
public class ExpressionRewriterTests
{
    /// <summary>The hint the rewriter adds when a constant is used where a member access was expected.</summary>
    private const string ExpressionHintMessage = "Did you meant to use expressions";

    /// <summary>The name of the property used in the rewriter tests.</summary>
    private const string LengthPropertyName = "Length";

    /// <summary>The right-hand operand of the comparison in the expression under test.</summary>
    private const int ComparisonThreshold = 5;

    /// <summary>The constant value embedded in the expression under test.</summary>
    private const int SampleConstant = 42;

    /// <summary>The name of the property used across rewriter tests.</summary>
    private const string PropertyName = "Property";

    /// <summary>The expected number of rewritten arguments in the argument-list test.</summary>
    private const int ExpectedArgumentCount = 3;

    /// <summary>The index of the third rewritten argument.</summary>
    private const int ThirdArgumentIndex = 2;

    /// <summary>The second constant value used in the argument-list test.</summary>
    private const int SecondConstant = 2;

    /// <summary>The third constant value used in the argument-list test.</summary>
    private const int ThirdConstant = 3;

    /// <summary>Verifies that array index expressions are rewritten to index expressions.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithArrayIndex_ReturnsIndexExpression()
    {
        Expression<Func<TestClass, int>> expr = x => x.Array[0];

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>Verifies that array index with non-constant index throws.</summary>
    /// <param name="index">A non-constant index, supplied as a parameter so it is captured as a
    ///     non-constant closure reference (a <c>const</c> or literal would be inlined into the tree).</param>
    [Test]
    [Arguments(0)]
    public void Rewrite_WithArrayIndexNonConstant_Throws(int index)
    {
        Expression<Func<TestClass, int>> expr = x => x.Array[index];

        _ = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
    }

    /// <summary>Verifies that array length is rewritten to member access.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithArrayLength_ReturnsMemberAccess()
    {
        Expression<Func<TestClass, int>> expr = x => x.Array.Length;

        var result = Reflection.Rewrite(expr.Body);

        // ArrayLength should be rewritten to MemberAccess of Length property
        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        await Assert.That(((MemberExpression)result).Member.Name).IsEqualTo(LengthPropertyName);
    }

    /// <summary>Verifies that constant expressions pass through unchanged.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithConstant_ReturnsConstantExpression()
    {
        Expression<Func<string>> expr = () => "test";

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Constant);
    }

    /// <summary>Verifies that convert expressions are unwrapped.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithConvert_ReturnsUnderlyingExpression()
    {
        Expression<Func<TestClass, object>> expr = x => x.Property!;

        var result = Reflection.Rewrite(expr.Body);

        // Convert should be unwrapped to the underlying MemberAccess
        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>Verifies that index expression with constant arguments are validated.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithIndexExpression_ValidatesConstantArguments()
    {
        Expression<Func<TestClass, int>> expr = x => x.List[1];

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>Verifies that index expression with non-constant arguments throws.</summary>
    [Test]
    public void Rewrite_WithIndexExpressionNonConstantArguments_Throws()
    {
        // Create an IndexExpression with non-constant arguments
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var listProperty = System.Linq.Expressions.Expression.Property(parameter, "List");
        var indexer = typeof(List<int>).GetProperty("Item")!;
        var nonConstantArg = System.Linq.Expressions.Expression.Parameter(typeof(int), "index");
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(listProperty, indexer, [nonConstantArg]);

        _ = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(indexExpr));
    }

    /// <summary>Verifies that list indexer expressions are rewritten to index expressions.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithListIndexer_ReturnsIndexExpression()
    {
        Expression<Func<TestClass, int>> expr = x => x.List[0];

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>Verifies that list indexer with non-constant index throws.</summary>
    /// <param name="index">A non-constant index, supplied as a parameter so it is captured as a
    ///     non-constant closure reference (a <c>const</c> or literal would be inlined into the tree).</param>
    [Test]
    [Arguments(0)]
    public void Rewrite_WithListIndexerNonConstant_Throws(int index)
    {
        Expression<Func<TestClass, int>> expr = x => x.List[index];

        _ = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
    }

    /// <summary>Verifies that member access expressions pass through unchanged.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithMemberAccess_ReturnsMemberExpression()
    {
        Expression<Func<TestClass, string?>> expr = x => x.Property;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>Verifies that non-special method calls throw.</summary>
    [Test]
    public void Rewrite_WithMethodCallNonSpecialName_Throws()
    {
        Expression<Func<TestClass, string?>> expr = x => x.GetValue();

        _ = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
    }

    /// <summary>Verifies that nested member access expressions pass through unchanged.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithNestedMemberAccess_ReturnsMemberExpression()
    {
        Expression<Func<TestClass, string?>> expr = x => x.Nested!.Property;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
    }

    /// <summary>Verifies that null expression throws.</summary>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Rewrite_WithNullExpression_Throws() =>
        Assert.Throws<ArgumentNullException>(static () => Reflection.Rewrite(null));

    /// <summary>Verifies that parameter expressions pass through unchanged.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithParameterExpression_ReturnsParameterExpression()
    {
        Expression<Func<TestClass, TestClass>> expr = x => x;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Parameter);
    }

    /// <summary>Verifies that unsupported unary expressions throw.</summary>
    [Test]
    public void Rewrite_WithUnaryExpressionNotArrayLengthOrConvert_Throws()
    {
        // Create a unary expression that is not ArrayLength or Convert (e.g., Not)
        // This should trigger the unsupported expression path
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(bool), "x");
        var notExpr = System.Linq.Expressions.Expression.Not(parameter);

        _ = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(notExpr));
    }

    /// <summary>Verifies that unsupported binary expressions throw with helpful message.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithUnsupportedBinaryExpression_ThrowsWithHelpfulMessage()
    {
        Expression<Func<int, bool>> expr = x => x > ComparisonThreshold;

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
        await Assert.That(ex!.Message).Contains(ExpressionHintMessage);
    }

    /// <summary>Verifies that unsupported expressions throw with node type in message.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithUnsupportedExpression_Throws()
    {
        Expression<Func<int, int>> expr = x => x + 1;

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(expr.Body));
        await Assert.That(ex!.Message).Contains("Unsupported expression");
        await Assert.That(ex.Message).Contains("Add");
    }

    /// <summary>Verifies that Convert expression wrapping a member access is unwrapped to the inner member access.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithConvertWrappingMemberAccess_UnwrapsToMemberAccess()
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, PropertyName);
        var convert = System.Linq.Expressions.Expression.Convert(property, typeof(object));

        var result = Reflection.Rewrite(convert);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        await Assert.That(((MemberExpression)result).Member.Name).IsEqualTo(PropertyName);
    }

    /// <summary>Verifies that a static method call (non-special-name) throws NotSupportedException.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Rewrite_WithStaticMethodCall_ThrowsNotSupportedException()
    {
        var methodCall = System.Linq.Expressions.Expression.Call(
            typeof(int).GetMethod("Parse", [typeof(string)])!,
            System.Linq.Expressions.Expression.Constant("42"));

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(methodCall));
        await Assert.That(ex).IsNotNull();
    }

    /// <summary>
    /// Verifies that GetItemProperty throws InvalidOperationException for a type without an "Item" indexer.
    /// Covers ExpressionRewriter line 78 (null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetItemProperty_TypeWithoutIndexer_ThrowsInvalidOperationException()
    {
        var action = static () => ExpressionRewriter.GetItemProperty(typeof(string));

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that GetLengthProperty throws InvalidOperationException for a type without a "Length" property.
    /// Covers ExpressionRewriter line 86 (null branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLengthProperty_TypeWithoutLength_ThrowsInvalidOperationException()
    {
        var action = static () => ExpressionRewriter.GetLengthProperty(typeof(int));

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that Rewrite handles array index expressions where the left side is an array type,
    /// producing an ArrayAccess IndexExpression.
    /// Covers ExpressionRewriter line 132 (IsArray TRUE branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_ArrayIndexWithArrayType_ProducesArrayAccess()
    {
        // x => x.Array[0] produces BinaryExpression(ArrayIndex) for int[]
        Expression<Func<TestClass, int>> expr = x => x.Array[0];

        var result = Reflection.Rewrite(expr.Body);

        // After rewrite, it should be an IndexExpression with NodeType == Index
        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);

        // The object type should be int[] (array type)
        await Assert.That(((IndexExpression)result).Object!.Type.IsArray).IsTrue();
    }

    /// <summary>
    /// Verifies that Rewrite handles ArrayLength unary expressions by converting them to MemberAccess on Length.
    /// Covers ExpressionRewriter line 155 (ArrayLength TRUE branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_ArrayLength_ProducesMemberAccessOnLength()
    {
        // x => x.Array.Length creates a UnaryExpression(ArrayLength)
        Expression<Func<TestClass, int>> expr = x => x.Array.Length;

        var result = Reflection.Rewrite(expr.Body);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        var memberExpr = (MemberExpression)result;
        await Assert.That(memberExpr.Member.Name).IsEqualTo(LengthPropertyName);
        await Assert.That(memberExpr.Expression!.Type).IsEqualTo(typeof(int[]));
    }

    /// <summary>
    /// Verifies that VisitIndex throws NotSupportedException when IndexExpression has non-constant arguments.
    /// Covers ExpressionRewriter line 190 (!AllConstant branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_IndexExpressionWithNonConstantArguments_ThrowsNotSupportedException()
    {
        // Manually build an IndexExpression with a non-constant argument
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var listProperty = System.Linq.Expressions.Expression.Property(parameter, "List");
        var indexer = typeof(List<int>).GetProperty("Item")!;
        var nonConstantArg = System.Linq.Expressions.Expression.Parameter(typeof(int), "idx");
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(listProperty, indexer, [nonConstantArg]);

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(indexExpr));
        await Assert.That(ex!.Message).Contains("only supported with constants");
    }

    /// <summary>
    /// Verifies that AllConstant returns true for an empty argument collection.
    /// Covers ExpressionRewriter line 105 (count==0 FALSE path, i.e. loop never executes).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AllConstant_EmptyCollection_ReturnsTrue()
    {
        var emptyList = new System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.Expression>(
            []);

        var result = ExpressionRewriter.AllConstant(emptyList);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that AllConstant returns false when a non-constant expression is present.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AllConstant_WithNonConstant_ReturnsFalse()
    {
        var list = new System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.Expression>(
        [
            System.Linq.Expressions.Expression.Constant(1),
            System.Linq.Expressions.Expression.Parameter(typeof(int), "x")
        ]);

        var result = ExpressionRewriter.AllConstant(list);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that AllConstant returns true when all expressions are constants.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AllConstant_WithAllConstants_ReturnsTrue()
    {
        var list = new System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.Expression>(
        [
            System.Linq.Expressions.Expression.Constant(1),
            System.Linq.Expressions.Expression.Constant(SecondConstant)
        ]);

        var result = ExpressionRewriter.AllConstant(list);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that VisitArgumentList returns an empty array when called with an empty argument collection.
    /// Covers ExpressionRewriter.cs VisitArgumentList line 105-107 (count == 0 early return).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task VisitArgumentList_EmptyArguments_ReturnsEmptyArray()
    {
        var rewriter = new ExpressionRewriter();
        var emptyArgs = new System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.Expression>(
            []);

        var result = rewriter.VisitArgumentList(emptyArgs);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that VisitArgumentList visits each argument in the collection.
    /// Covers ExpressionRewriter.cs VisitArgumentList lines 110-116 (non-empty loop).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task VisitArgumentList_WithConstantArguments_VisitsEach()
    {
        var rewriter = new ExpressionRewriter();
        var args = new System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.Expression>(
        [
            System.Linq.Expressions.Expression.Constant(1),
            System.Linq.Expressions.Expression.Constant(SecondConstant),
            System.Linq.Expressions.Expression.Constant(ThirdConstant)
        ]);

        var result = rewriter.VisitArgumentList(args);

        await Assert.That(result.Length).IsEqualTo(ExpectedArgumentCount);
        await Assert.That(result[0].NodeType).IsEqualTo(ExpressionType.Constant);
        await Assert.That(result[1].NodeType).IsEqualTo(ExpressionType.Constant);
        await Assert.That(result[ThirdArgumentIndex].NodeType).IsEqualTo(ExpressionType.Constant);
    }

    /// <summary>
    /// Verifies that CreateUnsupportedNodeException produces a message containing the node type
    /// for a non-binary expression (no "Did you meant to use" hint).
    /// Covers ExpressionRewriter.cs CreateUnsupportedNodeException line 54-59 without line 61-68.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateUnsupportedNodeException_NonBinaryExpression_DoesNotContainBinaryHint()
    {
        var constant = System.Linq.Expressions.Expression.Constant(SampleConstant);

        var exception = ExpressionRewriter.CreateUnsupportedNodeException(constant);

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception.Message).Contains("Unsupported expression of type 'Constant'");
        await Assert.That(exception.Message).DoesNotContain(ExpressionHintMessage);
    }

    /// <summary>
    /// Verifies that CreateUnsupportedNodeException includes binary operand hints for BinaryExpression nodes.
    /// Covers ExpressionRewriter.cs CreateUnsupportedNodeException lines 61-68 (BinaryExpression branch).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateUnsupportedNodeException_BinaryExpression_ContainsBinaryHint()
    {
        var left = System.Linq.Expressions.Expression.Constant(1);
        var right = System.Linq.Expressions.Expression.Constant(SecondConstant);
        var binary = System.Linq.Expressions.Expression.Add(left, right);

        var exception = ExpressionRewriter.CreateUnsupportedNodeException(binary);

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception.Message).Contains(ExpressionHintMessage);
        await Assert.That(exception.Message).Contains("1");
        await Assert.That(exception.Message).Contains("2");
    }

    /// <summary>
    /// Verifies that Convert wrapping a parameter expression is unwrapped to the parameter.
    /// Covers the Convert path through VisitUnary where the operand is a ParameterExpression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_ConvertWrappingParameter_UnwrapsToParameter()
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(string), "x");
        var convert = System.Linq.Expressions.Expression.Convert(parameter, typeof(object));

        var result = Reflection.Rewrite(convert);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Parameter);
    }

    /// <summary>
    /// Verifies that a double-nested Convert expression is fully unwrapped.
    /// Covers ExpressionRewriter VisitUnary Convert path recursion.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_DoubleConvert_UnwrapsCompletely()
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, PropertyName);
        var innerConvert = System.Linq.Expressions.Expression.Convert(property, typeof(object));
        var outerConvert = System.Linq.Expressions.Expression.Convert(innerConvert, typeof(object));

        var result = Reflection.Rewrite(outerConvert);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        await Assert.That(((MemberExpression)result).Member.Name).IsEqualTo(PropertyName);
    }

    /// <summary>
    /// Verifies that a special-name method call with non-constant arguments throws NotSupportedException.
    /// Covers ExpressionRewriter.cs VisitMethodCall line 171 (!AllConstant path for special-name methods).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_SpecialNameMethodCallWithNonConstantArgs_ThrowsNotSupportedException()
    {
        // Create a method call for List<int>.get_Item(param) where param is non-constant
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var listProperty = System.Linq.Expressions.Expression.Property(parameter, "List");
        var indexerGetter = typeof(List<int>).GetMethod("get_Item")!;
        var nonConstantArg = System.Linq.Expressions.Expression.Parameter(typeof(int), "idx");

        var methodCall = System.Linq.Expressions.Expression.Call(
            listProperty,
            indexerGetter,
            nonConstantArg);

        var ex = Assert.Throws<NotSupportedException>(() => Reflection.Rewrite(methodCall));
        await Assert.That(ex!.Message).Contains("only supported with constants");
    }

    /// <summary>
    /// Verifies that a special-name method call with constant arguments rewrites to an IndexExpression.
    /// Covers ExpressionRewriter.cs VisitMethodCall lines 181-184 (happy path: instance, special-name, constant args).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_SpecialNameMethodCallWithConstantArgs_RewritesToIndex()
    {
        // Create a method call for List<int>.get_Item(0) — special-name, constant arg, instance call
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TestClass), "x");
        var listProperty = System.Linq.Expressions.Expression.Property(parameter, "List");
        var indexerGetter = typeof(List<int>).GetMethod("get_Item")!;
        var constantArg = System.Linq.Expressions.Expression.Constant(0);

        var methodCall = System.Linq.Expressions.Expression.Call(
            listProperty,
            indexerGetter,
            constantArg);

        var result = Reflection.Rewrite(methodCall);

        await Assert.That(result.NodeType).IsEqualTo(ExpressionType.Index);
    }

    /// <summary>
    /// Verifies that GetItemProperty returns a valid property for a type with an indexer.
    /// Covers ExpressionRewriter.cs GetItemProperty line 77 (non-null return path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetItemProperty_TypeWithIndexer_ReturnsProperty()
    {
        var property = ExpressionRewriter.GetItemProperty(typeof(List<int>));

        await Assert.That(property).IsNotNull();
        await Assert.That(property.Name).IsEqualTo("Item");
    }

    /// <summary>
    /// Verifies that GetLengthProperty returns a valid property for a type with a Length property.
    /// Covers ExpressionRewriter.cs GetLengthProperty line 85 (non-null return path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetLengthProperty_TypeWithLength_ReturnsProperty()
    {
        var property = ExpressionRewriter.GetLengthProperty(typeof(int[]));

        await Assert.That(property).IsNotNull();
        await Assert.That(property.Name).IsEqualTo(LengthPropertyName);
    }

    /// <summary>Test class used as a type parameter in expression lambdas for rewriter tests.</summary>
    private sealed class TestClass
    {
        /// <summary>The second sample value stored in <see cref="Array"/>.</summary>
        private const int ArraySecondValue = 2;

        /// <summary>The third sample value stored in <see cref="Array"/>.</summary>
        private const int ArrayThirdValue = 3;

        /// <summary>The first sample value stored in <see cref="List"/>.</summary>
        private const int ListFirstValue = 4;

        /// <summary>The second sample value stored in <see cref="List"/>.</summary>
        private const int ListSecondValue = 5;

        /// <summary>The third sample value stored in <see cref="List"/>.</summary>
        private const int ListThirdValue = 6;

        /// <summary>Gets an integer array for testing array index and length expressions.</summary>
        public int[] Array { get; } = [1, ArraySecondValue, ArrayThirdValue];

        /// <summary>Gets a list of integers for testing list indexer expressions.</summary>
        public List<int> List { get; } = [ListFirstValue, ListSecondValue, ListThirdValue];

        /// <summary>Gets or sets a nested instance for testing nested member access expressions.</summary>
        public TestClass? Nested { get; set; }

        /// <summary>Gets or sets a string property for testing simple member access expressions.</summary>
        public string? Property { get; set; }

        /// <summary>Gets the value of <see cref="Property"/>. Used to test non-special-name method call rewriting.</summary>
        /// <returns>The current value of the <see cref="Property"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetValue() => Property;
    }
}
