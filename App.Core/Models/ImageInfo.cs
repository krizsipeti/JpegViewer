using System;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Holds information about an image.
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// Full path to the image file.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Date and time when the image was taken.
        /// </summary>
        public DateTime DateTaken { get; }

        /// <summary>
        /// Full path to the image file and the date it was taken.
        /// </summary>
        public ImageInfo(string path, DateTime dateTaken)
        {
            Path = path;
            DateTaken = dateTaken;
        }
    }
}
