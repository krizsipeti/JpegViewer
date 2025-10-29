using System;
using System.Collections.Generic;
using System.Linq;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Represents a timeline item for the months of a specific year.
    /// </summary>
    public class TimelineItemMonthsOfYear : TimelineItem
    {
        /// <summary>
        /// Returns the images taken in January of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfJan => Images.Where(i => i.DateTaken.Month == 1);

        /// <summary>
        /// Returns the images taken in February of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfFeb => Images.Where(i => i.DateTaken.Month == 2);

        /// <summary>
        /// Returns the images taken in March of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfMar => Images.Where(i => i.DateTaken.Month == 3);

        /// <summary>
        /// Returns the images taken in April of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfApr => Images.Where(i => i.DateTaken.Month == 4);

        /// <summary>
        /// Returns the images taken in May of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfMay => Images.Where(i => i.DateTaken.Month == 5);

        /// <summary>
        /// Returns the images taken in June of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfJun => Images.Where(i => i.DateTaken.Month == 6);

        /// <summary>
        /// Returns the images taken in July of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfJul => Images.Where(i => i.DateTaken.Month == 7);

        /// <summary>
        /// Returns the images taken in August of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfAug => Images.Where(i => i.DateTaken.Month == 8);

        /// <summary>
        /// Returns the images taken in September of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfSep => Images.Where(i => i.DateTaken.Month == 9);

        /// <summary>
        /// Returns the images taken in October of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfOct => Images.Where(i => i.DateTaken.Month == 10);

        /// <summary>
        /// Returns the images taken in November of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfNov => Images.Where(i => i.DateTaken.Month == 11);

        /// <summary>
        /// Returns the images taken in December of the year.
        /// </summary>
        public IEnumerable<ImageInfo> ImagesOfDec => Images.Where(i => i.DateTaken.Month == 12);

        /// <summary>
        /// Creates a new timeline item representing the months of a specific year.
        /// </summary>
        public TimelineItemMonthsOfYear(int year, List<ImageInfo> images) : base(new DateTime(year, 1, 1), Types.ETimelineItemType.MonthsOfYear, images)
        {
        }
    }
}
