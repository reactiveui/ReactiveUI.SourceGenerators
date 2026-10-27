// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Provides event detection helpers for command binding.</summary>
internal static class EventHelpers
{
    /// <summary>Finds the default event on a control type for command binding. Searches for Click, TouchUpInside, Pressed in order.</summary>
    /// <param name="controlType">The control type.</param>
    /// <param name="eventArgsType">The resulting event args type name.</param>
    /// <returns>The name of the default event, or null if not found.</returns>
    internal static string? FindDefaultEvent(INamedTypeSymbol controlType, out string? eventArgsType)
    {
        eventArgsType = null;
        string[] defaultEvents = ["Click", "TouchUpInside", "Pressed"];

        for (var i = 0; i < defaultEvents.Length; i++)
        {
            var argsType = FindEventArgsType(controlType, defaultEvents[i]);
            if (argsType is null)
            {
                continue;
            }

            eventArgsType = argsType;
            return defaultEvents[i];
        }

        return null;
    }

    /// <summary>Finds the EventArgs type for a named event on a type, searching the type hierarchy.</summary>
    /// <param name="type">The type symbol.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The name of the EventArgs type, or null if the event was not found.</returns>
    internal static string? FindEventArgsType(INamedTypeSymbol type, string eventName)
    {
        var current = type;
        while (current is not null)
        {
            var members = current.GetMembers(eventName);
            for (var i = 0; i < members.Length; i++)
            {
                if (members[i] is IEventSymbol eventSymbol)
                {
                    return ExtractorValidation.ResolveEventArgsType(eventSymbol.Type);
                }
            }

            current = current.BaseType;
        }

        return null;
    }
}
