using System;
using System.Drawing;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control for displaying and interacting with a timeline.
    /// </summary>
    public sealed partial class CtrlTimeline : UserControl
    {
        /// <summary>
        /// Scale transform for zooming the timeline content.
        /// </summary>
        private ScaleTransform ScaleTransform { get; } = new ScaleTransform();

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlTimeline"/> class.
        /// </summary>
        public CtrlTimeline()
        {
            InitializeComponent();

            // Set the DataContext to the viewmodel of the Timeline control
            DataContext = App.GetService<VmdCtrlTimeline>();
        }

        /// <summary>
        /// Handle pointer wheel changes on the ItemsRepeater to allow horizontal scrolling.
        /// </summary>
        private void ItemsRepeater_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            //ScrollViewer? scrollViewer = (sender as ItemsRepeater)?.Parent as ScrollViewer;
            //if (scrollViewer == null)
            //{
            //    return;
            //}

            int delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta; // ±120 per notch

            if (e.KeyModifiers == VirtualKeyModifiers.Control)
            {
                //scrollViewer.RenderTransform = ScaleTransform;
                //
                //// Zooming behavior could be implemented here if needed
                //// Zoom: prefer a ScaleTransform on content for smooth, layout-free zooming
                double zoomStep = 100; // adjust sensitivity
                double change = (delta > 0) ? zoomStep : -zoomStep;
                //
                //// Example using a ScaleTransform named "contentScale"
                //double newScale = Math.Clamp(ScaleTransform.ScaleX + change, 0.5, 3.0);
                //ScaleTransform.ScaleX = newScale;
                ////ScaleTransform.ScaleY = newScale;
                //
                //// If using ScrollViewer's zoom (if supported), use ChangeView with zoomFactor:
                //// scrollViewer.ChangeView(null, null, (float)newZoomFactor, disableAnimation: false);

                ((VmdCtrlTimeline)DataContext)!.ItemsWidth += change;

                //// viewport rectangle in ScrollViewer coordinates
                //var viewport = new Rectangle(0, 0, (int)scrollViewer.ViewportWidth, (int)scrollViewer.ViewportHeight);
                //
                //int count = itemsRepeater.ItemsSourceView?.Count ?? 0;
                //for (int i = 0; i < count; i++)
                //{
                //    if (!(itemsRepeater.TryGetElement(i) is FrameworkElement element))
                //        continue;
                //
                //    // element bounds relative to ScrollViewer
                //    GeneralTransform gt = element.TransformToVisual(scrollViewer);
                //    Rect elemRect = gt.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                //    Rectangle rc = new Rectangle((int)elemRect.X, (int)elemRect.Y, (int)elemRect.Width, (int)elemRect.Height);
                //
                //    // check intersection (visible on screen)
                //    if (!rc.IntersectsWith(viewport))
                //        continue;
                //
                //    // animate Width
                //    //element.CancelAnimations();
                //    var da = new DoubleAnimation
                //    {
                //        To = ((VmdCtrlTimeline)DataContext)!.ItemsWidth + 100,
                //        Duration = TimeSpan.FromMilliseconds(500),
                //        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
                //    };
                //    var sb = new Storyboard();
                //    sb.Children.Add(da);
                //    Storyboard.SetTarget(da, element);
                //    Storyboard.SetTargetProperty(da, "Width");
                //    sb.Begin();
                //    break;
                //}
            }
            else
            {
                // Convert vertical wheel delta to horizontal offset change
                // Adjust sensitivity as needed
                double scrollAmount = -delta * 0.5; // negative to follow normal wheel direction
                scrollViewer.ChangeView(scrollViewer.HorizontalOffset + scrollAmount, null, null, disableAnimation: false);
            }

            e.Handled = true;
        }
    }
}
