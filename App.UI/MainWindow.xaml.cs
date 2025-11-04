using JpegViewer.App.UI.Support;
using JpegViewer.App.Vmd;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace JpegViewer.App.UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Holds an animation helper class used for control buttons.
        /// </summary>
        private AnimationHelper Animation { get; } = new AnimationHelper();

        /// <summary>
        /// Set the view model and initialize things.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the MainWindow
            root.DataContext = App.GetService<VmdMainWindow>();
        }

        /// <summary>
        /// Called when the pointer enters a FontIcon, triggering a spring animation to scale up the item.
        /// </summary>
        private void StackPanel_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerEntered(sender, (sender as StackPanel)!.Children[0], typeof(FontIcon));
            e.Handled = true;
        }

        /// <summary>
        /// Called when the pointer exits a FontIcon, triggering a spring animation to scale down the item.
        /// </summary>
        private void StackPanel_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerExited(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Scale animation of button pressed state.
        /// </summary>
        private void StackPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerPressed(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Scale animation of button released state.
        /// </summary>
        private void StackPanel_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerReleased(sender);
            if ((sender as StackPanel) == MenuButton)
            {
                FlyoutBase.ShowAttachedFlyout(MenuButton);
            }
            e.Handled = true;
        }
    }
}
