using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Storage;
using Windows.Storage.Streams;

namespace JpegViewer.App.Core.Services
{
    public class ImageService : IImageService
    {
        /// <summary>
        /// Lock object to protect our images list.
        /// </summary>
        private readonly object _lockImages = new object();

        /// <summary>
        /// Holds a cancellation token for the async LoadImages function.
        /// </summary>
        private CancellationTokenSource? CancellationToken { get; set; }

        /// <summary>
        /// The task bound to the last image load action started.
        /// </summary>
        private Task? ImageLoadTask { get; set; }

        /// <summary>
        /// Holds the possible image extensions we support.
        /// </summary>
        private string[] ExtensionPatterns { get; } = new[] { "*.jpg", "*.jpeg" };

        /// <summary>
        /// Holds the loaded images.
        /// </summary>
        private List<ImageInfo> Images { get; } = new List<ImageInfo>();

        /// <summary>
        /// An event happen when a new image is loaded.
        /// Clients can register for this event to get notified about new images.
        /// </summary>
        public event EventHandler<ImageInfo>? ImageFound;

        /// <summary>
        /// Happens when image service finishes loading images from the specified folder.
        /// Clients can register for this event to get notified about new image list loaded.
        /// </summary>
        public event EventHandler<ImageInfo>? ImagesLoaded;

        /// <summary>
        /// Creation time of the eraliest picture.
        /// </summary>
        public DateTime? MinDateTaken
        {
            get
            {
                lock (_lockImages)
                {
                    return Images?.MinBy(i => i.DateTaken)?.DateTaken;
                }
            }
        }

        /// <summary>
        /// Creation time of the latest picture.
        /// </summary>
        public DateTime? MaxDateTaken
        {
            get
            {
                lock (_lockImages)
                {
                    return Images?.MaxBy(i => i.DateTaken)?.DateTaken;
                }
            }
        }

        /// <summary>
        /// Returns images belonging to the range specified by start and end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ImageInfo> GetImagesForDateRange(DateTime start, DateTime end)
        {
            lock (_lockImages)
            {
                return Images.Where(i => i.DateTaken >= start && i.DateTaken <= end).ToList();
            }
        }

        /// <summary>
        /// Creates a bitmap from the given ImageInfo.
        /// </summary>
        /// <param name="imgInfo"></param>
        /// <returns></returns>
        public async Task<BitmapImage?> GetBimap(ImageInfo imgInfo)
        {
            try
            {
                StorageFile storageFile = await StorageFile.GetFileFromPathAsync(imgInfo.Path);
                using IRandomAccessStream fileStream = await storageFile.OpenReadAsync();

                // Create a bitmap to be the image source.
                BitmapImage bitmapImage = new();
                bitmapImage.SetSource(fileStream);

                return bitmapImage;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="subFolderRecursion"></param>
        /// <param name="maxRecursionDepth"></param>
        public async void LoadImages(string path, ESubFolderRecursion subFolderRecursion, int maxRecursionDepth = int.MaxValue)
        {
            while (ImageLoadTask != null && CancellationToken != null && !ImageLoadTask.IsCompleted)
            {
                CancellationToken.Cancel();
            }

            CancellationToken?.Dispose();
            CancellationToken = new CancellationTokenSource();
            ImageLoadTask = LoadImages(CancellationToken.Token, path, subFolderRecursion, maxRecursionDepth);
            await ImageLoadTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="path"></param>
        /// <param name="subFolderRecursion"></param>
        /// <param name="maxRecursionDepth"></param>
        /// <returns></returns>
        private async Task LoadImages(CancellationToken token, string path, ESubFolderRecursion subFolderRecursion, int maxRecursionDepth = int.MaxValue)
        {
            if (token.IsCancellationRequested || !System.IO.Directory.Exists(path))
            {
                return;
            }

            lock(_lockImages)
            { 
                Images.Clear();
            }

            try
            {
                string[] patterns = new[] { "*.jpg", "*.jpeg" };
                EnumerationOptions enumOption = new EnumerationOptions()
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MaxRecursionDepth = maxRecursionDepth,
                    RecurseSubdirectories = (subFolderRecursion == ESubFolderRecursion.IncludeSubFolders)
                };

                var files = patterns.SelectMany(pattern => System.IO.Directory.GetFiles(path, pattern, enumOption)).Distinct();
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested || !System.IO.Directory.Exists(path))
                    {
                        return;
                    }

                    try
                    {
                        var directories = ImageMetadataReader.ReadMetadata(file);
                        var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                        if (subIfd == null)
                        {
                            continue;
                        }

                        // Try to get the image creation time with some fallback options
                        DateTime dateTaken;
                        if (!subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTaken))
                        {
                            if (!subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out dateTaken))
                            {
                                if (!subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTime, out dateTaken))
                                {
                                    dateTaken = File.GetCreationTime(path);
                                }
                            }
                        }

                        ImageInfo image = new ImageInfo(file, dateTaken);
                        lock (_lockImages)
                        {
                            int index = Images.BinarySearch(image, Comparer<ImageInfo>.Create((a, b) => a.DateTaken.CompareTo(b.DateTaken)));
                            if (index < 0) index = ~index;
                            Images.Insert(index, image);
                        }
                        OnImageFound(image);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message); // We continue on exception
                    }
                    await Task.Yield();
                }
                OnImagesLoaded(Images.First());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Raise the ImageFound event for the registered members.
        /// </summary>
        /// <param name="image"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnImageFound(ImageInfo image)
        {
            ImageFound?.Invoke(this, image);
        }

        /// <summary>
        /// Raise the ImagesLoaded event for the registered members.
        /// </summary>
        /// <param name="image"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnImagesLoaded(ImageInfo image)
        {
            ImagesLoaded?.Invoke(this, image);
        }
    }
}
