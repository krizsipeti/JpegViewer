using CommunityToolkit.Mvvm.ComponentModel;
using JpegViewer.App.Core.Interfaces;

namespace JpegViewer.App.Vmd
{
    /// <summary>
    /// The base class for all view models in the application.
    /// </summary>
    public class VmdBase : ObservableObject
    {
        /// <summary>
        /// Holds 
        /// </summary>
        public IDispatcherService DispatcherService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmdBase"/> class.
        /// </summary>
        public VmdBase(IDispatcherService dispatcherService)
        {
            DispatcherService = dispatcherService;
        }
    }
}
