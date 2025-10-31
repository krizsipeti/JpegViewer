using System.Collections.Generic;
using System.Linq;
using JpegViewer.App.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Implementation of helpers for XAML binding.
    /// </summary>
    public static class XamlBindHelpers
    {
        /// <summary>
        /// Property to help alternation state.
        /// </summary>
        private static bool AlternationHelper { get; set; } = false;

        public static Brush ColorBrushA => (Brush)Application.Current.Resources["colorA"];
        public static Brush ColorBrushB => (Brush)Application.Current.Resources["colorB"];

        /// <summary>
        /// Converts count to visibility.
        /// </summary>
        public static Visibility CountToVisibility(IEnumerable<ImageInfo> images)
        {
            if (images == null || !images.Any())
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        /// <summary>
        /// Returns colorA or colorB based in inner alternation state.
        /// </summary>
        public static Brush GetAlternationColor(Brush colorA, Brush colorB)
        {
            AlternationHelper = !AlternationHelper;
            return AlternationHelper ? colorA : colorB;
        }
    }
}
