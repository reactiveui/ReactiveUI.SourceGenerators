// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>A form field for the reference on the payee's statement. The bank prints at most 18 characters, so the field refuses longer text.</summary>
[System.Diagnostics.DebuggerDisplay("StatementReferenceView: Reference = {Reference}")]
public sealed class StatementReferenceView : ObservableObject
{
    /// <summary>The number of characters the bank prints for a reference.</summary>
    private const int MaxLength = 18;

    /// <summary>Gets or sets the reference in the field.</summary>
    /// <exception cref="ArgumentException">The text is longer than 18 characters.</exception>
    public string Reference
    {
        get;
        set
        {
            if (value.Length > MaxLength)
            {
                throw new ArgumentException($"A statement reference has at most {MaxLength} characters.", nameof(value));
            }

            _ = SetProperty(ref field, value);
        }
    } = string.Empty;
}
