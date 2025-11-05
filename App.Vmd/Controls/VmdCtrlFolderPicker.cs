using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Models;
using JpegViewer.App.Core.Types;

namespace JpegViewer.App.Vmd.Controls
{
    /// <summary>
    /// View model for folder picker control.
    /// </summary>
    public partial class VmdCtrlFolderPicker : VmdBase, IRecipient<FolderPickerItemExpandedMessage>
    {
        private FolderPickerItem? _selectedItem;
        private readonly object _lockRefresh = new object();

        /// <summary>
        /// Holds the current selected folder item if any.
        /// </summary>
        public FolderPickerItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        /// <summary>
        /// Holds the loaded folder items.
        /// </summary>
        public ObservableCollection<FolderPickerItem> Folders { get; } = new ObservableCollection<FolderPickerItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdCtrlFolderPicker"/> class.
        /// </summary>
        public VmdCtrlFolderPicker(IDispatcherService dispatcherService) : base(dispatcherService)
        {
            // Register this instance to receive messages
            WeakReferenceMessenger.Default.Register<FolderPickerItemExpandedMessage>(this);
        }

        /// <summary>
        /// Refreshes the folder list by reloading the folder tree.
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task RefreshPressed()
        {
            await Task.Run(() => LoadDrives());
        }

        /// <summary>
        /// Hide 
        /// </summary>
        [RelayCommand]
        private void OkPressed()
        {
        }

        /// <summary>
        /// Clear current selection.
        /// </summary>
        [RelayCommand]
        private void CancelPressed()
        {
            SelectedItem = null;
        }

        /// <summary>
        /// Fills the folder list with available drives when the control is loaded.
        /// </summary>
        [RelayCommand]
        private async Task Loaded()
        {
            await Task.Run(() => LoadDrives());
        }

        /// <summary>
        /// Loads child directories when a folder item is expanded.
        /// </summary>
        /// <param name="message"></param>
        public void Receive(FolderPickerItemExpandedMessage message)
        {
            Task.Run(() =>
            {
                lock (message.ExpandedItem)
                {
                    PreloadChildsOfChildDirectories(message.ExpandedItem);
                }
            });
        }

        /// <summary>
        /// Loads the list of available drives with their childs.
        /// </summary>
        private void LoadDrives()
        {
            lock (_lockRefresh)
            {
                try
                {
                    // First clear the folders collection
                    DispatcherService.Invoke(() => Folders.Clear());

                    // Get drives and their root directories
                    var list = new List<FolderPickerItem>();
                    foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable || d.DriveType == DriveType.CDRom)).Select(d => new FolderPickerItem(d.Name)))
                    {
                        foreach (var item in Directory.EnumerateDirectories(drive.Name).Select(d => Path.GetFileName(d) ?? string.Empty).Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => new FolderPickerItem(d, drive)))
                        {
                            drive.Childs.Add(item);
                        }
                        list.Add(drive);
                    }

                    // Update the Folders collection on the UI thread
                    DispatcherService.Invoke(() =>
                    {
                        foreach (var drive in list)
                        {
                            Folders.Add(drive);
                        }
                    });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Loads child directories for a given folder picker item.
        /// </summary>
        private void LoadChildDirectories(FolderPickerItem item)
        {
            if (item.ChildsLoaded)
            {
                return;
            }

            try
            {
                foreach (var dir in Directory.EnumerateDirectories(item.GetFullPath()).Select(d => Path.GetFileName(d) ?? string.Empty).Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => new FolderPickerItem(d, item)))
                {
                    DispatcherService.Invoke(() => item.Childs.Add(dir));
                }
                item.ChildsLoaded = true;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Loads child directories of the child directories of a given folder picker item.
        /// </summary>
        /// <param name="item"></param>
        private void PreloadChildsOfChildDirectories(FolderPickerItem item)
        {
            foreach (var child in item.Childs.Where(c => !c.ChildsLoaded))
            {
                LoadChildDirectories(child);
            }
        }
    }
}
