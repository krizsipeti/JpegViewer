using System;
using System.Collections.Generic;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    public class TimelineItemDaysOfMonth : TimelineItem
    {
        /// <summary>
        /// Creates a new timeline item representing the days of a specific month.
        /// </summary>
        public TimelineItemDaysOfMonth(int year, int month, List<TimelineItemBaseUnit> units) : base(new DateTime(year, month, 1), ETimelineItemType.DaysOfMonth, units)
        {
        }
    }
}
