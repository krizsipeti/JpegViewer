using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Item representing a time intervall in the timeline.
    /// </summary>
    public class TimelineItem : ObservableObject
    {
        private DateTime _date;

        /// <summary>
        /// Holds the base date of the timeline item.
        /// </summary>
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
    }
}
