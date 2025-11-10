using System;
using JpegViewer.App.Core.Models;

namespace JpegViewer.App.Core.Types
{
    /// <summary>
    /// Possible item types in the folder picker.
    /// </summary>
    public enum EFolderPickerItemType
    {
        Drive,
        Folder
    }

    /// <summary>
    /// Holds segoe-fluent-icons for folder picker items.
    /// </summary>
    public enum EFolderPickerItemIcon
    {
        Drive = 0xEDA2,
        FolderClosed = 0xE8B7,
        FolderOpen = 0xE838
    }

    /// <summary>
    /// Message sent when a folder picker item is expanded.
    /// </summary>
    /// <param name="ExpandedItem"></param>
    public record FolderPickerItemExpandedMessage(FolderPickerItem ExpandedItem);

    /// <summary>
    /// Possible zoom levels for the timeline control.
    /// </summary>
    public enum ETimelineZoomLevel
    {
        Years,
        Months,
        Days,
        Hours,
        Minutes,
        Seconds
    }

    /// <summary>
    /// The type of timeline item.
    /// </summary>
    public enum ETimelineItemType
    {
        YearsOfDecade,
        MonthsOfYear,
        DaysOfMonth,
        HoursOfDay,
        MinutesOfHour,
        SecondsOfMinute
    }

    /// <summary>
    /// The type of the timeline base units.
    /// </summary>
    public enum ETimelineBaseUnitType
    { 
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second
    }

    /// <summary>
    /// Options how to return timeline items for a date time.
    /// </summary>
    public enum ETimelineGetOption
    {
        Pre,
        Post,
        Both
    }

    /// <summary>
    /// Small helper used in timeline for jump requests done
    /// on full (re)initialization or when changing among zoom levels.
    /// </summary>
    public class JumpRequest
    {
        /// <summary>
        /// If true, we did not done jumping yet, so it is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The DateTime we want to jump to in our timeline.
        /// </summary>
        public DateTime JumpTo { get; set; }

        /// <summary>
        /// Sets active and the jumpto properties always.
        /// </summary>
        /// <param name="active"></param>
        /// <param name="jumpTo"></param>
        public JumpRequest(bool active, DateTime jumpTo)
        {
            Active = active;
            JumpTo = jumpTo;
        }
    }

    /// <summary>
    /// Message sent from view model to code behind on position change.
    /// </summary>
    /// <param name="NewPosition"></param>
    /// <param name="TimelineItem"></param>
    public record TimelineCurrentPositionChange(DateTime NewPosition, TimelineItem? TimelineItem);

    /// <summary>
    /// How image search should be done on a specific folder.
    /// </summary>
    public enum ESubFolderRecursion
    {
        ExcludeSubFolders,
        IncludeSubFolders
    }
}