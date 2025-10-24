using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml.Controls;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// User control for displaying a photo.
    /// </summary>
    public sealed partial class CtrlPhoto : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlPhoto"/> class.
        /// </summary>
        public CtrlPhoto()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the Photo control
            root.DataContext = App.GetService<VmdCtrlPhoto>();
        }
    }
}
