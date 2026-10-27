// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>The named arguments of an <c>[ObservableAsProperty]</c> that change what the generator writes.</summary>
/// <param name="ReadOnly">Whether the helper field is <c>readonly</c>.</param>
/// <param name="UseProtected">Whether the helper field is <c>protected</c> rather than <c>private</c>.</param>
/// <param name="InitialValue">The value the property returns until its helper is assigned, or null.</param>
internal readonly record struct ObservableAsPropertyOptions(bool ReadOnly, bool UseProtected, string? InitialValue)
{
    /// <summary>Reads the options from an attribute, ignoring any argument of an unexpected type.</summary>
    /// <param name="attribute">The <c>[ObservableAsProperty]</c> attribute.</param>
    /// <returns>The options; the defaults for any argument not given.</returns>
    internal static ObservableAsPropertyOptions Read(AttributeData attribute)
    {
        var readOnly = false;
        var useProtected = false;
        string? initialValue = null;
        var arguments = attribute.NamedArguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            var (name, value) = (arguments[i].Key, arguments[i].Value.Value);
            switch (name)
            {
                case "ReadOnly" when value is bool flag:
                {
                    readOnly = flag;
                    break;
                }

                case "UseProtected" when value is bool flag:
                {
                    useProtected = flag;
                    break;
                }

                case "InitialValue" when value is string text:
                {
                    initialValue = text;
                    break;
                }
            }
        }

        return new(readOnly, useProtected, initialValue);
    }
}
