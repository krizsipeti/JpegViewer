using CommunityToolkit.Mvvm.Messaging;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;
using Microsoft.UI.Xaml.Media.Imaging;

namespace JpegViewer.App.Vmd.Controls
{
    /// <summary>
    /// View model for photo control.
    /// </summary>
    public class VmdCtrlPhoto : VmdBase, IRecipient<CurrentImageChanged>
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
            set
            {
                if (SetProperty(ref _currentImage, value))
                {
                    if (_currentImage != null)
                    {
                        DispatcherService.InvokeAsync(async () => CurrentBitmap = await ImageService.GetBimap(_currentImage));
                    }
                }
            }
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

            // Register this instance to receive messages
            WeakReferenceMessenger.Default.Register<CurrentImageChanged>(this);
        }

        /// <summary>
        /// Message handler for events sent by timeline when it points to a new image.
        /// </summary>
        /// <param name="message"></param>
        public void Receive(CurrentImageChanged message)
        {
            CurrentImageInfo = message.ImageInfo;
        }
    }
}
