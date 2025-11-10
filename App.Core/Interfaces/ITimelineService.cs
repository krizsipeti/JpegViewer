using System;
using System.Collections.Generic;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Interfaces
{
    public interface ITimelineService
    {
        /// <summary>
        /// Get or set the default number of TimelineItems to create before and after the middle item.
        /// </summary>
        public byte ItemBufferSize { get; set; }

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

        /// <summary>
        /// Returns a list of TimelineItems based on the given option for the DateTime.
        /// itemBufferSize defines how many pre and post items to create if asked for in getOption parameter.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="dateTime"></param>
        /// <param name="itemBufferSize"></param>
        /// <param name="getOption"></param>
        /// <returns></returns>
        List<TimelineItem> GetItemsForZoomLevel(ETimelineZoomLevel zoomLevel, DateTime dateTime, byte itemBufferSize, ETimelineGetOption getOption);
    }
}
