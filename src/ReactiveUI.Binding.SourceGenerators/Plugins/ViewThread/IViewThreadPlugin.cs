// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Names the runtime invoker a generated binding routes its writes through for one UI platform.</summary>
/// <remarks>
/// The invokers live in the platform packages, one per runtime flavour. A generated binding references whichever
/// the consumer's compilation can see; when it sees neither, the binding carries no invoker and the runtime
/// registry resolves one.
/// </remarks>
internal interface IViewThreadPlugin
{
    /// <summary>Gets the metadata name of the platform type whose instances this invoker writes to.</summary>
    string OwnerMetadataName { get; }

    /// <summary>Gets the metadata name of the invoker in the lean runtime's platform package.</summary>
    string InvokerMetadataName { get; }

    /// <summary>Gets the metadata name of the invoker in the System.Reactive runtime's platform package.</summary>
    string ReactiveInvokerMetadataName { get; }

    /// <summary>Gets the name of the lean platform package that ships the invoker.</summary>
    /// <remarks>
    /// Null when a binding onto the platform's objects without the package is not reported, because the platform's
    /// type is shared with a platform the package does not serve.
    /// </remarks>
    string? PackageName { get; }
}
