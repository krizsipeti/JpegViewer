using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Represents a timeline item for a decade (10 years).
    /// </summary>
    public class TimelineItemYearsOfDecade : TimelineItem
    {
        /// <summary>
        /// Creates a new timeline item representing a decade (10 years).
        /// </summary>
        public TimelineItemYearsOfDecade(int startYear, Dictionary<int, ObservableCollection<ImageInfo>> images) : base(new DateTime(startYear, 1, 1), ETimelineItemType.YearsOfDecade, images)
        {
        }
    }
}
