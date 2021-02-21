// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace BenchmarkLogGenerator.Utilities
{

    public static class ExtendedDateTime
    {
        #region Public constants
        /// <summary>
        /// The min value of a DateTime, in UTC.
        /// </summary>
        public static readonly DateTime MinValueUtc = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc);

        /// <summary>
        /// The max value of a DateTime, in UTC.
        /// </summary>
        public static readonly DateTime MaxValueUtc = new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc);

        /// <summary>
        /// A list of datetime formats which we support but aren't supported by the default IFormatProvider
        /// which we use for DateTime.Parse.
        /// </summary>
        public static readonly Dictionary<int, string[]> SupportedNonStandardFormats = new Dictionary<int, string[]>()
        {
            { 4, new [] { "yyyy" } },
            { 6, new [] { "yyyyMM" } },
            { 8, new [] { "yyyyMMdd" } },
            { 10, new [] { "yyyyMMddHH" } },
            { 12, new [] { "yyyyMMddHHmm" } },
            { 14, new [] { "yyyyMMddHHmmss" } },
            { 17, new [] { "yyyyMMdd HH:mm:ss" } },
            { 19, new [] { "yyyyMMdd HH:mm:ss.f" } },
            { 20, new [] { "yyyyMMdd HH:mm:ss.ff" } },
            { 21, new [] { "yyyyMMdd HH:mm:ss.fff" } },
            { 22, new [] { "yyyyMMdd HH:mm:ss.ffff" } },
            { 23, new [] { "yyyyMMdd HH:mm:ss.fffff" } },
            { 24, new [] { "yyyyMMdd HH:mm:ss.ffffff" } },
            { 25, new [] { "yyyyMMdd HH:mm:ss.fffffff" } },
        };

        /// <summary>
        /// Jan 1 1970 ("epoch")
        /// </summary>
        public static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        #endregion

        #region Constants
        private static readonly int s_numCharactersInIso8601 = MinValueUtc.ToString("O").Length;
        #endregion

        #region fast tostring()
        /// <summary>
        /// This function provides an optimized implementation of <see cref="DateTime.ToString(string)"/>
        /// for the case of the format string being "O" (the round-trip format, a.k.a. ISO8601).
        /// </summary>
        public static string FastToString(this DateTime value)
        {
            var sb = UtilsStringBuilderCache.Acquire();
            FastAppendToStringBuilder(value, sb);
            return UtilsStringBuilderCache.GetStringAndRelease(sb);
        }

        public static void FastAppendToStringBuilder(this DateTime value, System.Text.StringBuilder sb)
        {
            sb.EnsureCapacity(s_numCharactersInIso8601); // TODO: Make sure that this ensures the _remaining_ capacity! Also note that the capacity is different for UTC and LOCAL

            int year, month, day, hour, minute, second;
            long fraction;
            FastGetParts(value, out year, out month, out day, out hour, out minute, out second, out fraction);

            FastAppendFormattedInt4(sb, year);
            sb.Append('-');
            FastAppendFormattedInt2(sb, month);
            sb.Append('-');
            FastAppendFormattedInt2(sb, day);
            sb.Append('T');
            FastAppendFormattedInt2(sb, hour);
            sb.Append(':');
            FastAppendFormattedInt2(sb, minute);
            sb.Append(':');
            FastAppendFormattedInt2(sb, second);
            sb.Append('.');
            FastAppendFormattedInt7(sb, fraction);

            switch (value.Kind)
            {
                case DateTimeKind.Local:
                    TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(value);
                    if (offset >= TimeSpan.Zero)
                    {
                        sb.Append('+');
                    }
                    else
                    {
                        sb.Append('-');
                        offset = offset.Negate();
                    }

                    FastAppendFormattedInt2(sb, offset.Hours);
                    sb.Append(':');
                    FastAppendFormattedInt2(sb, offset.Minutes);
                    break;

                case DateTimeKind.Unspecified:
                    break;

                case DateTimeKind.Utc:
                    sb.Append('Z');
                    break;
            }
        }

        private static void FastAppendFormattedInt7(System.Text.StringBuilder sb, long value)
        {
            char g = (char)('0' + value % 10);
            value = value / 10;
            char f = (char)('0' + value % 10);
            value = value / 10;
            char e = (char)('0' + value % 10);
            value = value / 10;
            char d = (char)('0' + value % 10);
            value = value / 10;
            char c = (char)('0' + value % 10);
            value = value / 10;
            char b = (char)('0' + value % 10);
            value = value / 10;
            char a = (char)('0' + value % 10);
            sb.Append(a);
            sb.Append(b);
            sb.Append(c);
            sb.Append(d);
            sb.Append(e);
            sb.Append(f);
            sb.Append(g);
        }

        private static void FastAppendFormattedInt4(System.Text.StringBuilder sb, int value)
        {
            char d = (char)('0' + value % 10);
            value = value / 10;
            char c = (char)('0' + value % 10);
            value = value / 10;
            char b = (char)('0' + value % 10);
            value = value / 10;
            char a = (char)('0' + value % 10);
            sb.Append(a);
            sb.Append(b);
            sb.Append(c);
            sb.Append(d);
        }

        private static void FastAppendFormattedInt2(System.Text.StringBuilder sb, int value)
        {
            char b = (char)('0' + value % 10);
            value = value / 10;
            char a = (char)('0' + value % 10);
            sb.Append(a);
            sb.Append(b);
        }

        public static void FastGetParts(this DateTime value, out int year, out int month, out int day, out int hour, out int minute, out int second, out long fraction)
        {
            Int64 ticks = value.Ticks;
            // n = number of days since 1/1/0001
            int n = (int)(ticks / TicksPerDay);
            // y400 = number of whole 400-year periods since 1/1/0001
            int y400 = n / DaysPer400Years;
            // n = day number within 400-year period
            n -= y400 * DaysPer400Years;
            // y100 = number of whole 100-year periods within 400-year period
            int y100 = n / DaysPer100Years;
            // Last 100-year period has an extra day, so decrement result if 4
            if (y100 == 4) y100 = 3;
            // n = day number within 100-year period
            n -= y100 * DaysPer100Years;
            // y4 = number of whole 4-year periods within 100-year period
            int y4 = n / DaysPer4Years;
            // n = day number within 4-year period
            n -= y4 * DaysPer4Years;
            // y1 = number of whole years within 4-year period
            int y1 = n / DaysPerYear;
            // Last year has an extra day, so decrement result if 4
            if (y1 == 4) y1 = 3;

            year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;

            // n = day number within year
            n -= y1 * DaysPerYear;

            // If day-of-year was requested, return it
            //if (part == DatePartDayOfYear) return n + 1;

            // Leap year calculation looks different from IsLeapYear since y1, y4,
            // and y100 are relative to year 1, not year 0
            bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
            int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
            // All months have less than 32 days, so n >> 5 is a good conservative
            // estimate for the month
            int m = n >> 5 + 1;
            // m = 1-based month number
            while (n >= days[m]) m++;

            month = m;

            // 1-based day-of-month
            day = n - days[m - 1] + 1;

            hour = value.Hour;

            minute = value.Minute;

            second = value.Second;

            fraction = ticks % TicksPerSecond;
        }
        #endregion

        #region Constants 
        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;          // 584388
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/1969
        internal const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059

        internal const long MinTicks = 0;
        internal const long MaxTicks = DaysTo10000 * TicksPerDay - 1;
        private const long MaxMillis = (long)DaysTo10000 * MillisPerDay;

        private const long FileTimeOffset = DaysTo1601 * TicksPerDay;
        private const long DoubleDateOffset = DaysTo1899 * TicksPerDay;
        // The minimum OA date is 0100/01/01 (Note it's year 100).
        // The maximum OA date is 9999/12/31
        private const long OADateMinAsTicks = (DaysPer100Years - DaysPerYear) * TicksPerDay;
        // All OA dates must be greater than (not >=) OADateMinAsDouble
        private const double OADateMinAsDouble = -657435.0;
        // All OA dates must be less than (not <=) OADateMaxAsDouble
        private const double OADateMaxAsDouble = 2958466.0;

        private const int DatePartYear = 0;
        private const int DatePartDayOfYear = 1;
        private const int DatePartMonth = 2;
        private const int DatePartDay = 3;

        private static readonly int[] DaysToMonth365 = {
            0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
        private static readonly int[] DaysToMonth366 = {
            0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};
        #endregion

    }
   
}