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
        /// <inheritdoc cref="Test7Property"/>
        private object? _test7Property;

        /// <inheritdoc cref="_test7PropertyHelper"/>
        private ReactiveUI.ObservableAsPropertyHelper<object?>? _test7PropertyHelper;

        /// <inheritdoc cref="_test7Property"/>
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [global::System.Text.Json.Serialization.JsonIncludeAttribute()]
        public object? Test7Property { get => _test7Property = (_test7PropertyHelper == null ? _test7Property : _test7PropertyHelper.Value); }

        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected void InitializeOAPH()
        {
            _test7PropertyHelper = Test7!.ToProperty(this, nameof(Test7Property));
        }
    }
}
#nullable restore
#pragma warning restore