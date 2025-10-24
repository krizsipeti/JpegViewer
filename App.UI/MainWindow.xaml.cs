using JpegViewer.App.Vmd;
using Microsoft.UI.Xaml;

namespace JpegViewer.App.UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the MainWindow
            root.DataContext = App.GetService<VmdMainWindow>();
        }
    }
}
