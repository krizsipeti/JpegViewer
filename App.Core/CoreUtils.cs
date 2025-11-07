using System;

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
    }
}
