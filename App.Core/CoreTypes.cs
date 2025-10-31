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
        Minutes
    }

    /// <summary>
    /// The type of timeline item.
    /// </summary>
    public enum ETimelineItemType
    {
        YearsOfDecade,
        MonthsOfYear,
        DaysOfMonth,
        HoursOfDay
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
}