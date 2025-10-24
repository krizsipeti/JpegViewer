using System.Collections;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control that provides functionality for selecting a folder.
    /// </summary>
    public sealed partial class CtrlFolderPicker : UserControl
    {
        #region Dependency Property registrations

        public static readonly DependencyProperty FolderListProperty = DependencyProperty.Register(nameof(Folders), typeof(IEnumerable), typeof(CtrlFolderPicker), null);

        #endregion Dependency Property registrations

        #region Properties

        /// <summary>
        /// Holds the list of folders to be displayed in the folder picker.
        /// </summary>
        public IEnumerable Folders
        {
            get => (IEnumerable)GetValue(FolderListProperty);
            set => SetValue(FolderListProperty, value);
        }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlFolderPicker"/> class.
        /// </summary>
        public CtrlFolderPicker()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the FolderPicker control
            root.DataContext = App.GetService<VmdCtrlFolderPicker>();
        }
    }
}
