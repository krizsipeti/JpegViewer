using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;

namespace JpegViewer.App.Vmd.Controls
{
    /// <summary>
    /// View model for photo control.
    /// </summary>
    public class VmdCtrlPhoto : VmdBase
    {
        private BitmapImage? _currentBitmap;
        private ImageInfo? _currentImage;

        /// <summary>
        /// Holds a reference to our image service.
        /// </summary>
        public IImageService ImageService { get; }

        /// <summary>
        /// Holds the currently shown image or null.
        /// </summary>
        public ImageInfo? CurrentImageInfo
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        /// <summary>
        /// Holds a reference to the currently shown bitmap image or null.
        /// </summary>
        public BitmapImage? CurrentBitmap
        {
            get => _currentBitmap;
            set => SetProperty(ref _currentBitmap, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdCtrlPhoto"/> class.
        /// </summary>
        public VmdCtrlPhoto(IDispatcherService dispatcherService, IImageService imageService) : base(dispatcherService)
        {
            ImageService = imageService;
            //ImageService.ImageFound += ImageService_ImageFound;
            ImageService.ImagesLoaded += ImageService_ImagesLoaded;
        }

        /// <summary>
        /// Called when image service loads an image.
        /// </summary>
        /// <param name="image"></param>
        private void ImageService_ImageFound(object? sender, ImageInfo image)
        {
            //CurrentImageInfo = image;
        }

        /// <summary>
        /// Called when image service finished loading images of the selected folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">earliest image of the folder</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private async void ImageService_ImagesLoaded(object? sender, ImageInfo e)
        {
            await DispatcherService.InvokeAsync(async () => CurrentBitmap = await ImageService.GetBimap(e));
        }
    }
}
