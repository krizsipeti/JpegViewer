using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using JpegViewer.App.Core.Interfaces;

namespace JpegViewer.App.Vmd.Controls
{
    /// <summary>
    /// View model for folder picker control.
    /// </summary>
    public partial class VmdCtrlFolderPicker : VmdBase
    {
        public ObservableCollection<string> Folders { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdCtrlFolderPicker"/> class.
        /// </summary>
        public VmdCtrlFolderPicker(IDispatcherService dispatcherService) : base(dispatcherService)
        {
        }

        /// <summary>
        /// Fills the folder list with available drives when the control is loaded.
        /// </summary>
        [RelayCommand]
        private async Task Loaded()
        {
            await Task.Run(() =>
            {
                // Get drives and their root directories
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable || d.DriveType == DriveType.CDRom)).Select(d => d.Name);
                var list = new List<string>();
                foreach (var drive in drives)
                {
                    list.Add(drive);
                    list.AddRange(Directory.EnumerateDirectories(drive).Select(d => Path.GetFileName(d) ?? string.Empty).Where(d => !string.IsNullOrWhiteSpace(d)));
                }

                // Update the Folders collection on the UI thread
                DispatcherService.Invoke(() =>
                {
                    foreach (var item in list)
                    {
                        Folders.Add(item);
                    }
                });
            });
        }
    }
}
