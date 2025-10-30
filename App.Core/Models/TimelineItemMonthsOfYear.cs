using System;
using System.Collections.Generic;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Represents a timeline item for the months of a specific year.
    /// </summary>
    public class TimelineItemMonthsOfYear : TimelineItem
    {
        /// <summary>
        /// Creates a new timeline item representing the months of a specific year.
        /// </summary>
        public TimelineItemMonthsOfYear(int year, List<TimelineItemBaseUnit> units) : base(new DateTime(year, 1, 1), Types.ETimelineItemType.MonthsOfYear, units)
        {
        }
    }
}
