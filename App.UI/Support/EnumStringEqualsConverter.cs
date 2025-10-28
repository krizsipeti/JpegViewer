using System;
using Microsoft.UI.Xaml.Data;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Converts an enum value to a boolean by comparing its name to a provided string parameter.
    /// </summary>
    public class EnumStringEqualsConverter : IValueConverter
    {
        /// <summary>
        /// Returns true if the enum value's name matches the parameter string; otherwise, false.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null || value == null || !value.GetType().IsEnum)
            {
                return false;
            }

            Type valueType = value.GetType();
            if (!valueType.IsEnum)
            {
                return false;
            }

            string? paramStr = parameter.ToString();
            string? enumName = Enum.GetName(valueType, value);
            if (string.IsNullOrEmpty(paramStr) || string.IsNullOrEmpty(enumName))
            {
                return false;
            }

            return enumName.Equals(paramStr);
        }

        /// <summary>
        /// Not implemented and always returns false.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return false; // Not implemented since it's not needed
        }
    }
}
