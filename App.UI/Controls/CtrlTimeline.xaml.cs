using System;
using JpegViewer.App.Core.Types;
using JpegViewer.App.Vmd.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace JpegViewer.App.UI.Controls
{
    /// <summary>
    /// Represents a user control for displaying and interacting with a timeline.
    /// </summary>
    public sealed partial class CtrlTimeline : UserControl
    {
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
        /// The last X coordinate during dragging action.
        /// </summary>
        private double LastPointerX { get; set; }

        /// <summary>
        /// Speed of scrolling in pixels per millisecond.
        /// </summary>
        private double Velocity { get; set; }

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
                LastPointerX = StartPointerX;
                LastTime = DateTime.UtcNow;
                Velocity = 0;

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
            if (dt > 0)
            {
                Velocity = (currentX - LastPointerX) / dt; // px per ms
            }

            var delta = StartPointerX - currentX; // move opposite to pointer
            var newOffset = StartOffset + delta;
            scrollViewer.ChangeView(newOffset, null, null, true);
            LastPointerX = currentX;
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

            // Apply simple inertia: continue scrolling based on velocity, decelerate
            const double deceleration = 0.0025; // px/ms^2
            double v = Velocity; // px/ms
            if (Math.Abs(v) < 0.01)
            {
                return;
            }

            // compute target offset with simple exponential decay/integration
            // s = v^2 / (2*a)
            double sign = Math.Sign(v);
            double stoppingDistance = (v * v) / (2 * deceleration);
            double target = scrollViewer.HorizontalOffset - sign * stoppingDistance;

            // clamp target within scrollable range
            var maxOffset = scrollViewer.ExtentWidth - scrollViewer.ViewportWidth;
            if (target < 0)
            {
                target = 0;
            }

            if (target > maxOffset)
            {
                target = maxOffset;
            }

            // use ChangeView with animation (true). The duration isn't directly controllable here.
            scrollViewer.ChangeView(target, null, null, true);
            e.Handled = true;
        }
    }
}
