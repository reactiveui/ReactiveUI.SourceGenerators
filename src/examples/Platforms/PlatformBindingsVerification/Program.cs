// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if PLATFORM_WINDOWS
using System.Runtime.ExceptionServices;
using PlatformBindingsVerification.WinForms;
using PlatformBindingsVerification.Wpf;
#elif PLATFORM_MAUI
using PlatformBindingsVerification.Maui;
#endif

if (args.Length == 0 || args[0] != "--verify")
{
    Console.WriteLine("Pass --verify to run this platform's headless self-check.");
    return 1;
}

var failures = 0;

#if PLATFORM_WINDOWS
failures += Run("WPF: ToProperty helper fed by a background thread, delivered to a TextBlock through the dispatcher invoker", static () => OnStaThread(WpfVerification.Verify));

failures += Run("WPF: BindCommand on a plain view writes a command replaced on a background thread through the button's dispatcher", static () => OnStaThread(WpfCommandVerification.Verify));

failures += Run("WinForms: ToProperty helper fed by a background thread, delivered to a Label through the control invoker", static () => OnStaThread(WinFormsVerification.Verify));
#elif PLATFORM_MAUI
failures += Run("MAUI: ToProperty helper fed by a background thread, delivered to a Label inline (no application dispatcher)", MauiVerification.Verify);
#endif

Console.WriteLine();

Console.WriteLine(failures == 0 ? "ALL SCENARIOS PASSED" : $"{failures} SCENARIO(S) FAILED");

return failures == 0 ? 0 : 1;

#if PLATFORM_WINDOWS
// WPF and WinForms controls need a single-threaded apartment, which a top-level program's main thread is not.
static bool OnStaThread(Func<bool> scenario)
{
    var passed = false;
    ExceptionDispatchInfo? failure = null;
    Thread thread = new(() =>
    {
        try
        {
            passed = scenario();
        }
        catch (Exception ex)
        {
            failure = ExceptionDispatchInfo.Capture(ex);
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();
    failure?.Throw();
    return passed;
}
#endif

static int Run(string name, Func<bool> scenario)
{
    try
    {
        if (scenario())
        {
            Console.WriteLine($"PASS: {name}");
            return 0;
        }

        Console.WriteLine($"FAIL: {name}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAIL: {name}");
        Console.WriteLine($"      {ex}");
        return 1;
    }
}
