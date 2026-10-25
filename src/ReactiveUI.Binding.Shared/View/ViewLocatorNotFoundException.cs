// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Exception thrown when a view locator is not registered with the dependency resolver.</summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class ViewLocatorNotFoundException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="ViewLocatorNotFoundException"/> class with a message naming the builder calls that register the default locator.</summary>
    public ViewLocatorNotFoundException()
        : base(
            "No IViewLocator is registered. Call RxBindingBuilder.CreateReactiveUIBindingBuilder().WithCoreServices().BuildApp() to register default services.")
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ViewLocatorNotFoundException"/> class with a specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public ViewLocatorNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ViewLocatorNotFoundException"/> class with a specified error message and a reference to the inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ViewLocatorNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
