using System;
using System.Collections.Generic;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Represents a timeline item for a decade (10 years).
    /// </summary>
    public class TimelineItemYearsOfDecade : TimelineItem
    {
        /// <summary>
        /// Returns the starting year of the decade.
        /// </summary>
        public int StartYear { get => ItemKey.Year; }

        /// <summary>
        /// Returns the ending year of the decade.
        /// </summary>
        public int EndYear { get => ItemKey.Year + 9; }

        /// <summary>
        /// Creates a new timeline item representing a decade (10 years).
        /// </summary>
        public TimelineItemYearsOfDecade(int startYear, List<ImageInfo> images) : base(new DateTime(startYear, 1, 1), ETimelineItemType.YearsOfDecade, images)
        {
        }
    }
}
