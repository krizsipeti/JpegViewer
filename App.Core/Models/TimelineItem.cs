using System;
using System.Collections.Generic;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// The base class for timeline items.
    /// Represents a time intervall in the timeline.
    /// </summary>
    public class TimelineItem
    {
        /// <summary>
        /// Holds the key date representing the timeline item.
        /// It is read-only and set via constructor.
        /// </summary>
        public DateTime ItemKey { get; }

        /// <summary>
        /// Type of the timeline item (Year, Month, Day, etc.).
        /// Read-only and set via constructor.
        /// </summary>
        public ETimelineItemType ItemType { get; }

        /// <summary>
        /// Images associated with this timeline item.
        /// </summary>
        public List<ImageInfo> Images { get; }

        public TimelineItem(DateTime itemKey, ETimelineItemType itemType, List<ImageInfo> images)
        {
            ItemKey = itemKey;
            ItemType = itemType;
            Images = images;
        }
    }
}
