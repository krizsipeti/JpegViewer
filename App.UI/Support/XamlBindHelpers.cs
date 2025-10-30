using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JpegViewer.App.Core.Models;
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

        /// <summary>
        /// Converts count to visibility.
        /// </summary>
        public static Visibility CountToVisibility(List<TimelineItemBaseUnit> units, int index)
        {
            if (units == null || units.Count <= index || index < 0)
            {
                return Visibility.Collapsed;
            }
            return (units[index]?.Images?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
