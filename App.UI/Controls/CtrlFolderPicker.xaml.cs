using System.Collections;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control that provides functionality for selecting a folder.
    /// </summary>
    public sealed partial class CtrlFolderPicker : UserControl
    {
        private SpringVector3NaturalMotionAnimation SpringAnimation { get; }

        #region Dependency Property registrations

        /// <summary>
        /// Dependency property for the list of folders to be displayed in the folder picker.
        /// </summary>
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
            SpringAnimation = ElementCompositionPreview.GetElementVisual(root).Compositor.CreateSpringVector3Animation();
            SpringAnimation.Target = "Scale";
        }

        /// <summary>
        /// Called when the pointer enters a TreeViewItem, triggering a spring animation to scale up the item.
        /// </summary>
        private void TreeViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SpringAnimation.FinalValue = new System.Numerics.Vector3(1.05f);
            (sender as UIElement)!.StartAnimation(SpringAnimation);
        }

        /// <summary>
        /// Called when the pointer exits a TreeViewItem, triggering a spring animation to scale down the item.
        /// </summary>
        private void TreeViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SpringAnimation.FinalValue = new System.Numerics.Vector3(1.0f);
            (sender as UIElement)!.StartAnimation(SpringAnimation);
        }
    }
}
