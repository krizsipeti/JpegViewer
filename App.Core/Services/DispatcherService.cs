using System;
using JpegViewer.App.Core.Interfaces;
using Windows.System;

namespace JpegViewer.App.UI.Services
{
    /// <summary>
    /// Provides methods for executing actions on a specific thread or dispatcher, ensuring thread-safe operations.
    /// </summary>
    public class DispatcherService : IDispatcherService
    {
        /// <summary>
        /// 
        /// </summary>
        private DispatcherQueue DispatcherQueue { get; }

        #region Public Methods

        /// <summary>
        /// The constructor stores the dispatcher queue of the current calling thread.
        /// </summary>
        public DispatcherService()
        {
            DispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        /// <summary>
        /// True if the calling thread has access to the dispatcher.
        /// </summary>
        public bool CheckAccess()
        {
            // Check if the current thread's dispatcher queue matches the stored dispatcher queue.
            return DispatcherQueue.GetForCurrentThread() == DispatcherQueue;
        }

        /// <summary>
        /// Invokes the specified action on the dispatcher thread.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public void Invoke(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (CheckAccess())
            {
                action();
                return;
            }
            DispatcherQueue.TryEnqueue(() => action());
        }

        #endregion Public Methods
    }
}
