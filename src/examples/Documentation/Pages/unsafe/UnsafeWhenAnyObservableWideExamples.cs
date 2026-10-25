// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>WhenAnyObservableUnsafe</c> over three to twelve properties. Calling an <c>Unsafe</c> method always reads the property paths at run time.</summary>
public static class UnsafeWhenAnyObservableWideExamples
{
    /// <summary>The announcement an example emits.</summary>
    private const string SyncComplete = "Sync complete";

    /// <summary>The urgent announcement an example emits.</summary>
    private const string DiskFull = "Disk full";

    /// <summary>The text placed between two announcements.</summary>
    private const string Separator = " / ";

    /// <summary>Merges the announcement streams of three properties.</summary>
    public static void MergeThreeAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(x => x.Latest, x => x.Urgent, x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of four properties.</summary>
    public static void MergeFourAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of five properties.</summary>
    public static void MergeFiveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of six properties.</summary>
    public static void MergeSixAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of seven properties.</summary>
    public static void MergeSevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of eight properties.</summary>
    public static void MergeEightAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of nine properties.</summary>
    public static void MergeNineAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of ten properties.</summary>
    public static void MergeTenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of eleven properties.</summary>
    public static void MergeElevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of twelve properties.</summary>
    public static void MergeTwelveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent).Subscribe(Console.WriteLine))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete
    }

    /// <summary>Combines the latest announcement of three properties with a selector.</summary>
    public static void CombineThreeAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(x => x.Latest, x => x.Urgent, x => x.Urgent, static (c1, c2, c3) => string.Join(Separator, c1, c2, c3)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of four properties with a selector.</summary>
    public static void CombineFourAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4) =>
            string.Join(Separator, c1, c2, c3, c4)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of five properties with a selector.</summary>
    public static void CombineFiveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5) =>
            string.Join(Separator, c1, c2, c3, c4, c5)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of six properties with a selector.</summary>
    public static void CombineSixAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of seven properties with a selector.</summary>
    public static void CombineSevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6, c7) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of eight properties with a selector.</summary>
    public static void CombineEightAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of nine properties with a selector.</summary>
    public static void CombineNineAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of ten properties with a selector.</summary>
    public static void CombineTenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of eleven properties with a selector.</summary>
    public static void CombineElevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full
    }

    /// <summary>Combines the latest announcement of twelve properties with a selector.</summary>
    public static void CombineTwelveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();

        using (announcer.WhenAnyObservableUnsafe(
            x => x.Latest,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            x => x.Urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12)).Subscribe(Console.WriteLine))
        {
            announcer.Urgent = Signal.Return(DiskFull);
            announcer.Latest = Signal.Return(SyncComplete);
        }

        // Output:
        // Sync complete / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full / Disk full
    }
}
