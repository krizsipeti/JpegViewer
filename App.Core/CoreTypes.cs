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
}