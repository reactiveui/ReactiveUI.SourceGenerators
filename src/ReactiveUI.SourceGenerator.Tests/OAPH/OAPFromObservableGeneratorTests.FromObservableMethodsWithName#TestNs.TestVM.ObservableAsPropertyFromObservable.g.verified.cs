﻿//HintName: TestNs.TestVM.ObservableAsPropertyFromObservable.g.cs
using ReactiveUI;
// <auto-generated/>
#pragma warning disable
#nullable enable
namespace TestNs
{
    /// <inheritdoc/>
    partial class TestVM
    {
        /// <inheritdoc cref="MyNamedProperty"/>
        [global::System.CodeDom.Compiler.GeneratedCode("ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator", "1.1.0.0")]
        private int _myNamedProperty;
        /// <inheritdoc cref="_myNamedPropertyHelper"/>
        [global::System.CodeDom.Compiler.GeneratedCode("ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator", "1.1.0.0")]
        private ReactiveUI.ObservableAsPropertyHelper<int>? _myNamedPropertyHelper;
        /// <inheritdoc cref="_myNamedProperty"/>
        [global::System.CodeDom.Compiler.GeneratedCode("ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator", "1.1.0.0")]
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public int MyNamedProperty { get => _myNamedProperty = _myNamedPropertyHelper?.Value ?? _myNamedProperty; }

        [global::System.CodeDom.Compiler.GeneratedCode("ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator", "1.1.0.0")]
        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected void InitializeOAPH()
        {
            _myNamedPropertyHelper = Test3()!.ToProperty(this, nameof(MyNamedProperty));
        }
    }
}