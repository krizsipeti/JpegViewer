using System;
using System.Collections.Generic;
using System.Linq;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    public class TimelineItemDaysOfMonth : TimelineItem
    {
        /// <summary>
        /// Returns the images taken on the specified day of the month.
        /// </summary>
        public IEnumerable<ImageInfo> GetImagesOfDay(int day)
        {
            return Images.Where(i => i.DateTaken.Day == day);
        }

        /// <summary>
        /// Creates a new timeline item representing the days of a specific month.
        /// </summary>
        public TimelineItemDaysOfMonth(int year, int month, List<ImageInfo> images) : base(new DateTime(year, month, 1), ETimelineItemType.DaysOfMonth, images)
        {
        }
    }
}
