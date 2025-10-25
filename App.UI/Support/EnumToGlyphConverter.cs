using System;
using JpegViewer.App.Core.Types;
using Microsoft.UI.Xaml.Data;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Converts an enum value to a glyph string for UI representation.
    /// </summary>
    public class EnumToGlyphConverter : IValueConverter
    {
        /// <summary>
        /// Enumeration to glyph string conversion.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is EFolderPickerItemIcon icon)
            {
                return char.ConvertFromUtf32((int)icon);
            }
            return char.ConvertFromUtf32(0xf142); // Default is a question mark icon
        }

        /// <summary>
        /// Glyph string to enumeration conversion is not supported.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return 0xf142; // Default is a question mark icon
        }
    }
}
