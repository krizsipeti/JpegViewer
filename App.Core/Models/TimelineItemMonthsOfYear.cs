using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JpegViewer.App.Core.Types;

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
        public TimelineItemMonthsOfYear(int year, Dictionary<int, ObservableCollection<ImageInfo>> images) : base(new DateTime(year, 1, 1), Types.ETimelineItemType.MonthsOfYear, images)
        {
        }
    }
}
