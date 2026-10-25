// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

/// <summary>Raises notifications through a public or internal raise method that generated code can call directly.</summary>
/// <remarks>
/// A view model that exposes <c>RaisePropertyChanged(string)</c> or <c>OnPropertyChanged(PropertyChangedEventArgs)</c>
/// to its own assembly needs nothing more. The method is looked for by its conventional names, on the type and its
/// base types, and has to be accessible from the consumer's assembly, which is where generated code runs.
/// </remarks>
internal sealed class RaiseMethodRaisePlugin : IPropertyRaisePlugin
{
    /// <summary>The name this mechanism is recorded under.</summary>
    internal const string MechanismName = "RaiseMethod";

    /// <inheritdoc/>
    public int Affinity => BindingAffinity.Explicit;

    /// <inheritdoc/>
    public PropertyRaiseInfo? Select(INamedTypeSymbol type, Compilation compilation)
    {
        var changed = RaiseMembers.FindMethod(
            type,
            changing: false,
            compilation.GetTypeByMetadataName(Constants.PropertyChangedEventArgsMetadataName),
            compilation.Assembly,
            compilation);
        if (changed is not { } changedCall)
        {
            return null;
        }

        var changing = RaiseMembers.FindMethod(
            type,
            changing: true,
            compilation.GetTypeByMetadataName(Constants.PropertyChangingEventArgsMetadataName),
            compilation.Assembly,
            compilation);

        return new(MechanismName, changedCall, changing, null);
    }
}
