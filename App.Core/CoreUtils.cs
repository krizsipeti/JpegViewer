using System;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core
{
    /// <summary>
    /// Contains some helpful static functions.
    /// </summary>
    public static class CoreUtils
    {
        /// <summary>
        /// Compare two DateTime by year.
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public static bool HasSameYear(in DateTime dateA, in DateTime dateB)
        {
            return dateA.Year == dateB.Year;
        }

        /// <summary>
        /// Compare two DateTime by year and month.
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public static bool HasSameYearMonth(in DateTime dateA, in DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month;
        }

        /// <summary>
        /// Compare two DateTime by year, month and day.
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public static bool HasSameYearMonthDay(in DateTime dateA, in DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month && dateA.Day == dateB.Day;
        }

        /// <summary>
        /// Compare two DateTime by year, month, day and hour.
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public static bool HasSameYearMonthDayHour(in DateTime dateA, in DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month && dateA.Day == dateB.Day && dateA.Hour == dateB.Hour;
        }

        /// <summary>
        /// Compare two DateTime by year, month, day, hour and minute.
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public static bool HasSameYearMonthDayHourMinute(in DateTime dateA, in DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month && dateA.Day == dateB.Day &&
                dateA.Hour == dateB.Hour && dateA.Minute == dateB.Minute;
        }

        /// <summary>
        /// Compare two DateTime by year, month, day, hour, minute and second.
        /// </summary>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        public static bool HasSameYearMonthDayHourMinuteSecond(in DateTime dateA, in DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month && dateA.Day == dateB.Day &&
                dateA.Hour == dateB.Hour && dateA.Minute == dateB.Minute && dateA.Second == dateB.Second;
        }
        

        /// <summary>
        /// Returns the duration of the given TimelineItemBaseUnit in microseconds.
        /// </summary>
        /// <param name="parentYear">used when base unit is month type</param>
        /// <param name="baseUnit"></param>
        /// <returns></returns>
        public static double DurationInMicros(int parentYear, TimelineItemBaseUnit baseUnit)
        {
            return baseUnit.Type switch
            {
                ETimelineBaseUnitType.Year => TimeSpan.FromDays(DateTime.IsLeapYear(baseUnit.Value) ? 366 : 355).TotalMicroseconds,
                ETimelineBaseUnitType.Month => TimeSpan.FromDays(DateTime.DaysInMonth(parentYear, baseUnit.Value)).TotalMicroseconds,
                ETimelineBaseUnitType.Day => TimeSpan.FromDays(1).TotalMicroseconds,
                ETimelineBaseUnitType.Hour => TimeSpan.FromHours(1).TotalMicroseconds,
                ETimelineBaseUnitType.Minute => TimeSpan.FromMinutes(1).TotalMicroseconds,
                ETimelineBaseUnitType.Second => TimeSpan.FromSeconds(1).TotalMicroseconds,
                _ => 0
            };
        }

        /// <summary>
        /// Returns the current time defined by the timeline item, base unit and offset.
        /// </summary>
        /// <param name="parentUnit"></param>
        /// <param name="baseUnit"></param>
        /// <param name="offsetInMicros"></param>
        /// <returns></returns>
        public static DateTime CurrentTime(TimelineItem parentUnit, TimelineItemBaseUnit baseUnit, double offsetInMicros)
        {
            return baseUnit.Type switch
            {
                ETimelineBaseUnitType.Year => new DateTime(baseUnit.Value, 1, 1).AddMicroseconds(offsetInMicros),
                ETimelineBaseUnitType.Month => parentUnit.ItemKey.AddMonths(baseUnit.Value - 1).AddMicroseconds(offsetInMicros),
                ETimelineBaseUnitType.Day => parentUnit.ItemKey.AddDays(baseUnit.Value - 1).AddMicroseconds(offsetInMicros),
                ETimelineBaseUnitType.Hour => parentUnit.ItemKey.AddHours(baseUnit.Value).AddMicroseconds(offsetInMicros),
                ETimelineBaseUnitType.Minute => parentUnit.ItemKey.AddMinutes(baseUnit.Value).AddMicroseconds(offsetInMicros),
                ETimelineBaseUnitType.Second => parentUnit.ItemKey.AddSeconds(baseUnit.Value).AddMicroseconds(offsetInMicros),
                _ => parentUnit.ItemKey
            };
        }
    }
}
