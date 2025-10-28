using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Increases the given integer value by n and returns it as a string.
        /// </summary>
        public static string IncreaseByN(int value, int n) => (value + n).ToString();
    }
}
