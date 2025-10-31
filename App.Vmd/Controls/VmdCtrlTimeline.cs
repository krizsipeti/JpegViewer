using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
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
        private ObservableCollection<TimelineItem> _itemsYears = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsMonths = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsDays = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsHours = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsMinutes = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsSeconds = new ObservableCollection<TimelineItem>();
        private DateTime _currentPosition;
        private double _itemsWidth;
        private ETimelineZoomLevel _zoomLevel;

        /// <summary>
        /// The current position of the timeline control.
        /// </summary>
        public DateTime CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

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
                    switch (value)
                    {
                        case ETimelineZoomLevel.Years:
                            Items = ItemsYears;
                            break;
                        case ETimelineZoomLevel.Months:
                            Items = ItemsMonths;
                            break;
                        case ETimelineZoomLevel.Days:
                            Items = ItemsDays;
                            break;
                        case ETimelineZoomLevel.Hours:
                            Items = ItemsHours;
                            break;
                        case ETimelineZoomLevel.Minutes:
                            Items = ItemsMinutes;
                            break;
                        case ETimelineZoomLevel.Seconds:
                            Items = ItemsSeconds;
                            break;
                    }
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
        /// Timeline items for years zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> ItemsYears
        {
            get => _itemsYears;
            set => SetProperty(ref _itemsYears, value);
        }

        /// <summary>
        /// Timeline items for month zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> ItemsMonths
        {
            get => _itemsMonths;
            set => SetProperty(ref _itemsMonths, value);
        }

        /// <summary>
        /// Timeline items for days zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> ItemsDays
        {
            get => _itemsDays;
            set => SetProperty(ref _itemsDays, value);
        }

        /// <summary>
        /// Timeline items for hours zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> ItemsHours
        {
            get => _itemsHours;
            set => SetProperty(ref _itemsHours, value);
        }

        /// <summary>
        /// Timeline items for minutes zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> ItemsMinutes
        {
            get => _itemsMinutes;
            set => SetProperty(ref _itemsMinutes, value);
        }

        /// <summary>
        /// Timeline items for seconds zoom level.
        /// </summary>
        public ObservableCollection<TimelineItem> ItemsSeconds
        {
            get => _itemsSeconds;
            set => SetProperty(ref _itemsSeconds, value);
        }

        /// <summary>
        /// Holds picture dates to be displayed on the timeline.
        /// </summary>
        public List<ImageInfo> Images = new List<ImageInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdCtrlTimeline"/> class.
        /// </summary>
        public VmdCtrlTimeline(IDispatcherService dispatcherService) : base(dispatcherService)
        {
            ItemsWidth = MinItemsWidth;
            ZoomLevel = ETimelineZoomLevel.Days;
        }

        /// <summary>
        /// Fills the timeline with dummy picture dates when the control is loaded.
        /// </summary>
        [RelayCommand]
        private async Task Loaded()
        {
            await Task.Run(() => CreateDummyDates());
        }

        /// <summary>
        /// Generates dummy picture dates for testing purposes.
        /// </summary>
        private void CreateDummyDates()
        {
            Images.Clear();
            var start = DateTime.Now;//.AddYears(100);
            var end = start.AddYears(-1);
            var range = (end - start).TotalSeconds;
            // secure random double in [0,1)
            Span<byte> bytes = stackalloc byte[8];
            for (int i = 0; i < 10000; i++)
            {
                RandomNumberGenerator.Fill(bytes);
                ulong u = BitConverter.ToUInt64(bytes);
                double sample = (u >> 11) * (1.0 / (1UL << 53)); // 53-bit precision
                var secs = (long)(sample * range);
                Images.Add(new ImageInfo(Guid.NewGuid().ToString("X"), start.AddSeconds(secs)));
            }
            Images.Sort((a, b) => a.DateTaken.CompareTo(b.DateTaken));

            // After generating dates, calculate timeline items
            Task.Run(() =>
            {
                var yearsItems = GetItemsForYearZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsYears = new ObservableCollection<TimelineItem>(yearsItems); ZoomLevel = ETimelineZoomLevel.Years; });
            });

            Task.Run(() =>
            {
                var monthsItems = GetItemsForMonthZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsMonths = new ObservableCollection<TimelineItem>(monthsItems); });
            });

            Task.Run(() =>
            {
                var daysItems = GetItemsForDayZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsDays = new ObservableCollection<TimelineItem>(daysItems); });
            });

            Task.Run(() =>
            {
                var hoursItems = GetItemsForHourZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsHours = new ObservableCollection<TimelineItem>(hoursItems); });
            });

            Task.Run(() =>
            {
                var minutesItems = GetItemsForMinuteZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsMinutes = new ObservableCollection<TimelineItem>(minutesItems); });
            });

            Task.Run(() =>
            {
                var secondsItems = GetItemsForSecondZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsSeconds = new ObservableCollection<TimelineItem>(secondsItems); });
            });
        }

        /// <summary>
        /// Calculates timeline items for year zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForYearZoom()
        {
            // First group images by year
            var imagesByYear = Images.GroupBy(i => i.DateTaken.Year).ToDictionary(g => g.Key, g => new List<ImageInfo>(g));
            if (imagesByYear.Count == 0)
            {
                return new List<TimelineItem>(); // Empty timeline if no images found
            }

            // Determine the start and end decade
            int minYear = imagesByYear.Keys.Min();
            int maxYear = imagesByYear.Keys.Max();
            int startDecade = (minYear / 10) * 10;
            int endDecade = (maxYear / 10) * 10;

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
        private List<TimelineItem> GetItemsForMonthZoom()
        {
            // First group images by year and month
            var imagesByYearMonth = Images.GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => new List<ImageInfo>(mg)));
            if (imagesByYearMonth.Count == 0)
            {
                return new List<TimelineItem>(); // Empty timeline if no images found
            }

            // Determine the min and max year
            int minYear = imagesByYearMonth.Keys.Min();
            int maxYear = imagesByYearMonth.Keys.Max();

            // Allocate list with the calculated number of years and create the TimelineItems
            var result = new List<TimelineItem>(maxYear - minYear + 1);
            for (int year = minYear; year <= maxYear; year++)
            {
                // Try to get the current year with the month groups
                imagesByYearMonth.TryGetValue(year, out var monthsDict);

                // Allocate list for every month base units
                var baseUnits = new List<TimelineItemBaseUnit>(12);
                for (int month = 1; month <= 12; month++)
                {
                    List<ImageInfo>? images = null;
                    monthsDict?.TryGetValue(month, out images);
                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Month, month) { Images = images });
                }
                result.Add(new TimelineItem(new DateTime(year, 1, 1), ETimelineItemType.MonthsOfYear, baseUnits));
            }

            return result;
        }

        /// <summary>
        /// Calculates timeline items for day zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForDayZoom()
        {
            // First group images by year, month and day
            var imagesByYearMonthDay = Images.GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => new List<ImageInfo>(dg))));
            if (imagesByYearMonthDay.Count == 0)
            {
                return new List<TimelineItem>(); // Empty timeline if no images found
            }

            // Determine the min and max year
            int minYear = imagesByYearMonthDay.Keys.Min();
            int maxYear = imagesByYearMonthDay.Keys.Max();

            // Allocate list with the calculated number of months and create the timeline items
            var result = new List<TimelineItem>((maxYear - minYear + 1) * 12);
            for (int year = minYear; year <= maxYear; year++)
            {
                // Try to get the current year with the month groups
                imagesByYearMonthDay.TryGetValue(year, out var monthsDict);

                for (int month = 1; month <= 12; month++)
                {
                    // Try to get current month with the days group
                    Dictionary<int, List<ImageInfo>>? daysDict = null;
                    monthsDict?.TryGetValue(month, out daysDict);

                    // Allocate list for every day base units
                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    var baseUnits = new List<TimelineItemBaseUnit>(daysInMonth);
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        List<ImageInfo>? images = null;
                        daysDict?.TryGetValue(day, out images);
                        baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Day, day) { Images = images });
                    }
                    result.Add(new TimelineItem(new DateTime(year, month, 1), ETimelineItemType.DaysOfMonth, baseUnits));
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates timeline items for hour zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForHourZoom()
        {
            // First group images by year, month, day and hour
            var imagesByYearMonthDayHour = Images.GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Hour)
                .ToDictionary(hg => hg.Key, hg => new List<ImageInfo>(hg)))));
            if (imagesByYearMonthDayHour.Count == 0)
            {
                return new List<TimelineItem>(); // Empty timeline if no images found
            }

            // Determine the min and max year
            int minYear = imagesByYearMonthDayHour.Keys.Min();
            int maxYear = imagesByYearMonthDayHour.Keys.Max();

            // Allocate list with the calculated number of months and create the timeline items
            var result = new List<TimelineItem>((maxYear - minYear + 1) * 12);
            for (int year = minYear; year <= maxYear; year++)
            {
                // Try to get the current year with the month groups
                imagesByYearMonthDayHour.TryGetValue(year, out var monthsDict);

                for (int month = 1; month <= 12; month++)
                {
                    // Try to get current month with the days group
                    Dictionary<int, Dictionary<int, List<ImageInfo>>>? daysDict = null;
                    monthsDict?.TryGetValue(month, out daysDict);

                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        // Try to get current day with the hours group
                        Dictionary<int, List<ImageInfo>>? hoursDict = null;
                        daysDict?.TryGetValue(day, out hoursDict);

                        // Allocate list for hour base units
                        var baseUnits = new List<TimelineItemBaseUnit>(24);
                        for (int hour = 0; hour < 24; hour++)
                        {
                            List<ImageInfo>? images = null;
                            hoursDict?.TryGetValue(hour, out images);
                            baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Hour, hour) { Images = images });
                        }
                        result.Add(new TimelineItem(new DateTime(year, month, day), ETimelineItemType.HoursOfDay, baseUnits));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates timeline items for minute zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForMinuteZoom()
        {
            // First group images by year, month, day and hour
            var imagesByYearMonthDayHour = Images.GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Hour)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Minute)
                .ToDictionary(hg => hg.Key, hg => new List<ImageInfo>(hg))))));
            if (imagesByYearMonthDayHour.Count == 0)
            {
                return new List<TimelineItem>(); // Empty timeline if no images found
            }

            // Determine the min and max year
            int minYear = imagesByYearMonthDayHour.Keys.Min();
            int maxYear = imagesByYearMonthDayHour.Keys.Max();

            // Allocate list with the calculated number of months and create the timeline items
            var result = new List<TimelineItem>((maxYear - minYear + 1) * 12);
            for (int year = minYear; year <= maxYear; year++)
            {
                // Try to get the current year with the month groups
                imagesByYearMonthDayHour.TryGetValue(year, out var monthsDict);

                for (int month = 1; month <= 12; month++)
                {
                    // Try to get current month with the days group
                    Dictionary<int, Dictionary<int, Dictionary<int, List<ImageInfo>>>>? daysDict = null;
                    monthsDict?.TryGetValue(month, out daysDict);

                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        // Try to get current day with the hours group
                        Dictionary<int, Dictionary<int, List<ImageInfo>>>? hoursDict = null;
                        daysDict?.TryGetValue(day, out hoursDict);

                        for (int hour = 0; hour < 24; hour++)
                        {
                            // Try to get current hour with the minutes group
                            Dictionary<int, List<ImageInfo>>? minutesDict = null;
                            hoursDict?.TryGetValue(hour, out minutesDict);

                            // Allocate list for minute base units
                            var baseUnits = new List<TimelineItemBaseUnit>(60);
                            for (int minute = 0; minute < 60; minute++)
                            {
                                List<ImageInfo>? images = null;
                                minutesDict?.TryGetValue(minute, out images);
                                baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Minute, minute) { Images = images });
                            }
                            result.Add(new TimelineItem(new DateTime(year, month, day, hour, 0, 0), ETimelineItemType.MinutesOfHour, baseUnits));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates timeline items for second zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForSecondZoom()
        {
            // First group images by year, month, day and hour
            var imagesByYearMonthDayHour = Images.GroupBy(i => i.DateTaken.Year)
                .ToDictionary(yg => yg.Key, yg => yg.GroupBy(i => i.DateTaken.Month)
                .ToDictionary(mg => mg.Key, mg => mg.GroupBy(i => i.DateTaken.Day)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Hour)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Minute)
                .ToDictionary(dg => dg.Key, dg => dg.GroupBy(i => i.DateTaken.Second)
                .ToDictionary(hg => hg.Key, hg => new List<ImageInfo>(hg)))))));
            if (imagesByYearMonthDayHour.Count == 0)
            {
                return new List<TimelineItem>(); // Empty timeline if no images found
            }

            // Determine the min and max year
            int minYear = imagesByYearMonthDayHour.Keys.Min();
            int maxYear = imagesByYearMonthDayHour.Keys.Max();

            // Allocate list with the calculated number of months and create the timeline items
            var result = new List<TimelineItem>((maxYear - minYear + 1) * 12);
            for (int year = minYear; year <= maxYear; year++)
            {
                // Try to get the current year with the month groups
                imagesByYearMonthDayHour.TryGetValue(year, out var monthsDict);

                for (int month = 1; month <= 12; month++)
                {
                    // Try to get current month with the days group
                    Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<ImageInfo>>>>>? daysDict = null;
                    monthsDict?.TryGetValue(month, out daysDict);

                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        // Try to get current day with the hours group
                        Dictionary<int, Dictionary<int, Dictionary<int, List<ImageInfo>>>>? hoursDict = null;
                        daysDict?.TryGetValue(day, out hoursDict);

                        for (int hour = 0; hour < 24; hour++)
                        {
                            // Try to get current hour with the minutes group
                            Dictionary<int, Dictionary<int, List<ImageInfo>>>? minutesDict = null;
                            hoursDict?.TryGetValue(hour, out minutesDict);

                            for (int minute = 0; minute < 60; minute++)
                            {
                                // Try to get current minute with the seconds group
                                Dictionary<int, List<ImageInfo>>? secondsDict = null;
                                minutesDict?.TryGetValue(minute, out secondsDict);

                                // Allocate list for minute base units
                                var baseUnits = new List<TimelineItemBaseUnit>(60);
                                for (int second = 0; second < 60; second++)
                                {
                                    List<ImageInfo>? images = null;
                                    secondsDict?.TryGetValue(second, out images);
                                    baseUnits.Add(new TimelineItemBaseUnit(ETimelineBaseUnitType.Second, second) { Images = images });
                                }
                                result.Add(new TimelineItem(new DateTime(year, month, day, hour, minute, 0), ETimelineItemType.SecondsOfMinute, baseUnits));
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}