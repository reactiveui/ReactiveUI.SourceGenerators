﻿//HintName: TestNs.TestVM.ObservableAsPropertyFromObservable.g.cs
// <auto-generated/>
using ReactiveUI;

#pragma warning disable
#nullable enable

namespace TestNs
{
    /// <summary>
    /// Partial class for the TestVM which contains ReactiveUI Observable As Property initialization.
    /// </summary>
    public partial class TestVM
    {
        /// <inheritdoc cref="Test3Property"/>
        private int _test3Property;

        /// <inheritdoc cref="_test3PropertyHelper"/>
        private ReactiveUI.ObservableAsPropertyHelper<int>? _test3PropertyHelper;

        /// <inheritdoc cref="_test3Property"/>
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public int Test3Property { get => _test3Property = _test3PropertyHelper?.Value ?? _test3Property; }

        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected void InitializeOAPH()
        {
            _test3PropertyHelper = Test3()!.ToProperty(this, nameof(Test3Property));
        }
    }
}
#nullable restore
#pragma warning restore