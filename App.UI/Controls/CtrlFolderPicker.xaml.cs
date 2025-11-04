using JpegViewer.App.UI.Support;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control that provides functionality for selecting a folder.
    /// </summary>
    public sealed partial class CtrlFolderPicker : UserControl
    {
        #region Fields and private properties

        /// <summary>
        /// Holds an animation helper class used for control buttons.
        /// </summary>
        private AnimationHelper Animation { get; } = new AnimationHelper();

        #endregion Fields and private properties

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlFolderPicker"/> class.
        /// </summary>
        public CtrlFolderPicker()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the FolderPicker control
            root.DataContext = App.GetService<VmdCtrlFolderPicker>();
        }

        /// <summary>
        /// Called when the pointer enters a TreeViewItem, triggering a spring animation to scale up the item.
        /// </summary>
        private void TreeViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerEntered(sender, ((sender as TreeViewItem)!.Content as Grid)!.Children[1], typeof(TextBlock));
            e.Handled = true;
        }

        /// <summary>
        /// Called when the pointer exits a TreeViewItem, triggering a spring animation to scale down the item.
        /// </summary>
        private void TreeViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerExited(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the pointer enters a button, triggering a spring animation to scale up.
        /// </summary>
        private void FontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerEntered(sender, typeof(FontIcon));
            e.Handled = true;
        }

        /// <summary>
        ///  Called when the pointer exits a button, triggering a spring animation to scale down.
        /// </summary>
        private void FontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerExited(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Scale animation of button pressed state.
        /// </summary>
        private void FontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerPressed(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Scale animation of button released state.
        /// </summary>
        private void FontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerReleased(sender);
            e.Handled = true;
        }
    }
}
