﻿//HintName: TestVM.ObservableAsPropertyFromObservable.g.cs
// <auto-generated/>
using ReactiveUI;

#pragma warning disable
#nullable enable

namespace TestNs
{
    /// <summary>
    /// Partial class for the TestVM which contains ReactiveUI Reactive property initialization.
    /// </summary>
    public partial class TestVM
    {
        /// <inheritdoc cref="Test6Property"/>
        private object _test6Property;

        /// <inheritdoc cref="_test6PropertyHelper"/>
        private ReactiveUI.ObservableAsPropertyHelper<object>? _test6PropertyHelper;

        /// <inheritdoc cref="_test6Property"/>
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [global::System.Text.Json.Serialization.JsonIncludeAttribute()]
        public object Test6Property { get => _test6Property = _test6PropertyHelper?.Value ?? _test6Property; }

        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected void InitializeOAPH()
        {
            _test6PropertyHelper = Test6!.ToProperty(this, nameof(Test6Property));
        }
    }
}
#nullable restore
#pragma warning restore