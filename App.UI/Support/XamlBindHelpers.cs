using System;
using Microsoft.UI.Xaml;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Implementation of helpers for XAML binding.
    /// </summary>
    public static class XamlBindHelpers
    {
        /// <summary>
        /// Hides days that are not valid for the given month and year.
        /// </summary>
        public static Visibility IsDayVisible(int year, int month, int day)
        {
            return DateTime.DaysInMonth(year, month) < day ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Removes the column width for days that are not valid for the given month and year.
        /// </summary>
        public static GridLength AutoOrStarForGridDayColumnWidth(int year, int month, int day)
        {
            return DateTime.DaysInMonth(year, month) < day ? new GridLength(0, GridUnitType.Auto) : new GridLength(1, GridUnitType.Star);
        }
    }
}
