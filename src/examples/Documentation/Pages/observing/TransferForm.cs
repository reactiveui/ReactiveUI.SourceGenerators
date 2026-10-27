// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// The transfer screen: the draft the customer fills in, the source account it draws on and the delivery options.
/// It reports each change before and after it is applied.
/// </summary>
[System.Diagnostics.DebuggerDisplay("TransferForm: {Amount} from {SourceName}")]
public sealed class TransferForm : ChangingObject
{
    /// <summary>Gets or sets the amount to send, in the currency of the source account.</summary>
    public decimal Amount
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the text that appears on the payee's statement.</summary>
    public string Reference
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the number that identifies the source account.</summary>
    public string SourceId
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the name the customer gave the source account.</summary>
    public string SourceName
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the kind of the source account.</summary>
    public AccountKind SourceKind
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the code of the currency the source account holds.</summary>
    public string Currency
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the money in the source account.</summary>
    public decimal Balance
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets how far below zero the source account may go.</summary>
    public decimal OverdraftLimit
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the money the customer can spend: the balance plus the overdraft limit.</summary>
    public decimal AvailableBalance
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the payee the money goes to.</summary>
    public Payee? Payee
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the day the transfer is sent.</summary>
    public DateOnly ScheduledFor
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets a value indicating whether the transfer repeats every month.</summary>
    public bool IsRecurring
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets a value indicating whether the payee receives a notification.</summary>
    public bool NotifyPayee
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets a private note the customer keeps for the transfer.</summary>
    public string Memo
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets what the transfer pays for.</summary>
    public string Purpose
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the fee the bank charges for the transfer.</summary>
    public decimal Fee
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }
}
