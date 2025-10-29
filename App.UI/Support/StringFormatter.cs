using System.Globalization;

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

        /// <summary>
        /// Increases the given integer value by n and returns it as a string.
        /// </summary>
        public static string IncreaseByN(int value, int n) => (value + n).ToString();
    }
}
