using System;
using System.Collections.Generic;
using System.Linq;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Services
{
    /// <summary>
    /// Responsible to prepare and create TimelineItems.
    /// </summary>
    public class TimelineService : ITimelineService
    {
        /// <summary>
        /// Holds a referenc to image service.
        /// </summary>
        private IImageService ImageService { get; }

        /// <summary>
        /// Get or set the default number of TimelineItems to create before and after the middle item.
        /// By default, buffer size is set to 3, so 7 items generated: 3(pre) - 1(main item based on given datetime) - 3(post).
        /// </summary>
        public byte ItemBufferSize { get; set; } = 3;

        /// <summary>
        /// Store the incoming image service reference.
        /// </summary>
        /// <param name="imageService"></param>
        public TimelineService(IImageService imageService)
        {
            ImageService = imageService;
        }

        /// <summary>
        /// Creates and prepares timeline items around the given datetime
        /// using the default item buffer size defined in ItemBufferSize property.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TimelineItem> GetItemsForZoomLevel(ETimelineZoomLevel zoomLevel, DateTime dateTime)
        {
            return GetItemsForZoomLevel(zoomLevel, dateTime, ItemBufferSize);
        }

        /// <summary>
        /// Creates and prepares timeline items around the given datetime.
        /// itemBufferSize defines how many items to create before and after
        /// the middle (main) item created for the given datetime.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="dateTime"></param>
        /// <param name="itemBufferSize"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TimelineItem> GetItemsForZoomLevel(ETimelineZoomLevel zoomLevel, DateTime dateTime, byte itemBufferSize)
        {
            return GetItemsForZoomLevel(zoomLevel, dateTime, itemBufferSize, ETimelineGetOption.Both);
        }

        /// <summary>
        /// Creates and prepares timeline items based on the given date time and options.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="dateTime"></param>
        /// <param name="itemBufferSize"></param>
        /// <param name="getOption"></param>
        /// <returns></returns>
        public List<TimelineItem> GetItemsForZoomLevel(ETimelineZoomLevel zoomLevel, DateTime dateTime, byte itemBufferSize, ETimelineGetOption getOption)
        {
            try
            {
                return zoomLevel switch
                {
                    ETimelineZoomLevel.Years => GetItemsForYearZoom(dateTime.AddYears(getOption == ETimelineGetOption.Post ? 10 : -itemBufferSize * 10),
                                                                    dateTime.AddYears(getOption == ETimelineGetOption.Pre ? -10 : itemBufferSize * 10)),

                    ETimelineZoomLevel.Months => GetItemsForMonthZoom(dateTime.AddYears(getOption == ETimelineGetOption.Post ? 1 : -itemBufferSize),
                                                                      dateTime.AddYears(getOption == ETimelineGetOption.Pre ? -1 : itemBufferSize)),

                    ETimelineZoomLevel.Days => GetItemsForDayZoom(dateTime.AddMonths(getOption == ETimelineGetOption.Post ? 1 : -itemBufferSize),
                                                                  dateTime.AddMonths(getOption == ETimelineGetOption.Pre ? -1 : itemBufferSize)),

                    ETimelineZoomLevel.Hours => GetItemsForHourZoom(dateTime.AddDays(getOption == ETimelineGetOption.Post ? 1 : -itemBufferSize),
                                                                    dateTime.AddDays(getOption == ETimelineGetOption.Pre ? -1 : itemBufferSize)),

                    ETimelineZoomLevel.Minutes => GetItemsForMinuteZoom(dateTime.AddHours(getOption == ETimelineGetOption.Post ? 1 : -itemBufferSize),
                                                                        dateTime.AddHours(getOption == ETimelineGetOption.Pre ? -1 : itemBufferSize)),

                    ETimelineZoomLevel.Seconds => GetItemsForSecondZoom(dateTime.AddMinutes(getOption == ETimelineGetOption.Post ? 1 : -itemBufferSize),
                                                                        dateTime.AddMinutes(getOption == ETimelineGetOption.Pre ? -1 : itemBufferSize)),

                    _ => new List<TimelineItem>()
                };
            }
            catch
            {
                return new List<TimelineItem>();
            }
        }

        /// <summary>
        /// Calculates timeline items for year zoom level.
        /// </summary>
        private List<TimelineItem> GetItemsForYearZoom(DateTime start, DateTime end)
        {
            // First group images by year
            var imagesByYear = ImageService.GetImagesForDateRange(start, end)
                .GroupBy(i => i.DateTaken.Year).ToDictionary(g => g.Key, g => new List<ImageInfo>(g));

            // Determine the start and end decade
            int startDecade = (start.Year / 10) * 10;
            int endDecade = (end.Year / 10) * 10;

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
