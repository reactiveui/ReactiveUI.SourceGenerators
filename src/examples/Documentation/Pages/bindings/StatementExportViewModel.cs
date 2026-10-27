// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>The dialog that exports an account statement. The export runs only while a statement is chosen.</summary>
[System.Diagnostics.DebuggerDisplay("StatementExportViewModel: ExportCount = {ExportCount}")]
public sealed class StatementExportViewModel : ObservableObject
{
    /// <summary>Initializes a new instance of the <see cref="StatementExportViewModel"/> class.</summary>
    public StatementExportViewModel() => ExportCommand = new(() => ExportCount++, () => HasStatement);

    /// <summary>Gets the command that exports the statement; it runs only while <see cref="HasStatement"/> is <see langword="true"/>.</summary>
    public Command ExportCommand { get; }

    /// <summary>Gets the number of times the export has run.</summary>
    public int ExportCount
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets a value indicating whether the customer has chosen a statement to export.</summary>
    public bool HasStatement
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                ExportCommand.ChangeCanExecute();
            }
        }
    }
}
