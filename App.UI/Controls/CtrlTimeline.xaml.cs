using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using JpegViewer.App.Core;
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
        /// Holds the timeline control's viewmodel instance.
        /// </summary>
        private VmdCtrlTimeline ViewModel { get; } = App.GetService<VmdCtrlTimeline>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrlTimeline"/> class.
        /// </summary>
        public CtrlTimeline()
        {
            InitializeComponent();

            // Set the DataContext to the viewmodel of the Timeline control
            DataContext = ViewModel;

            // Register this instance to receive messages
            WeakReferenceMessenger.Default.Register<TimelineCurrentPositionChange>(this);
        }

        /// <summary>
        /// Handle pointer wheel changes on the ItemsRepeater to allow horizontal scrolling.
        /// </summary>
        private void ItemsRepeater_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (IsDragging || ViewModel.JumpRequest.Active || ViewModel.RefreshTimelineItemsRequestQueued)
            {
                return;
            }

            // First get delta value from mouse wheel
            int delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta; // ±120 per notch

            // Zoom in/out if Ctrl is pressed, otherwise scroll horizontally
            if (e.KeyModifiers == VirtualKeyModifiers.Control)
            {
                ViewModel.ScheduleZoomInZoomOut(delta);
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

            var delta = StartPointerX - currentX; // move opposite to pointer
            var newOffset = StartOffset + delta;
            scrollViewer.ChangeView(newOffset, null, null, true);
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
        }

        /// <summary>
        /// Calculates the current time based on the scrollviewer state.
        /// Current time is represented by the center position of the scrollviewer.
        /// </summary>
        private void CalculateCurrentTime()
        {
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
                        ViewModel.CurrentTime = item.ItemKey.AddMicroseconds(microsPerPixel * currentOffset);
                        ViewModel.CurrentItem = item;

                        int baseUnitIndex = (int)(currentOffset / ViewModel.ItemsWidth);
                        if (baseUnitIndex >= 0 && baseUnitIndex < item.Units.Count)
                        {
                            ViewModel.CurrentBaseUnit = item.Units[baseUnitIndex];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle element prepared event of base units to do scrolling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void itemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (ViewModel.JumpRequest.Active)
            {
                Grid? baseGrid = args.Element as Grid;
                if (baseGrid == null) return;
            
                TimelineItemBaseUnit? baseUnit = baseGrid.Tag as TimelineItemBaseUnit;
                if (baseUnit == null) return;
            
                Grid? parentGrid = (baseGrid.Parent as ItemsRepeater)?.Parent as Grid;
                if (parentGrid == null) return;
                
                TimelineItem? parentUnit = parentGrid.Tag as TimelineItem;
                if (parentUnit == null) return;
            
                int indexOfBaseUnit = parentUnit.Units.IndexOf(baseUnit);
                double actualOffset = parentGrid.ActualOffset.X + (baseGrid.ActualWidth * (double)indexOfBaseUnit);
                double halfViewPort = scrollViewer.ViewportWidth / 2;
                
                // Calculate additional offset based on unit type
                bool bScrollingDone = false;
                switch (baseUnit.Type)
                {
                    case ETimelineBaseUnitType.Year:
                    {
                        if (baseUnit.Value == ViewModel.JumpRequest.JumpTo.Year)
                        {
                            double diffMicro = (ViewModel.JumpRequest.JumpTo - parentUnit.ItemKey.AddYears(indexOfBaseUnit)).TotalMicroseconds;
                            double yearMicro = TimeSpan.FromDays(DateTime.IsLeapYear(ViewModel.JumpRequest.JumpTo.Year) ? 366 : 365).TotalMicroseconds;
                            double additionalOffset = baseGrid.ActualWidth / yearMicro * diffMicro;
                            actualOffset += additionalOffset - halfViewPort;
                            bScrollingDone = scrollViewer.ChangeView(actualOffset, null, null, true);
                        }
                        break;
                    }
                    case ETimelineBaseUnitType.Month:
                    {
                        if (CoreUtils.HasSameYear(parentUnit.ItemKey, ViewModel.JumpRequest.JumpTo) && baseUnit.Value == ViewModel.JumpRequest.JumpTo.Month)
                        {
                            double diffMicro = (ViewModel.JumpRequest.JumpTo - parentUnit.ItemKey.AddMonths(indexOfBaseUnit)).TotalMicroseconds;
                            double monthMicro = TimeSpan.FromDays(DateTime.DaysInMonth(ViewModel.JumpRequest.JumpTo.Year, ViewModel.JumpRequest.JumpTo.Month)).TotalMicroseconds;
                            double additionalOffset = baseGrid.ActualWidth / monthMicro * diffMicro;
                            actualOffset += additionalOffset - halfViewPort;
                            bScrollingDone = scrollViewer.ChangeView(actualOffset, null, null, true);
                        }
                        break;
                    }
                    case ETimelineBaseUnitType.Day:
                    {
                        if (CoreUtils.HasSameYearMonth(parentUnit.ItemKey, ViewModel.JumpRequest.JumpTo) && baseUnit.Value == ViewModel.JumpRequest.JumpTo.Day)
                        {
                            double diffMicro = (ViewModel.JumpRequest.JumpTo - parentUnit.ItemKey.AddDays(indexOfBaseUnit)).TotalMicroseconds;
                            double dayMicro = TimeSpan.FromDays(1).TotalMicroseconds;
                            double additionalOffset = baseGrid.ActualWidth / dayMicro * diffMicro;
                            actualOffset += additionalOffset - halfViewPort;
                            bScrollingDone = scrollViewer.ChangeView(actualOffset, null, null, true);
                        }
                        break;
                    }
                    case ETimelineBaseUnitType.Hour:
                    {
                        if (CoreUtils.HasSameYearMonthDay(parentUnit.ItemKey, ViewModel.JumpRequest.JumpTo) && baseUnit.Value == ViewModel.JumpRequest.JumpTo.Hour)
                        {
                            double diffMicro = (ViewModel.JumpRequest.JumpTo - parentUnit.ItemKey.AddHours(indexOfBaseUnit)).TotalMicroseconds;
                            double hourMicro = TimeSpan.FromHours(1).TotalMicroseconds;
                            double additionalOffset = baseGrid.ActualWidth / hourMicro * diffMicro;
                            actualOffset += additionalOffset - halfViewPort;
                            bScrollingDone = scrollViewer.ChangeView(actualOffset, null, null, true);
                        }
                        break;
                    }
                    case ETimelineBaseUnitType.Minute:
                    {
                        if (CoreUtils.HasSameYearMonthDayHour(parentUnit.ItemKey, ViewModel.JumpRequest.JumpTo) && baseUnit.Value == ViewModel.JumpRequest.JumpTo.Minute)
                        {
                            double diffMicro = (ViewModel.JumpRequest.JumpTo - parentUnit.ItemKey.AddMinutes(indexOfBaseUnit)).TotalMicroseconds;
                            double minuteMicro = TimeSpan.FromMinutes(1).TotalMicroseconds;
                            double additionalOffset = baseGrid.ActualWidth / minuteMicro * diffMicro;
                            actualOffset += additionalOffset - halfViewPort;
                            bScrollingDone = scrollViewer.ChangeView(actualOffset, null, null, true);
                        }
                        break;
                    }
                    case ETimelineBaseUnitType.Second:
                    {
                        if (CoreUtils.HasSameYearMonthDayHourMinute(parentUnit.ItemKey, ViewModel.JumpRequest.JumpTo) && baseUnit.Value == ViewModel.JumpRequest.JumpTo.Second)
                        {
                            double diffMicro = (ViewModel.JumpRequest.JumpTo - parentUnit.ItemKey.AddSeconds(indexOfBaseUnit)).TotalMicroseconds;
                            double secondMicro = TimeSpan.FromSeconds(1).TotalMicroseconds;
                            double additionalOffset = baseGrid.ActualWidth / secondMicro * diffMicro;
                            actualOffset += additionalOffset - halfViewPort;
                            bScrollingDone = scrollViewer.ChangeView(actualOffset, null, null, true);
                        }
                        break;
                    }
                    default:
                        break;
                }
            
                if (bScrollingDone)
                {
                    // We have found the element we wanted and scrolled to it
                    ViewModel.JumpRequest.Active = false;
                }
                else if (actualOffset > scrollViewer.ViewportWidth)
                {
                    // Scroll toward the end to bring into our view the wanted element
                    scrollViewer.ChangeView(actualOffset, null, null, true);
                }
            }
        }

        /// <summary>
        /// Set the anchor for zooming to the center element of our scrollviewer that is under the hair line.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void scrollViewer_AnchorRequested(ScrollViewer sender, AnchorRequestedEventArgs args)
        {
            if (ViewModel.JumpRequest.Active || IsDragging)
            {
                return;
            }

            // Transform our ScrollViewers visible center point to ItemsRepeater center point
            var center = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            var pt = scrollViewer.TransformToVisual(itemsRepeater).TransformPoint(center);

            foreach (var element in args.AnchorCandidates.Where(e => (e as Grid)?.Tag is TimelineItemBaseUnit))
            {
                // Get element bounds in repeater coordinates
                var transform = element.TransformToVisual(itemsRepeater);
                var topLeft = transform.TransformPoint(new Point(0, 0));
                var size = (element as FrameworkElement)?.RenderSize ?? new Size(0, 0);
                var rect = new Rect(topLeft, size);

                // Hitest the center point to the element rect
                if (rect.Contains(pt))
                {
                    args.Anchor = element;
                    return;
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
                CalculateCurrentTime();

                if (!ViewModel.JumpRequest.Active)
                {
                    if (!IsDragging && !ViewModel.RefreshTimelineItemsRequestQueued)
                    {
                        ViewModel.ScheduleRefreshTimelineItems();
                    }
                }
            }
        }

        /// <summary>
        /// Called on LayoutUpdated events of the scrollview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_LayoutUpdated(object sender, object e)
        {
            if (!ViewModel.JumpRequest.Active && !IsDragging && !ViewModel.RefreshTimelineItemsRequestQueued)
            {
                CalculateCurrentTime();
            }
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
