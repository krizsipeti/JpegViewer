using System;
using System.Globalization;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Helpers for formatting strings.
    /// </summary>
    public static class StringFormatter
    {
        /// <summary>
        /// Gets a year range string based on the start year and range.
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static string YearRange(int startYear, int range) => range > 1 ? $"{startYear} - {(startYear + (range - 1))}" : startYear.ToString();

        /// <summary>
        /// Returns a formatted string with the month name and year.
        /// </summary>
        public static string MonthAndYear(int month, int year) => $"{MonthNameConverter(month)}, {year}";

        /// <summary>
        /// Returns the localized month name for the given month integer (1-12).
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string MonthNameConverter(int month)
        {
            if (month < 1 || month > 12)
            {
                return string.Empty;
            }
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
        }

        public static string TimelineItemHeaderText(ETimelineItemType timelineItemType, DateTime itemKey)
        {
            return timelineItemType switch
            {
                ETimelineItemType.YearsOfDecade => YearRange(itemKey.Year, 10),
                ETimelineItemType.MonthsOfYear => itemKey.Year.ToString(),
                ETimelineItemType.DaysOfMonth => MonthAndYear(itemKey.Month, itemKey.Year),
                _ => string.Empty
            };
        }

        /// <summary>
        /// Returns the unit text.
        /// </summary>
        public static string TimeLineItemUnitText(ETimelineBaseUnitType type, int value)
        {
            return type switch
            {
                ETimelineBaseUnitType.Month => MonthNameConverter(value),
                _ => value.ToString()
            };
        }
    }
}
