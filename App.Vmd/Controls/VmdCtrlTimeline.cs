using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
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
        private DateTime _currentTime;
        private TimelineItem? _item;
        private TimelineItemBaseUnit? _baseUnit;
        private double _itemsWidth;
        private ETimelineZoomLevel _zoomLevel;
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
        /// Guard flag to prevent too much refresh actions on timeline items.
        /// </summary>
        public bool RefreshTimelineItemsRequestQueued { get; set; } = false;

        /// <summary>
        /// Holds a DateTime to jump on timeline full (re)initialization or changing among zoom levels.
        /// On application startup we jump to current DateTime by default.
        /// </summary>
        public JumpRequest JumpRequest { get; } = new JumpRequest(true, DateTime.Now);

        /// <summary>
        /// The current time represented by the absolute center position of the timeline control.
        /// </summary>
        public DateTime CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        /// <summary>
        /// Holds the current TimelineItem that belongs to CurrentTime.
        /// </summary>
        public TimelineItem? CurrentItem
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        /// <summary>
        /// Holds the current TimelineItemBaseUnit that belongs to CurrrentTime.
        /// </summary>
        public TimelineItemBaseUnit? CurrentBaseUnit
        {
            get => _baseUnit;
            set
            {
                if (SetProperty(ref _baseUnit, value))
                {
                    var imageInfo = _baseUnit?.Images?.FirstOrDefault();
                    if (imageInfo != null)
                    {
                        WeakReferenceMessenger.Default.Send(new CurrentImageChanged(imageInfo));
                    }
                }
            }
        }

        /// <summary>
        /// Start date of the timeline.
        /// </summary>
        public DateTime StartTime { get => _items.Any() ? _items.First().ItemKey : DateTime.MinValue; }

        /// <summary>
        /// End date of the timeline.
        /// </summary>
        public DateTime EndTime { get => _items.Any() ? _items.Last().EndTime : DateTime.MaxValue; }

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
        public double MinItemsWidth => _zoomLevel switch
        {
            ETimelineZoomLevel.Years => 100,
            ETimelineZoomLevel.Months => 100,
            ETimelineZoomLevel.Days => 32,
            ETimelineZoomLevel.Hours => 32,
            ETimelineZoomLevel.Minutes => 32,
            ETimelineZoomLevel.Seconds => 32,
            _ => 100
        };

        /// <summary>
        /// The maximum width allowed for the timeline items.
        /// </summary>
        public double MaxItemsWidth => _zoomLevel switch
        {
            ETimelineZoomLevel.Years => 130,
            ETimelineZoomLevel.Months => 130,
            ETimelineZoomLevel.Days => 62,
            ETimelineZoomLevel.Hours => 62,
            ETimelineZoomLevel.Minutes => 62,
            ETimelineZoomLevel.Seconds => 62,
            _ => 130
        };

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
            set => SetProperty(ref _zoomLevel, value);
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
            CurrentTime = DateTime.Now;
            ZoomLevel = ETimelineZoomLevel.Years;

            // Initialize the timeline with items
            ChangeItemsToZoomLevel(ETimelineZoomType.None);
        }

        /// <summary>
        /// Puts a task to the UI dispatcher to do zoom in/out action.
        /// </summary>
        /// <param name="deltaMouseWheel"></param>
        public void ScheduleZoomInZoomOut(int deltaMouseWheel)
        {
            if (RefreshTimelineItemsRequestQueued)
            {
                return;
            }

            // Set our guard flag to prevent flood of refreshes
            RefreshTimelineItemsRequestQueued = true;

            // Fire-and-forget debounce task
            Task.Run(() =>
            {
                // Enqueue to dispatcher — runs after current layout/measure/arrange
                DispatcherService.Invoke(() =>
                {
                    ZoomInZoomOut(deltaMouseWheel);

                    // Clear our guard flag to allow next refresh action
                    RefreshTimelineItemsRequestQueued = false;
                });
            });
        }

        /// <summary>
        /// Puts a task to the UI dispatcher to refresh timeline items.
        /// </summary>
        public void ScheduleRefreshTimelineItems()
        {
            // Set our guard flag to prevent flood of refreshes
            RefreshTimelineItemsRequestQueued = true;

            // Fire-and-forget debounce task
            Task.Run(() =>
            {
                // Enqueue to dispatcher — runs after current layout/measure/arrange
                DispatcherService.Invoke(() =>
                {
                    RefreshTimelineItems();

                    // Clear our guard flag to allow next refresh action
                    RefreshTimelineItemsRequestQueued = false;
                });
            });
        }

        /// <summary>
        /// Do zoom in our zoom out based on mouse wheel delta.
        /// </summary>
        /// <param name="deltaMouseWheel"></param>
        private void ZoomInZoomOut(int deltaMouseWheel)
        {
            if (JumpRequest.Active)
            {
                return;
            }

            double zoomStep = ZoomStep; // adjust sensitivity
            double change = (deltaMouseWheel > 0) ? zoomStep : -zoomStep;
            JumpRequest.JumpTo = CurrentTime;            

            if (ItemsWidth + change > MaxItemsWidth && ZoomLevel < ETimelineZoomLevel.Seconds)
            {
                ChangeItemsToZoomLevel(ETimelineZoomType.ZoomIn);
            }
            else if (ItemsWidth + change < MinItemsWidth && ZoomLevel > ETimelineZoomLevel.Years)
            {
                ChangeItemsToZoomLevel(ETimelineZoomType.ZoomOut);
            }
            else
            {
                ItemsWidth += change;
            }
        }

        /// <summary>
        /// Changes and fills the timeline with new items according to the zoom level.
        /// Checks whether zoom in or zoom out happened and sets the items width.
        /// </summary>
        /// <param name="zoomType"></param>
        private void ChangeItemsToZoomLevel(ETimelineZoomType zoomType)
        {
            if (zoomType == ETimelineZoomType.ZoomIn)
            {
                ZoomLevel++;
            }
            else if (zoomType == ETimelineZoomType.ZoomOut)
            {
                ZoomLevel--;
            }

            List<TimelineItem>? itemsList = TimelineService.GetItemsForZoomLevel(ZoomLevel, JumpRequest.JumpTo);
            if (itemsList != null)
            {
                lock (_itemsLock)
                {
                    Items.Clear();
                    ItemsWidth = (zoomType == ETimelineZoomType.ZoomOut ? MaxItemsWidth : MinItemsWidth);
                    JumpRequest.Active = true;
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
        private void RefreshTimelineItems()
        {
            lock (_itemsLock)
            {
                // If zero elements or more than the expected maximum then return
                if (Items.Count == 0 || Items.Count > 255)
                {
                    return;
                }
                
                int index = 0;
                for (; index < Items.Count; index++)
                {
                    if (Items[index].EndTime > CurrentTime)
                    {
                        break;
                    }
                }

                int itemIndexDiffFromCenter = index - TimelineService.ItemBufferSize;
                if (itemIndexDiffFromCenter == 0)
                {
                    // Current time belongs to center item. Nothing to do.
                    return;
                }
                else if (itemIndexDiffFromCenter < 0)
                {
                    // Try to keep the timeline above the year of 1100
                    if (Items.First().ItemKey.Year <= 1100)
                    {
                        return;
                    }

                    // We are closer to the start of our current timeline, so insert items to the beginning and remove from the end
                    var items = TimelineService.GetItemsForZoomLevel(ZoomLevel, Items.First().ItemKey, (byte)Math.Abs(itemIndexDiffFromCenter), ETimelineGetOption.Pre);
                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        Items.Insert(0, items[i]);
                        Items.RemoveAt(Items.Count - 1);
                    }
                }
                else
                {
                    // Try to keep the timeline under the year of 2100
                    if (Items.Last().ItemKey.Year >= 2100)
                    {
                        return;
                    }

                    // We are closer to the end of our current timeline, so insert items to the end and remove from the beginning
                    var items = TimelineService.GetItemsForZoomLevel(ZoomLevel, Items.Last().ItemKey, (byte)itemIndexDiffFromCenter, ETimelineGetOption.Post);
                    for (int i = 0; i < items.Count; i++)
                    {
                        Items.Add(items[i]);
                        Items.RemoveAt(0);
                    }
                }
            }
        }

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