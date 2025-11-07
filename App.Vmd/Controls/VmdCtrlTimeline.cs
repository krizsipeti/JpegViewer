using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
                    Task.Run(() => ChangeItemsToZoomLevel(value));
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
        public VmdCtrlTimeline(IDispatcherService dispatcherService, IImageService imageService) : base(dispatcherService)
        {
            ItemsWidth = MinItemsWidth;
            CurrentPosition = DateTime.Now;
            ZoomLevel = ETimelineZoomLevel.Years;
            ImageService = imageService;
            //ImageService.ImageFound += ImageService_ImageFound;
            ImageService.ImagesLoaded += ImageService_ImagesLoaded;
        }

        /// <summary>
        /// Changes and fills the timeline with new items according to the new zoom level.
        /// </summary>
        /// <param name="zoomLevel"></param>
        private void ChangeItemsToZoomLevel(ETimelineZoomLevel zoomLevel)
        {
            List<TimelineItem>? itemsList = null;
            switch (zoomLevel)
            {
                case ETimelineZoomLevel.Years:
                    itemsList = GetItemsForYearZoom(CurrentPosition.AddYears(-30), CurrentPosition.AddYears(30));
                    break;
                case ETimelineZoomLevel.Months:
                    itemsList = GetItemsForMonthZoom(CurrentPosition.AddYears(-3), CurrentPosition.AddYears(3));
                    break;
                case ETimelineZoomLevel.Days:
                    itemsList = GetItemsForDayZoom(CurrentPosition.AddMonths(-3), CurrentPosition.AddMonths(3));
                    break;
                case ETimelineZoomLevel.Hours:
                    itemsList = GetItemsForHourZoom(CurrentPosition.AddDays(-3), CurrentPosition.AddDays(3));
                    break;
                case ETimelineZoomLevel.Minutes:
                    itemsList = GetItemsForMinuteZoom(CurrentPosition.AddHours(-3), CurrentPosition.AddHours(3));
                    break;
                case ETimelineZoomLevel.Seconds:
                    itemsList = GetItemsForSecondZoom(CurrentPosition.AddMinutes(-3), CurrentPosition.AddMinutes(3));
                    break;
            }

            if (itemsList != null)
            {
                DispatcherService.Invoke(() =>
                {
                    lock (_itemsLock)
                    {
                        Items.Clear();
                        foreach (var item in itemsList)
                        {
                            Items.Add(item);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Updates time line when scrolling toward the left or right end.
        /// </summary>
        private void RefreshTimeLineItems()
        {
            if (ImageService.MinDateTaken == null) return;
            lock (_itemsLock)
            {
                if (CurrentPosition - StartPosition < EndPosition - CurrentPosition)
                {
                    // We are closer to the start (left end) of our time line
                    // Check if there are images created before our current start position
                    if (/*(*/ImageService.MinDateTaken /*- (Items.First().Duration * 3))*/ < StartPosition)
                    {
                        int amountToAdd = (int)((CurrentPosition - StartPosition).TotalMicroseconds / Items.First().Duration.TotalMicroseconds);
                        if (amountToAdd <= 0)
                        {
                            return;
                        }

                        List<TimelineItem>? itemsList = null;
                        switch (ZoomLevel)
                        {
                            case ETimelineZoomLevel.Years:
                                itemsList = GetItemsForYearZoom(StartPosition.AddYears(-(amountToAdd * 10)), StartPosition.AddYears(-10));
                                break;
                            case ETimelineZoomLevel.Months:
                                itemsList = GetItemsForMonthZoom(StartPosition.AddYears(-amountToAdd), StartPosition.AddYears(-1));
                                break;
                            case ETimelineZoomLevel.Days:
                                itemsList = GetItemsForDayZoom(StartPosition.AddMonths(-amountToAdd), StartPosition.AddMonths(-1));
                                break;
                            case ETimelineZoomLevel.Hours:
                                itemsList = GetItemsForHourZoom(StartPosition.AddDays(-amountToAdd), StartPosition.AddDays(-1));
                                break;
                            case ETimelineZoomLevel.Minutes:
                                itemsList = GetItemsForMinuteZoom(StartPosition.AddHours(-amountToAdd), StartPosition.AddHours(-1));
                                break;
                            case ETimelineZoomLevel.Seconds:
                                itemsList = GetItemsForSecondZoom(StartPosition.AddMinutes(-amountToAdd), StartPosition.AddMinutes(-1));
                                break;
                        }

                        if (itemsList == null) return;
                        DispatcherService.Invoke(() =>
                        {
                            for (int i = itemsList.Count - 1; i >= 0; i--)
                            {
                                Items.Insert(0, itemsList[i]);
                                Items.RemoveAt(Items.Count - 1);
                            }
                        });                        
                    }
                }
                else
                {
                    // We are closer to the end (right side) of our time line
                    // Check if there are images created after our current end position
                    if (/*(*/ImageService.MaxDateTaken/* + (Items.Last().Duration * 3))*/ > EndPosition)
                    {
                        int amountToAdd = (int)((EndPosition - CurrentPosition).TotalMicroseconds / Items.Last().Duration.TotalMicroseconds);
                        if (amountToAdd <= 0)
                        {
                            return;
                        }

                        List<TimelineItem>? itemsList = null;
                        switch (ZoomLevel)
                        {
                            case ETimelineZoomLevel.Years:
                                itemsList = GetItemsForYearZoom(EndPosition.AddYears(10), EndPosition.AddYears(amountToAdd * 10));
                                break;
                            case ETimelineZoomLevel.Months:
                                itemsList = GetItemsForMonthZoom(EndPosition.AddYears(1), StartPosition.AddYears(amountToAdd));
                                break;
                            case ETimelineZoomLevel.Days:
                                itemsList = GetItemsForDayZoom(EndPosition.AddMonths(1), StartPosition.AddMonths(amountToAdd));
                                break;
                            case ETimelineZoomLevel.Hours:
                                itemsList = GetItemsForHourZoom(EndPosition.AddDays(1), StartPosition.AddDays(amountToAdd));
                                break;
                            case ETimelineZoomLevel.Minutes:
                                itemsList = GetItemsForMinuteZoom(EndPosition.AddHours(1), StartPosition.AddHours(amountToAdd));
                                break;
                            case ETimelineZoomLevel.Seconds:
                                itemsList = GetItemsForSecondZoom(EndPosition.AddMinutes(1), StartPosition.AddMinutes(amountToAdd));
                                break;
                        }

                        if (itemsList == null) return;
                        DispatcherService.Invoke(() =>
                        {
                            for (int i = 0; i < itemsList.Count; i++)
                            {
                                Items.Insert(Items.Count(), itemsList[i]);
                                Items.RemoveAt(0);
                            }
                        });
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
            
            var itemsList = GetItemsForYearZoom(new DateTime(e.DateTaken.Year - 20, 1, 1), new DateTime(e.DateTaken.Year + 20, 1, 1));
            DispatcherService.Invoke(() =>
            {
                lock (_itemsLock)
                {
                    Items.Clear();
                    foreach (var item in itemsList)
                    {
                        Items.Add(item);
                    }
                }
            });
        }

        /// <summary>
        /// Calculates timeline items for year zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForYearZoom(DateTime startYear, DateTime endYear)
        {
            // First group images by year
            var imagesByYear = ImageService.GetImagesForDateRange(startYear, endYear)
                .GroupBy(i => i.DateTaken.Year).ToDictionary(g => g.Key, g => new List<ImageInfo>(g));

            // Determine the start and end decade
            int startDecade = (startYear.Year / 10) * 10;
            int endDecade = (endYear.Year / 10) * 10;

            // Allocate list with the calculated number of decades and create the TimelineItems
            var result = new List<TimelineItem>((endDecade - startDecade) / 10 + 1);
            for (int decade = startDecade; decade <= endDecade; decade += 10)
            {
                // Allocate list for base units. We show 10 in every decade.
                var baseUnits = new List<TimelineItemBaseUnit>(10);
                for (int year = decade; year < decade + 10; year++)
                {
                    imagesByYear.TryGetValue(year, out var images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Year, year) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(decade, 1, 1), ETimelineItemType.YearsOfDecade, baseUnits));
            }

            return result;
        }

        /// <summary>
        /// Calculates timeline items for month zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForMonthZoom(DateTime start, DateTime end)
        {
            // Check for wrong parameters
            if (start > end)
            {
                return new List<TimelineItem>();
            }

            // First group images by year and month
            var imagesByYM = ImageService.GetImagesForDateRange(start, end).GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => new List<ImageInfo>(mg)));

            // Allocate list and create the timeline items
            var result = new List<TimelineItem>(end.Year - start.Year + 1);
            for (DateTime s = start; s <= end; s = s.AddYears(1))
            {
                // Try to get the current year with the month groups
                imagesByYM.TryGetValue(s.Year, out var monthsDict);

                // Allocate list for every month base units
                var baseUnits = new List<TimelineItemBaseUnit>(12);
                for (int month = 1; month <= 12; month++)
                {
                    List<ImageInfo>? images = null;
                    monthsDict?.TryGetValue(month, out images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Month, month) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(s.Year, 1, 1), ETimelineItemType.MonthsOfYear, baseUnits));
            }
            return result;
        }

        /// <summary>
        /// Calculates timeline items for day zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForDayZoom(DateTime start, DateTime end)
        {
            // Check for wrong parameters
            if (start > end)
            {
                return new List<TimelineItem>();
            }

            // First group images by year, month and day
            var imagesByYMD = ImageService.GetImagesForDateRange(start, end).GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => new List<ImageInfo>(dg))));

            // Allocate list and create the timeline items
            var result = new List<TimelineItem>();
            for (DateTime s = start; s <= end; s = s.AddMonths(1))
            {
                // Try to get the current year with the month groups
                imagesByYMD.TryGetValue(s.Year, out var monthsDict);

                // Try to get current month with the days group
                Dictionary<int, List<ImageInfo>>? daysDict = null;
                monthsDict?.TryGetValue(s.Month, out daysDict);

                // Allocate list for every day base units
                int daysInMonth = DateTime.DaysInMonth(s.Year, s.Month);
                var baseUnits = new List<TimelineItemBaseUnit>(daysInMonth);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    List<ImageInfo>? images = null;
                    daysDict?.TryGetValue(day, out images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Day, day) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(s.Year, s.Month, 1), ETimelineItemType.DaysOfMonth, baseUnits));
            }
            return result;
        }

        /// <summary>
        /// Calculates timeline items for hour zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForHourZoom(DateTime start, DateTime end)
        {
            // Check for wrong parameters
            if (start > end)
            {
                return new List<TimelineItem>();
            }

            // First group images by year, month, day and hour
            var imagesByYMDH = ImageService.GetImagesForDateRange(start, end).GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Hour)
                .ToDictionary(hg => hg.Key, hg => new List<ImageInfo>(hg)))));

            // Allocate list with the calculated number of days and create the timeline items
            var result = new List<TimelineItem>(((int)(end - start).TotalDays) + 1);
            for (DateTime s = start; s <= end; s = s.AddDays(1))
            {
                // Try to get the current year with the month groups
                imagesByYMDH.TryGetValue(s.Year, out var monthsDict);

                // Try to get current month with the days group
                Dictionary<int, Dictionary<int, List<ImageInfo>>>? daysDict = null;
                monthsDict?.TryGetValue(s.Month, out daysDict);

                // Try to get current day with the hours group
                Dictionary<int, List<ImageInfo>>? hoursDict = null;
                daysDict?.TryGetValue(s.Day, out hoursDict);

                // Allocate list for hour base units
                var baseUnits = new List<TimelineItemBaseUnit>(24);
                for (int hour = 0; hour < 24; hour++)
                {
                    List<ImageInfo>? images = null;
                    hoursDict?.TryGetValue(hour, out images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Hour, hour) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(s.Year, s.Month, s.Day), ETimelineItemType.HoursOfDay, baseUnits));
            }
            return result;
        }

        /// <summary>
        /// Calculates timeline items for minute zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForMinuteZoom(DateTime start, DateTime end)
        {
            // Check for wrong parameters
            if (start > end)
            {
                return new List<TimelineItem>();
            }

            // First group images by year, month, day and hour
            var imagesByYMDHM = ImageService.GetImagesForDateRange(start, end).GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Hour)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Minute)
                .ToDictionary(hg => hg.Key, hg => new List<ImageInfo>(hg))))));

            // Allocate list with the calculated number of hours and create the timeline items
            var result = new List<TimelineItem>(((int)(end - start).TotalHours) + 1);
            for (DateTime s = start; s <= end; s = s.AddHours(1))
            {
                // Try to get the current year with the month groups
                imagesByYMDHM.TryGetValue(s.Year, out var monthsDict);

                // Try to get current month with the days group
                Dictionary<int, Dictionary<int, Dictionary<int, List<ImageInfo>>>>? daysDict = null;
                monthsDict?.TryGetValue(s.Month, out daysDict);

                // Try to get current day with the hours group
                Dictionary<int, Dictionary<int, List<ImageInfo>>>? hoursDict = null;
                daysDict?.TryGetValue(s.Day, out hoursDict);

                // Try to get current hour with the minutes group
                Dictionary<int, List<ImageInfo>>? minutesDict = null;
                hoursDict?.TryGetValue(s.Hour, out minutesDict);

                // Allocate list for minute base units
                var baseUnits = new List<TimelineItemBaseUnit>(60);
                for (int minute = 0; minute < 60; minute++)
                {
                    List<ImageInfo>? images = null;
                    minutesDict?.TryGetValue(minute, out images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Minute, minute) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(s.Year, s.Month, s.Day, s.Hour, 0, 0), ETimelineItemType.MinutesOfHour, baseUnits));
            }
            return result;
        }

        /// <summary>
        /// Calculates timeline items for second zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForSecondZoom(DateTime start, DateTime end)
        {
            // Check for wrong parameters
            if (start > end)
            {
                return new List<TimelineItem>();
            }

            // First group images by year, month, day and hour
            var imagesByYMDHMS = ImageService.GetImagesForDateRange(start, end).GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Hour)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Minute)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Second)
                .ToDictionary(hg => hg.Key, hg => new List<ImageInfo>(hg)))))));

            // Allocate list with the calculated number of minutes and create the timeline items
            var result = new List<TimelineItem>(((int)(end - start).TotalMinutes) + 1);
            for (DateTime s = start; s <= end; s = s.AddMinutes(1))
            {
                // Try to get the current year with the month groups
                imagesByYMDHMS.TryGetValue(s.Year, out var monthsDict);

                // Try to get current month with the days group
                Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<ImageInfo>>>>>? daysDict = null;
                monthsDict?.TryGetValue(s.Month, out daysDict);

                // Try to get current day with the hours group
                Dictionary<int, Dictionary<int, Dictionary<int, List<ImageInfo>>>>? hoursDict = null;
                daysDict?.TryGetValue(s.Day, out hoursDict);

                // Try to get current hour with the minutes group
                Dictionary<int, Dictionary<int, List<ImageInfo>>>? minutesDict = null;
                hoursDict?.TryGetValue(s.Hour, out minutesDict);

                // Try to get current minute with the seconds group
                Dictionary<int, List<ImageInfo>>? secondsDict = null;
                minutesDict?.TryGetValue(s.Minute, out secondsDict);

                // Allocate list for seconds base units
                var baseUnits = new List<TimelineItemBaseUnit>(60);
                for (int second = 0; second < 60; second++)
                {
                    List<ImageInfo>? images = null;
                    secondsDict?.TryGetValue(second, out images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Second, second) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(s.Year, s.Month, s.Day, s.Hour, s.Minute, 0), ETimelineItemType.SecondsOfMinute, baseUnits));
            }
            return result;
        }
    }
}