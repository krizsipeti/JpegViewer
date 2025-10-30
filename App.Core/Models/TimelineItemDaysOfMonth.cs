using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    public class TimelineItemDaysOfMonth : TimelineItem
    {
        /// <summary>
        /// Creates a new timeline item representing the days of a specific month.
        /// </summary>
        public TimelineItemDaysOfMonth(int year, int month, Dictionary<int, ObservableCollection<ImageInfo>> images) : base(new DateTime(year, month, 1), ETimelineItemType.DaysOfMonth, images)
        {
        }
    }
}
