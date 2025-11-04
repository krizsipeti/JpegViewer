using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;
using Microsoft.UI.Xaml.Media.Imaging;

namespace JpegViewer.App.Core.Interfaces
{
    public interface IImageService
    {
        /// <summary>
        /// Event of a single image found.
        /// </summary>
        event EventHandler<ImageInfo>? ImageFound;

        /// <summary>
        /// Happens when image service finishes loading images from the specified folder.
        /// </summary>
        event EventHandler<ImageInfo>? ImagesLoaded;

        /// <summary>
        /// Creation time of the eraliest picture.
        /// </summary>
        DateTime? MinDateTaken { get; }

        /// <summary>
        /// Creation time of the latest picture.
        /// </summary>
        DateTime? MaxDateTaken { get; }

        /// <summary>
        /// Load the images in an asyn method.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="subFolderRecursion"></param>
        /// <param name="maxRecursionDepth"></param>
        /// <returns></returns>
        void LoadImages(string path, ESubFolderRecursion subFolderRecursion, int maxRecursionDepth = int.MaxValue);

        /// <summary>
        /// Returns images belonging to the range specified by start and end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ImageInfo> GetImagesForDateRange(DateTime start, DateTime end);

        /// <summary>
        /// Create a bitmap image from the given ImageInfo.
        /// </summary>
        /// <param name="imgInfo"></param>
        /// <returns></returns>
        Task<BitmapImage?> GetBimap(ImageInfo imgInfo);
    }
}
