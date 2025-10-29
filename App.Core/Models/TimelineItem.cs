using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JpegViewer.App.Core.Models
{
    /// <summary>
    /// Item representing a time intervall in the timeline.
    /// </summary>
    public class TimelineItem : ObservableObject
    {
        private DateTime _itemKey;
        private List<DateTime> _dates = new List<DateTime>();

        /// <summary>
        /// Holds the base date of the timeline item.
        /// </summary>
        public DateTime ItemKey
        {
            get => _itemKey;
            set => SetProperty(ref _itemKey, value);
        }

        /// <summary>
        /// Holds the list of dates contained in the timeline item.
        /// </summary>
        public List<DateTime> Dates
        {
            get => _dates;
            set => SetProperty(ref _dates, value);
        }
    }
}
