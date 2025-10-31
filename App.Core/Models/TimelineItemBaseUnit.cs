using System.Collections.Generic;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// The base unit of the current timeline item.
    /// Depends on the timeline item type.
    /// </summary>
    public class TimelineItemBaseUnit
    {
        /// <summary>
        /// The type of this base unit.
        /// </summary>
        public ETimelineBaseUnitType Type { get; }

        /// <summary>
        /// The value of this item in base units.
        /// For example year, month, day, hour, etc.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// All the images belongs to this item based on their creation time or null of none.
        /// </summary>
        public List<ImageInfo>? Images { get; set; }

        /// <summary>
        /// Create base unit with immutable value
        /// </summary>
        /// <param name="value"></param>
        public TimelineItemBaseUnit(ETimelineBaseUnitType type, int value)
        {
            Type = type;
            Value = value;
        }
    }
}
