using System;
using System.Collections.Generic;
using Oberyn.FishingBuddy.Models;

// Tyria's clock is 70/5/40/5 mins split, Cantha/Castora is 55/5/55/5 mins

namespace Oberyn.FishingBuddy.Services
{
    // pull from DateTime.UtcNow. Tyrian time is 12x faster, values below are in seconds (in case you forget)
    public static class TyrianClock
    {
        private static readonly TimeSpan ReferenceUtcTimeOfDay = new TimeSpan(16, 30, 0);
        private const int TyrianSecondsAtReference = 6 * 3600;
        private const int CycleLengthRealSeconds = 7200;
        private const int TyrianSecondsPerRealSecond = 12;
        private const int SecondsPerDay = 86400;

        // logic: Tyrian-second-of-day the time of the day starts, time of the day tag.
        private static readonly (int Start, TimeOfDay State)[] TyriaSegments =
        {
            (18000, TimeOfDay.Dawn),
            (21600, TimeOfDay.Day),
            (72000, TimeOfDay.Dusk),
            (75600, TimeOfDay.Night),
        };

        private static readonly (int Start, TimeOfDay State)[] CanthaCastoraSegments =
        {
            (25200, TimeOfDay.Dawn),
            (28800, TimeOfDay.Day),
            (68400, TimeOfDay.Dusk),
            (72000, TimeOfDay.Night),
        };

        // Tyrian seconds since Tyrian 00:00 (0-86399).
        public static int GetCurrentTyrianSecondOfDay(DateTime utcNow)
        {
            double elapsedRealSeconds = (utcNow.TimeOfDay - ReferenceUtcTimeOfDay).TotalSeconds;

            // normalize into [0, CycleLengthRealSeconds) - works for both before and after the reference point.
            elapsedRealSeconds = ((elapsedRealSeconds % CycleLengthRealSeconds) + CycleLengthRealSeconds)
                                  % CycleLengthRealSeconds;

            int tyrianSeconds = (int)(elapsedRealSeconds * TyrianSecondsPerRealSecond) + TyrianSecondsAtReference;
            return tyrianSeconds % SecondsPerDay;
        }

        // current state + time remaining until the next change. Don't call with Cycle.Global - check fish.Cycle first.
        public static (TimeOfDay State, TimeSpan TimeRemaining) GetState(Cycle cycle, DateTime? utcNow = null)
        {
            if (cycle == Cycle.Global)
                throw new ArgumentException("Global fish aren't tied to a day/night cycle.", nameof(cycle));

            var now = utcNow ?? DateTime.UtcNow;
            int tyrianSecondOfDay = GetCurrentTyrianSecondOfDay(now);

            var segments = SegmentsFor(cycle);
            int index = FindSegmentIndex(tyrianSecondOfDay, segments);
            int elapsed = tyrianSecondOfDay - EffectiveStart(segments, index, tyrianSecondOfDay);
            int remaining = Duration(segments, index) - elapsed;

            return (segments[index].State, ToRealTimeSpan(remaining));
        }

        // current state plus the next count states, each with real time until it starts.
        // index 0 = current state. Feeds the "Dusk in H:MM:SS" / "Night in H:MM:SS" notes in the displayed module.
        public static List<(TimeOfDay State, TimeSpan TimeUntilStart)> GetUpcomingStates(
            Cycle cycle, int count, DateTime? utcNow = null)
        {
            if (cycle == Cycle.Global)
                throw new ArgumentException("Global fish aren't tied to a day/night cycle.", nameof(cycle));

            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Must request at least the current state.");

            var now = utcNow ?? DateTime.UtcNow;
            int tyrianSecondOfDay = GetCurrentTyrianSecondOfDay(now);

            var segments = SegmentsFor(cycle);
            int index = FindSegmentIndex(tyrianSecondOfDay, segments);
            int elapsed = tyrianSecondOfDay - EffectiveStart(segments, index, tyrianSecondOfDay);

            var results = new List<(TimeOfDay State, TimeSpan TimeUntilStart)>(count);

            int cumulativeTyrianSeconds = Duration(segments, index) - elapsed;
            results.Add((segments[index].State, ToRealTimeSpan(cumulativeTyrianSeconds)));

            for (int step = 1; step < count; step++)
            {
                index = (index + 1) % segments.Length;
                results.Add((segments[index].State, ToRealTimeSpan(cumulativeTyrianSeconds)));
                cumulativeTyrianSeconds += Duration(segments, index);
            }

            return results;
        }

        private static (int Start, TimeOfDay State)[] SegmentsFor(Cycle cycle) =>
            cycle == Cycle.CanthaCastora ? CanthaCastoraSegments : TyriaSegments;

        private static int FindSegmentIndex(int secondsOfDay, (int Start, TimeOfDay State)[] segments)
        {
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                if (secondsOfDay >= segments[i].Start)
                    return i;
            }

            // midnight cheating below:
            // before segments[0].Start = still in Night, wrapped from before midnight.
            return segments.Length - 1;
        }

        // preventing negative values for elapsed times (midnight wrap bypass))
        private static int EffectiveStart(
            (int Start, TimeOfDay State)[] segments, int index, int secondsOfDay)
        {
            int start = segments[index].Start;
            return start > secondsOfDay ? start - SecondsPerDay : start;
        }

        // full segment length in Tyrian seconds, wrapping Night around to segment 0's start + a day.
        private static int Duration((int Start, TimeOfDay State)[] segments, int index)
        {
            int nextIndex = (index + 1) % segments.Length;
            int nextStart = segments[nextIndex].Start;
            if (nextIndex == 0)
                nextStart += SecondsPerDay;

            return nextStart - segments[index].Start;
        }

        private static TimeSpan ToRealTimeSpan(int tyrianSeconds) =>
            TimeSpan.FromSeconds(tyrianSeconds / (double)TyrianSecondsPerRealSecond);
    }
}