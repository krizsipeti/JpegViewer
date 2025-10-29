using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Template selector for timeline items.
    /// </summary>
    public class TimelineItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Holds the template for year-level timeline items.
        /// </summary>
        public DataTemplate YearTemplate { get; set; } = default!;

        /// <summary>
        /// Holds the template for month-level timeline items.
        /// </summary>
        public DataTemplate MonthTemplate { get; set; } = default!;

        /// <summary>
        /// Holds the template for day-level timeline items.
        /// </summary>
        public DataTemplate DayTemplate { get; set; } = default!;

        /// <summary>
        /// Override to select the appropriate template based on the timeline zoom level.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is null)
            {
                return base.SelectTemplateCore(item);
            }

            if (item is TimelineItem timelineItem)
            {
                return timelineItem.ItemType switch
                {
                    ETimelineItemType.YearsOfDecade => YearTemplate,
                    ETimelineItemType.MonthsOfYear => MonthTemplate,
                    ETimelineItemType.DaysOfMonth => DayTemplate,
                    _ => base.SelectTemplateCore(item),
                };
            }

            return base.SelectTemplateCore(item);
        }
    }
}
