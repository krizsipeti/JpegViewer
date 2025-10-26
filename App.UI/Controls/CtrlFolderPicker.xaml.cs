using System;
using System.Collections;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control that provides functionality for selecting a folder.
    /// </summary>
    public sealed partial class CtrlFolderPicker : UserControl
    {
        #region Fields and private properties

        /// <summary>
        /// Holds the spring animation used for scaling folder items on hover.
        /// </summary>
        private SpringVector3NaturalMotionAnimation SpringAnimation { get; }

        /// <summary>
        /// Holds the storyboard for folder hover color animation.
        /// </summary>
        private Storyboard Storyboard { get; }

        /// <summary>
        /// Holds the color animation for folder hover effect.
        /// </summary>
        private ColorAnimation FolderHoverColorAnimation { get; }

        #endregion Fields and private properties

        #region Dependency Property registrations

        /// <summary>
        /// Dependency property for the list of folders to be displayed in the folder picker.
        /// </summary>
        public static readonly DependencyProperty FolderListProperty = DependencyProperty.Register(nameof(Folders), typeof(IEnumerable), typeof(CtrlFolderPicker), null);

        #endregion Dependency Property registrations

        #region Public properties

        /// <summary>
        /// Holds the list of folders to be displayed in the folder picker.
        /// </summary>
        public IEnumerable Folders
        {
            get => (IEnumerable)GetValue(FolderListProperty);
            set => SetValue(FolderListProperty, value);
        }

        #endregion Public properties

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlFolderPicker"/> class.
        /// </summary>
        public CtrlFolderPicker()
        {
            InitializeComponent();

            // Set the root element's DataContext to the viewmodel of the FolderPicker control
            root.DataContext = App.GetService<VmdCtrlFolderPicker>();

            // Folder hover scale spring animation setup
            SpringAnimation = ElementCompositionPreview.GetElementVisual(root).Compositor.CreateSpringVector3Animation();
            SpringAnimation.DampingRatio = 0.2f;
            SpringAnimation.Target = "Scale";

            // Folder hover color animation setup
            Storyboard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            FolderHoverColorAnimation = new ColorAnimation() { To = Colors.Red, Duration = new Duration(TimeSpan.FromMilliseconds(450)) };
            Storyboard.Children.Add(FolderHoverColorAnimation);
        }

        /// <summary>
        /// Called when the pointer enters a TreeViewItem, triggering a spring animation to scale up the item.
        /// </summary>
        private void TreeViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Do Scale animation
            SpringAnimation.FinalValue = new System.Numerics.Vector3(1.05f);
            (sender as UIElement)!.StartAnimation(SpringAnimation);

            // Do Color animation
            Storyboard.Stop();
            Storyboard.SetTarget(FolderHoverColorAnimation, ((sender as TreeViewItem)!.Content as Grid)!.Children[1]);
            Storyboard.SetTargetProperty(FolderHoverColorAnimation, "(TextBlock.Foreground).(SolidColorBrush.Color)");
            Storyboard.Begin();
        }

        /// <summary>
        /// Called when the pointer exits a TreeViewItem, triggering a spring animation to scale down the item.
        /// </summary>
        private void TreeViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Stop Color animation
            Storyboard.Stop();

            // Do Scale animation back to normal
            SpringAnimation.FinalValue = new System.Numerics.Vector3(1.0f);
            (sender as UIElement)!.StartAnimation(SpringAnimation);
        }
    }
}
