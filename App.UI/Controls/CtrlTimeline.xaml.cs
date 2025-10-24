using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml.Controls;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control for displaying and interacting with a timeline.
    /// </summary>
    public sealed partial class CtrlTimeline : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlTimeline"/> class.
        /// </summary>
        public CtrlTimeline()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the Timeline control
            root.DataContext = App.GetService<VmdCtrlTimeline>();
        }
    }
}
