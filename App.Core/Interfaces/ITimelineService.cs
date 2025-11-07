using System;
using System.Collections.Generic;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Interfaces
{
    public interface ITimelineService
    {
        /// <summary>
        /// Returns a list of TimelineItems prepared around the given DateTime
        /// using the default number for item buffer size.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        List<TimelineItem> GetItemsForZoomLevel(ETimelineZoomLevel zoomLevel, DateTime dateTime);

        /// <summary>
        /// Returns a list of TimelineItems prepared around the given DateTime.
        /// itemBufferSize defines how many pre and post items to create.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        List<TimelineItem> GetItemsForZoomLevel(ETimelineZoomLevel zoomLevel, DateTime dateTime, byte itemBufferSize);
    }
}
