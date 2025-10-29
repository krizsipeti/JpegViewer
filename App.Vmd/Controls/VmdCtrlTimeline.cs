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
        private ObservableCollection<TimelineItem> _itemsYears = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsMonths = new ObservableCollection<TimelineItem>();
        private ObservableCollection<TimelineItem> _itemsDays = new ObservableCollection<TimelineItem>();
        private double _itemsWidth;
        private ETimelineZoomLevel _zoomLevel;

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
        public double MinItemsWidth => 1000;

        /// <summary>
        /// The maximum width allowed for the timeline items.
        /// </summary>
        public double MaxItemsWidth => 2000;

        /// <summary>
        /// The current zoom level of the timeline.
        /// </summary>
        public ETimelineZoomLevel ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
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
        /// Holds picture dates to be displayed on the timeline.
        /// </summary>
        public List<DateTime> PictureDates = new List<DateTime>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdCtrlTimeline"/> class.
        /// </summary>
        public VmdCtrlTimeline(IDispatcherService dispatcherService) : base(dispatcherService)
        {
            ItemsWidth = MinItemsWidth;
            ZoomLevel = ETimelineZoomLevel.Years;
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
            PictureDates.Clear();
            var start = DateTime.Now;//.AddYears(100);
            var end = start.AddYears(-1000);
            var range = (end - start).TotalSeconds;
            // secure random double in [0,1)
            Span<byte> bytes = stackalloc byte[8];
            for (int i = 0; i < 10000; i++)
            {
                RandomNumberGenerator.Fill(bytes);
                ulong u = BitConverter.ToUInt64(bytes);
                double sample = (u >> 11) * (1.0 / (1UL << 53)); // 53-bit precision
                var secs = (long)(sample * range);
                PictureDates.Add(start.AddSeconds(secs));
            }

            // After generating dates, calculate timeline items
            Task.Run(() =>
            {
                var yearsItems = GetItemsForYearZoom();
                // Update collections on the UI thread
                DispatcherService.Invoke(() => { ItemsYears = new ObservableCollection<TimelineItem>(yearsItems); });
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
        }

        /// <summary>
        /// Calculates timeline items for year zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForYearZoom()
        {
            int minYear = PictureDates.Min(d => d.Year);
            int maxYear = PictureDates.Max(d => d.Year);
            int startDecade = (minYear / 10) * 10;
            int endDecade = (maxYear / 10) * 10;

            // Group existing dates by decade start year
            var grouped = PictureDates.GroupBy(d => (d.Year / 10) * 10).ToDictionary(g => g.Key, g => g.Order().ToList());

            // Create timeline items for each decade in the range
            var result = new List<TimelineItem>();
            for (int decade = startDecade; decade <= endDecade; decade += 10)
            {
                grouped.TryGetValue(decade, out var list);
                result.Add(new TimelineItem()
                {
                    ItemKey = new DateTime(decade, 1, 1),
                    Dates = list ?? new List<DateTime>()
                });
            }
            return result;
        }

        /// <summary>
        /// Calculates timeline items for month zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForMonthZoom()
        {
            int minYear = PictureDates.Min(d => d.Year);
            int maxYear = PictureDates.Max(d => d.Year);

            // Group existing dates by year
            var grouped = PictureDates.GroupBy(d => d.Year).ToDictionary(g => g.Key, g => g.Order().ToList());

            // Create timeline items for each year in the range
            var result = new List<TimelineItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                grouped.TryGetValue(year, out var list);
                result.Add(new TimelineItem()
                {
                    ItemKey = new DateTime(year, 1, 1),
                    Dates = list ?? new List<DateTime>()
                });
            }
            return result;
        }

        /// <summary>
        /// Calculates timeline items for day zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForDayZoom()
        {
            int minYear = PictureDates.Min(d => d.Year);
            int maxYear = PictureDates.Max(d => d.Year);

            // Group existing dates by (year, month)
            var grouped = PictureDates.GroupBy(d => (d.Year, d.Month)).ToDictionary(g => g.Key, g => g.Order().ToList());

            // Create timeline items for each month in the range
            var result = new List<TimelineItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    grouped.TryGetValue((year, month), out var list);
                    result.Add(new TimelineItem()
                    {
                        ItemKey = new DateTime(year, month, 1),
                        Dates = list ?? new List<DateTime>()
                    });
                }
            }
            return result;
        }
    }
}
