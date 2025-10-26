using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Item representing a folder in the folder picker.
    /// </summary>
    public class FolderPickerItem : ObservableObject
    {
        #region Fields

        private string _name = string.Empty;
        private bool _isExpanded = false;
        private bool _isSelected = false;
        private EFolderPickerItemType _itemType;

        #endregion Fields

        #region Public properties

        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// True if the folder item is expanded to show its child items.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    if (value)
                    {
                        WeakReferenceMessenger.Default.Send(new FolderPickerItemExpandedMessage(this));
                    }
                    OnPropertyChanged(nameof(ItemIcon));
                }
            }
        }

        /// <summary>
        /// True if the folder item is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// The type of the folder picker item.
        /// </summary>
        public EFolderPickerItemType ItemType
        {
            get => _itemType;
            set
            {
                if (SetProperty(ref _itemType, value))
                {
                    OnPropertyChanged(nameof(ItemIcon));
                }
            }
        }

        /// <summary>
        /// Returns the icon associated with the folder picker item based on its type and state.
        /// </summary>
        public EFolderPickerItemIcon ItemIcon
        {
            get
            {
                return ItemType == EFolderPickerItemType.Drive ? EFolderPickerItemIcon.Drive : IsExpanded ? EFolderPickerItemIcon.FolderOpen : EFolderPickerItemIcon.FolderClosed;
            }
        }

        /// <summary>
        /// Parent folder picker item or null if this is a root item. Can be set only in constructor.
        /// </summary>
        public FolderPickerItem? Parent { get; }

        /// <summary>
        /// Collection of child folder picker items. Can be empty.
        /// </summary>
        public ObservableCollection<FolderPickerItem> Childs { get; } = new ObservableCollection<FolderPickerItem>();

        /// <summary>
        /// True if the child items have been loaded.
        /// </summary>
        public bool ChildsLoaded { get; set; } = false;

        #endregion Public properties

        #region Constructors

        /// <summary>
        /// Parent folder picker item or null if this is a root item.
        /// </summary>
        /// <param name="parent"></param>
        public FolderPickerItem(string name, FolderPickerItem? parent = null)
        {
            Name = name;
            Parent = parent;
            ItemType = parent == null ? EFolderPickerItemType.Drive : EFolderPickerItemType.Folder;
        }

        #endregion Constructors

        /// <summary>
        /// Calculates the full path of the folder picker item by traversing up to the root.
        /// </summary>
        public string GetFullPath()
        {
            string path = Name;
            FolderPickerItem? current = this;
            while (current.Parent != null)
            {
                path = Path.Combine(current.Parent.Name, path);
                current = current.Parent;
            }
            return path;
        }
    }
}