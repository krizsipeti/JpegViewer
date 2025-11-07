using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;


namespace JpegViewer.App.Vmd.Controls
{
    /// <summary>
    /// View model for timeline control.
    /// </summary>
    public partial class VmdCtrlTimeline : VmdBase
    {
        private ObservableCollection<TimelineItem> _items = new ObservableCollection<TimelineItem>();
        private DateTime _currentPosition;
        private double _itemsWidth;
        private ETimelineZoomLevel _zoomLevel = ETimelineZoomLevel.Seconds;
        private readonly object _itemsLock = new object();

        /// <summary>
        /// Holds a reference to our TimelineService instance.
        /// </summary>
        private ITimelineService TimelineService { get; }

        /// <summary>
        /// Holds a reference to the only ImageService.
        /// </summary>
        private IImageService ImageService { get; }

        /// <summary>
        /// Holds a DateTime to jump on timeline full (re)initialization or changing among zoom levels.
        /// On application startup we jump to current DateTime by default.
        /// </summary>
        public JumpRequest JumpRequest { get; } = new JumpRequest(true, DateTime.Now);

        /// <summary>
        /// The current position of the timeline control.
        /// </summary>
        public DateTime CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        /// <summary>
        /// Start date of the timeline.
        /// </summary>
        public DateTime StartPosition { get => _items.Any() ? _items.First().ItemKey : DateTime.MinValue; }

        /// <summary>
        /// End date of the timeline.
        /// </summary>
        public DateTime EndPosition { get => _items.Any() ? _items.Last().EndTime : DateTime.MaxValue; }

        /// <summary>
        /// The current lenght of the timeline elements.
        /// </summary>
        public TimeSpan CurrentLenght
        {
            get
            {
                if (!_items.Any())
                {
                    return TimeSpan.Zero;
                }
                return _items.Last().EndTime - _items.First().ItemKey;
            }
        }

        /// <summary>
        /// Holds the width of the timeline items.
        /// </summary>
        public double ItemsWidth
        {
            get => _itemsWidth;
            set
            {
                // Minimum and maximum width constraints
                if (value >= MinItemsWidth && MaxItemsWidth >= value)
                {
                    SetProperty(ref _itemsWidth, value);
                }
            }
        }

        /// <summary>
        /// The minimum width allowed for the timeline items.
        /// </summary>
        public double MinItemsWidth => 100;

        /// <summary>
        /// The maximum width allowed for the timeline items.
        /// </summary>
        public double MaxItemsWidth => 200;

        /// <summary>
        /// The zoom step applied on mouse wheel action.
        /// </summary>
        public double ZoomStep => 10;

        /// <summary>
        /// The current zoom level of the timeline.
        /// </summary>
        public ETimelineZoomLevel ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (SetProperty(ref _zoomLevel, value))
                {
                    ChangeItemsToZoomLevel(value);
                }
            }
        }

        /// <summary>
        /// The current timeline items set based on the zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdCtrlTimeline"/> class.
        /// </summary>
        public VmdCtrlTimeline(IDispatcherService dispatcherService, ITimelineService timelineService, IImageService imageService) : base(dispatcherService)
        {
            // First setup our services
            TimelineService = timelineService;
            ImageService = imageService;
            ImageService.ImagesLoaded += ImageService_ImagesLoaded;

            // Set further default startup values
            ItemsWidth = MinItemsWidth;
            CurrentPosition = DateTime.Now;
            ZoomLevel = ETimelineZoomLevel.Years;
        }

        /// <summary>
        /// Do zoom in our zoom out based on mouse wheel delta.
        /// </summary>
        /// <param name="deltaMouseWheel"></param>
        public void ZoomInZoomOut(int deltaMouseWheel)
        {
            if (JumpRequest.Active)
            {
                return;
            }

            double zoomStep = ZoomStep; // adjust sensitivity
            double change = (deltaMouseWheel > 0) ? zoomStep : -zoomStep;
            JumpRequest.JumpTo = CurrentPosition;            

            if (ItemsWidth + change > MaxItemsWidth && ZoomLevel < ETimelineZoomLevel.Seconds)
            {
                // Do zoom in
                lock (_itemsLock)
                {
                    Items.Clear();
                }
                JumpRequest.Active = true;
                ItemsWidth = MinItemsWidth;
                ZoomLevel++;
            }
            else if (ItemsWidth + change < MinItemsWidth && ZoomLevel > ETimelineZoomLevel.Years)
            {
                // Do zoom out
                lock (_itemsLock)
                {
                    Items.Clear();
                }
                JumpRequest.Active = true;
                ItemsWidth = MaxItemsWidth;
                ZoomLevel--;
            }
            else
            {
                ItemsWidth += change;
            }
        }

        /// <summary>
        /// Changes and fills the timeline with new items according to the new zoom level.
        /// </summary>
        /// <param name="zoomLevel"></param>
        private void ChangeItemsToZoomLevel(ETimelineZoomLevel zoomLevel)
        {
            List<TimelineItem>? itemsList = TimelineService.GetItemsForZoomLevel(zoomLevel, CurrentPosition);
            if (itemsList != null)
            {
                lock (_itemsLock)
                {
                    Items.Clear();
                    foreach (var item in itemsList)
                    {
                        Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Updates time line when scrolling toward the left or right end.
        /// </summary>
        //private void RefreshTimeLineItems()
        //{
            //if (ImageService.MinDateTaken == null) return;
            //lock (_itemsLock)
            //{
                //if (CurrentPosition - StartPosition < EndPosition - CurrentPosition)
                //{
                    //// We are closer to the start (left end) of our time line
                    //// Check if there are images created before our current start position
                    //if (/*(*/ImageService.MinDateTaken /*- (Items.First().Duration * 3))*/ < StartPosition)
                    //{
                        //int amountToAdd = (int)((CurrentPosition - StartPosition).TotalMicroseconds / Items.First().Duration.TotalMicroseconds);
                        //if (amountToAdd <= 0)
                        //{
                            //return;
                        //}

                        //List<TimelineItem>? itemsList = null;
                        //switch (ZoomLevel)
                        //{
                            //case ETimelineZoomLevel.Years:
                                //itemsList = GetItemsForYearZoom(StartPosition.AddYears(-(amountToAdd * 10)), StartPosition.AddYears(-10));
                                //break;
                            //case ETimelineZoomLevel.Months:
                                //itemsList = GetItemsForMonthZoom(StartPosition.AddYears(-amountToAdd), StartPosition.AddYears(-1));
                                //break;
                            //case ETimelineZoomLevel.Days:
                                //itemsList = GetItemsForDayZoom(StartPosition.AddMonths(-amountToAdd), StartPosition.AddMonths(-1));
                                //break;
                            //case ETimelineZoomLevel.Hours:
                                //itemsList = GetItemsForHourZoom(StartPosition.AddDays(-amountToAdd), StartPosition.AddDays(-1));
                                //break;
                            //case ETimelineZoomLevel.Minutes:
                                //itemsList = GetItemsForMinuteZoom(StartPosition.AddHours(-amountToAdd), StartPosition.AddHours(-1));
                                //break;
                            //case ETimelineZoomLevel.Seconds:
                                //itemsList = GetItemsForSecondZoom(StartPosition.AddMinutes(-amountToAdd), StartPosition.AddMinutes(-1));
                                //break;
                        //}

                        //if (itemsList == null) return;
                        //DispatcherService.Invoke(() =>
                        //{
                            //for (int i = itemsList.Count - 1; i >= 0; i--)
                            //{
                                //Items.Insert(0, itemsList[i]);
                                //Items.RemoveAt(Items.Count - 1);
                            //}
                        //});                        
                    //}
                //}
                //else
                //{
                    //// We are closer to the end (right side) of our time line
                    //// Check if there are images created after our current end position
                    //if (/*(*/ImageService.MaxDateTaken/* + (Items.Last().Duration * 3))*/ > EndPosition)
                    //{
                        //int amountToAdd = (int)((EndPosition - CurrentPosition).TotalMicroseconds / Items.Last().Duration.TotalMicroseconds);
                        //if (amountToAdd <= 0)
                        //{
                            //return;
                        //}

                        //List<TimelineItem>? itemsList = null;
                        //switch (ZoomLevel)
                        //{
                            //case ETimelineZoomLevel.Years:
                                //itemsList = GetItemsForYearZoom(EndPosition.AddYears(10), EndPosition.AddYears(amountToAdd * 10));
                                //break;
                            //case ETimelineZoomLevel.Months:
                                //itemsList = GetItemsForMonthZoom(EndPosition.AddYears(1), StartPosition.AddYears(amountToAdd));
                                //break;
                            //case ETimelineZoomLevel.Days:
                                //itemsList = GetItemsForDayZoom(EndPosition.AddMonths(1), StartPosition.AddMonths(amountToAdd));
                                //break;
                            //case ETimelineZoomLevel.Hours:
                                //itemsList = GetItemsForHourZoom(EndPosition.AddDays(1), StartPosition.AddDays(amountToAdd));
                                //break;
                            //case ETimelineZoomLevel.Minutes:
                                //itemsList = GetItemsForMinuteZoom(EndPosition.AddHours(1), StartPosition.AddHours(amountToAdd));
                                //break;
                            //case ETimelineZoomLevel.Seconds:
                                //itemsList = GetItemsForSecondZoom(EndPosition.AddMinutes(1), StartPosition.AddMinutes(amountToAdd));
                                //break;
                        //}

                        //if (itemsList == null) return;
                        //DispatcherService.Invoke(() =>
                        //{
                            //for (int i = 0; i < itemsList.Count; i++)
                            //{
                                //Items.Insert(Items.Count(), itemsList[i]);
                                //Items.RemoveAt(0);
                            //}
                        //});
                    //}
                //}
            //}
        //}

        /// <summary>
        /// Handle the event of a new image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ImageService_ImageFound(object? sender, ImageInfo e)
        {
        }

        /// <summary>
        /// Handle the event of image folder fully loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The first image by its taken date</param>
        /// <exception cref="NotImplementedException"></exception>
        private void ImageService_ImagesLoaded(object? sender, ImageInfo e)
        {
            if (e == null)
            {
                // Means no images were found in the specified folder.
                return;
            }
            
            //var itemsList = GetItemsForYearZoom(new DateTime(e.DateTaken.Year - 20, 1, 1), new DateTime(e.DateTaken.Year + 20, 1, 1));
            var itemsList = TimelineService.GetItemsForZoomLevel(ZoomLevel, e.DateTaken);
            DispatcherService.Invoke(() =>
            {
                lock (_itemsLock)
                {
                    Items.Clear();
                    JumpRequest.JumpTo = e.DateTaken;
                    JumpRequest.Active = true;
                    foreach (var item in itemsList)
                    {
                        Items.Add(item);
                    }
                }
            });
        }

        
    }
}