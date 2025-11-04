using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using JpegViewer.App.Core.Interfaces;

namespace JpegViewer.App.Vmd
{
    /// <summary>
    /// View model for main window.
    /// </summary>
    public partial class VmdMainWindow : VmdBase
    {
        private bool _isFolderPickerVisible;
        private string? _currentFolder;

        /// <summary>
        /// Holds the image service.
        /// </summary>
        private IImageService ImageService { get; }

        /// <summary>
        /// Shows or hides the folder picker.
        /// </summary>
        public bool IsFolderPickerVisible
        {
            get => _isFolderPickerVisible;
            set => SetProperty(ref _isFolderPickerVisible, value);
        }

        /// <summary>
        /// The full path of the currently selected folder or null.
        /// </summary>
        public string? CurrentFolder
        {
            get => _currentFolder;
            set
            {
                if (SetProperty(ref _currentFolder, value))
                {
                    Task.Run(() => LoadImages());
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdMainWindow"/> class.
        /// </summary>
        public VmdMainWindow(IDispatcherService dispatcherService, IImageService imageService) : base(dispatcherService)
        {
            ImageService = imageService;
        }

        /// <summary>
        /// Called when the open folder picker button is pressed.
        /// </summary>
        [RelayCommand]
        private void SelectFolder()
        {
            IsFolderPickerVisible = !IsFolderPickerVisible;
        }

        /// <summary>
        /// Try to load the images of the currently set folder.
        /// </summary>
        private void LoadImages()
        {
            if (string.IsNullOrEmpty(CurrentFolder))
            {
                return;
            }
            ImageService.LoadImages(CurrentFolder, Core.Types.ESubFolderRecursion.ExcludeSubFolders);
        }
    }
}
