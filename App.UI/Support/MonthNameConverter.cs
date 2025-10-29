using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Converts a DateTime or month integer to the localized month name.
    /// </summary>
    public class MonthNameConverter : IValueConverter
    {
        /// <summary>
        /// Convert a DateTime or month integer to the localized month name.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return string.Empty;
            }

            // Accept DateTime
            if (value is DateTime dt)
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dt.Month);
            }

            // Accept integer month (1-12)
            if (value is int month && month >= 1 && month <= 12)
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            }

            return string.Empty;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return false;
        }
    }
}
