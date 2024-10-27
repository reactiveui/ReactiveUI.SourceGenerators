// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

internal static class FieldSyntaxExtensions
{
    /// <summary>
    /// Get the generated property name for an input field.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>The generated property name for <paramref name="fieldSymbol"/>.</returns>
    internal static string GetGeneratedPropertyName(this IFieldSymbol fieldSymbol)
    {
        var propertyName = fieldSymbol.Name;

        if (propertyName.StartsWith("m_"))
        {
            propertyName = propertyName.Substring(2);
        }
        else if (propertyName.StartsWith("_"))
        {
            propertyName = propertyName.TrimStart('_');
        }

        return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
    }
}
