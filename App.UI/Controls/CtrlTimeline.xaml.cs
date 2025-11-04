using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;
using JpegViewer.App.UI.Support;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.System;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control for displaying and interacting with a timeline.
    /// </summary>
    public sealed partial class CtrlTimeline : UserControl, IRecipient<TimelineCurrentPositionChange>
    {
        /// <summary>
        /// Holds an animation helper class used for control buttons.
        /// </summary>
        private AnimationHelper Animation { get; } = new AnimationHelper();

        /// <summary>
        /// True if dragging is in action, false otherwise.
        /// </summary>
        private bool IsDragging { get; set; } = false;

        /// <summary>
        /// Holds the start offset for dragging.
        /// </summary>
        private double StartOffset { get; set; }

        /// <summary>
        /// Holds the start X coordinate of dragging action.
        /// </summary>
        private double StartPointerX { get; set; }

        /// <summary>
        /// Holds a previous time point where dragging happened.
        /// </summary>
        private DateTime LastTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlTimeline"/> class.
        /// </summary>
        public CtrlTimeline()
        {
            InitializeComponent();

            // Set the DataContext to the viewmodel of the Timeline control
            DataContext = App.GetService<VmdCtrlTimeline>();

            // Register this instance to receive messages
            WeakReferenceMessenger.Default.Register<TimelineCurrentPositionChange>(this);
        }

        /// <summary>
        /// Handle pointer wheel changes on the ItemsRepeater to allow horizontal scrolling.
        /// </summary>
        private void ItemsRepeater_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            // First get delta value from mouse wheel
            int delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta; // ±120 per notch

            // Zoom in/out if Ctrl is pressed, otherwise scroll horizontally
            if (e.KeyModifiers == VirtualKeyModifiers.Control)
            {
                VmdCtrlTimeline vmdCtrlTimeline = (VmdCtrlTimeline)DataContext;
                if (vmdCtrlTimeline == null)
                {
                    return;
                }

                double zoomStep = vmdCtrlTimeline.ZoomStep; // adjust sensitivity
                double change = (delta > 0) ? zoomStep : -zoomStep;
                if (vmdCtrlTimeline.ItemsWidth + change > vmdCtrlTimeline.MaxItemsWidth)
                {
                    if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Years)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MinItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Months;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Months)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MinItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Days;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Days)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MinItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Hours;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Hours)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MinItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Minutes;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Minutes)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MinItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Seconds;
                    }
                }
                else if (vmdCtrlTimeline.ItemsWidth + change < vmdCtrlTimeline.MinItemsWidth)
                {
                    if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Months)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MaxItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Years;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Days)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MaxItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Months;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Hours)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MaxItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Days;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Minutes)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MaxItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Hours;
                    }
                    else if (vmdCtrlTimeline.ZoomLevel == ETimelineZoomLevel.Seconds)
                    {
                        vmdCtrlTimeline.ItemsWidth = vmdCtrlTimeline.MaxItemsWidth;
                        vmdCtrlTimeline.ZoomLevel = ETimelineZoomLevel.Minutes;
                    }
                }
                else
                {
                    vmdCtrlTimeline.ItemsWidth += change;
                }
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

        /// <summary>
        /// Called when pointer is pressed, starts dragging.
        /// </summary>
        private void Item_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(scrollViewer);
            if (pt.Properties.IsLeftButtonPressed)
            {
                IsDragging = true;
                StartPointerX = pt.Position.X;
                StartOffset = scrollViewer.HorizontalOffset;
                LastTime = DateTime.UtcNow;

                UIElement? uiSender = sender as UIElement;
                if (uiSender != null)
                {
                    uiSender.CapturePointer(e.Pointer);
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Called when pointer is moved, updates scrolling if dragging.
        /// </summary>
        private void Item_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsDragging)
            {
                return;
            }

            var pt = e.GetCurrentPoint(scrollViewer);
            var currentX = pt.Position.X;
            var now = DateTime.UtcNow;
            var dt = (now - LastTime).TotalMilliseconds;

            var delta = StartPointerX - currentX; // move opposite to pointer
            var newOffset = StartOffset + delta;
            scrollViewer.ChangeView(newOffset, null, null, true);
            LastTime = now;
            e.Handled = true;
        }

        /// <summary>
        /// Called when pointer is released, ends dragging.
        /// </summary>
        private void Item_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!IsDragging)
            {
                return;
            }
            EndDrag(sender, e);
        }

        /// <summary>
        /// Called when pointer capture is canceled, ends dragging.
        /// </summary>
        private void Item_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (!IsDragging)
            {
                return;
            }
            EndDrag(sender, e);
        }

        /// <summary>
        /// Stops the dragging action and applies inertia based on the last recorded velocity.
        /// </summary>
        private void EndDrag(object sender, PointerRoutedEventArgs e)
        {
            IsDragging = false;
            UIElement? uiSender = sender as UIElement;
            if (uiSender != null)
            {
                uiSender.ReleasePointerCaptures();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Received from view model. Requests the new position in the scrollview.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Receive(TimelineCurrentPositionChange message)
        {
            //VmdCtrlTimeline? vmd = DataContext as VmdCtrlTimeline;
            //if (vmd == null)
            //{
            //    return;
            //}

            //var offset = (message.NewPosition - vmd.StartPosition).TotalMicroseconds / vmd.CurrentLenght.TotalMicroseconds * scrollViewer.ExtentWidth;
            //scrollViewer.ChangeView(1000d/*offset*/, null, null, false);
        }

        /// <summary>
        /// Calculates the current position based on the scrollviewer state.
        /// Current position is the center of the scrollviewer.
        /// </summary>
        private void SetCurrentPosition()
        {
            VmdCtrlTimeline? vmd = DataContext as VmdCtrlTimeline;
            if (vmd == null)
            {
                return;
            }

            // Transform our ScrollViewers visible center point to ItemsRepeater center point
            var center = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            var pt = scrollViewer.TransformToVisual(itemsRepeater).TransformPoint(center);

            // Get the ItemsRepeater collection
            var items = itemsRepeater.ItemsSource as ObservableCollection<TimelineItem>;
            if (items == null)
            {
                return;
            }

            // Enumerate realized elements in ItemsRepeater by TryGetElement using indexes
            for (int i = 0; i < items?.Count; i++)
            {
                var element = itemsRepeater.TryGetElement(i);
                if (element == null) continue;

                // Get element bounds in repeater coordinates
                var transform = element.TransformToVisual(itemsRepeater);
                var topLeft = transform.TransformPoint(new Point(0, 0));
                var size = (element as FrameworkElement)?.RenderSize ?? new Size(0, 0);
                var rect = new Rect(topLeft, size);

                // Hitest the center point to the element rect
                if (rect.Contains(pt))
                {
                    Grid? grid = (element as Grid);
                    TimelineItem? item = items?[i];
                    if (item != null && grid != null)
                    {
                        double microsPerPixel = item.Duration.TotalMicroseconds / grid.ActualWidth;
                        double currentOffset = pt.X - grid.ActualOffset.X;
                        vmd.CurrentPosition = item.ItemKey.AddMicroseconds(microsPerPixel * currentOffset);
                    }
                }
            }
        }

        /// <summary>
        /// Called on ViewChanged events of the scrollview.
        /// We calculate here the current time and set for our view model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                SetCurrentPosition();
            }
        }

        /// <summary>
        /// Called on LayoutUpdated events of the scrollview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_LayoutUpdated(object sender, object e)
        {
            SetCurrentPosition();
        }

        /// <summary>
        /// Called when the pointer enters a FontIcon, triggering a spring animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerEntered(sender, typeof(FontIcon));
            e.Handled = true;
        }

        /// <summary>
        /// Called when the pointer exits a FontIcon, triggering a spring animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerExited(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the pointer is pressed on a FontIcon, triggering a spring animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerPressed(sender);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the pointer is released on a FontIcon, triggering a spring animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Animation.DoAnimationPointerReleased(sender);
            e.Handled = true;
        }
    }
}
