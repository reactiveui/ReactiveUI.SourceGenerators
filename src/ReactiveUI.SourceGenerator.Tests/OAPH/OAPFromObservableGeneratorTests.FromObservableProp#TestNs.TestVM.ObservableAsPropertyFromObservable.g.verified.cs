﻿//HintName: TestNs.TestVM.ObservableAsPropertyFromObservable.g.cs
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
        /// <inheritdoc cref="Test1Property"/>
        private int _test1Property;

        /// <inheritdoc cref="_test1PropertyHelper"/>
        private ReactiveUI.ObservableAsPropertyHelper<int>? _test1PropertyHelper;

        /// <inheritdoc cref="_test1Property"/>
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public int Test1Property { get => _test1Property = _test1PropertyHelper?.Value ?? _test1Property; }

        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected void InitializeOAPH()
        {
            _test1PropertyHelper = Test1!.ToProperty(this, nameof(Test1Property));
        }
    }
}
#nullable restore
#pragma warning restore